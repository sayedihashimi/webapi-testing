using System.Globalization;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class BookingService(AppDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<(BookingResponse? Result, string? Error)> CreateAsync(
        CreateBookingRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct);
        if (member is null)
        {
            return (null, "Member not found");
        }

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct);

        if (schedule is null)
        {
            return (null, "Class schedule not found");
        }

        if (schedule.Status is not ClassScheduleStatus.Scheduled)
        {
            return (null, "Class is not available for booking");
        }

        // Business Rule 1: Booking window (7 days ahead, 30 min before)
        var now = DateTime.UtcNow;
        if (schedule.StartTime > now.AddDays(7))
        {
            return (null, "Cannot book classes more than 7 days in advance");
        }

        if (schedule.StartTime <= now.AddMinutes(30))
        {
            return (null, "Cannot book classes less than 30 minutes before start time");
        }

        // Business Rule 6: Active membership required
        var activeMembership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms =>
                ms.MemberId == request.MemberId &&
                ms.Status == MembershipStatus.Active, ct);

        if (activeMembership is null)
        {
            return (null, "Member does not have an active membership. Only active memberships can book classes.");
        }

        // Business Rule 4: Premium class access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
        {
            return (null, "Your membership plan does not include access to premium classes. Please upgrade to Premium or Elite.");
        }

        // Business Rule 5: Weekly booking limit
        var plan = activeMembership.MembershipPlan;
        if (plan.MaxClassBookingsPerWeek != -1)
        {
            var isoWeek = ISOWeek.GetWeekOfYear(schedule.StartTime);
            var isoYear = ISOWeek.GetYear(schedule.StartTime);
            var weekStart = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookings = await db.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b =>
                    b.MemberId == request.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.ClassSchedule.StartTime >= weekStart &&
                    b.ClassSchedule.StartTime < weekEnd, ct);

            if (weeklyBookings >= plan.MaxClassBookingsPerWeek)
            {
                return (null, $"Weekly booking limit reached ({plan.MaxClassBookingsPerWeek} per week for {plan.Name} plan)");
            }
        }

        // Business Rule 7: No double booking (overlapping classes)
        var hasOverlap = await db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b =>
                b.MemberId == request.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
                b.ClassSchedule.StartTime < schedule.EndTime &&
                b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
        {
            return (null, "You already have a booking for an overlapping class at this time");
        }

        // Check if already booked for this class
        var alreadyBooked = await db.Bookings.AnyAsync(b =>
            b.MemberId == request.MemberId &&
            b.ClassScheduleId == request.ClassScheduleId &&
            b.Status != BookingStatus.Cancelled, ct);

        if (alreadyBooked)
        {
            return (null, "You are already booked for this class");
        }

        // Business Rule 2: Capacity management
        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId,
            BookingDate = now
        };

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

        schedule.UpdatedAt = now;
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        await db.Entry(booking).Reference(b => b.ClassSchedule).LoadAsync(ct);
        await db.Entry(booking.ClassSchedule).Reference(cs => cs.ClassType).LoadAsync(ct);
        await db.Entry(booking).Reference(b => b.Member).LoadAsync(ct);

        logger.LogInformation("Booking created: {BookingId} for member {MemberId} in class {ClassId} - Status: {Status}",
            booking.Id, booking.MemberId, booking.ClassScheduleId, booking.Status);

        return (MapToResponse(booking), null);
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : MapToResponse(booking);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(
        int id, CancelBookingRequest? request, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
        {
            return (false, "Booking not found");
        }

        if (booking.Status is BookingStatus.Cancelled)
        {
            return (false, "Booking is already cancelled");
        }

        if (booking.Status is BookingStatus.Attended or BookingStatus.NoShow)
        {
            return (false, "Cannot cancel a booking that has been attended or marked as no-show");
        }

        var now = DateTime.UtcNow;

        // Business Rule 3: Cannot cancel classes that have started or completed
        if (booking.ClassSchedule.StartTime <= now)
        {
            return (false, "Cannot cancel a booking for a class that has already started");
        }

        var hoursUntilClass = (booking.ClassSchedule.StartTime - now).TotalHours;
        var reason = request?.Reason;

        if (hoursUntilClass < 2)
        {
            reason = $"Late cancellation: {reason ?? "No reason provided"}";
        }

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;
        booking.UpdatedAt = now;

        var schedule = booking.ClassSchedule;

        if (wasConfirmed)
        {
            schedule.CurrentEnrollment = Math.Max(0, schedule.CurrentEnrollment - 1);

            // Promote first waitlisted person
            var nextWaitlisted = await db.Bookings
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

                // Re-number remaining waitlist
                var remainingWaitlist = await db.Bookings
                    .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (int i = 0; i < remainingWaitlist.Count; i++)
                {
                    remainingWaitlist[i].WaitlistPosition = i + 1;
                }

                logger.LogInformation("Promoted booking {BookingId} from waitlist for class {ClassId}",
                    nextWaitlisted.Id, schedule.Id);
            }
        }
        else if (wasWaitlisted)
        {
            schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

            // Re-number remaining waitlist
            var remainingWaitlist = await db.Bookings
                .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            for (int i = 0; i < remainingWaitlist.Count; i++)
            {
                remainingWaitlist[i].WaitlistPosition = i + 1;
            }
        }

        schedule.UpdatedAt = now;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled booking {BookingId} for class {ClassId}", id, schedule.Id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
        {
            return (false, "Booking not found");
        }

        if (booking.Status is not BookingStatus.Confirmed)
        {
            return (false, "Only confirmed bookings can be checked in");
        }

        // Business Rule 11: Check-in window (15 min before to 15 min after start)
        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;
        var windowStart = classStart.AddMinutes(-15);
        var windowEnd = classStart.AddMinutes(15);

        if (now < windowStart)
        {
            return (false, "Check-in is not yet available. Check-in opens 15 minutes before class start.");
        }

        if (now > windowEnd)
        {
            return (false, "Check-in window has closed. Check-in is available up to 15 minutes after class start.");
        }

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Member checked in for booking {BookingId}", id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
        {
            return (false, "Booking not found");
        }

        if (booking.Status is not BookingStatus.Confirmed)
        {
            return (false, "Only confirmed bookings can be marked as no-show");
        }

        // Business Rule 12: Can mark no-show after 15 minutes past class start
        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
        {
            return (false, "Cannot mark as no-show until 15 minutes after class start time");
        }

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Marked booking {BookingId} as no-show", id);
        return (true, null);
    }

    private static BookingResponse MapToResponse(Booking b) =>
        new(b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
            b.ClassSchedule.StartTime, b.MemberId,
            $"{b.Member.FirstName} {b.Member.LastName}",
            b.BookingDate, b.Status.ToString(), b.WaitlistPosition,
            b.CheckInTime, b.CancellationDate, b.CancellationReason);
}
