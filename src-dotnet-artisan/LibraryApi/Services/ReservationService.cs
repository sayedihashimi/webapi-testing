using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class ReservationService(LibraryDbContext context, ILogger<ReservationService> logger) : IReservationService
{
    public async Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Reservations.Include(r => r.Book).Include(r => r.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var resStatus))
        {
            query = query.Where(r => r.Status == resStatus);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync(ct);

        return new PaginatedResponse<ReservationResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public async Task<ReservationDetailResponse?> GetReservationByIdAsync(int id, CancellationToken ct)
    {
        var reservation = await context.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reservation is null)
        {
            return null;
        }

        return MapToDetail(reservation);
    }

    public async Task<Result<ReservationDetailResponse>> CreateReservationAsync(CreateReservationRequest request, CancellationToken ct)
    {
        var book = await context.Books.FindAsync([request.BookId], ct);
        if (book is null)
        {
            return Result<ReservationDetailResponse>.Failure("Book not found.", 404);
        }

        var patron = await context.Patrons.FindAsync([request.PatronId], ct);
        if (patron is null)
        {
            return Result<ReservationDetailResponse>.Failure("Patron not found.", 404);
        }

        // Check if patron already has an active loan for this book
        var hasActiveLoan = await context.Loans
            .AnyAsync(l => l.BookId == request.BookId && l.PatronId == request.PatronId && l.Status == LoanStatus.Active, ct);

        if (hasActiveLoan)
        {
            return Result<ReservationDetailResponse>.Failure("Patron already has an active loan for this book.", 409);
        }

        // Check if patron already has a pending/ready reservation for this book
        var hasExistingReservation = await context.Reservations
            .AnyAsync(r => r.BookId == request.BookId && r.PatronId == request.PatronId &&
                          (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready), ct);

        if (hasExistingReservation)
        {
            return Result<ReservationDetailResponse>.Failure("Patron already has an active reservation for this book.", 409);
        }

        var maxQueuePosition = await context.Reservations
            .Where(r => r.BookId == request.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition, ct) ?? 0;

        var now = DateTime.UtcNow;
        var reservation = new Reservation
        {
            BookId = request.BookId,
            PatronId = request.PatronId,
            ReservationDate = now,
            Status = ReservationStatus.Pending,
            QueuePosition = maxQueuePosition + 1,
            CreatedAt = now
        };

        context.Reservations.Add(reservation);
        await context.SaveChangesAsync(ct);

        var created = await context.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstAsync(r => r.Id == reservation.Id, ct);

        logger.LogInformation("Reservation {ReservationId} created for book {BookId} by patron {PatronId}", reservation.Id, request.BookId, request.PatronId);
        return Result<ReservationDetailResponse>.Success(MapToDetail(created));
    }

    public async Task<Result<ReservationDetailResponse>> CancelReservationAsync(int id, CancellationToken ct)
    {
        var reservation = await context.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reservation is null)
        {
            return Result<ReservationDetailResponse>.Failure("Reservation not found.", 404);
        }

        if (reservation.Status is ReservationStatus.Fulfilled or ReservationStatus.Cancelled or ReservationStatus.Expired)
        {
            return Result<ReservationDetailResponse>.Failure($"Cannot cancel a reservation with status '{reservation.Status}'.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Reservation {ReservationId} cancelled", id);
        return Result<ReservationDetailResponse>.Success(MapToDetail(reservation));
    }

    public async Task<Result<LoanDetailResponse>> FulfillReservationAsync(int id, CancellationToken ct)
    {
        var reservation = await context.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (reservation is null)
        {
            return Result<LoanDetailResponse>.Failure("Reservation not found.", 404);
        }

        if (reservation.Status != ReservationStatus.Ready)
        {
            return Result<LoanDetailResponse>.Failure("Only reservations with 'Ready' status can be fulfilled.");
        }

        if (reservation.Book.AvailableCopies < 1)
        {
            return Result<LoanDetailResponse>.Failure("No available copies to fulfill this reservation.");
        }

        var patron = reservation.Patron;
        var loanDays = patron.MembershipType switch
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

        reservation.Status = ReservationStatus.Fulfilled;
        reservation.Book.AvailableCopies--;
        reservation.Book.UpdatedAt = now;

        context.Loans.Add(loan);
        await context.SaveChangesAsync(ct);

        var createdLoan = await context.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .FirstAsync(l => l.Id == loan.Id, ct);

        logger.LogInformation("Reservation {ReservationId} fulfilled, loan {LoanId} created", id, loan.Id);

        return Result<LoanDetailResponse>.Success(new LoanDetailResponse(
            createdLoan.Id, createdLoan.BookId, createdLoan.Book.Title, createdLoan.Book.ISBN,
            createdLoan.PatronId, createdLoan.Patron.FirstName + " " + createdLoan.Patron.LastName,
            createdLoan.Patron.Email, createdLoan.LoanDate, createdLoan.DueDate, createdLoan.ReturnDate,
            createdLoan.Status, createdLoan.RenewalCount, createdLoan.CreatedAt));
    }

    private static ReservationDetailResponse MapToDetail(Reservation r) =>
        new(r.Id, r.BookId, r.Book.Title, r.PatronId,
            r.Patron.FirstName + " " + r.Patron.LastName,
            r.Patron.Email, r.ReservationDate, r.ExpirationDate,
            r.Status, r.QueuePosition, r.CreatedAt);
}
