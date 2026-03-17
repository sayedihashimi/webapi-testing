using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
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

    public async Task<PagedResult<PatronDto>> GetAllAsync(string? search, MembershipType? membershipType, int page, int pageSize)
    {
        var query = _db.Patrons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.FirstName.ToLower().Contains(s) ||
                                     p.LastName.ToLower().Contains(s) ||
                                     p.Email.ToLower().Contains(s));
        }

        if (membershipType.HasValue)
            query = query.Where(p => p.MembershipType == membershipType.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return new PagedResult<PatronDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PatronDetailDto?> GetByIdAsync(int id)
    {
        var patron = await _db.Patrons.FirstOrDefaultAsync(p => p.Id == id);
        if (patron is null) return null;

        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        var unpaidFines = await _db.Fines
            .Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0m;

        return new PatronDetailDto
        {
            Id = patron.Id,
            FirstName = patron.FirstName,
            LastName = patron.LastName,
            Email = patron.Email,
            Phone = patron.Phone,
            Address = patron.Address,
            MembershipDate = patron.MembershipDate,
            MembershipType = patron.MembershipType,
            IsActive = patron.IsActive,
            CreatedAt = patron.CreatedAt,
            UpdatedAt = patron.UpdatedAt,
            ActiveLoansCount = activeLoans,
            UnpaidFinesTotal = unpaidFines
        };
    }

    public async Task<PatronDto> CreateAsync(PatronCreateDto dto)
    {
        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email))
            throw new InvalidOperationException($"A patron with email '{dto.Email}' already exists");

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
        _logger.LogInformation("Created patron {Id}: {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);

        return MapToDto(patron);
    }

    public async Task<PatronDto?> UpdateAsync(int id, PatronUpdateDto dto)
    {
        var patron = await _db.Patrons.FindAsync(id);
        if (patron is null) return null;

        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email && p.Id != id))
            throw new InvalidOperationException($"A patron with email '{dto.Email}' already exists");

        patron.FirstName = dto.FirstName;
        patron.LastName = dto.LastName;
        patron.Email = dto.Email;
        patron.Phone = dto.Phone;
        patron.Address = dto.Address;
        patron.MembershipType = dto.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated patron {Id}", id);

        return MapToDto(patron);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var patron = await _db.Patrons.FindAsync(id);

        if (patron is null)
            throw new KeyNotFoundException($"Patron with id {id} not found");

        var hasActiveLoans = await _db.Loans.AnyAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans)
            throw new InvalidOperationException($"Cannot deactivate patron with id {id} because they have active loans");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deactivated patron {Id}", id);

        return true;
    }

    public async Task<PagedResult<LoanDto>> GetLoansAsync(int patronId, LoanStatus? status, int page, int pageSize)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with id {patronId} not found");

        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => LoanService.MapToDto(l))
            .ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<ReservationDto>> GetReservationsAsync(int patronId)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with id {patronId} not found");

        return await _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.PatronId == patronId &&
                       (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .OrderBy(r => r.ReservationDate)
            .Select(r => ReservationService.MapToDto(r))
            .ToListAsync();
    }

    public async Task<List<FineDto>> GetFinesAsync(int patronId, FineStatus? status)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new KeyNotFoundException($"Patron with id {patronId} not found");

        var query = _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Where(f => f.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        return await query
            .OrderByDescending(f => f.IssuedDate)
            .Select(f => FineService.MapToDto(f))
            .ToListAsync();
    }

    private static PatronDto MapToDto(Patron p) => new()
    {
        Id = p.Id,
        FirstName = p.FirstName,
        LastName = p.LastName,
        Email = p.Email,
        Phone = p.Phone,
        Address = p.Address,
        MembershipDate = p.MembershipDate,
        MembershipType = p.MembershipType,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
