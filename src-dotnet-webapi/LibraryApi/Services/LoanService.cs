using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
{
    private const decimal OverdueFinePerDay = 0.25m;
    private const decimal FineThreshold = 10.00m;

    public async Task<PaginatedResponse<LoanResponse>> GetAllAsync(
        LoanStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<LoanDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Loans.AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new LoanDetailResponse(
                l.Id, l.BookId, l.Book.Title, l.Book.ISBN,
                l.PatronId, $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.Patron.Email,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt,
                l.Fines.Select(f => new FineResponse(
                    f.Id, f.PatronId, $"{f.Patron.FirstName} {f.Patron.LastName}",
                    f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt)).ToList()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<LoanResponse> CheckoutAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron account is not active.");

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines.AsNoTracking()
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= FineThreshold)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold: ${FineThreshold:F2}). Please pay fines before borrowing.");

        // Check available copies
        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException($"No available copies of '{book.Title}'.");

        // Check borrowing limit
        var activeLoans = await db.Loans.AsNoTracking()
            .CountAsync(l => l.PatronId == patron.Id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        var maxLoans = GetMaxLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            throw new InvalidOperationException($"Patron has reached the borrowing limit of {maxLoans} for {patron.MembershipType} membership.");

        // Check not already borrowing this book
        var alreadyBorrowing = await db.Loans.AsNoTracking()
            .AnyAsync(l => l.BookId == book.Id && l.PatronId == patron.Id &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);
        if (alreadyBorrowing)
            throw new InvalidOperationException($"Patron already has an active loan for '{book.Title}'.");

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
        book.UpdatedAt = now;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Checkout: Patron {PatronId} borrowed Book {BookId} (Loan {LoanId})", patron.Id, book.Id, loan.Id);

        return new LoanResponse(
            loan.Id, loan.BookId, book.Title, loan.PatronId,
            $"{patron.FirstName} {patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);
    }

    public async Task<LoanResponse> ReturnAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Auto-generate fine for overdue returns
        if (now > loan.DueDate)
        {
            var daysOverdue = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = daysOverdue * OverdueFinePerDay;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue: {daysOverdue} day{(daysOverdue != 1 ? "s" : "")} late",
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            };
            db.Fines.Add(fine);
            logger.LogInformation("Fine generated: ${Amount} for Loan {LoanId} ({Days} days overdue)", fineAmount, loan.Id, daysOverdue);
        }

        // Promote first pending reservation to Ready
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            loan.Book.AvailableCopies--; // Hold copy for reservation
            logger.LogInformation("Reservation {ReservationId} promoted to Ready for Book {BookId}", nextReservation.Id, loan.BookId);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Return: Loan {LoanId} returned", loan.Id);

        return new LoanResponse(
            loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            $"{loan.Patron.FirstName} {loan.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);
    }

    public async Task<LoanResponse> RenewAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("Cannot renew a returned loan.");

        if (loan.Status == LoanStatus.Overdue)
            throw new InvalidOperationException("Cannot renew an overdue loan. Please return the book first.");

        if (loan.RenewalCount >= 2)
            throw new InvalidOperationException("Maximum number of renewals (2) reached.");

        // Check fine threshold
        var unpaidFines = await db.Fines.AsNoTracking()
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= FineThreshold)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines. Please pay fines before renewing.");

        // Check pending reservations
        var hasPendingReservations = await db.Reservations.AsNoTracking()
            .AnyAsync(r => r.BookId == loan.BookId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasPendingReservations)
            throw new InvalidOperationException("Cannot renew because there are pending reservations for this book.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Renewed: Loan {LoanId} (renewal #{Count})", loan.Id, loan.RenewalCount);

        return new LoanResponse(
            loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            $"{loan.Patron.FirstName} {loan.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);
    }

    public async Task<IReadOnlyList<LoanResponse>> GetOverdueAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Update status for loans that are past due
        var overdueLoans = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync(ct);

        foreach (var loan in overdueLoans)
        {
            loan.Status = LoanStatus.Overdue;
        }

        if (overdueLoans.Count > 0)
            await db.SaveChangesAsync(ct);

        // Return all overdue loans
        return await db.Loans.AsNoTracking()
            .Where(l => l.Status == LoanStatus.Overdue)
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);
    }

    private static int GetMaxLoans(MembershipType type) => type switch
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
}
