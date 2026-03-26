using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class UnitService(ApplicationDbContext db) : IUnitService
{
    public async Task<PaginatedList<Unit>> GetUnitsAsync(int? propertyId, UnitStatus? status, int? bedrooms, decimal? minRent, decimal? maxRent, string? search, int pageNumber, int pageSize)
    {
        var query = db.Units.Include(u => u.Property).AsNoTracking().AsQueryable();

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
            query = query.Where(u => u.UnitNumber.Contains(search));
        }

        query = query.OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<Unit>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Unit?> GetByIdAsync(int id)
    {
        return await db.Units.Include(u => u.Property).AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Unit?> GetWithDetailsAsync(int id)
    {
        return await db.Units
            .Include(u => u.Property)
            .Include(u => u.Leases).ThenInclude(l => l.Tenant)
            .Include(u => u.Leases).ThenInclude(l => l.Payments)
            .Include(u => u.MaintenanceRequests).ThenInclude(m => m.Tenant)
            .Include(u => u.Inspections)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<Unit>> GetAvailableUnitsAsync()
    {
        return await db.Units.Include(u => u.Property)
            .Where(u => u.Status == UnitStatus.Available)
            .AsNoTracking()
            .OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber)
            .ToListAsync();
    }

    public async Task<List<Unit>> GetByPropertyIdAsync(int propertyId)
    {
        return await db.Units.Where(u => u.PropertyId == propertyId).AsNoTracking().OrderBy(u => u.UnitNumber).ToListAsync();
    }

    public async Task CreateAsync(Unit unit)
    {
        db.Units.Add(unit);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Unit unit)
    {
        db.Units.Update(unit);
        await db.SaveChangesAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await db.Units.CountAsync();
    }

    public async Task<int> GetOccupiedCountAsync()
    {
        return await db.Units.CountAsync(u => u.Status == UnitStatus.Occupied);
    }
}
