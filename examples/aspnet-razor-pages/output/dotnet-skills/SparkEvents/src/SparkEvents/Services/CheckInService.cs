using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class CheckInService : ICheckInService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<CheckInService> _logger;

    public CheckInService(SparkEventsDbContext db, ILogger<CheckInService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes)
    {
        var registration = await _db.Registrations
            .Include(r => r.Event)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.Id == registrationId);

        if (registration == null)
            return (false, "Registration not found.");

        if (registration.Status != RegistrationStatus.Confirmed)
            return (false, $"Only confirmed registrations can be checked in. Current status: {registration.Status}.");

        if (registration.CheckIn != null)
            return (false, "This registration has already been checked in.");

        // Check-in window validation
        var now = DateTime.UtcNow;
        var windowStart = registration.Event.StartDate.AddHours(-1);
        var windowEnd = registration.Event.EndDate;

        if (now < windowStart || now > windowEnd)
            return (false, "Check-in is only allowed between 1 hour before the event starts and when the event ends.");

        var checkIn = new CheckIn
        {
            RegistrationId = registrationId,
            CheckInTime = now,
            CheckedInBy = checkedInBy,
            Notes = notes
        };

        registration.Status = RegistrationStatus.CheckedIn;
        registration.CheckInTime = now;
        registration.UpdatedAt = now;

        _db.CheckIns.Add(checkIn);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Check-in processed: Registration {ConfirmationNumber} by {Staff}",
            registration.ConfirmationNumber, checkedInBy);

        return (true, null);
    }

    public async Task<List<Registration>> SearchForCheckInAsync(int eventId, string? searchTerm)
    {
        var query = _db.Registrations
            .AsNoTracking()
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .AsSplitQuery()
            .Where(r => r.EventId == eventId &&
                       (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(r =>
                r.ConfirmationNumber.ToLower().Contains(term) ||
                r.Attendee.FirstName.ToLower().Contains(term) ||
                r.Attendee.LastName.ToLower().Contains(term));
        }

        return await query.OrderBy(r => r.Attendee.LastName).ThenBy(r => r.Attendee.FirstName).ToListAsync();
    }

    public async Task<(int CheckedIn, int Total)> GetCheckInStatsAsync(int eventId)
    {
        var checkedIn = await _db.Registrations
            .CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.CheckedIn);
        var total = await _db.Registrations
            .CountAsync(r => r.EventId == eventId &&
                           (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn));
        return (checkedIn, total);
    }

    public async Task<bool> IsCheckInWindowOpenAsync(int eventId)
    {
        var evt = await _db.Events.FindAsync(eventId);
        if (evt == null) return false;

        var now = DateTime.UtcNow;
        return now >= evt.StartDate.AddHours(-1) && now <= evt.EndDate;
    }
}
