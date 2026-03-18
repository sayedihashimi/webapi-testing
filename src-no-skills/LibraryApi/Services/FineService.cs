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

    public async Task<PagedResult<FineDto>> GetFinesAsync(string? status, PaginationParams pagination)
    {
        var query = _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<FineStatus>(status, true, out var fs))
        {
            query = query.Where(f => f.Status == fs);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(f => f.IssuedDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(f => new FineDto
            {
                Id = f.Id,
                PatronId = f.PatronId,
                PatronName = f.Patron.FirstName + " " + f.Patron.LastName,
                LoanId = f.LoanId,
                BookTitle = f.Loan.Book.Title,
                Amount = f.Amount,
                Reason = f.Reason,
                IssuedDate = f.IssuedDate,
                PaidDate = f.PaidDate,
                Status = f.Status,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<FineDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        };
    }

    public async Task<FineDto> GetFineByIdAsync(int id)
    {
        var fine = await _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found");

        return MapToDto(fine);
    }

    public async Task<FineDto> PayFineAsync(int id)
    {
        var fine = await _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found");

        if (fine.Status == FineStatus.Paid)
            throw new BusinessRuleException("This fine has already been paid.");

        if (fine.Status == FineStatus.Waived)
            throw new BusinessRuleException("This fine has been waived and cannot be paid.");

        fine.Status = FineStatus.Paid;
        fine.PaidDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine paid: FineId={Id}, Amount=${Amount}, PatronId={PatronId}",
            fine.Id, fine.Amount, fine.PatronId);

        return MapToDto(fine);
    }

    public async Task<FineDto> WaiveFineAsync(int id)
    {
        var fine = await _db.Fines
            .Include(f => f.Patron)
            .Include(f => f.Loan).ThenInclude(l => l.Book)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Fine with ID {id} not found");

        if (fine.Status == FineStatus.Paid)
            throw new BusinessRuleException("This fine has already been paid and cannot be waived.");

        if (fine.Status == FineStatus.Waived)
            throw new BusinessRuleException("This fine has already been waived.");

        fine.Status = FineStatus.Waived;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Fine waived: FineId={Id}, Amount=${Amount}, PatronId={PatronId}",
            fine.Id, fine.Amount, fine.PatronId);

        return MapToDto(fine);
    }

    private static FineDto MapToDto(Fine fine) => new()
    {
        Id = fine.Id,
        PatronId = fine.PatronId,
        PatronName = fine.Patron.FirstName + " " + fine.Patron.LastName,
        LoanId = fine.LoanId,
        BookTitle = fine.Loan.Book.Title,
        Amount = fine.Amount,
        Reason = fine.Reason,
        IssuedDate = fine.IssuedDate,
        PaidDate = fine.PaidDate,
        Status = fine.Status,
        CreatedAt = fine.CreatedAt
    };
}
