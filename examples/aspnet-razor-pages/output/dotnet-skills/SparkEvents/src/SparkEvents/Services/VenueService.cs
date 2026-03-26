using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class VenueService : IVenueService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<VenueService> _logger;

    public VenueService(SparkEventsDbContext db, ILogger<VenueService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedList<Venue>> GetVenuesAsync(int pageNumber = 1, int pageSize = 10)
    {
        var query = _db.Venues.OrderBy(v => v.Name);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<Venue>(items, count, pageNumber, pageSize);
    }

    public async Task<Venue?> GetVenueByIdAsync(int id) =>
        await _db.Venues
            .AsNoTracking()
            .Include(v => v.Events.Where(e => e.StartDate >= DateTime.UtcNow).OrderBy(e => e.StartDate))
            .ThenInclude(e => e.EventCategory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<Venue> CreateVenueAsync(Venue venue)
    {
        venue.CreatedAt = DateTime.UtcNow;
        _db.Venues.Add(venue);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created venue: {Name}", venue.Name);
        return venue;
    }

    public async Task UpdateVenueAsync(Venue venue)
    {
        _db.Venues.Update(venue);
        await _db.SaveChangesAsync();
    }

    public async Task<(bool Success, string? Error)> DeleteVenueAsync(int id)
    {
        var venue = await _db.Venues.Include(v => v.Events).FirstOrDefaultAsync(v => v.Id == id);
        if (venue == null) return (false, "Venue not found.");
        if (venue.Events.Any(e => e.StartDate >= DateTime.UtcNow))
            return (false, "Cannot delete a venue that has future events.");
        _db.Venues.Remove(venue);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted venue: {Name}", venue.Name);
        return (true, null);
    }

    public async Task<List<Venue>> GetAllVenuesAsync() =>
        await _db.Venues.OrderBy(v => v.Name).ToListAsync();
}
