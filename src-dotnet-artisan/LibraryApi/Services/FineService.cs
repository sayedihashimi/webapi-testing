using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
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

    public async Task<PagedResult<FineDto>> GetAllAsync(FineStatus? status, int page, int pageSize)
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
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => MapToDto(f))
            .ToListAsync();

        return new PagedResult<FineDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<FineDto?> GetByIdAsync(int id)
    {
        var fine = await _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id);

        return fine is null ? null : MapToDto(fine);
    }

    public async Task<FineDto> PayAsync(int id)
    {
        var fine = await _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with id {id} not found");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Paid fine {Id}, amount ${Amount}", id, fine.Amount);

        return MapToDto(fine);
    }

    public async Task<FineDto> WaiveAsync(int id)
    {
        var fine = await _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException($"Fine with id {id} not found");

        if (fine.Status != FineStatus.Unpaid)
            throw new InvalidOperationException($"Fine is already '{fine.Status}'");

        fine.Status = FineStatus.Waived;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Waived fine {Id}, amount ${Amount}", id, fine.Amount);

        return MapToDto(fine);
    }

    public static FineDto MapToDto(Fine f) => new()
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
