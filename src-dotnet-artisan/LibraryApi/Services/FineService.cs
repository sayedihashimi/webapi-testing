using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public sealed class FineService(LibraryDbContext context, ILogger<FineService> logger) : IFineService
{
    public async Task<PaginatedResponse<FineResponse>> GetFinesAsync(string? status, int page, int pageSize, CancellationToken ct)
    {
        var query = context.Fines.Include(f => f.Patron).AsQueryable();

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

    public async Task<FineDetailResponse?> GetFineByIdAsync(int id, CancellationToken ct)
    {
        var fine = await context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (fine is null)
        {
            return null;
        }

        return new FineDetailResponse(fine.Id, fine.PatronId,
            fine.Patron.FirstName + " " + fine.Patron.LastName,
            fine.LoanId, fine.Loan.Book.Title, fine.Amount, fine.Reason,
            fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt);
    }

    public async Task<Result<FineDetailResponse>> PayFineAsync(int id, CancellationToken ct)
    {
        var fine = await context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (fine is null)
        {
            return Result<FineDetailResponse>.Failure("Fine not found.", 404);
        }

        if (fine.Status != FineStatus.Unpaid)
        {
            return Result<FineDetailResponse>.Failure($"Fine is already '{fine.Status}'. Only unpaid fines can be paid.");
        }

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} paid by patron {PatronId}", id, fine.PatronId);

        return Result<FineDetailResponse>.Success(new FineDetailResponse(
            fine.Id, fine.PatronId, fine.Patron.FirstName + " " + fine.Patron.LastName,
            fine.LoanId, fine.Loan.Book.Title, fine.Amount, fine.Reason,
            fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt));
    }

    public async Task<Result<FineDetailResponse>> WaiveFineAsync(int id, CancellationToken ct)
    {
        var fine = await context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (fine is null)
        {
            return Result<FineDetailResponse>.Failure("Fine not found.", 404);
        }

        if (fine.Status != FineStatus.Unpaid)
        {
            return Result<FineDetailResponse>.Failure($"Fine is already '{fine.Status}'. Only unpaid fines can be waived.");
        }

        fine.Status = FineStatus.Waived;
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Fine {FineId} waived for patron {PatronId}", id, fine.PatronId);

        return Result<FineDetailResponse>.Success(new FineDetailResponse(
            fine.Id, fine.PatronId, fine.Patron.FirstName + " " + fine.Patron.LastName,
            fine.LoanId, fine.Loan.Book.Title, fine.Amount, fine.Reason,
            fine.IssuedDate, fine.PaidDate, fine.Status, fine.CreatedAt));
    }
}
