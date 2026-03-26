using SparkEvents.Models;

namespace SparkEvents.Services;

public interface IRegistrationService
{
    Task<Registration> RegisterAsync(int eventId, int attendeeId, int ticketTypeId, string? specialRequests, CancellationToken ct = default);
    Task<Registration?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Registration?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<bool> CancelRegistrationAsync(int id, string? reason, CancellationToken ct = default);
    Task<PaginatedList<Registration>> GetEventRosterAsync(int eventId, string? search, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<List<Registration>> GetEventWaitlistAsync(int eventId, CancellationToken ct = default);
    Task<List<Registration>> GetRecentRegistrationsAsync(int count, CancellationToken ct = default);
    Task<int> GetTotalRegistrationsCountAsync(CancellationToken ct = default);
    Task<bool> HasActiveRegistrationAsync(int attendeeId, int eventId, CancellationToken ct = default);
    Task<Registration?> GetByConfirmationNumberAsync(string confirmationNumber, CancellationToken ct = default);
    Task<List<Registration>> SearchForCheckInAsync(int eventId, string search, CancellationToken ct = default);
}
