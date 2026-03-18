using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        if (!member.IsActive)
            throw new ArgumentException("Member is not active.");

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {request.ClassScheduleId} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new ArgumentException("Class is not available for booking.");

        // Rule 1: Booking window - up to 7 days in advance, no less than 30 min before
        var now = DateTime.UtcNow;
        if (schedule.StartTime > now.AddDays(7))
            throw new ArgumentException("Cannot book more than 7 days in advance.");

        if (schedule.StartTime <= now.AddMinutes(30))
            throw new ArgumentException("Cannot book less than 30 minutes before class start time.");

        // Rule 6: Active membership required
        var activeMembership = await db.Memberships.AsNoTracking()
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms =>
                ms.MemberId == request.MemberId &&
                ms.Status == MembershipStatus.Active, ct)
            ?? throw new ArgumentException("Member does not have an active membership.");

        // Rule 4: Premium class access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new ArgumentException("Your membership plan does not allow premium class bookings.");

        // Rule 5: Weekly booking limits
        if (activeMembership.MembershipPlan.MaxClassBookingsPerWeek != -1)
        {
            var startOfWeek = GetStartOfIsoWeek(now);
            var endOfWeek = startOfWeek.AddDays(7);

            var weeklyBookings = await db.Bookings.AsNoTracking()
                .CountAsync(b =>
                    b.MemberId == request.MemberId &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                    b.BookingDate >= startOfWeek &&
                    b.BookingDate < endOfWeek, ct);

            if (weeklyBookings >= activeMembership.MembershipPlan.MaxClassBookingsPerWeek)
                throw new ArgumentException($"Weekly booking limit of {activeMembership.MembershipPlan.MaxClassBookingsPerWeek} reached.");
        }

        // Rule 7: No double booking (overlapping class times)
        var hasOverlap = await db.Bookings.AsNoTracking()
            .Where(b => b.MemberId == request.MemberId &&
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended))
            .Join(db.ClassSchedules, b => b.ClassScheduleId, cs => cs.Id, (b, cs) => cs)
            .AnyAsync(cs =>
                cs.StartTime < schedule.EndTime &&
                cs.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
            throw new InvalidOperationException("Member already has a booking for an overlapping class time.");

        // Check if already booked for this specific class
        var existingBooking = await db.Bookings.AnyAsync(b =>
            b.MemberId == request.MemberId &&
            b.ClassScheduleId == request.ClassScheduleId &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Waitlisted), ct);

        if (existingBooking)
            throw new InvalidOperationException("Member already has a booking for this class.");

        // Rule 2: Capacity management
        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId
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

        logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ClassId} with status {Status}",
            booking.Id, booking.MemberId, booking.ClassScheduleId, booking.Status);

        return await GetByIdAsync(booking.Id, ct) ?? throw new InvalidOperationException("Failed to retrieve created booking.");
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings.AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking is null ? null : MapToResponse(booking);
    }

    public async Task<BookingResponse> CancelAsync(int id, CancelBookingRequest request, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Attended)
            throw new InvalidOperationException("Cannot cancel an attended booking.");

        if (booking.Status == BookingStatus.NoShow)
            throw new InvalidOperationException("Cannot cancel a no-show booking.");

        // Rule 3: Cannot cancel started/completed class
        if (booking.ClassSchedule.Status == ClassScheduleStatus.InProgress)
            throw new InvalidOperationException("Cannot cancel booking for a class that has already started.");

        if (booking.ClassSchedule.Status == ClassScheduleStatus.Completed)
            throw new InvalidOperationException("Cannot cancel booking for a completed class.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = DateTime.UtcNow;
        booking.CancellationReason = request.CancellationReason;
        booking.UpdatedAt = DateTime.UtcNow;

        if (wasConfirmed)
        {
            booking.ClassSchedule.CurrentEnrollment--;

            // Promote first waitlisted
            var nextWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (nextWaitlisted is not null)
            {
                nextWaitlisted.Status = BookingStatus.Confirmed;
                nextWaitlisted.WaitlistPosition = null;
                nextWaitlisted.UpdatedAt = DateTime.UtcNow;
                booking.ClassSchedule.CurrentEnrollment++;
                booking.ClassSchedule.WaitlistCount--;

                // Reorder remaining waitlist positions
                var remainingWaitlisted = await db.Bookings
                    .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (int i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                }

                logger.LogInformation("Promoted booking {BookingId} from waitlist", nextWaitlisted.Id);
            }
        }
        else if (wasWaitlisted)
        {
            booking.ClassSchedule.WaitlistCount--;

            // Reorder waitlist
            var remainingWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == booking.ClassScheduleId && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            for (int i = 0; i < remainingWaitlisted.Count; i++)
            {
                remainingWaitlisted[i].WaitlistPosition = i + 1;
            }
        }

        booking.ClassSchedule.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled booking {BookingId}", booking.Id);
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be checked in.");

        // Rule 11: Check-in window - 15 min before to 15 min after class start
        var now = DateTime.UtcNow;
        var windowStart = booking.ClassSchedule.StartTime.AddMinutes(-15);
        var windowEnd = booking.ClassSchedule.StartTime.AddMinutes(15);

        if (now < windowStart)
            throw new ArgumentException("Check-in is not yet available. You can check in starting 15 minutes before class.");

        if (now > windowEnd)
            throw new ArgumentException("Check-in window has passed. Check-in is available up to 15 minutes after class start.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Checked in booking {BookingId}", booking.Id);
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be marked as no-show.");

        // Rule 12: Can only mark no-show 15 minutes after class start
        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new ArgumentException("Cannot mark as no-show until 15 minutes after class start.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Marked booking {BookingId} as no-show", booking.Id);
        return MapToResponse(booking);
    }

    private static DateTime GetStartOfIsoWeek(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var daysToMonday = (dayOfWeek == 0 ? 6 : dayOfWeek - 1);
        return date.Date.AddDays(-daysToMonday);
    }

    private static BookingResponse MapToResponse(Booking b) => new()
    {
        Id = b.Id,
        ClassScheduleId = b.ClassScheduleId,
        ClassName = b.ClassSchedule?.ClassType?.Name ?? string.Empty,
        MemberId = b.MemberId,
        MemberName = b.Member is not null ? $"{b.Member.FirstName} {b.Member.LastName}" : string.Empty,
        BookingDate = b.BookingDate,
        Status = b.Status,
        WaitlistPosition = b.WaitlistPosition,
        CheckInTime = b.CheckInTime,
        CancellationDate = b.CancellationDate,
        CancellationReason = b.CancellationReason,
        ClassStartTime = b.ClassSchedule?.StartTime ?? default,
        ClassEndTime = b.ClassSchedule?.EndTime ?? default,
        Room = b.ClassSchedule?.Room ?? string.Empty,
        InstructorName = b.ClassSchedule?.Instructor is not null
            ? $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}"
            : string.Empty,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
