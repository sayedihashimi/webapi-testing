using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface ITenantService
{
    Task<PaginatedList<Tenant>> GetTenantsAsync(string? searchTerm, bool? isActive, int pageNumber, int pageSize);
    Task<Tenant?> GetTenantByIdAsync(int id);
    Task<Tenant?> GetTenantWithDetailsAsync(int id);
    Task CreateTenantAsync(Tenant tenant);
    Task UpdateTenantAsync(Tenant tenant);
    Task<(bool Success, string? ErrorMessage)> DeactivateTenantAsync(int id);
    Task<List<Tenant>> GetActiveTenantsAsync();
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
}
