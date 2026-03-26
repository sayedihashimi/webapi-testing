using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class PropertyService(ApplicationDbContext db, ILogger<PropertyService> logger) : IPropertyService
{
    public async Task<PaginatedList<Property>> GetPropertiesAsync(string? search, PropertyType? type, bool? isActive, int pageNumber, int pageSize)
    {
        var query = db.Properties.Include(p => p.Units).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.City.Contains(search));
        }

        if (type.HasValue)
        {
            query = query.Where(p => p.PropertyType == type.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        query = query.OrderBy(p => p.Name);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<Property>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Property?> GetByIdAsync(int id)
    {
        return await db.Properties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Property?> GetWithUnitsAsync(int id)
    {
        return await db.Properties
            .Include(p => p.Units)
                .ThenInclude(u => u.Leases.Where(l => l.Status == LeaseStatus.Active))
                    .ThenInclude(l => l.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task CreateAsync(Property property)
    {
        db.Properties.Add(property);
        await db.SaveChangesAsync();
        logger.LogInformation("Property created: {PropertyName} (ID: {PropertyId})", property.Name, property.Id);
    }

    public async Task UpdateAsync(Property property)
    {
        db.Properties.Update(property);
        await db.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var property = await db.Properties.Include(p => p.Units).ThenInclude(u => u.Leases).FirstOrDefaultAsync(p => p.Id == id);
        if (property is null)
        {
            return (false, "Property not found.");
        }

        var hasActiveLeases = property.Units.Any(u => u.Leases.Any(l => l.Status == LeaseStatus.Active));
        if (hasActiveLeases)
        {
            return (false, "Cannot deactivate property with active leases.");
        }

        property.IsActive = false;
        await db.SaveChangesAsync();
        logger.LogInformation("Property deactivated: {PropertyName} (ID: {PropertyId})", property.Name, property.Id);
        return (true, null);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await db.Properties.CountAsync(p => p.IsActive);
    }
}
