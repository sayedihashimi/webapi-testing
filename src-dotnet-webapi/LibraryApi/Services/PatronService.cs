using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class PatronService(LibraryDbContext db, ILogger<PatronService> logger) : IPatronService
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
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatronResponse(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipDate, p.MembershipType, p.IsActive, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<PatronResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PatronDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (patron is null) return null;

        var activeLoans = await db.Loans.CountAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);
        var unpaidFines = await db.Fines.Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid).SumAsync(f => f.Amount, ct);

        return new PatronDetailResponse(
            patron.Id, patron.FirstName, patron.LastName, patron.Email,
            patron.Phone, patron.Address, patron.MembershipDate,
            patron.MembershipType, patron.IsActive, patron.CreatedAt, patron.UpdatedAt,
            activeLoans, unpaidFines);
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
            MembershipType = request.MembershipType
        };

        db.Patrons.Add(patron);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created patron {PatronId}: {Email}", patron.Id, patron.Email);
        return new PatronResponse(
            patron.Id, patron.FirstName, patron.LastName, patron.Email,
            patron.Phone, patron.Address, patron.MembershipDate,
            patron.MembershipType, patron.IsActive, patron.CreatedAt, patron.UpdatedAt);
    }

    public async Task<PatronResponse> UpdateAsync(int id, UpdatePatronRequest request, CancellationToken ct)
    {
        var patron = await db.Patrons.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {id} not found.");

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

        logger.LogInformation("Updated patron {PatronId}", patron.Id);
        return new PatronResponse(
            patron.Id, patron.FirstName, patron.LastName, patron.Email,
            patron.Phone, patron.Address, patron.MembershipDate,
            patron.MembershipType, patron.IsActive, patron.CreatedAt, patron.UpdatedAt);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var patron = await db.Patrons.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Patron with ID {id} not found.");

        var hasActiveLoans = await db.Loans.AnyAsync(l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);
        if (hasActiveLoans)
            throw new InvalidOperationException($"Cannot deactivate patron with ID {id} because they have active loans.");

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated patron {PatronId}", id);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetLoansAsync(
        int patronId, LoanStatus? status, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Loans.AsNoTracking()
            .Where(l => l.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanResponse(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status,
                l.RenewalCount, l.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<LoanResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetReservationsAsync(
        int patronId, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Reservations.AsNoTracking()
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationResponse(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status,
                r.QueuePosition, r.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<ReservationResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PaginatedResponse<FineResponse>> GetFinesAsync(
        int patronId, FineStatus? status, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Patrons.AnyAsync(p => p.Id == patronId, ct))
            throw new KeyNotFoundException($"Patron with ID {patronId} not found.");

        var query = db.Fines.AsNoTracking()
            .Where(f => f.PatronId == patronId);

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineResponse(
                f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Loan.Book.Title, f.Amount, f.Reason,
                f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<FineResponse>.Create(items, page, pageSize, totalCount);
    }
}
