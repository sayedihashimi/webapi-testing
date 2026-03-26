using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IRegistrationService
{
    Task<Registration> RegisterAsync(int eventId, int attendeeId, int ticketTypeId, string? specialRequests);
    Task<Registration?> GetByIdAsync(int id);
    Task<Registration?> GetByConfirmationNumberAsync(string confirmationNumber);
    Task CancelAsync(int id, string? reason);
    Task<(List<Registration> Items, int TotalCount)> GetEventRosterAsync(int eventId, string? search, int page, int pageSize);
    Task<List<Registration>> GetEventWaitlistAsync(int eventId);
    Task<List<Registration>> GetRecentRegistrationsAsync(int count = 10);
    Task<string> GenerateConfirmationNumberAsync(int eventId, DateTime eventStartDate);
}
