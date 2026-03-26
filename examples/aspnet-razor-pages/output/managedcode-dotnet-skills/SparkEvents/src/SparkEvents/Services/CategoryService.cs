using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class CategoryService(SparkEventsDbContext context, ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<List<EventCategory>> GetAllAsync()
    {
        return await context.EventCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<EventCategory?> GetByIdAsync(int id)
    {
        return await context.EventCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<EventCategory> CreateAsync(EventCategory category)
    {
        context.EventCategories.Add(category);
        await context.SaveChangesAsync();

        logger.LogInformation("Created category '{Name}' with Id {Id}", category.Name, category.Id);
        return category;
    }

    public async Task UpdateAsync(EventCategory category)
    {
        context.EventCategories.Update(category);
        await context.SaveChangesAsync();

        logger.LogInformation("Updated category '{Name}' (Id: {Id})", category.Name, category.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await context.EventCategories
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            return false;
        }

        if (category.Events.Count > 0)
        {
            logger.LogWarning("Cannot delete category '{Name}' (Id: {Id}) because it has {Count} event(s)",
                category.Name, id, category.Events.Count);
            return false;
        }

        context.EventCategories.Remove(category);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted category '{Name}' (Id: {Id})", category.Name, id);
        return true;
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await context.EventCategories
            .AsNoTracking()
            .AnyAsync(c => c.Name == name && (!excludeId.HasValue || c.Id != excludeId.Value));
    }

    public async Task<bool> HasEventsAsync(int id)
    {
        return await context.Events
            .AsNoTracking()
            .AnyAsync(e => e.EventCategoryId == id);
    }
}
