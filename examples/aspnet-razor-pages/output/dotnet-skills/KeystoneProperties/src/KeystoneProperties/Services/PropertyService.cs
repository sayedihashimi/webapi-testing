using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public async Task<PaginatedList<Property>> GetPropertiesAsync(
        string? search, PropertyType? type, bool? isActive, int pageNumber, int pageSize)
    {
        var query = _context.Properties
            .Include(p => p.Units)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name, term) ||
                EF.Functions.Like(p.City, term));
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

        return await PaginatedList<Property>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Property?> GetByIdAsync(int id)
    {
        return await _context.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Property?> GetWithUnitsAsync(int id)
    {
        return await _context.Properties
            .Include(p => p.Units)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task CreateAsync(Property property)
    {
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created property {PropertyId}: {PropertyName}", property.Id, property.Name);
    }

    public async Task UpdateAsync(Property property)
    {
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated property {PropertyId}: {PropertyName}", property.Id, property.Name);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var property = await _context.Properties
            .Include(p => p.Units)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
        {
            return (false, "Property not found.");
        }

        var unitIds = property.Units.Select(u => u.Id).ToList();

        var hasActiveLeases = await _context.Leases
            .AnyAsync(l => unitIds.Contains(l.UnitId) && l.Status == LeaseStatus.Active);

        if (hasActiveLeases)
        {
            return (false, "Cannot deactivate property with active leases.");
        }

        property.IsActive = false;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deactivated property {PropertyId}: {PropertyName}", property.Id, property.Name);

        return (true, null);
    }

    public async Task<List<Property>> GetAllActiveAsync()
    {
        return await _context.Properties
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync();
    }
}
