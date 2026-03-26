using SparkEvents.Models;

namespace SparkEvents.Services;

public interface ITicketTypeService
{
    Task<List<TicketType>> GetByEventIdAsync(int eventId, CancellationToken ct = default);
    Task<TicketType?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TicketType> CreateAsync(TicketType ticketType, CancellationToken ct = default);
    Task UpdateAsync(TicketType ticketType, CancellationToken ct = default);
    Task<bool> ToggleActiveAsync(int id, CancellationToken ct = default);
}
