using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IUnitService
{
    Task<PaginatedList<Unit>> GetUnitsAsync(int? propertyId, UnitStatus? status, int? bedrooms, decimal? minRent, decimal? maxRent, string? search, int pageNumber, int pageSize);
    Task<Unit?> GetByIdAsync(int id);
    Task<Unit?> GetWithDetailsAsync(int id);
    Task CreateAsync(Unit unit);
    Task UpdateAsync(Unit unit);
    Task<List<Unit>> GetAvailableUnitsAsync();
    Task UpdateStatusAsync(int unitId, UnitStatus status);
}
