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
            ?? throw new KeyNotFoundException($"Member {request.MemberId} not found.");

        if (!member.IsActive)
            throw new ArgumentException("Cannot book for an inactive member.");

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new KeyNotFoundException($"Class schedule {request.ClassScheduleId} not found.");

        if (schedule.Status != ClassScheduleStatus.Scheduled)
            throw new ArgumentException($"Cannot book a class with status '{schedule.Status}'.");

        // Booking window: up to 7 days in advance and no less than 30 min before start
        var now = DateTime.UtcNow;
        if (schedule.StartTime > now.AddDays(7))
            throw new ArgumentException("Cannot book a class more than 7 days in advance.");

        if (schedule.StartTime <= now.AddMinutes(30))
            throw new ArgumentException("Cannot book a class less than 30 minutes before start.");

        // Active membership required
        var activeMembership = await db.Memberships
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.MemberId == request.MemberId && ms.Status == MembershipStatus.Active, ct)
            ?? throw new ArgumentException("Member does not have an active membership.");

        // Premium class access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new ArgumentException("Member's plan does not allow premium class bookings.");

        // Weekly booking limit
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            var startOfWeek = GetStartOfIsoWeek(now);
            var endOfWeek = startOfWeek.AddDays(7);

            var weeklyCount = await db.Bookings.CountAsync(b =>
                b.MemberId == request.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                b.ClassSchedule.StartTime >= startOfWeek &&
                b.ClassSchedule.StartTime < endOfWeek, ct);

            if (weeklyCount >= maxPerWeek)
                throw new ArgumentException($"Member has reached the weekly booking limit of {maxPerWeek} classes.");
        }

        // No double booking - check for overlapping confirmed classes
        var hasOverlap = await db.Bookings.AnyAsync(b =>
            b.MemberId == request.MemberId &&
            b.Status == BookingStatus.Confirmed &&
            b.ClassSchedule.StartTime < schedule.EndTime &&
            b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
            throw new InvalidOperationException("Member already has a booking that overlaps with this class time.");

        // Check for existing booking in this class (not cancelled)
        var existingBooking = await db.Bookings.AnyAsync(b =>
            b.ClassScheduleId == request.ClassScheduleId &&
            b.MemberId == request.MemberId &&
            b.Status != BookingStatus.Cancelled, ct);

        if (existingBooking)
            throw new InvalidOperationException("Member already has an active booking for this class.");

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
            booking.WaitlistPosition = schedule.WaitlistCount + 1;
            schedule.WaitlistCount++;
        }

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ScheduleId} with status {Status}",
            booking.Id, member.Id, schedule.Id, booking.Status);

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
            ?? throw new KeyNotFoundException($"Booking {id} not found.");

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Attended or BookingStatus.NoShow)
            throw new InvalidOperationException($"Cannot cancel a booking with status '{booking.Status}'.");

        var now = DateTime.UtcNow;
        var schedule = booking.ClassSchedule;

        if (schedule.Status is ClassScheduleStatus.InProgress or ClassScheduleStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a booking for a class that has started or completed.");

        // Cancellation policy: free cancel 2+ hours before, late cancel < 2 hours
        var hoursUntilStart = (schedule.StartTime - now).TotalHours;
        string? reason = request.Reason;
        if (hoursUntilStart < 2 && hoursUntilStart > 0)
        {
            reason = $"Late cancellation: {reason ?? "No reason provided"}";
        }

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = reason;

        if (wasConfirmed)
        {
            schedule.CurrentEnrollment = Math.Max(0, schedule.CurrentEnrollment - 1);

            // Promote first waitlisted member
            var firstWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (firstWaitlisted is not null)
            {
                firstWaitlisted.Status = BookingStatus.Confirmed;
                firstWaitlisted.WaitlistPosition = null;
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

                logger.LogInformation("Promoted booking {BookingId} from waitlist to confirmed", firstWaitlisted.Id);
            }
        }
        else if (booking.Status == BookingStatus.Waitlisted)
        {
            schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled booking {BookingId}", id);
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> CheckInAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException($"Can only check in confirmed bookings. Current status: {booking.Status}.");

        var now = DateTime.UtcNow;
        var schedule = booking.ClassSchedule;
        var windowStart = schedule.StartTime.AddMinutes(-15);
        var windowEnd = schedule.StartTime.AddMinutes(15);

        if (now < windowStart || now > windowEnd)
            throw new ArgumentException("Check-in is only available 15 minutes before to 15 minutes after class start time.");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Checked in booking {BookingId}", id);
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.Instructor)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new InvalidOperationException($"Can only mark confirmed bookings as no-show. Current status: {booking.Status}.");

        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new ArgumentException("Cannot mark as no-show until 15 minutes after class start time.");

        booking.Status = BookingStatus.NoShow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Marked booking {BookingId} as no-show", id);
        return MapToResponse(booking);
    }

    private static DateTime GetStartOfIsoWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
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
        ScheduleStartTime = b.ClassSchedule?.StartTime ?? default,
        ScheduleEndTime = b.ClassSchedule?.EndTime ?? default,
        Room = b.ClassSchedule?.Room ?? string.Empty,
        InstructorName = b.ClassSchedule?.Instructor is not null ? $"{b.ClassSchedule.Instructor.FirstName} {b.ClassSchedule.Instructor.LastName}" : string.Empty,
        CreatedAt = b.CreatedAt,
        UpdatedAt = b.UpdatedAt
    };
}
