using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KeystoneProperties.Services;

public class UnitService : IUnitService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitService> _logger;

    public UnitService(ApplicationDbContext context, ILogger<UnitService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Unit>> GetUnitsAsync(
        int? propertyId, UnitStatus? status, int? bedrooms,
        decimal? minRent, decimal? maxRent, string? search,
        int pageNumber, int pageSize)
    {
        var query = _context.Units
            .Include(u => u.Property)
            .AsNoTracking()
            .AsQueryable();

        if (propertyId.HasValue)
        {
            query = query.Where(u => u.PropertyId == propertyId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(u => u.Status == status.Value);
        }

        if (bedrooms.HasValue)
        {
            query = query.Where(u => u.Bedrooms == bedrooms.Value);
        }

        if (minRent.HasValue)
        {
            query = query.Where(u => u.MonthlyRent >= minRent.Value);
        }

        if (maxRent.HasValue)
        {
            query = query.Where(u => u.MonthlyRent <= maxRent.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(u => EF.Functions.Like(u.UnitNumber, term));
        }

        query = query.OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber);

        return await PaginatedList<Unit>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Unit?> GetByIdAsync(int id)
    {
        return await _context.Units
            .Include(u => u.Property)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Unit?> GetWithDetailsAsync(int id)
    {
        return await _context.Units
            .Include(u => u.Property)
            .Include(u => u.Leases)
                .ThenInclude(l => l.Tenant)
            .Include(u => u.MaintenanceRequests)
            .Include(u => u.Inspections)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task CreateAsync(Unit unit)
    {
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created unit {UnitId}: {UnitNumber} for property {PropertyId}",
            unit.Id, unit.UnitNumber, unit.PropertyId);
    }

    public async Task UpdateAsync(Unit unit)
    {
        _context.Units.Update(unit);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated unit {UnitId}: {UnitNumber}", unit.Id, unit.UnitNumber);
    }

    public async Task<List<Unit>> GetAvailableUnitsAsync()
    {
        return await _context.Units
            .Include(u => u.Property)
            .Where(u => u.Status == UnitStatus.Available)
            .OrderBy(u => u.Property.Name)
            .ThenBy(u => u.UnitNumber)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateStatusAsync(int unitId, UnitStatus status)
    {
        var unit = await _context.Units.FindAsync(unitId);
        if (unit == null)
        {
            throw new InvalidOperationException($"Unit with ID {unitId} not found.");
        }

        unit.Status = status;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated unit {UnitId} status to {Status}", unitId, status);
    }
}
