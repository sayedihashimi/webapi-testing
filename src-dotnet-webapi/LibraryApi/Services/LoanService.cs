using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface ILoanService
{
    Task<PaginatedResponse<LoanResponse>> GetAllAsync(LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken ct);
    Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<LoanResponse> CheckoutAsync(CreateLoanRequest request, CancellationToken ct);
    Task<LoanResponse> ReturnAsync(int id, CancellationToken ct);
    Task<LoanResponse> RenewAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetOverdueAsync(int page, int pageSize, CancellationToken ct);
}

public class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
{
    private const decimal OverdueFinePerDay = 0.25m;
    private const decimal FineThreshold = 10.00m;
    private const int MaxRenewals = 2;

    public async Task<PaginatedResponse<LoanResponse>> GetAllAsync(
        LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Overdue ||
                (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null));

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => MapToResponse(l))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Id == id)
            .Select(l => MapToResponse(l))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<LoanResponse> CheckoutAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron account is inactive.");

        // Check unpaid fines
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= FineThreshold)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold: ${FineThreshold:F2}). Fines must be paid before checkout.");

        // Check borrowing limit
        var (maxLoans, loanDays) = GetLoanLimits(patron.MembershipType);
        var activeLoans = await db.Loans.CountAsync(
            l => l.PatronId == patron.Id && l.Status != LoanStatus.Returned, ct);

        if (activeLoans >= maxLoans)
            throw new InvalidOperationException($"Patron has reached the borrowing limit of {maxLoans} active loans for {patron.MembershipType} membership.");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException("No available copies of this book.");

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active,
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
        var loan = await db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;

        // Generate fine if overdue
        if (loan.DueDate < now)
        {
            var overdueDays = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = overdueDays * OverdueFinePerDay;

            db.Fines.Add(new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue return - {overdueDays} day{(overdueDays != 1 ? "s" : "")} late",
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            });

            logger.LogInformation("Fine of ${Amount} generated for overdue Loan {LoanId}", fineAmount, loan.Id);
        }

        // Check pending reservations for this book
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            logger.LogInformation("Reservation {ReservationId} set to Ready for Book {BookId}", nextReservation.Id, loan.BookId);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Book {BookId} returned by Patron {PatronId}, Loan {LoanId}", loan.BookId, loan.PatronId, loan.Id);
        return (await GetByIdAsync(loan.Id, ct))!;
    }

    public async Task<LoanResponse> RenewAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans.Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("Cannot renew a returned loan.");

        if (loan.Status == LoanStatus.Overdue || loan.DueDate < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot renew an overdue loan.");

        if (loan.RenewalCount >= MaxRenewals)
            throw new InvalidOperationException($"Maximum renewal limit ({MaxRenewals}) reached.");

        // Check for pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasPendingReservations)
            throw new InvalidOperationException("Cannot renew: there are pending reservations for this book.");

        // Check unpaid fines
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= FineThreshold)
            throw new InvalidOperationException($"Cannot renew: patron has ${unpaidFines:F2} in unpaid fines (threshold: ${FineThreshold:F2}).");

        var (_, loanDays) = GetLoanLimits(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Loan {LoanId} renewed (count: {Count})", loan.Id, loan.RenewalCount);
        return (await GetByIdAsync(loan.Id, ct))!;
    }

    public async Task<PaginatedResponse<LoanResponse>> GetOverdueAsync(int page, int pageSize, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Flag any active loans that are now overdue
        var newlyOverdue = await db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync(ct);

        foreach (var loan in newlyOverdue)
            loan.Status = LoanStatus.Overdue;

        if (newlyOverdue.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("{Count} loans flagged as overdue", newlyOverdue.Count);
        }

        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => MapToResponse(l))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    internal static LoanResponse MapToResponse(Loan l) => new()
    {
        Id = l.Id,
        BookId = l.BookId,
        BookTitle = l.Book.Title,
        BookISBN = l.Book.ISBN,
        PatronId = l.PatronId,
        PatronName = $"{l.Patron.FirstName} {l.Patron.LastName}",
        LoanDate = l.LoanDate,
        DueDate = l.DueDate,
        ReturnDate = l.ReturnDate,
        Status = l.Status,
        RenewalCount = l.RenewalCount,
        CreatedAt = l.CreatedAt
    };

    private static (int maxLoans, int loanDays) GetLoanLimits(MembershipType type) => type switch
    {
        MembershipType.Standard => (5, 14),
        MembershipType.Premium => (10, 21),
        MembershipType.Student => (3, 7),
        _ => (5, 14)
    };
}
