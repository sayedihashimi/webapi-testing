using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KeystoneProperties.Services;

public class TenantService : ITenantService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TenantService> _logger;

    public TenantService(ApplicationDbContext context, ILogger<TenantService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Tenant>> GetTenantsAsync(
        string? search, bool? isActive, int pageNumber, int pageSize)
    {
        var query = _context.Tenants
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(t =>
                EF.Functions.Like(t.FirstName, term) ||
                EF.Functions.Like(t.LastName, term) ||
                EF.Functions.Like(t.Email, term));
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        query = query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName);

        return await PaginatedList<Tenant>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Tenant?> GetByIdAsync(int id)
    {
        return await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tenant?> GetWithDetailsAsync(int id)
    {
        return await _context.Tenants
            .Include(t => t.Leases)
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Property)
            .Include(t => t.Leases)
                .ThenInclude(l => l.Payments)
            .Include(t => t.MaintenanceRequests)
                .ThenInclude(mr => mr.Unit)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task CreateAsync(Tenant tenant)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created tenant {TenantId}: {TenantName}", tenant.Id, tenant.FullName);
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated tenant {TenantId}: {TenantName}", tenant.Id, tenant.FullName);
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);

        if (tenant == null)
        {
            return (false, "Tenant not found.");
        }

        var hasActiveLeases = await _context.Leases
            .AnyAsync(l => l.TenantId == id && l.Status == LeaseStatus.Active);

        if (hasActiveLeases)
        {
            return (false, "Cannot deactivate tenant with active leases.");
        }

        tenant.IsActive = false;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deactivated tenant {TenantId}: {TenantName}", tenant.Id, tenant.FullName);

        return (true, null);
    }

    public async Task<List<Tenant>> GetAllActiveAsync()
    {
        return await _context.Tenants
            .Where(t => t.IsActive)
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var query = _context.Tenants.Where(t => t.Email == email);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
