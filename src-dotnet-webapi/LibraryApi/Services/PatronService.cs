using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface IPatronService
{
    Task<PaginatedResponse<PatronResponse>> GetAllAsync(string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct);
    Task<PatronDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PatronResponse> CreateAsync(CreatePatronRequest request, CancellationToken ct);
    Task<PatronResponse?> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(int patronId, LoanStatus? status, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize, CancellationToken ct);
    Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(int patronId, FineStatus? status, int page, int pageSize, CancellationToken ct);
}

public class PatronService(LibraryDbContext db) : IPatronService
{
    public async Task<PaginatedResponse<PatronResponse>> GetAllAsync(
        string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Patrons.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        if (membershipType.HasValue)
            query = query.Where(p => p.MembershipType == membershipType.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);

        return PaginatedResponse<PatronResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PatronDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Patrons.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatronDetailResponse
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
                UpdatedAt = p.UpdatedAt,
                ActiveLoansCount = p.Loans.Count(l => l.Status != LoanStatus.Returned),
                UnpaidFinesBalance = p.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount)
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PatronResponse> CreateAsync(CreatePatronRequest request, CancellationToken ct)
    {
        if (await db.Patrons.AnyAsync(p => p.Email == request.Email, ct))
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        var patron = new Patron
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            MembershipDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MembershipType = request.MembershipType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Patrons.Add(patron);
        await db.SaveChangesAsync(ct);
        return MapToResponse(patron);
    }

    public async Task<PatronResponse?> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct)
    {
        var patron = await db.Patrons.FindAsync([id], ct);
        if (patron is null) return null;

        if (await db.Patrons.AnyAsync(p => p.Email == request.Email && p.Id != id, ct))
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return MapToResponse(patron);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException($"Patron with ID {id} not found.");

        if (await db.Loans.AnyAsync(l => l.PatronId == id && l.Status != LoanStatus.Returned, ct))
            throw new InvalidOperationException("Cannot deactivate a patron with active loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(
        int patronId, LoanStatus? status, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Loans.AsNoTracking()
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => LoanService.MapToResponse(l))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(
        int patronId, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Reservations.AsNoTracking()
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => ReservationService.MapToResponse(r))
            .ToListAsync(ct);

        return PaginatedResponse<ReservationResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(
        int patronId, FineStatus? status, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Fines.AsNoTracking()
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Include(f => f.Patron)
            .Where(f => f.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => FineService.MapToResponse(f))
            .ToListAsync(ct);

        return PaginatedResponse<FineResponse>.Create(items, page, pageSize, totalCount);
    }

    private static PatronResponse MapToResponse(Patron p) => new()
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
