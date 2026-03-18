using LibraryApi.Data;
using LibraryApi.DTOs;
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

    private static int GetLoanPeriodDays(MembershipType type) => type switch
    {
        MembershipType.Standard => 14,
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };

    private static int GetMaxActiveLoans(MembershipType type) => type switch
    {
        MembershipType.Standard => 5,
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    public async Task<PagedResult<LoanDto>> GetLoansAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, PaginationParams pagination)
    {
        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
        {
            query = query.Where(l => l.Status == ls);
        }

        if (overdue == true)
        {
            query = query.Where(l => l.Status == LoanStatus.Overdue || (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null));
        }

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                PatronId = l.PatronId,
                PatronName = l.Patron.FirstName + " " + l.Patron.LastName,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                Status = l.Status,
                RenewalCount = l.RenewalCount,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<LoanDto> GetLoanByIdAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException($"Loan with ID {id} not found");

        return MapToDto(loan);
    }

    public async Task<LoanDto> CheckoutBookAsync(CreateLoanDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new NotFoundException($"Book with ID {dto.BookId} not found");

        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new NotFoundException($"Patron with ID {dto.PatronId} not found");

        // Enforce checkout rules
        if (!patron.IsActive)
            throw new BusinessRuleException("Patron's membership is inactive. Cannot check out books.");

        if (book.AvailableCopies <= 0)
            throw new BusinessRuleException("No available copies of this book.");

        var unpaidFines = await _db.Fines
            .Where(f => f.PatronId == dto.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0;

        if (unpaidFines >= 10.00m)
            throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold: $10.00). Please pay outstanding fines before checking out books.");

        var activeLoansCount = await _db.Loans.CountAsync(l => l.PatronId == dto.PatronId && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        var maxLoans = GetMaxActiveLoans(patron.MembershipType);

        if (activeLoansCount >= maxLoans)
            throw new BusinessRuleException($"Patron has reached the maximum number of active loans ({maxLoans}) for {patron.MembershipType} membership.");

        var loanPeriodDays = GetLoanPeriodDays(patron.MembershipType);
        var now = DateTime.UtcNow;

        var loan = new Loan
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            LoanDate = now,
            DueDate = now.AddDays(loanPeriodDays),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        book.AvailableCopies--;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Book checked out: BookId={BookId}, PatronId={PatronId}, LoanId={LoanId}, DueDate={DueDate}",
            loan.BookId, loan.PatronId, loan.Id, loan.DueDate);

        // Reload with includes
        await _db.Entry(loan).Reference(l => l.Book).LoadAsync();
        await _db.Entry(loan).Reference(l => l.Patron).LoadAsync();

        return MapToDto(loan);
    }

    public async Task<LoanDto> ReturnBookAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException($"Loan with ID {id} not found");

        if (loan.Status == LoanStatus.Returned)
            throw new BusinessRuleException("This book has already been returned.");

        var now = DateTime.UtcNow;

        // 1. Set return date and status
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        // 2. Increment available copies
        loan.Book.AvailableCopies++;

        // 3. If overdue, generate a fine
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

        await _db.SaveChangesAsync();

        // 4. Check for pending reservations and activate the next one
        var nextReservation = await _db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation != null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Reservation ready: ReservationId={ReservationId}, BookId={BookId}, PatronId={PatronId}",
                nextReservation.Id, nextReservation.BookId, nextReservation.PatronId);
        }

        _logger.LogInformation("Book returned: LoanId={LoanId}, BookId={BookId}, PatronId={PatronId}",
            loan.Id, loan.BookId, loan.PatronId);

        return MapToDto(loan);
    }

    public async Task<LoanDto> RenewLoanAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new NotFoundException($"Loan with ID {id} not found");

        if (loan.Status == LoanStatus.Returned)
            throw new BusinessRuleException("Cannot renew a returned loan.");

        if (loan.Status == LoanStatus.Overdue || (loan.DueDate < DateTime.UtcNow && loan.ReturnDate == null))
            throw new BusinessRuleException("Cannot renew an overdue loan. Please return the book first.");

        if (loan.RenewalCount >= 2)
            throw new BusinessRuleException("Maximum renewal limit (2) has been reached for this loan.");

        // Check for pending reservations
        var hasPendingReservations = await _db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasPendingReservations)
            throw new BusinessRuleException("Cannot renew this loan because there are pending reservations for this book.");

        // Check fine threshold
        var unpaidFines = await _db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0;

        if (unpaidFines >= 10.00m)
            throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold: $10.00). Please pay outstanding fines before renewing.");

        var loanPeriodDays = GetLoanPeriodDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanPeriodDays);
        loan.RenewalCount++;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Loan renewed: LoanId={LoanId}, NewDueDate={DueDate}, RenewalCount={Count}",
            loan.Id, loan.DueDate, loan.RenewalCount);

        return MapToDto(loan);
    }

    public async Task<PagedResult<LoanDto>> GetOverdueLoansAsync(PaginationParams pagination)
    {
        // Also flag overdue loans
        var overdueLoans = await _db.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null)
            .ToListAsync();

        foreach (var loan in overdueLoans)
        {
            loan.Status = LoanStatus.Overdue;
        }

        if (overdueLoans.Any())
            await _db.SaveChangesAsync();

        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(l => l.DueDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                PatronId = l.PatronId,
                PatronName = l.Patron.FirstName + " " + l.Patron.LastName,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                Status = l.Status,
                RenewalCount = l.RenewalCount,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    private static LoanDto MapToDto(Loan loan) => new()
    {
        Id = loan.Id,
        BookId = loan.BookId,
        BookTitle = loan.Book.Title,
        PatronId = loan.PatronId,
        PatronName = loan.Patron.FirstName + " " + loan.Patron.LastName,
        LoanDate = loan.LoanDate,
        DueDate = loan.DueDate,
        ReturnDate = loan.ReturnDate,
        Status = loan.Status,
        RenewalCount = loan.RenewalCount,
        CreatedAt = loan.CreatedAt
    };
}
