# SparkEvents — Generation Notes

## Summary

A fully functional ASP.NET Core Razor Pages community event registration portal built on .NET 10 with EF Core SQLite.

## What Was Generated

### Project Structure (`src/SparkEvents/`)

- **Models/** — 9 files: Entity classes (EventCategory, Venue, Event, TicketType, Attendee, Registration, CheckIn) and enums (EventStatus, RegistrationStatus)
- **Data/** — 2 files: `SparkEventsDbContext` with full relationship configuration and `DataSeeder` with realistic demo data
- **Services/** — 15 files: 7 service interfaces + implementations covering events, registrations, check-in, categories, venues, attendees, ticket types, plus `PaginatedList<T>` helper
- **Pages/** — 40+ Razor Pages organized by feature area:
  - **Dashboard** (`/`) — Stats cards, today's events with check-in progress bars, upcoming events, recent registrations
  - **Categories** — Full CRUD (list, create, edit, delete with dependency check)
  - **Venues** — Full CRUD with details showing upcoming events
  - **Events** — List with search/filter/pagination, details with capacity bar, create/edit with venue capacity validation, cancel with cascade, complete with end-date guard, publish from draft
  - **Ticket Types** — Manage per-event ticket types (create, edit, activate/deactivate)
  - **Registrations** — Register for events (attendee + ticket selection, early-bird pricing), details, cancel with 24-hour policy
  - **Roster/Waitlist** — Event roster with search and check-in status, waitlist view
  - **Check-In** — Dashboard with stats, search by confirmation # or name, process check-in with staff name
- **Shared/** — Layout with Bootstrap 5 navbar, status badge partials (event + registration), reusable pagination partial

### Key Features Implemented

- Capacity management with automatic SoldOut status transitions
- Waitlist with automatic promotion on cancellation
- Early-bird pricing with deadline enforcement
- Duplicate registration prevention
- 24-hour cancellation policy
- Event cancellation cascade (auto-cancels all registrations)
- Event status workflow (Draft → Published → SoldOut → Completed/Cancelled)
- Venue capacity constraint validation
- Confirmation number format: SPK-YYYYMMDD-NNNN
- Check-in window enforcement (1 hour before start to end)
- TempData flash messages for success/error notifications
- Post-Redirect-Get (PRG) pattern on all forms
- InputModel pattern with Data Annotations validation
- Comprehensive seed data (4 categories, 3 venues, 6 events, 12 attendees, 25+ registrations, 5 check-ins)

### Tech Stack

- ASP.NET Core Razor Pages (.NET 10)
- Entity Framework Core with SQLite
- Bootstrap 5 (CDN)
- jQuery Validation Unobtrusive (client-side validation)
