using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class UnitService : IUnitService
{
    private readonly ApplicationDbContext _context;

    public UnitService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Unit> Items, int TotalCount)> GetUnitsAsync(
        int? propertyId, UnitStatus? status, int? bedrooms, decimal? minRent, decimal? maxRent,
        string? search, int page, int pageSize)
    {
        var query = _context.Units.Include(u => u.Property).AsQueryable();

        if (propertyId.HasValue) query = query.Where(u => u.PropertyId == propertyId.Value);
        if (status.HasValue) query = query.Where(u => u.Status == status.Value);
        if (bedrooms.HasValue) query = query.Where(u => u.Bedrooms == bedrooms.Value);
        if (minRent.HasValue) query = query.Where(u => u.MonthlyRent >= minRent.Value);
        if (maxRent.HasValue) query = query.Where(u => u.MonthlyRent <= maxRent.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.UnitNumber.ToLower().Contains(search.ToLower()));

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Unit?> GetByIdAsync(int id) =>
        await _context.Units.Include(u => u.Property).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Unit?> GetWithDetailsAsync(int id) =>
        await _context.Units
            .Include(u => u.Property)
            .Include(u => u.Leases).ThenInclude(l => l.Tenant)
            .Include(u => u.Leases).ThenInclude(l => l.Payments)
            .Include(u => u.MaintenanceRequests).ThenInclude(m => m.Tenant)
            .Include(u => u.Inspections)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task CreateAsync(Unit unit)
    {
        unit.CreatedAt = DateTime.UtcNow;
        unit.UpdatedAt = DateTime.UtcNow;
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Unit unit)
    {
        unit.UpdatedAt = DateTime.UtcNow;
        _context.Units.Update(unit);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Unit>> GetAvailableUnitsAsync() =>
        await _context.Units.Include(u => u.Property)
            .Where(u => u.Status == UnitStatus.Available)
            .OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber)
            .ToListAsync();
}
