using SparkEvents.Models;

namespace SparkEvents.Services;

public interface ITicketTypeService
{
    Task<List<TicketType>> GetByEventIdAsync(int eventId);
    Task<TicketType?> GetByIdAsync(int id);
    Task<TicketType> CreateAsync(TicketType ticketType);
    Task UpdateAsync(TicketType ticketType);
    Task DeactivateAsync(int id);
}
