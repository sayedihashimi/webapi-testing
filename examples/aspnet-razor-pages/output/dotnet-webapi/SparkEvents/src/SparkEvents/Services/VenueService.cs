using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class VenueService(SparkEventsDbContext db, ILogger<VenueService> logger)
    : IVenueService
{
    public async Task<PaginatedList<Venue>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Venues.AsNoTracking().OrderBy(v => v.Name);
        return await PaginatedList<Venue>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<List<Venue>> GetAllForDropdownAsync(CancellationToken ct = default)
    {
        return await db.Venues.AsNoTracking().OrderBy(v => v.Name).ToListAsync(ct);
    }

    public async Task<Venue?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Venues.FindAsync([id], ct);
    }

    public async Task<Venue?> GetByIdWithEventsAsync(int id, CancellationToken ct = default)
    {
        return await db.Venues
            .Include(v => v.Events.Where(e => e.StartDate >= DateTime.UtcNow))
                .ThenInclude(e => e.EventCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<Venue> CreateAsync(Venue venue, CancellationToken ct = default)
    {
        venue.CreatedAt = DateTime.UtcNow;
        db.Venues.Add(venue);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created venue {VenueName} (ID: {VenueId})", venue.Name, venue.Id);
        return venue;
    }

    public async Task UpdateAsync(Venue venue, CancellationToken ct = default)
    {
        db.Venues.Update(venue);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated venue {VenueId}", venue.Id);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var venue = await db.Venues.FindAsync([id], ct);
        if (venue is null) return false;

        db.Venues.Remove(venue);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Deleted venue {VenueId}", id);
        return true;
    }

    public async Task<bool> HasFutureEventsAsync(int id, CancellationToken ct = default)
    {
        return await db.Events.AnyAsync(e => e.VenueId == id && e.StartDate >= DateTime.UtcNow, ct);
    }
}
