using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using System.Globalization;

namespace FitnessStudioApi.Services;

public class BookingService
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
            ?? throw new BusinessRuleException($"Member with ID {dto.MemberId} not found.", 404);

        if (!member.IsActive)
            throw new BusinessRuleException("Member account is not active.");

        var classSchedule = await _db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId)
            ?? throw new BusinessRuleException($"Class schedule with ID {dto.ClassScheduleId} not found.", 404);

        if (classSchedule.Status != ClassScheduleStatus.Scheduled)
            throw new BusinessRuleException("Cannot book a class that is not scheduled.");

        // Rule 6: Active membership required
        var activeMembership = await _db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms =>
                ms.MemberId == dto.MemberId &&
                ms.Status == MembershipStatus.Active);

        if (activeMembership == null)
        {
            var hasFrozen = await _db.Memberships.AnyAsync(ms =>
                ms.MemberId == dto.MemberId && ms.Status == MembershipStatus.Frozen);

            throw new BusinessRuleException(hasFrozen
                ? "Your membership is currently frozen. Unfreeze it to make bookings."
                : "An active membership is required to book classes.");
        }

        // Rule 4: Premium class access
        if (classSchedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new BusinessRuleException(
                $"Your {activeMembership.MembershipPlan.Name} membership does not include access to premium classes. Please upgrade to a plan that includes premium class access.");

        // Rule 1: Booking window (7 days ahead, no less than 30 min before)
        var now = DateTime.UtcNow;
        if (classSchedule.StartTime > now.AddDays(7))
            throw new BusinessRuleException("Cannot book more than 7 days in advance.");

        if (classSchedule.StartTime <= now.AddMinutes(30))
            throw new BusinessRuleException("Cannot book less than 30 minutes before class starts.");

        // Rule 7: No double booking (overlapping classes)
        var hasOverlap = await _db.Bookings
            .Include(b => b.ClassSchedule)
            .AnyAsync(b =>
                b.MemberId == dto.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted) &&
                b.ClassSchedule.StartTime < classSchedule.EndTime &&
                b.ClassSchedule.EndTime > classSchedule.StartTime);

        if (hasOverlap)
            throw new BusinessRuleException("You already have a booking for an overlapping class.", 409);

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
                throw new BusinessRuleException(
                    $"Weekly booking limit reached ({plan.MaxClassBookingsPerWeek} classes per week for {plan.Name} plan).");
        }

        // Rule 2: Capacity management
        var booking = new Booking
        {
            ClassScheduleId = dto.ClassScheduleId,
            MemberId = dto.MemberId
        };

        if (classSchedule.CurrentEnrollment < classSchedule.Capacity)
        {
            booking.Status = BookingStatus.Confirmed;
            classSchedule.CurrentEnrollment++;
        }
        else
        {
            booking.Status = BookingStatus.Waitlisted;
            booking.WaitlistPosition = classSchedule.WaitlistCount + 1;
            classSchedule.WaitlistCount++;
        }

        classSchedule.UpdatedAt = DateTime.UtcNow;
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ClassId} with status {Status}",
            booking.Id, dto.MemberId, dto.ClassScheduleId, booking.Status);

        return await GetByIdAsync(booking.Id) ?? throw new InvalidOperationException();
    }

    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id);

        return booking == null ? null : MapToDto(booking);
    }

    public async Task<BookingDto> CancelAsync(int id, CancelBookingDto dto)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new BusinessRuleException($"Booking with ID {id} not found.", 404);

        if (booking.Status == BookingStatus.Cancelled)
            throw new BusinessRuleException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Attended || booking.Status == BookingStatus.NoShow)
            throw new BusinessRuleException("Cannot cancel a completed booking.");

        var cs = booking.ClassSchedule;

        // Rule 3: Cannot cancel started/completed class
        if (cs.Status == ClassScheduleStatus.InProgress)
            throw new BusinessRuleException("Cannot cancel booking for a class that has already started.");

        if (cs.Status == ClassScheduleStatus.Completed)
            throw new BusinessRuleException("Cannot cancel booking for a completed class.");

        var now = DateTime.UtcNow;
        var hoursUntilStart = (cs.StartTime - now).TotalHours;

        // Late cancellation warning
        var reason = dto.Reason ?? string.Empty;
        if (hoursUntilStart < 2 && hoursUntilStart > 0)
            reason = string.IsNullOrEmpty(reason) ? "Late cancellation (less than 2 hours before class)" : $"{reason} [Late cancellation]";

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;
        booking.UpdatedAt = now;

        if (wasConfirmed)
        {
            cs.CurrentEnrollment = Math.Max(0, cs.CurrentEnrollment - 1);

            // Promote first waitlisted member
            var firstWaitlisted = await _db.Bookings
                .Where(b => b.ClassScheduleId == cs.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync();

            if (firstWaitlisted != null)
            {
                firstWaitlisted.Status = BookingStatus.Confirmed;
                firstWaitlisted.WaitlistPosition = null;
                firstWaitlisted.UpdatedAt = now;
                cs.CurrentEnrollment++;
                cs.WaitlistCount = Math.Max(0, cs.WaitlistCount - 1);

                // Reorder remaining waitlist
                var remainingWaitlisted = await _db.Bookings
                    .Where(b => b.ClassScheduleId == cs.Id && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync();

                for (int i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                    remainingWaitlisted[i].UpdatedAt = now;
                }

                _logger.LogInformation("Promoted waitlisted booking {BookingId} to confirmed for class {ClassId}",
                    firstWaitlisted.Id, cs.Id);
            }
        }
        else if (wasWaitlisted)
        {
            cs.WaitlistCount = Math.Max(0, cs.WaitlistCount - 1);

            var remainingWaitlisted = await _db.Bookings
                .Where(b => b.ClassScheduleId == cs.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync();

            for (int i = 0; i < remainingWaitlisted.Count; i++)
            {
                remainingWaitlisted[i].WaitlistPosition = i + 1;
                remainingWaitlisted[i].UpdatedAt = now;
            }
        }

        cs.UpdatedAt = now;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Cancelled booking {BookingId}", id);
        return MapToDto(booking);
    }

    public async Task<BookingDto> CheckInAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new BusinessRuleException($"Booking with ID {id} not found.", 404);

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be checked in.");

        // Rule 11: Check-in window (15 min before to 15 min after start)
        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;
        var windowStart = classStart.AddMinutes(-15);
        var windowEnd = classStart.AddMinutes(15);

        if (now < windowStart)
            throw new BusinessRuleException("Check-in is not open yet. Check-in opens 15 minutes before class starts.");

        if (now > windowEnd)
            throw new BusinessRuleException("Check-in window has closed. Check-in is available up to 15 minutes after class starts.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Checked in booking {BookingId} for member {MemberId}", id, booking.MemberId);
        return MapToDto(booking);
    }

    public async Task<BookingDto> NoShowAsync(int id)
    {
        var booking = await _db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new BusinessRuleException($"Booking with ID {id} not found.", 404);

        if (booking.Status != BookingStatus.Confirmed)
            throw new BusinessRuleException("Only confirmed bookings can be marked as no-show.");

        // Rule 12: Must be at least 15 min after start
        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new BusinessRuleException("Cannot mark as no-show until 15 minutes after class starts.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Marked booking {BookingId} as no-show for member {MemberId}", id, booking.MemberId);
        return MapToDto(booking);
    }

    private static DateTime GetIsoWeekStart(DateTime date)
    {
        var dayOfWeek = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        var diff = (7 + (dayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    public static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassTypeName = b.ClassSchedule.ClassType.Name,
        ClassStartTime = b.ClassSchedule.StartTime,
        ClassEndTime = b.ClassSchedule.EndTime,
        Room = b.ClassSchedule.Room,
        InstructorName = $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}",
        MemberId = b.MemberId,
        MemberName = $"{b.Member.FirstName} {b.Member.LastName}",
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
