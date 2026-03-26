using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Data;

public sealed class DataSeeder
{
    public static async Task SeedAsync(HorizonDbContext db)
    {
        if (await db.Departments.AnyAsync())
            return;

        // Leave Types
        var vacation = new LeaveType { Id = 1, Name = "Vacation", DefaultDaysPerYear = 15, IsCarryOverAllowed = true, MaxCarryOverDays = 5, RequiresApproval = true, IsPaid = true };
        var sick = new LeaveType { Id = 2, Name = "Sick", DefaultDaysPerYear = 10, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var personal = new LeaveType { Id = 3, Name = "Personal", DefaultDaysPerYear = 3, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var bereavement = new LeaveType { Id = 4, Name = "Bereavement", DefaultDaysPerYear = 5, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = false, IsPaid = true };
        db.LeaveTypes.AddRange(vacation, sick, personal, bereavement);

        // Departments (no managers yet)
        var engineering = new Department { Id = 1, Name = "Engineering", Code = "ENG", Description = "Software engineering department", IsActive = true };
        var frontend = new Department { Id = 2, Name = "Frontend Engineering", Code = "FE", Description = "Frontend development team", ParentDepartmentId = 1, IsActive = true };
        var backend = new Department { Id = 3, Name = "Backend Engineering", Code = "BE", Description = "Backend development team", ParentDepartmentId = 1, IsActive = true };
        var hr = new Department { Id = 4, Name = "Human Resources", Code = "HR", Description = "Human resources department", IsActive = true };
        var marketing = new Department { Id = 5, Name = "Marketing", Code = "MKT", Description = "Marketing and communications", IsActive = true };
        db.Departments.AddRange(engineering, frontend, backend, hr, marketing);
        await db.SaveChangesAsync();

        // Employees
        var employees = new List<Employee>
        {
            new() { Id = 1, EmployeeNumber = "EMP-0001", FirstName = "Sarah", LastName = "Chen", Email = "sarah.chen@horizonhr.com", Phone = "555-0101", DateOfBirth = new DateOnly(1985, 3, 15), HireDate = new DateOnly(2019, 1, 10), DepartmentId = 1, JobTitle = "VP of Engineering", EmploymentType = EmploymentType.FullTime, Salary = 180000m, Status = EmployeeStatus.Active, City = "San Francisco", State = "CA", ZipCode = "94105" },
            new() { Id = 2, EmployeeNumber = "EMP-0002", FirstName = "Marcus", LastName = "Johnson", Email = "marcus.johnson@horizonhr.com", Phone = "555-0102", DateOfBirth = new DateOnly(1990, 7, 22), HireDate = new DateOnly(2020, 3, 15), DepartmentId = 2, JobTitle = "Frontend Lead", EmploymentType = EmploymentType.FullTime, Salary = 145000m, ManagerId = 1, Status = EmployeeStatus.Active, City = "San Francisco", State = "CA", ZipCode = "94105" },
            new() { Id = 3, EmployeeNumber = "EMP-0003", FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@horizonhr.com", Phone = "555-0103", DateOfBirth = new DateOnly(1992, 11, 8), HireDate = new DateOnly(2021, 6, 1), DepartmentId = 2, JobTitle = "Senior Frontend Developer", EmploymentType = EmploymentType.FullTime, Salary = 130000m, ManagerId = 2, Status = EmployeeStatus.Active, City = "Austin", State = "TX", ZipCode = "73301" },
            new() { Id = 4, EmployeeNumber = "EMP-0004", FirstName = "David", LastName = "Kim", Email = "david.kim@horizonhr.com", Phone = "555-0104", DateOfBirth = new DateOnly(1988, 5, 30), HireDate = new DateOnly(2020, 9, 1), DepartmentId = 3, JobTitle = "Backend Lead", EmploymentType = EmploymentType.FullTime, Salary = 150000m, ManagerId = 1, Status = EmployeeStatus.Active, City = "Seattle", State = "WA", ZipCode = "98101" },
            new() { Id = 5, EmployeeNumber = "EMP-0005", FirstName = "Priya", LastName = "Patel", Email = "priya.patel@horizonhr.com", Phone = "555-0105", DateOfBirth = new DateOnly(1993, 2, 14), HireDate = new DateOnly(2022, 1, 15), DepartmentId = 3, JobTitle = "Backend Developer", EmploymentType = EmploymentType.FullTime, Salary = 120000m, ManagerId = 4, Status = EmployeeStatus.Active, City = "Seattle", State = "WA", ZipCode = "98102" },
            new() { Id = 6, EmployeeNumber = "EMP-0006", FirstName = "James", LastName = "Wilson", Email = "james.wilson@horizonhr.com", Phone = "555-0106", DateOfBirth = new DateOnly(1987, 9, 3), HireDate = new DateOnly(2018, 4, 1), DepartmentId = 4, JobTitle = "HR Director", EmploymentType = EmploymentType.FullTime, Salary = 140000m, Status = EmployeeStatus.Active, City = "San Francisco", State = "CA", ZipCode = "94105" },
            new() { Id = 7, EmployeeNumber = "EMP-0007", FirstName = "Lisa", LastName = "Thompson", Email = "lisa.thompson@horizonhr.com", Phone = "555-0107", DateOfBirth = new DateOnly(1991, 12, 20), HireDate = new DateOnly(2021, 2, 1), DepartmentId = 4, JobTitle = "HR Specialist", EmploymentType = EmploymentType.FullTime, Salary = 85000m, ManagerId = 6, Status = EmployeeStatus.Active, City = "San Francisco", State = "CA", ZipCode = "94106" },
            new() { Id = 8, EmployeeNumber = "EMP-0008", FirstName = "Alex", LastName = "Martinez", Email = "alex.martinez@horizonhr.com", Phone = "555-0108", DateOfBirth = new DateOnly(1994, 6, 11), HireDate = new DateOnly(2023, 7, 10), DepartmentId = 5, JobTitle = "Marketing Manager", EmploymentType = EmploymentType.FullTime, Salary = 110000m, Status = EmployeeStatus.Active, City = "New York", State = "NY", ZipCode = "10001" },
            new() { Id = 9, EmployeeNumber = "EMP-0009", FirstName = "Rachel", LastName = "Green", Email = "rachel.green@horizonhr.com", Phone = "555-0109", DateOfBirth = new DateOnly(1995, 8, 25), HireDate = new DateOnly(2023, 9, 1), DepartmentId = 5, JobTitle = "Content Strategist", EmploymentType = EmploymentType.PartTime, Salary = 65000m, ManagerId = 8, Status = EmployeeStatus.Active, City = "New York", State = "NY", ZipCode = "10002" },
            new() { Id = 10, EmployeeNumber = "EMP-0010", FirstName = "Tom", LastName = "Baker", Email = "tom.baker@horizonhr.com", Phone = "555-0110", DateOfBirth = new DateOnly(1996, 4, 5), HireDate = new DateOnly(2024, 1, 8), DepartmentId = 3, JobTitle = "Junior Developer", EmploymentType = EmploymentType.Intern, Salary = 55000m, ManagerId = 4, Status = EmployeeStatus.Active, City = "Seattle", State = "WA", ZipCode = "98103" },
            new() { Id = 11, EmployeeNumber = "EMP-0011", FirstName = "Jennifer", LastName = "Lee", Email = "jennifer.lee@horizonhr.com", Phone = "555-0111", DateOfBirth = new DateOnly(1989, 10, 17), HireDate = new DateOnly(2020, 5, 20), DepartmentId = 2, JobTitle = "UI Designer", EmploymentType = EmploymentType.Contract, Salary = 95000m, ManagerId = 2, Status = EmployeeStatus.OnLeave, City = "Portland", State = "OR", ZipCode = "97201" },
            new() { Id = 12, EmployeeNumber = "EMP-0012", FirstName = "Michael", LastName = "Brown", Email = "michael.brown@horizonhr.com", Phone = "555-0112", DateOfBirth = new DateOnly(1986, 1, 30), HireDate = new DateOnly(2019, 8, 12), DepartmentId = 1, JobTitle = "DevOps Engineer", EmploymentType = EmploymentType.FullTime, Salary = 135000m, ManagerId = 1, Status = EmployeeStatus.Terminated, TerminationDate = new DateOnly(2025, 12, 31), City = "San Francisco", State = "CA", ZipCode = "94107" },
            new() { Id = 13, EmployeeNumber = "EMP-0013", FirstName = "Olivia", LastName = "Davis", Email = "olivia.davis@horizonhr.com", Phone = "555-0113", DateOfBirth = new DateOnly(1997, 3, 8), HireDate = new DateOnly(2026, 3, 1), DepartmentId = 2, JobTitle = "Frontend Developer", EmploymentType = EmploymentType.FullTime, Salary = 105000m, ManagerId = 2, Status = EmployeeStatus.Active, City = "Austin", State = "TX", ZipCode = "73301" },
        };
        db.Employees.AddRange(employees);
        await db.SaveChangesAsync();

        // Set department managers
        engineering.ManagerId = 1;
        frontend.ManagerId = 2;
        backend.ManagerId = 4;
        hr.ManagerId = 6;
        marketing.ManagerId = 8;
        await db.SaveChangesAsync();

        // Leave Balances for active employees
        var currentYear = DateTime.UtcNow.Year;
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

        // Adjust some used days to match approved leave requests
        var sarahVacBal = await db.LeaveBalances.FirstAsync(lb => lb.EmployeeId == 1 && lb.LeaveTypeId == 1 && lb.Year == currentYear);
        sarahVacBal.UsedDays = 5;
        var marcusSickBal = await db.LeaveBalances.FirstAsync(lb => lb.EmployeeId == 2 && lb.LeaveTypeId == 2 && lb.Year == currentYear);
        marcusSickBal.UsedDays = 2;
        var jenniferVacBal = await db.LeaveBalances.FirstAsync(lb => lb.EmployeeId == 11 && lb.LeaveTypeId == 1 && lb.Year == currentYear);
        jenniferVacBal.UsedDays = 3;
        var priyaPersonalBal = await db.LeaveBalances.FirstAsync(lb => lb.EmployeeId == 5 && lb.LeaveTypeId == 3 && lb.Year == currentYear);
        priyaPersonalBal.UsedDays = 1;

        // Leave Requests
        db.LeaveRequests.AddRange(
            new LeaveRequest { EmployeeId = 1, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 2, 10), EndDate = new DateOnly(currentYear, 2, 14), TotalDays = 5, Status = LeaveRequestStatus.Approved, Reason = "Family vacation", ReviewedById = 6, ReviewDate = new DateTime(currentYear, 2, 5), ReviewNotes = "Approved. Enjoy!", SubmittedDate = new DateTime(currentYear, 1, 28) },
            new LeaveRequest { EmployeeId = 2, LeaveTypeId = 2, StartDate = new DateOnly(currentYear, 3, 3), EndDate = new DateOnly(currentYear, 3, 4), TotalDays = 2, Status = LeaveRequestStatus.Approved, Reason = "Flu recovery", ReviewedById = 1, ReviewDate = new DateTime(currentYear, 3, 2), ReviewNotes = "Get well soon", SubmittedDate = new DateTime(currentYear, 3, 1) },
            new LeaveRequest { EmployeeId = 3, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 4, 21), EndDate = new DateOnly(currentYear, 4, 25), TotalDays = 5, Status = LeaveRequestStatus.Submitted, Reason = "Spring break trip", SubmittedDate = new DateTime(currentYear, 3, 15) },
            new LeaveRequest { EmployeeId = 5, LeaveTypeId = 3, StartDate = new DateOnly(currentYear, 3, 20), EndDate = new DateOnly(currentYear, 3, 20), TotalDays = 1, Status = LeaveRequestStatus.Approved, Reason = "Personal appointment", ReviewedById = 4, ReviewDate = new DateTime(currentYear, 3, 18), SubmittedDate = new DateTime(currentYear, 3, 15) },
            new LeaveRequest { EmployeeId = 7, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 5, 5), EndDate = new DateOnly(currentYear, 5, 9), TotalDays = 5, Status = LeaveRequestStatus.Submitted, Reason = "Beach vacation", SubmittedDate = new DateTime(currentYear, 3, 20) },
            new LeaveRequest { EmployeeId = 9, LeaveTypeId = 2, StartDate = new DateOnly(currentYear, 3, 10), EndDate = new DateOnly(currentYear, 3, 10), TotalDays = 1, Status = LeaveRequestStatus.Rejected, Reason = "Doctor visit", ReviewedById = 8, ReviewDate = new DateTime(currentYear, 3, 9), ReviewNotes = "Please reschedule to a non-deadline day", SubmittedDate = new DateTime(currentYear, 3, 8) },
            new LeaveRequest { EmployeeId = 11, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 3, 24), EndDate = new DateOnly(currentYear, 3, 26), TotalDays = 3, Status = LeaveRequestStatus.Approved, Reason = "Family event", ReviewedById = 2, ReviewDate = new DateTime(currentYear, 3, 20), SubmittedDate = new DateTime(currentYear, 3, 18) },
            new LeaveRequest { EmployeeId = 4, LeaveTypeId = 1, StartDate = new DateOnly(currentYear, 6, 2), EndDate = new DateOnly(currentYear, 6, 6), TotalDays = 5, Status = LeaveRequestStatus.Cancelled, Reason = "Summer trip - cancelled", SubmittedDate = new DateTime(currentYear, 3, 1) }
        );
        await db.SaveChangesAsync();

        // Performance Reviews
        db.PerformanceReviews.AddRange(
            new PerformanceReview { EmployeeId = 2, ReviewerId = 1, ReviewPeriodStart = new DateOnly(currentYear - 1, 7, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.ExceedsExpectations, SelfAssessment = "Led the frontend migration to React 19 and mentored two junior developers.", ManagerAssessment = "Marcus consistently delivers high-quality work and shows strong leadership.", Goals = "Take on more architectural decisions, mentor the new hires.", StrengthsNoted = "Technical leadership, code quality, mentorship", AreasForImprovement = "Cross-team communication could improve", CompletedDate = new DateOnly(currentYear, 1, 15) },
            new PerformanceReview { EmployeeId = 3, ReviewerId = 2, ReviewPeriodStart = new DateOnly(currentYear - 1, 7, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.MeetsExpectations, SelfAssessment = "Delivered multiple features on time and improved test coverage.", ManagerAssessment = "Emily is a reliable contributor who meets deadlines consistently.", Goals = "Learn backend skills, contribute to system design.", StrengthsNoted = "Reliability, attention to detail", AreasForImprovement = "Could take more initiative on new projects", CompletedDate = new DateOnly(currentYear, 1, 20) },
            new PerformanceReview { EmployeeId = 5, ReviewerId = 4, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.Draft, SelfAssessment = null, ManagerAssessment = null },
            new PerformanceReview { EmployeeId = 7, ReviewerId = 6, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.SelfAssessmentPending },
            new PerformanceReview { EmployeeId = 9, ReviewerId = 8, ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30), Status = ReviewStatus.ManagerReviewPending, SelfAssessment = "I have adapted well to the part-time role and produced engaging content for social media and blog." },
            new PerformanceReview { EmployeeId = 4, ReviewerId = 1, ReviewPeriodStart = new DateOnly(currentYear - 1, 7, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31), Status = ReviewStatus.Completed, OverallRating = OverallRating.Outstanding, SelfAssessment = "Designed and implemented the new microservices architecture.", ManagerAssessment = "David is an exceptional engineer and leader. His architecture work has been transformative.", Goals = "Continue architectural leadership, explore AI integration.", StrengthsNoted = "Architecture, leadership, problem solving", AreasForImprovement = "Documentation could be more thorough", CompletedDate = new DateOnly(currentYear, 1, 10) }
        );
        await db.SaveChangesAsync();

        // Skills
        var skills = new List<Skill>
        {
            new() { Id = 1, Name = "Python", Category = "Programming Language", Description = "General-purpose programming language" },
            new() { Id = 2, Name = "JavaScript", Category = "Programming Language", Description = "Web programming language" },
            new() { Id = 3, Name = "C#", Category = "Programming Language", Description = ".NET programming language" },
            new() { Id = 4, Name = "React", Category = "Framework", Description = "Frontend JavaScript library" },
            new() { Id = 5, Name = "ASP.NET Core", Category = "Framework", Description = "Web framework for .NET" },
            new() { Id = 6, Name = "Docker", Category = "Tool", Description = "Containerization platform" },
            new() { Id = 7, Name = "SQL", Category = "Programming Language", Description = "Database query language" },
            new() { Id = 8, Name = "Project Management", Category = "Soft Skill", Description = "Planning and executing projects" },
            new() { Id = 9, Name = "Communication", Category = "Soft Skill", Description = "Effective verbal and written communication" },
            new() { Id = 10, Name = "Git", Category = "Tool", Description = "Version control system" },
            new() { Id = 11, Name = "TypeScript", Category = "Programming Language", Description = "Typed superset of JavaScript" },
            new() { Id = 12, Name = "Kubernetes", Category = "Tool", Description = "Container orchestration platform" },
        };
        db.Skills.AddRange(skills);
        await db.SaveChangesAsync();

        // Employee Skills
        db.EmployeeSkills.AddRange(
            new EmployeeSkill { EmployeeId = 1, SkillId = 3, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 12, LastAssessedDate = new DateOnly(currentYear, 1, 1) },
            new EmployeeSkill { EmployeeId = 1, SkillId = 5, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 8, LastAssessedDate = new DateOnly(currentYear, 1, 1) },
            new EmployeeSkill { EmployeeId = 1, SkillId = 8, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 6 },
            new EmployeeSkill { EmployeeId = 2, SkillId = 2, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 8, LastAssessedDate = new DateOnly(currentYear, 1, 1) },
            new EmployeeSkill { EmployeeId = 2, SkillId = 4, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 5 },
            new EmployeeSkill { EmployeeId = 2, SkillId = 11, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new EmployeeSkill { EmployeeId = 3, SkillId = 2, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4, LastAssessedDate = new DateOnly(currentYear, 1, 1) },
            new EmployeeSkill { EmployeeId = 3, SkillId = 4, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 3 },
            new EmployeeSkill { EmployeeId = 4, SkillId = 3, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10, LastAssessedDate = new DateOnly(currentYear, 1, 1) },
            new EmployeeSkill { EmployeeId = 4, SkillId = 5, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 6 },
            new EmployeeSkill { EmployeeId = 4, SkillId = 6, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { EmployeeId = 4, SkillId = 7, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new EmployeeSkill { EmployeeId = 5, SkillId = 1, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },
            new EmployeeSkill { EmployeeId = 5, SkillId = 3, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },
            new EmployeeSkill { EmployeeId = 5, SkillId = 7, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 3 },
            new EmployeeSkill { EmployeeId = 6, SkillId = 8, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new EmployeeSkill { EmployeeId = 6, SkillId = 9, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 12 },
            new EmployeeSkill { EmployeeId = 8, SkillId = 9, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new EmployeeSkill { EmployeeId = 8, SkillId = 8, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },
            new EmployeeSkill { EmployeeId = 10, SkillId = 1, ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },
            new EmployeeSkill { EmployeeId = 10, SkillId = 10, ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },
            new EmployeeSkill { EmployeeId = 13, SkillId = 2, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },
            new EmployeeSkill { EmployeeId = 13, SkillId = 4, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 1 }
        );
        await db.SaveChangesAsync();
    }
}
