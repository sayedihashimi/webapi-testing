using FitnessStudioApi.Models;

namespace FitnessStudioApi.Data;

public static class DataSeeder
{
    public static void Seed(FitnessDbContext db)
    {
        if (db.MembershipPlans.Any()) return;

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.Today);

        // === Membership Plans ===
        var basicPlan = new MembershipPlan
        {
            Name = "Basic", Description = "Access to standard classes only. Perfect for beginners.",
            DurationMonths = 1, Price = 29.99m, MaxClassBookingsPerWeek = 3,
            AllowsPremiumClasses = false, CreatedAt = now, UpdatedAt = now
        };
        var premiumPlan = new MembershipPlan
        {
            Name = "Premium", Description = "Access to all classes including premium. Great value for regulars.",
            DurationMonths = 1, Price = 49.99m, MaxClassBookingsPerWeek = 5,
            AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };
        var elitePlan = new MembershipPlan
        {
            Name = "Elite", Description = "Unlimited access to all classes. Priority booking and VIP perks.",
            DurationMonths = 1, Price = 79.99m, MaxClassBookingsPerWeek = -1,
            AllowsPremiumClasses = true, CreatedAt = now, UpdatedAt = now
        };

        db.MembershipPlans.AddRange(basicPlan, premiumPlan, elitePlan);
        db.SaveChanges();

        // === Members ===
        var members = new List<Member>
        {
            new() { FirstName = "Emma", LastName = "Johnson", Email = "emma.johnson@email.com", Phone = "555-0101",
                DateOfBirth = new DateOnly(1990, 3, 15), EmergencyContactName = "Michael Johnson", EmergencyContactPhone = "555-0102",
                JoinDate = today.AddMonths(-6), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Liam", LastName = "Williams", Email = "liam.williams@email.com", Phone = "555-0103",
                DateOfBirth = new DateOnly(1985, 7, 22), EmergencyContactName = "Sarah Williams", EmergencyContactPhone = "555-0104",
                JoinDate = today.AddMonths(-4), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Olivia", LastName = "Brown", Email = "olivia.brown@email.com", Phone = "555-0105",
                DateOfBirth = new DateOnly(1992, 11, 8), EmergencyContactName = "David Brown", EmergencyContactPhone = "555-0106",
                JoinDate = today.AddMonths(-3), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Noah", LastName = "Davis", Email = "noah.davis@email.com", Phone = "555-0107",
                DateOfBirth = new DateOnly(1988, 1, 30), EmergencyContactName = "Jessica Davis", EmergencyContactPhone = "555-0108",
                JoinDate = today.AddMonths(-5), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Ava", LastName = "Martinez", Email = "ava.martinez@email.com", Phone = "555-0109",
                DateOfBirth = new DateOnly(1995, 5, 12), EmergencyContactName = "Carlos Martinez", EmergencyContactPhone = "555-0110",
                JoinDate = today.AddMonths(-2), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Ethan", LastName = "Garcia", Email = "ethan.garcia@email.com", Phone = "555-0111",
                DateOfBirth = new DateOnly(1993, 9, 25), EmergencyContactName = "Maria Garcia", EmergencyContactPhone = "555-0112",
                JoinDate = today.AddMonths(-1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Sophia", LastName = "Anderson", Email = "sophia.anderson@email.com", Phone = "555-0113",
                DateOfBirth = new DateOnly(1991, 4, 18), EmergencyContactName = "Robert Anderson", EmergencyContactPhone = "555-0114",
                JoinDate = today.AddMonths(-7), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Mason", LastName = "Taylor", Email = "mason.taylor@email.com", Phone = "555-0115",
                DateOfBirth = new DateOnly(1987, 12, 3), EmergencyContactName = "Linda Taylor", EmergencyContactPhone = "555-0116",
                JoinDate = today.AddMonths(-8), CreatedAt = now, UpdatedAt = now }
        };

        db.Members.AddRange(members);
        db.SaveChanges();

        // === Memberships ===
        var memberships = new List<Membership>
        {
            // Active memberships
            new() { MemberId = members[0].Id, MembershipPlanId = premiumPlan.Id,
                StartDate = today.AddDays(-15), EndDate = today.AddDays(15),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[1].Id, MembershipPlanId = elitePlan.Id,
                StartDate = today.AddDays(-10), EndDate = today.AddDays(20),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[2].Id, MembershipPlanId = basicPlan.Id,
                StartDate = today.AddDays(-5), EndDate = today.AddDays(25),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[3].Id, MembershipPlanId = premiumPlan.Id,
                StartDate = today.AddDays(-20), EndDate = today.AddDays(10),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[4].Id, MembershipPlanId = elitePlan.Id,
                StartDate = today.AddDays(-8), EndDate = today.AddDays(22),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            new() { MemberId = members[5].Id, MembershipPlanId = basicPlan.Id,
                StartDate = today.AddDays(-3), EndDate = today.AddDays(27),
                Status = MembershipStatus.Active, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for member 7 (Sophia)
            new() { MemberId = members[6].Id, MembershipPlanId = basicPlan.Id,
                StartDate = today.AddMonths(-3), EndDate = today.AddMonths(-2),
                Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now },
            // Expired membership for member 8 (Mason)
            new() { MemberId = members[7].Id, MembershipPlanId = premiumPlan.Id,
                StartDate = today.AddMonths(-4), EndDate = today.AddMonths(-3),
                Status = MembershipStatus.Expired, PaymentStatus = PaymentStatus.Paid, CreatedAt = now, UpdatedAt = now }
        };

        db.Memberships.AddRange(memberships);
        db.SaveChanges();

        // === Instructors ===
        var instructors = new List<Instructor>
        {
            new() { FirstName = "Sarah", LastName = "Chen", Email = "sarah.chen@zenithfitness.com", Phone = "555-0201",
                Bio = "Certified yoga instructor with 10 years of experience. Specializes in Vinyasa and Hatha yoga.",
                Specializations = "Yoga, Pilates, Meditation", HireDate = new DateOnly(2020, 1, 15), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Marcus", LastName = "Rivera", Email = "marcus.rivera@zenithfitness.com", Phone = "555-0202",
                Bio = "Former professional athlete turned fitness coach. Expert in high-intensity training and boxing.",
                Specializations = "HIIT, Boxing, Spin", HireDate = new DateOnly(2019, 6, 1), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Aisha", LastName = "Patel", Email = "aisha.patel@zenithfitness.com", Phone = "555-0203",
                Bio = "Pilates and meditation specialist with a background in physical therapy.",
                Specializations = "Pilates, Meditation, Yoga", HireDate = new DateOnly(2021, 3, 10), CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Jake", LastName = "Thompson", Email = "jake.thompson@zenithfitness.com", Phone = "555-0204",
                Bio = "Spin and HIIT enthusiast. Known for energetic classes and motivating playlists.",
                Specializations = "Spin, HIIT, Boxing", HireDate = new DateOnly(2022, 9, 1), CreatedAt = now, UpdatedAt = now }
        };

        db.Instructors.AddRange(instructors);
        db.SaveChanges();

        // === Class Types ===
        var classTypes = new List<ClassType>
        {
            new() { Name = "Yoga", Description = "Traditional yoga focusing on flexibility, balance, and mindfulness.",
                DefaultDurationMinutes = 60, DefaultCapacity = 20, IsPremium = false,
                CaloriesPerSession = 250, DifficultyLevel = DifficultyLevel.AllLevels, CreatedAt = now, UpdatedAt = now },
            new() { Name = "HIIT", Description = "High-Intensity Interval Training for maximum calorie burn.",
                DefaultDurationMinutes = 45, DefaultCapacity = 15, IsPremium = false,
                CaloriesPerSession = 500, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Spin", Description = "Indoor cycling class with varying intensity levels.",
                DefaultDurationMinutes = 45, DefaultCapacity = 20, IsPremium = false,
                CaloriesPerSession = 450, DifficultyLevel = DifficultyLevel.Intermediate, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Pilates", Description = "Core-strengthening exercises for flexibility and posture.",
                DefaultDurationMinutes = 60, DefaultCapacity = 15, IsPremium = false,
                CaloriesPerSession = 300, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Boxing", Description = "Premium boxing class with personal attention and advanced techniques.",
                DefaultDurationMinutes = 60, DefaultCapacity = 10, IsPremium = true,
                CaloriesPerSession = 600, DifficultyLevel = DifficultyLevel.Advanced, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Meditation", Description = "Premium guided meditation and breathwork for deep relaxation.",
                DefaultDurationMinutes = 45, DefaultCapacity = 12, IsPremium = true,
                CaloriesPerSession = 80, DifficultyLevel = DifficultyLevel.Beginner, CreatedAt = now, UpdatedAt = now }
        };

        db.ClassTypes.AddRange(classTypes);
        db.SaveChanges();

        // === Class Schedules (over next 7 days) ===
        var tomorrow = DateTime.Today.AddDays(1);

        var schedules = new List<ClassSchedule>
        {
            // Day 1 classes
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddHours(7), EndTime = tomorrow.AddHours(8),
                Capacity = 20, CurrentEnrollment = 5, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddHours(9), EndTime = tomorrow.AddHours(9).AddMinutes(45),
                Capacity = 15, CurrentEnrollment = 10, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            // Full class with waitlist
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddHours(17), EndTime = tomorrow.AddHours(17).AddMinutes(45),
                Capacity = 3, CurrentEnrollment = 3, WaitlistCount = 2, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Day 2 classes
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddDays(1).AddHours(8), EndTime = tomorrow.AddDays(1).AddHours(9),
                Capacity = 15, CurrentEnrollment = 8, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(1).AddHours(10), EndTime = tomorrow.AddDays(1).AddHours(11),
                Capacity = 10, CurrentEnrollment = 6, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            // Day 3 classes
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddDays(2).AddHours(7), EndTime = tomorrow.AddDays(2).AddHours(7).AddMinutes(45),
                Capacity = 12, CurrentEnrollment = 4, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[0].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddDays(2).AddHours(18), EndTime = tomorrow.AddDays(2).AddHours(19),
                Capacity = 20, CurrentEnrollment = 12, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
            // Day 4 classes
            new() { ClassTypeId = classTypes[1].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddDays(3).AddHours(6), EndTime = tomorrow.AddDays(3).AddHours(6).AddMinutes(45),
                Capacity = 15, CurrentEnrollment = 2, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            new() { ClassTypeId = classTypes[2].Id, InstructorId = instructors[3].Id,
                StartTime = tomorrow.AddDays(3).AddHours(17), EndTime = tomorrow.AddDays(3).AddHours(17).AddMinutes(45),
                Capacity = 20, CurrentEnrollment = 0, Room = "Studio B", CreatedAt = now, UpdatedAt = now },
            // Day 5 - cancelled class
            new() { ClassTypeId = classTypes[3].Id, InstructorId = instructors[2].Id,
                StartTime = tomorrow.AddDays(4).AddHours(9), EndTime = tomorrow.AddDays(4).AddHours(10),
                Capacity = 15, CurrentEnrollment = 0, Room = "Studio A",
                Status = ClassScheduleStatus.Cancelled, CancellationReason = "Instructor unavailable", CreatedAt = now, UpdatedAt = now },
            // Day 5 replacement
            new() { ClassTypeId = classTypes[4].Id, InstructorId = instructors[1].Id,
                StartTime = tomorrow.AddDays(4).AddHours(10), EndTime = tomorrow.AddDays(4).AddHours(11),
                Capacity = 10, CurrentEnrollment = 3, Room = "Main Floor", CreatedAt = now, UpdatedAt = now },
            // Day 6
            new() { ClassTypeId = classTypes[5].Id, InstructorId = instructors[0].Id,
                StartTime = tomorrow.AddDays(5).AddHours(7), EndTime = tomorrow.AddDays(5).AddHours(7).AddMinutes(45),
                Capacity = 12, CurrentEnrollment = 7, Room = "Studio A", CreatedAt = now, UpdatedAt = now },
        };

        db.ClassSchedules.AddRange(schedules);
        db.SaveChanges();

        // === Bookings ===
        // Confirmed bookings for schedule 1 (Yoga tomorrow 7am)
        var bookings = new List<Booking>
        {
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[0].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-24), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[1].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-23), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[0].Id, MemberId = members[2].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-22), CreatedAt = now, UpdatedAt = now },
            // Bookings for schedule 2 (HIIT)
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[3].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-20), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[1].Id, MemberId = members[4].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-19), CreatedAt = now, UpdatedAt = now },
            // Full class (Spin schedule 3) - confirmed + waitlisted
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[0].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-18), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[1].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-17), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[3].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-16), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[4].Id,
                Status = BookingStatus.Waitlisted, WaitlistPosition = 1, BookingDate = now.AddHours(-15), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[2].Id, MemberId = members[5].Id,
                Status = BookingStatus.Waitlisted, WaitlistPosition = 2, BookingDate = now.AddHours(-14), CreatedAt = now, UpdatedAt = now },
            // Cancelled booking
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[2].Id,
                Status = BookingStatus.Cancelled, BookingDate = now.AddHours(-48),
                CancellationDate = now.AddHours(-24), CancellationReason = "Schedule conflict", CreatedAt = now, UpdatedAt = now },
            // Bookings for Pilates
            new() { ClassScheduleId = schedules[3].Id, MemberId = members[0].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-12), CreatedAt = now, UpdatedAt = now },
            // Boxing (premium) bookings
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[0].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-10), CreatedAt = now, UpdatedAt = now },
            new() { ClassScheduleId = schedules[4].Id, MemberId = members[1].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-9), CreatedAt = now, UpdatedAt = now },
            // Meditation booking
            new() { ClassScheduleId = schedules[5].Id, MemberId = members[4].Id,
                Status = BookingStatus.Confirmed, BookingDate = now.AddHours(-8), CreatedAt = now, UpdatedAt = now },
        };

        db.Bookings.AddRange(bookings);
        db.SaveChanges();
    }
}
