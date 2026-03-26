using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public class CheckInService(SparkEventsDbContext context, ILogger<CheckInService> logger) : ICheckInService
{
    public async Task<CheckIn> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes)
    {
        var registration = await context.Registrations
            .Include(r => r.Event)
            .Include(r => r.Attendee)
            .FirstOrDefaultAsync(r => r.Id == registrationId)
            ?? throw new InvalidOperationException($"Registration with Id {registrationId} not found.");

        if (registration.Status != RegistrationStatus.Confirmed)
        {
            throw new InvalidOperationException(
                $"Only Confirmed registrations can be checked in. Current status: {registration.Status}.");
        }

        var now = DateTime.UtcNow;
        var checkInWindowStart = registration.Event.StartDate.AddHours(-1);
        var checkInWindowEnd = registration.Event.EndDate;

        if (now < checkInWindowStart)
        {
            throw new InvalidOperationException(
                $"Check-in is not available until {checkInWindowStart:g} (1 hour before event start).");
        }

        if (now > checkInWindowEnd)
        {
            throw new InvalidOperationException("Check-in is no longer available. The event has ended.");
        }

        var checkIn = new CheckIn
        {
            RegistrationId = registrationId,
            CheckedInBy = checkedInBy,
            Notes = notes,
            CheckInTime = now
        };

        context.CheckIns.Add(checkIn);

        registration.Status = RegistrationStatus.CheckedIn;
        registration.CheckInTime = now;
        registration.UpdatedAt = now;

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Checked in attendee '{FirstName} {LastName}' (Registration {ConfirmationNumber}) for Event {EventId}",
            registration.Attendee.FirstName, registration.Attendee.LastName,
            registration.ConfirmationNumber, registration.EventId);

        return checkIn;
    }

    public async Task<Registration?> LookupForCheckInAsync(int eventId, string searchTerm)
    {
        var term = searchTerm.Trim().ToLower();

        return await context.Registrations
            .AsNoTracking()
            .Include(r => r.Attendee)
            .Include(r => r.TicketType)
            .Where(r => r.EventId == eventId
                && r.Status == RegistrationStatus.Confirmed
                && (r.ConfirmationNumber.ToLower().Contains(term)
                    || r.Attendee.FirstName.ToLower().Contains(term)
                    || r.Attendee.LastName.ToLower().Contains(term)))
            .FirstOrDefaultAsync();
    }

    public async Task<(int TotalConfirmed, int CheckedIn)> GetCheckInStatsAsync(int eventId)
    {
        var totalConfirmed = await context.Registrations
            .AsNoTracking()
            .CountAsync(r => r.EventId == eventId
                && (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn));

        var checkedIn = await context.Registrations
            .AsNoTracking()
            .CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.CheckedIn);

        return (totalConfirmed, checkedIn);
    }
}
