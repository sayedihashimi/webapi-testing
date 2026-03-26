using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IRegistrationService
{
    Task<Registration> RegisterAsync(int eventId, int attendeeId, int ticketTypeId, string? specialRequests);
    Task<Registration?> GetRegistrationByIdAsync(int id);
    Task<bool> CancelRegistrationAsync(int id, string? reason);
    Task<PaginatedList<Registration>> GetEventRosterAsync(int eventId, string? search, int pageIndex = 1, int pageSize = 10);
    Task<List<Registration>> GetEventWaitlistAsync(int eventId);
    Task<List<Registration>> GetRecentRegistrationsAsync(int count = 10);
    Task<int> GetTotalRegistrationsCountAsync();
    Task<bool> CanRegisterAsync(int eventId, int attendeeId);
    Task<string> GenerateConfirmationNumberAsync(int eventId);
}
