using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanService : ILoanService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<LoanService> _logger;

    public LoanService(LibraryDbContext context, ILogger<LoanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static int GetLoanDays(MembershipType type) => type switch
    {
        MembershipType.Premium => 21,
        MembershipType.Student => 7,
        _ => 14
    };

    private static int GetMaxLoans(MembershipType type) => type switch
    {
        MembershipType.Premium => 10,
        MembershipType.Student => 3,
        _ => 5
    };

    public async Task<PaginatedResponse<LoanDto>> GetAllAsync(string? status, bool? overdue, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
            query = query.Where(l => l.Status == loanStatus);

        if (overdue == true)
            query = query.Where(l => l.Status == LoanStatus.Overdue || (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && l.ReturnDate == null));

        if (fromDate.HasValue)
            query = query.Where(l => l.LoanDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(l => l.LoanDate <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<LoanDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LoanDto?> GetByIdAsync(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id);

        return loan == null ? null : MapToDto(loan);
    }

    public async Task<(LoanDto? Loan, string? Error)> CheckoutAsync(CreateLoanDto dto)
    {
        var book = await _context.Books.FindAsync(dto.BookId);
        if (book == null) return (null, "Book not found.");

        var patron = await _context.Patrons
            .Include(p => p.Loans)
            .Include(p => p.Fines)
            .FirstOrDefaultAsync(p => p.Id == dto.PatronId);

        if (patron == null) return (null, "Patron not found.");

        // Check patron is active
        if (!patron.IsActive)
            return (null, "Patron membership is not active.");

        // Check available copies
        if (book.AvailableCopies <= 0)
            return (null, "No available copies of this book.");

        // Check unpaid fines
        var unpaidFines = patron.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount);
        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines (threshold: $10.00). Please pay outstanding fines before checking out.");

        // Check borrowing limit
        var activeLoans = patron.Loans.Count(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue);
        var maxLoans = GetMaxLoans(patron.MembershipType);
        if (activeLoans >= maxLoans)
            return (null, $"Patron has reached the maximum borrowing limit ({maxLoans}) for {patron.MembershipType} membership.");

        var loanDays = GetLoanDays(patron.MembershipType);
        var loan = new Loan
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(loanDays),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        book.AvailableCopies--;
        book.UpdatedAt = DateTime.UtcNow;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Book {BookId} checked out to patron {PatronId}. Loan {LoanId} created. Due: {DueDate}",
            dto.BookId, dto.PatronId, loan.Id, loan.DueDate);

        // Reload with includes
        return (await GetByIdAsync(loan.Id), null);
    }

    public async Task<(LoanDto? Loan, string? Error)> ReturnAsync(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null) return (null, "Loan not found.");

        if (loan.Status == LoanStatus.Returned)
            return (null, "This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnDate = now;
        loan.Status = LoanStatus.Returned;

        // Increment available copies
        loan.Book.AvailableCopies++;
        loan.Book.UpdatedAt = now;

        // Check for overdue and generate fine
        if (now > loan.DueDate)
        {
            var daysOverdue = (int)Math.Ceiling((now - loan.DueDate).TotalDays);
            var fineAmount = daysOverdue * 0.25m;

            var fine = new Fine
            {
                PatronId = loan.PatronId,
                LoanId = loan.Id,
                Amount = fineAmount,
                Reason = $"Overdue return — {daysOverdue} days late",
                IssuedDate = now,
                Status = FineStatus.Unpaid,
                CreatedAt = now
            };
            _context.Fines.Add(fine);

            _logger.LogInformation("Fine of ${Amount:F2} issued to patron {PatronId} for overdue loan {LoanId} ({DaysOverdue} days late)",
                fineAmount, loan.PatronId, loan.Id, daysOverdue);
        }

        await _context.SaveChangesAsync();

        // Check for pending reservations for this book
        var nextReservation = await _context.Reservations
            .Where(r => r.BookId == loan.BookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (nextReservation != null)
        {
            nextReservation.Status = ReservationStatus.Ready;
            nextReservation.ExpirationDate = now.AddDays(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} for book {BookId} moved to Ready status. Patron {PatronId} has 3 days to pick up.",
                nextReservation.Id, loan.BookId, nextReservation.PatronId);
        }

        _logger.LogInformation("Book {BookId} returned by patron {PatronId}. Loan {LoanId} completed.",
            loan.BookId, loan.PatronId, loan.Id);

        return (await GetByIdAsync(loan.Id), null);
    }

    public async Task<(LoanDto? Loan, string? Error)> RenewAsync(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron).ThenInclude(p => p.Fines)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null) return (null, "Loan not found.");

        if (loan.Status == LoanStatus.Returned)
            return (null, "Cannot renew a returned loan.");

        if (loan.Status == LoanStatus.Overdue || (loan.DueDate < DateTime.UtcNow && loan.ReturnDate == null))
            return (null, "Cannot renew an overdue loan. Please return the book first.");

        if (loan.RenewalCount >= 2)
            return (null, "Maximum renewal limit (2) reached for this loan.");

        // Check for pending reservations
        var hasPendingReservations = await _context.Reservations
            .AnyAsync(r => r.BookId == loan.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasPendingReservations)
            return (null, "Cannot renew: there are pending reservations for this book.");

        // Check unpaid fines threshold
        var unpaidFines = loan.Patron.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount);
        if (unpaidFines >= 10.00m)
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines (threshold: $10.00). Please pay outstanding fines before renewing.");

        var loanDays = GetLoanDays(loan.Patron.MembershipType);
        loan.DueDate = DateTime.UtcNow.AddDays(loanDays);
        loan.RenewalCount++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Loan {LoanId} renewed (renewal #{RenewalCount}). New due date: {DueDate}",
            loan.Id, loan.RenewalCount, loan.DueDate);

        return (await GetByIdAsync(loan.Id), null);
    }

    public async Task<PaginatedResponse<LoanDto>> GetOverdueAsync(int page, int pageSize)
    {
        var now = DateTime.UtcNow;

        // Also update status of loans that are past due
        var overdueLoans = await _context.Loans
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < now && l.ReturnDate == null)
            .ToListAsync();

        foreach (var loan in overdueLoans)
            loan.Status = LoanStatus.Overdue;

        if (overdueLoans.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Flagged {Count} loans as overdue", overdueLoans.Count);
        }

        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.Status == LoanStatus.Overdue)
            .OrderBy(l => l.DueDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<LoanDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public static LoanDto MapToDto(Loan l) => new()
    {
        Id = l.Id,
        BookId = l.BookId,
        BookTitle = l.Book?.Title ?? string.Empty,
        BookISBN = l.Book?.ISBN ?? string.Empty,
        PatronId = l.PatronId,
        PatronName = l.Patron != null ? $"{l.Patron.FirstName} {l.Patron.LastName}" : string.Empty,
        LoanDate = l.LoanDate,
        DueDate = l.DueDate,
        ReturnDate = l.ReturnDate,
        Status = l.Status,
        RenewalCount = l.RenewalCount,
        CreatedAt = l.CreatedAt
    };
}
