using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface IReservationService
{
    Task<PaginatedResponse<ReservationResponse>> GetAllAsync(ReservationStatus? status, int page, int pageSize, CancellationToken ct);
    Task<ReservationResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct);
    Task<ReservationResponse> CancelAsync(int id, CancellationToken ct);
    Task<LoanResponse> FulfillAsync(int id, CancellationToken ct);
}

public class ReservationService(LibraryDbContext db, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PaginatedResponse<ReservationResponse>> GetAllAsync(
        ReservationStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => MapToResponse(r))
            .ToListAsync(ct);

        return PaginatedResponse<ReservationResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<ReservationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.Id == id)
            .Select(r => MapToResponse(r))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct)
    {
        var book = await db.Books.FindAsync([request.BookId], ct)
            ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");

        var patron = await db.Patrons.FindAsync([request.PatronId], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {request.PatronId} not found.");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron account is inactive.");

        // Cannot reserve a book the patron already has on active loan
        var hasActiveLoan = await db.Loans.AnyAsync(
            l => l.BookId == request.BookId && l.PatronId == request.PatronId && l.Status != LoanStatus.Returned, ct);

        if (hasActiveLoan)
            throw new InvalidOperationException("Cannot reserve a book that you already have on an active loan.");

        // Check for existing active reservation by same patron for same book
        var hasActiveReservation = await db.Reservations.AnyAsync(
            r => r.BookId == request.BookId && r.PatronId == request.PatronId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasActiveReservation)
            throw new InvalidOperationException("Patron already has an active reservation for this book.");

        // Determine queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == request.BookId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
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
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{reservation.Status}'.");

        reservation.Status = ReservationStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} cancelled", reservation.Id);
        return MapToResponse(reservation);
    }

    public async Task<LoanResponse> FulfillAsync(int id, CancellationToken ct)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException("Only reservations with status 'Ready' can be fulfilled.");

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate < DateTime.UtcNow)
            throw new InvalidOperationException("This reservation has expired.");

        var patron = reservation.Patron;
        var book = reservation.Book;

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron account is inactive.");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException("No available copies of this book.");

        var now = DateTime.UtcNow;
        var loanDays = patron.MembershipType switch
        {
            MembershipType.Standard => 14,
            MembershipType.Premium => 21,
            MembershipType.Student => 7,
            _ => 14
        };

        var loan = new Loan
        {
            BookId = book.Id,
            PatronId = patron.Id,
            LoanDate = now,
            DueDate = now.AddDays(loanDays),
            Status = LoanStatus.Active,
            CreatedAt = now
        };

        reservation.Status = ReservationStatus.Fulfilled;
        book.AvailableCopies--;

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} fulfilled, created Loan {LoanId}", reservation.Id, loan.Id);

        return (await db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.Id == loan.Id)
            .Select(l => LoanService.MapToResponse(l))
            .FirstAsync(ct));
    }

    internal static ReservationResponse MapToResponse(Reservation r) => new()
    {
        Id = r.Id,
        BookId = r.BookId,
        BookTitle = r.Book.Title,
        PatronId = r.PatronId,
        PatronName = $"{r.Patron.FirstName} {r.Patron.LastName}",
        ReservationDate = r.ReservationDate,
        ExpirationDate = r.ExpirationDate,
        Status = r.Status,
        QueuePosition = r.QueuePosition,
        CreatedAt = r.CreatedAt
    };
}
