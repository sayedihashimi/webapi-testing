using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
{
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

    public async Task<PaginatedResponse<LoanResponse>> GetAllAsync(
        LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Loans.AsNoTracking().AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null);

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status,
                l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<LoanDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Loans.AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new LoanDetailResponse(
                l.Id, l.BookId, l.Book.Title, l.Book.ISBN,
                l.PatronId, l.Patron.FirstName + " " + l.Patron.LastName,
                l.Patron.Email, l.LoanDate, l.DueDate, l.ReturnDate,
                l.Status, l.RenewalCount, l.CreatedAt))
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

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException($"No available copies of '{book.Title}'.");

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);
        if (unpaidFines >= 10.00m)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold: $10.00). Please pay fines before checking out.");

        // Check borrowing limit
        var activeLoans = await db.Loans.CountAsync(
            l => l.PatronId == patron.Id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);
        var maxLoans = GetMaxLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            throw new InvalidOperationException($"Patron has reached the maximum borrowing limit ({maxLoans}) for {patron.MembershipType} membership.");

        var loanDays = GetLoanDays(patron.MembershipType);
        var loan = new Loan
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;
        book.UpdatedAt = DateTime.UtcNow;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Checkout: Patron {PatronId} borrowed Book {BookId}, due {DueDate}",
            patron.Id, book.Id, loan.DueDate);

        return new LoanResponse(
            loan.Id, loan.BookId, book.Title, loan.PatronId,
            patron.FirstName + " " + patron.LastName,
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status,
            loan.RenewalCount, loan.CreatedAt);
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

        loan.ReturnDate = DateTime.UtcNow;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = DateTime.UtcNow;

        // Generate fine if overdue
        if (loan.ReturnDate > loan.DueDate)
        {
            var daysOverdue = (int)(loan.ReturnDate.Value - loan.DueDate).TotalDays;
            var fineAmount = daysOverdue * 0.25m;
            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue book: {loan.Book.Title} ({daysOverdue} day(s) late)",
                Status = FineStatus.Unpaid
            };
            db.Fines.Add(fine);
            logger.LogInformation("Generated fine of ${Amount} for patron {PatronId} (loan {LoanId})",
                fineAmount, loan.PatronId, loan.Id);
        }

        // Check pending reservations for this book
        var nextReservation = await db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = DateTime.UtcNow.AddDays(3);
            logger.LogInformation("Reservation {ReservationId} is now Ready for pickup (expires {Expiration})",
                nextReservation.Id, nextReservation.ExpirationDate);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Return: Loan {LoanId} returned by patron {PatronId}", loan.Id, loan.PatronId);

        return new LoanResponse(
            loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            loan.Patron.FirstName + " " + loan.Patron.LastName,
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status,
            loan.RenewalCount, loan.CreatedAt);
    }

    public async Task<LoanResponse> RenewAsync(int id, CancellationToken ct)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status != LoanStatus.Active)
            throw new InvalidOperationException("Only active loans can be renewed.");

        if (loan.DueDate < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot renew an overdue loan.");

        if (loan.RenewalCount >= 2)
            throw new InvalidOperationException("Maximum number of renewals (2) reached.");

        // Check pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId &&
                          (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);
        if (hasPendingReservations)
            throw new InvalidOperationException("Cannot renew: there are pending reservations for this book.");

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);
        if (unpaidFines >= 10.00m)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines. Please pay fines before renewing.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Renewed loan {LoanId}, new due date: {DueDate}", loan.Id, loan.DueDate);

        return new LoanResponse(
            loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            loan.Patron.FirstName + " " + loan.Patron.LastName,
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status,
            loan.RenewalCount, loan.CreatedAt);
    }

    public async Task<IReadOnlyList<LoanResponse>> GetOverdueAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Also update status of loans that are past due
        var overdueLoans = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync(ct);

        foreach (var loan in overdueLoans)
            loan.Status = LoanStatus.Overdue;

        if (overdueLoans.Count > 0)
            await db.SaveChangesAsync(ct);

        var allOverdue = await db.Loans.AsNoTracking()
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status,
                l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);

        return allOverdue;
    }
}
