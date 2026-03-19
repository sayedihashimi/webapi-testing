using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
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

    public async Task<BookingResponseDto> CreateAsync(BookingCreateDto dto)
    {
        var member = await _db.Members.FindAsync(dto.MemberId)
            ?? throw new BusinessRuleException("Member not found.", 404);

        if (!member.IsActive)
            throw new BusinessRuleException("Member account is not active.");

        var schedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId)
            ?? throw new BusinessRuleException("Class schedule not found.", 404);

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException($"Cannot book a class with status '{schedule.Status}'.");

        // Active membership required
        var activeMembership = await _db.Memberships
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m =>
                m.MemberId == dto.MemberId && m.Status == MembershipStatus.Active);

        if (activeMembership is null)
            throw new BusinessRuleException("An active membership is required to book classes. Frozen, expired, or cancelled memberships cannot be used for booking.");

        var now = DateTime.UtcNow;

        // Booking window: 7 days in advance max, at least 30 minutes before start
        if (schedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book classes more than 7 days in advance.");
        if (schedule.StartTime <= now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book a class less than 30 minutes before start time.");

        // Premium class access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException("Your current membership plan does not allow access to premium classes. Please upgrade to Premium or Elite.");

        // Weekly booking limit
        var plan = activeMembership.MembershipPlan;
        if (plan.MaxClassBookingsPerWeek != -1)
        {
            var isoWeekStart = GetIsoWeekStart(now);
            var isoWeekEnd = isoWeekStart.AddDays(7);

            var weeklyBookings = await _db.Bookings
                .Include(b => b.ClassSchedule)
                .CountAsync(b =>
                    b.MemberId == dto.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.ClassSchedule.StartTime >= isoWeekStart &&
                    b.ClassSchedule.StartTime < isoWeekEnd);

            if (weeklyBookings >= plan.MaxClassBookingsPerWeek)
                throw new BusinessRuleException($"Weekly booking limit reached ({plan.MaxClassBookingsPerWeek} per week for {plan.Name} plan).");
        }

        // No double booking (time overlap)
        var hasOverlap = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b =>
                b.MemberId == dto.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                b.ClassSchedule.StartTime < schedule.EndTime &&
                b.ClassSchedule.EndTime > schedule.StartTime);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking for an overlapping class time.");

        // Check for duplicate booking on same class
        var existingBooking = await _db.Bookings.AnyAsync(b =>
            b.MemberId == dto.MemberId &&
            b.ClassScheduleId == dto.ClassScheduleId &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted));

        if (existingBooking)
            throw new BusinessRuleException("You already have a booking for this class.");

        // Create booking
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
            booking.WaitlistPosition = schedule.WaitlistCount + 1;
            schedule.WaitlistCount++;
        }

        schedule.UpdatedAt = DateTime.UtcNow;
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created booking {BookingId} for member {MemberId} on class {ClassId}. Status: {Status}",
            booking.Id, dto.MemberId, dto.ClassScheduleId, booking.Status);

        return await GetByIdAsync(booking.Id) ?? throw new BusinessRuleException("Failed to create booking.");
    }

    public async Task<BookingResponseDto?> GetByIdAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id);
        return booking is null ? null : MapToDto(booking);
    }

    public async Task<BookingResponseDto> CancelAsync(int id, CancelBookingDto? dto)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new BusinessRuleException("Booking not found.", 404);

        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessRuleException("Booking is already cancelled.");
        if (booking.Status == BookingStatus.Attended)
            throw new BusinessRuleException("Cannot cancel an attended booking.");
        if (booking.Status == BookingStatus.NoShow)
            throw new BusinessRuleException("Cannot cancel a no-show booking.");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        if (classStart <= now)
            throw new BusinessRuleException("Cannot cancel a class that has already started or completed.");

        var isLateCancellation = (classStart - now).TotalHours < 2;
        var reason = dto?.Reason ?? (isLateCancellation ? "Late cancellation (less than 2 hours before class)" : "Cancelled by member");

        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;
        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var oldWaitlistPos = booking.WaitlistPosition;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;
        booking.WaitlistPosition = null;
        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;

            // Promote from waitlist
            var nextWaitlisted = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                            b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (nextWaitlisted != null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = now;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Reorder remaining waitlist
                var remainingWaitlisted = await _db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                                b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync();

                for (int i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                    remainingWaitlisted[i].UpdatedAt = now;
                }

                _logger.LogInformation("Promoted booking {BookingId} from waitlist for class {ClassId}",
                    nextWaitlisted.Id, booking.ClassScheduleId);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;

            // Reorder waitlist
            var remaining = await _db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
                            b.Status == BookingStatus.Waitlisted &&
                            b.Id != id)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync();

            for (int i = 0; i < remaining.Count; i++)
            {
                remaining[i].WaitlistPosition = i + 1;
                remaining[i].UpdatedAt = now;
            }
        }

        booking.ClassSchedule.UpdatedAt = now;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Cancelled booking {BookingId}. Late: {IsLate}", id, isLateCancellation);
        return MapToDto(booking);
    }

    public async Task<BookingResponseDto> CheckInAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new BusinessRuleException("Booking not found.", 404);

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Only confirmed bookings can be checked in. Current status: {booking.Status}");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        // Check-in window: 15 minutes before to 15 minutes after class start
        if (now < classStart.AddMinutes(-15))
            throw new BusinessRuleException("Check-in is not yet available. You can check in starting 15 minutes before class.");
        if (now > classStart.AddMinutes(15))
            throw new BusinessRuleException("Check-in window has closed. Check-in is available up to 15 minutes after class start.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Member {MemberId} checked in for booking {BookingId}", booking.MemberId, id);
        return MapToDto(booking);
    }

    public async Task<BookingResponseDto> MarkNoShowAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new BusinessRuleException("Booking not found.", 404);

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException($"Only confirmed bookings can be marked as no-show. Current status: {booking.Status}");

        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        if (now < classStart.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Marked booking {BookingId} as no-show", id);
        return MapToDto(booking);
    }

    internal static BookingResponseDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassName = b.ClassSchedule?.ClassType?.Name ?? string.Empty,
        ClassStartTime = b.ClassSchedule?.StartTime ?? default,
        ClassEndTime = b.ClassSchedule?.EndTime ?? default,
        Room = b.ClassSchedule?.Room ?? string.Empty,
        InstructorName = b.ClassSchedule?.Instructor != null
            ? $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}"
            : string.Empty,
        MemberId = b.MemberId,
        MemberName = b.Member != null ? $"{b.Member.FirstName} {b.Member.LastName}" : string.Empty,
        BookingDate = b.BookingDate,
        Status = b.Status.ToString(),
        WaitlistPosition = b.WaitlistPosition,
        CheckInTime = b.CheckInTime,
        CancellationDate = b.CancellationDate,
        CancellationReason = b.CancellationReason,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var diff = (dayOfWeek == 0 ? 6 : dayOfWeek - 1); // Monday = 0
        return date.Date.AddDays(-diff);
    }
}
