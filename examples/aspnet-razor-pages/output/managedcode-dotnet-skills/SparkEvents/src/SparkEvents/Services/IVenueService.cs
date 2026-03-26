using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IVenueService
{
    Task<List<Venue>> GetAllAsync();
    Task<Venue?> GetByIdAsync(int id);
    Task<Venue?> GetByIdWithEventsAsync(int id);
    Task<Venue> CreateAsync(Venue venue);
    Task UpdateAsync(Venue venue);
    Task<bool> DeleteAsync(int id);
    Task<bool> HasFutureEventsAsync(int venueId);
}
