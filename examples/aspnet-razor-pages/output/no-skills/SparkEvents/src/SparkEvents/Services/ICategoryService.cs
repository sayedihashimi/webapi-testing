using SparkEvents.Models;

namespace SparkEvents.Services;

public interface ICategoryService
{
    Task<List<EventCategory>> GetAllCategoriesAsync();
    Task<EventCategory?> GetCategoryByIdAsync(int id);
    Task<EventCategory> CreateCategoryAsync(EventCategory category);
    Task UpdateCategoryAsync(EventCategory category);
    Task<bool> DeleteCategoryAsync(int id);
    Task<bool> HasEventsAsync(int id);
}
