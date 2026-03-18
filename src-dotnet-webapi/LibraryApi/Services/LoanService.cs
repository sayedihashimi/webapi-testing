using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
{
    private static int GetBorrowingLimit(MembershipType type) => type switch
    {
        MembershipType.Standard => 5,
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    private static int GetLoanDays(MembershipType type) => type switch
    {
        MembershipType.Standard => 14,
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };

    public async Task<PagedResponse<LoanResponse>> GetAllAsync(LoanStatus? status, bool? overdue, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue) query = query.Where(l => l.Status == status.Value);
        if (overdue == true) query = query.Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null);

        query = query.OrderByDescending(l => l.LoanDate);
        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => MapToResponse(l)).ToListAsync(ct);

        return new PagedResponse<LoanResponse>
        {
            Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            HasNextPage = page * pageSize < totalCount, HasPreviousPage = page > 1
        };
    }

    public async Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        return loan is null ? null : MapToResponse(loan);
    }

    public async Task<LoanResponse> CheckoutAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new ArgumentException("Patron membership is not active.");

        if (book.AvailableCopies < 1)
            throw new ArgumentException("No available copies of this book.");

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10m)
            throw new ArgumentException($"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to borrow.");

        // Check borrowing limit
        var activeLoans = await db.Loans.CountAsync(l => l.PatronId == patron.Id && l.Status == LoanStatus.Active, ct);
        var limit = GetBorrowingLimit(patron.MembershipType);
        if (activeLoans >= limit)
            throw new ArgumentException($"Patron has reached the borrowing limit of {limit} active loans for {patron.MembershipType} membership.");

        var now = DateTime.UtcNow;
        var loanDays = GetLoanDays(patron.MembershipType);

        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        book.AvailableCopies--;
        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Book {BookId} checked out to Patron {PatronId}, Loan {LoanId}", book.Id, patron.Id, loan.Id);

        return (await GetByIdAsync(loan.Id, ct))!;
    }

    public async Task<LoanResponse> ReturnAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new ArgumentException("This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;

        // Auto-fine for overdue
        if (loan.DueDate < now)
        {
            var overdueDays = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = overdueDays * 0.25m;
            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue by {overdueDays} day(s) at $0.25/day",
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            };
            db.Fines.Add(fine);
            logger.LogInformation("Fine of ${Amount} issued to Patron {PatronId} for overdue Loan {LoanId}", fineAmount, loan.PatronId, loan.Id);
        }

        // Promote first pending reservation
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            loan.Book.AvailableCopies--; // Reserve the copy
            logger.LogInformation("Reservation {ReservationId} promoted to Ready for Book {BookId}", nextReservation.Id, loan.BookId);
        }

        await db.SaveChangesAsync(ct);
        return (await GetByIdAsync(loan.Id, ct))!;
    }

    public async Task<LoanResponse> RenewAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status != LoanStatus.Active)
            throw new ArgumentException("Only active loans can be renewed.");

        if (loan.DueDate < DateTime.UtcNow)
            throw new ArgumentException("Cannot renew an overdue loan.");

        if (loan.RenewalCount >= 2)
            throw new ArgumentException("Maximum renewal limit (2) reached.");

        // Check for pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasPendingReservations)
            throw new ArgumentException("Cannot renew — there are pending reservations for this book.");

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10m)
            throw new ArgumentException($"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to renew.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Loan {LoanId} renewed (count: {Count})", loan.Id, loan.RenewalCount);

        return (await GetByIdAsync(loan.Id, ct))!;
    }

    public async Task<List<LoanResponse>> GetOverdueAsync(CancellationToken ct)
    {
        return await db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null)
            .OrderBy(l => l.DueDate)
            .Select(l => MapToResponse(l))
            .ToListAsync(ct);
    }

    private static LoanResponse MapToResponse(Loan l) => new()
    {
        Id = l.Id, BookId = l.BookId, BookTitle = l.Book.Title, BookISBN = l.Book.ISBN,
        PatronId = l.PatronId, PatronName = l.Patron.FirstName + " " + l.Patron.LastName,
        LoanDate = l.LoanDate, DueDate = l.DueDate, ReturnDate = l.ReturnDate,
        Status = l.Status, RenewalCount = l.RenewalCount, CreatedAt = l.CreatedAt
    };
}
