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

    public async Task<BookingDto> GetByIdAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        return MapToDto(booking);
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
    {
        var member = await _db.Members.FindAsync(dto.MemberId)
            ?? throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");

        if (!member.IsActive)
            throw new BusinessRuleException("Member account is not active.");

        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId)
            ?? throw new KeyNotFoundException($"Class schedule with ID {dto.ClassScheduleId} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("Cannot book a class that is not in Scheduled status.");

        var now = DateTime.UtcNow;

        // Booking window: up to 7 days in advance, no less than 30 minutes before
        if (schedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book a class more than 7 days in advance.");

        if (schedule.StartTime <= now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book a class less than 30 minutes before start time.");

        // Active membership required
        var activeMembership = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms =>
                ms.MemberId == dto.MemberId &&
                ms.Status == MembershipStatus.Active);

        if (activeMembership == null)
            throw new BusinessRuleException("Member does not have an active membership. Only active (not frozen, expired, or cancelled) memberships can book classes.");

        // Premium class check
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException("Your membership plan does not allow booking premium classes. Please upgrade to a Premium or Elite plan.");

        // Weekly booking limit
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var weekStart = GetIsoWeekStart(now);
            var weekEnd = weekStart.AddDays(7);

            var weeklyBookings = await _db.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b =>
                    b.MemberId == dto.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.ClassSchedule.StartTime >= weekStart &&
                    b.ClassSchedule.StartTime < weekEnd);

            if (weeklyBookings >= maxPerWeek)
                throw new BusinessRuleException($"Weekly booking limit reached ({maxPerWeek} per week for your plan).");
        }

        // No double booking (time overlap)
        var hasOverlap = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b =>
                b.MemberId == dto.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
                b.ClassSchedule.StartTime < schedule.EndTime &&
                b.ClassSchedule.EndTime > schedule.StartTime);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking for an overlapping class at this time.");

        // Determine if confirmed or waitlisted
        var booking = new Booking
        {
            ClassScheduleId = dto.ClassScheduleId,
            MemberId = dto.MemberId,
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

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created booking (ID {Id}) for member {MemberId} in class {ClassId} - Status: {Status}",
            booking.Id, booking.MemberId, booking.ClassScheduleId, booking.Status);

        return await GetByIdAsync(booking.Id);
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
            throw new BusinessRuleException("Cannot cancel a booking that has been attended.");

        if (booking.Status == BookingStatus.NoShow)
            throw new BusinessRuleException("Cannot cancel a no-show booking.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        if (classStart <= now)
            throw new BusinessRuleException("Cannot cancel a class that has already started or completed.");

        var isLateCancellation = (classStart - now).TotalHours < 2;
        var reason = dto.CancellationReason ?? "";
        if (isLateCancellation)
            reason = string.IsNullOrEmpty(reason)
                ? "Late cancellation (less than 2 hours before class)"
                : $"{reason} [Late cancellation]";

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment = Math.Max(0, booking.ClassSchedule.CurrentEnrollment - 1);

            // Promote first waitlisted member
            var nextWaitlisted = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                            b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (nextWaitlisted != null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount = Math.Max(0, booking.ClassSchedule.WaitlistCount - 1);

                // Reorder waitlist positions
                var remaining = await _db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                                b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync();

                for (int i = 0; i < remaining.Count; i++)
                    remaining[i].WaitlistPosition = i + 1;

                _logger.LogInformation("Promoted booking (ID {Id}) from waitlist to confirmed", nextWaitlisted.Id);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount = Math.Max(0, booking.ClassSchedule.WaitlistCount - 1);

            // Reorder waitlist
            var remaining = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                            b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync();

            for (int i = 0; i < remaining.Count; i++)
                remaining[i].WaitlistPosition = i + 1;
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Cancelled booking (ID {Id}){Late}", booking.Id, isLateCancellation ? " [Late]" : "");
        return MapToDto(booking);
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
            throw new BusinessRuleException("Only confirmed bookings can be checked in.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;
        var windowStart = classStart.AddMinutes(-15);
        var windowEnd = classStart.AddMinutes(15);

        if (now < windowStart)
            throw new BusinessRuleException("Check-in is not open yet. Check-in opens 15 minutes before class start.");

        if (now > windowEnd)
            throw new BusinessRuleException("Check-in window has closed. Check-in is available up to 15 minutes after class start.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Member {MemberId} checked in to booking (ID {Id})", booking.MemberId, booking.Id);
        return MapToDto(booking);
    }

    public async Task<BookingDto> MarkNoShowAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be marked as no-show.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        if (now < classStart.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Marked booking (ID {Id}) as no-show", booking.Id);
        return MapToDto(booking);
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        var diff = (7 + (dayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassName = b.ClassSchedule?.ClassType?.Name ?? "",
        ClassStartTime = b.ClassSchedule?.StartTime ?? default,
        ClassEndTime = b.ClassSchedule?.EndTime ?? default,
        Room = b.ClassSchedule?.Room ?? "",
        InstructorName = b.ClassSchedule?.Instructor != null
            ? $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}" : "",
        MemberId = b.MemberId,
        MemberName = b.Member != null ? $"{b.Member.FirstName} {b.Member.LastName}" : "",
        BookingDate = b.BookingDate,
        Status = b.Status,
        WaitlistPosition = b.WaitlistPosition,
        CheckInTime = b.CheckInTime,
        CancellationDate = b.CancellationDate,
        CancellationReason = b.CancellationReason,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
