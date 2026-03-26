using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class TicketTypeService(SparkEventsDbContext db, ILogger<TicketTypeService> logger)
    : ITicketTypeService
{
    public async Task<List<TicketType>> GetByEventIdAsync(int eventId, CancellationToken ct = default)
    {
        return await db.TicketTypes
            .AsNoTracking()
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<TicketType?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.TicketTypes
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<TicketType> CreateAsync(TicketType ticketType, CancellationToken ct = default)
    {
        ticketType.CreatedAt = DateTime.UtcNow;
        db.TicketTypes.Add(ticketType);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created ticket type {TicketTypeName} for event {EventId}", ticketType.Name, ticketType.EventId);
        return ticketType;
    }

    public async Task UpdateAsync(TicketType ticketType, CancellationToken ct = default)
    {
        db.TicketTypes.Update(ticketType);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated ticket type {TicketTypeId}", ticketType.Id);
    }

    public async Task<bool> ToggleActiveAsync(int id, CancellationToken ct = default)
    {
        var ticketType = await db.TicketTypes.FindAsync([id], ct);
        if (ticketType is null) return false;

        ticketType.IsActive = !ticketType.IsActive;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Toggled ticket type {TicketTypeId} active status to {IsActive}", id, ticketType.IsActive);
        return true;
    }
}
