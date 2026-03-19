using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Middleware;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class ReservationService : IReservationService
{
    private readonly LibraryDbContext _db;
    private readonly ILoanService _loanService;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(LibraryDbContext db, ILoanService loanService, ILogger<ReservationService> logger)
    {
        _db = db;
        _loanService = loanService;
        _logger = logger;
    }

    public async Task<PagedResult<ReservationDto>> GetReservationsAsync(string? status, int page, int pageSize)
    {
        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var rs))
            query = query.Where(r => r.Status == rs);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(r => r.ReservationDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<ReservationDto> GetReservationByIdAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found.");
        return MapToDto(reservation);
    }

    public async Task<ReservationDto> CreateReservationAsync(ReservationCreateDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new NotFoundException($"Book with ID {dto.BookId} not found.");
        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new NotFoundException($"Patron with ID {dto.PatronId} not found.");

        // Cannot reserve a book they already have on active loan
        var hasActiveLoan = await _db.Loans.AnyAsync(l => l.BookId == dto.BookId && l.PatronId == dto.PatronId && l.Status == LoanStatus.Active);
        if (hasActiveLoan)
            throw new BusinessRuleException("Patron already has this book on an active loan and cannot reserve it.");

        // Cannot have a duplicate pending/ready reservation
        var hasActiveReservation = await _db.Reservations.AnyAsync(r =>
            r.BookId == dto.BookId && r.PatronId == dto.PatronId &&
            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasActiveReservation)
            throw new BusinessRuleException("Patron already has an active reservation for this book.");

        // Determine queue position
        var maxPosition = await _db.Reservations
            .Where(r => r.BookId == dto.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            ReservationDate = now,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1,
            CreatedAt = now
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation created: ReservationId={ReservationId}, BookId={BookId}, PatronId={PatronId}, QueuePosition={Position}",
            reservation.Id, dto.BookId, dto.PatronId, reservation.QueuePosition);

        return await ReloadReservationDto(reservation.Id);
    }

    public async Task<ReservationDto> CancelReservationAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Ready)
            throw new BusinessRuleException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        reservation.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation cancelled: ReservationId={ReservationId}", id);
        return MapToDto(reservation);
    }

    public async Task<LoanDto> FulfillReservationAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new BusinessRuleException($"Only reservations with 'Ready' status can be fulfilled. Current status: '{reservation.Status}'.");

        // Create a loan via the loan service
        var loanDto = await _loanService.CheckoutBookAsync(new LoanCreateDto
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId
        });

        reservation.Status = ReservationStatus.Fulfilled;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation fulfilled: ReservationId={ReservationId}, LoanId={LoanId}", id, loanDto.Id);
        return loanDto;
    }

    private async Task<ReservationDto> ReloadReservationDto(int id)
    {
        var r = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron).FirstAsync(r => r.Id == id);
        return MapToDto(r);
    }

    internal static ReservationDto MapToDto(Reservation r) => new()
    {
        Id = r.Id,
        BookId = r.BookId,
        BookTitle = r.Book.Title,
        PatronId = r.PatronId,
        PatronName = $"{r.Patron.FirstName} {r.Patron.LastName}",
        ReservationDate = r.ReservationDate,
        ExpirationDate = r.ExpirationDate,
        Status = r.Status.ToString(),
        QueuePosition = r.QueuePosition,
        CreatedAt = r.CreatedAt
    };
}
