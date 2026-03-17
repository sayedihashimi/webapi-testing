using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class PatronService
{
    private readonly LibraryDbContext _db;

    public PatronService(LibraryDbContext db) => _db = db;

    public async Task<PaginatedResponse<PatronDto>> GetAllAsync(string? search, MembershipType? membershipType, int page = 1, int pageSize = 10)
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

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return new PaginatedResponse<PatronDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<PatronDetailDto> GetByIdAsync(int id)
    {
        var patron = await _db.Patrons.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Patron with ID {id} not found.");

        var activeLoans = await _db.Loans.CountAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        var unpaidFines = await _db.Fines.Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid).SumAsync(f => f.Amount);

        return new PatronDetailDto
        {
            Id = patron.Id, FirstName = patron.FirstName, LastName = patron.LastName,
            Email = patron.Email, Phone = patron.Phone, Address = patron.Address,
            MembershipDate = patron.MembershipDate, MembershipType = patron.MembershipType.ToString(),
            IsActive = patron.IsActive, CreatedAt = patron.CreatedAt,
            ActiveLoansCount = activeLoans, UnpaidFinesBalance = unpaidFines
        };
    }

    public async Task<PatronDto> CreateAsync(CreatePatronDto dto)
    {
        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email))
            throw new BusinessRuleException("A patron with this email already exists.", 409);

        var patron = new Patron
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email,
            Phone = dto.Phone, Address = dto.Address, MembershipType = dto.MembershipType
        };
        _db.Patrons.Add(patron);
        await _db.SaveChangesAsync();
        return MapToDto(patron);
    }

    public async Task<PatronDto> UpdateAsync(int id, UpdatePatronDto dto)
    {
        var patron = await _db.Patrons.FindAsync(id)
            ?? throw new NotFoundException($"Patron with ID {id} not found.");

        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email && p.Id != id))
            throw new BusinessRuleException("A patron with this email already exists.", 409);

        patron.FirstName = dto.FirstName; patron.LastName = dto.LastName;
        patron.Email = dto.Email; patron.Phone = dto.Phone;
        patron.Address = dto.Address; patron.MembershipType = dto.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(patron);
    }

    public async Task DeleteAsync(int id)
    {
        var patron = await _db.Patrons.FindAsync(id)
            ?? throw new NotFoundException($"Patron with ID {id} not found.");

        var hasActiveLoans = await _db.Loans.AnyAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        if (hasActiveLoans)
            throw new BusinessRuleException("Cannot deactivate patron with active or overdue loans.", 409);

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PaginatedResponse<LoanDto>> GetPatronLoansAsync(int patronId, string? status, int page = 1, int pageSize = 10)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
            query = query.Where(l => l.Status == ls);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => BookService.MapLoanDto(l)).ToListAsync();

        return new PaginatedResponse<LoanDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<PaginatedResponse<ReservationDto>> GetPatronReservationsAsync(int patronId, int page = 1, int pageSize = 10)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Reservations.Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.PatronId == patronId).OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => BookService.MapReservationDto(r)).ToListAsync();

        return new PaginatedResponse<ReservationDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<PaginatedResponse<FineDto>> GetPatronFinesAsync(int patronId, string? status, int page = 1, int pageSize = 10)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found.");

        var query = _db.Fines.Include(f => f.Loan).ThenInclude(l => l.Book)
            .Include(f => f.Patron)
            .Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => FineService.MapFineDto(f)).ToListAsync();

        return new PaginatedResponse<FineDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    private static PatronDto MapToDto(Patron p) => new()
    {
        Id = p.Id, FirstName = p.FirstName, LastName = p.LastName,
        Email = p.Email, Phone = p.Phone, Address = p.Address,
        MembershipDate = p.MembershipDate, MembershipType = p.MembershipType.ToString(),
        IsActive = p.IsActive, CreatedAt = p.CreatedAt
    };
}
