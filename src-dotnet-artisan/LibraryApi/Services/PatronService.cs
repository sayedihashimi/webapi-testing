using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class PatronService(LibraryDbContext db, ILogger<PatronService> logger) : IPatronService
{
    public async Task<PagedResult<PatronSummaryDto>> GetPatronsAsync(
        string? search, MembershipType? membershipType,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Patrons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
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
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PatronSummaryDto(p.Id, p.FirstName, p.LastName, p.Email, p.MembershipType, p.IsActive))
            .ToListAsync(ct);

        return new PagedResult<PatronSummaryDto>(items, totalCount, page, pageSize);
    }

    public async Task<PatronDetailDto?> GetPatronByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Patrons
            .Where(p => p.Id == id)
            .Select(p => new PatronDetailDto(
                p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.Address,
                p.MembershipDate, p.MembershipType, p.IsActive,
                p.CreatedAt, p.UpdatedAt,
                p.Loans.Count(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue),
                p.Fines.Where(f => f.Status == FineStatus.Unpaid).Sum(f => f.Amount)
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PatronDetailDto> CreatePatronAsync(CreatePatronDto dto, CancellationToken ct = default)
    {
        var patron = new Patron
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            MembershipType = dto.MembershipType
        };

        db.Patrons.Add(patron);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created patron {PatronId}: {FirstName} {LastName}", patron.Id, patron.FirstName, patron.LastName);

        return new PatronDetailDto(
            patron.Id, patron.FirstName, patron.LastName, patron.Email,
            patron.Phone, patron.Address, patron.MembershipDate, patron.MembershipType,
            patron.IsActive, patron.CreatedAt, patron.UpdatedAt, 0, 0m);
    }

    public async Task<PatronDetailDto?> UpdatePatronAsync(int id, UpdatePatronDto dto, CancellationToken ct = default)
    {
        var patron = await db.Patrons.FindAsync([id], ct);
        if (patron is null)
        {
            return null;
        }

        patron.FirstName = dto.FirstName;
        patron.LastName = dto.LastName;
        patron.Email = dto.Email;
        patron.Phone = dto.Phone;
        patron.Address = dto.Address;
        patron.MembershipType = dto.MembershipType;
        patron.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated patron {PatronId}", id);

        return await GetPatronByIdAsync(id, ct);
    }

    public async Task<(bool Found, bool HasActiveLoans)> DeactivatePatronAsync(int id, CancellationToken ct = default)
    {
        var patron = await db.Patrons.FindAsync([id], ct);
        if (patron is null)
        {
            return (false, false);
        }

        var hasActiveLoans = await db.Loans.AnyAsync(
            l => l.PatronId == id && (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue), ct);

        if (hasActiveLoans)
        {
            return (true, true);
        }

        patron.IsActive = false;
        patron.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated patron {PatronId}", id);

        return (true, false);
    }

    public async Task<List<LoanDto>> GetPatronLoansAsync(int patronId, string? status, CancellationToken ct = default)
    {
        var query = db.Loans
            .Include(l => l.Book)
            .Include(l => l.Patron)
            .Where(l => l.PatronId == patronId);

        if (Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
        {
            query = query.Where(l => l.Status == loanStatus);
        }

        return await query
            .OrderByDescending(l => l.LoanDate)
            .Select(l => new LoanDto(
                l.Id, l.BookId, l.Book.Title, l.PatronId,
                $"{l.Patron.FirstName} {l.Patron.LastName}",
                l.LoanDate, l.DueDate, l.ReturnDate, l.Status, l.RenewalCount, l.CreatedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<List<ReservationDto>> GetPatronReservationsAsync(int patronId, CancellationToken ct = default)
    {
        return await db.Reservations
            .Include(r => r.Book)
            .Include(r => r.Patron)
            .Where(r => r.PatronId == patronId)
            .OrderByDescending(r => r.ReservationDate)
            .Select(r => new ReservationDto(
                r.Id, r.BookId, r.Book.Title, r.PatronId,
                $"{r.Patron.FirstName} {r.Patron.LastName}",
                r.ReservationDate, r.ExpirationDate, r.Status, r.QueuePosition, r.CreatedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<List<FineDto>> GetPatronFinesAsync(int patronId, string? status, CancellationToken ct = default)
    {
        var query = db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Where(f => f.PatronId == patronId);

        if (Enum.TryParse<FineStatus>(status, true, out var fineStatus))
        {
            query = query.Where(f => f.Status == fineStatus);
        }

        return await query
            .OrderByDescending(f => f.IssuedDate)
            .Select(f => new FineDto(
                f.Id, f.PatronId, $"{f.Patron.FirstName} {f.Patron.LastName}",
                f.LoanId, f.Loan.Book.Title, f.Amount, f.Reason,
                f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt
            ))
            .ToListAsync(ct);
    }
}
