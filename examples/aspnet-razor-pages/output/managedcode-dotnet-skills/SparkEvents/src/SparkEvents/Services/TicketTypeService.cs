using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class TicketTypeService(SparkEventsDbContext context, ILogger<TicketTypeService> logger) : ITicketTypeService
{
    public async Task<List<TicketType>> GetByEventIdAsync(int eventId)
    {
        return await context.TicketTypes
            .AsNoTracking()
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<TicketType?> GetByIdAsync(int id)
    {
        return await context.TicketTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TicketType> CreateAsync(TicketType ticketType)
    {
        ticketType.CreatedAt = DateTime.UtcNow;

        context.TicketTypes.Add(ticketType);
        await context.SaveChangesAsync();

        logger.LogInformation("Created ticket type '{Name}' for Event {EventId} with Id {Id}",
            ticketType.Name, ticketType.EventId, ticketType.Id);
        return ticketType;
    }

    public async Task UpdateAsync(TicketType ticketType)
    {
        context.TicketTypes.Update(ticketType);
        await context.SaveChangesAsync();

        logger.LogInformation("Updated ticket type '{Name}' (Id: {Id})", ticketType.Name, ticketType.Id);
    }

    public async Task DeactivateAsync(int id)
    {
        var ticketType = await context.TicketTypes.FindAsync(id)
            ?? throw new InvalidOperationException($"Ticket type with Id {id} not found.");

        ticketType.IsActive = false;
        await context.SaveChangesAsync();

        logger.LogInformation("Deactivated ticket type '{Name}' (Id: {Id})", ticketType.Name, id);
    }
}
