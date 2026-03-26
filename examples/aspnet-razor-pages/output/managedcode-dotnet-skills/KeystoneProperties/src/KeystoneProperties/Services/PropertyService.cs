using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class PropertyService(ApplicationDbContext context, ILogger<PropertyService> logger) : IPropertyService
{
    public async Task<PaginatedList<Property>> GetPropertiesAsync(
        string? searchTerm, PropertyType? propertyType, bool? isActive, int pageNumber, int pageSize)
    {
        var query = context.Properties
            .Include(p => p.Units)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || p.City.Contains(searchTerm));
        }

        if (propertyType.HasValue)
        {
            query = query.Where(p => p.PropertyType == propertyType.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        query = query.OrderBy(p => p.Name);

        return await PaginatedList<Property>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Property?> GetPropertyByIdAsync(int id)
    {
        return await context.Properties.FindAsync(id);
    }

    public async Task<Property?> GetPropertyWithUnitsAsync(int id)
    {
        return await context.Properties
            .Include(p => p.Units)
                .ThenInclude(u => u.Leases.Where(l => l.Status == LeaseStatus.Active))
                    .ThenInclude(l => l.Tenant)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task CreatePropertyAsync(Property property)
    {
        property.CreatedAt = DateTime.UtcNow;
        property.UpdatedAt = DateTime.UtcNow;
        context.Properties.Add(property);
        await context.SaveChangesAsync();
        logger.LogInformation("Created property {PropertyId}: {PropertyName}", property.Id, property.Name);
    }

    public async Task UpdatePropertyAsync(Property property)
    {
        property.UpdatedAt = DateTime.UtcNow;
        context.Properties.Update(property);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated property {PropertyId}: {PropertyName}", property.Id, property.Name);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeactivatePropertyAsync(int id)
    {
        var property = await context.Properties
            .Include(p => p.Units)
                .ThenInclude(u => u.Leases)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property is null)
        {
            return (false, "Property not found.");
        }

        var hasActiveLeases = property.Units
            .SelectMany(u => u.Leases)
            .Any(l => l.Status == LeaseStatus.Active);

        if (hasActiveLeases)
        {
            return (false, "Cannot deactivate property with active leases. Please terminate or expire all leases first.");
        }

        property.IsActive = false;
        property.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Deactivated property {PropertyId}: {PropertyName}", property.Id, property.Name);
        return (true, null);
    }

    public async Task<int> GetTotalPropertiesCountAsync()
    {
        return await context.Properties.CountAsync();
    }
}
