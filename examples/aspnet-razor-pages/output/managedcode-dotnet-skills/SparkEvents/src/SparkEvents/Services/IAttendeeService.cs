using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IAttendeeService
{
    Task<(List<Attendee> Items, int TotalCount)> GetFilteredAsync(string? search, int page, int pageSize);
    Task<Attendee?> GetByIdAsync(int id);
    Task<Attendee?> GetByIdWithRegistrationsAsync(int id);
    Task<Attendee> CreateAsync(Attendee attendee);
    Task UpdateAsync(Attendee attendee);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    Task<List<Attendee>> GetAllAsync();
}
