using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Middleware;
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

    public async Task<PagedResult<FineDto>> GetFinesAsync(string? status, int page, int pageSize)
    {
        var query = _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(f => f.IssuedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<FineDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<FineDto> GetFineByIdAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found.");
        return MapToDto(fine);
    }

    public async Task<FineDto> PayFineAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new BusinessRuleException($"Fine is already '{fine.Status}'. Only unpaid fines can be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine paid: FineId={FineId}, Amount=${Amount}", fine.Id, fine.Amount);
        return MapToDto(fine);
    }

    public async Task<FineDto> WaiveFineAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new BusinessRuleException($"Fine is already '{fine.Status}'. Only unpaid fines can be waived.");

        fine.Status = FineStatus.Waived;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine waived: FineId={FineId}, Amount=${Amount}", fine.Id, fine.Amount);
        return MapToDto(fine);
    }

    internal static FineDto MapToDto(Fine f) => new()
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
        Status = f.Status.ToString(),
        CreatedAt = f.CreatedAt
    };
}
