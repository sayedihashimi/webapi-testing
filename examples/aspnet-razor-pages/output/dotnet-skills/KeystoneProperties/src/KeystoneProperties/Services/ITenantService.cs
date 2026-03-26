using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface ITenantService
{
    Task<PaginatedList<Tenant>> GetTenantsAsync(string? search, bool? isActive, int pageNumber, int pageSize);
    Task<Tenant?> GetByIdAsync(int id);
    Task<Tenant?> GetWithDetailsAsync(int id);
    Task CreateAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task<(bool Success, string? Error)> DeactivateAsync(int id);
    Task<List<Tenant>> GetAllActiveAsync();
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
}
