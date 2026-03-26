using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IPropertyService
{
    Task<PaginatedList<Property>> GetPropertiesAsync(string? search, PropertyType? type, bool? isActive, int pageNumber, int pageSize);
    Task<Property?> GetByIdAsync(int id);
    Task<Property?> GetWithUnitsAsync(int id);
    Task CreateAsync(Property property);
    Task UpdateAsync(Property property);
    Task<(bool Success, string? Error)> DeactivateAsync(int id);
    Task<List<Property>> GetAllActiveAsync();
}
