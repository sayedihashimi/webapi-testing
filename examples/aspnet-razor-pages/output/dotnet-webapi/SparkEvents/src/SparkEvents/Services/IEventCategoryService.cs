using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IEventCategoryService
{
    Task<PaginatedList<EventCategory>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<List<EventCategory>> GetAllForDropdownAsync(CancellationToken ct = default);
    Task<EventCategory?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<EventCategory> CreateAsync(EventCategory category, CancellationToken ct = default);
    Task UpdateAsync(EventCategory category, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> HasEventsAsync(int id, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
}
