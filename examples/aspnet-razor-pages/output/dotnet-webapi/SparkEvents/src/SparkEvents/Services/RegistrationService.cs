using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class RegistrationService(SparkEventsDbContext db, ILogger<RegistrationService> logger)
    : IRegistrationService
{
    public async Task<Registration> RegisterAsync(int eventId, int attendeeId, int ticketTypeId,
        string? specialRequests, CancellationToken ct = default)
    {
        var evt = await db.Events
            .Include(e => e.TicketTypes)
            .FirstOrDefaultAsync(e => e.Id == eventId, ct)
            ?? throw new KeyNotFoundException($"Event with ID {eventId} not found.");

        var ticketType = evt.TicketTypes.FirstOrDefault(t => t.Id == ticketTypeId)
            ?? throw new KeyNotFoundException($"Ticket type with ID {ticketTypeId} not found for this event.");

        // Validate registration window
        var now = DateTime.UtcNow;
        if (now < evt.RegistrationOpenDate)
            throw new InvalidOperationException("Registration has not opened yet.");
        if (now > evt.RegistrationCloseDate)
            throw new InvalidOperationException("Registration has closed.");

        // Check for duplicate registration
        var hasActive = await db.Registrations.AnyAsync(r =>
            r.AttendeeId == attendeeId && r.EventId == eventId && r.Status != RegistrationStatus.Cancelled, ct);
        if (hasActive)
            throw new InvalidOperationException("This attendee is already registered for this event.");

        // Check ticket type capacity
        if (!ticketType.IsActive)
            throw new InvalidOperationException("This ticket type is no longer available.");
        if (ticketType.QuantitySold >= ticketType.Quantity)
            throw new InvalidOperationException($"Ticket type '{ticketType.Name}' is sold out. Please choose a different ticket type.");

        // Determine price
        decimal price;
        if (evt.EarlyBirdDeadline.HasValue && now < evt.EarlyBirdDeadline.Value && ticketType.EarlyBirdPrice.HasValue)
            price = ticketType.EarlyBirdPrice.Value;
        else
            price = ticketType.Price;

        // Determine status
        var isWaitlisted = evt.CurrentRegistrations >= evt.TotalCapacity;

        // Generate confirmation number
        var confirmationNumber = await GenerateConfirmationNumberAsync(evt, ct);

        var registration = new Registration
        {
            EventId = eventId,
            AttendeeId = attendeeId,
            TicketTypeId = ticketTypeId,
            ConfirmationNumber = confirmationNumber,
            Status = isWaitlisted ? RegistrationStatus.Waitlisted : RegistrationStatus.Confirmed,
            AmountPaid = price,
            SpecialRequests = specialRequests,
            RegistrationDate = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (isWaitlisted)
        {
            registration.WaitlistPosition = evt.WaitlistCount + 1;
            evt.WaitlistCount++;
        }
        else
        {
            evt.CurrentRegistrations++;
            ticketType.QuantitySold++;

            // Check if event is now sold out
            if (evt.CurrentRegistrations >= evt.TotalCapacity)
                evt.Status = EventStatus.SoldOut;
        }

        db.Registrations.Add(registration);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Registration created: {ConfirmationNumber} for event {EventId}, attendee {AttendeeId}, status {Status}",
            confirmationNumber, eventId, attendeeId, registration.Status);

        return registration;
    }

    public async Task<Registration?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Registrations.FindAsync([id], ct);
    }

    public async Task<Registration?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
    {
        return await db.Registrations
            .Include(r => r.Event).ThenInclude(e => e.EventCategory)
            .Include(r => r.Event).ThenInclude(e => e.Venue)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<bool> CancelRegistrationAsync(int id, string? reason, CancellationToken ct = default)
    {
        var registration = await db.Registrations
            .Include(r => r.Event).ThenInclude(e => e.Registrations)
            .Include(r => r.TicketType)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (registration is null) return false;

        if (registration.Status == RegistrationStatus.Cancelled)
            throw new InvalidOperationException("Registration is already cancelled.");

        if (registration.Status == RegistrationStatus.CheckedIn)
            throw new InvalidOperationException("Cannot cancel a checked-in registration.");

        // Check 24-hour cancellation policy
        var hoursUntilEvent = (registration.Event.StartDate - DateTime.UtcNow).TotalHours;
        if (hoursUntilEvent < 24)
            throw new InvalidOperationException("Cancellations are not allowed within 24 hours of the event start.");

        var wasConfirmed = registration.Status == RegistrationStatus.Confirmed;

        registration.Status = RegistrationStatus.Cancelled;
        registration.CancellationDate = DateTime.UtcNow;
        registration.CancellationReason = reason;
        registration.UpdatedAt = DateTime.UtcNow;

        if (wasConfirmed)
        {
            registration.Event.CurrentRegistrations--;
            registration.TicketType.QuantitySold--;

            // Promote first waitlisted registration
            var nextWaitlisted = await db.Registrations
                .Where(r => r.EventId == registration.EventId && r.Status == RegistrationStatus.Waitlisted)
                .OrderBy(r => r.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = RegistrationStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = DateTime.UtcNow;
                registration.Event.CurrentRegistrations++;
                registration.Event.WaitlistCount--;

                // Update the promoted registration's ticket type sold count
                var promotedTicketType = await db.TicketTypes.FindAsync([nextWaitlisted.TicketTypeId], ct);
                if (promotedTicketType is not null)
                    promotedTicketType.QuantitySold++;

                // Reorder remaining waitlist positions
                var remainingWaitlisted = await db.Registrations
                    .Where(r => r.EventId == registration.EventId && r.Status == RegistrationStatus.Waitlisted)
                    .OrderBy(r => r.WaitlistPosition)
                    .ToListAsync(ct);

                for (int i = 0; i < remainingWaitlisted.Count; i++)
                    remainingWaitlisted[i].WaitlistPosition = i + 1;

                logger.LogInformation("Promoted waitlisted registration {RegistrationId} to confirmed for event {EventId}",
                    nextWaitlisted.Id, registration.EventId);
            }

            // Check if event should transition from SoldOut to Published
            if (registration.Event.Status == EventStatus.SoldOut &&
                registration.Event.CurrentRegistrations < registration.Event.TotalCapacity)
            {
                registration.Event.Status = EventStatus.Published;
            }
        }
        else if (registration.Status == RegistrationStatus.Waitlisted)
        {
            registration.Event.WaitlistCount--;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled registration {RegistrationId} for event {EventId}", id, registration.EventId);
        return true;
    }

    public async Task<PaginatedList<Registration>> GetEventRosterAsync(int eventId, string? search,
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .AsNoTracking()
            .Where(r => r.EventId == eventId && r.Status != RegistrationStatus.Cancelled && r.Status != RegistrationStatus.Waitlisted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                r.Attendee.FirstName.Contains(search) ||
                r.Attendee.LastName.Contains(search) ||
                r.ConfirmationNumber.Contains(search));
        }

        query = query.OrderBy(r => r.Attendee.LastName).ThenBy(r => r.Attendee.FirstName);
        return await PaginatedList<Registration>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<List<Registration>> GetEventWaitlistAsync(int eventId, CancellationToken ct = default)
    {
        return await db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .AsNoTracking()
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
            .OrderBy(r => r.WaitlistPosition)
            .ToListAsync(ct);
    }

    public async Task<List<Registration>> GetRecentRegistrationsAsync(int count, CancellationToken ct = default)
    {
        return await db.Registrations
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .AsNoTracking()
            .OrderByDescending(r => r.RegistrationDate)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalRegistrationsCountAsync(CancellationToken ct = default)
    {
        return await db.Registrations.CountAsync(r => r.Status != RegistrationStatus.Cancelled, ct);
    }

    public async Task<bool> HasActiveRegistrationAsync(int attendeeId, int eventId, CancellationToken ct = default)
    {
        return await db.Registrations.AnyAsync(r =>
            r.AttendeeId == attendeeId && r.EventId == eventId && r.Status != RegistrationStatus.Cancelled, ct);
    }

    public async Task<Registration?> GetByConfirmationNumberAsync(string confirmationNumber, CancellationToken ct = default)
    {
        return await db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.Event)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.ConfirmationNumber == confirmationNumber, ct);
    }

    public async Task<List<Registration>> SearchForCheckInAsync(int eventId, string search, CancellationToken ct = default)
    {
        return await db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .Where(r => r.EventId == eventId &&
                (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn) &&
                (r.Attendee.FirstName.Contains(search) ||
                 r.Attendee.LastName.Contains(search) ||
                 r.ConfirmationNumber.Contains(search)))
            .OrderBy(r => r.Attendee.LastName)
            .ToListAsync(ct);
    }

    private async Task<string> GenerateConfirmationNumberAsync(Event evt, CancellationToken ct)
    {
        var dateStr = evt.StartDate.ToString("yyyyMMdd");
        var count = await db.Registrations.CountAsync(r => r.EventId == evt.Id, ct);
        return $"SPK-{dateStr}-{(count + 1):D4}";
    }
}
