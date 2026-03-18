using FitnessStudioApi.Data;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public static class DataSeeder
{
    public static async Task SeedAsync(FitnessDbContext db)
    {
        if (await db.MembershipPlans.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Membership Plans
        var basicPlan = new MembershipPlan { Name = "Basic", Description = "Basic membership with limited class access", DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3, AllowsPremiumClasses = false, CreatedAt = now, UpdatedAt = now };
        var premiumPlan = new MembershipPlan { Name = "Premium", Description = "Premium membership with expanded class access", DurationMonths = 3, Price = 49.99m, MaxClassBookingsPerWeek = 5, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now };
        var elitePlan = new MembershipPlan { Name = "Elite", Description = "Elite membership with unlimited class access and all premium features", DurationMonths = 12, Price = 79.99m, MaxClassBookingsPerWeek = -1, AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now };

        db.MembershipPlans.AddRange(basicPlan, premiumPlan, elitePlan);
        await db.SaveChangesAsync();

        // Members
        var members = new List<Member>
        {
            new() { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@email.com", Phone = "555-0101", DateOfBirth = new DateOnly(1990, 5, 15), EmergencyContactName = "Bob Johnson", EmergencyContactPhone = "555-0102", JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Marcus", LastName = "Chen", Email = "marcus.chen@email.com", Phone = "555-0103", DateOfBirth = new DateOnly(1985, 8, 22), EmergencyContactName = "Li Chen", EmergencyContactPhone = "555-0104", JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Sofia", LastName = "Rodriguez", Email = "sofia.rodriguez@email.com", Phone = "555-0105", DateOfBirth = new DateOnly(1995, 3, 10), EmergencyContactName = "Carlos Rodriguez", EmergencyContactPhone = "555-0106", JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0107", DateOfBirth = new DateOnly(1988, 11, 30), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0108", JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Emma", LastName = "Davis", Email = "emma.davis@email.com", Phone = "555-0109", DateOfBirth = new DateOnly(1992, 7, 18), EmergencyContactName = "Tom Davis", EmergencyContactPhone = "555-0110", JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Raj", LastName = "Patel", Email = "raj.patel@email.com", Phone = "555-0111", DateOfBirth = new DateOnly(1993, 1, 25), EmergencyContactName = "Priya Patel", EmergencyContactPhone = "555-0112", JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Olivia", LastName = "Brown", Email = "olivia.brown@email.com", Phone = "555-0113", DateOfBirth = new DateOnly(1998, 9, 5), EmergencyContactName = "Michael Brown", EmergencyContactPhone = "555-0114", JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Liam", LastName = "Taylor", Email = "liam.taylor@email.com", Phone = "555-0115", DateOfBirth = new DateOnly(1987, 4, 12), EmergencyContactName = "Karen Taylor", EmergencyContactPhone = "555-0116", JoinDate = today.AddMonths(-7), IsActive = false, CreatedAt = now, UpdatedAt = now }
        };
        db.Members.AddRange(members);
        await db.SaveChangesAsync();

        // Memberships
        var memberships = new List<Membership>
        {
            new() { MemberId = members[0].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddMonths(-3), EndDate = today.AddMonths(0), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[1].Id, MembershipPlanId = elitePlan.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(10), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[2].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddMonths(-1), EndDate = today, Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[3].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddMonths(-6), EndDate = today.AddMonths(-3), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[3].Id, MembershipPlanId = elitePlan.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(11), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[4].Id, MembershipPlanId = premiumPlan.Id, StartDate = today.AddMonths(-1), EndDate = today.AddMonths(2), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[5].Id, MembershipPlanId = basicPlan.Id, StartDate = today.AddMonths(-2), EndDate = today.AddMonths(-1), Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[5].Id, MembershipPlanId = premiumPlan.Id, StartDate = today, EndDate = today.AddMonths(3), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[6].Id, MembershipPlanId = basicPlan.Id, StartDate = today, EndDate = today.AddMonths(1), Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Pending, CreatedAt = now, UpdatedAt = now },
        };
        db.Memberships.AddRange(memberships);
        await db.SaveChangesAsync();

        // Instructors
        var instructors = new List<Instructor>
        {
            new() { FirstName = "Maya", LastName = "Singh", Email = "maya.singh@zenith.com", Phone = "555-0201", Bio = "Certified yoga and pilates instructor with 10+ years of experience", Specializations = "Yoga,Pilates,Meditation", HireDate = today.AddYears(-3), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Jake", LastName = "Morrison", Email = "jake.morrison@zenith.com", Phone = "555-0202", Bio = "Former professional boxer turned fitness instructor", Specializations = "HIIT,Boxing,Strength", HireDate = today.AddYears(-2), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Luna", LastName = "Park", Email = "luna.park@zenith.com", Phone = "555-0203", Bio = "Spinning and cardio specialist with group fitness certification", Specializations = "Spin,HIIT,Cardio", HireDate = today.AddYears(-1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "David", LastName = "Okafor", Email = "david.okafor@zenith.com", Phone = "555-0204", Bio = "Mindfulness and meditation expert with background in sports psychology", Specializations = "Meditation,Yoga,Flexibility", HireDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now }
        };
        db.Instructors.AddRange(instructors);
        await db.SaveChangesAsync();

        // Class Types
        var yoga = new ClassType { Name = "Yoga", Description = "Traditional yoga combining poses, breathing exercises, and meditation", DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false, CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now };
        var hiit = new ClassType { Name = "HIIT", Description = "High-intensity interval training for maximum calorie burn", DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false, CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now };
        var spin = new ClassType { Name = "Spin", Description = "Indoor cycling class with motivating music and varied intensity", DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = true, CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now };
        var pilates = new ClassType { Name = "Pilates", Description = "Core-strengthening exercises focusing on flexibility and balance", DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = true, CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now };
        var boxing = new ClassType { Name = "Boxing", Description = "Boxing-inspired workout combining cardio with strength training", DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = false, CaloriesPerSession = 550, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now };
        var meditation = new ClassType { Name = "Meditation", Description = "Guided meditation for stress relief and mental clarity", DefaultDurationMinutes = 30, DefaultCapacity = 25, IsPremium = false, CaloriesPerSession = 50, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now };

        db.ClassTypes.AddRange(yoga, hiit, spin, pilates, boxing, meditation);
        await db.SaveChangesAsync();

        // Class Schedules (over next 7 days)
        var tomorrow = now.Date.AddDays(1);
        var schedules = new List<ClassSchedule>
        {
            // Tomorrow
            new() { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8), Capacity = 20, CurrentEnrollment = 5, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = hiit.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 15, WaitlistCount = 2, Room = "Studio B", CreatedAt = now, UpdatedAt = now }, // Full
            new() { ClassTypeId = spin.Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddHours(10), EndTime = tomorrow.AddHours(10).AddMinutes(45), Capacity = 12, CurrentEnrollment = 8, Room = "Spin Room", CreatedAt = now, UpdatedAt = now },
            // Day after tomorrow
            new() { ClassTypeId = pilates.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(1).AddHours(8), EndTime = tomorrow.AddDays(1).AddHours(9), Capacity = 15, CurrentEnrollment = 10, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = boxing.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(1).AddHours(11), EndTime = tomorrow.AddDays(1).AddHours(12), Capacity = 10, CurrentEnrollment = 3, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = meditation.Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(1).AddHours(17), EndTime = tomorrow.AddDays(1).AddHours(17).AddMinutes(30), Capacity = 25, CurrentEnrollment = 2, Room = "Zen Room", CreatedAt = now, UpdatedAt = now },
            // Day +3
            new() { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(8), Capacity = 20, CurrentEnrollment = 12, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = hiit.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(2).AddHours(9), EndTime = tomorrow.AddDays(2).AddHours(9).AddMinutes(45), Capacity = 15, CurrentEnrollment = 7, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Day +4
            new() { ClassTypeId = spin.Id, InstructorId = instructors[2].Id, StartTime = tomorrow.AddDays(3).AddHours(10), EndTime = tomorrow.AddDays(3).AddHours(10).AddMinutes(45), Capacity = 12, CurrentEnrollment = 12, WaitlistCount = 1, Room = "Spin Room", CreatedAt = now, UpdatedAt = now }, // Full
            new() { ClassTypeId = boxing.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(3).AddHours(14), EndTime = tomorrow.AddDays(3).AddHours(15), Capacity = 10, CurrentEnrollment = 5, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Day +5
            new() { ClassTypeId = pilates.Id, InstructorId = instructors[0].Id, StartTime = tomorrow.AddDays(4).AddHours(8), EndTime = tomorrow.AddDays(4).AddHours(9), Capacity = 15, CurrentEnrollment = 0, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = meditation.Id, InstructorId = instructors[3].Id, StartTime = tomorrow.AddDays(4).AddHours(18), EndTime = tomorrow.AddDays(4).AddHours(18).AddMinutes(30), Capacity = 25, CurrentEnrollment = 5, Room = "Zen Room", CreatedAt = now, UpdatedAt = now },
            // Cancelled class
            new() { ClassTypeId = hiit.Id, InstructorId = instructors[1].Id, StartTime = tomorrow.AddDays(5).AddHours(9), EndTime = tomorrow.AddDays(5).AddHours(9).AddMinutes(45), Capacity = 15, Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Past class (yesterday)
            new() { ClassTypeId = yoga.Id, InstructorId = instructors[0].Id, StartTime = now.Date.AddDays(-1).AddHours(7), EndTime = now.Date.AddDays(-1).AddHours(8), Capacity = 20, CurrentEnrollment = 15, Status = ClassScheduleStatus.Completed, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
        };

        db.ClassSchedules.AddRange(schedules);
        await db.SaveChangesAsync();

        // Bookings
        var bookings = new List<Booking>
        {
            // Confirmed bookings for tomorrow's yoga (schedule[0])
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[4].Id, BookingDate = now.AddHours(-6), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Full HIIT class with waitlist (schedule[1])
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[0].Id, BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[1].Id, BookingDate = now.AddDays(-2), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[2].Id, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id, BookingDate = now.AddHours(-12), Status = BookingStatus.Waitlisted, WaitlistPosition = 1, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[5].Id, BookingDate = now.AddHours(-6), Status = BookingStatus.Waitlisted, WaitlistPosition = 2, CreatedAt = now, UpdatedAt = now },

            // Spin class bookings (schedule[2])
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[1].Id, BookingDate = now.AddDays(-1), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[3].Id, BookingDate = now.AddHours(-8), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Pilates (schedule[3])
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[0].Id, BookingDate = now.AddHours(-4), Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },

            // Cancelled booking
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[2].Id, BookingDate = now.AddDays(-3), Status = BookingStatus.Cancelled, CancellationDate = now.AddDays(-1), CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },

            // Attended booking (past class, schedule[13])
            new() { ClassScheduleId = schedules[13].Id, MemberId = members[0].Id, BookingDate = now.AddDays(-3), Status = BookingStatus.Attended, CheckInTime = now.Date.AddDays(-1).AddHours(6).AddMinutes(50), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[13].Id, MemberId = members[1].Id, BookingDate = now.AddDays(-3), Status = BookingStatus.Attended, CheckInTime = now.Date.AddDays(-1).AddHours(6).AddMinutes(55), CreatedAt = now, UpdatedAt = now },

            // No-show
            new() { ClassScheduleId = schedules[13].Id, MemberId = members[5].Id, BookingDate = now.AddDays(-3), Status = BookingStatus.NoShow, CreatedAt = now, UpdatedAt = now },

            // More confirmed bookings
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[3].Id, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[6].Id, MemberId = members[4].Id, BookingDate = now, Status = BookingStatus.Confirmed, CreatedAt = now, UpdatedAt = now },
        };

        db.Bookings.AddRange(bookings);
        await db.SaveChangesAsync();
    }
}
