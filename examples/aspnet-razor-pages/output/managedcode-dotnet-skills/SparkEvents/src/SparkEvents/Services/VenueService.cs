using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class VenueService(SparkEventsDbContext context, ILogger<VenueService> logger) : IVenueService
{
    public async Task<List<Venue>> GetAllAsync()
    {
        return await context.Venues
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<Venue?> GetByIdAsync(int id)
    {
        return await context.Venues
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Venue?> GetByIdWithEventsAsync(int id)
    {
        return await context.Venues
            .AsNoTracking()
            .Include(v => v.Events.Where(e => e.StartDate >= DateTime.UtcNow))
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Venue> CreateAsync(Venue venue)
    {
        context.Venues.Add(venue);
        await context.SaveChangesAsync();

        logger.LogInformation("Created venue '{Name}' with Id {Id}", venue.Name, venue.Id);
        return venue;
    }

    public async Task UpdateAsync(Venue venue)
    {
        context.Venues.Update(venue);
        await context.SaveChangesAsync();

        logger.LogInformation("Updated venue '{Name}' (Id: {Id})", venue.Name, venue.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hasFutureEvents = await HasFutureEventsAsync(id);
        if (hasFutureEvents)
        {
            logger.LogWarning("Cannot delete venue (Id: {Id}) because it has future events", id);
            return false;
        }

        var venue = await context.Venues.FindAsync(id);
        if (venue is null)
        {
            return false;
        }

        context.Venues.Remove(venue);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted venue '{Name}' (Id: {Id})", venue.Name, id);
        return true;
    }

    public async Task<bool> HasFutureEventsAsync(int venueId)
    {
        return await context.Events
            .AsNoTracking()
            .AnyAsync(e => e.VenueId == venueId && e.StartDate >= DateTime.UtcNow);
    }
}
