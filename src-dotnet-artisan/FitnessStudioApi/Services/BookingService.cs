using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new InvalidOperationException("Member not found.");

        if (!member.IsActive)
        {
            throw new InvalidOperationException("Member account is inactive.");
        }

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new InvalidOperationException("Class schedule not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
        {
            throw new InvalidOperationException($"Cannot book a class that is {schedule.Status}.");
        }

        // Booking window: max 7 days in advance, min 30 min before class
        var now = DateTime.UtcNow;
        if (schedule.StartTime > now.AddDays(7))
        {
            throw new InvalidOperationException("Cannot book more than 7 days in advance.");
        }

        if (schedule.StartTime <= now.AddMinutes(30))
        {
            throw new InvalidOperationException("Cannot book less than 30 minutes before class start.");
        }

        // Active membership required
        var activeMembership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms =>
                ms.MemberId == request.MemberId &&
                ms.Status == MembershipStatus.Active, ct)
            ?? throw new InvalidOperationException("Member does not have an active membership.");

        // Premium class tier check
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
        {
            throw new InvalidOperationException("Your membership plan does not allow booking premium classes.");
        }

        // Weekly booking limit
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var isoWeekStart = GetIsoWeekStart(now);
            var isoWeekEnd = isoWeekStart.AddDays(7);

            var weeklyBookings = await db.Bookings.CountAsync(b =>
                b.MemberId == request.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                b.ClassSchedule.StartTime >= isoWeekStart &&
                b.ClassSchedule.StartTime < isoWeekEnd, ct);

            if (weeklyBookings >= maxPerWeek)
            {
                throw new InvalidOperationException($"Weekly booking limit of {maxPerWeek} reached.");
            }
        }

        // No double booking (overlapping class times)
        var hasOverlap = await db.Bookings.AnyAsync(b =>
            b.MemberId == request.MemberId &&
            b.Status == BookingStatus.Confirmed &&
            b.ClassSchedule.StartTime < schedule.EndTime &&
            b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
        {
            throw new InvalidOperationException("Member already has a booking for an overlapping class time.");
        }

        // Already booked this class?
        var alreadyBooked = await db.Bookings.AnyAsync(b =>
            b.MemberId == request.MemberId &&
            b.ClassScheduleId == request.ClassScheduleId &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted), ct);

        if (alreadyBooked)
        {
            throw new InvalidOperationException("Member already has an active booking for this class.");
        }

        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId
        };

        // Check capacity
        if (schedule.CurrentEnrollment < schedule.Capacity)
        {
            booking.Status = BookingStatus.Confirmed;
            schedule.CurrentEnrollment++;
        }
        else
        {
            booking.Status = BookingStatus.Waitlisted;
            schedule.WaitlistCount++;
            booking.WaitlistPosition = schedule.WaitlistCount;
        }

        schedule.UpdatedAt = DateTime.UtcNow;
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ClassId} with status {Status}",
            booking.Id, request.MemberId, request.ClassScheduleId, booking.Status);

        return await GetByIdAsync(booking.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created booking.");
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : MapToResponse(booking);
    }

    public async Task<BookingResponse> CancelAsync(int id, string? reason, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Attended or BookingStatus.NoShow)
        {
            throw new InvalidOperationException($"Cannot cancel a booking with status {booking.Status}.");
        }

        var schedule = booking.ClassSchedule;

        if (schedule.Status is ClassScheduleStatus.InProgress or ClassScheduleStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a booking for a class that has already started or completed.");
        }

        var now = DateTime.UtcNow;
        var hoursUntilClass = (schedule.StartTime - now).TotalHours;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = hoursUntilClass < 2
            ? $"Late cancellation: {reason ?? "No reason provided"}"
            : reason;
        booking.UpdatedAt = now;

        if (booking.WaitlistPosition is null)
        {
            // Was a confirmed booking
            schedule.CurrentEnrollment = Math.Max(0, schedule.CurrentEnrollment - 1);

            // Promote first waitlisted booking
            var nextWaitlisted = await db.Bookings
                .Include(b => b.Member)
                .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;
                schedule.CurrentEnrollment++;
                schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

                // Reorder remaining waitlist positions
                var remainingWaitlisted = await db.Bookings
                    .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (var i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                }

                logger.LogInformation("Promoted booking {BookingId} from waitlist for class {ClassId}",
                    nextWaitlisted.Id, schedule.Id);
            }
        }
        else
        {
            // Was a waitlisted booking
            schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

            // Reorder remaining waitlist
            var remainingWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == schedule.Id &&
                            b.Status == BookingStatus.Waitlisted &&
                            b.Id != booking.Id)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            for (var i = 0; i < remainingWaitlisted.Count; i++)
            {
                remainingWaitlisted[i].WaitlistPosition = i + 1;
            }
        }

        schedule.UpdatedAt = now;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled booking {BookingId}", id);

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException($"Cannot check in a booking with status {booking.Status}.");
        }

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;
        var minutesBefore = (classStart - now).TotalMinutes;

        // Check-in window: 15 min before to 15 min after class start
        if (minutesBefore > 15)
        {
            throw new InvalidOperationException("Check-in opens 15 minutes before class start.");
        }

        if (minutesBefore < -15)
        {
            throw new InvalidOperationException("Check-in window has closed (15 minutes after class start).");
        }

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Checked in booking {BookingId}", id);

        return MapToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule)
                .ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException($"Can only mark confirmed bookings as no-show. Current status: {booking.Status}.");
        }

        var now = DateTime.UtcNow;
        var minutesSinceStart = (now - booking.ClassSchedule.StartTime).TotalMinutes;

        if (minutesSinceStart < 15)
        {
            throw new InvalidOperationException("Can only mark as no-show after 15 minutes past class start.");
        }

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Marked booking {BookingId} as no-show", id);

        return MapToResponse(booking);
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var daysToMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return date.Date.AddDays(-daysToMonday);
    }

    private static BookingResponse MapToResponse(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason,
        b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
        b.ClassSchedule.Room,
        $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}",
        b.CreatedAt, b.UpdatedAt);
}
