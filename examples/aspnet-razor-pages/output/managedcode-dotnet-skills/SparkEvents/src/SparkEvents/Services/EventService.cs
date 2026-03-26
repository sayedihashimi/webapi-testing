using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class EventService(SparkEventsDbContext context, ILogger<EventService> logger) : IEventService
{
    public async Task<(List<Event> Items, int TotalCount)> GetFilteredAsync(
        string? search, int? categoryId, EventStatus? status,
        DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = context.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(term) ||
                e.Description.ToLower().Contains(term));
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
            query = query.Where(e => e.EndDate <= toDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.IsFeatured)
            .ThenBy(e => e.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await context.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes.OrderBy(t => t.SortOrder))
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event?> GetByIdWithRegistrationsAsync(int id)
    {
        return await context.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes.OrderBy(t => t.SortOrder))
            .Include(e => e.Registrations)
                .ThenInclude(r => r.Attendee)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event> CreateAsync(Event event_)
    {
        var venue = await context.Venues
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == event_.VenueId)
            ?? throw new InvalidOperationException($"Venue with Id {event_.VenueId} not found.");

        if (event_.TotalCapacity > venue.MaxCapacity)
        {
            throw new InvalidOperationException(
                $"Event capacity ({event_.TotalCapacity}) exceeds venue max capacity ({venue.MaxCapacity}).");
        }

        event_.CreatedAt = DateTime.UtcNow;
        event_.UpdatedAt = DateTime.UtcNow;

        context.Events.Add(event_);
        await context.SaveChangesAsync();

        logger.LogInformation("Created event '{Title}' with Id {Id}", event_.Title, event_.Id);
        return event_;
    }

    public async Task UpdateAsync(Event event_)
    {
        var currentRegistrations = await context.Events
            .AsNoTracking()
            .Where(e => e.Id == event_.Id)
            .Select(e => e.CurrentRegistrations)
            .FirstOrDefaultAsync();

        if (event_.TotalCapacity < currentRegistrations)
        {
            throw new InvalidOperationException(
                $"Cannot reduce capacity to {event_.TotalCapacity} because there are already {currentRegistrations} registration(s).");
        }

        event_.UpdatedAt = DateTime.UtcNow;

        context.Events.Update(event_);
        await context.SaveChangesAsync();

        logger.LogInformation("Updated event '{Title}' (Id: {Id})", event_.Title, event_.Id);
    }

    public async Task PublishAsync(int id)
    {
        var event_ = await context.Events
            .Include(e => e.TicketTypes)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new InvalidOperationException($"Event with Id {id} not found.");

        if (event_.Status != EventStatus.Draft)
        {
            throw new InvalidOperationException(
                $"Only Draft events can be published. Current status: {event_.Status}.");
        }

        var hasActiveTickets = event_.TicketTypes.Any(t => t.IsActive);
        if (!hasActiveTickets)
        {
            throw new InvalidOperationException("Cannot publish an event without active ticket types.");
        }

        event_.Status = EventStatus.Published;
        event_.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Published event '{Title}' (Id: {Id})", event_.Title, id);
    }

    public async Task CancelAsync(int id, string reason)
    {
        var event_ = await context.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new InvalidOperationException($"Event with Id {id} not found.");

        event_.Status = EventStatus.Cancelled;
        event_.CancellationReason = reason;
        event_.UpdatedAt = DateTime.UtcNow;

        foreach (var registration in event_.Registrations
            .Where(r => r.Status is RegistrationStatus.Confirmed or RegistrationStatus.Waitlisted))
        {
            registration.Status = RegistrationStatus.Cancelled;
            registration.CancellationDate = DateTime.UtcNow;
            registration.CancellationReason = $"Event cancelled: {reason}";
            registration.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Cancelled event '{Title}' (Id: {Id}). Reason: {Reason}",
            event_.Title, id, reason);
    }

    public async Task CompleteAsync(int id)
    {
        var event_ = await context.Events
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new InvalidOperationException($"Event with Id {id} not found.");

        if (event_.EndDate > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot complete an event before its end date.");
        }

        event_.Status = EventStatus.Completed;
        event_.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        logger.LogInformation("Completed event '{Title}' (Id: {Id})", event_.Title, id);
    }

    public async Task<List<Event>> GetUpcomingEventsAsync(int days = 7)
    {
        var cutoff = DateTime.UtcNow.AddDays(days);

        return await context.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Where(e => e.StartDate >= DateTime.UtcNow && e.StartDate <= cutoff
                && e.Status == EventStatus.Published)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<List<Event>> GetTodayEventsAsync()
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        return await context.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .Where(e => e.StartDate >= todayStart && e.StartDate < todayEnd
                && e.Status != EventStatus.Draft && e.Status != EventStatus.Cancelled)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<(int TotalEvents, int TotalRegistrations, int EventsThisMonth)> GetStatsAsync()
    {
        var totalEvents = await context.Events.AsNoTracking().CountAsync();

        var totalRegistrations = await context.Registrations
            .AsNoTracking()
            .CountAsync(r => r.Status == RegistrationStatus.Confirmed
                || r.Status == RegistrationStatus.CheckedIn);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var eventsThisMonth = await context.Events
            .AsNoTracking()
            .CountAsync(e => e.CreatedAt >= monthStart);

        return (totalEvents, totalRegistrations, eventsThisMonth);
    }

    public async Task<List<Event>> GetUpcomingByVenueAsync(int venueId)
    {
        return await context.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Where(e => e.VenueId == venueId && e.StartDate >= DateTime.UtcNow
                && e.Status != EventStatus.Cancelled)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }
}
