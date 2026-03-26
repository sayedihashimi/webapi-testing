using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Departments.AnyAsync())
            return;

        // Leave Types
        var vacation = new LeaveType { Name = "Vacation", DefaultDaysPerYear = 15, IsCarryOverAllowed = true, MaxCarryOverDays = 5, RequiresApproval = true, IsPaid = true };
        var sick = new LeaveType { Name = "Sick", DefaultDaysPerYear = 10, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var personal = new LeaveType { Name = "Personal", DefaultDaysPerYear = 3, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var bereavement = new LeaveType { Name = "Bereavement", DefaultDaysPerYear = 5, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = false, IsPaid = true };

        context.LeaveTypes.AddRange(vacation, sick, personal, bereavement);
        await context.SaveChangesAsync();

        // Departments (without managers first)
        var engineering = new Department { Name = "Engineering", Code = "ENG", Description = "Software engineering and development" };
        var frontend = new Department { Name = "Frontend Engineering", Code = "FE", Description = "Frontend development team" };
        var backend = new Department { Name = "Backend Engineering", Code = "BE", Description = "Backend development team" };
        var hr = new Department { Name = "Human Resources", Code = "HR", Description = "HR and people operations" };
        var marketing = new Department { Name = "Marketing", Code = "MKT", Description = "Marketing and communications" };

        context.Departments.AddRange(engineering, frontend, backend, hr, marketing);
        await context.SaveChangesAsync();

        // Set parent departments
        frontend.ParentDepartmentId = engineering.Id;
        backend.ParentDepartmentId = engineering.Id;
        await context.SaveChangesAsync();

        int currentYear = DateTime.UtcNow.Year;

        // Employees
        var employees = new List<Employee>
        {
            new Employee { EmployeeNumber = "EMP-0001", FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@horizon.com", Phone = "555-0101", DateOfBirth = new DateOnly(1985, 3, 15), HireDate = new DateOnly(2020, 1, 10), DepartmentId = engineering.Id, JobTitle = "VP of Engineering", EmploymentType = EmploymentType.FullTime, Salary = 180000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0002", FirstName = "Bob", LastName = "Smith", Email = "bob.smith@horizon.com", Phone = "555-0102", DateOfBirth = new DateOnly(1990, 7, 22), HireDate = new DateOnly(2021, 3, 1), DepartmentId = frontend.Id, JobTitle = "Frontend Lead", EmploymentType = EmploymentType.FullTime, Salary = 140000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0003", FirstName = "Carol", LastName = "Williams", Email = "carol.williams@horizon.com", Phone = "555-0103", DateOfBirth = new DateOnly(1992, 11, 5), HireDate = new DateOnly(2022, 6, 15), DepartmentId = frontend.Id, JobTitle = "Senior Frontend Developer", EmploymentType = EmploymentType.FullTime, Salary = 120000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0004", FirstName = "David", LastName = "Brown", Email = "david.brown@horizon.com", Phone = "555-0104", DateOfBirth = new DateOnly(1988, 5, 30), HireDate = new DateOnly(2021, 9, 1), DepartmentId = backend.Id, JobTitle = "Backend Lead", EmploymentType = EmploymentType.FullTime, Salary = 145000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0005", FirstName = "Eve", LastName = "Davis", Email = "eve.davis@horizon.com", Phone = "555-0105", DateOfBirth = new DateOnly(1995, 8, 12), HireDate = new DateOnly(2023, 1, 15), DepartmentId = backend.Id, JobTitle = "Backend Developer", EmploymentType = EmploymentType.FullTime, Salary = 110000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0006", FirstName = "Frank", LastName = "Miller", Email = "frank.miller@horizon.com", Phone = "555-0106", DateOfBirth = new DateOnly(1993, 2, 28), HireDate = new DateOnly(2023, 4, 1), DepartmentId = backend.Id, JobTitle = "Junior Developer", EmploymentType = EmploymentType.Contract, Salary = 85000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0007", FirstName = "Grace", LastName = "Wilson", Email = "grace.wilson@horizon.com", Phone = "555-0107", DateOfBirth = new DateOnly(1987, 9, 18), HireDate = new DateOnly(2019, 11, 1), DepartmentId = hr.Id, JobTitle = "HR Director", EmploymentType = EmploymentType.FullTime, Salary = 130000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0008", FirstName = "Henry", LastName = "Moore", Email = "henry.moore@horizon.com", Phone = "555-0108", DateOfBirth = new DateOnly(1991, 12, 10), HireDate = new DateOnly(2022, 2, 14), DepartmentId = hr.Id, JobTitle = "HR Specialist", EmploymentType = EmploymentType.FullTime, Salary = 75000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0009", FirstName = "Ivy", LastName = "Taylor", Email = "ivy.taylor@horizon.com", Phone = "555-0109", DateOfBirth = new DateOnly(1994, 4, 25), HireDate = new DateOnly(2021, 7, 1), DepartmentId = marketing.Id, JobTitle = "Marketing Manager", EmploymentType = EmploymentType.FullTime, Salary = 110000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0010", FirstName = "Jack", LastName = "Anderson", Email = "jack.anderson@horizon.com", Phone = "555-0110", DateOfBirth = new DateOnly(1996, 6, 8), HireDate = new DateOnly(2023, 8, 1), DepartmentId = marketing.Id, JobTitle = "Content Specialist", EmploymentType = EmploymentType.PartTime, Salary = 55000m, Status = EmployeeStatus.Active },
            new Employee { EmployeeNumber = "EMP-0011", FirstName = "Karen", LastName = "Thomas", Email = "karen.thomas@horizon.com", Phone = "555-0111", DateOfBirth = new DateOnly(1989, 10, 3), HireDate = new DateOnly(2020, 5, 1), DepartmentId = frontend.Id, JobTitle = "UI Designer", EmploymentType = EmploymentType.FullTime, Salary = 95000m, Status = EmployeeStatus.OnLeave },
            new Employee { EmployeeNumber = "EMP-0012", FirstName = "Leo", LastName = "Jackson", Email = "leo.jackson@horizon.com", Phone = "555-0112", DateOfBirth = new DateOnly(1991, 1, 20), HireDate = new DateOnly(2021, 3, 15), DepartmentId = backend.Id, JobTitle = "DevOps Engineer", EmploymentType = EmploymentType.FullTime, Salary = 125000m, Status = EmployeeStatus.Terminated, TerminationDate = new DateOnly(2025, 12, 31) },
            new Employee { EmployeeNumber = "EMP-0013", FirstName = "Mia", LastName = "White", Email = "mia.white@horizon.com", Phone = "555-0113", DateOfBirth = new DateOnly(2000, 3, 14), HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)), DepartmentId = engineering.Id, JobTitle = "Engineering Intern", EmploymentType = EmploymentType.Intern, Salary = 45000m, Status = EmployeeStatus.Active },
        };

        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();

        // Set managers
        var alice = employees[0]; // VP Eng
        var bob = employees[1];   // Frontend Lead
        var carol = employees[2];
        var david = employees[3]; // Backend Lead
        var eve = employees[4];
        var frank = employees[5];
        var grace = employees[6]; // HR Director
        var henry = employees[7];
        var ivy = employees[8];   // Marketing Manager
        var jack = employees[9];
        var karen = employees[10];
        var leo = employees[11]; // terminated
        var mia = employees[12];

        // Manager relationships
        bob.ManagerId = alice.Id;
        david.ManagerId = alice.Id;
        carol.ManagerId = bob.Id;
        karen.ManagerId = bob.Id;
        eve.ManagerId = david.Id;
        frank.ManagerId = david.Id;
        henry.ManagerId = grace.Id;
        jack.ManagerId = ivy.Id;
        mia.ManagerId = alice.Id;

        // Department managers
        engineering.ManagerId = alice.Id;
        frontend.ManagerId = bob.Id;
        backend.ManagerId = david.Id;
        hr.ManagerId = grace.Id;
        marketing.ManagerId = ivy.Id;

        await context.SaveChangesAsync();

        // Leave Balances for all active employees
        var leaveTypes = new[] { vacation, sick, personal, bereavement };
        var activeEmployees = employees.Where(e => e.Status != EmployeeStatus.Terminated).ToList();
        foreach (var emp in activeEmployees)
        {
            foreach (var lt in leaveTypes)
            {
                context.LeaveBalances.Add(new LeaveBalance
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
        await context.SaveChangesAsync();

        // Update some used days for balances
        var aliceVacBal = await context.LeaveBalances.FirstAsync(lb => lb.EmployeeId == alice.Id && lb.LeaveTypeId == vacation.Id);
        aliceVacBal.UsedDays = 3;

        var bobSickBal = await context.LeaveBalances.FirstAsync(lb => lb.EmployeeId == bob.Id && lb.LeaveTypeId == sick.Id);
        bobSickBal.UsedDays = 2;

        var carolVacBal = await context.LeaveBalances.FirstAsync(lb => lb.EmployeeId == carol.Id && lb.LeaveTypeId == vacation.Id);
        carolVacBal.UsedDays = 5;

        var karenVacBal = await context.LeaveBalances.FirstAsync(lb => lb.EmployeeId == karen.Id && lb.LeaveTypeId == vacation.Id);
        karenVacBal.UsedDays = 5;

        await context.SaveChangesAsync();

        // Leave Requests
        var leaveRequests = new List<LeaveRequest>
        {
            new LeaveRequest { EmployeeId = alice.Id, LeaveTypeId = vacation.Id, StartDate = new DateOnly(currentYear, 3, 10), EndDate = new DateOnly(currentYear, 3, 12), TotalDays = 3, Status = LeaveRequestStatus.Approved, Reason = "Family vacation", ReviewedById = grace.Id, ReviewDate = DateTime.UtcNow.AddDays(-30), ReviewNotes = "Approved. Enjoy!", SubmittedDate = DateTime.UtcNow.AddDays(-35) },
            new LeaveRequest { EmployeeId = bob.Id, LeaveTypeId = sick.Id, StartDate = new DateOnly(currentYear, 2, 5), EndDate = new DateOnly(currentYear, 2, 6), TotalDays = 2, Status = LeaveRequestStatus.Approved, Reason = "Flu recovery", ReviewedById = alice.Id, ReviewDate = DateTime.UtcNow.AddDays(-50), ReviewNotes = "Get well soon", SubmittedDate = DateTime.UtcNow.AddDays(-52) },
            new LeaveRequest { EmployeeId = carol.Id, LeaveTypeId = vacation.Id, StartDate = new DateOnly(currentYear, 6, 1), EndDate = new DateOnly(currentYear, 6, 7), TotalDays = 5, Status = LeaveRequestStatus.Approved, Reason = "Summer trip", ReviewedById = bob.Id, ReviewDate = DateTime.UtcNow.AddDays(-10), SubmittedDate = DateTime.UtcNow.AddDays(-15) },
            new LeaveRequest { EmployeeId = eve.Id, LeaveTypeId = personal.Id, StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), TotalDays = 1, Status = LeaveRequestStatus.Submitted, Reason = "Personal appointment", SubmittedDate = DateTime.UtcNow.AddDays(-2) },
            new LeaveRequest { EmployeeId = frank.Id, LeaveTypeId = vacation.Id, StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(24)), TotalDays = 5, Status = LeaveRequestStatus.Submitted, Reason = "Holiday travel", SubmittedDate = DateTime.UtcNow.AddDays(-1) },
            new LeaveRequest { EmployeeId = henry.Id, LeaveTypeId = sick.Id, StartDate = new DateOnly(currentYear, 1, 15), EndDate = new DateOnly(currentYear, 1, 15), TotalDays = 1, Status = LeaveRequestStatus.Rejected, Reason = "Dental appointment", ReviewedById = grace.Id, ReviewDate = DateTime.UtcNow.AddDays(-60), ReviewNotes = "Please use personal day instead", SubmittedDate = DateTime.UtcNow.AddDays(-62) },
            new LeaveRequest { EmployeeId = jack.Id, LeaveTypeId = vacation.Id, StartDate = new DateOnly(currentYear, 4, 1), EndDate = new DateOnly(currentYear, 4, 3), TotalDays = 3, Status = LeaveRequestStatus.Cancelled, Reason = "Spring break trip", SubmittedDate = DateTime.UtcNow.AddDays(-40) },
            new LeaveRequest { EmployeeId = karen.Id, LeaveTypeId = vacation.Id, StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)), EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), TotalDays = 5, Status = LeaveRequestStatus.Approved, Reason = "Extended vacation", ReviewedById = bob.Id, ReviewDate = DateTime.UtcNow.AddDays(-10), SubmittedDate = DateTime.UtcNow.AddDays(-15) },
        };

        context.LeaveRequests.AddRange(leaveRequests);
        await context.SaveChangesAsync();

        // Performance Reviews
        var reviews = new List<PerformanceReview>
        {
            new PerformanceReview { EmployeeId = bob.Id, ReviewerId = alice.Id, ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.ExceedsExpectations, SelfAssessment = "I led the frontend team through a major redesign.", ManagerAssessment = "Bob exceeded expectations with excellent leadership.", Goals = "Lead migration to new framework", StrengthsNoted = "Strong leadership and technical skills", AreasForImprovement = "Delegation could improve", CompletedDate = new DateOnly(currentYear, 1, 15) },
            new PerformanceReview { EmployeeId = carol.Id, ReviewerId = bob.Id, ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.MeetsExpectations, SelfAssessment = "Delivered all assigned projects on time.", ManagerAssessment = "Carol consistently meets expectations.", Goals = "Take on more complex projects", StrengthsNoted = "Reliable and detail-oriented", AreasForImprovement = "Could be more proactive", CompletedDate = new DateOnly(currentYear, 1, 20) },
            new PerformanceReview { EmployeeId = eve.Id, ReviewerId = david.Id, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.Draft, Goals = "Improve API design skills" },
            new PerformanceReview { EmployeeId = frank.Id, ReviewerId = david.Id, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.SelfAssessmentPending },
            new PerformanceReview { EmployeeId = henry.Id, ReviewerId = grace.Id, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.ManagerReviewPending, SelfAssessment = "I have streamlined the onboarding process significantly." },
            new PerformanceReview { EmployeeId = david.Id, ReviewerId = alice.Id, ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.Outstanding, SelfAssessment = "Built a scalable microservices architecture.", ManagerAssessment = "David is an outstanding technical leader.", Goals = "Mentor junior developers", StrengthsNoted = "Architecture and mentoring", AreasForImprovement = "Work-life balance", CompletedDate = new DateOnly(currentYear, 1, 10) },
        };

        context.PerformanceReviews.AddRange(reviews);
        await context.SaveChangesAsync();

        // Skills
        var skills = new List<Skill>
        {
            new Skill { Name = "Python", Category = "Programming Language", Description = "Python programming language" },
            new Skill { Name = "JavaScript", Category = "Programming Language", Description = "JavaScript/ECMAScript" },
            new Skill { Name = "C#", Category = "Programming Language", Description = "C# programming language" },
            new Skill { Name = "React", Category = "Framework", Description = "React frontend framework" },
            new Skill { Name = "ASP.NET Core", Category = "Framework", Description = "ASP.NET Core web framework" },
            new Skill { Name = "Docker", Category = "Tool", Description = "Container platform" },
            new Skill { Name = "SQL", Category = "Database", Description = "SQL database querying" },
            new Skill { Name = "Project Management", Category = "Soft Skill", Description = "Managing projects and timelines" },
            new Skill { Name = "Communication", Category = "Soft Skill", Description = "Effective communication skills" },
            new Skill { Name = "Git", Category = "Tool", Description = "Version control system" },
            new Skill { Name = "TypeScript", Category = "Programming Language", Description = "TypeScript superset of JavaScript" },
            new Skill { Name = "Kubernetes", Category = "Tool", Description = "Container orchestration platform" },
        };

        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();

        // Employee Skills (20+ records)
        var employeeSkills = new List<EmployeeSkill>
        {
            new EmployeeSkill { EmployeeId = alice.Id, SkillId = skills[2].Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 15, LastAssessedDate = new DateOnly(currentYear, 1, 1) },
            new EmployeeSkill { EmployeeId = alice.Id, SkillId = skills[7].Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new EmployeeSkill { EmployeeId = alice.Id, SkillId = skills[8].Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 12 },
            new EmployeeSkill { EmployeeId = bob.Id, SkillId = skills[1].Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 8 },
            new EmployeeSkill { EmployeeId = bob.Id, SkillId = skills[3].Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 6 },
            new EmployeeSkill { EmployeeId = bob.Id, SkillId = skills[10].Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { EmployeeId = carol.Id, SkillId = skills[1].Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new EmployeeSkill { EmployeeId = carol.Id, SkillId = skills[3].Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },
            new EmployeeSkill { EmployeeId = david.Id, SkillId = skills[2].Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new EmployeeSkill { EmployeeId = david.Id, SkillId = skills[4].Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 8 },
            new EmployeeSkill { EmployeeId = david.Id, SkillId = skills[5].Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { EmployeeId = david.Id, SkillId = skills[6].Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new EmployeeSkill { EmployeeId = eve.Id, SkillId = skills[0].Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },
            new EmployeeSkill { EmployeeId = eve.Id, SkillId = skills[6].Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },
            new EmployeeSkill { EmployeeId = frank.Id, SkillId = skills[2].Id, ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },
            new EmployeeSkill { EmployeeId = frank.Id, SkillId = skills[9].Id, ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },
            new EmployeeSkill { EmployeeId = grace.Id, SkillId = skills[7].Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 12 },
            new EmployeeSkill { EmployeeId = grace.Id, SkillId = skills[8].Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 15 },
            new EmployeeSkill { EmployeeId = ivy.Id, SkillId = skills[7].Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 6 },
            new EmployeeSkill { EmployeeId = ivy.Id, SkillId = skills[8].Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { EmployeeId = karen.Id, SkillId = skills[1].Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { EmployeeId = karen.Id, SkillId = skills[3].Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new EmployeeSkill { EmployeeId = mia.Id, SkillId = skills[0].Id, ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },
        };

        context.EmployeeSkills.AddRange(employeeSkills);
        await context.SaveChangesAsync();
    }
}
