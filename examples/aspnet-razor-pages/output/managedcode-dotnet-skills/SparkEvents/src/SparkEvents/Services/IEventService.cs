using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IEventService
{
    Task<(List<Event> Items, int TotalCount)> GetFilteredAsync(
        string? search, int? categoryId, EventStatus? status,
        DateTime? fromDate, DateTime? toDate, int page, int pageSize);

    Task<Event?> GetByIdAsync(int id);
    Task<Event?> GetByIdWithRegistrationsAsync(int id);
    Task<Event> CreateAsync(Event event_);
    Task UpdateAsync(Event event_);
    Task PublishAsync(int id);
    Task CancelAsync(int id, string reason);
    Task CompleteAsync(int id);
    Task<List<Event>> GetUpcomingEventsAsync(int days = 7);
    Task<List<Event>> GetTodayEventsAsync();
    Task<(int TotalEvents, int TotalRegistrations, int EventsThisMonth)> GetStatsAsync();
    Task<List<Event>> GetUpcomingByVenueAsync(int venueId);
}
