using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class RegistrationService(SparkEventsDbContext db, ILogger<RegistrationService> logger) : IRegistrationService
{
    public async Task<Registration> RegisterAsync(int eventId, int attendeeId, int ticketTypeId, string? specialRequests)
    {
        var evt = await db.Events
            .Include(e => e.TicketTypes)
            .FirstOrDefaultAsync(e => e.Id == eventId)
            ?? throw new InvalidOperationException("Event not found.");

        var now = DateTime.UtcNow;

        // Check registration window
        if (now < evt.RegistrationOpenDate || now > evt.RegistrationCloseDate)
        {
            throw new InvalidOperationException("Registration is not currently open for this event.");
        }

        // Check for duplicate registration
        var existingReg = await db.Registrations
            .AnyAsync(r => r.EventId == eventId && r.AttendeeId == attendeeId && r.Status != RegistrationStatus.Cancelled);

        if (existingReg)
        {
            throw new InvalidOperationException("This attendee is already registered for this event.");
        }

        var ticketType = evt.TicketTypes.FirstOrDefault(t => t.Id == ticketTypeId && t.IsActive)
            ?? throw new InvalidOperationException("Selected ticket type is not available.");

        // Check ticket type capacity
        if (ticketType.QuantitySold >= ticketType.Quantity)
        {
            throw new InvalidOperationException($"The '{ticketType.Name}' ticket type is sold out. Please choose a different ticket type.");
        }

        var confirmationNumber = await GenerateConfirmationNumberAsync(evt);
        var price = CalculatePrice(ticketType, evt);

        // Determine if going to waitlist
        var isWaitlisted = evt.CurrentRegistrations >= evt.TotalCapacity;

        var registration = new Registration
        {
            EventId = eventId,
            AttendeeId = attendeeId,
            TicketTypeId = ticketTypeId,
            ConfirmationNumber = confirmationNumber,
            Status = isWaitlisted ? RegistrationStatus.Waitlisted : RegistrationStatus.Confirmed,
            AmountPaid = isWaitlisted ? 0m : price,
            SpecialRequests = specialRequests,
            RegistrationDate = now,
        };

        if (isWaitlisted)
        {
            var maxPosition = await db.Registrations
                .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
                .MaxAsync(r => (int?)r.WaitlistPosition) ?? 0;
            registration.WaitlistPosition = maxPosition + 1;
            evt.WaitlistCount++;
        }
        else
        {
            evt.CurrentRegistrations++;
            ticketType.QuantitySold++;

            if (evt.CurrentRegistrations >= evt.TotalCapacity)
            {
                evt.Status = EventStatus.SoldOut;
            }
        }

        db.Registrations.Add(registration);
        await db.SaveChangesAsync();

        logger.LogInformation("Registration created: {ConfirmationNumber} for event {EventId}, attendee {AttendeeId}, status {Status}",
            confirmationNumber, eventId, attendeeId, registration.Status);

        return registration;
    }

    public async Task CancelRegistrationAsync(int registrationId, string? reason)
    {
        var registration = await db.Registrations
            .Include(r => r.Event)
            .ThenInclude(e => e.TicketTypes)
            .Include(r => r.TicketType)
            .FirstOrDefaultAsync(r => r.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found.");

        if (registration.Status is RegistrationStatus.Cancelled or RegistrationStatus.CheckedIn)
        {
            throw new InvalidOperationException("This registration cannot be cancelled.");
        }

        var evt = registration.Event;

        // Check 24-hour cancellation policy
        if (registration.Status != RegistrationStatus.Waitlisted)
        {
            var hoursUntilEvent = (evt.StartDate - DateTime.UtcNow).TotalHours;
            if (hoursUntilEvent < 24)
            {
                throw new InvalidOperationException("Cancellations are not allowed within 24 hours of the event start time.");
            }
        }

        var wasConfirmed = registration.Status == RegistrationStatus.Confirmed;

        registration.Status = RegistrationStatus.Cancelled;
        registration.CancellationDate = DateTime.UtcNow;
        registration.CancellationReason = reason;

        if (wasConfirmed)
        {
            evt.CurrentRegistrations--;
            registration.TicketType.QuantitySold--;

            // Promote first waitlisted registration
            var nextWaitlisted = await db.Registrations
                .Include(r => r.TicketType)
                .Where(r => r.EventId == evt.Id && r.Status == RegistrationStatus.Waitlisted)
                .OrderBy(r => r.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = RegistrationStatus.Confirmed;
                nextWaitlisted.AmountPaid = CalculatePrice(nextWaitlisted.TicketType, evt);
                nextWaitlisted.WaitlistPosition = null;
                evt.CurrentRegistrations++;
                evt.WaitlistCount--;
                nextWaitlisted.TicketType.QuantitySold++;

                logger.LogInformation("Waitlist promotion: {ConfirmationNumber} promoted to confirmed for event {EventId}",
                    nextWaitlisted.ConfirmationNumber, evt.Id);
            }

            if (evt.Status == EventStatus.SoldOut && evt.CurrentRegistrations < evt.TotalCapacity)
            {
                evt.Status = EventStatus.Published;
            }
        }
        else if (registration.Status == RegistrationStatus.Cancelled && registration.WaitlistPosition.HasValue)
        {
            evt.WaitlistCount--;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Registration cancelled: {ConfirmationNumber}", registration.ConfirmationNumber);
    }

    public async Task<Registration?> GetRegistrationByIdAsync(int id)
    {
        return await db.Registrations
            .Include(r => r.Event)
            .ThenInclude(e => e.Venue)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<PaginatedList<Registration>> GetEventRosterAsync(int eventId, string? search, int pageNumber, int pageSize)
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
                r.Attendee.Email.Contains(search) ||
                r.ConfirmationNumber.Contains(search));
        }

        query = query.OrderBy(r => r.Attendee.LastName).ThenBy(r => r.Attendee.FirstName);

        var count = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<Registration>(items, count, pageNumber, pageSize);
    }

    public async Task<List<Registration>> GetEventWaitlistAsync(int eventId)
    {
        return await db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .AsNoTracking()
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
            .OrderBy(r => r.WaitlistPosition)
            .ToListAsync();
    }

    public async Task<string> GenerateConfirmationNumberAsync(Event evt)
    {
        var maxNum = await db.Registrations
            .Where(r => r.EventId == evt.Id)
            .CountAsync();

        return $"SPK-{evt.StartDate:yyyyMMdd}-{(maxNum + 1):D4}";
    }

    public decimal CalculatePrice(TicketType ticketType, Event evt)
    {
        if (ticketType.EarlyBirdPrice.HasValue && evt.EarlyBirdDeadline.HasValue && DateTime.UtcNow <= evt.EarlyBirdDeadline.Value)
        {
            return ticketType.EarlyBirdPrice.Value;
        }

        return ticketType.Price;
    }
}
