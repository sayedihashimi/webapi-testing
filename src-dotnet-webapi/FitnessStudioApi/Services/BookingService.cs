using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FitnessStudioApi.Services;

public sealed class BookingService(StudioDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : MapToResponse(booking);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member {request.MemberId} not found.");

        if (!member.IsActive)
            throw new InvalidOperationException("Inactive members cannot book classes.");

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new KeyNotFoundException($"Class schedule {request.ClassScheduleId} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new InvalidOperationException("Cannot book a class that is not scheduled.");

        var now = DateTime.UtcNow;

        // Booking window: max 7 days in advance, min 30 minutes before class
        if (schedule.StartTime > now.AddDays(7))
            throw new InvalidOperationException("Cannot book classes more than 7 days in advance.");

        if (schedule.StartTime <= now.AddMinutes(30))
            throw new InvalidOperationException("Cannot book classes less than 30 minutes before start time.");

        // Active membership required
        var activeMembership = await db.Memberships
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.MemberId == request.MemberId && m.Status == MembershipStatus.Active, ct)
            ?? throw new InvalidOperationException("Member does not have an active membership. Frozen memberships cannot book.");

        // Premium class access check
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new InvalidOperationException("Your membership plan does not allow access to premium classes. Please upgrade to Premium or Elite.");

        // Weekly booking limit
        if (activeMembership.MembershipPlan.MaxClassBookingsPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(now);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookingCount = await db.Bookings.CountAsync(
                b => b.MemberId == request.MemberId
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended)
                    && b.ClassSchedule.StartTime >= weekStart
                    && b.ClassSchedule.StartTime < weekEnd, ct);

            if (weeklyBookingCount >= activeMembership.MembershipPlan.MaxClassBookingsPerWeek)
                throw new InvalidOperationException(
                    $"Weekly booking limit reached ({activeMembership.MembershipPlan.MaxClassBookingsPerWeek} per week for {activeMembership.MembershipPlan.Name} plan).");
        }

        // No double booking — check for overlapping confirmed bookings
        var hasOverlap = await db.Bookings.AnyAsync(
            b => b.MemberId == request.MemberId
                && b.Status == BookingStatus.Confirmed
                && b.ClassSchedule.StartTime < schedule.EndTime
                && b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
            throw new InvalidOperationException("You already have a booking for an overlapping class at this time.");

        // Check existing booking for same class
        var existingBooking = await db.Bookings.AnyAsync(
            b => b.ClassScheduleId == request.ClassScheduleId
                && b.MemberId == request.MemberId
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted), ct);

        if (existingBooking)
            throw new InvalidOperationException("You already have a booking for this class.");

        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId,
            BookingDate = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Capacity check — confirm or waitlist
        if (schedule.CurrentEnrollment < schedule.Capacity)
        {
            booking.Status = BookingStatus.Confirmed;
            schedule.CurrentEnrollment++;
        }
        else
        {
            booking.Status = BookingStatus.Waitlisted;
            booking.WaitlistPosition = schedule.WaitlistCount + 1;
            schedule.WaitlistCount++;
        }

        schedule.UpdatedAt = now;
        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        var result = await db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstAsync(b => b.Id == booking.Id, ct);

        logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ScheduleId} — Status: {Status}",
            booking.Id, request.MemberId, request.ClassScheduleId, booking.Status);

        return MapToResponse(result);
    }

    public async Task<BookingResponse> CancelAsync(int id, CancelBookingRequest request, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking {id} not found.");

        if (booking.Status is not (BookingStatus.Confirmed or BookingStatus.Waitlisted))
            throw new InvalidOperationException($"Cannot cancel a booking with status '{booking.Status}'.");

        var now = DateTime.UtcNow;
        if (booking.ClassSchedule.Status is ClassScheduleStatus.InProgress or ClassScheduleStatus.Completed)
            throw new InvalidOperationException("Cannot cancel booking for a class that has started or completed.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = request.Reason;

        // Late cancellation warning (less than 2 hours before class)
        if (booking.ClassSchedule.StartTime <= now.AddHours(2))
            booking.CancellationReason = $"[LATE CANCELLATION] {request.Reason}";

        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;
            booking.ClassSchedule.UpdatedAt = now;

            // Promote first waitlisted member
            var nextWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId
                    && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Reorder remaining waitlist positions
                var remainingWaitlisted = await db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId
                        && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                var pos = 1;
                foreach (var w in remainingWaitlisted)
                {
                    w.WaitlistPosition = pos++;
                    w.UpdatedAt = now;
                }

                logger.LogInformation("Promoted booking {BookingId} from waitlist for class {ScheduleId}",
                    nextWaitlisted.Id, booking.ClassScheduleId);
            }
        }
        else
        {
            // Was waitlisted
            booking.ClassSchedule.WaitlistCount--;
            booking.ClassSchedule.UpdatedAt = now;

            // Reorder waitlist positions
            var remainingWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId
                    && b.Status == BookingStatus.Waitlisted
                    && b.Id != id)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            var pos = 1;
            foreach (var w in remainingWaitlisted)
            {
                w.WaitlistPosition = pos++;
                w.UpdatedAt = now;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled booking {BookingId}", id);
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be checked in.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Check-in window: 15 min before to 15 min after class start
        if (now < classStart.AddMinutes(-15))
            throw new InvalidOperationException("Check-in is not yet available. Check-in opens 15 minutes before class.");

        if (now > classStart.AddMinutes(15))
            throw new InvalidOperationException("Check-in window has closed. Check-in is available up to 15 minutes after class start.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Checked in booking {BookingId} for member {MemberId}", id, booking.MemberId);
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be marked as no-show.");

        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new InvalidOperationException("Cannot mark no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Marked booking {BookingId} as no-show", id);
        return MapToResponse(booking);
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        // ISO weeks start on Monday (DayOfWeek.Monday = 1, Sunday = 0)
        var diff = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return date.Date.AddDays(-diff);
    }

    private static BookingResponse MapToResponse(Booking b) =>
        new(b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
            b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
            b.BookingDate, b.Status.ToString(), b.WaitlistPosition,
            b.CheckInTime, b.CancellationDate, b.CancellationReason,
            b.CreatedAt, b.UpdatedAt);
}
