using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<LoanService> _logger;

    public LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    {
        _db = db;
        _logger = logger;
    }

    private static int GetLoanDays(MembershipType type) => type switch
    {
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };

    private static int GetBorrowingLimit(MembershipType type) => type switch
    {
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    public async Task<PaginatedResponse<LoanDto>> GetAllAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 10)
    {
        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Overdue || (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null));

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => BookService.MapLoanDto(l)).ToListAsync();

        return new PaginatedResponse<LoanDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<LoanDto> GetByIdAsync(int id)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException($"Loan with ID {id} not found.");

        return BookService.MapLoanDto(loan);
    }

    public async Task<LoanDto> CheckoutAsync(CreateLoanDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new NotFoundException($"Book with ID {dto.BookId} not found.");
        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new NotFoundException($"Patron with ID {dto.PatronId} not found.");

        if (!patron.IsActive)
            throw new BusinessRuleException("Patron account is inactive.");

        if (book.AvailableCopies <= 0)
            throw new BusinessRuleException("No available copies of this book.");

        var unpaidFines = await _db.Fines.Where(f => f.PatronId == dto.PatronId && f.Status == FineStatus.Unpaid).SumAsync(f => f.Amount);
        if (unpaidFines >= 10m)
            throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines. Must be under $10.00 to checkout.");

        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == dto.PatronId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        var limit = GetBorrowingLimit(patron.MembershipType);
        if (activeLoans >= limit)
            throw new BusinessRuleException($"Patron has reached the borrowing limit of {limit} active loans for {patron.MembershipType} membership.");

        var loanDays = GetLoanDays(patron.MembershipType);
        var loan = new Loan
        {
            BookId = dto.BookId, PatronId = dto.PatronId,
            LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;
        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Book {BookId} checked out to Patron {PatronId}. Loan ID: {LoanId}", dto.BookId, dto.PatronId, loan.Id);

        return await GetByIdAsync(loan.Id);
    }

    public async Task<LoanDto> ReturnAsync(int loanId)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId)
            ?? throw new NotFoundException($"Loan with ID {loanId} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new BusinessRuleException("This loan has already been returned.");

        loan.ReturnDate = DateTime.UtcNow;
        loan.Status = LoanStatus.Returned;
        loan.Book.AvailableCopies++;

        // Check if overdue and create fine
        if (loan.DueDate < DateTime.UtcNow)
        {
            var overdueDays = (int)(DateTime.UtcNow - loan.DueDate).TotalDays;
            var fineAmount = overdueDays * 0.25m;
            var fine = new Fine
            {
                PatronId = loan.PatronId, LoanId = loan.Id,
                Amount = fineAmount, Reason = $"Overdue return - {overdueDays} days late",
                Status = FineStatus.Unpaid
            };
            _db.Fines.Add(fine);
            _logger.LogInformation("Fine of ${Amount} created for Patron {PatronId} on Loan {LoanId}", fineAmount, loan.PatronId, loan.Id);
        }

        // Check pending reservations for this book
        var nextReservation = await _db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation != null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = DateTime.UtcNow.AddDays(3);
            _logger.LogInformation("Reservation {ReservationId} moved to Ready status", nextReservation.Id);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Book {BookId} returned by Patron {PatronId}. Loan ID: {LoanId}", loan.BookId, loan.PatronId, loan.Id);

        return BookService.MapLoanDto(loan);
    }

    public async Task<LoanDto> RenewAsync(int loanId)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == loanId)
            ?? throw new NotFoundException($"Loan with ID {loanId} not found.");

        if (loan.Status != LoanStatus.Active)
            throw new BusinessRuleException("Only active loans can be renewed.");

        if (loan.RenewalCount >= 2)
            throw new BusinessRuleException("Maximum renewal limit of 2 has been reached.");

        if (loan.DueDate < DateTime.UtcNow)
            throw new BusinessRuleException("Cannot renew an overdue loan.");

        // Check for pending reservations
        var hasPendingReservations = await _db.Reservations.AnyAsync(r =>
            r.BookId == loan.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasPendingReservations)
            throw new BusinessRuleException("Cannot renew: there are pending reservations for this book.");

        // Check fine threshold
        var unpaidFines = await _db.Fines.Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid).SumAsync(f => f.Amount);
        if (unpaidFines >= 10m)
            throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines. Must be under $10.00 to renew.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Loan {LoanId} renewed. New due date: {DueDate}", loan.Id, loan.DueDate);

        return BookService.MapLoanDto(loan);
    }

    public async Task<List<LoanDto>> GetOverdueAsync()
    {
        // Also flag active loans that are past due
        var overdueActive = await _db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null)
            .ToListAsync();

        foreach (var loan in overdueActive)
        {
            loan.Status = LoanStatus.Overdue;
        }

        if (overdueActive.Count > 0)
        {
            await _db.SaveChangesAsync();
            _logger.LogInformation("Flagged {Count} loans as overdue", overdueActive.Count);
        }

        var allOverdue = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate)
            .Select(l => BookService.MapLoanDto(l))
            .ToListAsync();

        return allOverdue;
    }
}
