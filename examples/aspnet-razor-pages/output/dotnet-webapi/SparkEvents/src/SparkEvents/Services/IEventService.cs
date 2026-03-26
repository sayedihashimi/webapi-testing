using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IEventService
{
    Task<PaginatedList<Event>> GetAllAsync(string? search, int? categoryId, EventStatus? status,
        DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Event?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Event?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<Event> CreateAsync(Event evt, CancellationToken ct = default);
    Task UpdateAsync(Event evt, CancellationToken ct = default);
    Task<bool> PublishAsync(int id, CancellationToken ct = default);
    Task<bool> CancelAsync(int id, string reason, CancellationToken ct = default);
    Task<bool> CompleteAsync(int id, CancellationToken ct = default);
    Task<List<Event>> GetUpcomingEventsAsync(int days, CancellationToken ct = default);
    Task<List<Event>> GetTodaysEventsAsync(CancellationToken ct = default);
    Task<int> GetTotalEventsCountAsync(CancellationToken ct = default);
    Task<int> GetEventsThisMonthCountAsync(CancellationToken ct = default);
}
