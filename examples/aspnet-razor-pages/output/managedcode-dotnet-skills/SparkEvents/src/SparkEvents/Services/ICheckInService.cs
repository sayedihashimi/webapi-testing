using SparkEvents.Models;

namespace SparkEvents.Services;

public interface ICheckInService
{
    Task<CheckIn> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes);
    Task<Registration?> LookupForCheckInAsync(int eventId, string searchTerm);
    Task<(int TotalConfirmed, int CheckedIn)> GetCheckInStatsAsync(int eventId);
}
