using LibraryApi.Data;
using LibraryApi.DTOs;
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

    public async Task<PagedResult<ReservationDto>> GetReservationsAsync(string? status, PaginationParams pagination)
    {
        var query = _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var rs))
        {
            query = query.Where(r => r.Status == rs);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(r => new ReservationDto
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = r.Book.Title,
                PatronId = r.PatronId,
                PatronName = r.Patron.FirstName + " " + r.Patron.LastName,
                ReservationDate = r.ReservationDate,
                ExpirationDate = r.ExpirationDate,
                Status = r.Status,
                QueuePosition = r.QueuePosition,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<ReservationDto> GetReservationByIdAsync(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found");

        return MapToDto(reservation);
    }

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new NotFoundException($"Book with ID {dto.BookId} not found");

        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new NotFoundException($"Patron with ID {dto.PatronId} not found");

        if (!patron.IsActive)
            throw new BusinessRuleException("Patron's membership is inactive. Cannot make reservations.");

        // Check if patron already has an active loan for this book
        var hasActiveLoan = await _db.Loans.AnyAsync(l =>
            l.BookId == dto.BookId &&
            l.PatronId == dto.PatronId &&
            (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));

        if (hasActiveLoan)
            throw new BusinessRuleException("Patron already has an active loan for this book. Cannot reserve a book you already have checked out.");

        // Check if patron already has a pending/ready reservation for this book
        var hasActiveReservation = await _db.Reservations.AnyAsync(r =>
            r.BookId == dto.BookId &&
            r.PatronId == dto.PatronId &&
            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasActiveReservation)
            throw new BusinessRuleException("Patron already has an active reservation for this book.");

        // Determine queue position
        var maxQueuePosition = await _db.Reservations
            .Where(r => r.BookId == dto.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var now = DateTime.UtcNow;

        var reservation = new Reservation
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            ReservationDate = now,
            Status = ReservationStatus.Pending,
            QueuePosition = maxQueuePosition + 1,
            CreatedAt = now
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation created: ReservationId={Id}, BookId={BookId}, PatronId={PatronId}, QueuePosition={QueuePos}",
            reservation.Id, reservation.BookId, reservation.PatronId, reservation.QueuePosition);

        await _db.Entry(reservation).Reference(r => r.Book).LoadAsync();
        await _db.Entry(reservation).Reference(r => r.Patron).LoadAsync();

        return MapToDto(reservation);
    }

    public async Task<ReservationDto> CancelReservationAsync(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found");

        if (reservation.Status == ReservationStatus.Fulfilled || reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Expired)
            throw new BusinessRuleException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        reservation.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation cancelled: ReservationId={Id}", reservation.Id);

        // If this was a Ready reservation, move the next Pending to Ready
        if (reservation.Status == ReservationStatus.Cancelled)
        {
            var nextReservation = await _db.Reservations
                .Where(r => r.BookId == reservation.BookId && r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync();

            if (nextReservation != null && reservation.Book.AvailableCopies > 0)
            {
                nextReservation.Status = ReservationStatus.Ready;
                nextReservation.ExpirationDate = DateTime.UtcNow.AddDays(3);
                await _db.SaveChangesAsync();
            }
        }

        return MapToDto(reservation);
    }

    public async Task<LoanDto> FulfillReservationAsync(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found");

        if (reservation.Status != ReservationStatus.Ready)
            throw new BusinessRuleException($"Only reservations with 'Ready' status can be fulfilled. Current status: '{reservation.Status}'.");

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
        {
            reservation.Status = ReservationStatus.Expired;
            await _db.SaveChangesAsync();
            throw new BusinessRuleException("This reservation has expired.");
        }

        // Create a loan via the loan service
        var loanDto = await _loanService.CheckoutBookAsync(new CreateLoanDto
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId
        });

        reservation.Status = ReservationStatus.Fulfilled;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation fulfilled: ReservationId={Id}, LoanId={LoanId}", reservation.Id, loanDto.Id);

        return loanDto;
    }

    private static ReservationDto MapToDto(Reservation reservation) => new()
    {
        Id = reservation.Id,
        BookId = reservation.BookId,
        BookTitle = reservation.Book.Title,
        PatronId = reservation.PatronId,
        PatronName = reservation.Patron.FirstName + " " + reservation.Patron.LastName,
        ReservationDate = reservation.ReservationDate,
        ExpirationDate = reservation.ExpirationDate,
        Status = reservation.Status,
        QueuePosition = reservation.QueuePosition,
        CreatedAt = reservation.CreatedAt
    };
}
