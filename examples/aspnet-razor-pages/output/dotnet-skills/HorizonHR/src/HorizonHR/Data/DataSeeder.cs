using Microsoft.EntityFrameworkCore;
using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Departments.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var currentYear = DateTime.Now.Year;

        // ---------------------------------------------------------------
        // Departments (top-level first, then children)
        // ---------------------------------------------------------------
        var engineering = new Department
        {
            Name = "Engineering",
            Code = "ENG",
            Description = "Software engineering and development",
            IsActive = true
        };
        var humanResources = new Department
        {
            Name = "Human Resources",
            Code = "HR",
            Description = "People operations and talent management",
            IsActive = true
        };
        var marketing = new Department
        {
            Name = "Marketing",
            Code = "MKT",
            Description = "Brand, communications, and growth marketing",
            IsActive = true
        };

        context.Departments.AddRange(engineering, humanResources, marketing);
        await context.SaveChangesAsync();

        var frontend = new Department
        {
            Name = "Frontend",
            Code = "FE",
            Description = "Frontend web and mobile development",
            ParentDepartmentId = engineering.Id,
            IsActive = true
        };
        var backend = new Department
        {
            Name = "Backend",
            Code = "BE",
            Description = "Backend services and API development",
            ParentDepartmentId = engineering.Id,
            IsActive = true
        };

        context.Departments.AddRange(frontend, backend);
        await context.SaveChangesAsync();

        // ---------------------------------------------------------------
        // Leave Types
        // ---------------------------------------------------------------
        var vacation = new LeaveType
        {
            Name = "Vacation",
            DefaultDaysPerYear = 15,
            IsCarryOverAllowed = true,
            MaxCarryOverDays = 5,
            RequiresApproval = true,
            IsPaid = true
        };
        var sick = new LeaveType
        {
            Name = "Sick",
            DefaultDaysPerYear = 10,
            IsCarryOverAllowed = false,
            MaxCarryOverDays = 0,
            RequiresApproval = true,
            IsPaid = true
        };
        var personal = new LeaveType
        {
            Name = "Personal",
            DefaultDaysPerYear = 3,
            IsCarryOverAllowed = false,
            MaxCarryOverDays = 0,
            RequiresApproval = true,
            IsPaid = true
        };
        var bereavement = new LeaveType
        {
            Name = "Bereavement",
            DefaultDaysPerYear = 5,
            IsCarryOverAllowed = false,
            MaxCarryOverDays = 0,
            RequiresApproval = false,
            IsPaid = true
        };

        context.LeaveTypes.AddRange(vacation, sick, personal, bereavement);
        await context.SaveChangesAsync();

        // ---------------------------------------------------------------
        // Employees
        // ---------------------------------------------------------------

        // --- Engineering leadership ---
        var emp1 = new Employee
        {
            EmployeeNumber = "EMP-0001",
            FirstName = "Alice",
            LastName = "Chen",
            Email = "alice.chen@horizonhr.com",
            Phone = "555-100-0001",
            DateOfBirth = new DateOnly(1980, 3, 15),
            HireDate = new DateOnly(2015, 1, 10),
            DepartmentId = engineering.Id,
            JobTitle = "VP of Engineering",
            EmploymentType = EmploymentType.FullTime,
            Salary = 195000m,
            Status = EmployeeStatus.Active,
            Address = "100 Innovation Blvd",
            City = "San Francisco",
            State = "CA",
            ZipCode = "94105"
        };

        // --- HR leadership ---
        var emp2 = new Employee
        {
            EmployeeNumber = "EMP-0002",
            FirstName = "David",
            LastName = "Okoro",
            Email = "david.okoro@horizonhr.com",
            Phone = "555-100-0002",
            DateOfBirth = new DateOnly(1978, 7, 22),
            HireDate = new DateOnly(2016, 4, 1),
            DepartmentId = humanResources.Id,
            JobTitle = "Director of Human Resources",
            EmploymentType = EmploymentType.FullTime,
            Salary = 160000m,
            Status = EmployeeStatus.Active,
            Address = "200 People Way",
            City = "San Francisco",
            State = "CA",
            ZipCode = "94107"
        };

        // --- Marketing leadership ---
        var emp3 = new Employee
        {
            EmployeeNumber = "EMP-0003",
            FirstName = "Maria",
            LastName = "Santos",
            Email = "maria.santos@horizonhr.com",
            Phone = "555-100-0003",
            DateOfBirth = new DateOnly(1985, 11, 5),
            HireDate = new DateOnly(2017, 8, 15),
            DepartmentId = marketing.Id,
            JobTitle = "Marketing Manager",
            EmploymentType = EmploymentType.FullTime,
            Salary = 140000m,
            Status = EmployeeStatus.Active,
            Address = "300 Brand Ave",
            City = "San Francisco",
            State = "CA",
            ZipCode = "94108"
        };

        // Save managers first so their IDs are available
        context.Employees.AddRange(emp1, emp2, emp3);
        await context.SaveChangesAsync();

        // --- Frontend team lead (reports to Alice) ---
        var emp4 = new Employee
        {
            EmployeeNumber = "EMP-0004",
            FirstName = "Priya",
            LastName = "Sharma",
            Email = "priya.sharma@horizonhr.com",
            Phone = "555-100-0004",
            DateOfBirth = new DateOnly(1990, 5, 18),
            HireDate = new DateOnly(2018, 3, 1),
            DepartmentId = frontend.Id,
            JobTitle = "Frontend Team Lead",
            EmploymentType = EmploymentType.FullTime,
            Salary = 145000m,
            ManagerId = emp1.Id,
            Status = EmployeeStatus.Active,
            Address = "401 React Ln",
            City = "San Francisco",
            State = "CA",
            ZipCode = "94109"
        };

        // --- Backend team lead (reports to Alice) ---
        var emp5 = new Employee
        {
            EmployeeNumber = "EMP-0005",
            FirstName = "James",
            LastName = "Williams",
            Email = "james.williams@horizonhr.com",
            Phone = "555-100-0005",
            DateOfBirth = new DateOnly(1988, 9, 30),
            HireDate = new DateOnly(2017, 6, 15),
            DepartmentId = backend.Id,
            JobTitle = "Backend Team Lead",
            EmploymentType = EmploymentType.FullTime,
            Salary = 150000m,
            ManagerId = emp1.Id,
            Status = EmployeeStatus.Active,
            Address = "502 API Dr",
            City = "San Francisco",
            State = "CA",
            ZipCode = "94110"
        };

        context.Employees.AddRange(emp4, emp5);
        await context.SaveChangesAsync();

        // --- Frontend developer (reports to Priya — 2 levels deep) ---
        var emp6 = new Employee
        {
            EmployeeNumber = "EMP-0006",
            FirstName = "Liam",
            LastName = "Johnson",
            Email = "liam.johnson@horizonhr.com",
            Phone = "555-100-0006",
            DateOfBirth = new DateOnly(1995, 1, 12),
            HireDate = new DateOnly(2021, 2, 1),
            DepartmentId = frontend.Id,
            JobTitle = "Frontend Developer",
            EmploymentType = EmploymentType.FullTime,
            Salary = 105000m,
            ManagerId = emp4.Id,
            Status = EmployeeStatus.Active,
            Address = "603 Vue St",
            City = "Oakland",
            State = "CA",
            ZipCode = "94601"
        };

        // --- Frontend intern (reports to Priya) ---
        var emp7 = new Employee
        {
            EmployeeNumber = "EMP-0007",
            FirstName = "Sofia",
            LastName = "Reyes",
            Email = "sofia.reyes@horizonhr.com",
            Phone = "555-100-0007",
            DateOfBirth = new DateOnly(2001, 6, 28),
            HireDate = new DateOnly(2024, 6, 1),
            DepartmentId = frontend.Id,
            JobTitle = "Frontend Intern",
            EmploymentType = EmploymentType.Intern,
            Salary = 52000m,
            ManagerId = emp4.Id,
            Status = EmployeeStatus.Active,
            Address = "704 CSS Ct",
            City = "Oakland",
            State = "CA",
            ZipCode = "94602"
        };

        // --- Backend developer (reports to James — 2 levels deep) ---
        var emp8 = new Employee
        {
            EmployeeNumber = "EMP-0008",
            FirstName = "Nina",
            LastName = "Patel",
            Email = "nina.patel@horizonhr.com",
            Phone = "555-100-0008",
            DateOfBirth = new DateOnly(1992, 4, 3),
            HireDate = new DateOnly(2019, 9, 16),
            DepartmentId = backend.Id,
            JobTitle = "Senior Backend Developer",
            EmploymentType = EmploymentType.FullTime,
            Salary = 135000m,
            ManagerId = emp5.Id,
            Status = EmployeeStatus.Active,
            Address = "805 Lambda Blvd",
            City = "San Jose",
            State = "CA",
            ZipCode = "95110"
        };

        // --- Backend contractor (reports to James) ---
        var emp9 = new Employee
        {
            EmployeeNumber = "EMP-0009",
            FirstName = "Carlos",
            LastName = "Mendez",
            Email = "carlos.mendez@horizonhr.com",
            Phone = "555-100-0009",
            DateOfBirth = new DateOnly(1993, 12, 11),
            HireDate = new DateOnly(2023, 1, 9),
            DepartmentId = backend.Id,
            JobTitle = "Backend Developer (Contract)",
            EmploymentType = EmploymentType.Contract,
            Salary = 90000m,
            ManagerId = emp5.Id,
            Status = EmployeeStatus.Active,
            Address = "906 Node Rd",
            City = "San Jose",
            State = "CA",
            ZipCode = "95112"
        };

        // --- HR generalist (reports to David) ---
        var emp10 = new Employee
        {
            EmployeeNumber = "EMP-0010",
            FirstName = "Amara",
            LastName = "Washington",
            Email = "amara.washington@horizonhr.com",
            Phone = "555-100-0010",
            DateOfBirth = new DateOnly(1991, 8, 20),
            HireDate = new DateOnly(2020, 5, 11),
            DepartmentId = humanResources.Id,
            JobTitle = "HR Generalist",
            EmploymentType = EmploymentType.FullTime,
            Salary = 85000m,
            ManagerId = emp2.Id,
            Status = EmployeeStatus.Active,
            Address = "1010 Talent Ct",
            City = "San Francisco",
            State = "CA",
            ZipCode = "94111"
        };

        // --- HR part-time recruiter ---
        var emp11 = new Employee
        {
            EmployeeNumber = "EMP-0011",
            FirstName = "Tomasz",
            LastName = "Kowalski",
            Email = "tomasz.kowalski@horizonhr.com",
            Phone = "555-100-0011",
            DateOfBirth = new DateOnly(1994, 2, 14),
            HireDate = new DateOnly(2022, 11, 1),
            DepartmentId = humanResources.Id,
            JobTitle = "Recruiter",
            EmploymentType = EmploymentType.PartTime,
            Salary = 55000m,
            ManagerId = emp2.Id,
            Status = EmployeeStatus.OnLeave,
            Address = "1111 Hire Ln",
            City = "Berkeley",
            State = "CA",
            ZipCode = "94704"
        };

        // --- Marketing specialist (reports to Maria) ---
        var emp12 = new Employee
        {
            EmployeeNumber = "EMP-0012",
            FirstName = "Emily",
            LastName = "Nguyen",
            Email = "emily.nguyen@horizonhr.com",
            Phone = "555-100-0012",
            DateOfBirth = new DateOnly(1996, 10, 7),
            HireDate = new DateOnly(2021, 7, 19),
            DepartmentId = marketing.Id,
            JobTitle = "Marketing Specialist",
            EmploymentType = EmploymentType.FullTime,
            Salary = 80000m,
            ManagerId = emp3.Id,
            Status = EmployeeStatus.Active,
            Address = "1212 Campaign Way",
            City = "San Francisco",
            State = "CA",
            ZipCode = "94112"
        };

        // --- Terminated employee (was in Backend) ---
        var emp13 = new Employee
        {
            EmployeeNumber = "EMP-0013",
            FirstName = "Robert",
            LastName = "Kim",
            Email = "robert.kim@horizonhr.com",
            Phone = "555-100-0013",
            DateOfBirth = new DateOnly(1987, 3, 25),
            HireDate = new DateOnly(2019, 4, 1),
            DepartmentId = backend.Id,
            JobTitle = "Backend Developer",
            EmploymentType = EmploymentType.FullTime,
            Salary = 120000m,
            ManagerId = emp5.Id,
            Status = EmployeeStatus.Terminated,
            TerminationDate = new DateOnly(2024, 9, 30),
            Address = "1313 Exit Blvd",
            City = "Fremont",
            State = "CA",
            ZipCode = "94536"
        };

        context.Employees.AddRange(emp6, emp7, emp8, emp9, emp10, emp11, emp12, emp13);
        await context.SaveChangesAsync();

        // ---------------------------------------------------------------
        // Set department managers
        // ---------------------------------------------------------------
        engineering.ManagerId = emp1.Id;
        humanResources.ManagerId = emp2.Id;
        marketing.ManagerId = emp3.Id;
        frontend.ManagerId = emp4.Id;
        backend.ManagerId = emp5.Id;

        context.Departments.UpdateRange(engineering, humanResources, marketing, frontend, backend);
        await context.SaveChangesAsync();

        // ---------------------------------------------------------------
        // Leave Balances — all active employees for the current year
        // ---------------------------------------------------------------
        var activeEmployees = new[]
        {
            emp1, emp2, emp3, emp4, emp5, emp6, emp7, emp8, emp9, emp10, emp11, emp12
        };
        var leaveTypes = new[] { vacation, sick, personal, bereavement };

        var leaveBalances = new List<LeaveBalance>();
        foreach (var emp in activeEmployees)
        {
            foreach (var lt in leaveTypes)
            {
                leaveBalances.Add(new LeaveBalance
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

        // Pre-set some used days (will reconcile with approved requests below)
        // emp1 (Alice) — used 3 vacation days
        leaveBalances.First(lb => lb.EmployeeId == emp1.Id && lb.LeaveTypeId == vacation.Id).UsedDays = 3;
        // emp4 (Priya) — used 2 sick days
        leaveBalances.First(lb => lb.EmployeeId == emp4.Id && lb.LeaveTypeId == sick.Id).UsedDays = 2;
        // emp6 (Liam) — used 5 vacation days
        leaveBalances.First(lb => lb.EmployeeId == emp6.Id && lb.LeaveTypeId == vacation.Id).UsedDays = 5;
        // emp8 (Nina) — used 1 personal day
        leaveBalances.First(lb => lb.EmployeeId == emp8.Id && lb.LeaveTypeId == personal.Id).UsedDays = 1;
        // emp10 (Amara) — used 2 vacation days
        leaveBalances.First(lb => lb.EmployeeId == emp10.Id && lb.LeaveTypeId == vacation.Id).UsedDays = 2;
        // emp12 (Emily) — used 1 sick day
        leaveBalances.First(lb => lb.EmployeeId == emp12.Id && lb.LeaveTypeId == sick.Id).UsedDays = 1;

        context.LeaveBalances.AddRange(leaveBalances);
        await context.SaveChangesAsync();

        // ---------------------------------------------------------------
        // Leave Requests (8+)
        // ---------------------------------------------------------------

        // 1. Approved — Alice took 3 vacation days
        var lr1 = new LeaveRequest
        {
            EmployeeId = emp1.Id,
            LeaveTypeId = vacation.Id,
            StartDate = new DateOnly(currentYear, 2, 10),
            EndDate = new DateOnly(currentYear, 2, 12),
            TotalDays = 3,
            Status = LeaveRequestStatus.Approved,
            Reason = "Family vacation",
            ReviewedById = emp2.Id,
            ReviewDate = new DateTime(currentYear, 1, 25, 10, 0, 0, DateTimeKind.Utc),
            ReviewNotes = "Approved. Enjoy your time off!",
            SubmittedDate = new DateTime(currentYear, 1, 20, 9, 0, 0, DateTimeKind.Utc)
        };

        // 2. Approved — Liam took 5 vacation days
        var lr2 = new LeaveRequest
        {
            EmployeeId = emp6.Id,
            LeaveTypeId = vacation.Id,
            StartDate = new DateOnly(currentYear, 3, 17),
            EndDate = new DateOnly(currentYear, 3, 21),
            TotalDays = 5,
            Status = LeaveRequestStatus.Approved,
            Reason = "Spring break trip",
            ReviewedById = emp4.Id,
            ReviewDate = new DateTime(currentYear, 3, 5, 14, 30, 0, DateTimeKind.Utc),
            ReviewNotes = "Approved.",
            SubmittedDate = new DateTime(currentYear, 3, 1, 8, 0, 0, DateTimeKind.Utc)
        };

        // 3. Approved — Priya took 2 sick days
        var lr3 = new LeaveRequest
        {
            EmployeeId = emp4.Id,
            LeaveTypeId = sick.Id,
            StartDate = new DateOnly(currentYear, 4, 8),
            EndDate = new DateOnly(currentYear, 4, 9),
            TotalDays = 2,
            Status = LeaveRequestStatus.Approved,
            Reason = "Flu recovery",
            ReviewedById = emp1.Id,
            ReviewDate = new DateTime(currentYear, 4, 8, 7, 0, 0, DateTimeKind.Utc),
            ReviewNotes = "Get well soon.",
            SubmittedDate = new DateTime(currentYear, 4, 8, 6, 30, 0, DateTimeKind.Utc)
        };

        // 4. Submitted — Nina requesting vacation
        var lr4 = new LeaveRequest
        {
            EmployeeId = emp8.Id,
            LeaveTypeId = vacation.Id,
            StartDate = new DateOnly(currentYear, 7, 14),
            EndDate = new DateOnly(currentYear, 7, 18),
            TotalDays = 5,
            Status = LeaveRequestStatus.Submitted,
            Reason = "Summer holiday",
            SubmittedDate = new DateTime(currentYear, 6, 20, 11, 0, 0, DateTimeKind.Utc)
        };

        // 5. Submitted — Amara requesting personal day
        var lr5 = new LeaveRequest
        {
            EmployeeId = emp10.Id,
            LeaveTypeId = personal.Id,
            StartDate = new DateOnly(currentYear, 8, 5),
            EndDate = new DateOnly(currentYear, 8, 5),
            TotalDays = 1,
            Status = LeaveRequestStatus.Submitted,
            Reason = "Personal appointment",
            SubmittedDate = new DateTime(currentYear, 7, 28, 9, 15, 0, DateTimeKind.Utc)
        };

        // 6. Rejected — Carlos vacation request denied
        var lr6 = new LeaveRequest
        {
            EmployeeId = emp9.Id,
            LeaveTypeId = vacation.Id,
            StartDate = new DateOnly(currentYear, 5, 1),
            EndDate = new DateOnly(currentYear, 5, 9),
            TotalDays = 7,
            Status = LeaveRequestStatus.Rejected,
            Reason = "Extended travel plans",
            ReviewedById = emp5.Id,
            ReviewDate = new DateTime(currentYear, 4, 20, 16, 0, 0, DateTimeKind.Utc),
            ReviewNotes = "Critical project deadline — please reschedule.",
            SubmittedDate = new DateTime(currentYear, 4, 15, 10, 0, 0, DateTimeKind.Utc)
        };

        // 7. Rejected — Emily sick leave for questionable dates
        var lr7 = new LeaveRequest
        {
            EmployeeId = emp12.Id,
            LeaveTypeId = sick.Id,
            StartDate = new DateOnly(currentYear, 6, 6),
            EndDate = new DateOnly(currentYear, 6, 10),
            TotalDays = 5,
            Status = LeaveRequestStatus.Rejected,
            Reason = "Feeling unwell",
            ReviewedById = emp3.Id,
            ReviewDate = new DateTime(currentYear, 6, 3, 9, 0, 0, DateTimeKind.Utc),
            ReviewNotes = "Please provide a medical certificate for extended sick leave.",
            SubmittedDate = new DateTime(currentYear, 6, 1, 8, 0, 0, DateTimeKind.Utc)
        };

        // 8. Cancelled — Sofia cancelled her request
        var lr8 = new LeaveRequest
        {
            EmployeeId = emp7.Id,
            LeaveTypeId = vacation.Id,
            StartDate = new DateOnly(currentYear, 9, 1),
            EndDate = new DateOnly(currentYear, 9, 3),
            TotalDays = 3,
            Status = LeaveRequestStatus.Cancelled,
            Reason = "Travel plans changed",
            SubmittedDate = new DateTime(currentYear, 8, 10, 12, 0, 0, DateTimeKind.Utc)
        };

        // 9. Cancelled — terminated employee's request auto-cancelled
        var lr9 = new LeaveRequest
        {
            EmployeeId = emp13.Id,
            LeaveTypeId = vacation.Id,
            StartDate = new DateOnly(currentYear, 10, 14),
            EndDate = new DateOnly(currentYear, 10, 18),
            TotalDays = 5,
            Status = LeaveRequestStatus.Cancelled,
            Reason = "Planned vacation (cancelled due to termination)",
            SubmittedDate = new DateTime(currentYear, 9, 1, 8, 0, 0, DateTimeKind.Utc)
        };

        // 10. Approved — Amara took 2 vacation days
        var lr10 = new LeaveRequest
        {
            EmployeeId = emp10.Id,
            LeaveTypeId = vacation.Id,
            StartDate = new DateOnly(currentYear, 3, 3),
            EndDate = new DateOnly(currentYear, 3, 4),
            TotalDays = 2,
            Status = LeaveRequestStatus.Approved,
            Reason = "Short getaway",
            ReviewedById = emp2.Id,
            ReviewDate = new DateTime(currentYear, 2, 25, 14, 0, 0, DateTimeKind.Utc),
            ReviewNotes = "Approved.",
            SubmittedDate = new DateTime(currentYear, 2, 20, 10, 0, 0, DateTimeKind.Utc)
        };

        // 11. Approved — Nina took 1 personal day
        var lr11 = new LeaveRequest
        {
            EmployeeId = emp8.Id,
            LeaveTypeId = personal.Id,
            StartDate = new DateOnly(currentYear, 1, 15),
            EndDate = new DateOnly(currentYear, 1, 15),
            TotalDays = 1,
            Status = LeaveRequestStatus.Approved,
            Reason = "Personal errand",
            ReviewedById = emp5.Id,
            ReviewDate = new DateTime(currentYear, 1, 10, 11, 0, 0, DateTimeKind.Utc),
            ReviewNotes = "No problem.",
            SubmittedDate = new DateTime(currentYear, 1, 8, 9, 0, 0, DateTimeKind.Utc)
        };

        // 12. Approved — Emily used 1 sick day
        var lr12 = new LeaveRequest
        {
            EmployeeId = emp12.Id,
            LeaveTypeId = sick.Id,
            StartDate = new DateOnly(currentYear, 2, 20),
            EndDate = new DateOnly(currentYear, 2, 20),
            TotalDays = 1,
            Status = LeaveRequestStatus.Approved,
            Reason = "Dental appointment",
            ReviewedById = emp3.Id,
            ReviewDate = new DateTime(currentYear, 2, 18, 10, 0, 0, DateTimeKind.Utc),
            ReviewNotes = "Approved.",
            SubmittedDate = new DateTime(currentYear, 2, 15, 14, 0, 0, DateTimeKind.Utc)
        };

        context.LeaveRequests.AddRange(lr1, lr2, lr3, lr4, lr5, lr6, lr7, lr8, lr9, lr10, lr11, lr12);
        await context.SaveChangesAsync();

        // ---------------------------------------------------------------
        // Performance Reviews (6+)
        // ---------------------------------------------------------------
        var periodStart = new DateOnly(currentYear - 1, 1, 1);
        var periodEnd = new DateOnly(currentYear - 1, 12, 31);
        var currentPeriodStart = new DateOnly(currentYear, 1, 1);
        var currentPeriodEnd = new DateOnly(currentYear, 12, 31);

        // 1. Completed — Liam reviewed by Priya
        var pr1 = new PerformanceReview
        {
            EmployeeId = emp6.Id,
            ReviewerId = emp4.Id,
            ReviewPeriodStart = periodStart,
            ReviewPeriodEnd = periodEnd,
            Status = ReviewStatus.Completed,
            OverallRating = OverallRating.ExceedsExpectations,
            SelfAssessment = "I grew significantly as a frontend developer this year. I led the migration to React 18 and mentored our intern.",
            ManagerAssessment = "Liam has shown excellent growth. His technical leadership during the React migration was outstanding.",
            Goals = "Lead a major feature from design to deployment. Improve accessibility across the platform.",
            StrengthsNoted = "Technical depth, mentorship, initiative",
            AreasForImprovement = "Could improve on cross-team communication",
            CompletedDate = new DateOnly(currentYear, 1, 20)
        };

        // 2. Completed — Nina reviewed by James
        var pr2 = new PerformanceReview
        {
            EmployeeId = emp8.Id,
            ReviewerId = emp5.Id,
            ReviewPeriodStart = periodStart,
            ReviewPeriodEnd = periodEnd,
            Status = ReviewStatus.Completed,
            OverallRating = OverallRating.MeetsExpectations,
            SelfAssessment = "Delivered reliable backend services and improved our CI/CD pipeline significantly.",
            ManagerAssessment = "Nina consistently delivers solid work. Her CI/CD improvements reduced deployment time by 40%.",
            Goals = "Pursue system design skills. Take on architecture decisions for new microservices.",
            StrengthsNoted = "Reliability, DevOps mindset, attention to detail",
            AreasForImprovement = "Could be more proactive in design discussions",
            CompletedDate = new DateOnly(currentYear, 1, 25)
        };

        // 3. Draft — Emily (current year, not started)
        var pr3 = new PerformanceReview
        {
            EmployeeId = emp12.Id,
            ReviewerId = emp3.Id,
            ReviewPeriodStart = currentPeriodStart,
            ReviewPeriodEnd = currentPeriodEnd,
            Status = ReviewStatus.Draft
        };

        // 4. SelfAssessmentPending — Carlos
        var pr4 = new PerformanceReview
        {
            EmployeeId = emp9.Id,
            ReviewerId = emp5.Id,
            ReviewPeriodStart = currentPeriodStart,
            ReviewPeriodEnd = currentPeriodEnd,
            Status = ReviewStatus.SelfAssessmentPending
        };

        // 5. ManagerReviewPending — Amara (self-assessment filled)
        var pr5 = new PerformanceReview
        {
            EmployeeId = emp10.Id,
            ReviewerId = emp2.Id,
            ReviewPeriodStart = currentPeriodStart,
            ReviewPeriodEnd = currentPeriodEnd,
            Status = ReviewStatus.ManagerReviewPending,
            SelfAssessment = "I streamlined our onboarding process, reducing new-hire ramp-up time by two weeks. I also implemented a new benefits communication strategy that increased employee enrollment by 15%.",
            Goals = "Earn SHRM-CP certification. Build an employee engagement survey framework."
        };

        // 6. Completed — Priya reviewed by Alice (previous year)
        var pr6 = new PerformanceReview
        {
            EmployeeId = emp4.Id,
            ReviewerId = emp1.Id,
            ReviewPeriodStart = periodStart,
            ReviewPeriodEnd = periodEnd,
            Status = ReviewStatus.Completed,
            OverallRating = OverallRating.Outstanding,
            SelfAssessment = "Led the frontend team through a major product redesign while maintaining velocity. Grew the team from 2 to 4 members.",
            ManagerAssessment = "Priya is an exceptional team lead. She balances technical excellence with people leadership remarkably well.",
            Goals = "Explore full-stack architecture. Develop leadership coaching skills.",
            StrengthsNoted = "Leadership, technical vision, team building",
            AreasForImprovement = "Delegate more — tendency to take on too much personally",
            CompletedDate = new DateOnly(currentYear, 1, 15)
        };

        // 7. ManagerReviewPending — Sofia (self-assessment filled)
        var pr7 = new PerformanceReview
        {
            EmployeeId = emp7.Id,
            ReviewerId = emp4.Id,
            ReviewPeriodStart = currentPeriodStart,
            ReviewPeriodEnd = currentPeriodEnd,
            Status = ReviewStatus.ManagerReviewPending,
            SelfAssessment = "As an intern I learned React, TypeScript, and how to work in an agile team. I contributed to three feature PRs that shipped to production.",
            Goals = "Convert to full-time role. Deepen knowledge of testing and accessibility."
        };

        context.PerformanceReviews.AddRange(pr1, pr2, pr3, pr4, pr5, pr6, pr7);
        await context.SaveChangesAsync();

        // ---------------------------------------------------------------
        // Skills
        // ---------------------------------------------------------------
        var python = new Skill { Name = "Python", Category = "Programming Languages", Description = "General-purpose programming language" };
        var javascript = new Skill { Name = "JavaScript", Category = "Programming Languages", Description = "Web scripting language" };
        var csharp = new Skill { Name = "C#", Category = "Programming Languages", Description = ".NET programming language" };
        var java = new Skill { Name = "Java", Category = "Programming Languages", Description = "Enterprise programming language" };
        var react = new Skill { Name = "React", Category = "Frameworks", Description = "JavaScript UI library" };
        var dotnet = new Skill { Name = ".NET", Category = "Frameworks", Description = "Microsoft application framework" };
        var docker = new Skill { Name = "Docker", Category = "Tools", Description = "Container platform" };
        var sql = new Skill { Name = "SQL", Category = "Tools", Description = "Structured Query Language for databases" };
        var projectMgmt = new Skill { Name = "Project Management", Category = "Soft Skills", Description = "Planning, execution, and delivery of projects" };
        var communication = new Skill { Name = "Communication", Category = "Soft Skills", Description = "Written and verbal communication" };
        var leadership = new Skill { Name = "Leadership", Category = "Soft Skills", Description = "Leading teams and driving outcomes" };

        context.Skills.AddRange(python, javascript, csharp, java, react, dotnet, docker, sql, projectMgmt, communication, leadership);
        await context.SaveChangesAsync();

        // ---------------------------------------------------------------
        // Employee Skills (20+)
        // ---------------------------------------------------------------
        var employeeSkills = new List<EmployeeSkill>
        {
            // Alice — VP of Engineering
            new() { EmployeeId = emp1.Id, SkillId = csharp.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 15 },
            new() { EmployeeId = emp1.Id, SkillId = dotnet.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 14 },
            new() { EmployeeId = emp1.Id, SkillId = leadership.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new() { EmployeeId = emp1.Id, SkillId = projectMgmt.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 8 },

            // Priya — Frontend Team Lead
            new() { EmployeeId = emp4.Id, SkillId = javascript.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 8 },
            new() { EmployeeId = emp4.Id, SkillId = react.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 6 },
            new() { EmployeeId = emp4.Id, SkillId = leadership.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },

            // James — Backend Team Lead
            new() { EmployeeId = emp5.Id, SkillId = csharp.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },
            new() { EmployeeId = emp5.Id, SkillId = dotnet.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 9 },
            new() { EmployeeId = emp5.Id, SkillId = docker.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new() { EmployeeId = emp5.Id, SkillId = sql.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 8 },

            // Liam — Frontend Developer
            new() { EmployeeId = emp6.Id, SkillId = javascript.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new() { EmployeeId = emp6.Id, SkillId = react.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 3 },
            new() { EmployeeId = emp6.Id, SkillId = python.Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 2 },

            // Sofia — Frontend Intern
            new() { EmployeeId = emp7.Id, SkillId = javascript.Id, ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },
            new() { EmployeeId = emp7.Id, SkillId = react.Id, ProficiencyLevel = ProficiencyLevel.Beginner, YearsOfExperience = 1 },

            // Nina — Senior Backend Developer
            new() { EmployeeId = emp8.Id, SkillId = csharp.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 6 },
            new() { EmployeeId = emp8.Id, SkillId = docker.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 4 },
            new() { EmployeeId = emp8.Id, SkillId = sql.Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },

            // Carlos — Backend Developer (Contract)
            new() { EmployeeId = emp9.Id, SkillId = java.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
            new() { EmployeeId = emp9.Id, SkillId = python.Id, ProficiencyLevel = ProficiencyLevel.Intermediate, YearsOfExperience = 3 },

            // David — Director of HR
            new() { EmployeeId = emp2.Id, SkillId = communication.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 15 },
            new() { EmployeeId = emp2.Id, SkillId = leadership.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 12 },

            // Maria — Marketing Manager
            new() { EmployeeId = emp3.Id, SkillId = projectMgmt.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 6 },
            new() { EmployeeId = emp3.Id, SkillId = communication.Id, ProficiencyLevel = ProficiencyLevel.Expert, YearsOfExperience = 10 },

            // Amara — HR Generalist
            new() { EmployeeId = emp10.Id, SkillId = communication.Id, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 5 },
        };

        context.EmployeeSkills.AddRange(employeeSkills);
        await context.SaveChangesAsync();
    }
}
