---
mode: agent
description: "Create a Community Event Registration Portal using ASP.NET Core Razor Pages and .NET 10"
tools: ["changes", "codebase", "fetch", "findTestFiles", "githubRepo", "problems", "runner", "selection", "terminalLastCommand", "terminalSelection", "usages", "visionScreenshot"]
---

# Community Event Registration Portal — Spark Events

## App Overview

Build a community event registration portal for **"Spark Events"**, an event management company that organizes conferences, workshops, and meetups. This Razor Pages application allows staff to create and manage events, define ticket types with pricing tiers, process attendee registrations, and handle day-of check-in. The system tracks event capacity, manages waitlists when events sell out, and enforces business rules around pricing cutoffs and cancellation policies.

The application is a server-rendered web app used by event coordinators through a browser — it is **not** an API. All interactions happen through Razor Pages with forms, tables, and navigation links.

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
- **Validation**: Use Data Annotations on models and input models. Display validation errors inline on forms using `asp-validation-for` and `asp-validation-summary`.
- **Error Handling**: Global error handling middleware. Use a custom error page for unhandled exceptions. Use TempData for flash messages (success/error notifications after form submissions).
- **Project Location**: Create the project at `./src/SparkEvents/`

## Entities & Relationships

### EventCategory

| Field       | Type   | Constraints                      |
|-------------|--------|----------------------------------|
| Id          | int    | PK, auto-generated              |
| Name        | string | Required, max length 100, unique |
| Description | string | Optional, max length 500         |
| ColorHex    | string | Optional, max length 7 (e.g., "#FF5733" — used for UI badges) |

**Relationships**: A category has many Events.

### Venue

| Field        | Type   | Constraints                      |
|--------------|--------|----------------------------------|
| Id           | int    | PK, auto-generated              |
| Name         | string | Required, max length 200         |
| Address      | string | Required, max length 500         |
| City         | string | Required, max length 100         |
| State        | string | Required, max length 2           |
| ZipCode      | string | Required, max length 10          |
| MaxCapacity  | int    | Required, must be positive       |
| ContactEmail | string | Optional, valid email format     |
| ContactPhone | string | Optional                         |
| Notes        | string | Optional, max length 1000        |
| CreatedAt    | DateTime | Auto-set on creation           |

**Relationships**: A Venue hosts many Events.

### Event

| Field              | Type     | Constraints                                                              |
|--------------------|----------|--------------------------------------------------------------------------|
| Id                 | int      | PK, auto-generated                                                       |
| Title              | string   | Required, max length 300                                                 |
| Description        | string   | Required, max length 5000                                                |
| EventCategoryId    | int      | FK → EventCategory, required                                             |
| VenueId            | int      | FK → Venue, required                                                     |
| StartDate          | DateTime | Required                                                                 |
| EndDate            | DateTime | Required, must be after StartDate                                        |
| RegistrationOpenDate  | DateTime | Required — when registration opens                                    |
| RegistrationCloseDate | DateTime | Required — when registration closes, must be before or equal to StartDate |
| EarlyBirdDeadline  | DateTime | Optional — early-bird pricing ends after this date                       |
| TotalCapacity       | int     | Required, must be positive, cannot exceed Venue.MaxCapacity              |
| CurrentRegistrations | int    | Tracked — count of confirmed (non-cancelled) registrations               |
| WaitlistCount      | int     | Tracked — count of waitlisted registrations                              |
| Status             | enum    | Draft, Published, SoldOut, Completed, Cancelled                          |
| IsFeatured         | bool    | Default false — featured events appear first in listings                 |
| CancellationReason | string  | Optional — required when status is Cancelled                             |
| CreatedAt          | DateTime | Auto-set on creation                                                    |
| UpdatedAt          | DateTime | Auto-set on creation and update                                         |

**Relationships**: Belongs to EventCategory and Venue. Has many TicketTypes and Registrations.

### TicketType

| Field           | Type    | Constraints                                                            |
|-----------------|---------|------------------------------------------------------------------------|
| Id              | int     | PK, auto-generated                                                     |
| EventId         | int     | FK → Event, required                                                   |
| Name            | string  | Required, max length 100 (e.g., "General Admission", "VIP", "Student") |
| Description     | string  | Optional, max length 500                                               |
| Price           | decimal | Required, must be >= 0 (0 = free)                                      |
| EarlyBirdPrice  | decimal | Optional — if set, this price applies before Event.EarlyBirdDeadline   |
| Quantity         | int    | Required, must be positive                                             |
| QuantitySold     | int    | Tracked — count of confirmed registrations for this ticket type        |
| SortOrder       | int    | Default 0 — controls display order on registration page                |
| IsActive        | bool   | Default true                                                           |
| CreatedAt       | DateTime | Auto-set on creation                                                  |

**Relationships**: Belongs to Event. Has many Registrations.

### Attendee

| Field       | Type     | Constraints                          |
|-------------|----------|--------------------------------------|
| Id          | int      | PK, auto-generated                   |
| FirstName   | string   | Required, max length 100             |
| LastName    | string   | Required, max length 100             |
| Email       | string   | Required, unique, valid email format |
| Phone       | string   | Optional                             |
| Organization| string   | Optional, max length 200             |
| DietaryNeeds| string   | Optional, max length 500             |
| CreatedAt   | DateTime | Auto-set on creation                 |
| UpdatedAt   | DateTime | Auto-set on creation and update      |

**Relationships**: Has many Registrations.

### Registration

| Field              | Type     | Constraints                                                               |
|--------------------|----------|---------------------------------------------------------------------------|
| Id                 | int      | PK, auto-generated                                                        |
| EventId            | int      | FK → Event, required                                                      |
| AttendeeId         | int      | FK → Attendee, required                                                   |
| TicketTypeId       | int      | FK → TicketType, required                                                 |
| ConfirmationNumber | string   | Required, unique, auto-generated (e.g., "SPK-20260315-0042")             |
| Status             | enum     | Confirmed, Waitlisted, Cancelled, CheckedIn, NoShow                      |
| AmountPaid         | decimal  | Required — the price at time of registration (early-bird or regular)      |
| WaitlistPosition   | int      | Nullable — only set when Status is Waitlisted                             |
| RegistrationDate   | DateTime | Auto-set to now                                                           |
| CancellationDate   | DateTime | Nullable                                                                  |
| CancellationReason | string   | Optional                                                                  |
| CheckInTime        | DateTime | Nullable — set when attendee checks in                                    |
| SpecialRequests    | string   | Optional, max length 1000                                                 |
| CreatedAt          | DateTime | Auto-set on creation                                                      |
| UpdatedAt          | DateTime | Auto-set on creation and update                                           |

**Relationships**: Belongs to Event, Attendee, and TicketType.

### CheckIn

| Field          | Type     | Constraints                                |
|----------------|----------|--------------------------------------------|
| Id             | int      | PK, auto-generated                         |
| RegistrationId | int      | FK → Registration, required                |
| CheckInTime    | DateTime | Auto-set to now                            |
| CheckedInBy    | string   | Required, max length 200 (staff name)      |
| Notes          | string   | Optional, max length 500                   |

**Relationships**: Belongs to Registration. One-to-one (a registration can only be checked in once).

## Business Rules

1. **Registration Window**: Attendees can only register for an event when the current date/time is between `RegistrationOpenDate` and `RegistrationCloseDate`. Attempting to register outside this window must show a clear error.

2. **Capacity Management**: When an event reaches `TotalCapacity` confirmed registrations, new registrations go to the waitlist. The event status transitions to **SoldOut**. When a confirmed registration is cancelled, the first person on the waitlist is automatically promoted to Confirmed.

3. **Ticket Type Capacity**: Each ticket type has its own `Quantity`. A registration for a specific ticket type must be rejected if `QuantitySold >= Quantity` for that type, even if overall event capacity hasn't been reached. The attendee should be guided to choose a different ticket type.

4. **Early-Bird Pricing**: If the event has an `EarlyBirdDeadline` and a ticket type has an `EarlyBirdPrice`, registrations made before the deadline pay the early-bird price. Registrations after the deadline pay the regular `Price`. The `AmountPaid` on the registration must reflect the correct price at time of registration.

5. **No Duplicate Registrations**: An attendee cannot register for the same event more than once (unless their previous registration was cancelled). Enforce uniqueness on (AttendeeId, EventId) for non-cancelled registrations.

6. **Cancellation Policy**: Registrations can be cancelled up to 24 hours before the event `StartDate`. Cancellations within 24 hours of the event are not allowed. Cancelled registrations decrement the ticket type's `QuantitySold` and the event's `CurrentRegistrations`. If there are waitlisted registrations, the first one is promoted.

7. **Event Cancellation Cascade**: When an event is cancelled (status → Cancelled), all non-cancelled registrations for that event are automatically cancelled with reason "Event cancelled by organizer". A `CancellationReason` is required on the event itself.

8. **Event Status Workflow**:
   - **Draft** → Published (only if at least one active TicketType exists)
   - **Published** → SoldOut (automatic when capacity is reached), Cancelled
   - **SoldOut** → Published (automatic when a cancellation frees capacity), Cancelled
   - **Published/SoldOut** → Completed (manual, only after EndDate has passed)
   - **Draft** → Cancelled
   - **Completed** and **Cancelled** are terminal states

9. **Check-In Window**: Check-in is only allowed on the day of the event (between StartDate minus 1 hour and EndDate). Only registrations with status **Confirmed** can be checked in. Check-in transitions the registration status to **CheckedIn**.

10. **Venue Capacity Constraint**: An event's `TotalCapacity` cannot exceed the `Venue.MaxCapacity`. This must be validated when creating or editing an event.

11. **Confirmation Number Format**: Auto-generate confirmation numbers in the format `SPK-YYYYMMDD-NNNN` where YYYYMMDD is the event start date and NNNN is a zero-padded sequential number per event.

## Pages

### Event Categories

| Page               | Route                         | Description                                           |
|--------------------|-------------------------------|-------------------------------------------------------|
| Category List      | /Categories                   | Table of all categories with edit/delete links         |
| Create Category    | /Categories/Create            | Form to create a new category                          |
| Edit Category      | /Categories/Edit/{id}         | Form to edit an existing category                      |
| Delete Category    | /Categories/Delete/{id}       | Confirmation page before deleting (fail if category has events) |

### Venues

| Page          | Route                    | Description                                                     |
|---------------|--------------------------|---------------------------------------------------------------|
| Venue List    | /Venues                  | Table of all venues with capacity info and edit/delete links    |
| Venue Details | /Venues/Details/{id}     | Venue details with list of upcoming events at this venue        |
| Create Venue  | /Venues/Create           | Form to create a new venue                                      |
| Edit Venue    | /Venues/Edit/{id}        | Form to edit a venue                                            |
| Delete Venue  | /Venues/Delete/{id}      | Confirmation page (fail if venue has future events)             |

### Events

| Page               | Route                           | Description                                                            |
|--------------------|---------------------------------|------------------------------------------------------------------------|
| Event List         | /Events                         | Paginated, filterable list — search by title, filter by category/status/date range. Show status badges. Featured events first. |
| Event Details      | /Events/Details/{id}            | Full event info with ticket types, capacity bar, registration count, waitlist count. Links to register, view roster, manage. |
| Create Event       | /Events/Create                  | Multi-section form: basic info, venue selection (dropdown), dates, capacity. Venue dropdown shows max capacity. |
| Edit Event         | /Events/Edit/{id}               | Edit event details. Warn if reducing capacity below current registrations. |
| Cancel Event       | /Events/Cancel/{id}             | Confirmation page with required cancellation reason. Shows count of registrations that will be cancelled. |
| Complete Event     | /Events/Complete/{id}           | Confirmation page to mark event as completed (only after EndDate).     |
| Manage Ticket Types| /Events/{eventId}/TicketTypes   | List, create, edit, deactivate ticket types for an event               |

### Attendees

| Page                | Route                          | Description                                                      |
|---------------------|--------------------------------|------------------------------------------------------------------|
| Attendee List       | /Attendees                     | Searchable, paginated list of all attendees. Search by name/email. |
| Attendee Details    | /Attendees/Details/{id}        | Attendee info with registration history across all events          |
| Create Attendee     | /Attendees/Create              | Form to register a new attendee in the system                      |
| Edit Attendee       | /Attendees/Edit/{id}           | Form to update attendee information                                |

### Registrations

| Page                    | Route                                       | Description                                                       |
|-------------------------|---------------------------------------------|-------------------------------------------------------------------|
| Register for Event      | /Events/{eventId}/Register                  | Form: select attendee (or create new), select ticket type, confirm price. Show remaining capacity per ticket type. |
| Registration Details    | /Registrations/Details/{id}                 | Full registration info with confirmation number, payment amount, status badge |
| Cancel Registration     | /Registrations/Cancel/{id}                  | Confirmation page with cancellation policy info. Block if within 24 hours. |
| Event Roster            | /Events/{eventId}/Roster                    | Table of confirmed registrations for an event. Searchable. Shows check-in status. |
| Event Waitlist          | /Events/{eventId}/Waitlist                  | Table of waitlisted registrations in queue order                   |

### Check-In

| Page               | Route                             | Description                                                           |
|--------------------|-----------------------------------|-----------------------------------------------------------------------|
| Check-In Dashboard | /Events/{eventId}/CheckIn         | Lookup by confirmation number or attendee name. Show check-in stats (checked in / total). |
| Process Check-In   | /Events/{eventId}/CheckIn/Process | Form to confirm check-in: enter staff name, optional notes. Shows attendee and ticket info. |

### Dashboard

| Page      | Route | Description                                                                                     |
|-----------|-------|-------------------------------------------------------------------------------------------------|
| Dashboard | /     | Home page showing: upcoming events (next 7 days), today's events with check-in progress bars, recent registrations, and quick stats (total events, total registrations, events this month). |

## Seed Data

The application **MUST** seed the database on startup with realistic dummy data for demo and testing purposes. Include:

- At least **4 event categories** (e.g., "Technology", "Business", "Creative Arts", "Health & Wellness")
- At least **3 venues** with realistic names, addresses, and varying capacities (small: 50, medium: 200, large: 500)
- At least **6 events** across categories and venues:
  - 2 upcoming Published events with available capacity
  - 1 event that is SoldOut with waitlisted registrations
  - 1 event today or tomorrow (for check-in testing)
  - 1 Completed event (past)
  - 1 Draft event
- At least **3 ticket types per event** (e.g., "General Admission" at $25, "VIP" at $75, "Student" at $10 — some with early-bird pricing)
- At least **10 attendees** with realistic names, emails, and organizations
- At least **20 registrations** in various states (Confirmed, Waitlisted, Cancelled, CheckedIn)
- At least **5 check-ins** for the completed and today's events
- Ensure some events have early-bird deadlines that have passed (so both pricing tiers are exercised)
- Ensure capacity and ticket counts are internally consistent

Use the EF Core seeding mechanism or a data seeder service. Ensure the seed data only runs when the database is empty to avoid duplicates.

## Cross-Cutting Concerns

### Error Handling
- Global exception handler middleware that redirects to a custom error page (`/Error`) for unhandled exceptions
- Business rule violations displayed as TempData error messages or inline validation errors on forms
- Use the **Post-Redirect-Get (PRG)** pattern for all form submissions to prevent duplicate submissions on refresh

### Validation
- Use Data Annotations on all page model input properties
- Display validation errors inline using `asp-validation-for` tag helpers
- Include a validation summary at the top of forms using `asp-validation-summary`
- Enable client-side validation with jQuery Validation Unobtrusive

### Logging
- Use the built-in `ILogger` throughout the application
- Log key operations at **Information** level: registration created, event cancelled, check-in processed, waitlist promotion
- Log errors at **Error** level

### Layout & Navigation
- Use a shared `_Layout.cshtml` with Bootstrap 5 navbar
- Navigation links: Dashboard, Events, Attendees, Venues, Categories
- Use Bootstrap badges for status indicators (e.g., green for Published, red for Cancelled, yellow for SoldOut)
- Use Bootstrap alerts for TempData success/error messages
- Include a footer with the app name

### Pagination
- Use consistent pagination on all list pages
- Accept `pageNumber` and `pageSize` query parameters with sensible defaults (page 1, 10 items per page)
- Render Bootstrap pagination controls at the bottom of tables
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
- Use named handler methods (`OnPostApproveAsync`, `OnPostCancelAsync`) when a page has multiple form actions instead of a single `OnPost` with branching logic
- Use `asp-page-handler` tag helper to route to named handlers

### Reusable UI Components
- Create at least one reusable partial view or View Component for UI patterns that repeat across pages (e.g., a status badge component that takes a status value and renders the correct Bootstrap badge color, or a reusable pagination control)
- Avoid duplicating the same HTML patterns across multiple pages

## Project Location

Create the project at: `./src/SparkEvents/`

The project should be a standalone ASP.NET Core Razor Pages project with no dependencies on other projects in this repository. It should be fully self-contained and runnable with `dotnet run`.
