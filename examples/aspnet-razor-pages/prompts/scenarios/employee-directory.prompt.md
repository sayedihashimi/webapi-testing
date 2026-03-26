---
mode: agent
description: "Create an Employee Directory & HR Portal using ASP.NET Core Razor Pages and .NET 10"
tools: ["changes", "codebase", "fetch", "findTestFiles", "githubRepo", "problems", "runner", "selection", "terminalLastCommand", "terminalSelection", "usages", "visionScreenshot"]
---

# Employee Directory & HR Portal — Horizon HR

## App Overview

Build an employee directory and HR management portal for **"Horizon HR"**, an internal HR system for a mid-size company. This Razor Pages application allows HR staff to manage departments, employee records, leave requests, performance reviews, and employee skills/competencies. The system tracks departmental hierarchy, enforces leave balance rules, manages review cycles, and provides an employee directory with search and filtering capabilities.

The application is a server-rendered web app used by HR administrators through a browser — it is **not** an API. All interactions happen through Razor Pages with forms, tables, and navigation links.

## Technical Requirements

- **Framework**: ASP.NET Core Razor Pages targeting **.NET 10**
- **Database**: Entity Framework Core with **SQLite** (connection string configured in `appsettings.json`, database file stored in the project directory)
- **Authentication**: None — no authentication or authorization required
- **CSS Framework**: Use **Bootstrap 5** for styling. The generated pages must use Bootstrap classes for layout, forms, tables, buttons, badges, and alerts. Use the default Bootstrap theme.
- **Project Structure**: Structured with separation of concerns:
  - **Models** — Entity classes and enums
  - **Services** — Business logic behind interfaces (interface + implementation pattern)
  - **Pages** — Razor Pages organized by feature area
  - **Data** — DbContext and data seeder
- **Validation**: Use Data Annotations on models and input models. Display validation errors inline on forms.
- **Error Handling**: Global error handling middleware with a custom error page. Use TempData for flash messages after form submissions.
- **Project Location**: Create the project at `./src/HorizonHR/`

## Entities & Relationships

### Department

| Field          | Type     | Constraints                                        |
|----------------|----------|----------------------------------------------------|
| Id             | int      | PK, auto-generated                                 |
| Name           | string   | Required, max length 100, unique                   |
| Code           | string   | Required, max length 10, unique (e.g., "ENG", "HR", "MKT") |
| Description    | string   | Optional, max length 500                           |
| ManagerId      | int      | Nullable, FK → Employee (the department manager)   |
| ParentDepartmentId | int  | Nullable, FK → Department (self-referencing for hierarchy) |
| IsActive       | bool     | Default true                                       |
| CreatedAt      | DateTime | Auto-set on creation                               |
| UpdatedAt      | DateTime | Auto-set on creation and update                    |

**Relationships**: A Department has many Employees. A Department has one optional Manager (who is an Employee). A Department has one optional ParentDepartment (self-referencing for organizational hierarchy). A Department has many child Departments.

### Employee

| Field            | Type     | Constraints                                                    |
|------------------|----------|----------------------------------------------------------------|
| Id               | int      | PK, auto-generated                                             |
| EmployeeNumber   | string   | Required, unique, max length 20 (e.g., "EMP-0042")           |
| FirstName        | string   | Required, max length 100                                       |
| LastName         | string   | Required, max length 100                                       |
| Email            | string   | Required, unique, valid email format                           |
| Phone            | string   | Optional                                                       |
| DateOfBirth      | DateOnly | Required                                                       |
| HireDate         | DateOnly | Required                                                       |
| DepartmentId     | int      | FK → Department, required                                      |
| JobTitle         | string   | Required, max length 200                                       |
| EmploymentType   | enum     | FullTime, PartTime, Contract, Intern                           |
| Salary           | decimal  | Required, must be positive                                     |
| ManagerId        | int      | Nullable, FK → Employee (self-referencing — direct manager)    |
| Status           | enum     | Active, OnLeave, Terminated                                    |
| TerminationDate  | DateOnly | Nullable — set when status is Terminated                       |
| ProfileImageUrl  | string   | Optional, max length 500                                       |
| Address          | string   | Optional, max length 500                                       |
| City             | string   | Optional, max length 100                                       |
| State            | string   | Optional, max length 2                                         |
| ZipCode          | string   | Optional, max length 10                                        |
| CreatedAt        | DateTime | Auto-set on creation                                           |
| UpdatedAt        | DateTime | Auto-set on creation and update                                |

**Relationships**: Belongs to Department. Has one optional Manager (self-referencing). Has many direct reports (other Employees where ManagerId = this employee). Has many LeaveRequests, LeaveBalances, PerformanceReviews, and EmployeeSkills.

### LeaveType

| Field            | Type   | Constraints                                |
|------------------|--------|--------------------------------------------|
| Id               | int    | PK, auto-generated                         |
| Name             | string | Required, max length 100, unique (e.g., "Vacation", "Sick", "Personal", "Bereavement") |
| DefaultDaysPerYear | int  | Required, must be positive                 |
| IsCarryOverAllowed | bool | Default false — whether unused days carry over to next year |
| MaxCarryOverDays | int    | Default 0 — max days that can carry over    |
| RequiresApproval | bool   | Default true                               |
| IsPaid           | bool   | Default true                               |

**Relationships**: Has many LeaveBalances and LeaveRequests.

### LeaveBalance

| Field          | Type   | Constraints                                 |
|----------------|--------|---------------------------------------------|
| Id             | int    | PK, auto-generated                          |
| EmployeeId     | int    | FK → Employee, required                     |
| LeaveTypeId    | int    | FK → LeaveType, required                    |
| Year           | int    | Required (e.g., 2026)                       |
| TotalDays      | decimal| Required (e.g., 15.0 — supports half days) |
| UsedDays       | decimal| Default 0                                   |
| CarriedOverDays| decimal| Default 0                                   |

**Computed Properties**:
- **RemainingDays**: TotalDays + CarriedOverDays - UsedDays
- **Unique constraint** on (EmployeeId, LeaveTypeId, Year)

**Relationships**: Belongs to Employee and LeaveType.

### LeaveRequest

| Field          | Type     | Constraints                                                |
|----------------|----------|------------------------------------------------------------|
| Id             | int      | PK, auto-generated                                         |
| EmployeeId     | int      | FK → Employee, required                                    |
| LeaveTypeId    | int      | FK → LeaveType, required                                   |
| StartDate      | DateOnly | Required                                                   |
| EndDate        | DateOnly | Required, must be >= StartDate                             |
| TotalDays      | decimal  | Required, calculated as business days between Start and End (supports 0.5 for half days) |
| Status         | enum     | Submitted, Approved, Rejected, Cancelled                   |
| Reason         | string   | Required, max length 1000                                  |
| ReviewedById   | int      | Nullable, FK → Employee (the manager who approved/rejected)|
| ReviewDate     | DateTime | Nullable                                                   |
| ReviewNotes    | string   | Optional, max length 1000                                  |
| SubmittedDate  | DateTime | Auto-set to now                                            |
| CreatedAt      | DateTime | Auto-set on creation                                       |
| UpdatedAt      | DateTime | Auto-set on creation and update                            |

**Relationships**: Belongs to Employee and LeaveType. Optionally reviewed by another Employee (ReviewedById).

### PerformanceReview

| Field              | Type     | Constraints                                             |
|--------------------|----------|---------------------------------------------------------|
| Id                 | int      | PK, auto-generated                                      |
| EmployeeId         | int      | FK → Employee, required                                 |
| ReviewerId         | int      | FK → Employee, required (the manager conducting review) |
| ReviewPeriodStart  | DateOnly | Required                                                |
| ReviewPeriodEnd    | DateOnly | Required, must be after ReviewPeriodStart               |
| Status             | enum     | Draft, SelfAssessmentPending, ManagerReviewPending, Completed |
| OverallRating      | enum     | Nullable — Outstanding, ExceedsExpectations, MeetsExpectations, NeedsImprovement, Unsatisfactory |
| SelfAssessment     | string   | Optional, max length 5000                               |
| ManagerAssessment  | string   | Optional, max length 5000                               |
| Goals              | string   | Optional, max length 5000 (goals for next period)       |
| StrengthsNoted     | string   | Optional, max length 2000                               |
| AreasForImprovement| string   | Optional, max length 2000                               |
| CompletedDate      | DateOnly | Nullable                                                |
| CreatedAt          | DateTime | Auto-set on creation                                    |
| UpdatedAt          | DateTime | Auto-set on creation and update                         |

**Relationships**: Belongs to Employee (the reviewee) and Reviewer (an Employee).

### Skill

| Field       | Type   | Constraints                      |
|-------------|--------|----------------------------------|
| Id          | int    | PK, auto-generated              |
| Name        | string | Required, max length 100, unique |
| Category    | string | Required, max length 100 (e.g., "Programming Language", "Framework", "Soft Skill", "Tool") |
| Description | string | Optional, max length 500         |

**Relationships**: Has many EmployeeSkills.

### EmployeeSkill

| Field          | Type     | Constraints                                    |
|----------------|----------|------------------------------------------------|
| Id             | int      | PK, auto-generated                             |
| EmployeeId     | int      | FK → Employee, required                        |
| SkillId        | int      | FK → Skill, required                           |
| ProficiencyLevel | enum   | Beginner, Intermediate, Advanced, Expert       |
| YearsOfExperience | int   | Optional, must be >= 0                         |
| LastAssessedDate  | DateOnly | Optional                                     |

**Unique constraint** on (EmployeeId, SkillId).

**Relationships**: Belongs to Employee and Skill. This is the many-to-many join entity between Employee and Skill.

## Business Rules

1. **Leave Balance Enforcement**: When an employee submits a leave request, verify that their `RemainingDays` balance for the requested `LeaveType` and current year is sufficient to cover the `TotalDays` requested. Reject with a clear error if insufficient balance.

2. **Leave Request Workflow**:
   - **Submitted** → Approved, Rejected, Cancelled
   - **Approved**, **Rejected**, and **Cancelled** are terminal states
   - Approving requires `ReviewedById` and `ReviewDate` to be set
   - Leave types with `RequiresApproval = false` are automatically approved on submission

3. **Leave Balance Deduction**: When a leave request is **Approved**, deduct `TotalDays` from the employee's `LeaveBalance.UsedDays` for the appropriate year. When an **Approved** request is **Cancelled**, add the days back to the balance.

4. **No Overlapping Leave**: An employee cannot submit a leave request whose date range overlaps with an existing Submitted or Approved leave request.

5. **Department Manager Constraint**: A department's `ManagerId` must reference an employee who belongs to that department. If the manager is transferred to a different department, the ManagerId must be cleared.

6. **Employee Termination**: When an employee's status is set to **Terminated**:
   - `TerminationDate` must be set
   - All their **Submitted** leave requests are automatically cancelled
   - They are removed as manager from any departments (ManagerId set to null)
   - They are removed as manager from any direct reports (subordinate ManagerId set to null)

7. **Performance Review Workflow**:
   - **Draft** → SelfAssessmentPending
   - **SelfAssessmentPending** → ManagerReviewPending (employee submits self-assessment)
   - **ManagerReviewPending** → Completed (manager submits assessment and rating)
   - **Completed** is a terminal state
   - Moving to Completed requires `OverallRating`, `ManagerAssessment`, and `CompletedDate`

8. **No Duplicate Reviews Per Period**: An employee cannot have two performance reviews with overlapping review periods. Validate when creating a new review.

9. **Leave Balance Initialization**: When a new employee is created, automatically generate `LeaveBalance` records for the current year for all active leave types, using the `DefaultDaysPerYear` values.

10. **Employee Number Auto-Generation**: Employee numbers should be auto-generated in the format `EMP-NNNN` where NNNN is a zero-padded sequential number.

11. **Department Hierarchy**: A department cannot be its own parent. The hierarchy must not contain circular references (e.g., A → B → A).

## Pages

### Dashboard

| Page      | Route | Description                                                                                 |
|-----------|-------|---------------------------------------------------------------------------------------------|
| Dashboard | /     | Overview: total employees, department count, pending leave requests, upcoming reviews, recent hires (last 30 days), employees on leave today, headcount by department (bar chart data or table). |

### Departments

| Page                | Route                           | Description                                                |
|---------------------|--------------------------------|------------------------------------------------------------|
| Department List     | /Departments                   | Hierarchical list showing parent-child relationships. Show employee count per department. |
| Department Details  | /Departments/Details/{id}      | Department info, manager, parent department, list of employees in the department, child departments. |
| Create Department   | /Departments/Create            | Form: name, code, description, parent department dropdown, manager dropdown (populated with employees). |
| Edit Department     | /Departments/Edit/{id}         | Form to edit department details. Manager dropdown filtered to department's employees. |

### Employees

| Page               | Route                            | Description                                                        |
|--------------------|----------------------------------|--------------------------------------------------------------------|
| Employee Directory | /Employees                       | Paginated, searchable list. Search by name/email/employee number. Filter by department, employment type, status. Card or table view. |
| Employee Profile   | /Employees/Details/{id}          | Full profile: personal info, department, manager, direct reports, skills, current leave balances, recent reviews. |
| Create Employee    | /Employees/Create                | Form: personal info, department dropdown, manager dropdown, job title, employment type, salary. Auto-generates employee number. |
| Edit Employee      | /Employees/Edit/{id}             | Form to update employee info. Department change clears manager if old manager isn't in new department. |
| Terminate Employee | /Employees/Terminate/{id}        | Confirmation page with required termination date. Shows cascading effects (leave cancellations, manager removals). |
| Direct Reports     | /Employees/{id}/DirectReports    | List of employees reporting to this person                          |

### Leave Management

| Page                  | Route                              | Description                                                     |
|-----------------------|------------------------------------|-----------------------------------------------------------------|
| Leave Requests        | /Leave                             | Paginated list of all leave requests. Filter by status, employee, leave type, date range. |
| Leave Request Details | /Leave/Details/{id}                | Full request details with employee info, balance context, review info. |
| Submit Leave Request  | /Leave/Create                      | Form: select employee, leave type, dates. Show remaining balance. Auto-calculate business days. |
| Approve/Reject Leave  | /Leave/Review/{id}                 | Form for manager to approve or reject with notes. Show balance impact. |
| Cancel Leave          | /Leave/Cancel/{id}                 | Confirmation page. Show balance restoration if previously approved. |
| Leave Balances        | /Leave/Balances                    | Table showing all employees' leave balances for the current year. Filter by department. |
| Employee Leave Summary| /Leave/Employee/{id}               | Specific employee's leave balance breakdown and request history. |

### Performance Reviews

| Page                | Route                             | Description                                               |
|---------------------|-----------------------------------|-----------------------------------------------------------|
| Review List         | /Reviews                          | Paginated list. Filter by status, rating, period, department. |
| Review Details      | /Reviews/Details/{id}             | Full review with self-assessment, manager assessment, rating, goals. |
| Create Review       | /Reviews/Create                   | Form: select employee, reviewer (defaults to employee's manager), review period dates. |
| Self-Assessment     | /Reviews/SelfAssessment/{id}      | Form for employee self-assessment text entry. Transitions status. |
| Manager Review      | /Reviews/ManagerReview/{id}       | Form for manager: assessment, strengths, areas for improvement, rating dropdown, goals. Transitions to Completed. |

### Skills

| Page                 | Route                           | Description                                               |
|----------------------|---------------------------------|-----------------------------------------------------------|
| Skill List           | /Skills                         | List all skills grouped by category. Show employee count per skill. |
| Create Skill         | /Skills/Create                  | Form to add a new skill to the catalog                     |
| Edit Skill           | /Skills/Edit/{id}               | Form to edit a skill                                       |
| Manage Employee Skills | /Employees/{id}/Skills        | List of employee's skills with proficiency. Add/edit/remove skills. |
| Skill Search         | /Skills/Search                  | Find employees by skill: select skill and minimum proficiency, get matching employee list. |

## Seed Data

The application **MUST** seed the database on startup with realistic dummy data for demo and testing purposes. Include:

- At least **5 departments** with hierarchy (e.g., "Engineering" with children "Frontend" and "Backend", "Human Resources", "Marketing")
- At least **4 leave types** (Vacation: 15 days/year with 5-day carry-over, Sick: 10 days/year no carry-over, Personal: 3 days/year no carry-over, Bereavement: 5 days/year no approval required)
- At least **12 employees** across departments with a mix of employment types. Include:
  - Department managers assigned
  - Manager-report relationships (at least 2 levels deep)
  - One terminated employee
  - One employee currently on leave
- **Leave balances** for all active employees for the current year (auto-seeded per leave type defaults, some with used days)
- At least **8 leave requests** in various statuses (Submitted, Approved, Rejected, Cancelled)
- At least **6 performance reviews** in various statuses (Draft, SelfAssessmentPending, ManagerReviewPending, Completed)
- At least **10 skills** across categories ("Python", "JavaScript", "Project Management", "Communication", "Docker", "SQL", etc.)
- At least **20 employee-skill records** with varying proficiency levels
- Ensure data consistency: department managers belong to their departments, leave balances match used days from approved requests, terminated employee has cascading effects applied

Use the EF Core seeding mechanism or a data seeder service. Ensure the seed data only runs when the database is empty to avoid duplicates.

## Cross-Cutting Concerns

### Error Handling
- Global exception handler middleware redirecting to a custom `/Error` page
- Business rule violations displayed as TempData error messages or inline validation errors
- Use the **Post-Redirect-Get (PRG)** pattern for all form submissions

### Validation
- Use Data Annotations on all page model input properties
- Display validation errors inline using `asp-validation-for` tag helpers
- Include validation summary at the top of forms using `asp-validation-summary`
- Enable client-side validation with jQuery Validation Unobtrusive

### Logging
- Use the built-in `ILogger`
- Log key operations at **Information** level: employee created/terminated, leave approved/rejected, review completed, department manager changed
- Log errors at **Error** level

### Layout & Navigation
- Shared `_Layout.cshtml` with Bootstrap 5 navbar
- Navigation: Dashboard, Employees, Departments, Leave, Reviews, Skills
- Bootstrap badges for status indicators (color-coded by status and rating)
- Bootstrap alerts for TempData success/error messages
- Footer with app name

### Pagination
- Consistent pagination on all list pages
- Accept `pageNumber` and `pageSize` query parameters with defaults (page 1, 10 items)
- Render Bootstrap pagination controls
- Extract pagination into a reusable partial view or View Component so it is not duplicated across list pages

### Semantic HTML & Accessibility
- Use semantic HTML elements throughout: `<nav>` for navigation, `<main>` for primary content area, `<section>` for grouped content, `<table>` with `<thead>`/`<tbody>`, `<button>` for actions (not `<a>` styled as a button)
- Associate all `<label>` elements with their inputs using `asp-for`
- Add `role="alert"` on TempData flash message alerts for screen reader announcement
- Add `aria-label` on the pagination `<nav>` element and the main site navigation
- Show "No items found" messages when lists/tables are empty rather than rendering an empty table
- Use `aria-current="page"` on the active navigation item

### Input Models & Form Design
- Use dedicated input model classes (nested `InputModel` class or separate DTO) for form binding via `[BindProperty]` — do not bind directly to entity classes
- Use named handler methods (`OnPostApproveAsync`, `OnPostRejectAsync`) when a page has multiple form actions instead of a single `OnPost` with branching logic
- Use `asp-page-handler` tag helper to route to named handlers

### Reusable UI Components
- Create at least one reusable partial view or View Component for UI patterns that repeat across pages (e.g., a status badge component, an employee card, or a reusable pagination control)
- Avoid duplicating the same HTML patterns across multiple pages

## Project Location

Create the project at: `./src/HorizonHR/`

The project should be a standalone ASP.NET Core Razor Pages project with no dependencies on other projects in this repository. It should be fully self-contained and runnable with `dotnet run`.
