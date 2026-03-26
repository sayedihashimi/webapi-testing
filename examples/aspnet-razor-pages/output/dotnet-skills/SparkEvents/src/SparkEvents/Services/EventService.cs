using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class EventService : IEventService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<EventService> _logger;

    public EventService(SparkEventsDbContext db, ILogger<EventService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedList<Event>> GetEventsAsync(string? search, int? categoryId, EventStatus? status, DateTime? fromDate, DateTime? toDate, int pageNumber = 1, int pageSize = 10)
    {
        var query = _db.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(term));
        }

        if (categoryId.HasValue)
            query = query.Where(e => e.EventCategoryId == categoryId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(e => e.StartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.StartDate <= toDate.Value);

        query = query.OrderByDescending(e => e.IsFeatured).ThenBy(e => e.StartDate);

        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<Event>(items, count, pageNumber, pageSize);
    }

    public async Task<Event?> GetEventByIdAsync(int id) =>
        await _db.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes.OrderBy(t => t.SortOrder))
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Event> CreateEventAsync(Event evt)
    {
        evt.CreatedAt = DateTime.UtcNow;
        evt.UpdatedAt = DateTime.UtcNow;
        _db.Events.Add(evt);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created event: {Title}", evt.Title);
        return evt;
    }

    public async Task UpdateEventAsync(Event evt)
    {
        evt.UpdatedAt = DateTime.UtcNow;
        _db.Events.Update(evt);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> PublishEventAsync(int id)
    {
        var evt = await _db.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.Id == id);
        if (evt == null || evt.Status != EventStatus.Draft) return false;
        if (!evt.TicketTypes.Any(t => t.IsActive)) return false;

        evt.Status = EventStatus.Published;
        evt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Published event: {Title}", evt.Title);
        return true;
    }

    public async Task<bool> CancelEventAsync(int id, string reason)
    {
        var evt = await _db.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

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

        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled event: {Title}. Reason: {Reason}", evt.Title, reason);
        return true;
    }

    public async Task<bool> CompleteEventAsync(int id)
    {
        var evt = await _db.Events.FindAsync(id);
        if (evt == null) return false;
        if (evt.Status != EventStatus.Published && evt.Status != EventStatus.SoldOut) return false;
        if (evt.EndDate > DateTime.UtcNow) return false;

        evt.Status = EventStatus.Completed;
        evt.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Completed event: {Title}", evt.Title);
        return true;
    }

    public async Task<List<Event>> GetUpcomingEventsAsync(int days = 7)
    {
        var now = DateTime.UtcNow;
        var end = now.AddDays(days);
        return await _db.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Where(e => e.StartDate >= now && e.StartDate <= end && e.Status == EventStatus.Published)
            .OrderBy(e => e.StartDate)
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<Event>> GetTodaysEventsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return await _db.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Where(e => e.StartDate >= today && e.StartDate < tomorrow &&
                       (e.Status == EventStatus.Published || e.Status == EventStatus.SoldOut))
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<int> GetTotalEventsCountAsync() =>
        await _db.Events.CountAsync();

    public async Task<int> GetEventsThisMonthCountAsync()
    {
        var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var firstOfNextMonth = firstOfMonth.AddMonths(1);
        return await _db.Events.CountAsync(e => e.StartDate >= firstOfMonth && e.StartDate < firstOfNextMonth);
    }
}
