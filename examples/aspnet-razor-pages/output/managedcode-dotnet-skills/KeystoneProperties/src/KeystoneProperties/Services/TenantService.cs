using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class TenantService(ApplicationDbContext context, ILogger<TenantService> logger) : ITenantService
{
    public async Task<PaginatedList<Tenant>> GetTenantsAsync(
        string? searchTerm, bool? isActive, int pageNumber, int pageSize)
    {
        var query = context.Tenants
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t =>
                t.FirstName.Contains(searchTerm) ||
                t.LastName.Contains(searchTerm) ||
                t.Email.Contains(searchTerm));
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        return await PaginatedList<Tenant>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Tenant?> GetTenantByIdAsync(int id)
    {
        return await context.Tenants.FindAsync(id);
    }

    public async Task<Tenant?> GetTenantWithDetailsAsync(int id)
    {
        return await context.Tenants
            .Include(t => t.Leases)
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Property)
            .Include(t => t.Leases)
                .ThenInclude(l => l.Payments)
            .Include(t => t.MaintenanceRequests)
                .ThenInclude(mr => mr.Unit)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task CreateTenantAsync(Tenant tenant)
    {
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        logger.LogInformation("Created tenant {TenantId}: {TenantName}", tenant.Id, tenant.FullName);
    }

    public async Task UpdateTenantAsync(Tenant tenant)
    {
        tenant.UpdatedAt = DateTime.UtcNow;
        context.Tenants.Update(tenant);
        await context.SaveChangesAsync();
        logger.LogInformation("Updated tenant {TenantId}: {TenantName}", tenant.Id, tenant.FullName);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeactivateTenantAsync(int id)
    {
        var tenant = await context.Tenants
            .Include(t => t.Leases)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant is null)
        {
            return (false, "Tenant not found.");
        }

        var hasActiveLeases = tenant.Leases.Any(l => l.Status == LeaseStatus.Active);

        if (hasActiveLeases)
        {
            return (false, $"Cannot deactivate tenant {tenant.FullName} because they have active leases. Please terminate all active leases first.");
        }

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Deactivated tenant {TenantId}: {TenantName}", tenant.Id, tenant.FullName);
        return (true, null);
    }

    public async Task<List<Tenant>> GetActiveTenantsAsync()
    {
        return await context.Tenants
            .Where(t => t.IsActive)
            .AsNoTracking()
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var query = context.Tenants.Where(t => t.Email == email);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
