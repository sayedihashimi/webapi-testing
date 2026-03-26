using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IVenueService
{
    Task<List<Venue>> GetAllVenuesAsync();
    Task<Venue?> GetVenueByIdAsync(int id);
    Task<Venue> CreateVenueAsync(Venue venue);
    Task UpdateVenueAsync(Venue venue);
    Task<bool> DeleteVenueAsync(int id);
    Task<bool> HasFutureEventsAsync(int id);
    Task<List<Event>> GetUpcomingEventsForVenueAsync(int venueId);
}
