using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class EventService(SparkEventsDbContext db, ILogger<EventService> logger) : IEventService
{
    public async Task<PaginatedList<Event>> GetEventsAsync(
        string? search, int? categoryId, EventStatus? status,
        DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
    {
        var query = db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Title.Contains(search));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(e => e.EventCategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.StartDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.StartDate <= toDate.Value);
        }

        query = query
            .OrderByDescending(e => e.IsFeatured)
            .ThenBy(e => e.StartDate);

        var count = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<Event>(items, count, pageNumber, pageSize);
    }

    public async Task<Event?> GetEventByIdAsync(int id)
    {
        return await db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes.OrderBy(t => t.SortOrder))
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateEventAsync(Event evt)
    {
        db.Events.Add(evt);
        await db.SaveChangesAsync();
        logger.LogInformation("Event created: {EventTitle} (ID: {EventId})", evt.Title, evt.Id);
    }

    public async Task UpdateEventAsync(Event evt)
    {
        db.Events.Update(evt);
        await db.SaveChangesAsync();
        logger.LogInformation("Event updated: {EventTitle} (ID: {EventId})", evt.Title, evt.Id);
    }

    public async Task CancelEventAsync(int eventId, string reason)
    {
        var evt = await db.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId)
            ?? throw new InvalidOperationException("Event not found.");

        if (evt.Status is EventStatus.Completed or EventStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot cancel an event that is already completed or cancelled.");
        }

        evt.Status = EventStatus.Cancelled;
        evt.CancellationReason = reason;

        var activeRegistrations = evt.Registrations
            .Where(r => r.Status is not RegistrationStatus.Cancelled)
            .ToList();

        foreach (var reg in activeRegistrations)
        {
            reg.Status = RegistrationStatus.Cancelled;
            reg.CancellationDate = DateTime.UtcNow;
            reg.CancellationReason = "Event cancelled by organizer";
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Event cancelled: {EventTitle} (ID: {EventId}). {Count} registrations cancelled.", evt.Title, evt.Id, activeRegistrations.Count);
    }

    public async Task CompleteEventAsync(int eventId)
    {
        var evt = await db.Events.FindAsync(eventId)
            ?? throw new InvalidOperationException("Event not found.");

        if (evt.Status is not (EventStatus.Published or EventStatus.SoldOut))
        {
            throw new InvalidOperationException("Only published or sold-out events can be completed.");
        }

        if (evt.EndDate > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot complete an event before its end date.");
        }

        evt.Status = EventStatus.Completed;
        await db.SaveChangesAsync();
        logger.LogInformation("Event completed: {EventTitle} (ID: {EventId})", evt.Title, evt.Id);
    }

    public async Task PublishEventAsync(int eventId)
    {
        var evt = await db.Events
            .Include(e => e.TicketTypes)
            .FirstOrDefaultAsync(e => e.Id == eventId)
            ?? throw new InvalidOperationException("Event not found.");

        if (evt.Status != EventStatus.Draft)
        {
            throw new InvalidOperationException("Only draft events can be published.");
        }

        if (!evt.TicketTypes.Any(t => t.IsActive))
        {
            throw new InvalidOperationException("Cannot publish an event without at least one active ticket type.");
        }

        evt.Status = EventStatus.Published;
        await db.SaveChangesAsync();
        logger.LogInformation("Event published: {EventTitle} (ID: {EventId})", evt.Title, evt.Id);
    }
}
