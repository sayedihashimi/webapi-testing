using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class RegistrationService : IRegistrationService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(SparkEventsDbContext db, ILogger<RegistrationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(Registration? Registration, string? Error)> RegisterAsync(
        int eventId, int attendeeId, int ticketTypeId, string? specialRequests)
    {
        var evt = await _db.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.Id == eventId);
        if (evt == null) return (null, "Event not found.");

        // Check registration window
        var now = DateTime.UtcNow;
        if (now < evt.RegistrationOpenDate)
            return (null, "Registration has not opened yet.");
        if (now > evt.RegistrationCloseDate)
            return (null, "Registration is closed.");

        // Check event status
        if (evt.Status != EventStatus.Published && evt.Status != EventStatus.SoldOut)
            return (null, "This event is not accepting registrations.");

        // Check duplicate registration
        var existingReg = await _db.Registrations
            .AnyAsync(r => r.EventId == eventId && r.AttendeeId == attendeeId &&
                          r.Status != RegistrationStatus.Cancelled);
        if (existingReg)
            return (null, "This attendee is already registered for this event.");

        var ticketType = evt.TicketTypes.FirstOrDefault(t => t.Id == ticketTypeId);
        if (ticketType == null || !ticketType.IsActive)
            return (null, "Invalid or inactive ticket type.");

        // Determine price
        decimal price;
        if (evt.EarlyBirdDeadline.HasValue && ticketType.EarlyBirdPrice.HasValue && now < evt.EarlyBirdDeadline.Value)
            price = ticketType.EarlyBirdPrice.Value;
        else
            price = ticketType.Price;

        // Generate confirmation number
        var confirmationNumber = await GenerateConfirmationNumberAsync(evt);

        var registration = new Registration
        {
            EventId = eventId,
            AttendeeId = attendeeId,
            TicketTypeId = ticketTypeId,
            ConfirmationNumber = confirmationNumber,
            AmountPaid = price,
            SpecialRequests = specialRequests,
            RegistrationDate = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Check ticket type capacity
        if (ticketType.QuantitySold >= ticketType.Quantity)
            return (null, $"The \"{ticketType.Name}\" ticket type is sold out. Please choose a different ticket type.");

        // Check overall event capacity
        if (evt.CurrentRegistrations >= evt.TotalCapacity)
        {
            // Waitlist
            var maxWaitlistPos = await _db.Registrations
                .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
                .MaxAsync(r => (int?)r.WaitlistPosition) ?? 0;

            registration.Status = RegistrationStatus.Waitlisted;
            registration.WaitlistPosition = maxWaitlistPos + 1;
            evt.WaitlistCount++;
        }
        else
        {
            registration.Status = RegistrationStatus.Confirmed;
            evt.CurrentRegistrations++;
            ticketType.QuantitySold++;

            if (evt.CurrentRegistrations >= evt.TotalCapacity)
                evt.Status = EventStatus.SoldOut;
        }

        evt.UpdatedAt = now;
        _db.Registrations.Add(registration);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Registration created: {ConfirmationNumber} for event {EventTitle}",
            confirmationNumber, evt.Title);

        return (registration, null);
    }

    public async Task<Registration?> GetRegistrationByIdAsync(int id) =>
        await _db.Registrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<(bool Success, string? Error)> CancelRegistrationAsync(int id, string? reason)
    {
        var registration = await _db.Registrations
            .Include(r => r.Event)
            .Include(r => r.TicketType)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (registration == null) return (false, "Registration not found.");

        if (registration.Status == RegistrationStatus.Cancelled)
            return (false, "Registration is already cancelled.");

        if (registration.Status == RegistrationStatus.CheckedIn)
            return (false, "Cannot cancel a checked-in registration.");

        // 24-hour cancellation policy
        if (registration.Event.StartDate.AddHours(-24) <= DateTime.UtcNow)
            return (false, "Cancellations are not allowed within 24 hours of the event start.");

        var wasConfirmed = registration.Status == RegistrationStatus.Confirmed;
        var wasWaitlisted = registration.Status == RegistrationStatus.Waitlisted;

        registration.Status = RegistrationStatus.Cancelled;
        registration.CancellationDate = DateTime.UtcNow;
        registration.CancellationReason = reason;
        registration.UpdatedAt = DateTime.UtcNow;

        if (wasConfirmed)
        {
            registration.Event.CurrentRegistrations--;
            registration.TicketType.QuantitySold--;

            // Promote first waitlisted
            var firstWaitlisted = await _db.Registrations
                .Include(r => r.TicketType)
                .Where(r => r.EventId == registration.EventId && r.Status == RegistrationStatus.Waitlisted)
                .OrderBy(r => r.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (firstWaitlisted != null)
            {
                firstWaitlisted.Status = RegistrationStatus.Confirmed;
                firstWaitlisted.WaitlistPosition = null;
                firstWaitlisted.UpdatedAt = DateTime.UtcNow;
                registration.Event.CurrentRegistrations++;
                registration.Event.WaitlistCount--;
                firstWaitlisted.TicketType.QuantitySold++;

                _logger.LogInformation("Waitlist promotion: {ConfirmationNumber} promoted to confirmed",
                    firstWaitlisted.ConfirmationNumber);
            }

            // Update event status
            if (registration.Event.Status == EventStatus.SoldOut &&
                registration.Event.CurrentRegistrations < registration.Event.TotalCapacity)
            {
                registration.Event.Status = EventStatus.Published;
            }
        }
        else if (wasWaitlisted)
        {
            registration.Event.WaitlistCount--;

            // Re-order waitlist positions
            var remainingWaitlisted = await _db.Registrations
                .Where(r => r.EventId == registration.EventId &&
                           r.Status == RegistrationStatus.Waitlisted &&
                           r.WaitlistPosition > registration.WaitlistPosition)
                .OrderBy(r => r.WaitlistPosition)
                .ToListAsync();

            foreach (var wr in remainingWaitlisted)
            {
                wr.WaitlistPosition--;
            }

            registration.WaitlistPosition = null;
        }

        registration.Event.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Registration cancelled: {ConfirmationNumber}", registration.ConfirmationNumber);
        return (true, null);
    }

    public async Task<PaginatedList<Registration>> GetEventRosterAsync(
        int eventId, string? search, int pageNumber = 1, int pageSize = 10)
    {
        var query = _db.Registrations
            .AsNoTracking()
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .AsSplitQuery()
            .Where(r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled && r.Status != RegistrationStatus.Waitlisted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(r =>
                r.Attendee.FirstName.ToLower().Contains(term) ||
                r.Attendee.LastName.ToLower().Contains(term) ||
                r.ConfirmationNumber.ToLower().Contains(term));
        }

        query = query.OrderBy(r => r.Attendee.LastName).ThenBy(r => r.Attendee.FirstName);
        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<Registration>(items, count, pageNumber, pageSize);
    }

    public async Task<List<Registration>> GetEventWaitlistAsync(int eventId) =>
        await _db.Registrations
            .AsNoTracking()
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
            .OrderBy(r => r.WaitlistPosition)
            .ToListAsync();

    public async Task<List<Registration>> GetRecentRegistrationsAsync(int count = 10) =>
        await _db.Registrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .AsSplitQuery()
            .OrderByDescending(r => r.RegistrationDate)
            .Take(count)
            .ToListAsync();

    public async Task<int> GetTotalRegistrationsCountAsync() =>
        await _db.Registrations.CountAsync(r => r.Status != RegistrationStatus.Cancelled);

    private async Task<string> GenerateConfirmationNumberAsync(Event evt)
    {
        var dateStr = evt.StartDate.ToString("yyyyMMdd");
        var prefix = $"SPK-{dateStr}-";

        var lastNumber = await _db.Registrations
            .Where(r => r.EventId == evt.Id)
            .CountAsync();

        return $"{prefix}{(lastNumber + 1):D4}";
    }
}
