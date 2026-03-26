using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class VenueService : IVenueService
{
    private readonly SparkEventsDbContext _context;

    public VenueService(SparkEventsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Venue>> GetAllVenuesAsync()
    {
        return await _context.Venues.OrderBy(v => v.Name).ToListAsync();
    }

    public async Task<Venue?> GetVenueByIdAsync(int id)
    {
        return await _context.Venues.FindAsync(id);
    }

    public async Task<Venue> CreateVenueAsync(Venue venue)
    {
        venue.CreatedAt = DateTime.UtcNow;
        _context.Venues.Add(venue);
        await _context.SaveChangesAsync();
        return venue;
    }

    public async Task UpdateVenueAsync(Venue venue)
    {
        _context.Venues.Update(venue);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteVenueAsync(int id)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue == null) return false;
        if (await HasFutureEventsAsync(id)) return false;
        _context.Venues.Remove(venue);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasFutureEventsAsync(int id)
    {
        return await _context.Events.AnyAsync(e => e.VenueId == id && e.StartDate > DateTime.UtcNow);
    }

    public async Task<List<Event>> GetUpcomingEventsForVenueAsync(int venueId)
    {
        return await _context.Events
            .Include(e => e.EventCategory)
            .Where(e => e.VenueId == venueId && e.StartDate > DateTime.UtcNow && e.Status != EventStatus.Cancelled)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }
}
