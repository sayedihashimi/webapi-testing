using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Departments.AnyAsync())
            return;

        // ── Leave Types ──
        var vacation = new LeaveType { Name = "Vacation", DefaultDaysPerYear = 15, IsCarryOverAllowed = true, MaxCarryOverDays = 5, RequiresApproval = true, IsPaid = true };
        var sick = new LeaveType { Name = "Sick", DefaultDaysPerYear = 10, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var personal = new LeaveType { Name = "Personal", DefaultDaysPerYear = 3, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var bereavement = new LeaveType { Name = "Bereavement", DefaultDaysPerYear = 5, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = false, IsPaid = true };
        db.LeaveTypes.AddRange(vacation, sick, personal, bereavement);

        // ── Departments ──
        var engineering = new Department { Name = "Engineering", Code = "ENG", Description = "Software Engineering Division" };
        var hr = new Department { Name = "Human Resources", Code = "HR", Description = "People & Culture" };
        var marketing = new Department { Name = "Marketing", Code = "MKT", Description = "Marketing and Communications" };
        var frontend = new Department { Name = "Frontend Engineering", Code = "FE", Description = "Frontend Development Team", ParentDepartment = engineering };
        var backend = new Department { Name = "Backend Engineering", Code = "BE", Description = "Backend Development Team", ParentDepartment = engineering };
        db.Departments.AddRange(engineering, hr, marketing, frontend, backend);
        await db.SaveChangesAsync();

        // ── Employees ──
        var now = DateTime.UtcNow;
        var currentYear = DateTime.UtcNow.Year;

        var employees = new List<Employee>
        {
            new() { EmployeeNumber = "EMP-0001", FirstName = "Alice", LastName = "Chen", Email = "alice.chen@horizonhr.com", Phone = "555-0101", DateOfBirth = new DateOnly(1985, 3, 15), HireDate = new DateOnly(2018, 1, 10), Department = engineering, JobTitle = "VP of Engineering", EmploymentType = EmploymentType.FullTime, Salary = 185000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0002", FirstName = "Bob", LastName = "Martinez", Email = "bob.martinez@horizonhr.com", Phone = "555-0102", DateOfBirth = new DateOnly(1990, 7, 22), HireDate = new DateOnly(2019, 3, 15), Department = frontend, JobTitle = "Frontend Lead", EmploymentType = EmploymentType.FullTime, Salary = 145000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0003", FirstName = "Carol", LastName = "Johnson", Email = "carol.johnson@horizonhr.com", Phone = "555-0103", DateOfBirth = new DateOnly(1988, 11, 5), HireDate = new DateOnly(2020, 6, 1), Department = backend, JobTitle = "Backend Lead", EmploymentType = EmploymentType.FullTime, Salary = 150000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0004", FirstName = "David", LastName = "Kim", Email = "david.kim@horizonhr.com", Phone = "555-0104", DateOfBirth = new DateOnly(1992, 4, 18), HireDate = new DateOnly(2021, 2, 14), Department = frontend, JobTitle = "Senior Frontend Developer", EmploymentType = EmploymentType.FullTime, Salary = 125000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0005", FirstName = "Emily", LastName = "Davis", Email = "emily.davis@horizonhr.com", Phone = "555-0105", DateOfBirth = new DateOnly(1995, 8, 30), HireDate = new DateOnly(2022, 9, 5), Department = frontend, JobTitle = "Frontend Developer", EmploymentType = EmploymentType.FullTime, Salary = 105000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0006", FirstName = "Frank", LastName = "Wilson", Email = "frank.wilson@horizonhr.com", Phone = "555-0106", DateOfBirth = new DateOnly(1987, 1, 12), HireDate = new DateOnly(2019, 11, 20), Department = backend, JobTitle = "Senior Backend Developer", EmploymentType = EmploymentType.FullTime, Salary = 135000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0007", FirstName = "Grace", LastName = "Lee", Email = "grace.lee@horizonhr.com", Phone = "555-0107", DateOfBirth = new DateOnly(1993, 6, 25), HireDate = new DateOnly(2023, 1, 9), Department = backend, JobTitle = "Backend Developer", EmploymentType = EmploymentType.Contract, Salary = 110000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0008", FirstName = "Henry", LastName = "Taylor", Email = "henry.taylor@horizonhr.com", Phone = "555-0108", DateOfBirth = new DateOnly(1980, 9, 8), HireDate = new DateOnly(2017, 5, 1), Department = hr, JobTitle = "HR Director", EmploymentType = EmploymentType.FullTime, Salary = 140000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0009", FirstName = "Irene", LastName = "Brown", Email = "irene.brown@horizonhr.com", Phone = "555-0109", DateOfBirth = new DateOnly(1991, 12, 3), HireDate = new DateOnly(2020, 8, 17), Department = hr, JobTitle = "HR Specialist", EmploymentType = EmploymentType.FullTime, Salary = 85000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0010", FirstName = "Jack", LastName = "Anderson", Email = "jack.anderson@horizonhr.com", Phone = "555-0110", DateOfBirth = new DateOnly(1989, 5, 20), HireDate = new DateOnly(2018, 10, 1), Department = marketing, JobTitle = "Marketing Director", EmploymentType = EmploymentType.FullTime, Salary = 130000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0011", FirstName = "Karen", LastName = "Thomas", Email = "karen.thomas@horizonhr.com", Phone = "555-0111", DateOfBirth = new DateOnly(1994, 2, 14), HireDate = new DateOnly(2021, 7, 12), Department = marketing, JobTitle = "Marketing Specialist", EmploymentType = EmploymentType.PartTime, Salary = 65000m, Status = EmployeeStatus.OnLeave },
            new() { EmployeeNumber = "EMP-0012", FirstName = "Leo", LastName = "Garcia", Email = "leo.garcia@horizonhr.com", Phone = "555-0112", DateOfBirth = new DateOnly(1986, 10, 9), HireDate = new DateOnly(2019, 4, 22), Department = engineering, JobTitle = "DevOps Engineer", EmploymentType = EmploymentType.FullTime, Salary = 120000m, Status = EmployeeStatus.Terminated, TerminationDate = new DateOnly(2025, 12, 31) },
            new() { EmployeeNumber = "EMP-0013", FirstName = "Mia", LastName = "Robinson", Email = "mia.robinson@horizonhr.com", Phone = "555-0113", DateOfBirth = new DateOnly(1998, 7, 1), HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)), Department = backend, JobTitle = "Junior Developer", EmploymentType = EmploymentType.Intern, Salary = 55000m, Status = EmployeeStatus.Active },
        };

        db.Employees.AddRange(employees);
        await db.SaveChangesAsync();

        // Assign managers
        var alice = employees[0];    // VP Eng
        var bob = employees[1];      // Frontend Lead
        var carol = employees[2];    // Backend Lead
        var henry = employees[7];    // HR Director
        var jack = employees[9];     // Marketing Director

        bob.ManagerId = alice.Id;
        carol.ManagerId = alice.Id;
        employees[3].ManagerId = bob.Id;   // David -> Bob
        employees[4].ManagerId = bob.Id;   // Emily -> Bob
        employees[5].ManagerId = carol.Id; // Frank -> Carol
        employees[6].ManagerId = carol.Id; // Grace -> Carol
        employees[8].ManagerId = henry.Id; // Irene -> Henry
        employees[10].ManagerId = jack.Id; // Karen -> Jack
        employees[11].ManagerId = alice.Id; // Leo -> Alice (terminated)
        employees[12].ManagerId = carol.Id; // Mia -> Carol

        // Department managers
        engineering.ManagerId = alice.Id;
        frontend.ManagerId = bob.Id;
        backend.ManagerId = carol.Id;
        hr.ManagerId = henry.Id;
        marketing.ManagerId = jack.Id;

        await db.SaveChangesAsync();

        // ── Leave Balances (current year, all active employees) ──
        var leaveTypes = new[] { vacation, sick, personal, bereavement };
        var activeEmployees = employees.Where(e => e.Status != EmployeeStatus.Terminated).ToList();

        foreach (var emp in activeEmployees)
        {
            foreach (var lt in leaveTypes)
            {
                db.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = emp.Id,
                    LeaveTypeId = lt.Id,
                    Year = currentYear,
                    TotalDays = lt.DefaultDaysPerYear,
                    UsedDays = 0,
                    CarriedOverDays = 0
                });
            }
        }
        await db.SaveChangesAsync();

        // ── Leave Requests ──
        // Set some used days for employees with approved requests
        var bobVacBal = await db.LeaveBalances.FirstAsync(lb => lb.EmployeeId == bob.Id && lb.LeaveTypeId == vacation.Id && lb.Year == currentYear);
        bobVacBal.UsedDays = 3;
        var carolSickBal = await db.LeaveBalances.FirstAsync(lb => lb.EmployeeId == carol.Id && lb.LeaveTypeId == sick.Id && lb.Year == currentYear);
        carolSickBal.UsedDays = 2;
        var davidVacBal = await db.LeaveBalances.FirstAsync(lb => lb.EmployeeId == employees[3].Id && lb.LeaveTypeId == vacation.Id && lb.Year == currentYear);
        davidVacBal.UsedDays = 5;

        var leaveRequests = new List<LeaveRequest>
        {
            new() { Employee = bob, LeaveType = vacation, StartDate = new DateOnly(currentYear, 2, 10), EndDate = new DateOnly(currentYear, 2, 12), TotalDays = 3, Status = LeaveRequestStatus.Approved, Reason = "Family vacation", ReviewedBy = alice, ReviewDate = now.AddDays(-60), ReviewNotes = "Approved. Enjoy!", SubmittedDate = now.AddDays(-65) },
            new() { Employee = carol, LeaveType = sick, StartDate = new DateOnly(currentYear, 3, 5), EndDate = new DateOnly(currentYear, 3, 6), TotalDays = 2, Status = LeaveRequestStatus.Approved, Reason = "Flu recovery", ReviewedBy = alice, ReviewDate = now.AddDays(-30), SubmittedDate = now.AddDays(-32) },
            new() { Employee = employees[3], LeaveType = vacation, StartDate = new DateOnly(currentYear, 4, 14), EndDate = new DateOnly(currentYear, 4, 18), TotalDays = 5, Status = LeaveRequestStatus.Approved, Reason = "Spring break trip", ReviewedBy = bob, ReviewDate = now.AddDays(-10), SubmittedDate = now.AddDays(-20) },
            new() { Employee = employees[4], LeaveType = vacation, StartDate = new DateOnly(currentYear, 5, 1), EndDate = new DateOnly(currentYear, 5, 2), TotalDays = 2, Status = LeaveRequestStatus.Submitted, Reason = "Personal time off", SubmittedDate = now.AddDays(-3) },
            new() { Employee = employees[5], LeaveType = personal, StartDate = new DateOnly(currentYear, 3, 20), EndDate = new DateOnly(currentYear, 3, 20), TotalDays = 1, Status = LeaveRequestStatus.Rejected, Reason = "Moving day", ReviewedBy = carol, ReviewDate = now.AddDays(-15), ReviewNotes = "Too close to release date", SubmittedDate = now.AddDays(-18) },
            new() { Employee = employees[6], LeaveType = vacation, StartDate = new DateOnly(currentYear, 6, 1), EndDate = new DateOnly(currentYear, 6, 5), TotalDays = 5, Status = LeaveRequestStatus.Submitted, Reason = "Summer holiday", SubmittedDate = now.AddDays(-2) },
            new() { Employee = employees[8], LeaveType = sick, StartDate = new DateOnly(currentYear, 1, 15), EndDate = new DateOnly(currentYear, 1, 15), TotalDays = 1, Status = LeaveRequestStatus.Cancelled, Reason = "Doctor appointment - cancelled", SubmittedDate = now.AddDays(-80) },
            new() { Employee = employees[10], LeaveType = vacation, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), TotalDays = 4, Status = LeaveRequestStatus.Approved, Reason = "Extended personal leave", ReviewedBy = jack, ReviewDate = now.AddDays(-7), SubmittedDate = now.AddDays(-14) },
        };
        db.LeaveRequests.AddRange(leaveRequests);
        await db.SaveChangesAsync();

        // ── Skills ──
        var skills = new List<Skill>
        {
            new() { Name = "Python", Category = "Programming Language", Description = "Python programming" },
            new() { Name = "JavaScript", Category = "Programming Language", Description = "JavaScript / ES6+" },
            new() { Name = "C#", Category = "Programming Language", Description = "C# and .NET development" },
            new() { Name = "React", Category = "Framework", Description = "React frontend framework" },
            new() { Name = "ASP.NET Core", Category = "Framework", Description = "ASP.NET Core web framework" },
            new() { Name = "Docker", Category = "Tool", Description = "Container platform" },
            new() { Name = "SQL", Category = "Database", Description = "SQL and relational databases" },
            new() { Name = "Project Management", Category = "Soft Skill", Description = "Project planning and execution" },
            new() { Name = "Communication", Category = "Soft Skill", Description = "Written and verbal communication" },
            new() { Name = "TypeScript", Category = "Programming Language", Description = "Typed JavaScript" },
            new() { Name = "Kubernetes", Category = "Tool", Description = "Container orchestration" },
            new() { Name = "AWS", Category = "Cloud", Description = "Amazon Web Services" },
        };
        db.Skills.AddRange(skills);
        await db.SaveChangesAsync();

        // ── Employee Skills ──
        var employeeSkills = new List<EmployeeSkill>
        {
            new() { Employee = alice, Skill = skills[2], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 12 },   // C#
            new() { Employee = alice, Skill = skills[7], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },   // Proj Mgmt
            new() { Employee = alice, Skill = skills[8], ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 15 }, // Communication
            new() { Employee = bob, Skill = skills[1], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 8 },      // JavaScript
            new() { Employee = bob, Skill = skills[3], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 6 },      // React
            new() { Employee = bob, Skill = skills[9], ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },    // TypeScript
            new() { Employee = carol, Skill = skills[2], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 7 },    // C#
            new() { Employee = carol, Skill = skills[4], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 6 },    // ASP.NET Core
            new() { Employee = carol, Skill = skills[6], ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 8 },  // SQL
            new() { Employee = carol, Skill = skills[5], ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },  // Docker
            new() { Employee = employees[3], Skill = skills[1], ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 }, // David: JS
            new() { Employee = employees[3], Skill = skills[3], ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 }, // David: React
            new() { Employee = employees[4], Skill = skills[1], ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 }, // Emily: JS
            new() { Employee = employees[5], Skill = skills[2], ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 6 }, // Frank: C#
            new() { Employee = employees[5], Skill = skills[6], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 8 }, // Frank: SQL
            new() { Employee = employees[6], Skill = skills[0], ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 }, // Grace: Python
            new() { Employee = employees[6], Skill = skills[2], ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 }, // Grace: C#
            new() { Employee = henry, Skill = skills[7], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 12 }, // Henry: PM
            new() { Employee = henry, Skill = skills[8], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 15 }, // Henry: Communication
            new() { Employee = jack, Skill = skills[8], ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },  // Jack: Communication
            new() { Employee = jack, Skill = skills[7], ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 7 }, // Jack: PM
            new() { Employee = employees[12], Skill = skills[0], ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 0 }, // Mia: Python
        };
        db.EmployeeSkills.AddRange(employeeSkills);
        await db.SaveChangesAsync();

        // ── Performance Reviews ──
        var reviews = new List<PerformanceReview>
        {
            new() { Employee = bob, Reviewer = alice, ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.ExceedsExpectations, SelfAssessment = "I successfully led the frontend team migration to React 18 and improved page load times by 40%.", ManagerAssessment = "Bob has shown exceptional leadership. His technical guidance has elevated the entire frontend team.", Goals = "Lead the micro-frontend architecture initiative. Mentor two junior developers.", StrengthsNoted = "Technical leadership, code quality, mentoring", AreasForImprovement = "Cross-team communication could be more proactive", CompletedDate = new DateOnly(currentYear, 1, 15) },
            new() { Employee = carol, Reviewer = alice, ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.Outstanding, SelfAssessment = "Architected and delivered the new microservices platform, reducing deployment time by 60%.", ManagerAssessment = "Carol's contributions have been transformative. She is ready for a senior leadership role.", Goals = "Design the next-gen API gateway. Present at two industry conferences.", StrengthsNoted = "Architecture, system design, reliability engineering", AreasForImprovement = "Delegation — tends to take on too much individually", CompletedDate = new DateOnly(currentYear, 1, 20) },
            new() { Employee = employees[3], Reviewer = bob, ReviewPeriodStart = new DateOnly(currentYear - 1, 7, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.ManagerReviewPending, SelfAssessment = "Delivered the new dashboard component library and contributed to design system documentation." },
            new() { Employee = employees[5], Reviewer = carol, ReviewPeriodStart = new DateOnly(currentYear - 1, 7, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.SelfAssessmentPending },
            new() { Employee = employees[8], Reviewer = henry, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.Draft },
            new() { Employee = employees[4], Reviewer = bob, ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.MeetsExpectations, SelfAssessment = "Completed onboarding successfully and contributed to 3 major feature releases.", ManagerAssessment = "Emily has adapted well. Solid technical skills with room to grow in architecture.", Goals = "Take ownership of one feature end-to-end. Complete advanced React course.", StrengthsNoted = "Quick learner, attention to detail", AreasForImprovement = "Should take more initiative in code reviews", CompletedDate = new DateOnly(currentYear, 2, 1) },
        };
        db.PerformanceReviews.AddRange(reviews);
        await db.SaveChangesAsync();
    }
}
