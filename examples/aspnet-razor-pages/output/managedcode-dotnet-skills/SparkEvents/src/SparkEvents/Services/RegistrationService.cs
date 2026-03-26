using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class RegistrationService(SparkEventsDbContext context, ILogger<RegistrationService> logger) : IRegistrationService
{
    public async Task<Registration> RegisterAsync(int eventId, int attendeeId, int ticketTypeId, string? specialRequests)
    {
        var event_ = await context.Events
            .Include(e => e.TicketTypes)
            .FirstOrDefaultAsync(e => e.Id == eventId)
            ?? throw new InvalidOperationException($"Event with Id {eventId} not found.");

        var now = DateTime.UtcNow;

        // Validate registration window
        if (now < event_.RegistrationOpenDate)
        {
            throw new InvalidOperationException("Registration is not yet open for this event.");
        }

        if (now > event_.RegistrationCloseDate)
        {
            throw new InvalidOperationException("Registration is closed for this event.");
        }

        // Check for duplicate non-cancelled registration
        var hasDuplicate = await context.Registrations
            .AnyAsync(r => r.EventId == eventId
                && r.AttendeeId == attendeeId
                && r.Status != RegistrationStatus.Cancelled);

        if (hasDuplicate)
        {
            throw new InvalidOperationException("Attendee is already registered for this event.");
        }

        var ticketType = event_.TicketTypes.FirstOrDefault(t => t.Id == ticketTypeId)
            ?? throw new InvalidOperationException($"Ticket type with Id {ticketTypeId} not found for this event.");

        // Check ticket type capacity
        if (ticketType.QuantitySold >= ticketType.Quantity)
        {
            throw new InvalidOperationException("This ticket type is sold out.");
        }

        // Determine price
        var price = ticketType.Price;
        if (event_.EarlyBirdDeadline.HasValue
            && now < event_.EarlyBirdDeadline.Value
            && ticketType.EarlyBirdPrice.HasValue)
        {
            price = ticketType.EarlyBirdPrice.Value;
        }

        // Determine registration status
        var eventHasCapacity = event_.CurrentRegistrations < event_.TotalCapacity;

        RegistrationStatus registrationStatus;
        int? waitlistPosition = null;

        if (eventHasCapacity)
        {
            registrationStatus = RegistrationStatus.Confirmed;
        }
        else
        {
            // Event capacity full but ticket type has room — waitlist
            registrationStatus = RegistrationStatus.Waitlisted;
            waitlistPosition = event_.WaitlistCount + 1;
        }

        var confirmationNumber = await GenerateConfirmationNumberAsync(eventId, event_.StartDate);

        var registration = new Registration
        {
            EventId = eventId,
            AttendeeId = attendeeId,
            TicketTypeId = ticketTypeId,
            ConfirmationNumber = confirmationNumber,
            Status = registrationStatus,
            AmountPaid = registrationStatus == RegistrationStatus.Confirmed ? price : 0m,
            WaitlistPosition = waitlistPosition,
            SpecialRequests = specialRequests,
            RegistrationDate = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Registrations.Add(registration);

        // Update ticket type sold count (only for confirmed)
        if (registrationStatus == RegistrationStatus.Confirmed)
        {
            ticketType.QuantitySold++;
            event_.CurrentRegistrations++;
        }
        else
        {
            event_.WaitlistCount++;
        }

        // Check if event is now sold out
        if (event_.CurrentRegistrations >= event_.TotalCapacity
            && event_.Status == EventStatus.Published)
        {
            event_.Status = EventStatus.SoldOut;
        }

        event_.UpdatedAt = now;
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Registration {ConfirmationNumber} created for Event {EventId}, Attendee {AttendeeId}, Status: {Status}",
            confirmationNumber, eventId, attendeeId, registrationStatus);

        return registration;
    }

    public async Task<Registration?> GetByIdAsync(int id)
    {
        return await context.Registrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Registration?> GetByConfirmationNumberAsync(string confirmationNumber)
    {
        return await context.Registrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.ConfirmationNumber == confirmationNumber);
    }

    public async Task CancelAsync(int id, string? reason)
    {
        var registration = await context.Registrations
            .Include(r => r.Event)
            .Include(r => r.TicketType)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new InvalidOperationException($"Registration with Id {id} not found.");

        var now = DateTime.UtcNow;

        // Can't cancel within 24 hours of event start
        if (registration.Event.StartDate - now < TimeSpan.FromHours(24))
        {
            throw new InvalidOperationException("Cannot cancel a registration within 24 hours of the event start.");
        }

        var previousStatus = registration.Status;

        registration.Status = RegistrationStatus.Cancelled;
        registration.CancellationDate = now;
        registration.CancellationReason = reason;
        registration.UpdatedAt = now;

        if (previousStatus == RegistrationStatus.Confirmed)
        {
            registration.TicketType.QuantitySold--;
            registration.Event.CurrentRegistrations--;

            // Promote first waitlisted registration
            var nextWaitlisted = await context.Registrations
                .Include(r => r.TicketType)
                .Where(r => r.EventId == registration.EventId
                    && r.Status == RegistrationStatus.Waitlisted)
                .OrderBy(r => r.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = RegistrationStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;

                // Determine price for promoted registration
                var price = nextWaitlisted.TicketType.Price;
                if (registration.Event.EarlyBirdDeadline.HasValue
                    && now < registration.Event.EarlyBirdDeadline.Value
                    && nextWaitlisted.TicketType.EarlyBirdPrice.HasValue)
                {
                    price = nextWaitlisted.TicketType.EarlyBirdPrice.Value;
                }

                nextWaitlisted.AmountPaid = price;
                nextWaitlisted.TicketType.QuantitySold++;
                registration.Event.CurrentRegistrations++;
                registration.Event.WaitlistCount--;

                logger.LogInformation(
                    "Promoted waitlisted registration {ConfirmationNumber} to Confirmed for Event {EventId}",
                    nextWaitlisted.ConfirmationNumber, registration.EventId);
            }

            // If event was SoldOut and now has capacity, set back to Published
            if (registration.Event.Status == EventStatus.SoldOut
                && registration.Event.CurrentRegistrations < registration.Event.TotalCapacity)
            {
                registration.Event.Status = EventStatus.Published;
            }
        }
        else if (previousStatus == RegistrationStatus.Waitlisted)
        {
            registration.Event.WaitlistCount--;
        }

        registration.Event.UpdatedAt = now;
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Cancelled registration {ConfirmationNumber} (Id: {Id}). Reason: {Reason}",
            registration.ConfirmationNumber, id, reason);
    }

    public async Task<(List<Registration> Items, int TotalCount)> GetEventRosterAsync(
        int eventId, string? search, int page, int pageSize)
    {
        var query = context.Registrations
            .AsNoTracking()
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Where(r => r.EventId == eventId
                && (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(r =>
                r.Attendee.FirstName.ToLower().Contains(term) ||
                r.Attendee.LastName.ToLower().Contains(term) ||
                r.Attendee.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(r => r.Attendee.LastName)
            .ThenBy(r => r.Attendee.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Registration>> GetEventWaitlistAsync(int eventId)
    {
        return await context.Registrations
            .AsNoTracking()
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
            .OrderBy(r => r.WaitlistPosition)
            .ToListAsync();
    }

    public async Task<List<Registration>> GetRecentRegistrationsAsync(int count = 10)
    {
        return await context.Registrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .OrderByDescending(r => r.RegistrationDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<string> GenerateConfirmationNumberAsync(int eventId, DateTime eventStartDate)
    {
        var datePrefix = eventStartDate.ToString("yyyyMMdd");
        var prefix = $"SPK-{datePrefix}-";

        var maxNumber = await context.Registrations
            .AsNoTracking()
            .Where(r => r.EventId == eventId && r.ConfirmationNumber.StartsWith(prefix))
            .Select(r => r.ConfirmationNumber)
            .OrderByDescending(c => c)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (maxNumber is not null)
        {
            var lastSegment = maxNumber[prefix.Length..];
            if (int.TryParse(lastSegment, out var parsed))
            {
                nextNumber = parsed + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }
}
