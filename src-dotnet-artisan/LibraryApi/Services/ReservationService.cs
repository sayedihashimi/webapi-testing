using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
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

    public async Task<PagedResult<ReservationDto>> GetAllAsync(ReservationStatus? status, int page, int pageSize)
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
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => MapToDto(r))
            .ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ReservationDto?> GetByIdAsync(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id);

        return reservation is null ? null : MapToDto(reservation);
    }

    public async Task<ReservationDto> CreateAsync(ReservationCreateDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new KeyNotFoundException($"Book with id {dto.BookId} not found");

        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new KeyNotFoundException($"Patron with id {dto.PatronId} not found");

        if (!patron.IsActive)
            throw new InvalidOperationException("Patron account is not active");

        // Cannot reserve a book that patron has on active loan
        var hasActiveLoan = await _db.Loans
            .AnyAsync(l => l.BookId == dto.BookId && l.PatronId == dto.PatronId && l.Status == LoanStatus.Active);

        if (hasActiveLoan)
            throw new InvalidOperationException("Cannot reserve a book that you currently have on loan");

        // Check if already has a pending/ready reservation for this book
        var hasExistingReservation = await _db.Reservations
            .AnyAsync(r => r.BookId == dto.BookId && r.PatronId == dto.PatronId &&
                          (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));

        if (hasExistingReservation)
            throw new InvalidOperationException("You already have an active reservation for this book");

        // Determine queue position
        var maxPosition = await _db.Reservations
            .Where(r => r.BookId == dto.BookId &&
                       (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var reservation = new Reservation
        {
            BookId = dto.BookId,
            PatronId = dto.PatronId,
            ReservationDate = DateTime.UtcNow,
            Status = ReservationStatus.Pending,
            QueuePosition = maxPosition + 1
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created reservation {Id} for Book {BookId} by Patron {PatronId}, position {Position}",
            reservation.Id, dto.BookId, dto.PatronId, reservation.QueuePosition);

        await _db.Entry(reservation).Reference(r => r.Book).LoadAsync();
        await _db.Entry(reservation).Reference(r => r.Patron).LoadAsync();

        return MapToDto(reservation);
    }

    public async Task<ReservationDto> CancelAsync(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with id {id} not found");

        if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException($"Cannot cancel a reservation with status '{reservation.Status}'");

        reservation.Status = ReservationStatus.Cancelled;

        // Reorder queue positions for remaining reservations
        var remainingReservations = await _db.Reservations
            .Where(r => r.BookId == reservation.BookId &&
                       (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready) &&
                       r.QueuePosition > reservation.QueuePosition)
            .OrderBy(r => r.QueuePosition)
            .ToListAsync();

        foreach (var r in remainingReservations)
        {
            r.QueuePosition--;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled reservation {Id}", id);

        return MapToDto(reservation);
    }

    public async Task<LoanDto> FulfillAsync(int id)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reservation with id {id} not found");

        if (reservation.Status != ReservationStatus.Ready)
            throw new InvalidOperationException("Only reservations with 'Ready' status can be fulfilled");

        if (reservation.ExpirationDate.HasValue && reservation.ExpirationDate.Value < DateTime.UtcNow)
            throw new InvalidOperationException("This reservation has expired");

        if (reservation.Book.AvailableCopies <= 0)
            throw new InvalidOperationException("No copies available");

        // Create loan
        var loanDays = LoanService.GetLoanDays(reservation.Patron.MembershipType);
        var loan = new Loan
        {
            BookId = reservation.BookId,
            PatronId = reservation.PatronId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        reservation.Status = ReservationStatus.Fulfilled;
        reservation.Book.AvailableCopies--;
        reservation.Book.UpdatedAt = DateTime.UtcNow;

        // Reorder queue
        var remainingReservations = await _db.Reservations
            .Where(r => r.BookId == reservation.BookId &&
                       (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready) &&
                       r.QueuePosition > reservation.QueuePosition)
            .OrderBy(r => r.QueuePosition)
            .ToListAsync();

        foreach (var r in remainingReservations)
        {
            r.QueuePosition--;
        }

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Fulfilled reservation {ReservationId}, created loan {LoanId}", id, loan.Id);

        await _db.Entry(loan).Reference(l => l.Book).LoadAsync();
        await _db.Entry(loan).Reference(l => l.Patron).LoadAsync();

        return LoanService.MapToDto(loan);
    }

    public static ReservationDto MapToDto(Reservation r) => new()
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
