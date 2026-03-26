using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class UnitService(ApplicationDbContext context, ILogger<UnitService> logger) : IUnitService
{
    public async Task<PaginatedList<Unit>> GetUnitsAsync(
        int? propertyId, UnitStatus? status, int? bedrooms, decimal? minRent, decimal? maxRent,
        string? searchTerm, int pageNumber, int pageSize)
    {
        var query = context.Units
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

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => u.UnitNumber.Contains(searchTerm));
        }

        query = query.OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber);

        return await PaginatedList<Unit>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Unit?> GetUnitByIdAsync(int id)
    {
        return await context.Units
            .Include(u => u.Property)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Unit?> GetUnitWithDetailsAsync(int id)
    {
        return await context.Units
            .Include(u => u.Property)
            .Include(u => u.Leases)
                .ThenInclude(l => l.Tenant)
            .Include(u => u.Leases)
                .ThenInclude(l => l.Payments)
            .Include(u => u.MaintenanceRequests)
                .ThenInclude(mr => mr.Tenant)
            .Include(u => u.Inspections)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task CreateUnitAsync(Unit unit)
    {
        unit.CreatedAt = DateTime.UtcNow;
        unit.UpdatedAt = DateTime.UtcNow;
        context.Units.Add(unit);
        await context.SaveChangesAsync();
        logger.LogInformation("Created unit {UnitId}: {UnitNumber} for property {PropertyId}", unit.Id, unit.UnitNumber, unit.PropertyId);
    }

    public async Task UpdateUnitAsync(Unit unit)
    {
        unit.UpdatedAt = DateTime.UtcNow;
        context.Units.Update(unit);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated unit {UnitId}: {UnitNumber}", unit.Id, unit.UnitNumber);
    }

    public async Task<List<Unit>> GetAvailableUnitsAsync()
    {
        return await context.Units
            .Include(u => u.Property)
            .Where(u => u.Status == UnitStatus.Available)
            .AsNoTracking()
            .OrderBy(u => u.Property.Name)
            .ThenBy(u => u.UnitNumber)
            .ToListAsync();
    }

    public async Task<List<Property>> GetAllPropertiesAsync()
    {
        return await context.Properties
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
