using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;
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
            ?? throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");

        if (!member.IsActive)
            throw new BusinessRuleException("Member account is not active.");

        var classSchedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId)
            ?? throw new KeyNotFoundException($"Class schedule with ID {dto.ClassScheduleId} not found.");

        if (classSchedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException($"Cannot book a class with status '{classSchedule.Status}'.");

        // Rule 6: Active membership required
        var activeMembership = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms =>
                ms.MemberId == dto.MemberId &&
                ms.Status == MembershipStatus.Active);

        if (activeMembership == null)
            throw new BusinessRuleException("An active membership is required to book classes. Frozen, expired, or cancelled memberships cannot book.");

        // Rule 1: Booking window (7 days in advance, no less than 30 minutes before)
        var now = DateTime.UtcNow;
        if (classSchedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book classes more than 7 days in advance.");

        if (classSchedule.StartTime < now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book a class less than 30 minutes before start time.");

        // Rule 4: Premium class access
        if (classSchedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException("Your membership plan does not allow access to premium classes. Please upgrade to Premium or Elite.");

        // Rule 7: No double booking (time overlap)
        var hasOverlap = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b =>
                b.MemberId == dto.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
                b.ClassSchedule.StartTime < classSchedule.EndTime &&
                b.ClassSchedule.EndTime > classSchedule.StartTime);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking for a class that overlaps with this time slot.");

        // Rule 5: Weekly booking limits
        var plan = activeMembership.MembershipPlan;
        if (plan.MaxClassBookingsPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(classSchedule.StartTime);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookingCount = await _db.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b =>
                    b.MemberId == dto.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.ClassSchedule.StartTime >= weekStart &&
                    b.ClassSchedule.StartTime < weekEnd);

            if (weeklyBookingCount >= plan.MaxClassBookingsPerWeek)
                throw new BusinessRuleException($"You have reached your weekly booking limit of {plan.MaxClassBookingsPerWeek} classes for your {plan.Name} plan.");
        }

        // Rule 2: Capacity management
        var booking = new Booking
        {
            ClassScheduleId = dto.ClassScheduleId,
            MemberId = dto.MemberId,
            BookingDate = DateTime.UtcNow
        };

        if (classSchedule.CurrentEnrollment < classSchedule.Capacity)
        {
            booking.Status = BookingStatus.Confirmed;
            classSchedule.CurrentEnrollment++;
        }
        else
        {
            booking.Status = BookingStatus.Waitlisted;
            classSchedule.WaitlistCount++;
            booking.WaitlistPosition = classSchedule.WaitlistCount;
        }

        classSchedule.UpdatedAt = DateTime.UtcNow;
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking created: Member {MemberId} for class {ClassId}, Status: {Status}",
            dto.MemberId, dto.ClassScheduleId, booking.Status);

        return await GetByIdAsync(booking.Id);
    }

    public async Task<BookingDto> GetByIdAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        return MemberService.MapBookingToDto(booking);
    }

    public async Task<BookingDto> CancelAsync(int id, CancelBookingDto dto)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessRuleException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Attended)
            throw new BusinessRuleException("Cannot cancel a booking that has already been attended.");

        if (booking.Status == BookingStatus.NoShow)
            throw new BusinessRuleException("Cannot cancel a no-show booking.");

        var now = DateTime.UtcNow;
        if (booking.ClassSchedule.StartTime <= now)
            throw new BusinessRuleException("Cannot cancel a class that has already started or completed.");

        // Rule 3: Cancellation policy
        var hoursUntilClass = (booking.ClassSchedule.StartTime - now).TotalHours;
        var cancellationReason = dto.CancellationReason ?? "";
        if (hoursUntilClass < 2)
        {
            cancellationReason = $"[Late cancellation] {cancellationReason}".Trim();
        }

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = DateTime.UtcNow;
        booking.CancellationReason = cancellationReason;
        booking.UpdatedAt = DateTime.UtcNow;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;
            // Promote first waitlisted person
            await PromoteFromWaitlistAsync(booking.ClassScheduleId);
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;
            // Reorder waitlist positions
            await ReorderWaitlistAsync(booking.ClassScheduleId);
        }

        booking.ClassSchedule.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking {BookingId} cancelled for member {MemberId}", id, booking.MemberId);
        return MemberService.MapBookingToDto(booking);
    }

    public async Task<BookingDto> CheckInAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Only confirmed bookings can check in. Current status: {booking.Status}");

        // Rule 11: Check-in window (15 min before to 15 min after start)
        var now = DateTime.UtcNow;
        var windowStart = booking.ClassSchedule.StartTime.AddMinutes(-15);
        var windowEnd = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < windowStart)
            throw new BusinessRuleException("Check-in is not yet available. Check-in opens 15 minutes before class start time.");

        if (now > windowEnd)
            throw new BusinessRuleException("Check-in window has closed. Check-in is only available up to 15 minutes after class start time.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Member {MemberId} checked in for booking {BookingId}", booking.MemberId, id);
        return MemberService.MapBookingToDto(booking);
    }

    public async Task<BookingDto> NoShowAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Only confirmed bookings can be marked as no-show. Current status: {booking.Status}");

        // Rule 12: Can mark as no-show after 15 min past class start
        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Booking {BookingId} marked as no-show for member {MemberId}", id, booking.MemberId);
        return MemberService.MapBookingToDto(booking);
    }

    private async Task PromoteFromWaitlistAsync(int classScheduleId)
    {
        var nextInLine = await _db.Bookings
            .Where(b => b.ClassScheduleId == classScheduleId && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .FirstOrDefaultAsync();

        if (nextInLine != null)
        {
            nextInLine.Status = BookingStatus.Confirmed;
            nextInLine.WaitlistPosition = null;
            nextInLine.UpdatedAt = DateTime.UtcNow;

            var schedule = await _db.ClassSchedules.FindAsync(classScheduleId);
            if (schedule != null)
            {
                schedule.CurrentEnrollment++;
                schedule.WaitlistCount--;
            }

            _logger.LogInformation("Promoted booking {BookingId} from waitlist to confirmed", nextInLine.Id);

            // Reorder remaining waitlist
            await ReorderWaitlistAsync(classScheduleId);
        }
    }

    private async Task ReorderWaitlistAsync(int classScheduleId)
    {
        var waitlisted = await _db.Bookings
            .Where(b => b.ClassScheduleId == classScheduleId && b.Status == BookingStatus.Waitlisted)
            .OrderBy(b => b.WaitlistPosition)
            .ToListAsync();

        for (int i = 0; i < waitlisted.Count; i++)
        {
            waitlisted[i].WaitlistPosition = i + 1;
        }
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        // ISO week starts on Monday (DayOfWeek.Monday = 1, Sunday = 0)
        var diff = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return date.Date.AddDays(-diff);
    }
}
