using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class PropertyService : IPropertyService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PropertyService> _logger;

    public PropertyService(ApplicationDbContext context, ILogger<PropertyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<Property> Items, int TotalCount)> GetPropertiesAsync(
        string? search, PropertyType? type, bool? isActive, int page, int pageSize)
    {
        var query = _context.Properties.Include(p => p.Units).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || p.City.ToLower().Contains(s));
        }
        if (type.HasValue) query = query.Where(p => p.PropertyType == type.Value);
        if (isActive.HasValue) query = query.Where(p => p.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Property?> GetByIdAsync(int id) =>
        await _context.Properties.FindAsync(id);

    public async Task<Property?> GetWithUnitsAsync(int id) =>
        await _context.Properties.Include(p => p.Units)
            .ThenInclude(u => u.Leases.Where(l => l.Status == LeaseStatus.Active))
            .ThenInclude(l => l.Tenant)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task CreateAsync(Property property)
    {
        property.CreatedAt = DateTime.UtcNow;
        property.UpdatedAt = DateTime.UtcNow;
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Property created: {Name} (ID: {Id})", property.Name, property.Id);
    }

    public async Task UpdateAsync(Property property)
    {
        property.UpdatedAt = DateTime.UtcNow;
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var property = await _context.Properties.Include(p => p.Units)
            .ThenInclude(u => u.Leases)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null) return (false, "Property not found.");

        var hasActiveLeases = property.Units.Any(u =>
            u.Leases.Any(l => l.Status == LeaseStatus.Active));

        if (hasActiveLeases)
            return (false, "Cannot deactivate property — one or more units have active leases.");

        property.IsActive = false;
        property.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Property deactivated: {Name} (ID: {Id})", property.Name, property.Id);
        return (true, null);
    }

    public async Task<int> GetOccupiedUnitCountAsync(int propertyId) =>
        await _context.Units.CountAsync(u => u.PropertyId == propertyId && u.Status == UnitStatus.Occupied);
}
