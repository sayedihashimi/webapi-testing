using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class ReservationService : IReservationService
{
    private readonly LibraryDbContext _context;
    private readonly ILoanService _loanService;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(LibraryDbContext context, ILoanService loanService, ILogger<ReservationService> logger)
    {
        _context = context;
        _loanService = loanService;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ReservationDto>> GetAllAsync(string? status, int page, int pageSize)
    {
        var query = _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var resStatus))
            query = query.Where(r => r.Status == resStatus);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<ReservationDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReservationDto?> GetByIdAsync(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id);

        return reservation == null ? null : MapToDto(reservation);
    }

    public async Task<(ReservationDto? Reservation, string? Error)> CreateAsync(CreateReservationDto dto)
    {
        var book = await _context.Books.FindAsync(dto.BookId);
        if (book == null) return (null, "Book not found.");

        var patron = await _context.Patrons.FindAsync(dto.PatronId);
        if (patron == null) return (null, "Patron not found.");

        if (!patron.IsActive)
            return (null, "Patron membership is not active.");

        // Check if patron already has an active loan for this book
        var hasActiveLoan = await _context.Loans
            .AnyAsync(l => l.BookId == dto.BookId && l.PatronId == dto.PatronId &&
                           (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));

        if (hasActiveLoan)
            return (null, "Patron already has an active loan for this book. Cannot create a reservation.");

        // Check if patron already has a pending/ready reservation for this book
        var hasExistingReservation = await _context.Reservations
            .AnyAsync(r => r.BookId == dto.BookId && r.PatronId == dto.PatronId &&
                           (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasExistingReservation)
            return (null, "Patron already has an active reservation for this book.");

        // Get the next queue position
        var maxPosition = await _context.Reservations
            .Where(r => r.BookId == dto.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var reservation = new Reservation
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            ReservationDate = DateTime.UtcNow,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Reservation {ReservationId} created for book {BookId} by patron {PatronId}. Queue position: {Position}",
            reservation.Id, dto.BookId, dto.PatronId, reservation.QueuePosition);

        return (await GetByIdAsync(reservation.Id), null);
    }

    public async Task<(ReservationDto? Reservation, string? Error)> CancelAsync(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null) return (null, "Reservation not found.");

        if (reservation.Status == ReservationStatus.Fulfilled || reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Expired)
            return (null, $"Cannot cancel a reservation with status '{reservation.Status}'.");

        reservation.Status = ReservationStatus.Cancelled;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Reservation {ReservationId} cancelled", id);

        // If this was a Ready reservation, move the next pending to Ready
        await PromoteNextReservationAsync(reservation.BookId);

        return (MapToDto(reservation), null);
    }

    public async Task<(LoanDto? Loan, string? Error)> FulfillAsync(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null) return (null, "Reservation not found.");

        if (reservation.Status != ReservationStatus.Ready)
            return (null, $"Only 'Ready' reservations can be fulfilled. Current status: '{reservation.Status}'.");

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
        {
            reservation.Status = ReservationStatus.Expired;
            await _context.SaveChangesAsync();
            await PromoteNextReservationAsync(reservation.BookId);
            return (null, "Reservation has expired. The next reservation in the queue has been promoted.");
        }

        // Create a loan for the patron
        var (loan, error) = await _loanService.CheckoutAsync(new CreateLoanDto
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId
        });

        if (loan == null)
            return (null, $"Cannot fulfill reservation: {error}");

        reservation.Status = ReservationStatus.Fulfilled;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Reservation {ReservationId} fulfilled. Loan {LoanId} created.", id, loan.Id);

        return (loan, null);
    }

    private async Task PromoteNextReservationAsync(int bookId)
    {
        var next = await _context.Reservations
            .Where(r => r.BookId == bookId && r.Status == ReservationStatus.Pending)
            .OrderBy(r => r.QueuePosition)
            .FirstOrDefaultAsync();

        if (next != null)
        {
            next.Status = ReservationStatus.Ready;
            next.ExpirationDate = DateTime.UtcNow.AddDays(3);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reservation {ReservationId} promoted to Ready for book {BookId}", next.Id, bookId);
        }
    }

    public static ReservationDto MapToDto(Reservation r) => new()
    {
        Id = r.Id,
        BookId = r.BookId,
        BookTitle = r.Book?.Title ?? string.Empty,
        PatronId = r.PatronId,
        PatronName = r.Patron != null ? $"{r.Patron.FirstName} {r.Patron.LastName}" : string.Empty,
        ReservationDate = r.ReservationDate,
        ExpirationDate = r.ExpirationDate,
        Status = r.Status,
        QueuePosition = r.QueuePosition,
        CreatedAt = r.CreatedAt
    };
}
