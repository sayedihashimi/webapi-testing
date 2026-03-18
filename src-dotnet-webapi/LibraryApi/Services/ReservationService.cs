using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class ReservationService(LibraryDbContext db, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PagedResponse<ReservationResponse>> GetAllAsync(ReservationStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .AsQueryable();

        if (status.HasValue) query = query.Where(r => r.Status == status.Value);

        query = query.OrderByDescending(r => r.ReservationDate);
        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => MapToResponse(r)).ToListAsync(ct);

        return new PagedResponse<ReservationResponse>
        {
            Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            HasNextPage = page * pageSize < totalCount, HasPreviousPage = page > 1
        };
    }

    public async Task<ReservationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var r = await db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return r is null ? null : MapToResponse(r);
    }

    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new ArgumentException("Patron membership is not active.");

        // Cannot reserve book patron already has on active loan
        var hasActiveLoan = await db.Loans.AnyAsync(
            l => l.BookId == request.BookId && l.PatronId == request.PatronId && l.Status == LoanStatus.Active, ct);
        if (hasActiveLoan)
            throw new ArgumentException("Cannot reserve a book that is already on active loan to this patron.");

        // Check for existing active reservation by same patron for same book
        var hasActiveReservation = await db.Reservations.AnyAsync(
            r => r.BookId == request.BookId && r.PatronId == request.PatronId &&
                 (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);
        if (hasActiveReservation)
            throw new ArgumentException("Patron already has an active reservation for this book.");

        // Calculate queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == request.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition, ct) ?? 0;

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            ReservationDate = now,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1,
            CreatedAt = now
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} created for Book {BookId} by Patron {PatronId}", reservation.Id, book.Id, patron.Id);

        return (await GetByIdAsync(reservation.Id, ct))!;
    }

    public async Task<ReservationResponse> CancelAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
            throw new ArgumentException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        if (reservation.Status == ReservationStatus.Ready)
        {
            // Release the reserved copy
            reservation.Book.AvailableCopies++;

            // Promote next pending reservation
            var next = await db.Reservations
                .Where(r => r.BookId == reservation.BookId && r.Status == ReservationStatus.Pending && r.Id != id)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync(ct);

            if (next is not null)
            {
                next.Status = ReservationStatus.Ready;
                next.ExpirationDate = DateTime.UtcNow.AddDays(3);
                reservation.Book.AvailableCopies--;
            }
        }

        reservation.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(reservation.Id, ct))!;
    }

    public async Task<ReservationResponse> FulfillAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new ArgumentException("Only reservations with 'Ready' status can be fulfilled.");

        reservation.Status = ReservationStatus.Fulfilled;
        // The book copy was already held when status moved to Ready
        // Now create a loan for it
        var loanDays = reservation.Patron.MembershipType switch
        {
            MembershipType.Standard => 14,
            MembershipType.Premium => 21,
            MembershipType.Student => 7,
            _ => 14
        };

        var now = DateTime.UtcNow;
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

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} fulfilled, Loan {LoanId} created", reservation.Id, loan.Id);

        return (await GetByIdAsync(reservation.Id, ct))!;
    }

    private static ReservationResponse MapToResponse(Reservation r) => new()
    {
        Id = r.Id, BookId = r.BookId, BookTitle = r.Book.Title,
        PatronId = r.PatronId, PatronName = r.Patron.FirstName + " " + r.Patron.LastName,
        ReservationDate = r.ReservationDate, ExpirationDate = r.ExpirationDate,
        Status = r.Status, QueuePosition = r.QueuePosition, CreatedAt = r.CreatedAt
    };
}
