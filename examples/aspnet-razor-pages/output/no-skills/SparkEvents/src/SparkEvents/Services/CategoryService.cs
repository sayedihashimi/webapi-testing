using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class CategoryService : ICategoryService
{
    private readonly SparkEventsDbContext _context;

    public CategoryService(SparkEventsDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventCategory>> GetAllCategoriesAsync()
    {
        return await _context.EventCategories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<EventCategory?> GetCategoryByIdAsync(int id)
    {
        return await _context.EventCategories.FindAsync(id);
    }

    public async Task<EventCategory> CreateCategoryAsync(EventCategory category)
    {
        _context.EventCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(EventCategory category)
    {
        _context.EventCategories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.EventCategories.FindAsync(id);
        if (category == null) return false;
        if (await HasEventsAsync(id)) return false;
        _context.EventCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasEventsAsync(int id)
    {
        return await _context.Events.AnyAsync(e => e.EventCategoryId == id);
    }
}
