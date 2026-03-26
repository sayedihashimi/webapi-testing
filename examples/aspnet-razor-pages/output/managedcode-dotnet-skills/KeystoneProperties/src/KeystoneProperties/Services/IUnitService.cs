using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IUnitService
{
    Task<PaginatedList<Unit>> GetUnitsAsync(int? propertyId, UnitStatus? status, int? bedrooms, decimal? minRent, decimal? maxRent, string? searchTerm, int pageNumber, int pageSize);
    Task<Unit?> GetUnitByIdAsync(int id);
    Task<Unit?> GetUnitWithDetailsAsync(int id);
    Task CreateUnitAsync(Unit unit);
    Task UpdateUnitAsync(Unit unit);
    Task<List<Unit>> GetAvailableUnitsAsync();
    Task<List<Property>> GetAllPropertiesAsync();
}
