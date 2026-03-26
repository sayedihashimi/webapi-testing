using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class TenantService(ApplicationDbContext db, ILogger<TenantService> logger) : ITenantService
{
    public async Task<PaginatedList<Tenant>> GetTenantsAsync(string? search, bool? isActive, int pageNumber, int pageSize)
    {
        var query = db.Tenants.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.FirstName.Contains(search) || t.LastName.Contains(search) || t.Email.Contains(search));
        }
        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<Tenant>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Tenant?> GetByIdAsync(int id)
    {
        return await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tenant?> GetWithDetailsAsync(int id)
    {
        return await db.Tenants
            .Include(t => t.Leases).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .Include(t => t.Leases).ThenInclude(l => l.Payments)
            .Include(t => t.MaintenanceRequests).ThenInclude(m => m.Unit)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Tenant>> GetActiveTenantsAsync()
    {
        return await db.Tenants.Where(t => t.IsActive).AsNoTracking().OrderBy(t => t.LastName).ThenBy(t => t.FirstName).ToListAsync();
    }

    public async Task CreateAsync(Tenant tenant)
    {
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
        logger.LogInformation("Tenant created: {TenantName} (ID: {TenantId})", tenant.FullName, tenant.Id);
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        db.Tenants.Update(tenant);
        await db.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var tenant = await db.Tenants.Include(t => t.Leases).FirstOrDefaultAsync(t => t.Id == id);
        if (tenant is null)
        {
            return (false, "Tenant not found.");
        }

        if (tenant.Leases.Any(l => l.Status == LeaseStatus.Active))
        {
            return (false, "Cannot deactivate tenant with active leases.");
        }

        tenant.IsActive = false;
        await db.SaveChangesAsync();
        logger.LogInformation("Tenant deactivated: {TenantName} (ID: {TenantId})", tenant.FullName, tenant.Id);
        return (true, null);
    }
}
