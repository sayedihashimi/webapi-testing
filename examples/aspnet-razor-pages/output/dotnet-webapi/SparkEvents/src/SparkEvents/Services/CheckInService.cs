using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Services;

public sealed class CheckInService(SparkEventsDbContext db, ILogger<CheckInService> logger)
    : ICheckInService
{
    public async Task<CheckIn> ProcessCheckInAsync(int registrationId, string checkedInBy, string? notes,
        CancellationToken ct = default)
    {
        var registration = await db.Registrations
            .Include(r => r.Event)
            .Include(r => r.CheckIn)
            .FirstOrDefaultAsync(r => r.Id == registrationId, ct)
            ?? throw new KeyNotFoundException($"Registration with ID {registrationId} not found.");

        if (registration.Status != RegistrationStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed registrations can be checked in.");

        if (registration.CheckIn is not null)
            throw new InvalidOperationException("This registration has already been checked in.");

        // Validate check-in window
        var now = DateTime.UtcNow;
        var checkInStart = registration.Event.StartDate.AddHours(-1);
        var checkInEnd = registration.Event.EndDate;

        if (now < checkInStart)
            throw new InvalidOperationException("Check-in has not started yet. Check-in opens 1 hour before the event.");

        if (now > checkInEnd)
            throw new InvalidOperationException("Check-in has ended. The event is over.");

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

        db.CheckIns.Add(checkIn);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Checked in registration {RegistrationId} by {CheckedInBy}", registrationId, checkedInBy);
        return checkIn;
    }

    public async Task<int> GetCheckInCountAsync(int eventId, CancellationToken ct = default)
    {
        return await db.Registrations.CountAsync(r =>
            r.EventId == eventId && r.Status == RegistrationStatus.CheckedIn, ct);
    }

    public async Task<int> GetTotalConfirmedCountAsync(int eventId, CancellationToken ct = default)
    {
        return await db.Registrations.CountAsync(r =>
            r.EventId == eventId &&
            (r.Status == RegistrationStatus.Confirmed || r.Status == RegistrationStatus.CheckedIn), ct);
    }
}
