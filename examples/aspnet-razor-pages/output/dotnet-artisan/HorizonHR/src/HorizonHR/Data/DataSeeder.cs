using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Data;

public sealed class DataSeeder(ApplicationDbContext db)
{
    public async Task SeedAsync()
    {
        if (await db.Departments.AnyAsync())
        {
            return;
        }

        // --- Departments ---
        var engineering = new Department { Name = "Engineering", Code = "ENG", Description = "Software engineering and development" };
        var frontend = new Department { Name = "Frontend Engineering", Code = "FE", Description = "Frontend web development", ParentDepartment = engineering };
        var backend = new Department { Name = "Backend Engineering", Code = "BE", Description = "Backend services and APIs", ParentDepartment = engineering };
        var hr = new Department { Name = "Human Resources", Code = "HR", Description = "People operations and HR management" };
        var marketing = new Department { Name = "Marketing", Code = "MKT", Description = "Marketing and communications" };

        db.Departments.AddRange(engineering, frontend, backend, hr, marketing);
        await db.SaveChangesAsync();

        // --- Employees ---
        var employees = new List<Employee>
        {
            new() { EmployeeNumber = "EMP-0001", FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@horizonhr.com", Phone = "555-0101",
                DateOfBirth = new DateOnly(1985, 3, 15), HireDate = new DateOnly(2018, 1, 10), Department = engineering,
                JobTitle = "VP of Engineering", EmploymentType = EmploymentType.FullTime, Salary = 180000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0002", FirstName = "Bob", LastName = "Smith", Email = "bob.smith@horizonhr.com", Phone = "555-0102",
                DateOfBirth = new DateOnly(1990, 7, 22), HireDate = new DateOnly(2019, 3, 5), Department = frontend,
                JobTitle = "Frontend Lead", EmploymentType = EmploymentType.FullTime, Salary = 145000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0003", FirstName = "Carol", LastName = "Williams", Email = "carol.williams@horizonhr.com", Phone = "555-0103",
                DateOfBirth = new DateOnly(1992, 11, 8), HireDate = new DateOnly(2020, 6, 15), Department = frontend,
                JobTitle = "Frontend Developer", EmploymentType = EmploymentType.FullTime, Salary = 110000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0004", FirstName = "David", LastName = "Brown", Email = "david.brown@horizonhr.com", Phone = "555-0104",
                DateOfBirth = new DateOnly(1988, 5, 30), HireDate = new DateOnly(2019, 9, 1), Department = backend,
                JobTitle = "Backend Lead", EmploymentType = EmploymentType.FullTime, Salary = 150000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0005", FirstName = "Eva", LastName = "Davis", Email = "eva.davis@horizonhr.com", Phone = "555-0105",
                DateOfBirth = new DateOnly(1995, 1, 12), HireDate = new DateOnly(2021, 2, 20), Department = backend,
                JobTitle = "Backend Developer", EmploymentType = EmploymentType.FullTime, Salary = 105000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0006", FirstName = "Frank", LastName = "Miller", Email = "frank.miller@horizonhr.com", Phone = "555-0106",
                DateOfBirth = new DateOnly(1987, 9, 18), HireDate = new DateOnly(2017, 11, 1), Department = hr,
                JobTitle = "HR Director", EmploymentType = EmploymentType.FullTime, Salary = 130000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0007", FirstName = "Grace", LastName = "Wilson", Email = "grace.wilson@horizonhr.com", Phone = "555-0107",
                DateOfBirth = new DateOnly(1993, 4, 25), HireDate = new DateOnly(2021, 8, 10), Department = hr,
                JobTitle = "HR Specialist", EmploymentType = EmploymentType.FullTime, Salary = 75000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0008", FirstName = "Henry", LastName = "Taylor", Email = "henry.taylor@horizonhr.com", Phone = "555-0108",
                DateOfBirth = new DateOnly(1991, 12, 3), HireDate = new DateOnly(2020, 4, 15), Department = marketing,
                JobTitle = "Marketing Manager", EmploymentType = EmploymentType.FullTime, Salary = 120000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0009", FirstName = "Iris", LastName = "Anderson", Email = "iris.anderson@horizonhr.com", Phone = "555-0109",
                DateOfBirth = new DateOnly(1996, 6, 14), HireDate = new DateOnly(2022, 1, 3), Department = marketing,
                JobTitle = "Content Strategist", EmploymentType = EmploymentType.PartTime, Salary = 65000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0010", FirstName = "Jack", LastName = "Thomas", Email = "jack.thomas@horizonhr.com", Phone = "555-0110",
                DateOfBirth = new DateOnly(1998, 8, 7), HireDate = new DateOnly(2023, 5, 15), Department = backend,
                JobTitle = "Junior Developer", EmploymentType = EmploymentType.Intern, Salary = 50000m, Status = EmployeeStatus.Active },
            new() { EmployeeNumber = "EMP-0011", FirstName = "Karen", LastName = "Martinez", Email = "karen.martinez@horizonhr.com", Phone = "555-0111",
                DateOfBirth = new DateOnly(1989, 2, 28), HireDate = new DateOnly(2019, 7, 1), Department = engineering,
                JobTitle = "DevOps Engineer", EmploymentType = EmploymentType.Contract, Salary = 135000m, Status = EmployeeStatus.OnLeave },
            new() { EmployeeNumber = "EMP-0012", FirstName = "Leo", LastName = "Garcia", Email = "leo.garcia@horizonhr.com", Phone = "555-0112",
                DateOfBirth = new DateOnly(1994, 10, 20), HireDate = new DateOnly(2020, 3, 10), Department = frontend,
                JobTitle = "UI/UX Developer", EmploymentType = EmploymentType.FullTime, Salary = 105000m, Status = EmployeeStatus.Terminated,
                TerminationDate = new DateOnly(2025, 12, 31) },
            new() { EmployeeNumber = "EMP-0013", FirstName = "Mia", LastName = "Roberts", Email = "mia.roberts@horizonhr.com", Phone = "555-0113",
                DateOfBirth = new DateOnly(1997, 3, 5), HireDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-20)), Department = backend,
                JobTitle = "Backend Developer", EmploymentType = EmploymentType.FullTime, Salary = 100000m, Status = EmployeeStatus.Active },
        };

        db.Employees.AddRange(employees);
        await db.SaveChangesAsync();

        // Set managers
        var alice = employees[0];   // VP Engineering
        var bob = employees[1];     // Frontend Lead
        var carol = employees[2];
        var david = employees[3];   // Backend Lead
        var eva = employees[4];
        var frank = employees[5];   // HR Director
        var grace = employees[6];
        var henry = employees[7];   // Marketing Manager
        var iris = employees[8];
        var jack = employees[9];
        var karen = employees[10];
        var mia = employees[12];

        // Manager hierarchy: Alice manages Bob, David, Karen; Bob manages Carol, Leo(terminated); David manages Eva, Jack, Mia; Frank manages Grace; Henry manages Iris
        bob.ManagerId = alice.Id;
        david.ManagerId = alice.Id;
        karen.ManagerId = alice.Id;
        carol.ManagerId = bob.Id;
        eva.ManagerId = david.Id;
        jack.ManagerId = david.Id;
        mia.ManagerId = david.Id;
        grace.ManagerId = frank.Id;
        iris.ManagerId = henry.Id;

        // Department managers
        engineering.ManagerId = alice.Id;
        frontend.ManagerId = bob.Id;
        backend.ManagerId = david.Id;
        hr.ManagerId = frank.Id;
        marketing.ManagerId = henry.Id;

        await db.SaveChangesAsync();

        // --- Leave Types ---
        var vacation = new LeaveType { Name = "Vacation", DefaultDaysPerYear = 15, IsCarryOverAllowed = true, MaxCarryOverDays = 5, RequiresApproval = true, IsPaid = true };
        var sick = new LeaveType { Name = "Sick", DefaultDaysPerYear = 10, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var personal = new LeaveType { Name = "Personal", DefaultDaysPerYear = 3, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = true, IsPaid = true };
        var bereavement = new LeaveType { Name = "Bereavement", DefaultDaysPerYear = 5, IsCarryOverAllowed = false, MaxCarryOverDays = 0, RequiresApproval = false, IsPaid = true };
        db.LeaveTypes.AddRange(vacation, sick, personal, bereavement);
        await db.SaveChangesAsync();

        // --- Leave Balances (for active employees, current year) ---
        var currentYear = DateTime.Today.Year;
        var activeEmployees = employees.Where(e => e.Status != EmployeeStatus.Terminated).ToList();
        var leaveTypes = new[] { vacation, sick, personal, bereavement };

        foreach (var emp in activeEmployees)
        {
            foreach (var lt in leaveTypes)
            {
                var usedDays = 0m;
                // Give some employees used days
                if (emp == alice && lt == vacation) { usedDays = 5m; }
                if (emp == bob && lt == vacation) { usedDays = 3m; }
                if (emp == david && lt == sick) { usedDays = 2m; }
                if (emp == grace && lt == personal) { usedDays = 1m; }
                if (emp == karen && lt == vacation) { usedDays = 10m; }

                db.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = emp.Id,
                    LeaveTypeId = lt.Id,
                    Year = currentYear,
                    TotalDays = lt.DefaultDaysPerYear,
                    UsedDays = usedDays,
                    CarriedOverDays = 0
                });
            }
        }
        await db.SaveChangesAsync();

        // --- Leave Requests ---
        var leaveRequests = new List<LeaveRequest>
        {
            new() { EmployeeId = alice.Id, LeaveTypeId = vacation.Id, StartDate = new DateOnly(currentYear, 2, 10), EndDate = new DateOnly(currentYear, 2, 14),
                TotalDays = 5m, Status = LeaveRequestStatus.Approved, Reason = "Family vacation", ReviewedById = frank.Id, ReviewDate = DateTime.UtcNow.AddDays(-60) },
            new() { EmployeeId = bob.Id, LeaveTypeId = vacation.Id, StartDate = new DateOnly(currentYear, 3, 3), EndDate = new DateOnly(currentYear, 3, 5),
                TotalDays = 3m, Status = LeaveRequestStatus.Approved, Reason = "Personal trip", ReviewedById = alice.Id, ReviewDate = DateTime.UtcNow.AddDays(-30) },
            new() { EmployeeId = david.Id, LeaveTypeId = sick.Id, StartDate = new DateOnly(currentYear, 1, 15), EndDate = new DateOnly(currentYear, 1, 16),
                TotalDays = 2m, Status = LeaveRequestStatus.Approved, Reason = "Flu recovery", ReviewedById = alice.Id, ReviewDate = DateTime.UtcNow.AddDays(-90) },
            new() { EmployeeId = eva.Id, LeaveTypeId = vacation.Id, StartDate = new DateOnly(currentYear, 4, 7), EndDate = new DateOnly(currentYear, 4, 11),
                TotalDays = 5m, Status = LeaveRequestStatus.Submitted, Reason = "Spring break trip" },
            new() { EmployeeId = grace.Id, LeaveTypeId = personal.Id, StartDate = new DateOnly(currentYear, 3, 20), EndDate = new DateOnly(currentYear, 3, 20),
                TotalDays = 1m, Status = LeaveRequestStatus.Approved, Reason = "Personal appointment", ReviewedById = frank.Id, ReviewDate = DateTime.UtcNow.AddDays(-10) },
            new() { EmployeeId = henry.Id, LeaveTypeId = vacation.Id, StartDate = new DateOnly(currentYear, 5, 12), EndDate = new DateOnly(currentYear, 5, 16),
                TotalDays = 5m, Status = LeaveRequestStatus.Submitted, Reason = "Summer travel plans" },
            new() { EmployeeId = carol.Id, LeaveTypeId = sick.Id, StartDate = new DateOnly(currentYear, 2, 1), EndDate = new DateOnly(currentYear, 2, 1),
                TotalDays = 1m, Status = LeaveRequestStatus.Rejected, Reason = "Not feeling well", ReviewedById = bob.Id, ReviewDate = DateTime.UtcNow.AddDays(-50),
                ReviewNotes = "Insufficient documentation provided" },
            new() { EmployeeId = karen.Id, LeaveTypeId = vacation.Id, StartDate = new DateOnly(currentYear, 1, 2), EndDate = new DateOnly(currentYear, 1, 15),
                TotalDays = 10m, Status = LeaveRequestStatus.Cancelled, Reason = "Extended holiday break" },
        };
        db.LeaveRequests.AddRange(leaveRequests);
        await db.SaveChangesAsync();

        // --- Performance Reviews ---
        var reviews = new List<PerformanceReview>
        {
            new() { EmployeeId = bob.Id, ReviewerId = alice.Id,
                ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 6, 30),
                Status = ReviewStatus.Completed, OverallRating = OverallRating.ExceedsExpectations,
                SelfAssessment = "I led the migration to the new React framework and mentored two junior developers.",
                ManagerAssessment = "Bob consistently delivers high-quality work and shows strong leadership potential.",
                Goals = "Lead the next major frontend architecture initiative.", StrengthsNoted = "Technical leadership, mentoring",
                AreasForImprovement = "Could delegate more to team members", CompletedDate = new DateOnly(currentYear - 1, 7, 15) },
            new() { EmployeeId = carol.Id, ReviewerId = bob.Id,
                ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 6, 30),
                Status = ReviewStatus.Completed, OverallRating = OverallRating.MeetsExpectations,
                SelfAssessment = "Completed all assigned tasks on time and contributed to code reviews.",
                ManagerAssessment = "Carol is a reliable team member who meets expectations consistently.",
                Goals = "Develop expertise in accessibility and performance optimization.",
                StrengthsNoted = "Reliability, attention to detail", AreasForImprovement = "Take on more challenging tasks",
                CompletedDate = new DateOnly(currentYear - 1, 7, 20) },
            new() { EmployeeId = david.Id, ReviewerId = alice.Id,
                ReviewPeriodStart = new DateOnly(currentYear - 1, 7, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31),
                Status = ReviewStatus.ManagerReviewPending,
                SelfAssessment = "Designed and implemented the new microservices architecture. Reduced API latency by 40%." },
            new() { EmployeeId = eva.Id, ReviewerId = david.Id,
                ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30),
                Status = ReviewStatus.SelfAssessmentPending },
            new() { EmployeeId = grace.Id, ReviewerId = frank.Id,
                ReviewPeriodStart = new DateOnly(currentYear - 1, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear - 1, 12, 31),
                Status = ReviewStatus.Completed, OverallRating = OverallRating.Outstanding,
                SelfAssessment = "Streamlined the onboarding process, reducing time-to-productivity by 30%.",
                ManagerAssessment = "Grace has been exceptional in improving HR operations.",
                Goals = "Lead the employee engagement initiative.", StrengthsNoted = "Process improvement, initiative",
                AreasForImprovement = "Public speaking skills", CompletedDate = new DateOnly(currentYear, 1, 15) },
            new() { EmployeeId = henry.Id, ReviewerId = alice.Id,
                ReviewPeriodStart = new DateOnly(currentYear, 1, 1), ReviewPeriodEnd = new DateOnly(currentYear, 6, 30),
                Status = ReviewStatus.Draft },
        };
        db.PerformanceReviews.AddRange(reviews);
        await db.SaveChangesAsync();

        // --- Skills ---
        var python = new Skill { Name = "Python", Category = "Programming Language", Description = "General-purpose programming language" };
        var javascript = new Skill { Name = "JavaScript", Category = "Programming Language", Description = "Web scripting language" };
        var csharp = new Skill { Name = "C#", Category = "Programming Language", Description = ".NET programming language" };
        var react = new Skill { Name = "React", Category = "Framework", Description = "JavaScript UI framework" };
        var dotnet = new Skill { Name = ".NET", Category = "Framework", Description = "Microsoft application framework" };
        var docker = new Skill { Name = "Docker", Category = "Tool", Description = "Container platform" };
        var sql = new Skill { Name = "SQL", Category = "Programming Language", Description = "Database query language" };
        var projectMgmt = new Skill { Name = "Project Management", Category = "Soft Skill", Description = "Planning and executing projects" };
        var communication = new Skill { Name = "Communication", Category = "Soft Skill", Description = "Verbal and written communication" };
        var git = new Skill { Name = "Git", Category = "Tool", Description = "Version control system" };
        var typescript = new Skill { Name = "TypeScript", Category = "Programming Language", Description = "Typed superset of JavaScript" };
        var kubernetes = new Skill { Name = "Kubernetes", Category = "Tool", Description = "Container orchestration platform" };

        db.Skills.AddRange(python, javascript, csharp, react, dotnet, docker, sql, projectMgmt, communication, git, typescript, kubernetes);
        await db.SaveChangesAsync();

        // --- Employee Skills ---
        var employeeSkills = new List<EmployeeSkill>
        {
            new() { EmployeeId = alice.Id, SkillId = csharp.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 15, LastAssessedDate = new DateOnly(currentYear, 1, 1) },
            new() { EmployeeId = alice.Id, SkillId = projectMgmt.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new() { EmployeeId = alice.Id, SkillId = dotnet.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 12 },
            new() { EmployeeId = bob.Id, SkillId = javascript.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 8 },
            new() { EmployeeId = bob.Id, SkillId = react.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 6 },
            new() { EmployeeId = bob.Id, SkillId = typescript.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new() { EmployeeId = carol.Id, SkillId = javascript.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new() { EmployeeId = carol.Id, SkillId = react.Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },
            new() { EmployeeId = david.Id, SkillId = csharp.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new() { EmployeeId = david.Id, SkillId = dotnet.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 9 },
            new() { EmployeeId = david.Id, SkillId = docker.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new() { EmployeeId = david.Id, SkillId = sql.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new() { EmployeeId = eva.Id, SkillId = python.Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },
            new() { EmployeeId = eva.Id, SkillId = sql.Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },
            new() { EmployeeId = frank.Id, SkillId = communication.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 12 },
            new() { EmployeeId = frank.Id, SkillId = projectMgmt.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 8 },
            new() { EmployeeId = grace.Id, SkillId = communication.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new() { EmployeeId = henry.Id, SkillId = communication.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 7 },
            new() { EmployeeId = henry.Id, SkillId = projectMgmt.Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },
            new() { EmployeeId = jack.Id, SkillId = python.Id, ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },
            new() { EmployeeId = jack.Id, SkillId = git.Id, ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },
            new() { EmployeeId = karen.Id, SkillId = docker.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 7 },
            new() { EmployeeId = karen.Id, SkillId = kubernetes.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new() { EmployeeId = mia.Id, SkillId = csharp.Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },
        };
        db.EmployeeSkills.AddRange(employeeSkills);
        await db.SaveChangesAsync();
    }
}
