using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class LoanService(LibraryDbContext context, ILogger<LoanService> logger) : ILoanService
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

    public async Task<PaginatedResponse<LoanResponse>> GetLoansAsync(
        string? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct)
    {
        var query = context.Loans.Include(l => l.Book).Include(l => l.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
        {
            query = query.Where(l => l.Status == loanStatus);
        }

        if (overdue == true)
        {
            query = query.Where(l => l.Status == LoanStatus.Overdue || (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null));
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
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync(ct);

        return new PaginatedResponse<LoanResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public async Task<LoanDetailResponse?> GetLoanByIdAsync(int id, CancellationToken ct)
    {
        var loan = await context.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (loan is null)
        {
            return null;
        }

        return MapToDetail(loan);
    }

    public async Task<Result<LoanDetailResponse>> CheckoutBookAsync(CreateLoanRequest request, CancellationToken ct)
    {
        var book = await context.Books.FindAsync([request.BookId], ct);
        if (book is null)
        {
            return Result<LoanDetailResponse>.Failure("Book not found.", 404);
        }

        var patron = await context.Patrons.FindAsync([request.PatronId], ct);
        if (patron is null)
        {
            return Result<LoanDetailResponse>.Failure("Patron not found.", 404);
        }

        if (!patron.IsActive)
        {
            return Result<LoanDetailResponse>.Failure("Patron membership is not active.");
        }

        if (book.AvailableCopies < 1)
        {
            return Result<LoanDetailResponse>.Failure("No available copies of this book.");
        }

        var unpaidFines = await context.Fines
            .Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10.00m)
        {
            return Result<LoanDetailResponse>.Failure($"Patron has ${unpaidFines:F2} in unpaid fines (limit is $10.00). Please pay outstanding fines first.");
        }

        var activeLoans = await context.Loans.CountAsync(l => l.PatronId == patron.Id && l.Status == LoanStatus.Active, ct);
        var maxLoans = GetMaxLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
        {
            return Result<LoanDetailResponse>.Failure($"Patron has reached the maximum of {maxLoans} active loans for {patron.MembershipType} membership.");
        }

        var loanDays = GetLoanDays(patron.MembershipType);
        var now = DateTime.UtcNow;

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

        context.Loans.Add(loan);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Book {BookId} checked out to patron {PatronId}, loan {LoanId}", book.Id, patron.Id, loan.Id);

        var result = await context.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .FirstAsync(l => l.Id == loan.Id, ct);

        return Result<LoanDetailResponse>.Success(MapToDetail(result));
    }

    public async Task<Result<LoanDetailResponse>> ReturnBookAsync(int loanId, CancellationToken ct)
    {
        var loan = await context.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId, ct);

        if (loan is null)
        {
            return Result<LoanDetailResponse>.Failure("Loan not found.", 404);
        }

        if (loan.Status == LoanStatus.Returned)
        {
            return Result<LoanDetailResponse>.Failure("This loan has already been returned.");
        }

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Check for overdue and create fine
        if (now > loan.DueDate)
        {
            var daysOverdue = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = daysOverdue * 0.25m;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue return — {daysOverdue} day(s) late",
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            };

            context.Fines.Add(fine);
            logger.LogInformation("Fine of ${Amount:F2} issued to patron {PatronId} for overdue loan {LoanId}", fineAmount, loan.PatronId, loan.Id);
        }

        // Check for pending reservations
        var nextReservation = await context.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync(ct);

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            logger.LogInformation("Reservation {ReservationId} for book {BookId} moved to Ready status", nextReservation.Id, loan.BookId);
        }

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Book {BookId} returned by patron {PatronId}, loan {LoanId}", loan.BookId, loan.PatronId, loan.Id);
        return Result<LoanDetailResponse>.Success(MapToDetail(loan));
    }

    public async Task<Result<LoanDetailResponse>> RenewLoanAsync(int loanId, CancellationToken ct)
    {
        var loan = await context.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId, ct);

        if (loan is null)
        {
            return Result<LoanDetailResponse>.Failure("Loan not found.", 404);
        }

        if (loan.Status == LoanStatus.Returned)
        {
            return Result<LoanDetailResponse>.Failure("Cannot renew a returned loan.");
        }

        if (loan.Status == LoanStatus.Overdue || (loan.DueDate < DateTime.UtcNow && loan.ReturnDate is null))
        {
            return Result<LoanDetailResponse>.Failure("Cannot renew an overdue loan.");
        }

        if (loan.RenewalCount >= 2)
        {
            return Result<LoanDetailResponse>.Failure("Maximum renewal limit (2) reached.");
        }

        // Check unpaid fines threshold
        var unpaidFines = await context.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10.00m)
        {
            return Result<LoanDetailResponse>.Failure($"Patron has ${unpaidFines:F2} in unpaid fines. Please pay outstanding fines first.");
        }

        var hasPendingReservations = await context.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending, ct);

        if (hasPendingReservations)
        {
            return Result<LoanDetailResponse>.Failure("Cannot renew — there are pending reservations for this book.");
        }

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await context.SaveChangesAsync(ct);

        logger.LogInformation("Loan {LoanId} renewed (count: {RenewalCount})", loan.Id, loan.RenewalCount);
        return Result<LoanDetailResponse>.Success(MapToDetail(loan));
    }

    public async Task<PaginatedResponse<LoanResponse>> GetOverdueLoansAsync(int page, int pageSize, CancellationToken ct)
    {
        // Also flag any active loans past due date
        var overdueActive = await context.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null)
            .ToListAsync(ct);

        foreach (var loan in overdueActive)
        {
            loan.Status = LoanStatus.Overdue;
        }

        if (overdueActive.Count > 0)
        {
            await context.SaveChangesAsync(ct);
        }

        var query = context.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync(ct);

        return new PaginatedResponse<LoanResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    private static LoanDetailResponse MapToDetail(Loan loan) =>
        new(loan.Id, loan.BookId, loan.Book.Title, loan.Book.ISBN,
            loan.PatronId, loan.Patron.FirstName + " " + loan.Patron.LastName,
            loan.Patron.Email, loan.LoanDate, loan.DueDate, loan.ReturnDate,
            loan.Status, loan.RenewalCount, loan.CreatedAt);
}
