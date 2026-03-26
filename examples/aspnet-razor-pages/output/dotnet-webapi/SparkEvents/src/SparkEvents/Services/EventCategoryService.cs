using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class EventCategoryService(SparkEventsDbContext db, ILogger<EventCategoryService> logger)
    : IEventCategoryService
{
    public async Task<PaginatedList<EventCategory>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.EventCategories.AsNoTracking().OrderBy(c => c.Name);
        return await PaginatedList<EventCategory>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<List<EventCategory>> GetAllForDropdownAsync(CancellationToken ct = default)
    {
        return await db.EventCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<EventCategory?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.EventCategories.FindAsync([id], ct);
    }

    public async Task<EventCategory> CreateAsync(EventCategory category, CancellationToken ct = default)
    {
        db.EventCategories.Add(category);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created event category {CategoryName} (ID: {CategoryId})", category.Name, category.Id);
        return category;
    }

    public async Task UpdateAsync(EventCategory category, CancellationToken ct = default)
    {
        db.EventCategories.Update(category);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated event category {CategoryId}", category.Id);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await db.EventCategories.FindAsync([id], ct);
        if (category is null) return false;

        db.EventCategories.Remove(category);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted event category {CategoryId}", id);
        return true;
    }

    public async Task<bool> HasEventsAsync(int id, CancellationToken ct = default)
    {
        return await db.Events.AnyAsync(e => e.EventCategoryId == id, ct);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var query = db.EventCategories.Where(c => c.Name == name);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }
}
