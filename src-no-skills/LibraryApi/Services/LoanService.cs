using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loan;
using LibraryApi.Models;
using LibraryApi.Models.Enums;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanService : ILoanService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<LoanService> _logger;

    public LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    {
        _db = db;
        _logger = logger;
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

    public async Task<PagedResult<LoanListDto>> GetAllAsync(LoanStatus? status, PaginationParams pagination)
    {
        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l => new LoanListDto(
                l.Id, l.Book.Title, $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync();

        return new PagedResult<LoanListDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<LoanDetailDto> GetByIdAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        return MapToDetail(loan);
    }

    public async Task<LoanDetailDto> CheckoutAsync(CreateLoanDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {dto.BookId} not found.");

        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new KeyNotFoundException($"Patron with ID {dto.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron account is inactive.");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException($"No available copies of '{book.Title}'.");

        // Check unpaid fines threshold
        var unpaidFineTotal = await _db.Fines
            .Where(f => f.PatronId == dto.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);

        if (unpaidFineTotal >= 10.00m)
            throw new InvalidOperationException($"Patron has ${unpaidFineTotal:F2} in unpaid fines. Must be under $10.00 to checkout.");

        // Check borrowing limit
        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == dto.PatronId && l.Status == LoanStatus.Active);
        var maxLoans = GetMaxLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            throw new InvalidOperationException($"Patron has reached the maximum of {maxLoans} active loans for {patron.MembershipType} membership.");

        var loanDays = GetLoanDays(patron.MembershipType);
        var loan = new Loan
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            DueDate = DateTime.UtcNow.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;
        book.UpdatedAt = DateTime.UtcNow;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Checkout: Book {BookId} to Patron {PatronId}, Loan {LoanId}, Due {DueDate}",
            dto.BookId, dto.PatronId, loan.Id, loan.DueDate);

        return await GetByIdAsync(loan.Id);
    }

    public async Task<ReturnLoanDto> ReturnAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        // Increment available copies
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Calculate overdue fine
        bool isOverdue = now > loan.DueDate;
        int overdueDays = 0;
        decimal? fineAmount = null;

        if (isOverdue)
        {
            overdueDays = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            fineAmount = overdueDays * 0.25m;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount.Value,
                Reason = $"Overdue return - {overdueDays} day(s) late",
                IssuedDate = now
            };
            _db.Fines.Add(fine);
        }

        // Check pending reservations and promote first to "Ready"
        var nextReservation = await _db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation != null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            _logger.LogInformation("Reservation {ReservationId} promoted to Ready for Book {BookId}", nextReservation.Id, loan.BookId);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Return: Loan {LoanId}, Overdue={IsOverdue}, Fine={FineAmount}",
            id, isOverdue, fineAmount);

        return new ReturnLoanDto(loan.Id, now, isOverdue, overdueDays, fineAmount);
    }

    public async Task<RenewLoanDto> RenewAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Patron)
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with ID {id} not found.");

        if (loan.Status != LoanStatus.Active)
            throw new InvalidOperationException("Only active loans can be renewed.");

        if (loan.RenewalCount >= 2)
            throw new InvalidOperationException("Maximum of 2 renewals reached.");

        if (DateTime.UtcNow > loan.DueDate)
            throw new InvalidOperationException("Cannot renew an overdue loan.");

        // Check for pending reservations
        var hasPendingReservations = await _db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId &&
                          (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasPendingReservations)
            throw new InvalidOperationException("Cannot renew because there are pending reservations for this book.");

        // Check unpaid fines threshold
        var unpaidFineTotal = await _db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount);

        if (unpaidFineTotal >= 10.00m)
            throw new InvalidOperationException($"Patron has ${unpaidFineTotal:F2} in unpaid fines. Must be under $10.00 to renew.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Renewed loan {LoanId}, new due date {DueDate}, renewal #{Count}",
            id, loan.DueDate, loan.RenewalCount);

        return new RenewLoanDto(loan.Id, loan.DueDate, loan.RenewalCount);
    }

    public async Task<List<LoanListDto>> GetOverdueLoansAsync()
    {
        var now = DateTime.UtcNow;

        // Also update status for loans that are past due
        var overdueLoans = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.ReturnDate == null && l.DueDate < now && l.Status == LoanStatus.Active)
            .ToListAsync();

        foreach (var loan in overdueLoans)
        {
            loan.Status = LoanStatus.Overdue;
        }
        await _db.SaveChangesAsync();

        // Return all overdue loans
        return await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate)
            .Select(l => new LoanListDto(
                l.Id, l.Book.Title, $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync();
    }

    private static LoanDetailDto MapToDetail(Loan loan)
    {
        return new LoanDetailDto(
            loan.Id, loan.BookId, loan.Book.Title, loan.Book.ISBN,
            loan.PatronId, $"{loan.Patron.FirstName} {loan.Patron.LastName}", loan.Patron.Email,
            loan.LoanDate, loan.DueDate, loan.ReturnDate,
            loan.Status, loan.RenewalCount, loan.CreatedAt);
    }
}
