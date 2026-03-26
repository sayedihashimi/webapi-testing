using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using Microsoft.EntityFrameworkCore;

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

    public async Task<(List<Tenant> Items, int TotalCount)> GetTenantsAsync(
        string? search, bool? isActive, int page, int pageSize)
    {
        var query = _context.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(t => t.FirstName.ToLower().Contains(s) ||
                                     t.LastName.ToLower().Contains(s) ||
                                     t.Email.ToLower().Contains(s));
        }
        if (isActive.HasValue) query = query.Where(t => t.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Tenant?> GetByIdAsync(int id) =>
        await _context.Tenants.FindAsync(id);

    public async Task<Tenant?> GetWithDetailsAsync(int id) =>
        await _context.Tenants
            .Include(t => t.Leases).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .Include(t => t.Leases).ThenInclude(l => l.Payments)
            .Include(t => t.MaintenanceRequests).ThenInclude(m => m.Unit)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task CreateAsync(Tenant tenant)
    {
        tenant.CreatedAt = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Tenant created: {Name} (ID: {Id})", tenant.FullName, tenant.Id);
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        tenant.UpdatedAt = DateTime.UtcNow;
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> DeactivateAsync(int id)
    {
        var tenant = await _context.Tenants.Include(t => t.Leases)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant == null) return (false, "Tenant not found.");

        if (tenant.Leases.Any(l => l.Status == LeaseStatus.Active))
            return (false, "Cannot deactivate tenant — they have active leases.");

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Tenant deactivated: {Name} (ID: {Id})", tenant.FullName, tenant.Id);
        return (true, null);
    }

    public async Task<List<Tenant>> GetActiveTenantsAsync() =>
        await _context.Tenants.Where(t => t.IsActive)
            .OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
            .ToListAsync();

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        var query = _context.Tenants.Where(t => t.Email.ToLower() == email.ToLower());
        if (excludeId.HasValue) query = query.Where(t => t.Id != excludeId.Value);
        return !await query.AnyAsync();
    }
}
