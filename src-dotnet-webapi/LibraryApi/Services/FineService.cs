using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class FineService(LibraryDbContext db, ILogger<FineService> logger) : IFineService
{
    public async Task<PaginatedResponse<FineResponse>> GetAllAsync(
        FineStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Fines.AsNoTracking().AsQueryable();

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

    public async Task<FineResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Fines.AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FineResponse(
                f.Id, f.PatronId,
                f.Patron.FirstName + " " + f.Patron.LastName,
                f.LoanId, f.Loan.Book.Title, f.Amount, f.Reason,
                f.IssuedDate, f.PaidDate, f.Status, f.CreatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<FineResponse> PayAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} paid by patron {PatronId}", id, fine.PatronId);

        return new FineResponse(
            fine.Id, fine.PatronId,
            fine.Patron.FirstName + " " + fine.Patron.LastName,
            fine.LoanId, fine.Loan.Book.Title, fine.Amount, fine.Reason,
            fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt);
    }

    public async Task<FineResponse> WaiveAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'.");

        fine.Status = FineStatus.Waived;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} waived for patron {PatronId}", id, fine.PatronId);

        return new FineResponse(
            fine.Id, fine.PatronId,
            fine.Patron.FirstName + " " + fine.Patron.LastName,
            fine.LoanId, fine.Loan.Book.Title, fine.Amount, fine.Reason,
            fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt);
    }
}
