using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
{
    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        if (!member.IsActive)
            throw new ArgumentException("Inactive members cannot book classes.");

        var schedule = await db.ClassSchedules
            .Include(cs => cs.ClassType)
            .Include(cs => cs.Instructor)
            .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct)
            ?? throw new KeyNotFoundException($"Class schedule with ID {request.ClassScheduleId} not found.");

        if (schedule.Status != ClassStatus.Scheduled)
            throw new ArgumentException("Cannot book a class that is not in Scheduled status.");

        var now = DateTime.UtcNow;

        // Rule 1: Booking window - max 7 days in advance, min 30 min before
        if (schedule.StartTime > now.AddDays(7))
            throw new ArgumentException("Cannot book classes more than 7 days in advance.");

        if (schedule.StartTime <= now.AddMinutes(30))
            throw new ArgumentException("Cannot book classes less than 30 minutes before start time.");

        // Rule 6: Active membership required
        var activeMembership = await db.Memberships
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m =>
                m.MemberId == request.MemberId &&
                m.Status == MembershipStatus.Active, ct)
            ?? throw new ArgumentException("Member does not have an active membership.");

        // Rule 4: Premium class access
        if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
            throw new ArgumentException("Your membership plan does not include access to premium classes.");

        // Rule 5: Weekly booking limits
        var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
        if (maxPerWeek != -1)
        {
            // Count bookings for current ISO week
            var startOfWeek = GetStartOfIsoWeek(now);
            var endOfWeek = startOfWeek.AddDays(7);

            var weeklyBookings = await db.Bookings.CountAsync(b =>
                b.MemberId == request.MemberId &&
                (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
                b.ClassSchedule.StartTime >= startOfWeek &&
                b.ClassSchedule.StartTime < endOfWeek, ct);

            if (weeklyBookings >= maxPerWeek)
                throw new ArgumentException($"Weekly booking limit of {maxPerWeek} reached.");
        }

        // Rule 8: No double booking (check for overlapping classes)
        var hasOverlap = await db.Bookings.AnyAsync(b =>
            b.MemberId == request.MemberId &&
            (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
            b.ClassSchedule.StartTime < schedule.EndTime &&
            b.ClassSchedule.EndTime > schedule.StartTime, ct);

        if (hasOverlap)
            throw new InvalidOperationException("Member already has a booking for an overlapping class.");

        // Determine if confirmed or waitlisted
        var isWaitlisted = schedule.CurrentEnrollment >= schedule.Capacity;

        var booking = new Booking
        {
            ClassScheduleId = request.ClassScheduleId,
            MemberId = request.MemberId,
            BookingDate = now,
            Status = isWaitlisted ? BookingStatus.Waitlisted : BookingStatus.Confirmed,
            WaitlistPosition = isWaitlisted ? schedule.WaitlistCount + 1 : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (isWaitlisted)
            schedule.WaitlistCount++;
        else
            schedule.CurrentEnrollment++;

        schedule.UpdatedAt = now;

        db.Bookings.Add(booking);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ClassId} (Status: {Status})",
            booking.Id, request.MemberId, request.ClassScheduleId, booking.Status);

        return new BookingResponse(
            booking.Id, booking.ClassScheduleId, schedule.ClassType.Name,
            booking.MemberId, $"{member.FirstName} {member.LastName}",
            booking.BookingDate, booking.Status, booking.WaitlistPosition,
            booking.CheckInTime, booking.CancellationDate, booking.CancellationReason,
            schedule.StartTime, schedule.EndTime, schedule.Room,
            booking.CreatedAt, booking.UpdatedAt);
    }

    public async Task<BookingResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .AsNoTracking()
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking is null) return null;

        return MapToResponse(booking);
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
            throw new ArgumentException("Booking is already cancelled.");

        if (booking.Status == BookingStatus.Attended)
            throw new ArgumentException("Cannot cancel an attended booking.");

        // Rule 3: Cannot cancel started or completed classes
        var now = DateTime.UtcNow;
        if (booking.ClassSchedule.Status == ClassStatus.InProgress)
            throw new ArgumentException("Cannot cancel booking for a class that has already started.");

        if (booking.ClassSchedule.Status == ClassStatus.Completed)
            throw new ArgumentException("Cannot cancel booking for a completed class.");

        var wasConfirmed = booking.Status == BookingStatus.Confirmed;
        var wasWaitlisted = booking.Status == BookingStatus.Waitlisted;

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationDate = now;
        booking.CancellationReason = request.Reason;
        booking.UpdatedAt = now;

        var schedule = booking.ClassSchedule;

        if (wasConfirmed)
        {
            schedule.CurrentEnrollment = Math.Max(0, schedule.CurrentEnrollment - 1);

            // Promote first waitlisted person
            var firstWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .FirstOrDefaultAsync(ct);

            if (firstWaitlisted is not null)
            {
                firstWaitlisted.Status = BookingStatus.Confirmed;
                firstWaitlisted.WaitlistPosition = null;
                firstWaitlisted.UpdatedAt = now;
                schedule.CurrentEnrollment++;
                schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

                // Reorder remaining waitlist positions
                var remainingWaitlisted = await db.Bookings
                    .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                    .OrderBy(b => b.WaitlistPosition)
                    .ToListAsync(ct);

                for (var i = 0; i < remainingWaitlisted.Count; i++)
                {
                    remainingWaitlisted[i].WaitlistPosition = i + 1;
                    remainingWaitlisted[i].UpdatedAt = now;
                }

                logger.LogInformation("Promoted booking {BookingId} from waitlist for class {ClassId}",
                    firstWaitlisted.Id, schedule.Id);
            }
        }
        else if (wasWaitlisted)
        {
            schedule.WaitlistCount = Math.Max(0, schedule.WaitlistCount - 1);

            // Reorder waitlist
            var remainingWaitlisted = await db.Bookings
                .Where(b => b.ClassScheduleId == schedule.Id && b.Status == BookingStatus.Waitlisted)
                .OrderBy(b => b.WaitlistPosition)
                .ToListAsync(ct);

            for (var i = 0; i < remainingWaitlisted.Count; i++)
            {
                remainingWaitlisted[i].WaitlistPosition = i + 1;
                remainingWaitlisted[i].UpdatedAt = now;
            }
        }

        schedule.UpdatedAt = now;
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
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new ArgumentException("Only confirmed bookings can be checked in.");

        // Rule 11: Check-in window: 15 min before to 15 min after class start
        var now = DateTime.UtcNow;
        var classStart = booking.ClassSchedule.StartTime;

        if (now < classStart.AddMinutes(-15))
            throw new ArgumentException("Check-in is only available 15 minutes before class starts.");

        if (now > classStart.AddMinutes(15))
            throw new ArgumentException("Check-in window has expired (15 minutes after class start).");

        booking.Status = BookingStatus.Attended;
        booking.CheckInTime = now;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Checked in booking {BookingId}", id);
        return MapToResponse(booking);
    }

    public async Task<BookingResponse> MarkNoShowAsync(int id, CancellationToken ct)
    {
        var booking = await db.Bookings
            .Include(b => b.ClassSchedule).ThenInclude(cs => cs.ClassType)
            .Include(b => b.Member)
            .FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new KeyNotFoundException($"Booking with ID {id} not found.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new ArgumentException("Only confirmed bookings can be marked as no-show.");

        // Rule 12: Can only mark as no-show after 15 min past class start
        var now = DateTime.UtcNow;
        if (now < booking.ClassSchedule.StartTime.AddMinutes(15))
            throw new ArgumentException("Cannot mark as no-show until 15 minutes after class start.");

        booking.Status = BookingStatus.NoShow;
        booking.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Marked booking {BookingId} as no-show", id);
        return MapToResponse(booking);
    }

    private static BookingResponse MapToResponse(Booking b) =>
        new(b.Id, b.ClassScheduleId, b.ClassSchedule.ClassType.Name,
            b.MemberId, $"{b.Member.FirstName} {b.Member.LastName}",
            b.BookingDate, b.Status, b.WaitlistPosition, b.CheckInTime,
            b.CancellationDate, b.CancellationReason,
            b.ClassSchedule.StartTime, b.ClassSchedule.EndTime,
            b.ClassSchedule.Room, b.CreatedAt, b.UpdatedAt);

    private static DateTime GetStartOfIsoWeek(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        // ISO: Monday = 0, Sunday = 6
        var diff = (dayOfWeek == 0 ? 6 : dayOfWeek - 1);
        return date.Date.AddDays(-diff);
    }
}
