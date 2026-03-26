using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IUnitService
{
    Task<PaginatedList<Unit>> GetAllAsync(int? propertyId, UnitStatus? status, int? bedrooms, decimal? minRent, decimal? maxRent, string? search, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Unit?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Unit> CreateAsync(Unit unit, CancellationToken ct = default);
    Task UpdateAsync(Unit unit, CancellationToken ct = default);
    Task<List<Unit>> GetAvailableUnitsAsync(CancellationToken ct = default);
    Task<List<Property>> GetAllPropertiesAsync(CancellationToken ct = default);
}
