using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class FineService(LibraryDbContext db, ILogger<FineService> logger) : IFineService
{
    public async Task<PagedResult<FineDto>> GetFinesAsync(
        string? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .AsQueryable();

        if (Enum.TryParse<FineStatus>(status, true, out var fineStatus))
        {
            query = query.Where(f => f.Status == fineStatus);
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FineDto(
                f.Id, f.PatronId, $"{f.Patron.FirstName} {f.Patron.LastName}",
                f.LoanId, f.Loan.Book.Title, f.Amount, f.Reason,
                f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<FineDto>(items, totalCount, page, pageSize);
    }

    public async Task<FineDto?> GetFineByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Where(f => f.Id == id)
            .Select(f => new FineDto(
                f.Id, f.PatronId, $"{f.Patron.FirstName} {f.Patron.LastName}",
                f.LoanId, f.Loan.Book.Title, f.Amount, f.Reason,
                f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(FineDto? Fine, string? Error)> PayFineAsync(int id, CancellationToken ct = default)
    {
        var fine = await db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (fine is null)
        {
            return (null, "Fine not found.");
        }

        if (fine.Status != FineStatus.Unpaid)
        {
            return (null, $"Fine is already {fine.Status.ToString().ToLower()}.");
        }

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} paid: ${Amount}", fine.Id, fine.Amount);

        var result = new FineDto(
            fine.Id, fine.PatronId, $"{fine.Patron.FirstName} {fine.Patron.LastName}",
            fine.LoanId, fine.Loan.Book.Title, fine.Amount, fine.Reason,
            fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt);

        return (result, null);
    }

    public async Task<(FineDto? Fine, string? Error)> WaiveFineAsync(int id, CancellationToken ct = default)
    {
        var fine = await db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (fine is null)
        {
            return (null, "Fine not found.");
        }

        if (fine.Status != FineStatus.Unpaid)
        {
            return (null, $"Fine is already {fine.Status.ToString().ToLower()}.");
        }

        fine.Status = FineStatus.Waived;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} waived: ${Amount}", fine.Id, fine.Amount);

        var result = new FineDto(
            fine.Id, fine.PatronId, $"{fine.Patron.FirstName} {fine.Patron.LastName}",
            fine.LoanId, fine.Loan.Book.Title, fine.Amount, fine.Reason,
            fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt);

        return (result, null);
    }
}
