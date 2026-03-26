using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface ITenantService
{
    Task<PaginatedList<Tenant>> GetAllAsync(string? search, bool? isActive, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Tenant?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Tenant> CreateAsync(Tenant tenant, CancellationToken ct = default);
    Task UpdateAsync(Tenant tenant, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeactivateAsync(int id, CancellationToken ct = default);
    Task<List<Tenant>> GetActiveTenantsAsync(CancellationToken ct = default);
}
