using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class RegistrationService : IRegistrationService
{
    private readonly SparkEventsDbContext _context;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(SparkEventsDbContext context, ILogger<RegistrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Registration> RegisterAsync(int eventId, int attendeeId, int ticketTypeId, string? specialRequests)
    {
        var evt = await _context.Events.Include(e => e.TicketTypes).FirstOrDefaultAsync(e => e.Id == eventId);
        if (evt == null) throw new InvalidOperationException("Event not found.");

        var now = DateTime.UtcNow;
        if (now < evt.RegistrationOpenDate || now > evt.RegistrationCloseDate)
            throw new InvalidOperationException("Registration is not open for this event.");

        if (evt.Status == EventStatus.Cancelled || evt.Status == EventStatus.Completed || evt.Status == EventStatus.Draft)
            throw new InvalidOperationException("Registration is not available for this event.");

        // Check duplicate
        var existing = await _context.Registrations.AnyAsync(r =>
            r.EventId == eventId && r.AttendeeId == attendeeId && r.Status != RegistrationStatus.Cancelled);
        if (existing) throw new InvalidOperationException("This attendee is already registered for this event.");

        var ticketType = evt.TicketTypes.FirstOrDefault(t => t.Id == ticketTypeId && t.IsActive);
        if (ticketType == null) throw new InvalidOperationException("Invalid or inactive ticket type.");

        // Determine price
        decimal price;
        if (evt.EarlyBirdDeadline.HasValue && now <= evt.EarlyBirdDeadline.Value && ticketType.EarlyBirdPrice.HasValue)
            price = ticketType.EarlyBirdPrice.Value;
        else
            price = ticketType.Price;

        var confirmationNumber = await GenerateConfirmationNumberAsync(eventId);

        // Determine if waitlisted or confirmed
        bool isWaitlisted = false;
        if (ticketType.QuantitySold >= ticketType.Quantity)
        {
            throw new InvalidOperationException($"The '{ticketType.Name}' ticket type is sold out. Please choose a different ticket type.");
        }

        if (evt.CurrentRegistrations >= evt.TotalCapacity)
        {
            isWaitlisted = true;
        }

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
            var maxPos = await _context.Registrations
                .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
                .MaxAsync(r => (int?)r.WaitlistPosition) ?? 0;
            registration.WaitlistPosition = maxPos + 1;
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

        evt.UpdatedAt = now;
        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Registration created: {ConfirmationNumber} for event {EventId}, attendee {AttendeeId}, status {Status}",
            confirmationNumber, eventId, attendeeId, registration.Status);

        return registration;
    }

    public async Task<Registration?> GetRegistrationByIdAsync(int id)
    {
        return await _context.Registrations
            .Include(r => r.Event).ThenInclude(e => e.Venue)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> CancelRegistrationAsync(int id, string? reason)
    {
        var registration = await _context.Registrations
            .Include(r => r.Event)
            .Include(r => r.TicketType)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (registration == null) return false;
        if (registration.Status == RegistrationStatus.Cancelled || registration.Status == RegistrationStatus.CheckedIn)
            return false;

        var evt = registration.Event;

        // Check 24-hour policy
        if ((evt.StartDate - DateTime.UtcNow).TotalHours < 24)
            throw new InvalidOperationException("Cancellations are not allowed within 24 hours of the event start.");

        var wasConfirmed = registration.Status == RegistrationStatus.Confirmed;

        registration.Status = RegistrationStatus.Cancelled;
        registration.CancellationDate = DateTime.UtcNow;
        registration.CancellationReason = reason;
        registration.UpdatedAt = DateTime.UtcNow;

        if (wasConfirmed)
        {
            evt.CurrentRegistrations--;
            registration.TicketType.QuantitySold--;

            // Promote from waitlist
            var nextWaitlisted = await _context.Registrations
                .Include(r => r.TicketType)
                .Where(r => r.EventId == evt.Id && r.Status == RegistrationStatus.Waitlisted)
                .OrderBy(r => r.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (nextWaitlisted != null)
            {
                nextWaitlisted.Status = RegistrationStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = DateTime.UtcNow;
                evt.CurrentRegistrations++;
                nextWaitlisted.TicketType.QuantitySold++;
                evt.WaitlistCount--;

                // Reorder remaining waitlist positions
                var remaining = await _context.Registrations
                    .Where(r => r.EventId == evt.Id && r.Status == RegistrationStatus.Waitlisted)
                    .OrderBy(r => r.WaitlistPosition)
                    .ToListAsync();
                for (int i = 0; i < remaining.Count; i++)
                    remaining[i].WaitlistPosition = i + 1;

                _logger.LogInformation("Waitlist promotion: Registration {Id} promoted for event {EventId}", nextWaitlisted.Id, evt.Id);
            }

            // Update event status if it was SoldOut
            if (evt.Status == EventStatus.SoldOut && evt.CurrentRegistrations < evt.TotalCapacity)
                evt.Status = EventStatus.Published;
        }
        else if (registration.Status == RegistrationStatus.Waitlisted)
        {
            evt.WaitlistCount--;
        }

        evt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Registration cancelled: {Id}, Reason: {Reason}", id, reason);
        return true;
    }

    public async Task<PaginatedList<Registration>> GetEventRosterAsync(int eventId, string? search, int pageIndex = 1, int pageSize = 10)
    {
        var query = _context.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .Where(r => r.EventId == eventId && (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                r.Attendee.FirstName.Contains(search) ||
                r.Attendee.LastName.Contains(search) ||
                r.Attendee.Email.Contains(search) ||
                r.ConfirmationNumber.Contains(search));
        }

        query = query.OrderBy(r => r.Attendee.LastName).ThenBy(r => r.Attendee.FirstName);

        return await PaginatedList<Registration>.CreateAsync(query, pageIndex, pageSize);
    }

    public async Task<List<Registration>> GetEventWaitlistAsync(int eventId)
    {
        return await _context.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
            .OrderBy(r => r.WaitlistPosition)
            .ToListAsync();
    }

    public async Task<List<Registration>> GetRecentRegistrationsAsync(int count = 10)
    {
        return await _context.Registrations
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .OrderByDescending(r => r.RegistrationDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<int> GetTotalRegistrationsCountAsync()
    {
        return await _context.Registrations.CountAsync(r => r.Status != RegistrationStatus.Cancelled);
    }

    public async Task<bool> CanRegisterAsync(int eventId, int attendeeId)
    {
        return !await _context.Registrations.AnyAsync(r =>
            r.EventId == eventId && r.AttendeeId == attendeeId && r.Status != RegistrationStatus.Cancelled);
    }

    public async Task<string> GenerateConfirmationNumberAsync(int eventId)
    {
        var evt = await _context.Events.FindAsync(eventId);
        if (evt == null) throw new InvalidOperationException("Event not found.");

        var dateStr = evt.StartDate.ToString("yyyyMMdd");
        var lastReg = await _context.Registrations
            .Where(r => r.EventId == eventId)
            .OrderByDescending(r => r.ConfirmationNumber)
            .FirstOrDefaultAsync();

        int nextNum = 1;
        if (lastReg != null)
        {
            var parts = lastReg.ConfirmationNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
                nextNum = lastNum + 1;
        }

        return $"SPK-{dateStr}-{nextNum:D4}";
    }
}
