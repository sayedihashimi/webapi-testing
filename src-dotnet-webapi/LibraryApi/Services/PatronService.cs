using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class PatronService(LibraryDbContext db, ILogger<PatronService> logger) : IPatronService
{
    public async Task<PaginatedResponse<PatronResponse>> GetAllAsync(
        string? search, MembershipType? membershipType, bool? isActive,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Patrons.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        if (membershipType.HasValue)
            query = query.Where(p => p.MembershipType == membershipType.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatronResponse(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipDate, p.MembershipType, p.IsActive,
                p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<PatronResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PatronDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Patrons.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PatronDetailResponse(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipDate, p.MembershipType, p.IsActive,
                p.CreatedAt, p.UpdatedAt,
                p.Loans.Count(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue),
                p.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount)))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PatronResponse> CreateAsync(CreatePatronRequest request, CancellationToken ct)
    {
        var emailExists = await db.Patrons.AsNoTracking().AnyAsync(p => p.Email.ToLower() == request.Email.ToLower(), ct);
        if (emailExists)
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Patrons.Add(patron);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created patron {PatronId}: {Email}", patron.Id, patron.Email);

        return MapToResponse(patron);
    }

    public async Task<PatronResponse?> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct)
    {
        var patron = await db.Patrons.FindAsync([id], ct);
        if (patron is null) return null;

        var emailDuplicate = await db.Patrons.AsNoTracking()
            .AnyAsync(p => p.Id != id && p.Email.ToLower() == request.Email.ToLower(), ct);
        if (emailDuplicate)
            throw new InvalidOperationException($"A patron with email '{request.Email}' already exists.");

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated patron {PatronId}", id);

        return MapToResponse(patron);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons
            .Include(p => p.Loans)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (patron is null)
            throw new KeyNotFoundException($"Patron with ID {id} not found.");

        if (patron.Loans.Any(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue))
            throw new InvalidOperationException($"Cannot deactivate patron with ID {id} because they have active loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deactivated patron {PatronId}", id);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetLoansAsync(
        int patronId, LoanStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var exists = await db.Patrons.AsNoTracking().AnyAsync(p => p.Id == patronId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Loans.AsNoTracking()
            .Where(l => l.PatronId == patronId)
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetReservationsAsync(int patronId, CancellationToken ct)
    {
        var exists = await db.Patrons.AsNoTracking().AnyAsync(p => p.Id == patronId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        return await db.Reservations.AsNoTracking()
            .Where(r => r.PatronId == patronId &&
                (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Ready))
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .OrderBy(r => r.ReservationDate)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<PaginatedResponse<FineResponse>> GetFinesAsync(
        int patronId, FineStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var exists = await db.Patrons.AsNoTracking().AnyAsync(p => p.Id == patronId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Fines.AsNoTracking()
            .Where(f => f.PatronId == patronId)
            .Include(f => f.Patron)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineResponse(
                f.Id, f.PatronId, $"{f.Patron.FirstName} {f.Patron.LastName}",
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<FineResponse>.Create(items, page, pageSize, totalCount);
    }

    private static PatronResponse MapToResponse(Patron p) =>
        new(p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
            p.MembershipDate, p.MembershipType, p.IsActive,
            p.CreatedAt, p.UpdatedAt);
}
