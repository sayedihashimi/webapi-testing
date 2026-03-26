using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IEventService
{
    Task<PaginatedList<Event>> GetEventsAsync(string? search, int? categoryId, EventStatus? status, DateTime? startDate, DateTime? endDate, int pageIndex = 1, int pageSize = 10);
    Task<Event?> GetEventByIdAsync(int id);
    Task<Event> CreateEventAsync(Event evt);
    Task UpdateEventAsync(Event evt);
    Task<bool> PublishEventAsync(int id);
    Task<bool> CancelEventAsync(int id, string reason);
    Task<bool> CompleteEventAsync(int id);
    Task<List<Event>> GetUpcomingEventsAsync(int days = 7);
    Task<List<Event>> GetTodaysEventsAsync();
    Task<int> GetTotalEventsCountAsync();
    Task<int> GetEventsThisMonthCountAsync();
}
