using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class EventService : IEventService
{
    private readonly SparkEventsDbContext _context;
    private readonly ILogger<EventService> _logger;

    public EventService(SparkEventsDbContext context, ILogger<EventService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Event>> GetEventsAsync(string? search, int? categoryId, EventStatus? status, DateTime? startDate, DateTime? endDate, int pageIndex = 1, int pageSize = 10)
    {
        var query = _context.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Title.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(e => e.EventCategoryId == categoryId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (startDate.HasValue)
            query = query.Where(e => e.StartDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.StartDate <= endDate.Value);

        query = query.OrderByDescending(e => e.IsFeatured).ThenBy(e => e.StartDate);

        return await PaginatedList<Event>.CreateAsync(query, pageIndex, pageSize);
    }

    public async Task<Event?> GetEventByIdAsync(int id)
    {
        return await _context.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes.OrderBy(t => t.SortOrder))
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event> CreateEventAsync(Event evt)
    {
        evt.CreatedAt = DateTime.UtcNow;
        evt.UpdatedAt = DateTime.UtcNow;
        _context.Events.Add(evt);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Event created: {Title} (ID: {Id})", evt.Title, evt.Id);
        return evt;
    }

    public async Task UpdateEventAsync(Event evt)
    {
        evt.UpdatedAt = DateTime.UtcNow;
        _context.Events.Update(evt);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Event updated: {Title} (ID: {Id})", evt.Title, evt.Id);
    }

    public async Task<bool> PublishEventAsync(int id)
    {
        var evt = await _context.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.Id == id);
        if (evt == null || evt.Status != EventStatus.Draft) return false;
        if (!evt.TicketTypes.Any(t => t.IsActive)) return false;

        evt.Status = EventStatus.Published;
        evt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Event published: {Title} (ID: {Id})", evt.Title, evt.Id);
        return true;
    }

    public async Task<bool> CancelEventAsync(int id, string reason)
    {
        var evt = await _context.Events.Include(e => e.Registrations).FirstOrDefaultAsync(e => e.Id == id);
        if (evt == null) return false;
        if (evt.Status == EventStatus.Completed || evt.Status == EventStatus.Cancelled) return false;

        evt.Status = EventStatus.Cancelled;
        evt.CancellationReason = reason;
        evt.UpdatedAt = DateTime.UtcNow;

        // Cancel all non-cancelled registrations
        foreach (var reg in evt.Registrations.Where(r => r.Status != RegistrationStatus.Cancelled))
        {
            reg.Status = RegistrationStatus.Cancelled;
            reg.CancellationDate = DateTime.UtcNow;
            reg.CancellationReason = "Event cancelled by organizer";
            reg.UpdatedAt = DateTime.UtcNow;
        }

        evt.CurrentRegistrations = 0;
        evt.WaitlistCount = 0;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Event cancelled: {Title} (ID: {Id}), Reason: {Reason}", evt.Title, evt.Id, reason);
        return true;
    }

    public async Task<bool> CompleteEventAsync(int id)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (evt == null) return false;
        if (evt.Status != EventStatus.Published && evt.Status != EventStatus.SoldOut) return false;
        if (evt.EndDate > DateTime.UtcNow) return false;

        evt.Status = EventStatus.Completed;
        evt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Event completed: {Title} (ID: {Id})", evt.Title, evt.Id);
        return true;
    }

    public async Task<List<Event>> GetUpcomingEventsAsync(int days = 7)
    {
        var now = DateTime.UtcNow;
        var end = now.AddDays(days);
        return await _context.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Where(e => e.StartDate >= now && e.StartDate <= end && e.Status != EventStatus.Cancelled && e.Status != EventStatus.Draft)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<List<Event>> GetTodaysEventsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return await _context.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Where(e => e.StartDate >= today && e.StartDate < tomorrow && e.Status != EventStatus.Cancelled && e.Status != EventStatus.Draft)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<int> GetTotalEventsCountAsync()
    {
        return await _context.Events.CountAsync();
    }

    public async Task<int> GetEventsThisMonthCountAsync()
    {
        var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var firstOfNext = firstOfMonth.AddMonths(1);
        return await _context.Events.CountAsync(e => e.StartDate >= firstOfMonth && e.StartDate < firstOfNext);
    }
}
