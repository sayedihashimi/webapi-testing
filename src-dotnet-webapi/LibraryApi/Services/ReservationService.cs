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
        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<ReservationResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<ReservationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Reservations.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
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

        // Cannot reserve a book you have on active loan
        var hasActiveLoan = await db.Loans.AsNoTracking()
            .AnyAsync(l => l.BookId == book.Id && l.PatronId == patron.Id &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);
        if (hasActiveLoan)
            throw new InvalidOperationException("Cannot reserve a book you currently have on loan.");

        // Cannot have duplicate active reservation
        var hasActiveReservation = await db.Reservations.AsNoTracking()
            .AnyAsync(r => r.BookId == book.Id && r.PatronId == patron.Id &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);
        if (hasActiveReservation)
            throw new InvalidOperationException("You already have an active reservation for this book.");

        // Calculate queue position
        var maxPosition = await db.Reservations.AsNoTracking()
            .Where(r => r.BookId == book.Id &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition, ct) ?? 0;

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = book.Id,
            PatronId = patron.Id,
            ReservationDate = now,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1,
            CreatedAt = now
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation created: {ReservationId} for Book {BookId} by Patron {PatronId}", reservation.Id, book.Id, patron.Id);

        return new ReservationResponse(
            reservation.Id, reservation.BookId, book.Title, reservation.PatronId,
            $"{patron.FirstName} {patron.LastName}",
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

        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Ready))
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        // If Ready, release the held copy
        if (reservation.Status == ReservationStatus.Ready)
        {
            reservation.Book.AvailableCopies++;
            reservation.Book.UpdatedAt = DateTime.UtcNow;
        }

        reservation.Status = ReservationStatus.Cancelled;

        // Re-number queue positions for remaining reservations
        var remaining = await db.Reservations
            .Where(r => r.BookId == reservation.BookId && r.Id != id &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync(ct);

        for (var i = 0; i < remaining.Count; i++)
            remaining[i].QueuePosition = i + 1;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Reservation {ReservationId} cancelled", id);

        return new ReservationResponse(
            reservation.Id, reservation.BookId, reservation.Book.Title, reservation.PatronId,
            $"{reservation.Patron.FirstName} {reservation.Patron.LastName}",
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status,
            reservation.QueuePosition, reservation.CreatedAt);
    }

    public async Task<LoanResponse> FulfillAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException("Only reservations with 'Ready' status can be fulfilled.");

        // Check patron is still active
        if (!reservation.Patron.IsActive)
            throw new InvalidOperationException("Patron account is not active.");

        var now = DateTime.UtcNow;
        var loanDays = reservation.Patron.MembershipType switch
        {
            MembershipType.Standard => 14,
            MembershipType.Premium => 21,
            MembershipType.Student => 7,
            _ => 14
        };

        var loan = new Loan
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active,
            RenewalCount = 0,
            CreatedAt = now
        };

        reservation.Status = ReservationStatus.Fulfilled;

        // The copy was already held when reservation became Ready, so no change to AvailableCopies

        // Re-number queue positions
        var remaining = await db.Reservations
            .Where(r => r.BookId == reservation.BookId && r.Id != id &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync(ct);

        for (var i = 0; i < remaining.Count; i++)
            remaining[i].QueuePosition = i + 1;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} fulfilled, created Loan {LoanId}", id, loan.Id);

        return new LoanResponse(
            loan.Id, loan.BookId, reservation.Book.Title, loan.PatronId,
            $"{reservation.Patron.FirstName} {reservation.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);
    }
}
