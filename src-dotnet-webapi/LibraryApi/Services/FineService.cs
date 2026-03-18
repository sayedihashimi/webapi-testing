using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public interface IFineService
{
    Task<PaginatedResponse<FineResponse>> GetAllAsync(FineStatus? status, int page, int pageSize, CancellationToken ct);
    Task<FineResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<FineResponse> PayAsync(int id, CancellationToken ct);
    Task<FineResponse> WaiveAsync(int id, CancellationToken ct);
}

public class FineService(LibraryDbContext db, ILogger<FineService> logger) : IFineService
{
    public async Task<PaginatedResponse<FineResponse>> GetAllAsync(
        FineStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Fines.AsNoTracking()
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => MapToResponse(f))
            .ToListAsync(ct);

        return PaginatedResponse<FineResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<FineResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Fines.AsNoTracking()
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .Where(f => f.Id == id)
            .Select(f => MapToResponse(f))
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
            throw new InvalidOperationException($"Fine is already '{fine.Status}'. Only unpaid fines can be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} paid by Patron {PatronId}", fine.Id, fine.PatronId);
        return MapToResponse(fine);
    }

    public async Task<FineResponse> WaiveAsync(int id, CancellationToken ct)
    {
        var fine = await db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'. Only unpaid fines can be waived.");

        fine.Status = FineStatus.Waived;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} waived for Patron {PatronId}", fine.Id, fine.PatronId);
        return MapToResponse(fine);
    }

    internal static FineResponse MapToResponse(Fine f) => new()
    {
        Id = f.Id,
        PatronId = f.PatronId,
        PatronName = $"{f.Patron.FirstName} {f.Patron.LastName}",
        LoanId = f.LoanId,
        BookTitle = f.Loan.Book.Title,
        Amount = f.Amount,
        Reason = f.Reason,
        IssuedDate = f.IssuedDate,
        PaidDate = f.PaidDate,
        Status = f.Status,
        CreatedAt = f.CreatedAt
    };
}
