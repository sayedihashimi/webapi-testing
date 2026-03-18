using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class PatronService : IPatronService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<PatronService> _logger;

    public PatronService(LibraryDbContext context, ILogger<PatronService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<PatronDto>> GetAllAsync(string? search, MembershipType? membershipType, int page, int pageSize)
    {
        var query = _context.Patrons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.FirstName.ToLower().Contains(s) || p.LastName.ToLower().Contains(s) || p.Email.ToLower().Contains(s));
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

        return new PaginatedResponse<PatronDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PatronDetailDto?> GetByIdAsync(int id)
    {
        var patron = await _context.Patrons
            .Include(p => p.Loans)
            .Include(p => p.Fines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patron == null) return null;

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
            ActiveLoansCount = patron.Loans.Count(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue),
            TotalUnpaidFines = patron.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount)
        };
    }

    public async Task<PatronDto> CreateAsync(CreatePatronDto dto)
    {
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

        _context.Patrons.Add(patron);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created patron {PatronId}: {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);
        return MapToDto(patron);
    }

    public async Task<PatronDto?> UpdateAsync(int id, UpdatePatronDto dto)
    {
        var patron = await _context.Patrons.FindAsync(id);
        if (patron == null) return null;

        patron.FirstName = dto.FirstName;
        patron.LastName = dto.LastName;
        patron.Email = dto.Email;
        patron.Phone = dto.Phone;
        patron.Address = dto.Address;
        patron.MembershipType = dto.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated patron {PatronId}", id);
        return MapToDto(patron);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var patron = await _context.Patrons
            .Include(p => p.Loans)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patron == null) return (false, "Patron not found");

        if (patron.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            return (false, "Cannot deactivate patron with active loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deactivated patron {PatronId}", id);
        return (true, null);
    }

    public async Task<PaginatedResponse<LoanDto>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize)
    {
        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
                query = query.Where(l => l.Status == loanStatus);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<LoanDto>
        {
            Items = items.Select(LoanService.MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResponse<ReservationDto>> GetPatronReservationsAsync(int patronId, int page, int pageSize)
    {
        var query = _context.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<ReservationDto>
        {
            Items = items.Select(ReservationService.MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResponse<FineDto>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize)
    {
        var query = _context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<FineStatus>(status, true, out var fineStatus))
                query = query.Where(f => f.Status == fineStatus);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<FineDto>
        {
            Items = items.Select(FineService.MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
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
        MembershipType = p.MembershipType,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
