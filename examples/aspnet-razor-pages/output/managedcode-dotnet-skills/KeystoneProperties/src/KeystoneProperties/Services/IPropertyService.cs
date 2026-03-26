using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IPropertyService
{
    Task<PaginatedList<Property>> GetPropertiesAsync(string? searchTerm, PropertyType? propertyType, bool? isActive, int pageNumber, int pageSize);
    Task<Property?> GetPropertyByIdAsync(int id);
    Task<Property?> GetPropertyWithUnitsAsync(int id);
    Task CreatePropertyAsync(Property property);
    Task UpdatePropertyAsync(Property property);
    Task<(bool Success, string? ErrorMessage)> DeactivatePropertyAsync(int id);
    Task<int> GetTotalPropertiesCountAsync();
}
