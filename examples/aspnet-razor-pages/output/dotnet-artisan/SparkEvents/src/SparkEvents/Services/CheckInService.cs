using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class CheckInService(SparkEventsDbContext db, ILogger<CheckInService> logger) : ICheckInService
{
    public async Task<CheckIn> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes)
    {
        var registration = await db.Registrations
            .Include(r => r.Event)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found.");

        if (registration.Status != RegistrationStatus.Confirmed)
        {
            throw new InvalidOperationException("Only confirmed registrations can be checked in.");
        }

        if (registration.CheckIn is not null)
        {
            throw new InvalidOperationException("This registration has already been checked in.");
        }

        var now = DateTime.UtcNow;
        var evt = registration.Event;

        // Check-in window: StartDate - 1 hour to EndDate
        if (now < evt.StartDate.AddHours(-1) || now > evt.EndDate)
        {
            throw new InvalidOperationException("Check-in is only available on the day of the event (from 1 hour before start to end time).");
        }

        var checkIn = new CheckIn
        {
            RegistrationId = registrationId,
            CheckInTime = now,
            CheckedInBy = checkedInBy,
            Notes = notes,
        };

        registration.Status = RegistrationStatus.CheckedIn;
        registration.CheckInTime = now;

        db.CheckIns.Add(checkIn);
        await db.SaveChangesAsync();

        logger.LogInformation("Check-in processed: Registration {RegistrationId} by {Staff}", registrationId, checkedInBy);
        return checkIn;
    }

    public async Task<List<Registration>> SearchForCheckInAsync(int eventId, string query)
    {
        return await db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Include(r => r.CheckIn)
            .Where(r => r.EventId == eventId &&
                        (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn) &&
                        (r.ConfirmationNumber.Contains(query) ||
                         r.Attendee.FirstName.Contains(query) ||
                         r.Attendee.LastName.Contains(query) ||
                         r.Attendee.Email.Contains(query)))
            .OrderBy(r => r.Attendee.LastName)
            .Take(20)
            .ToListAsync();
    }

    public async Task<(int checkedIn, int total)> GetCheckInStatsAsync(int eventId)
    {
        var stats = await db.Registrations
            .Where(r => r.EventId == eventId && (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn))
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                CheckedIn = g.Count(r => r.Status == RegistrationStatus.CheckedIn),
            })
            .FirstOrDefaultAsync();

        return stats is null ? (0, 0) : (stats.CheckedIn, stats.Total);
    }
}
