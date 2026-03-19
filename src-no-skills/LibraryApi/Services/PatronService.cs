using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Middleware;
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

    public async Task<PagedResult<PatronDto>> GetPatronsAsync(string? search, string? membershipType, int page, int pageSize)
    {
        var query = _db.Patrons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.FirstName.ToLower().Contains(s) || p.LastName.ToLower().Contains(s) || p.Email.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(membershipType) && Enum.TryParse<MembershipType>(membershipType, true, out var mt))
            query = query.Where(p => p.MembershipType == mt);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return new PagedResult<PatronDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<PatronDetailDto> GetPatronByIdAsync(int id)
    {
        var patron = await _db.Patrons.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Patron with ID {id} not found.");

        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        var totalUnpaidFines = await _db.Fines.Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid).SumAsync(f => (decimal?)f.Amount) ?? 0m;

        var dto = MapToDto(patron);
        return new PatronDetailDto
        {
            Id = dto.Id, FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email,
            Phone = dto.Phone, Address = dto.Address, MembershipDate = dto.MembershipDate,
            MembershipType = dto.MembershipType, IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt, UpdatedAt = dto.UpdatedAt,
            ActiveLoansCount = activeLoans,
            TotalUnpaidFines = totalUnpaidFines
        };
    }

    public async Task<PatronDto> CreatePatronAsync(PatronCreateDto dto)
    {
        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email))
            throw new ConflictException($"A patron with email '{dto.Email}' already exists.");

        var patron = new Patron
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            MembershipDate = DateOnly.FromDateTime(DateTime.Today),
            MembershipType = dto.MembershipType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Patrons.Add(patron);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Patron created: {PatronId} - {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);
        return MapToDto(patron);
    }

    public async Task<PatronDto> UpdatePatronAsync(int id, PatronUpdateDto dto)
    {
        var patron = await _db.Patrons.FindAsync(id)
            ?? throw new NotFoundException($"Patron with ID {id} not found.");

        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email && p.Id != id))
            throw new ConflictException($"A patron with email '{dto.Email}' already exists.");

        patron.FirstName = dto.FirstName;
        patron.LastName = dto.LastName;
        patron.Email = dto.Email;
        patron.Phone = dto.Phone;
        patron.Address = dto.Address;
        patron.MembershipType = dto.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(patron);
    }

    public async Task DeactivatePatronAsync(int id)
    {
        var patron = await _db.Patrons.FindAsync(id)
            ?? throw new NotFoundException($"Patron with ID {id} not found.");

        var hasActiveLoans = await _db.Loans.AnyAsync(l => l.PatronId == id && l.Status == LoanStatus.Active);
        if (hasActiveLoans)
            throw new ConflictException($"Cannot deactivate patron with ID {id} because they have active loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Patron deactivated: {PatronId}", id);
    }

    public async Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron).Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.LoanDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items.Select(LoanService.MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items.Select(ReservationService.MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Fines.Include(f => f.Loan).ThenInclude(l => l.Book).Include(f => f.Patron)
            .Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(f => f.IssuedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<FineDto>
        {
            Items = items.Select(FineService.MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
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
        MembershipType = p.MembershipType.ToString(),
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
