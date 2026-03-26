using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class CategoryService : ICategoryService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(SparkEventsDbContext db, ILogger<CategoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedList<EventCategory>> GetCategoriesAsync(int pageNumber = 1, int pageSize = 10)
    {
        var query = _db.EventCategories.OrderBy(c => c.Name);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<EventCategory>(items, count, pageNumber, pageSize);
    }

    public async Task<EventCategory?> GetCategoryByIdAsync(int id) =>
        await _db.EventCategories.FindAsync(id);

    public async Task<EventCategory> CreateCategoryAsync(EventCategory category)
    {
        _db.EventCategories.Add(category);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created category: {Name}", category.Name);
        return category;
    }

    public async Task UpdateCategoryAsync(EventCategory category)
    {
        _db.EventCategories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> DeleteCategoryAsync(int id)
    {
        var category = await _db.EventCategories.Include(c => c.Events).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return (false, "Category not found.");
        if (category.Events.Any()) return (false, "Cannot delete a category that has events.");
        _db.EventCategories.Remove(category);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted category: {Name}", category.Name);
        return (true, null);
    }

    public async Task<List<EventCategory>> GetAllCategoriesAsync() =>
        await _db.EventCategories.OrderBy(c => c.Name).ToListAsync();
}
