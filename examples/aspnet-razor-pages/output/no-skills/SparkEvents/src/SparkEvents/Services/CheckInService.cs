using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class CheckInService : ICheckInService
{
    private readonly SparkEventsDbContext _context;
    private readonly ILogger<CheckInService> _logger;

    public CheckInService(SparkEventsDbContext context, ILogger<CheckInService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CheckIn> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes)
    {
        var registration = await _context.Registrations
            .Include(r => r.Event)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.Id == registrationId);

        if (registration == null) throw new InvalidOperationException("Registration not found.");
        if (registration.Status != RegistrationStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed registrations can be checked in.");
        if (registration.CheckIn != null)
            throw new InvalidOperationException("This registration has already been checked in.");

        var evt = registration.Event;
        var now = DateTime.UtcNow;
        var checkInStart = evt.StartDate.AddHours(-1);

        if (now < checkInStart || now > evt.EndDate)
            throw new InvalidOperationException("Check-in is only available from 1 hour before the event start until the event ends.");

        registration.Status = RegistrationStatus.CheckedIn;
        registration.CheckInTime = now;
        registration.UpdatedAt = now;

        var checkIn = new CheckIn
        {
            RegistrationId = registrationId,
            CheckInTime = now,
            CheckedInBy = checkedInBy,
            Notes = notes
        };

        _context.CheckIns.Add(checkIn);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Check-in processed: Registration {Id} by {StaffName}", registrationId, checkedInBy);
        return checkIn;
    }

    public async Task<List<Registration>> SearchForCheckInAsync(int eventId, string searchTerm)
    {
        return await _context.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .Where(r => r.EventId == eventId &&
                (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn) &&
                (r.Attendee.FirstName.Contains(searchTerm) ||
                 r.Attendee.LastName.Contains(searchTerm) ||
                 r.ConfirmationNumber.Contains(searchTerm)))
            .OrderBy(r => r.Attendee.LastName)
            .ToListAsync();
    }

    public async Task<(int CheckedIn, int Total)> GetCheckInStatsAsync(int eventId)
    {
        var checkedIn = await _context.Registrations.CountAsync(r =>
            r.EventId == eventId && r.Status == RegistrationStatus.CheckedIn);
        var total = await _context.Registrations.CountAsync(r =>
            r.EventId == eventId && (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn));
        return (checkedIn, total);
    }

    public async Task<bool> CanCheckInAsync(int eventId)
    {
        var evt = await _context.Events.FindAsync(eventId);
        if (evt == null) return false;
        var now = DateTime.UtcNow;
        return now >= evt.StartDate.AddHours(-1) && now <= evt.EndDate;
    }
}
