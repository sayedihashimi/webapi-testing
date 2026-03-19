using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Fine;
using LibraryApi.Models.Enums;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class FineService : IFineService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<FineService> _logger;

    public FineService(LibraryDbContext db, ILogger<FineService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<FineListDto>> GetAllAsync(FineStatus? status, PaginationParams pagination)
    {
        var query = _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(f => f.Status == status.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(f => new FineListDto(
                f.Id, $"{f.Patron.FirstName} {f.Patron.LastName}", f.Loan.Book.Title,
                f.Amount, f.Reason, f.IssuedDate, f.PaidDate, f.Status))
            .ToListAsync();

        return new PagedResult<FineListDto>
        {
            Items = items, TotalCount = totalCount,
            Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<FineDetailDto> GetByIdAsync(int id)
    {
        var fine = await _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        return MapToDetail(fine);
    }

    public async Task<FineDetailDto> PayAsync(int id)
    {
        var fine = await _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Paid fine {FineId} of ${Amount}", id, fine.Amount);

        return MapToDetail(fine);
    }

    public async Task<FineDetailDto> WaiveAsync(int id)
    {
        var fine = await _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'.");

        fine.Status = FineStatus.Waived;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Waived fine {FineId} of ${Amount}", id, fine.Amount);

        return MapToDetail(fine);
    }

    private static FineDetailDto MapToDetail(Models.Fine fine)
    {
        return new FineDetailDto(
            fine.Id, fine.PatronId, $"{fine.Patron.FirstName} {fine.Patron.LastName}",
            fine.LoanId, fine.Loan.Book.Title,
            fine.Amount, fine.Reason, fine.IssuedDate,
            fine.PaidDate, fine.Status, fine.CreatedAt);
    }
}
