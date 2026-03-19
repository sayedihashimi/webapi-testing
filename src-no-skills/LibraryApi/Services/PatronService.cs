using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Fine;
using LibraryApi.DTOs.Loan;
using LibraryApi.DTOs.Patron;
using LibraryApi.DTOs.Reservation;
using LibraryApi.Models;
using LibraryApi.Models.Enums;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class PatronService : IPatronService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<PatronService> _logger;

    public PatronService(LibraryDbContext db, ILogger<PatronService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<PatronListDto>> GetAllAsync(
        string? search, MembershipType? membershipType, bool? isActive, PaginationParams pagination)
    {
        var query = _db.Patrons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(s) ||
                p.LastName.ToLower().Contains(s) ||
                p.Email.ToLower().Contains(s));
        }

        if (membershipType.HasValue)
            query = query.Where(p => p.MembershipType == membershipType.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(p => new PatronListDto(
                p.Id, p.FirstName, p.LastName, p.Email,
                p.MembershipType, p.IsActive, p.MembershipDate))
            .ToListAsync();

        return new PagedResult<PatronListDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<PatronDetailDto> GetByIdAsync(int id)
    {
        var patron = await _db.Patrons
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Patron with ID {id} not found.");

        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        var unpaidFines = await _db.Fines.Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid).ToListAsync();

        return new PatronDetailDto(
            patron.Id, patron.FirstName, patron.LastName, patron.Email,
            patron.Phone, patron.Address, patron.MembershipDate,
            patron.MembershipType, patron.IsActive,
            patron.CreatedAt, patron.UpdatedAt,
            activeLoans, unpaidFines.Count, unpaidFines.Sum(f => f.Amount));
    }

    public async Task<PatronDetailDto> CreateAsync(CreatePatronDto dto)
    {
        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email))
            throw new ArgumentException($"A patron with email '{dto.Email}' already exists.");

        var patron = new Patron
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            MembershipType = dto.MembershipType
        };

        _db.Patrons.Add(patron);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created patron {PatronId}: {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);

        return await GetByIdAsync(patron.Id);
    }

    public async Task<PatronDetailDto> UpdateAsync(int id, UpdatePatronDto dto)
    {
        var patron = await _db.Patrons.FindAsync(id)
            ?? throw new KeyNotFoundException($"Patron with ID {id} not found.");

        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email && p.Id != id))
            throw new ArgumentException($"A patron with email '{dto.Email}' already exists.");

        patron.FirstName = dto.FirstName;
        patron.LastName = dto.LastName;
        patron.Email = dto.Email;
        patron.Phone = dto.Phone;
        patron.Address = dto.Address;
        patron.MembershipType = dto.MembershipType;
        patron.IsActive = dto.IsActive;
        patron.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated patron {PatronId}", id);

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var patron = await _db.Patrons.FindAsync(id)
            ?? throw new KeyNotFoundException($"Patron with ID {id} not found.");

        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        if (activeLoans > 0)
            throw new InvalidOperationException($"Cannot deactivate patron with ID {id} because they have {activeLoans} active loan(s).");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deactivated patron {PatronId}", id);
    }

    public async Task<PagedResult<LoanListDto>> GetPatronLoansAsync(int patronId, LoanStatus? status, PaginationParams pagination)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l => new LoanListDto(
                l.Id, l.Book.Title, $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync();

        return new PagedResult<LoanListDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<List<ReservationListDto>> GetPatronReservationsAsync(int patronId)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        return await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.PatronId == patronId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.ReservationDate)
            .Select(r => new ReservationListDto(
                r.Id, r.Book.Title, $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync();
    }

    public async Task<PagedResult<FineListDto>> GetPatronFinesAsync(int patronId, FineStatus? status, PaginationParams pagination)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Where(f => f.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(f => new FineListDto(
                f.Id, $"{f.Patron.FirstName} {f.Patron.LastName}", f.Loan.Book.Title,
                f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status))
            .ToListAsync();

        return new PagedResult<FineListDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }
}
