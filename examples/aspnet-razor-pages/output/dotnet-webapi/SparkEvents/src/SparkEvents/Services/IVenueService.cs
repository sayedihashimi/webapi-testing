using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IVenueService
{
    Task<PaginatedList<Venue>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<List<Venue>> GetAllForDropdownAsync(CancellationToken ct = default);
    Task<Venue?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Venue?> GetByIdWithEventsAsync(int id, CancellationToken ct = default);
    Task<Venue> CreateAsync(Venue venue, CancellationToken ct = default);
    Task UpdateAsync(Venue venue, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> HasFutureEventsAsync(int id, CancellationToken ct = default);
}
