using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class ReservationService(LibraryDbContext db, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PaginatedResponse<ReservationResponse>> GetAllAsync(
        ReservationStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations.AsNoTracking().AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status,
                r.QueuePosition, r.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<ReservationResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<ReservationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Reservations.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status,
                r.QueuePosition, r.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron account is not active.");

        // Cannot reserve a book the patron already has on active loan
        var hasActiveLoan = await db.Loans.AnyAsync(
            l => l.BookId == request.BookId && l.PatronId == request.PatronId &&
                 (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);
        if (hasActiveLoan)
            throw new InvalidOperationException("Patron already has this book on an active loan.");

        // Check if patron already has a pending/ready reservation for this book
        var hasExistingReservation = await db.Reservations.AnyAsync(
            r => r.BookId == request.BookId && r.PatronId == request.PatronId &&
                 (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);
        if (hasExistingReservation)
            throw new InvalidOperationException("Patron already has an active reservation for this book.");

        // Determine queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == request.BookId &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition, ct) ?? 0;

        var reservation = new Reservation
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created reservation {ReservationId}: Patron {PatronId} for Book {BookId}, position {Position}",
            reservation.Id, patron.Id, book.Id, reservation.QueuePosition);

        return new ReservationResponse(
            reservation.Id, reservation.BookId, book.Title, reservation.PatronId,
            patron.FirstName + " " + patron.LastName,
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status,
            reservation.QueuePosition, reservation.CreatedAt);
    }

    public async Task<ReservationResponse> CancelAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        reservation.Status = ReservationStatus.Cancelled;

        // Reorder queue positions
        var laterReservations = await db.Reservations
            .Where(r => r.BookId == reservation.BookId &&
                        r.QueuePosition > reservation.QueuePosition &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync(ct);

        foreach (var r in laterReservations)
            r.QueuePosition--;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled reservation {ReservationId}", id);

        return new ReservationResponse(
            reservation.Id, reservation.BookId, reservation.Book.Title, reservation.PatronId,
            reservation.Patron.FirstName + " " + reservation.Patron.LastName,
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status,
            reservation.QueuePosition, reservation.CreatedAt);
    }

    public async Task<ReservationResponse> FulfillAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException("Only reservations with 'Ready' status can be fulfilled.");

        reservation.Status = ReservationStatus.Fulfilled;

        // Reorder remaining queue positions
        var laterReservations = await db.Reservations
            .Where(r => r.BookId == reservation.BookId &&
                        r.QueuePosition > reservation.QueuePosition &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync(ct);

        foreach (var r in laterReservations)
            r.QueuePosition--;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Fulfilled reservation {ReservationId}", id);

        return new ReservationResponse(
            reservation.Id, reservation.BookId, reservation.Book.Title, reservation.PatronId,
            reservation.Patron.FirstName + " " + reservation.Patron.LastName,
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status,
            reservation.QueuePosition, reservation.CreatedAt);
    }
}
