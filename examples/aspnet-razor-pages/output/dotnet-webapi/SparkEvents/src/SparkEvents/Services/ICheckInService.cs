using SparkEvents.Models;

namespace SparkEvents.Services;

public interface ICheckInService
{
    Task<CheckIn> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes, CancellationToken ct = default);
    Task<int> GetCheckInCountAsync(int eventId, CancellationToken ct = default);
    Task<int> GetTotalConfirmedCountAsync(int eventId, CancellationToken ct = default);
}
