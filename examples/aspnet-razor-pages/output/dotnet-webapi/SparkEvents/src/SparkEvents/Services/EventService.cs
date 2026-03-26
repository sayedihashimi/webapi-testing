using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class EventService(SparkEventsDbContext db, ILogger<EventService> logger)
    : IEventService
{
    public async Task<PaginatedList<Event>> GetAllAsync(string? search, int? categoryId, EventStatus? status,
        DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.Title.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(e => e.EventCategoryId == categoryId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(e => e.StartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.StartDate <= toDate.Value);

        query = query.OrderByDescending(e => e.IsFeatured).ThenBy(e => e.StartDate);

        return await PaginatedList<Event>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<Event?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<Event?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
    {
        return await db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .Include(e => e.TicketTypes.OrderBy(t => t.SortOrder))
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<Event> CreateAsync(Event evt, CancellationToken ct = default)
    {
        evt.CreatedAt = DateTime.UtcNow;
        evt.UpdatedAt = DateTime.UtcNow;
        db.Events.Add(evt);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created event {EventTitle} (ID: {EventId})", evt.Title, evt.Id);
        return evt;
    }

    public async Task UpdateAsync(Event evt, CancellationToken ct = default)
    {
        evt.UpdatedAt = DateTime.UtcNow;
        db.Events.Update(evt);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated event {EventId}", evt.Id);
    }

    public async Task<bool> PublishAsync(int id, CancellationToken ct = default)
    {
        var evt = await db.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.Id == id, ct);
        if (evt is null) return false;

        if (evt.Status != EventStatus.Draft)
            throw new InvalidOperationException("Only draft events can be published.");

        if (!evt.TicketTypes.Any(t => t.IsActive))
            throw new InvalidOperationException("Event must have at least one active ticket type before publishing.");

        evt.Status = EventStatus.Published;
        evt.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Published event {EventId}", id);
        return true;
    }

    public async Task<bool> CancelAsync(int id, string reason, CancellationToken ct = default)
    {
        var evt = await db.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
        if (evt is null) return false;

        if (evt.Status == EventStatus.Completed || evt.Status == EventStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel an event that is already completed or cancelled.");

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

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled event {EventId} with reason: {Reason}", id, reason);
        return true;
    }

    public async Task<bool> CompleteAsync(int id, CancellationToken ct = default)
    {
        var evt = await db.Events.FindAsync([id], ct);
        if (evt is null) return false;

        if (evt.Status != EventStatus.Published && evt.Status != EventStatus.SoldOut)
            throw new InvalidOperationException("Only published or sold-out events can be completed.");

        if (evt.EndDate > DateTime.UtcNow)
            throw new InvalidOperationException("Cannot complete an event that hasn't ended yet.");

        evt.Status = EventStatus.Completed;
        evt.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Completed event {EventId}", id);
        return true;
    }

    public async Task<List<Event>> GetUpcomingEventsAsync(int days, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(days);
        return await db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .AsNoTracking()
            .Where(e => e.StartDate >= now && e.StartDate <= cutoff && e.Status != EventStatus.Cancelled)
            .OrderBy(e => e.StartDate)
            .ToListAsync(ct);
    }

    public async Task<List<Event>> GetTodaysEventsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        return await db.Events
            .Include(e => e.EventCategory)
            .Include(e => e.Venue)
            .AsNoTracking()
            .Where(e => e.StartDate >= today && e.StartDate < tomorrow && e.Status != EventStatus.Cancelled)
            .OrderBy(e => e.StartDate)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalEventsCountAsync(CancellationToken ct = default)
    {
        return await db.Events.CountAsync(ct);
    }

    public async Task<int> GetEventsThisMonthCountAsync(CancellationToken ct = default)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        return await db.Events.CountAsync(e => e.StartDate >= startOfMonth && e.StartDate < endOfMonth, ct);
    }
}
