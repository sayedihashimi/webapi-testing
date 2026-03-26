using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class PropertyService(ApplicationDbContext db, ILogger<PropertyService> logger)
    : IPropertyService
{
    public async Task<PaginatedList<Property>> GetAllAsync(
        string? search, PropertyType? type, bool? isActive,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Properties.AsNoTracking().Include(p => p.Units).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || p.City.ToLower().Contains(s));
        }
        if (type.HasValue)
            query = query.Where(p => p.PropertyType == type.Value);
        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        query = query.OrderBy(p => p.Name);
        return await PaginatedList<Property>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<Property?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Properties
            .Include(p => p.Units)
                .ThenInclude(u => u.Leases.Where(l => l.Status == LeaseStatus.Active))
                    .ThenInclude(l => l.Tenant)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Property> CreateAsync(Property property, CancellationToken ct = default)
    {
        db.Properties.Add(property);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Property created: {PropertyName} (ID: {PropertyId})", property.Name, property.Id);
        return property;
    }

    public async Task UpdateAsync(Property property, CancellationToken ct = default)
    {
        db.Properties.Update(property);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Property updated: {PropertyName} (ID: {PropertyId})", property.Name, property.Id);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var property = await db.Properties
            .Include(p => p.Units)
                .ThenInclude(u => u.Leases)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (property is null)
            return (false, "Property not found.");

        var hasActiveLeases = property.Units.Any(u => u.Leases.Any(l => l.Status == LeaseStatus.Active));
        if (hasActiveLeases)
            return (false, "Cannot deactivate property with active leases.");

        property.IsActive = false;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Property deactivated: {PropertyName} (ID: {PropertyId})", property.Name, property.Id);
        return (true, null);
    }

    public async Task<int> GetOccupiedUnitCountAsync(int propertyId, CancellationToken ct = default)
    {
        return await db.Units.AsNoTracking()
            .CountAsync(u => u.PropertyId == propertyId && u.Status == UnitStatus.Occupied, ct);
    }
}
