using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanService : ILoanService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<LoanService> _logger;

    private const decimal FinePerDay = 0.25m;
    private const decimal FineThreshold = 10.00m;
    private const int MaxRenewals = 2;

    public LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public static int GetLoanDays(MembershipType type) => type switch
    {
        MembershipType.Standard => 14,
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };

    public static int GetMaxLoans(MembershipType type) => type switch
    {
        MembershipType.Standard => 5,
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    public async Task<PagedResult<LoanDto>> GetAllAsync(LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null);

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => MapToDto(l))
            .ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<LoanDto?> GetByIdAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id);

        return loan is null ? null : MapToDto(loan);
    }

    public async Task<LoanDto> CheckoutAsync(LoanCreateDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new KeyNotFoundException($"Book with id {dto.BookId} not found");

        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new KeyNotFoundException($"Patron with id {dto.PatronId} not found");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron account is not active");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException("No copies available for checkout");

        // Check unpaid fines
        var unpaidFines = await _db.Fines
            .Where(f => f.PatronId == dto.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0m;

        if (unpaidFines >= FineThreshold)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines (threshold: ${FineThreshold:F2}). Please pay fines before checking out.");

        // Check borrowing limit
        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == dto.PatronId && l.Status == LoanStatus.Active);
        var maxLoans = GetMaxLoans(patron.MembershipType);

        if (activeLoans >= maxLoans)
            throw new InvalidOperationException($"Patron has reached the maximum of {maxLoans} active loans for {patron.MembershipType} membership");

        var loanDays = GetLoanDays(patron.MembershipType);
        var loan = new Loan
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        book.AvailableCopies--;
        book.UpdatedAt = DateTime.UtcNow;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Checkout: Book {BookId} to Patron {PatronId}, Loan {LoanId}, Due {DueDate}",
            dto.BookId, dto.PatronId, loan.Id, loan.DueDate);

        // Reload with navigation properties
        await _db.Entry(loan).Reference(l => l.Book).LoadAsync();
        await _db.Entry(loan).Reference(l => l.Patron).LoadAsync();

        return MapToDto(loan);
    }

    public async Task<LoanDto> ReturnAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with id {id} not found");

        if (loan.Status == LoanStatus.Returned)
            throw new InvalidOperationException("This loan has already been returned");

        loan.ReturnDate = DateTime.UtcNow;
        loan.Status = LoanStatus.Returned;

        // Increment available copies
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = DateTime.UtcNow;

        // Check if overdue and generate fine
        if (loan.DueDate < DateTime.UtcNow)
        {
            var daysOverdue = (int)(DateTime.UtcNow - loan.DueDate).TotalDays;
            if (daysOverdue > 0)
            {
                var fineAmount = daysOverdue * FinePerDay;
                var fine = new Fine
                {
                    PatronId = loan.PatronId,
                    LoanId = loan.Id,
                    Amount = fineAmount,
                    Reason = $"Overdue by {daysOverdue} day(s) at ${FinePerDay}/day",
                    IssuedDate = DateTime.UtcNow,
                    Status = FineStatus.Unpaid
                };
                _db.Fines.Add(fine);
                _logger.LogInformation("Generated fine of ${Amount} for overdue loan {LoanId}", fineAmount, id);
            }
        }

        // Check pending reservations for this book and promote first to Ready
        var nextReservation = await _db.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation is not null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = DateTime.UtcNow.AddDays(3);
            _logger.LogInformation("Reservation {ReservationId} is now Ready, expires {Expiration}",
                nextReservation.Id, nextReservation.ExpirationDate);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Returned: Loan {LoanId}, Book {BookId}", id, loan.BookId);

        return MapToDto(loan);
    }

    public async Task<LoanDto> RenewAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Loan with id {id} not found");

        if (loan.Status != LoanStatus.Active)
            throw new InvalidOperationException("Only active loans can be renewed");

        if (loan.DueDate < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot renew an overdue loan");

        if (loan.RenewalCount >= MaxRenewals)
            throw new InvalidOperationException($"Maximum of {MaxRenewals} renewals reached");

        // Check if there are pending reservations for this book
        var hasPendingReservations = await _db.Reservations
            .AnyAsync(r => r.BookId == loan.BookId &&
                          (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasPendingReservations)
            throw new InvalidOperationException("Cannot renew because there are pending reservations for this book");

        // Check unpaid fines
        var unpaidFines = await _db.Fines
            .Where(f => f.PatronId == loan.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0m;

        if (unpaidFines >= FineThreshold)
            throw new InvalidOperationException($"Patron has ${unpaidFines:F2} in unpaid fines. Please pay fines before renewing.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Renewed: Loan {LoanId}, new due date {DueDate}, renewal #{Count}", id, loan.DueDate, loan.RenewalCount);

        return MapToDto(loan);
    }

    public async Task<List<LoanDto>> GetOverdueAsync()
    {
        return await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null)
            .OrderBy(l => l.DueDate)
            .Select(l => MapToDto(l))
            .ToListAsync();
    }

    public static LoanDto MapToDto(Loan l) => new()
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
}
