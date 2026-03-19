using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Middleware;
using LibraryApi.Models;
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

    public static int GetLoanPeriodDays(MembershipType type) => type switch
    {
        MembershipType.Standard => 14,
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };

    public static int GetMaxActiveLoans(MembershipType type) => type switch
    {
        MembershipType.Standard => 5,
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    public async Task<PagedResult<LoanDto>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null);

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.LoanDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<LoanDto> GetLoanByIdAsync(int id)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException($"Loan with ID {id} not found.");
        return MapToDto(loan);
    }

    public async Task<LoanDto> CheckoutBookAsync(LoanCreateDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new NotFoundException($"Book with ID {dto.BookId} not found.");
        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new NotFoundException($"Patron with ID {dto.PatronId} not found.");

        // Validate checkout rules
        if (!patron.IsActive)
            throw new BusinessRuleException("Patron's membership is not active.");

        if (book.AvailableCopies <= 0)
            throw new BusinessRuleException($"No available copies of '{book.Title}'.");

        var unpaidFines = await _db.Fines.Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid).SumAsync(f => (decimal?)f.Amount) ?? 0m;
        if (unpaidFines >= 10.00m)
            throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold is $10.00). Please pay outstanding fines before checking out.");

        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == patron.Id && l.Status == LoanStatus.Active);
        var maxLoans = GetMaxActiveLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            throw new BusinessRuleException($"Patron has reached the maximum of {maxLoans} active loans for {patron.MembershipType} membership.");

        var loanPeriod = GetLoanPeriodDays(patron.MembershipType);
        var now = DateTime.UtcNow;

        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanPeriod),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        book.AvailableCopies--;
        book.UpdatedAt = now;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Book checked out: LoanId={LoanId}, BookId={BookId}, PatronId={PatronId}, DueDate={DueDate}",
            loan.Id, book.Id, patron.Id, loan.DueDate);

        return await ReloadLoanDto(loan.Id);
    }

    public async Task<LoanDto> ReturnBookAsync(int id)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new BusinessRuleException("This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        // Increment available copies
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Check for overdue fine
        if (now > loan.DueDate)
        {
            var overdueDays = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = overdueDays * 0.25m;
            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue return - {overdueDays} day(s) late",
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            };
            _db.Fines.Add(fine);
            _logger.LogInformation("Fine issued: PatronId={PatronId}, LoanId={LoanId}, Amount=${Amount}, Days={Days}",
                loan.PatronId, loan.Id, fineAmount, overdueDays);
        }

        // Check for pending reservations
        var nextReservation = await _db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation != null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            _logger.LogInformation("Reservation ready: ReservationId={ReservationId}, BookId={BookId}, PatronId={PatronId}",
                nextReservation.Id, nextReservation.BookId, nextReservation.PatronId);
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Book returned: LoanId={LoanId}, BookId={BookId}, PatronId={PatronId}",
            loan.Id, loan.BookId, loan.PatronId);

        return await ReloadLoanDto(loan.Id);
    }

    public async Task<LoanDto> RenewLoanAsync(int id)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException($"Loan with ID {id} not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new BusinessRuleException("Cannot renew a returned loan.");

        if (loan.Status == LoanStatus.Overdue || (loan.DueDate < DateTime.UtcNow && loan.ReturnDate == null))
            throw new BusinessRuleException("Cannot renew an overdue loan.");

        if (loan.RenewalCount >= 2)
            throw new BusinessRuleException("Maximum renewal limit (2) has been reached.");

        // Check for pending reservations on the book
        var hasPendingReservations = await _db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending);
        if (hasPendingReservations)
            throw new BusinessRuleException("Cannot renew this loan because there are pending reservations for the book.");

        // Check fine threshold
        var unpaidFines = await _db.Fines.Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid).SumAsync(f => (decimal?)f.Amount) ?? 0m;
        if (unpaidFines >= 10.00m)
            throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines. Cannot renew.");

        var loanPeriod = GetLoanPeriodDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanPeriod);
        loan.RenewalCount++;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Loan renewed: LoanId={LoanId}, NewDueDate={DueDate}, RenewalCount={RenewalCount}",
            loan.Id, loan.DueDate, loan.RenewalCount);

        return await ReloadLoanDto(loan.Id);
    }

    public async Task<PagedResult<LoanDto>> GetOverdueLoansAsync(int page, int pageSize)
    {
        var now = DateTime.UtcNow;

        // Also flag overdue loans
        var overdueLoans = await _db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync();
        foreach (var loan in overdueLoans)
            loan.Status = LoanStatus.Overdue;
        if (overdueLoans.Any())
            await _db.SaveChangesAsync();

        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue && l.ReturnDate == null)
            .OrderBy(l => l.DueDate);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    private async Task<LoanDto> ReloadLoanDto(int loanId)
    {
        var loan = await _db.Loans.Include(l => l.Book).Include(l => l.Patron).FirstAsync(l => l.Id == loanId);
        return MapToDto(loan);
    }

    internal static LoanDto MapToDto(Loan l) => new()
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
        Status = l.Status.ToString(),
        RenewalCount = l.RenewalCount,
        CreatedAt = l.CreatedAt
    };
}
