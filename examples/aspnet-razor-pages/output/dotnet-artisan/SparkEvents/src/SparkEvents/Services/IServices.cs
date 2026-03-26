using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IEventService
{
    Task<PaginatedList<Event>> GetEventsAsync(string? search, int? categoryId, EventStatus? status, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize);
    Task<Event?> GetEventByIdAsync(int id);
    Task CreateEventAsync(Event evt);
    Task UpdateEventAsync(Event evt);
    Task CancelEventAsync(int eventId, string reason);
    Task CompleteEventAsync(int eventId);
    Task PublishEventAsync(int eventId);
}

public interface IRegistrationService
{
    Task<Registration> RegisterAsync(int eventId, int attendeeId, int ticketTypeId, string? specialRequests);
    Task CancelRegistrationAsync(int registrationId, string? reason);
    Task<Registration?> GetRegistrationByIdAsync(int id);
    Task<PaginatedList<Registration>> GetEventRosterAsync(int eventId, string? search, int pageNumber, int pageSize);
    Task<List<Registration>> GetEventWaitlistAsync(int eventId);
    Task<string> GenerateConfirmationNumberAsync(Event evt);
    decimal CalculatePrice(TicketType ticketType, Event evt);
}

public interface ICheckInService
{
    Task<CheckIn> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes);
    Task<List<Registration>> SearchForCheckInAsync(int eventId, string query);
    Task<(int checkedIn, int total)> GetCheckInStatsAsync(int eventId);
}
