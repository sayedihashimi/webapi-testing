using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IEventService
{
    Task<PaginatedList<Event>> GetEventsAsync(string? search, int? categoryId, EventStatus? status, DateTime? fromDate, DateTime? toDate, int pageNumber = 1, int pageSize = 10);
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

public interface ITicketTypeService
{
    Task<List<TicketType>> GetTicketTypesForEventAsync(int eventId);
    Task<TicketType?> GetTicketTypeByIdAsync(int id);
    Task<TicketType> CreateTicketTypeAsync(TicketType ticketType);
    Task UpdateTicketTypeAsync(TicketType ticketType);
    Task<bool> ToggleActiveAsync(int id);
}

public interface IAttendeeService
{
    Task<PaginatedList<Attendee>> GetAttendeesAsync(string? search, int pageNumber = 1, int pageSize = 10);
    Task<Attendee?> GetAttendeeByIdAsync(int id);
    Task<Attendee?> GetAttendeeByEmailAsync(string email);
    Task<Attendee> CreateAttendeeAsync(Attendee attendee);
    Task UpdateAttendeeAsync(Attendee attendee);
    Task<List<Attendee>> GetAllAttendeesAsync();
}

public interface IRegistrationService
{
    Task<(Registration? Registration, string? Error)> RegisterAsync(int eventId, int attendeeId, int ticketTypeId, string? specialRequests);
    Task<Registration?> GetRegistrationByIdAsync(int id);
    Task<(bool Success, string? Error)> CancelRegistrationAsync(int id, string? reason);
    Task<PaginatedList<Registration>> GetEventRosterAsync(int eventId, string? search, int pageNumber = 1, int pageSize = 10);
    Task<List<Registration>> GetEventWaitlistAsync(int eventId);
    Task<List<Registration>> GetRecentRegistrationsAsync(int count = 10);
    Task<int> GetTotalRegistrationsCountAsync();
}

public interface ICheckInService
{
    Task<(bool Success, string? Error)> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes);
    Task<List<Registration>> SearchForCheckInAsync(int eventId, string? searchTerm);
    Task<(int CheckedIn, int Total)> GetCheckInStatsAsync(int eventId);
    Task<bool> IsCheckInWindowOpenAsync(int eventId);
}

public interface IVenueService
{
    Task<PaginatedList<Venue>> GetVenuesAsync(int pageNumber = 1, int pageSize = 10);
    Task<Venue?> GetVenueByIdAsync(int id);
    Task<Venue> CreateVenueAsync(Venue venue);
    Task UpdateVenueAsync(Venue venue);
    Task<(bool Success, string? Error)> DeleteVenueAsync(int id);
    Task<List<Venue>> GetAllVenuesAsync();
}

public interface ICategoryService
{
    Task<PaginatedList<EventCategory>> GetCategoriesAsync(int pageNumber = 1, int pageSize = 10);
    Task<EventCategory?> GetCategoryByIdAsync(int id);
    Task<EventCategory> CreateCategoryAsync(EventCategory category);
    Task UpdateCategoryAsync(EventCategory category);
    Task<(bool Success, string? Error)> DeleteCategoryAsync(int id);
    Task<List<EventCategory>> GetAllCategoriesAsync();
}
