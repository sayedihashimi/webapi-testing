using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class BookingService(StudioDbContext db, ILogger<BookingService> logger)
{
    public async Task<(string? Error, BookingResponse? Result)> CreateAsync(
        CreateBookingRequest request, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct);
        if (member is null)
        {
            return ("member_not_found", null);
        }

        if (!member.IsActive)
        {
            return ("member_inactive", null);
        }

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct);

        if (schedule is null)
        {
            return ("class_not_found", null);
        }

        if (schedule.Status != ClassScheduleStatus.Scheduled)
        {
            return ("class_not_available", null);
        }

        // Booking window: 7 days in advance, at least 30 min before
        var now = DateTime.UtcNow;
        if (schedule.StartTime > now.AddDays(7))
        {
            return ("too_far_in_advance", null);
        }

        if (schedule.StartTime <= now.AddMinutes(30))
        {
            return ("too_late_to_book", null);
        }

        // Active membership required
        var membership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.MemberId == request.MemberId &&
                                       ms.Status == MembershipStatus.Active, ct);

        if (membership is null)
        {
            return ("no_active_membership", null);
        }

        // Premium class access
        if (schedule.ClassType.IsPremium && !membership.MembershipPlan.AllowsPremiumClasses)
        {
            return ("premium_access_denied", null);
        }

        // Weekly booking limit
        var maxPerWeek = membership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(now);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookingCount = await db.Bookings
                .CountAsync(b => b.MemberId == request.MemberId &&
                                b.ClassSchedule.StartTime >= weekStart &&
                                b.ClassSchedule.StartTime < weekEnd &&
                                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended), ct);

            if (weeklyBookingCount >= maxPerWeek)
            {
                return ("weekly_limit_reached", null);
            }
        }

        // No double booking (overlapping class times)
        var hasOverlap = await db.Bookings
            .AnyAsync(b => b.MemberId == request.MemberId &&
                          b.Status == BookingStatus.Confirmed &&
                          b.ClassSchedule.StartTime < schedule.EndTime &&
                          b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
        {
            return ("double_booking", null);
        }

        // Check if already booked this class
        var existingBooking = await db.Bookings
            .AnyAsync(b => b.ClassScheduleId == request.ClassScheduleId &&
                          b.MemberId == request.MemberId &&
                          (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted), ct);

        if (existingBooking)
        {
            return ("already_booked", null);
        }

        // Create booking
        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId,
            Member = member,
            ClassSchedule = schedule
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

        schedule.UpdatedAt = DateTime.UtcNow;
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Booking {BookingId} created for member {MemberId} in class {ClassId} - Status: {Status}",
            booking.Id, request.MemberId, request.ClassScheduleId, booking.Status);

        return (null, ToResponse(booking));
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : ToResponse(booking);
    }

    public async Task<string?> CancelAsync(int id, string? reason, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
        {
            return "not_found";
        }

        if (booking.Status is not (BookingStatus.Confirmed or BookingStatus.Waitlisted))
        {
            return "cannot_cancel";
        }

        var now = DateTime.UtcNow;

        // Cannot cancel started/completed classes
        if (booking.ClassSchedule.Status is ClassScheduleStatus.InProgress or ClassScheduleStatus.Completed)
        {
            return "class_started";
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;
        booking.UpdatedAt = now;

        if (booking.WaitlistPosition.HasValue)
        {
            // Was waitlisted — update positions and schedule count
            var waitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                           b.Status == BookingStatus.Waitlisted &&
                           b.WaitlistPosition > booking.WaitlistPosition)
                .ToListAsync(ct);

            foreach (var w in waitlisted)
            {
                w.WaitlistPosition--;
                w.UpdatedAt = now;
            }

            booking.ClassSchedule.WaitlistCount--;
        }
        else
        {
            // Was confirmed — update enrollment and promote from waitlist
            booking.ClassSchedule.CurrentEnrollment--;

            var nextWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                           b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Reorder remaining waitlist
                var remaining = await db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                               b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                var pos = 1;
                foreach (var w in remaining)
                {
                    w.WaitlistPosition = pos++;
                    w.UpdatedAt = now;
                }

                logger.LogInformation("Booking {BookingId} promoted from waitlist to confirmed", nextWaitlisted.Id);
            }
        }

        booking.ClassSchedule.UpdatedAt = now;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Booking {BookingId} cancelled", id);
        return null;
    }

    public async Task<string?> CheckInAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
        {
            return "not_found";
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            return "not_confirmed";
        }

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Check-in window: 15 min before to 15 min after class start
        if (now < classStart.AddMinutes(-15) || now > classStart.AddMinutes(15))
        {
            return "outside_checkin_window";
        }

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Booking {BookingId} checked in", id);
        return null;
    }

    public async Task<string?> MarkNoShowAsync(int id, CancellationToken ct = default)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null)
        {
            return "not_found";
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            return "not_confirmed";
        }

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Can only mark no-show after 15 min past class start
        if (now < classStart.AddMinutes(15))
        {
            return "too_early_for_noshow";
        }

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Booking {BookingId} marked as no-show", id);
        return null;
    }

    internal static BookingResponse ToResponse(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.ClassSchedule.StartTime, b.MemberId,
        $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status.ToString(), b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason);

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var diff = dayOfWeek == 0 ? 6 : dayOfWeek - 1; // Monday = 0
        return date.Date.AddDays(-diff);
    }
}
