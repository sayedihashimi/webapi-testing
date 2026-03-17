using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class FineService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<FineService> _logger;

    public FineService(LibraryDbContext db, ILogger<FineService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResponse<FineDto>> GetAllAsync(string? status, int page = 1, int pageSize = 10)
    {
        var query = _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
            query = query.Where(f => f.Status == fs);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(f => f.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => MapFineDto(f)).ToListAsync();

        return new PaginatedResponse<FineDto> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<FineDto> GetByIdAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found.");

        return MapFineDto(fine);
    }

    public async Task<FineDto> PayAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new BusinessRuleException("Only unpaid fines can be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine {FineId} paid by Patron {PatronId}. Amount: ${Amount}", id, fine.PatronId, fine.Amount);

        return MapFineDto(fine);
    }

    public async Task<FineDto> WaiveAsync(int id)
    {
        var fine = await _db.Fines.Include(f => f.Patron).Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found.");

        if (fine.Status != FineStatus.Unpaid)
            throw new BusinessRuleException("Only unpaid fines can be waived.");

        fine.Status = FineStatus.Waived;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine {FineId} waived for Patron {PatronId}. Amount: ${Amount}", id, fine.PatronId, fine.Amount);

        return MapFineDto(fine);
    }

    internal static FineDto MapFineDto(Fine f) => new()
    {
        Id = f.Id, PatronId = f.PatronId, PatronName = $"{f.Patron.FirstName} {f.Patron.LastName}",
        LoanId = f.LoanId, BookTitle = f.Loan.Book.Title,
        Amount = f.Amount, Reason = f.Reason, IssuedDate = f.IssuedDate,
        PaidDate = f.PaidDate, Status = f.Status.ToString(), CreatedAt = f.CreatedAt
    };
}
