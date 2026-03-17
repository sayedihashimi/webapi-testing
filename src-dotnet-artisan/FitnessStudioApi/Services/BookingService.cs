using System.Globalization;
using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.Services;

public class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id);
        return booking is null ? null : ToDto(booking);
    }

    public async Task<(BookingDto? Result, string? Error)> CreateAsync(CreateBookingDto dto)
    {
        var member = await db.Members.FindAsync(dto.MemberId);
        if (member is null) return (null, "Member not found.");
        if (!member.IsActive) return (null, "Member is not active.");

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId);
        if (schedule is null) return (null, "Class schedule not found.");
        if (schedule.Status != ClassScheduleStatus.Scheduled)
            return (null, "Class is not available for booking.");

        var now = DateTime.UtcNow;

        // Booking window: up to 7 days in advance, no less than 30 minutes before
        if (schedule.StartTime > now.AddDays(7))
            return (null, "Cannot book classes more than 7 days in advance.");
        if (schedule.StartTime < now.AddMinutes(30))
            return (null, "Cannot book classes less than 30 minutes before start time.");

        // Active membership required
        var membership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .Where(ms => ms.MemberId == dto.MemberId && ms.Status == MembershipStatus.Active)
            .FirstOrDefaultAsync();
        if (membership is null)
            return (null, "Member does not have an active membership.");

        // Premium class access
        if (schedule.ClassType.IsPremium && !membership.MembershipPlan.AllowsPremiumClasses)
            return (null, "Your membership plan does not allow premium class bookings.");

        // Weekly booking limit
        var maxPerWeek = membership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(now);
            var weekEnd = weekStart.AddDays(7);
            var weeklyBookings = await db.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b => b.MemberId == dto.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.ClassSchedule.StartTime >= weekStart && b.ClassSchedule.StartTime < weekEnd);
            if (weeklyBookings >= maxPerWeek)
                return (null, $"Weekly booking limit of {maxPerWeek} reached.");
        }

        // No double booking (overlapping classes)
        var hasOverlap = await db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == dto.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                b.ClassSchedule.StartTime < schedule.EndTime && b.ClassSchedule.EndTime > schedule.StartTime);
        if (hasOverlap)
            return (null, "You already have a booking for an overlapping class.");

        // Already booked this class?
        var alreadyBooked = await db.Bookings
            .AnyAsync(b => b.MemberId == dto.MemberId && b.ClassScheduleId == dto.ClassScheduleId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted));
        if (alreadyBooked)
            return (null, "You already have a booking for this class.");

        // Create booking: confirmed or waitlisted
        var booking = new Booking
        {
            ClassScheduleId = dto.ClassScheduleId,
            MemberId = dto.MemberId
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
        await db.SaveChangesAsync();

        logger.LogInformation("Created booking {BookingId} (Status={Status}) for member {MemberId}, class {ClassId}",
            booking.Id, booking.Status, dto.MemberId, dto.ClassScheduleId);

        var result = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstAsync(b => b.Id == booking.Id);
        return (ToDto(result), null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int id, CancelBookingDto dto)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null) return (false, "Booking not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Attended or BookingStatus.NoShow)
            return (false, "Booking cannot be cancelled in its current state.");

        var now = DateTime.UtcNow;
        if (booking.ClassSchedule.Status is ClassScheduleStatus.InProgress or ClassScheduleStatus.Completed)
            return (false, "Cannot cancel booking for a class that has started or completed.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = dto.Reason;

        // Late cancellation tracking (less than 2 hours)
        if (booking.ClassSchedule.StartTime - now < TimeSpan.FromHours(2))
            booking.CancellationReason = (dto.Reason ?? "") + " [Late cancellation]";

        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;
            booking.ClassSchedule.UpdatedAt = now;

            // Promote first waitlisted member
            var nextWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;
                logger.LogInformation("Promoted booking {BookingId} from waitlist for class {ClassId}", nextWaitlisted.Id, booking.ClassScheduleId);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;
            booking.ClassSchedule.UpdatedAt = now;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Cancelled booking {BookingId}", id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CheckInAsync(int id)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null) return (false, "Booking not found.");
        if (booking.Status != BookingStatus.Confirmed)
            return (false, "Only confirmed bookings can be checked in.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Check-in window: 15 min before to 15 min after class start
        if (now < classStart.AddMinutes(-15))
            return (false, "Check-in is not yet available. Opens 15 minutes before class.");
        if (now > classStart.AddMinutes(15))
            return (false, "Check-in window has closed (15 minutes after class start).");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;
        await db.SaveChangesAsync();
        logger.LogInformation("Checked in booking {BookingId}", id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> MarkNoShowAsync(int id)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (booking is null) return (false, "Booking not found.");
        if (booking.Status != BookingStatus.Confirmed)
            return (false, "Only confirmed bookings can be marked as no-show.");

        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            return (false, "Cannot mark no-show until 15 minutes after class start.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;
        await db.SaveChangesAsync();
        logger.LogInformation("Marked booking {BookingId} as no-show", id);
        return (true, null);
    }

    internal static BookingDto ToDto(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition,
        b.CheckInTime, b.CancellationDate, b.CancellationReason,
        b.CreatedAt, b.UpdatedAt);

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        var diff = (7 + (dayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}
