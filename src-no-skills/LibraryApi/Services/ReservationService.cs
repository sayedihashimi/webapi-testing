using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Reservation;
using LibraryApi.Models;
using LibraryApi.Models.Enums;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class ReservationService : IReservationService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(LibraryDbContext db, ILogger<ReservationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<ReservationListDto>> GetAllAsync(ReservationStatus? status, PaginationParams pagination)
    {
        var query = _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(r => new ReservationListDto(
                r.Id, r.Book.Title, $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync();

        return new PagedResult<ReservationListDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<ReservationDetailDto> GetByIdAsync(int id)
    {
        var r = await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        return MapToDetail(r);
    }

    public async Task<ReservationDetailDto> CreateAsync(CreateReservationDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {dto.BookId} not found.");

        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new KeyNotFoundException($"Patron with ID {dto.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron account is inactive.");

        // Cannot reserve a book already on active loan by same patron
        var hasActiveLoan = await _db.Loans.AnyAsync(l =>
            l.BookId == dto.BookId && l.PatronId == dto.PatronId && l.Status == LoanStatus.Active);
        if (hasActiveLoan)
            throw new InvalidOperationException("Cannot reserve a book that you already have on active loan.");

        // Check if patron already has a pending/ready reservation for this book
        var hasExistingReservation = await _db.Reservations.AnyAsync(r =>
            r.BookId == dto.BookId && r.PatronId == dto.PatronId &&
            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasExistingReservation)
            throw new InvalidOperationException("Patron already has an active reservation for this book.");

        // Determine queue position
        var maxPosition = await _db.Reservations
            .Where(r => r.BookId == dto.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var reservation = new Reservation
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            QueuePosition = maxPosition + 1
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created reservation {ReservationId} for Book {BookId} by Patron {PatronId}, position {Position}",
            reservation.Id, dto.BookId, dto.PatronId, reservation.QueuePosition);

        return await GetByIdAsync(reservation.Id);
    }

    public async Task CancelAsync(int id)
    {
        var reservation = await _db.Reservations.FindAsync(id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        reservation.Status = ReservationStatus.Cancelled;

        // Reorder queue positions
        var subsequentReservations = await _db.Reservations
            .Where(r => r.BookId == reservation.BookId &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready) &&
                        r.QueuePosition > reservation.QueuePosition)
            .OrderBy(r => r.QueuePosition)
            .ToListAsync();

        foreach (var r in subsequentReservations)
        {
            r.QueuePosition--;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled reservation {ReservationId}", id);
    }

    public async Task<ReservationDetailDto> FulfillAsync(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException("Only reservations with 'Ready' status can be fulfilled.");

        reservation.Status = ReservationStatus.Fulfilled;

        // Reorder remaining queue
        var subsequentReservations = await _db.Reservations
            .Where(r => r.BookId == reservation.BookId &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready) &&
                        r.QueuePosition > reservation.QueuePosition)
            .OrderBy(r => r.QueuePosition)
            .ToListAsync();

        foreach (var r in subsequentReservations)
        {
            r.QueuePosition--;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Fulfilled reservation {ReservationId}", id);

        return MapToDetail(reservation);
    }

    private static ReservationDetailDto MapToDetail(Reservation r)
    {
        return new ReservationDetailDto(
            r.Id, r.BookId, r.Book.Title,
            r.PatronId, $"{r.Patron.FirstName} {r.Patron.LastName}", r.Patron.Email,
            r.ReservationDate, r.ExpirationDate,
            r.Status, r.QueuePosition, r.CreatedAt);
    }
}
