using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IPropertyService
{
    Task<PaginatedList<Property>> GetAllAsync(string? search, PropertyType? type, bool? isActive, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Property?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Property> CreateAsync(Property property, CancellationToken ct = default);
    Task UpdateAsync(Property property, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeactivateAsync(int id, CancellationToken ct = default);
    Task<int> GetOccupiedUnitCountAsync(int propertyId, CancellationToken ct = default);
}
