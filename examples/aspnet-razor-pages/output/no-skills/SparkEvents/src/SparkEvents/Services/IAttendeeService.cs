using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IAttendeeService
{
    Task<PaginatedList<Attendee>> GetAttendeesAsync(string? search, int pageIndex = 1, int pageSize = 10);
    Task<Attendee?> GetAttendeeByIdAsync(int id);
    Task<Attendee?> GetAttendeeByEmailAsync(string email);
    Task<Attendee> CreateAttendeeAsync(Attendee attendee);
    Task UpdateAttendeeAsync(Attendee attendee);
    Task<List<Attendee>> GetAllAttendeesAsync();
    Task<List<Registration>> GetAttendeeRegistrationsAsync(int attendeeId);
}
