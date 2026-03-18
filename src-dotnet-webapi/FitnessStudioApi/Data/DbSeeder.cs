using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(FitnessDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Membership Plans
        var basic = new MembershipPlan { Name = "Basic", Description = "Access to standard classes with limited weekly bookings.", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false, CreatedAt = now, UpdatedAt = now };
        var premium = new MembershipPlan { Name = "Premium", Description = "Access to all classes including premium with generous weekly bookings.", DurationMonths = 3, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now };
        var elite = new MembershipPlan { Name = "Elite", Description = "Unlimited access to all classes and premium features.", DurationMonths = 12, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now };
        db.MembershipPlans.AddRange(basic, premium, elite);
        await db.SaveChangesAsync();

        // Members
        var members = new[]
        {
            new Member { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Brian", LastName = "Smith", Email = "brian.smith@email.com", Phone = "555-0103", DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Sarah Smith", EmergencyContactPhone = "555-0104", JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Carla", LastName = "Martinez", Email = "carla.martinez@email.com", Phone = "555-0105", DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "Diego Martinez", EmergencyContactPhone = "555-0106", JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "David", LastName = "Lee", Email = "david.lee@email.com", Phone = "555-0107", DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Emily Lee", EmergencyContactPhone = "555-0108", JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Elena", LastName = "Popov", Email = "elena.popov@email.com", Phone = "555-0109", DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Viktor Popov", EmergencyContactPhone = "555-0110", JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Frank", LastName = "Williams", Email = "frank.williams@email.com", Phone = "555-0111", DateOfBirth = new DateOnly(1980, 9, 25), EmergencyContactName = "Grace Williams", EmergencyContactPhone = "555-0112", JoinDate = today.AddMonths(-10), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Gina", LastName = "Chen", Email = "gina.chen@email.com", Phone = "555-0113", DateOfBirth = new DateOnly(1993, 4, 17), EmergencyContactName = "Henry Chen", EmergencyContactPhone = "555-0114", JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new Member { FirstName = "Hassan", LastName = "Ali", Email = "hassan.ali@email.com", Phone = "555-0115", DateOfBirth = new DateOnly(1991, 12, 3), EmergencyContactName = "Fatima Ali", EmergencyContactPhone = "555-0116", JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
        };
        db.Members.AddRange(members);
        await db.SaveChangesAsync();

        // Memberships
        var memberships = new[]
        {
            new Membership { MemberId = members[0].Id, MembershipPlanId = elite.Id, StartDate = today.AddMonths(-6), EndDate = today.AddMonths(6), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[1].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[2].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-1), EndDate = today, Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[3].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-6), EndDate = today.AddMonths(-3), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[3].Id, MembershipPlanId = elite.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[4].Id, MembershipPlanId = premium.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(2), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[5].Id, MembershipPlanId = basic.Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[6].Id, MembershipPlanId = basic.Id, StartDate = today, EndDate = today.AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new Membership { MemberId = members[7].Id, MembershipPlanId = elite.Id, StartDate = today.AddMonths(-4), EndDate = today.AddMonths(8), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
        };
        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();

        // Instructors
        var instructors = new[]
        {
            new Instructor { FirstName = "Maya", LastName = "Patel", Email = "maya.patel@zenithfitness.com", Phone = "555-0201", Bio = "Certified yoga instructor with 10 years of experience in Vinyasa and Hatha yoga.", Specializations = "Yoga,Pilates,Meditation", HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Jake", LastName = "Torres", Email = "jake.torres@zenithfitness.com", Phone = "555-0202", Bio = "Former competitive athlete specializing in high-intensity training and boxing.", Specializations = "HIIT,Boxing,Spin", HireDate = new DateOnly(2021, 6, 1), CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Sophie", LastName = "Kim", Email = "sophie.kim@zenithfitness.com", Phone = "555-0203", Bio = "Pilates and meditation expert focused on mind-body connection.", Specializations = "Pilates,Yoga,Meditation", HireDate = new DateOnly(2019, 9, 10), CreatedAt = now, UpdatedAt = now },
            new Instructor { FirstName = "Marcus", LastName = "Rivera", Email = "marcus.rivera@zenithfitness.com", Phone = "555-0204", Bio = "Spin and HIIT specialist with a passion for motivating people.", Specializations = "Spin,HIIT,Boxing", HireDate = new DateOnly(2022, 3, 20), CreatedAt = now, UpdatedAt = now },
        };
        db.Instructors.AddRange(instructors);
        await db.SaveChangesAsync();

        // Class Types
        var classTypes = new[]
        {
            new ClassType { Name = "Yoga Flow", Description = "A dynamic Vinyasa-style yoga class for flexibility and strength.", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "HIIT Blast", Description = "High-intensity interval training for maximum calorie burn.", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Spin Cycle", Description = "Indoor cycling class with varied intensity and terrain simulations.", DefaultDurationMinutes = 45, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Pilates Core", Description = "Core-focused Pilates session for strength and posture.", DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Boxing Fit", Description = "Boxing-inspired fitness class combining technique with cardio.", DefaultDurationMinutes = 60, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new ClassType { Name = "Zen Meditation", Description = "Guided meditation for stress relief and mental clarity.", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = true, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
        };
        db.ClassTypes.AddRange(classTypes);
        await db.SaveChangesAsync();

        // Class Schedules - spread over next 7 days
        var baseDate = now.Date.AddDays(1);
        var schedules = new[]
        {
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddHours(7), EndTime = baseDate.AddHours(8), Capacity = 20, CurrentEnrollment = 3, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddHours(9), EndTime = baseDate.AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 15, WaitlistCount = 2, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[3].Id, StartTime = baseDate.AddDays(1).AddHours(8), EndTime = baseDate.AddDays(1).AddHours(8).AddMinutes(45), Capacity = 20, CurrentEnrollment = 10, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[2].Id, StartTime = baseDate.AddDays(1).AddHours(10), EndTime = baseDate.AddDays(1).AddHours(11), Capacity = 15, CurrentEnrollment = 8, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(2).AddHours(17), EndTime = baseDate.AddDays(2).AddHours(18), Capacity = 12, CurrentEnrollment = 5, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[5].Id, InstructorId = instructors[2].Id, StartTime = baseDate.AddDays(2).AddHours(19), EndTime = baseDate.AddDays(2).AddHours(19).AddMinutes(30), Capacity = 25, CurrentEnrollment = 12, Room = "Zen Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddDays(3).AddHours(7), EndTime = baseDate.AddDays(3).AddHours(8), Capacity = 20, CurrentEnrollment = 0, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(3).AddHours(18), EndTime = baseDate.AddDays(3).AddHours(18).AddMinutes(45), Capacity = 15, CurrentEnrollment = 7, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[2].Id, InstructorId = instructors[3].Id, StartTime = baseDate.AddDays(4).AddHours(8), EndTime = baseDate.AddDays(4).AddHours(8).AddMinutes(45), Capacity = 20, CurrentEnrollment = 14, Room = "Spin Room", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[3].Id, InstructorId = instructors[2].Id, StartTime = baseDate.AddDays(5).AddHours(10), EndTime = baseDate.AddDays(5).AddHours(11), Capacity = 15, CurrentEnrollment = 4, Room = "Studio A", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            new ClassSchedule { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id, StartTime = baseDate.AddDays(5).AddHours(17), EndTime = baseDate.AddDays(5).AddHours(18), Capacity = 12, CurrentEnrollment = 12, WaitlistCount = 1, Room = "Studio B", Status = ClassScheduleStatus.Scheduled, CreatedAt = now, UpdatedAt = now },
            // Cancelled class
            new ClassSchedule { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id, StartTime = baseDate.AddDays(6).AddHours(19), EndTime = baseDate.AddDays(6).AddHours(19).AddMinutes(30), Capacity = 25, CurrentEnrollment = 0, Room = "Zen Room", Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now },
        };
        db.ClassSchedules.AddRange(schedules);
        await db.SaveChangesAsync();

        // Bookings
        var bookings = new List<Booking>
        {
            // Yoga Flow (schedule[0]) - 3 confirmed
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[4].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-12), CreatedAt = now, UpdatedAt = now },

            // HIIT Blast (schedule[1]) - full with 15 confirmed + 2 waitlisted
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-2), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[1].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-2), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[2].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-6), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[7].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 2, BookingDate = now.AddHours(-4), CreatedAt = now, UpdatedAt = now },

            // Spin Cycle (schedule[2]) - some confirmed
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-8), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-6), CreatedAt = now, UpdatedAt = now },

            // Boxing Fit (schedule[4]) - premium class
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-10), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[3].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-8), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[7].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-4), CreatedAt = now, UpdatedAt = now },

            // Boxing Fit full (schedule[10]) - full with waitlist
            new() { ClassScheduleId = schedules[10].Id, MemberId = members[0].Id, Status = BookingStatus.Confirmed, BookingDate = now.AddDays(-1), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[10].Id, MemberId = members[7].Id, Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-2), CreatedAt = now, UpdatedAt = now },

            // A cancelled booking
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[2].Id, Status = BookingStatus.Cancelled, BookingDate = now.AddDays(-2), CancellationDate = now.AddDays(-1), CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },
        };
        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();
    }
}
