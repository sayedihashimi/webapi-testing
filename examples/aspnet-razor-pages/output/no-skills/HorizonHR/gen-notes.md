# HorizonHR — Generation Notes

## Summary

A complete ASP.NET Core Razor Pages HR portal built with .NET 10, EF Core with SQLite, and Bootstrap 5.

## What Was Generated

### Models (8 entities + 6 enums)
- **Department**, **Employee**, **LeaveType**, **LeaveBalance**, **LeaveRequest**, **PerformanceReview**, **Skill**, **EmployeeSkill**
- Enums: EmploymentType, EmployeeStatus, LeaveRequestStatus, ReviewStatus, OverallRating, ProficiencyLevel

### Data Layer
- **ApplicationDbContext** with full relationship configuration, unique constraints, and automatic timestamp updates
- **DataSeeder** with 5 departments (hierarchical), 13 employees, 4 leave types, leave balances, 8 leave requests, 6 performance reviews, 12 skills, and 23 employee-skill records

### Services (5 service interfaces + implementations)
- DepartmentService, EmployeeService, LeaveService, ReviewService, SkillService
- Business rules: leave balance enforcement, overlapping leave detection, review period overlap checks, employee termination cascading, circular department hierarchy prevention, auto-generated employee numbers

### Pages (30+ Razor Pages across 6 areas)
- **Dashboard** — overview stats, recent hires, on-leave employees, headcount by department
- **Departments** — list (hierarchical), details, create, edit
- **Employees** — directory (paginated, searchable, filterable), profile, create, edit, terminate, direct reports, manage skills
- **Leave** — requests list, details, submit, review (approve/reject), cancel, balances, employee summary
- **Reviews** — list, details, create, self-assessment, manager review (with workflow transitions)
- **Skills** — catalog (grouped by category), create, edit, search employees by skill

### Shared Components
- **_Layout.cshtml** — Bootstrap 5 navbar, TempData alerts, footer
- **_StatusBadge.cshtml** — reusable color-coded badge partial for all status/rating/type values
- **_Pagination.cshtml** — reusable pagination partial

## How to Run

```bash
cd src/HorizonHR
dotnet run
```

The database is automatically created and seeded on first run.
