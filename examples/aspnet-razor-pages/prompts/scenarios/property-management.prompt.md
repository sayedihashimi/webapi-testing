---
mode: agent
description: "Create a Residential Property Management App using ASP.NET Core Razor Pages and .NET 10"
tools: ["changes", "codebase", "fetch", "findTestFiles", "githubRepo", "problems", "runner", "selection", "terminalLastCommand", "terminalSelection", "usages", "visionScreenshot"]
---

# Residential Property Management — Keystone Properties

## App Overview

Build a residential property management application for **"Keystone Properties"**, a company that manages apartment buildings and rental units. This Razor Pages application allows property managers to track properties and units, manage tenants and leases, process rent payments, handle maintenance requests, and schedule inspections. The system enforces lease rules, calculates late fees, and tracks unit availability across the portfolio.

The application is a server-rendered web app used by property managers and office staff through a browser — it is **not** an API. All interactions happen through Razor Pages with forms, tables, dashboards, and navigation links.

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
- **Project Location**: Create the project at `./src/KeystoneProperties/`

## Entities & Relationships

### Property

| Field          | Type     | Constraints                                |
|----------------|----------|--------------------------------------------|
| Id             | int      | PK, auto-generated                         |
| Name           | string   | Required, max length 200 (e.g., "Maple Ridge Apartments") |
| Address        | string   | Required, max length 500                   |
| City           | string   | Required, max length 100                   |
| State          | string   | Required, max length 2                     |
| ZipCode        | string   | Required, max length 10                    |
| PropertyType   | enum     | Apartment, Townhouse, SingleFamily, Condo  |
| YearBuilt      | int      | Optional                                   |
| TotalUnits     | int      | Required, must be positive                 |
| Description    | string   | Optional, max length 2000                  |
| IsActive       | bool     | Default true                               |
| CreatedAt      | DateTime | Auto-set on creation                       |
| UpdatedAt      | DateTime | Auto-set on creation and update            |

**Relationships**: A Property has many Units.

### Unit

| Field          | Type     | Constraints                                          |
|----------------|----------|------------------------------------------------------|
| Id             | int      | PK, auto-generated                                   |
| PropertyId     | int      | FK → Property, required                              |
| UnitNumber     | string   | Required, max length 20 (e.g., "101", "2B")         |
| Floor          | int      | Optional                                             |
| Bedrooms       | int      | Required, range 0–5 (0 = studio)                    |
| Bathrooms      | decimal  | Required, range 0.5–4 (supports half baths)         |
| SquareFeet     | int      | Required, must be positive                           |
| MonthlyRent    | decimal  | Required, must be positive                           |
| DepositAmount  | decimal  | Required, must be positive                           |
| Status         | enum     | Available, Occupied, Maintenance, OffMarket          |
| Amenities      | string   | Optional, max length 1000 (e.g., "Washer/Dryer, Balcony, Parking") |
| CreatedAt      | DateTime | Auto-set on creation                                 |
| UpdatedAt      | DateTime | Auto-set on creation and update                      |

**Relationships**: Belongs to Property. Has many Leases, MaintenanceRequests, and Inspections. **Unique constraint** on (PropertyId, UnitNumber).

### Tenant

| Field          | Type     | Constraints                          |
|----------------|----------|--------------------------------------|
| Id             | int      | PK, auto-generated                   |
| FirstName      | string   | Required, max length 100             |
| LastName       | string   | Required, max length 100             |
| Email          | string   | Required, unique, valid email format |
| Phone          | string   | Required                             |
| DateOfBirth    | DateOnly | Required, must be at least 18 years old |
| EmergencyContactName  | string | Required, max length 200       |
| EmergencyContactPhone | string | Required                       |
| IsActive       | bool     | Default true                         |
| CreatedAt      | DateTime | Auto-set on creation                 |
| UpdatedAt      | DateTime | Auto-set on creation and update      |

**Relationships**: Has many Leases.

### Lease

| Field              | Type     | Constraints                                                        |
|--------------------|----------|--------------------------------------------------------------------|
| Id                 | int      | PK, auto-generated                                                 |
| UnitId             | int      | FK → Unit, required                                                |
| TenantId           | int      | FK → Tenant, required                                              |
| StartDate          | DateOnly | Required                                                           |
| EndDate            | DateOnly | Required, must be after StartDate                                  |
| MonthlyRentAmount  | decimal  | Required, must be positive (may differ from Unit.MonthlyRent due to negotiation) |
| DepositAmount      | decimal  | Required, must be positive                                         |
| DepositStatus      | enum     | Held, PartiallyReturned, Returned, Forfeited                      |
| Status             | enum     | Active, Expired, Renewed, Terminated, Pending                     |
| RenewalOfLeaseId   | int      | Nullable, FK → Lease (self-referencing — tracks which lease this renewed) |
| TerminationDate    | DateOnly | Nullable — set when lease is terminated early                      |
| TerminationReason  | string   | Optional — required when Status is Terminated                      |
| Notes              | string   | Optional, max length 2000                                          |
| CreatedAt          | DateTime | Auto-set on creation                                               |
| UpdatedAt          | DateTime | Auto-set on creation and update                                    |

**Relationships**: Belongs to Unit and Tenant. Has many Payments. Self-references via RenewalOfLeaseId.

### Payment

| Field          | Type     | Constraints                                              |
|----------------|----------|----------------------------------------------------------|
| Id             | int      | PK, auto-generated                                       |
| LeaseId        | int      | FK → Lease, required                                     |
| Amount         | decimal  | Required, must be positive                               |
| PaymentDate    | DateOnly | Required                                                 |
| DueDate        | DateOnly | Required                                                 |
| PaymentMethod  | enum     | Check, BankTransfer, CreditCard, Cash, MoneyOrder        |
| PaymentType    | enum     | Rent, LateFee, Deposit, DepositReturn, Other             |
| Status         | enum     | Completed, Pending, Failed, Refunded                     |
| ReferenceNumber| string   | Optional, max length 100                                 |
| Notes          | string   | Optional, max length 500                                 |
| CreatedAt      | DateTime | Auto-set on creation                                     |

**Relationships**: Belongs to Lease.

### MaintenanceRequest

| Field             | Type     | Constraints                                                 |
|-------------------|----------|-------------------------------------------------------------|
| Id                | int      | PK, auto-generated                                          |
| UnitId            | int      | FK → Unit, required                                         |
| TenantId          | int      | FK → Tenant, required (the tenant who submitted the request)|
| Title             | string   | Required, max length 200                                    |
| Description       | string   | Required, max length 2000                                   |
| Priority          | enum     | Low, Medium, High, Emergency                                |
| Status            | enum     | Submitted, Assigned, InProgress, Completed, Cancelled       |
| Category          | enum     | Plumbing, Electrical, HVAC, Appliance, Structural, Pest, General |
| AssignedTo        | string   | Optional, max length 200 (maintenance worker name)          |
| SubmittedDate     | DateTime | Auto-set to now                                             |
| AssignedDate      | DateTime | Nullable                                                    |
| CompletedDate     | DateTime | Nullable                                                    |
| CompletionNotes   | string   | Optional, max length 2000                                   |
| EstimatedCost     | decimal  | Optional                                                    |
| ActualCost        | decimal  | Optional                                                    |
| CreatedAt         | DateTime | Auto-set on creation                                        |
| UpdatedAt         | DateTime | Auto-set on creation and update                             |

**Relationships**: Belongs to Unit and Tenant.

### Inspection

| Field           | Type     | Constraints                                              |
|-----------------|----------|----------------------------------------------------------|
| Id              | int      | PK, auto-generated                                       |
| UnitId          | int      | FK → Unit, required                                      |
| InspectionType  | enum     | MoveIn, MoveOut, Routine, Emergency                      |
| ScheduledDate   | DateOnly | Required                                                 |
| CompletedDate   | DateOnly | Nullable                                                 |
| InspectorName   | string   | Required, max length 200                                 |
| OverallCondition| enum     | Excellent, Good, Fair, Poor                              |
| Notes           | string   | Optional, max length 5000                                |
| FollowUpRequired| bool     | Default false                                            |
| LeaseId         | int      | Nullable, FK → Lease (links inspection to a specific lease, e.g., move-in/move-out) |
| CreatedAt       | DateTime | Auto-set on creation                                     |

**Relationships**: Belongs to Unit. Optionally belongs to Lease.

## Business Rules

1. **No Overlapping Leases**: A unit cannot have two Active or Pending leases whose date ranges overlap. When creating or editing a lease, verify there is no existing Active/Pending lease for that unit that overlaps with the proposed (StartDate, EndDate) range.

2. **Unit Status Sync**: When a lease becomes Active, the unit's status should transition to **Occupied**. When a lease expires or is terminated and no other active lease exists for the unit, the unit's status should transition to **Available**.

3. **Lease Status Workflow**:
   - **Pending** → Active (when StartDate arrives or manually activated), Terminated
   - **Active** → Expired (when EndDate passes), Renewed, Terminated
   - **Expired**, **Renewed**, and **Terminated** are terminal states
   - Setting status to Terminated requires a `TerminationReason` and sets `TerminationDate`

4. **Late Fee Calculation**: Rent payments made more than 5 days after the `DueDate` incur a late fee of **$50.00 plus $5.00 per additional day** late (capped at $200.00). When a late rent payment is recorded, automatically generate a corresponding LateFee payment record.

5. **Lease Renewal**: Renewing a lease creates a new Lease record with `RenewalOfLeaseId` pointing to the original. The new lease's `StartDate` must be the day after the original lease's `EndDate`. The original lease's status transitions to **Renewed**.

6. **Maintenance Request Workflow**:
   - **Submitted** → Assigned, Cancelled
   - **Assigned** → InProgress, Cancelled
   - **InProgress** → Completed, Cancelled
   - **Completed** and **Cancelled** are terminal states
   - Moving to Assigned requires `AssignedTo` and sets `AssignedDate`
   - Moving to Completed sets `CompletedDate`

7. **Emergency Maintenance**: Maintenance requests with Priority **Emergency** must be assigned within the same page flow (the user must provide `AssignedTo` when creating an Emergency request). The unit's status temporarily changes to **Maintenance** when an Emergency request is submitted and returns to its previous status when the request is completed.

8. **Tenant Active Lease Requirement**: A tenant's `IsActive` flag cannot be set to false (deactivated) if they have any Active leases. Deactivation must fail with a descriptive error.

9. **Deposit Tracking**: When a lease is terminated or expires:
   - The `DepositStatus` should be updated (Held → Returned, PartiallyReturned, or Forfeited)
   - If a deposit is returned (full or partial), a Payment record of type `DepositReturn` should be created with a negative amount

10. **Property Deactivation**: A property cannot be deactivated (`IsActive = false`) if any of its units have Active leases.

## Pages

### Dashboard

| Page      | Route | Description                                                                                |
|-----------|-------|--------------------------------------------------------------------------------------------|
| Dashboard | /     | Overview: total properties, total units, occupancy rate, rent collected this month, overdue payments count, open maintenance requests count, upcoming lease expirations (next 30 days). |

### Properties

| Page              | Route                         | Description                                                     |
|-------------------|-------------------------------|-----------------------------------------------------------------|
| Property List     | /Properties                   | Paginated list with search by name/city, filter by type/active status. Shows unit count and occupancy rate per property. |
| Property Details  | /Properties/Details/{id}      | Property info with table of all units showing status, rent, current tenant name. Occupancy summary. |
| Create Property   | /Properties/Create            | Form to create a new property                                    |
| Edit Property     | /Properties/Edit/{id}         | Form to edit property details                                    |
| Deactivate        | /Properties/Deactivate/{id}   | Confirmation page (fail if any units have active leases)         |

### Units

| Page            | Route                          | Description                                                         |
|-----------------|--------------------------------|---------------------------------------------------------------------|
| Unit List       | /Units                         | Paginated list with filter by property, status, bedrooms, rent range. Search by unit number. |
| Unit Details    | /Units/Details/{id}            | Unit info, current lease, lease history, maintenance history, inspections. |
| Create Unit     | /Units/Create                  | Form with property dropdown to create a new unit                     |
| Edit Unit       | /Units/Edit/{id}               | Form to edit unit details                                            |

### Tenants

| Page             | Route                         | Description                                                  |
|------------------|-------------------------------|--------------------------------------------------------------|
| Tenant List      | /Tenants                      | Paginated, searchable list. Search by name/email. Filter by active. |
| Tenant Details   | /Tenants/Details/{id}         | Tenant info, current and past leases, payment history, maintenance requests. |
| Create Tenant    | /Tenants/Create               | Form to add a new tenant (enforce 18+ age requirement)        |
| Edit Tenant      | /Tenants/Edit/{id}            | Form to update tenant info                                    |
| Deactivate       | /Tenants/Deactivate/{id}      | Confirmation page (fail if active leases exist)               |

### Leases

| Page             | Route                         | Description                                                        |
|------------------|-------------------------------|--------------------------------------------------------------------|
| Lease List       | /Leases                       | Paginated list with filter by status, property. Shows tenant, unit, dates, rent. |
| Lease Details    | /Leases/Details/{id}          | Full lease info with payment history and linked inspections. Shows renewal chain if applicable. |
| Create Lease     | /Leases/Create                | Form: select tenant and unit (dropdowns filtered to available), set dates and amounts. Validate no overlap. |
| Edit Lease       | /Leases/Edit/{id}             | Form to edit lease (limited fields if Active)                       |
| Terminate Lease  | /Leases/Terminate/{id}        | Confirmation page with required termination reason and deposit disposition. |
| Renew Lease      | /Leases/Renew/{id}            | Pre-filled form for renewal: new start date (day after current end), new end date, rent amount. |

### Payments

| Page             | Route                         | Description                                                         |
|------------------|-------------------------------|---------------------------------------------------------------------|
| Payment List     | /Payments                     | Paginated list with filter by type, status, date range, property. Sortable by date. |
| Payment Details  | /Payments/Details/{id}        | Payment info with lease and tenant context                           |
| Record Payment   | /Payments/Create              | Form: select lease (dropdown), enter amount, date, method. Auto-calculate late fee if applicable. Show due date and days late. |
| Overdue Payments | /Payments/Overdue             | List of all leases with overdue rent (DueDate passed, no matching Completed rent payment). |

### Maintenance

| Page                   | Route                               | Description                                                   |
|------------------------|-------------------------------------|---------------------------------------------------------------|
| Maintenance List       | /Maintenance                        | Paginated list with filter by status, priority, property, category. Badge colors by priority. |
| Maintenance Details    | /Maintenance/Details/{id}           | Full request details with unit and tenant info, status timeline. |
| Create Request         | /Maintenance/Create                 | Form: select unit and tenant (dropdown), enter details. If Emergency, require AssignedTo. |
| Update Status          | /Maintenance/UpdateStatus/{id}      | Form to transition status (dropdown shows only valid next states), add notes, set costs. |

### Inspections

| Page                | Route                          | Description                                          |
|---------------------|--------------------------------|------------------------------------------------------|
| Inspection List     | /Inspections                   | Paginated list with filter by type, unit, date range  |
| Inspection Details  | /Inspections/Details/{id}      | Full inspection info with unit context                 |
| Schedule Inspection | /Inspections/Create            | Form: select unit, type, date, inspector name          |
| Complete Inspection | /Inspections/Complete/{id}     | Form to record results: condition, notes, follow-up flag |

## Seed Data

The application **MUST** seed the database on startup with realistic dummy data for demo and testing purposes. Include:

- At least **3 properties** with realistic names and addresses (e.g., "Maple Ridge Apartments" — 12 units, "Cedar Park Townhomes" — 8 units, "Lakeview Condos" — 6 units)
- At least **15 units** across the properties with varying bedroom counts, rents ($800–$2,200/month), and statuses (most Occupied, some Available, one in Maintenance)
- At least **10 tenants** with realistic names, emails, and emergency contacts
- At least **12 leases** — most Active, some Expired, one Terminated, one Pending. Include a renewal chain (original → renewed lease).
- At least **20 payments** — mix of Rent, LateFee, and Deposit types. Include some overdue situations (payment after due date with corresponding late fees).
- At least **8 maintenance requests** in various statuses and priorities, including one Emergency
- At least **5 inspections** — MoveIn, MoveOut, and Routine types
- Ensure unit statuses match lease states (Occupied units have Active leases, Available units don't)
- Ensure payment amounts and dates are internally consistent

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
- Log key operations at **Information** level: lease created, payment recorded, maintenance status changed, tenant deactivated
- Log errors at **Error** level

### Layout & Navigation
- Shared `_Layout.cshtml` with Bootstrap 5 navbar
- Navigation: Dashboard, Properties, Units, Tenants, Leases, Payments, Maintenance, Inspections
- Bootstrap badges for status indicators (color-coded by status)
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
- Use named handler methods (`OnPostTerminateAsync`, `OnPostRenewAsync`) when a page has multiple form actions instead of a single `OnPost` with branching logic
- Use `asp-page-handler` tag helper to route to named handlers

### Reusable UI Components
- Create at least one reusable partial view or View Component for UI patterns that repeat across pages (e.g., a status badge component, a unit availability indicator, or a reusable pagination control)
- Avoid duplicating the same HTML patterns across multiple pages

## Project Location

Create the project at: `./src/KeystoneProperties/`

The project should be a standalone ASP.NET Core Razor Pages project with no dependencies on other projects in this repository. It should be fully self-contained and runnable with `dotnet run`.
