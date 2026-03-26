using SparkEvents.Models;

namespace SparkEvents.Services;

public interface ICategoryService
{
    Task<List<EventCategory>> GetAllAsync();
    Task<EventCategory?> GetByIdAsync(int id);
    Task<EventCategory> CreateAsync(EventCategory category);
    Task UpdateAsync(EventCategory category);
    Task<bool> DeleteAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<bool> HasEventsAsync(int id);
}
