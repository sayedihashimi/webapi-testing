using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FitnessStudioApi.Services;

public class BookingService : IBookingService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<BookingService> _logger;

    public BookingService(FitnessDbContext db, ILogger<BookingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
    {
        var member = await _db.Members.FindAsync(dto.MemberId)
            ?? throw new NotFoundException($"Member with ID {dto.MemberId} not found.");

        if (!member.IsActive)
            throw new BusinessRuleException("Member account is not active.");

        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId)
            ?? throw new NotFoundException($"Class schedule with ID {dto.ClassScheduleId} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException($"Cannot book a class with status '{schedule.Status}'.");

        // Rule 6: Active membership required
        var activeMembership = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.MemberId == dto.MemberId && ms.Status == MembershipStatus.Active);

        if (activeMembership is null)
            throw new BusinessRuleException("Member does not have an active membership. Frozen, expired, or cancelled memberships cannot book classes.");

        // Rule 4: Premium class tier access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException("Your membership plan does not allow access to premium classes. Please upgrade to Premium or Elite.");

        // Rule 1: Booking window (7 days in advance, 30 min before start)
        var now = DateTime.UtcNow;
        if (schedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book classes more than 7 days in advance.");
        if (schedule.StartTime < now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book a class that starts within 30 minutes or has already started.");

        // Rule 7: No double booking (overlapping classes)
        var hasOverlap = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b => b.MemberId == dto.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
                b.ClassSchedule.StartTime < schedule.EndTime &&
                b.ClassSchedule.EndTime > schedule.StartTime);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking for an overlapping class at this time.");

        // Rule 5: Weekly booking limit
        var plan = activeMembership.MembershipPlan;
        if (plan.MaxClassBookingsPerWeek != -1)
        {
            var isoWeek = ISOWeek.GetWeekOfYear(schedule.StartTime);
            var isoYear = ISOWeek.GetYear(schedule.StartTime);
            var weekStart = ISOWeek.ToDateTime(isoYear, isoWeek, DayOfWeek.Monday);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookings = await _db.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b => b.MemberId == dto.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.ClassSchedule.StartTime >= weekStart && b.ClassSchedule.StartTime < weekEnd);

            if (weeklyBookings >= plan.MaxClassBookingsPerWeek)
                throw new BusinessRuleException($"You have reached your weekly booking limit of {plan.MaxClassBookingsPerWeek} classes for this week.");
        }

        // Determine if confirmed or waitlisted
        BookingStatus status;
        int? waitlistPosition = null;

        if (schedule.CurrentEnrollment < schedule.Capacity)
        {
            status = BookingStatus.Confirmed;
            schedule.CurrentEnrollment++;
        }
        else
        {
            status = BookingStatus.Waitlisted;
            schedule.WaitlistCount++;
            waitlistPosition = schedule.WaitlistCount;
        }

        var booking = new Booking
        {
            ClassScheduleId = dto.ClassScheduleId,
            MemberId = dto.MemberId,
            Status = status,
            WaitlistPosition = waitlistPosition,
            BookingDate = now
        };

        _db.Bookings.Add(booking);
        schedule.UpdatedAt = now;
        await _db.SaveChangesAsync();

        await _db.Entry(booking).Reference(b => b.ClassSchedule).LoadAsync();
        await _db.Entry(booking.ClassSchedule).Reference(cs => cs.ClassType).LoadAsync();
        await _db.Entry(booking).Reference(b => b.Member).LoadAsync();

        _logger.LogInformation("Booking {BookingId} created: {MemberName} → {ClassName} [{Status}]",
            booking.Id, $"{member.FirstName} {member.LastName}", schedule.ClassType.Name, status);

        return ToDto(booking);
    }

    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id);

        return booking is null ? null : ToDto(booking);
    }

    public async Task<BookingDto> CancelAsync(int id, CancelBookingDto dto)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Booking with ID {id} not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Attended or BookingStatus.NoShow)
            throw new BusinessRuleException($"Cannot cancel a booking with status '{booking.Status}'.");

        var now = DateTime.UtcNow;

        // Rule 3: Cannot cancel a class that has started or completed
        if (booking.ClassSchedule.Status is ClassScheduleStatus.InProgress or ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel a booking for a class that has already started or completed.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        // Late cancellation check
        var hoursBeforeClass = (booking.ClassSchedule.StartTime - now).TotalHours;
        string? reason = dto.Reason;
        if (hoursBeforeClass < 2 && hoursBeforeClass > 0)
            reason = $"Late cancellation: {dto.Reason ?? "No reason provided"}";

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;
        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment = Math.Max(0, booking.ClassSchedule.CurrentEnrollment - 1);

            // Promote first waitlisted member
            var nextWaitlisted = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount = Math.Max(0, booking.ClassSchedule.WaitlistCount - 1);

                // Reorder remaining waitlist positions
                var remaining = await _db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync();
                for (int i = 0; i < remaining.Count; i++)
                    remaining[i].WaitlistPosition = i + 1;

                _logger.LogInformation("Waitlisted booking {BookingId} promoted to Confirmed", nextWaitlisted.Id);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount = Math.Max(0, booking.ClassSchedule.WaitlistCount - 1);
            booking.WaitlistPosition = null;

            // Reorder remaining waitlist
            var remaining = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync();
            for (int i = 0; i < remaining.Count; i++)
                remaining[i].WaitlistPosition = i + 1;
        }

        booking.ClassSchedule.UpdatedAt = now;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} cancelled", id);
        return ToDto(booking);
    }

    public async Task<BookingDto> CheckInAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Cannot check in a booking with status '{booking.Status}'. Only confirmed bookings can check in.");

        // Rule 11: Check-in window is 15 min before to 15 min after class start
        var now = DateTime.UtcNow;
        var windowStart = booking.ClassSchedule.StartTime.AddMinutes(-15);
        var windowEnd = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < windowStart)
            throw new BusinessRuleException("Check-in is not available yet. You can check in starting 15 minutes before class.");
        if (now > windowEnd)
            throw new BusinessRuleException("Check-in window has closed. Check-in is only available up to 15 minutes after class start time.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} checked in at {Time}", id, now);
        return ToDto(booking);
    }

    public async Task<BookingDto> MarkNoShowAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new NotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Only confirmed bookings can be marked as no-show. Current status: '{booking.Status}'.");

        // Rule 12: Only flag as no-show after 15 minutes past class start
        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} marked as no-show", id);
        return ToDto(booking);
    }

    internal static BookingDto ToDto(Booking b) => new(
        b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
        b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
        b.BookingDate, b.Status, b.WaitlistPosition, b.CheckInTime,
        b.CancellationDate, b.CancellationReason, b.CreatedAt, b.UpdatedAt);
}
