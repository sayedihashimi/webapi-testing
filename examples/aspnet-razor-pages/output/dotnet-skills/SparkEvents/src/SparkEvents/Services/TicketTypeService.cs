using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class TicketTypeService : ITicketTypeService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<TicketTypeService> _logger;

    public TicketTypeService(SparkEventsDbContext db, ILogger<TicketTypeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<TicketType>> GetTicketTypesForEventAsync(int eventId) =>
        await _db.TicketTypes
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

    public async Task<TicketType?> GetTicketTypeByIdAsync(int id) =>
        await _db.TicketTypes.Include(t => t.Event).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<TicketType> CreateTicketTypeAsync(TicketType ticketType)
    {
        ticketType.CreatedAt = DateTime.UtcNow;
        _db.TicketTypes.Add(ticketType);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created ticket type: {Name} for event {EventId}", ticketType.Name, ticketType.EventId);
        return ticketType;
    }

    public async Task UpdateTicketTypeAsync(TicketType ticketType)
    {
        _db.TicketTypes.Update(ticketType);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var ticketType = await _db.TicketTypes.FindAsync(id);
        if (ticketType == null) return false;
        ticketType.IsActive = !ticketType.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }
}
