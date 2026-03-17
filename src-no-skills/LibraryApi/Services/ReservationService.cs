using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class ReservationService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(LibraryDbContext db, ILogger<ReservationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ReservationDto>> GetAllAsync(string? status, int page = 1, int pageSize = 10)
    {
        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReservationStatus>(status, true, out var rs))
            query = query.Where(r => r.Status == rs);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(r => r.ReservationDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => BookService.MapReservationDto(r)).ToListAsync();

        return new PaginatedResponse<ReservationDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<ReservationDto> GetByIdAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found.");

        return BookService.MapReservationDto(reservation);
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
    {
        var book = await _db.Books.FindAsync(dto.BookId)
            ?? throw new NotFoundException($"Book with ID {dto.BookId} not found.");
        var patron = await _db.Patrons.FindAsync(dto.PatronId)
            ?? throw new NotFoundException($"Patron with ID {dto.PatronId} not found.");

        if (!patron.IsActive)
            throw new BusinessRuleException("Patron account is inactive.");

        // Can't reserve a book already on active loan by same patron
        var hasActiveLoan = await _db.Loans.AnyAsync(l =>
            l.BookId == dto.BookId && l.PatronId == dto.PatronId &&
            (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        if (hasActiveLoan)
            throw new BusinessRuleException("Patron already has an active loan for this book.");

        // Can't have duplicate pending reservation
        var hasExisting = await _db.Reservations.AnyAsync(r =>
            r.BookId == dto.BookId && r.PatronId == dto.PatronId &&
            (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready));
        if (hasExisting)
            throw new BusinessRuleException("Patron already has a pending or ready reservation for this book.");

        // Get next queue position
        var maxPosition = await _db.Reservations
            .Where(r => r.BookId == dto.BookId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .MaxAsync(r => (int?)r.QueuePosition) ?? 0;

        var reservation = new Reservation
        {
            BookId = dto.BookId, PatronId = dto.PatronId,
            QueuePosition = maxPosition + 1
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation {ReservationId} created for Book {BookId} by Patron {PatronId}", reservation.Id, dto.BookId, dto.PatronId);

        return await GetByIdAsync(reservation.Id);
    }

    public async Task<ReservationDto> CancelAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Ready)
            throw new BusinessRuleException("Only pending or ready reservations can be cancelled.");

        reservation.Status = ReservationStatus.Cancelled;

        // Re-order queue positions
        var laterReservations = await _db.Reservations
            .Where(r => r.BookId == reservation.BookId && r.QueuePosition > reservation.QueuePosition &&
                   (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync();

        foreach (var r in laterReservations)
            r.QueuePosition--;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation {ReservationId} cancelled", id);

        return BookService.MapReservationDto(reservation);
    }

    public async Task<LoanDto> FulfillAsync(int id)
    {
        var reservation = await _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException($"Reservation with ID {id} not found.");

        if (reservation.Status != ReservationStatus.Ready)
            throw new BusinessRuleException("Only reservations with 'Ready' status can be fulfilled.");

        if (reservation.Book.AvailableCopies <= 0)
            throw new BusinessRuleException("No available copies of this book.");

        var patron = reservation.Patron;

        // Check fine threshold
        var unpaidFines = await _db.Fines.Where(f => f.PatronId == patron.Id && f.Status == FineStatus.Unpaid).SumAsync(f => f.Amount);
        if (unpaidFines >= 10m)
            throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines. Must be under $10.00 to checkout.");

        var loanDays = patron.MembershipType switch
        {
            MembershipType.Premium => 21,
            MembershipType.Student => 7,
            _ => 14
        };

        var loan = new Loan
        {
            BookId = reservation.BookId, PatronId = reservation.PatronId,
            LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(loanDays),
            Status = LoanStatus.Active
        };

        reservation.Status = ReservationStatus.Fulfilled;
        reservation.Book.AvailableCopies--;

        // Re-order queue
        var laterReservations = await _db.Reservations
            .Where(r => r.BookId == reservation.BookId && r.QueuePosition > reservation.QueuePosition &&
                   (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.QueuePosition)
            .ToListAsync();

        foreach (var r in laterReservations)
            r.QueuePosition--;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reservation {ReservationId} fulfilled. Loan {LoanId} created.", id, loan.Id);

        var loanWithRelations = await _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .FirstAsync(l => l.Id == loan.Id);
        return BookService.MapLoanDto(loanWithRelations);
    }
}
