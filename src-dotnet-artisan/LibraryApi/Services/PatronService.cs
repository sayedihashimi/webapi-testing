using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class PatronService(LibraryDbContext context, ILogger<PatronService> logger) : IPatronService
{
    public async Task<PaginatedResponse<PatronResponse>> GetPatronsAsync(
        string? search, MembershipType? membershipType, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Patrons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        if (membershipType.HasValue)
        {
            query = query.Where(p => p.MembershipType == membershipType.Value);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PatronResponse(p.Id, p.FirstName, p.LastName, p.Email, p.MembershipType, p.IsActive, p.MembershipDate))
            .ToListAsync(ct);

        return new PaginatedResponse<PatronResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public async Task<PatronDetailResponse?> GetPatronByIdAsync(int id, CancellationToken ct)
    {
        var patron = await context.Patrons.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (patron is null)
        {
            return null;
        }

        var activeLoansCount = await context.Loans.CountAsync(l => l.PatronId == id && l.Status == LoanStatus.Active, ct);
        var totalUnpaidFines = await context.Fines
            .Where(f => f.PatronId == id && f.Status == FineStatus.Unpaid)
            .SumAsync(f => f.Amount, ct);

        return new PatronDetailResponse(
            patron.Id, patron.FirstName, patron.LastName, patron.Email,
            patron.Phone, patron.Address, patron.MembershipDate, patron.MembershipType,
            patron.IsActive, patron.CreatedAt, patron.UpdatedAt,
            activeLoansCount, totalUnpaidFines);
    }

    public async Task<PatronResponse> CreatePatronAsync(CreatePatronRequest request, CancellationToken ct)
    {
        var patron = new Patron
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            MembershipType = request.MembershipType,
            MembershipDate = DateOnly.FromDateTime(DateTime.UtcNow),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Patrons.Add(patron);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created patron {PatronId}: {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);
        return new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.MembershipType, patron.IsActive, patron.MembershipDate);
    }

    public async Task<PatronResponse?> UpdatePatronAsync(int id, UpdatePatronRequest request, CancellationToken ct)
    {
        var patron = await context.Patrons.FindAsync([id], ct);
        if (patron is null)
        {
            return null;
        }

        patron.FirstName = request.FirstName;
        patron.LastName = request.LastName;
        patron.Email = request.Email;
        patron.Phone = request.Phone;
        patron.Address = request.Address;
        patron.MembershipType = request.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return new PatronResponse(patron.Id, patron.FirstName, patron.LastName, patron.Email, patron.MembershipType, patron.IsActive, patron.MembershipDate);
    }

    public async Task<(bool Found, bool HasActiveLoans)> DeactivatePatronAsync(int id, CancellationToken ct)
    {
        var patron = await context.Patrons.FindAsync([id], ct);
        if (patron is null)
        {
            return (false, false);
        }

        var hasActiveLoans = await context.Loans.AnyAsync(l => l.PatronId == id && l.Status == LoanStatus.Active, ct);
        if (hasActiveLoans)
        {
            return (true, true);
        }

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Deactivated patron {PatronId}", id);
        return (true, false);
    }

    public async Task<PaginatedResponse<LoanResponse>> GetPatronLoansAsync(int patronId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Loans
            .Include(l => l.Book).Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
        {
            query = query.Where(l => l.Status == loanStatus);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new LoanResponse(l.Id, l.BookId, l.Book.Title, l.PatronId,
                l.Patron.FirstName + " " + l.Patron.LastName,
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount))
            .ToListAsync(ct);

        return new PaginatedResponse<LoanResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public async Task<PaginatedResponse<ReservationResponse>> GetPatronReservationsAsync(int patronId, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Reservations
            .Include(r => r.Book).Include(r => r.Patron)
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new ReservationResponse(r.Id, r.BookId, r.Book.Title, r.PatronId,
                r.Patron.FirstName + " " + r.Patron.LastName,
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition))
            .ToListAsync(ct);

        return new PaginatedResponse<ReservationResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }

    public async Task<PaginatedResponse<FineResponse>> GetPatronFinesAsync(int patronId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Fines
            .Include(f => f.Patron)
            .Where(f => f.PatronId == patronId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fineStatus))
        {
            query = query.Where(f => f.Status == fineStatus);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => new FineResponse(f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status))
            .ToListAsync(ct);

        return new PaginatedResponse<FineResponse>(items, totalCount, page, pageSize, (int)Math.Ceiling((double)totalCount / pageSize));
    }
}
