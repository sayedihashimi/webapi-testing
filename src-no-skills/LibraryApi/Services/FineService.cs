using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class FineService : IFineService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<FineService> _logger;

    public FineService(LibraryDbContext context, ILogger<FineService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<FineDto>> GetAllAsync(string? status, int page, int pageSize)
    {
        var query = _context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fineStatus))
            query = query.Where(f => f.Status == fineStatus);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<FineDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FineDto?> GetByIdAsync(int id)
    {
        var fine = await _context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id);

        return fine == null ? null : MapToDto(fine);
    }

    public async Task<(FineDto? Fine, string? Error)> PayAsync(int id)
    {
        var fine = await _context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fine == null) return (null, "Fine not found.");

        if (fine.Status == FineStatus.Paid)
            return (null, "Fine has already been paid.");

        if (fine.Status == FineStatus.Waived)
            return (null, "Fine has been waived and cannot be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Fine {FineId} paid (${Amount:F2}) by patron {PatronId}", id, fine.Amount, fine.PatronId);
        return (MapToDto(fine), null);
    }

    public async Task<(FineDto? Fine, string? Error)> WaiveAsync(int id)
    {
        var fine = await _context.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fine == null) return (null, "Fine not found.");

        if (fine.Status == FineStatus.Paid)
            return (null, "Fine has already been paid and cannot be waived.");

        if (fine.Status == FineStatus.Waived)
            return (null, "Fine has already been waived.");

        fine.Status = FineStatus.Waived;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Fine {FineId} waived (${Amount:F2}) for patron {PatronId}", id, fine.Amount, fine.PatronId);
        return (MapToDto(fine), null);
    }

    public static FineDto MapToDto(Fine f) => new()
    {
        Id = f.Id,
        PatronId = f.PatronId,
        PatronName = f.Patron != null ? $"{f.Patron.FirstName} {f.Patron.LastName}" : string.Empty,
        LoanId = f.LoanId,
        BookTitle = f.Loan?.Book?.Title ?? string.Empty,
        Amount = f.Amount,
        Reason = f.Reason,
        IssuedDate = f.IssuedDate,
        PaidDate = f.PaidDate,
        Status = f.Status,
        CreatedAt = f.CreatedAt
    };
}
