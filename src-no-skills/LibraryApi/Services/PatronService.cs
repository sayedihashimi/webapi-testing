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

    public async Task<PagedResult<PatronDto>> GetPatronsAsync(string? search, string? membershipType, PaginationParams pagination)
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

        if (!string.IsNullOrWhiteSpace(membershipType) && Enum.TryParse<MembershipType>(membershipType, true, out var mt))
        {
            query = query.Where(p => p.MembershipType == mt);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(p => new PatronDto
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
            })
            .ToListAsync();

        return new PagedResult<PatronDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PatronDetailDto> GetPatronByIdAsync(int id)
    {
        var patron = await _db.Patrons.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException($"Patron with ID {id} not found");

        var activeLoansCount = await _db.Loans.CountAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        var totalUnpaidFines = await _db.Fines
            .Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => (decimal?)f.Amount) ?? 0;

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
            ActiveLoansCount = activeLoansCount,
            TotalUnpaidFines = totalUnpaidFines
        };
    }

    public async Task<PatronDto> CreatePatronAsync(CreatePatronDto dto)
    {
        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email))
            throw new BusinessRuleException($"A patron with email '{dto.Email}' already exists.", 409);

        var patron = new Patron
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            MembershipType = dto.MembershipType,
            MembershipDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Patrons.Add(patron);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created patron: {FirstName} {LastName} (ID: {Id})", patron.FirstName, patron.LastName, patron.Id);

        return MapToDto(patron);
    }

    public async Task<PatronDto> UpdatePatronAsync(int id, UpdatePatronDto dto)
    {
        var patron = await _db.Patrons.FindAsync(id)
            ?? throw new NotFoundException($"Patron with ID {id} not found");

        if (await _db.Patrons.AnyAsync(p => p.Email == dto.Email && p.Id != id))
            throw new BusinessRuleException($"A patron with email '{dto.Email}' already exists.", 409);

        patron.FirstName = dto.FirstName;
        patron.LastName = dto.LastName;
        patron.Email = dto.Email;
        patron.Phone = dto.Phone;
        patron.Address = dto.Address;
        patron.MembershipType = dto.MembershipType;
        patron.IsActive = dto.IsActive;
        patron.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return MapToDto(patron);
    }

    public async Task DeactivatePatronAsync(int id)
    {
        var patron = await _db.Patrons.FindAsync(id)
            ?? throw new NotFoundException($"Patron with ID {id} not found");

        var hasActiveLoans = await _db.Loans.AnyAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        if (hasActiveLoans)
            throw new BusinessRuleException("Cannot deactivate a patron with active or overdue loans.", 409);

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deactivated patron: {FirstName} {LastName} (ID: {Id})", patron.FirstName, patron.LastName, patron.Id);
    }

    public async Task<PagedResult<LoanDto>> GetPatronLoansAsync(int patronId, string? status, PaginationParams pagination)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found");

        var query = _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var ls))
        {
            query = query.Where(l => l.Status == ls);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                BookId = l.BookId,
                BookTitle = l.Book.Title,
                PatronId = l.PatronId,
                PatronName = l.Patron.FirstName + " " + l.Patron.LastName,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                ReturnDate = l.ReturnDate,
                Status = l.Status,
                RenewalCount = l.RenewalCount,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<LoanDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<ReservationDto>> GetPatronReservationsAsync(int patronId, PaginationParams pagination)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found");

        var query = _db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(r => new ReservationDto
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = r.Book.Title,
                PatronId = r.PatronId,
                PatronName = r.Patron.FirstName + " " + r.Patron.LastName,
                ReservationDate = r.ReservationDate,
                ExpirationDate = r.ExpirationDate,
                Status = r.Status,
                QueuePosition = r.QueuePosition,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<PagedResult<FineDto>> GetPatronFinesAsync(int patronId, string? status, PaginationParams pagination)
    {
        if (!await _db.Patrons.AnyAsync(p => p.Id == patronId))
            throw new NotFoundException($"Patron with ID {patronId} not found");

        var query = _db.Fines
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Include(f => f.Patron)
            .Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
        {
            query = query.Where(f => f.Status == fs);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(f => new FineDto
            {
                Id = f.Id,
                PatronId = f.PatronId,
                PatronName = f.Patron.FirstName + " " + f.Patron.LastName,
                LoanId = f.LoanId,
                BookTitle = f.Loan.Book.Title,
                Amount = f.Amount,
                Reason = f.Reason,
                IssuedDate = f.IssuedDate,
                PaidDate = f.PaidDate,
                Status = f.Status,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<FineDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    private static PatronDto MapToDto(Patron patron) => new()
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
        UpdatedAt = patron.UpdatedAt
    };
}
