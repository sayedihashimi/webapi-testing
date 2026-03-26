using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class UnitService(ApplicationDbContext db, ILogger<UnitService> logger)
    : IUnitService
{
    public async Task<PaginatedList<Unit>> GetAllAsync(
        int? propertyId, UnitStatus? status, int? bedrooms,
        decimal? minRent, decimal? maxRent, string? search,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Units.AsNoTracking()
            .Include(u => u.Property)
            .Include(u => u.Leases.Where(l => l.Status == LeaseStatus.Active))
                .ThenInclude(l => l.Tenant)
            .AsQueryable();

        if (propertyId.HasValue)
            query = query.Where(u => u.PropertyId == propertyId.Value);
        if (status.HasValue)
            query = query.Where(u => u.Status == status.Value);
        if (bedrooms.HasValue)
            query = query.Where(u => u.Bedrooms == bedrooms.Value);
        if (minRent.HasValue)
            query = query.Where(u => u.MonthlyRent >= minRent.Value);
        if (maxRent.HasValue)
            query = query.Where(u => u.MonthlyRent <= maxRent.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.UnitNumber.ToLower().Contains(s));
        }

        query = query.OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber);
        return await PaginatedList<Unit>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<Unit?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Units
            .Include(u => u.Property)
            .Include(u => u.Leases.OrderByDescending(l => l.StartDate))
                .ThenInclude(l => l.Tenant)
            .Include(u => u.Leases)
                .ThenInclude(l => l.Payments)
            .Include(u => u.MaintenanceRequests.OrderByDescending(m => m.SubmittedDate))
                .ThenInclude(m => m.Tenant)
            .Include(u => u.Inspections.OrderByDescending(i => i.ScheduledDate))
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<Unit> CreateAsync(Unit unit, CancellationToken ct = default)
    {
        db.Units.Add(unit);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Unit created: {UnitNumber} in Property {PropertyId} (ID: {UnitId})", unit.UnitNumber, unit.PropertyId, unit.Id);
        return unit;
    }

    public async Task UpdateAsync(Unit unit, CancellationToken ct = default)
    {
        db.Units.Update(unit);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Unit updated: {UnitNumber} (ID: {UnitId})", unit.UnitNumber, unit.Id);
    }

    public async Task<List<Unit>> GetAvailableUnitsAsync(CancellationToken ct = default)
    {
        return await db.Units.AsNoTracking()
            .Include(u => u.Property)
            .Where(u => u.Status == UnitStatus.Available)
            .OrderBy(u => u.Property.Name).ThenBy(u => u.UnitNumber)
            .ToListAsync(ct);
    }

    public async Task<List<Property>> GetAllPropertiesAsync(CancellationToken ct = default)
    {
        return await db.Properties.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }
}
