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

    public async Task<PagedResult<LoanDto>> GetLoansAsync(
        string? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
        {
            query = query.Where(l => l.Status == loanStatus);
        }

        if (overdue == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(l => l.DueDate < now && l.ReturnDate == null);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.LoanDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.LoanDate <= toDate.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<LoanDto>(items, totalCount, page, pageSize);
    }

    public async Task<LoanDetailDto?> GetLoanByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Id == id)
            .Select(l => new LoanDetailDto(
                l.Id, l.BookId, l.Book.Title, l.Book.ISBN,
                l.PatronId, $"{l.Patron.FirstName} {l.Patron.LastName}", l.Patron.Email,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(LoanDto? Loan, string? Error)> CheckoutBookAsync(CreateLoanDto dto, CancellationToken ct = default)
    {
        var book = await db.Books.FindAsync([dto.BookId], ct);
        if (book is null)
        {
            return (null, "Book not found.");
        }

        var patron = await db.Patrons.FindAsync([dto.PatronId], ct);
        if (patron is null)
        {
            return (null, "Patron not found.");
        }

        if (!patron.IsActive)
        {
            return (null, "Patron account is inactive.");
        }

        if (book.AvailableCopies <= 0)
        {
            return (null, "No available copies of this book.");
        }

        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == dto.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10m)
        {
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines. Must be below $10.00 to checkout.");
        }

        var activeLoansCount = await db.Loans
            .CountAsync(l => l.PatronId == dto.PatronId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        var maxLoans = GetMaxLoans(patron.MembershipType);
        if (activeLoansCount >= maxLoans)
        {
            return (null, $"Patron has reached the maximum of {maxLoans} loans for {patron.MembershipType} membership.");
        }

        var loanDays = GetLoanDays(patron.MembershipType);
        var now = DateTime.UtcNow;

        var loan = new Loan
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Checkout: Book {BookId} to Patron {PatronId}, Loan {LoanId}", dto.BookId, dto.PatronId, loan.Id);

        var loanDto = new LoanDto(
            loan.Id, loan.BookId, book.Title, loan.PatronId,
            $"{patron.FirstName} {patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);

        return (loanDto, null);
    }

    public async Task<(LoanDto? Loan, string? Error)> ReturnBookAsync(int id, CancellationToken ct = default)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (loan is null)
        {
            return (null, "Loan not found.");
        }

        if (loan.Status == LoanStatus.Returned)
        {
            return (null, "This book has already been returned.");
        }

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Generate fine if overdue
        if (loan.DueDate < now)
        {
            var daysOverdue = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = daysOverdue * 0.25m;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue: {daysOverdue} day(s) late",
                Status = FineStatus.Unpaid
            };

            db.Fines.Add(fine);
            logger.LogInformation("Fine generated: ${Amount} for Loan {LoanId}", fineAmount, loan.Id);
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
            logger.LogInformation("Reservation {ReservationId} promoted to Ready", nextReservation.Id);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Return: Loan {LoanId} returned", loan.Id);

        var loanDto = new LoanDto(
            loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            $"{loan.Patron.FirstName} {loan.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);

        return (loanDto, null);
    }

    public async Task<(LoanDto? Loan, string? Error)> RenewLoanAsync(int id, CancellationToken ct = default)
    {
        var loan = await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (loan is null)
        {
            return (null, "Loan not found.");
        }

        if (loan.Status != LoanStatus.Active)
        {
            return (null, "Only active loans can be renewed.");
        }

        if (loan.RenewalCount >= 2)
        {
            return (null, "Maximum renewal limit (2) reached.");
        }

        // Check if overdue
        if (loan.DueDate < DateTime.UtcNow)
        {
            return (null, "Overdue loans cannot be renewed.");
        }

        // Check for pending reservations
        var hasPendingReservations = await db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasPendingReservations)
        {
            return (null, "Cannot renew: there are pending reservations for this book.");
        }

        // Check unpaid fines threshold
        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10m)
        {
            return (null, $"Cannot renew: patron has ${unpaidFines:F2} in unpaid fines.");
        }

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Renewal: Loan {LoanId}, renewal #{Count}", loan.Id, loan.RenewalCount);

        var loanDto = new LoanDto(
            loan.Id, loan.BookId, loan.Book.Title, loan.PatronId,
            $"{loan.Patron.FirstName} {loan.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);

        return (loanDto, null);
    }

    public async Task<List<LoanDto>> GetOverdueLoansAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.DueDate < now && l.ReturnDate == null)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanDto(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt
            ))
            .ToListAsync(ct);
    }
}
