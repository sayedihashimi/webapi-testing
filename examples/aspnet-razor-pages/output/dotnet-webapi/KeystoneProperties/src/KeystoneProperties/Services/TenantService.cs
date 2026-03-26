using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class TenantService(ApplicationDbContext db, ILogger<TenantService> logger)
    : ITenantService
{
    public async Task<PaginatedList<Tenant>> GetAllAsync(
        string? search, bool? isActive, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Tenants.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(t =>
                t.FirstName.ToLower().Contains(s) ||
                t.LastName.ToLower().Contains(s) ||
                t.Email.ToLower().Contains(s));
        }
        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);
        return await PaginatedList<Tenant>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<Tenant?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Tenants
            .Include(t => t.Leases.OrderByDescending(l => l.StartDate))
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Property)
            .Include(t => t.Leases)
                .ThenInclude(l => l.Payments.OrderByDescending(p => p.PaymentDate))
            .Include(t => t.MaintenanceRequests.OrderByDescending(m => m.SubmittedDate))
                .ThenInclude(m => m.Unit)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Tenant> CreateAsync(Tenant tenant, CancellationToken ct = default)
    {
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Tenant created: {TenantName} (ID: {TenantId})", tenant.FullName, tenant.Id);
        return tenant;
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken ct = default)
    {
        db.Tenants.Update(tenant);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Tenant updated: {TenantName} (ID: {TenantId})", tenant.FullName, tenant.Id);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id, CancellationToken ct = default)
    {
        var tenant = await db.Tenants
            .Include(t => t.Leases)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        if (tenant is null)
            return (false, "Tenant not found.");

        if (tenant.Leases.Any(l => l.Status == LeaseStatus.Active))
            return (false, "Cannot deactivate tenant with active leases.");

        tenant.IsActive = false;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Tenant deactivated: {TenantName} (ID: {TenantId})", tenant.FullName, tenant.Id);
        return (true, null);
    }

    public async Task<List<Tenant>> GetActiveTenantsAsync(CancellationToken ct = default)
    {
        return await db.Tenants.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
            .ToListAsync(ct);
    }
}
