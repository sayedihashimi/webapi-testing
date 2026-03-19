using FitnessStudioApi.Data;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class DatabaseSeeder
{
    public static async Task SeedRuntimeDataAsync(StudioDbContext db)
    {
        if (await db.ClassSchedules.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Memberships
        var memberships = new List<Membership>
        {
            new() { Id = 1, MemberId = 1, MembershipPlanId = 3, StartDate = new DateOnly(2024, 6, 1), EndDate = new DateOnly(2025, 6, 1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, MemberId = 2, MembershipPlanId = 2, StartDate = new DateOnly(2024, 7, 15), EndDate = new DateOnly(2024, 10, 15), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, MemberId = 2, MembershipPlanId = 2, StartDate = today.AddDays(-30), EndDate = today.AddDays(60), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, MemberId = 3, MembershipPlanId = 1, StartDate = today.AddDays(-15), EndDate = today.AddDays(15), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, MemberId = 4, MembershipPlanId = 3, StartDate = today.AddDays(-60), EndDate = today.AddDays(305), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, MemberId = 5, MembershipPlanId = 2, StartDate = today.AddDays(-10), EndDate = today.AddDays(80), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, MemberId = 6, MembershipPlanId = 1, StartDate = today.AddDays(-20), EndDate = today.AddDays(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, MemberId = 7, MembershipPlanId = 3, StartDate = today.AddDays(-90), EndDate = today.AddDays(275), Status = MembershipStatus.Frozen, PaymentStatus = PaymentStatus.Paid, FreezeStartDate = today.AddDays(-5), FreezeEndDate = today.AddDays(9), CreatedAt = now, UpdatedAt = now },
        };
        db.Memberships.AddRange(memberships);

        // Class Schedules — spread over next 7 days
        var baseDate = now.Date;
        var schedules = new List<ClassSchedule>
        {
            new() { Id = 1, ClassTypeId = 1, InstructorId = 1, StartTime = baseDate.AddDays(1).AddHours(7), EndTime = baseDate.AddDays(1).AddHours(8), Capacity = 25, CurrentEnrollment = 3, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddDays(1).AddHours(9), EndTime = baseDate.AddDays(1).AddHours(9).AddMinutes(45), Capacity = 20, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, ClassTypeId = 3, InstructorId = 4, StartTime = baseDate.AddDays(1).AddHours(17), EndTime = baseDate.AddDays(1).AddHours(17).AddMinutes(45), Capacity = 15, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 4, ClassTypeId = 4, InstructorId = 3, StartTime = baseDate.AddDays(2).AddHours(8), EndTime = baseDate.AddDays(2).AddHours(9), Capacity = 12, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio C", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, ClassTypeId = 5, InstructorId = 2, StartTime = baseDate.AddDays(2).AddHours(18), EndTime = baseDate.AddDays(2).AddHours(19), Capacity = 16, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 6, ClassTypeId = 6, InstructorId = 1, StartTime = baseDate.AddDays(3).AddHours(6).AddMinutes(30), EndTime = baseDate.AddDays(3).AddHours(7), Capacity = 30, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, ClassTypeId = 1, InstructorId = 1, StartTime = baseDate.AddDays(3).AddHours(10), EndTime = baseDate.AddDays(3).AddHours(11), Capacity = 25, CurrentEnrollment = 2, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, ClassTypeId = 2, InstructorId = 2, StartTime = baseDate.AddDays(4).AddHours(7), EndTime = baseDate.AddDays(4).AddHours(7).AddMinutes(45), Capacity = 20, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, ClassTypeId = 3, InstructorId = 4, StartTime = baseDate.AddDays(4).AddHours(17), EndTime = baseDate.AddDays(4).AddHours(17).AddMinutes(45), Capacity = 2, CurrentEnrollment = 2, WaitlistCount = 1, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, ClassTypeId = 4, InstructorId = 3, StartTime = baseDate.AddDays(5).AddHours(8), EndTime = baseDate.AddDays(5).AddHours(9), Capacity = 12, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio C", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, ClassTypeId = 5, InstructorId = 2, StartTime = baseDate.AddDays(6).AddHours(18), EndTime = baseDate.AddDays(6).AddHours(19), Capacity = 16, CurrentEnrollment = 1, WaitlistCount = 0, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new() { Id = 12, ClassTypeId = 1, InstructorId = 3, StartTime = baseDate.AddDays(6).AddHours(9), EndTime = baseDate.AddDays(6).AddHours(10), Capacity = 25, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now },
            new() { Id = 13, ClassTypeId = 6, InstructorId = 1, StartTime = baseDate.AddDays(7).AddHours(6).AddMinutes(30), EndTime = baseDate.AddDays(7).AddHours(7), Capacity = 30, CurrentEnrollment = 0, WaitlistCount = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
        };
        db.ClassSchedules.AddRange(schedules);

        // Bookings
        var bookings = new List<Booking>
        {
            // Yoga class (schedule 1) — 3 confirmed
            new() { Id = 1, ClassScheduleId = 1, MemberId = 1, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 2, ClassScheduleId = 1, MemberId = 2, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 3, ClassScheduleId = 1, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // HIIT class (schedule 2) — 2 confirmed
            new() { Id = 4, ClassScheduleId = 2, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 5, ClassScheduleId = 2, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Spin (schedule 3) — 2 confirmed
            new() { Id = 6, ClassScheduleId = 3, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, ClassScheduleId = 3, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Pilates premium (schedule 4) — 2 confirmed
            new() { Id = 8, ClassScheduleId = 4, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, ClassScheduleId = 4, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Boxing premium (schedule 5) — 2 confirmed
            new() { Id = 10, ClassScheduleId = 5, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 11, ClassScheduleId = 5, MemberId = 2, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Meditation (schedule 6)
            new() { Id = 12, ClassScheduleId = 6, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Yoga (schedule 7) — various statuses
            new() { Id = 13, ClassScheduleId = 7, MemberId = 3, BookingDate = now.AddDays(-2), Status = BookingStatus.Cancelled, CancellationDate = now.AddDays(-1), CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },
            new() { Id = 14, ClassScheduleId = 7, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Full spin class (schedule 9) — at capacity with waitlist
            new() { Id = 15, ClassScheduleId = 9, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 16, ClassScheduleId = 9, MemberId = 4, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { Id = 17, ClassScheduleId = 9, MemberId = 5, BookingDate = now, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, CreatedAt = now, UpdatedAt = now },
            // Pilates (schedule 10)
            new() { Id = 18, ClassScheduleId = 10, MemberId = 5, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // Boxing (schedule 11) — attended
            new() { Id = 19, ClassScheduleId = 11, MemberId = 1, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            // HIIT (schedule 8) — no-show example
            new() { Id = 20, ClassScheduleId = 8, MemberId = 6, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
        };
        db.Bookings.AddRange(bookings);

        await db.SaveChangesAsync();
    }
}
