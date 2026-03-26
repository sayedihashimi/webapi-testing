using SparkEvents.Models;

namespace SparkEvents.Services;

public interface ICheckInService
{
    Task<CheckIn> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes);
    Task<List<Registration>> SearchForCheckInAsync(int eventId, string searchTerm);
    Task<(int CheckedIn, int Total)> GetCheckInStatsAsync(int eventId);
    Task<bool> CanCheckInAsync(int eventId);
}
