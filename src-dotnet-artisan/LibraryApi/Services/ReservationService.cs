using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class ReservationService(LibraryDbContext db, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PagedResult<ReservationDto>> GetReservationsAsync(
        string? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .AsQueryable();

        if (Enum.TryParse<ReservationStatus>(status, true, out var reservationStatus))
        {
            query = query.Where(r => r.Status == reservationStatus);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationDto(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<ReservationDto>(items, totalCount, page, pageSize);
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.Id == id)
            .Select(r => new ReservationDto(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(ReservationDto? Reservation, string? Error)> CreateReservationAsync(
        CreateReservationDto dto, CancellationToken ct = default)
    {
        var book = await db.Books.FindAsync([dto.BookId], ct);
        if (book is null)
        {
            return (null, "Book not found.");
        }

        var patron = await db.Patrons.FindAsync([dto.PatronId], ct);
        if (patron is null)
        {
            return (null, "Patron not found.");
        }

        if (!patron.IsActive)
        {
            return (null, "Patron account is inactive.");
        }

        // Can't reserve a book patron currently has on active loan
        var hasActiveLoan = await db.Loans
            .AnyAsync(l => l.BookId == dto.BookId && l.PatronId == dto.PatronId &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        if (hasActiveLoan)
        {
            return (null, "Cannot reserve a book you currently have on loan.");
        }

        // Check for existing active reservation for this patron+book
        var hasActiveReservation = await db.Reservations
            .AnyAsync(r => r.BookId == dto.BookId && r.PatronId == dto.PatronId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasActiveReservation)
        {
            return (null, "You already have an active reservation for this book.");
        }

        // Determine queue position
        var maxPosition = await db.Reservations
            .Where(r => r.BookId == dto.BookId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition, ct) ?? 0;

        var reservation = new Reservation
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1
        };

        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation created: {ReservationId} for Book {BookId} by Patron {PatronId}",
            reservation.Id, dto.BookId, dto.PatronId);

        var result = new ReservationDto(
            reservation.Id, reservation.BookId, book.Title, reservation.PatronId,
            $"{patron.FirstName} {patron.LastName}",
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status,
            reservation.QueuePosition, reservation.CreatedAt);

        return (result, null);
    }

    public async Task<(ReservationDto? Reservation, string? Error)> CancelReservationAsync(
        int id, CancellationToken ct = default)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reservation is null)
        {
            return (null, "Reservation not found.");
        }

        if (reservation.Status is not (ReservationStatus.Pending or ReservationStatus.Ready))
        {
            return (null, "Only pending or ready reservations can be cancelled.");
        }

        reservation.Status = ReservationStatus.Cancelled;

        // Reorder queue positions
        var subsequentReservations = await db.Reservations
            .Where(r => r.BookId == reservation.BookId &&
                r.QueuePosition > reservation.QueuePosition &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync(ct);

        foreach (var r in subsequentReservations)
        {
            r.QueuePosition--;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} cancelled", id);

        var result = new ReservationDto(
            reservation.Id, reservation.BookId, reservation.Book.Title,
            reservation.PatronId, $"{reservation.Patron.FirstName} {reservation.Patron.LastName}",
            reservation.ReservationDate, reservation.ExpirationDate, reservation.Status,
            reservation.QueuePosition, reservation.CreatedAt);

        return (result, null);
    }

    public async Task<(LoanDto? Loan, string? Error)> FulfillReservationAsync(int id, CancellationToken ct = default)
    {
        var reservation = await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reservation is null)
        {
            return (null, "Reservation not found.");
        }

        if (reservation.Status != ReservationStatus.Ready)
        {
            return (null, "Only ready reservations can be fulfilled.");
        }

        if (reservation.Book.AvailableCopies <= 0)
        {
            return (null, "No available copies to fulfill reservation.");
        }

        // Check patron eligibility
        if (!reservation.Patron.IsActive)
        {
            return (null, "Patron account is inactive.");
        }

        var unpaidFines = await db.Fines
            .Where(f => f.PatronId == reservation.PatronId && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        if (unpaidFines >= 10m)
        {
            return (null, $"Patron has ${unpaidFines:F2} in unpaid fines.");
        }

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
            Status = LoanStatus.Active
        };

        reservation.Status = ReservationStatus.Fulfilled;
        reservation.Book.AvailableCopies--;
        reservation.Book.UpdatedAt = now;

        // Reorder queue
        var subsequentReservations = await db.Reservations
            .Where(r => r.BookId == reservation.BookId &&
                r.QueuePosition > reservation.QueuePosition &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync(ct);

        foreach (var r in subsequentReservations)
        {
            r.QueuePosition--;
        }

        db.Loans.Add(loan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} fulfilled, Loan {LoanId} created", id, loan.Id);

        var loanDto = new LoanDto(
            loan.Id, loan.BookId, reservation.Book.Title, loan.PatronId,
            $"{reservation.Patron.FirstName} {reservation.Patron.LastName}",
            loan.LoanDate, loan.DueDate, loan.ReturnDate, loan.Status, loan.RenewalCount, loan.CreatedAt);

        return (loanDto, null);
    }
}
