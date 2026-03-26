# SparkEvents — Generation Notes

## Skills Used

### 1. `optimizing-ef-core-queries`

**Purpose:** Optimize Entity Framework Core queries by fixing N+1 problems, choosing correct tracking modes, using split queries, and avoiding common performance traps.

**How it influenced the code:**

- **`AsNoTracking()` added to all read-only queries.** The skill's guidance on disabling change tracking for read-only queries was applied across all service methods that only retrieve data for display. This reduces EF Core's overhead since it doesn't need to create snapshot copies or track entity state for queries that never call `SaveChangesAsync()`. Affected services:
  - `EventService`: `GetEventsAsync`, `GetEventByIdAsync`, `GetUpcomingEventsAsync`, `GetTodaysEventsAsync`
  - `RegistrationService`: `GetRegistrationByIdAsync`, `GetEventRosterAsync`, `GetEventWaitlistAsync`, `GetRecentRegistrationsAsync`
  - `CheckInService`: `SearchForCheckInAsync`
  - `AttendeeService`: `GetAttendeeByIdAsync`
  - `VenueService`: `GetVenueByIdAsync`

- **`AsSplitQuery()` added to queries with multiple `Include()` chains.** Following the skill's guidance on avoiding Cartesian explosion when joining multiple navigation properties, split queries were applied to:
  - `GetEventByIdAsync` (Event + Category + Venue + TicketTypes)
  - `GetRegistrationByIdAsync` (Registration + Event + Attendee + TicketType + CheckIn)
  - `GetEventRosterAsync` (Registration + Attendee + TicketType + CheckIn)
  - `GetRecentRegistrationsAsync` (Registration + Event + Attendee + TicketType)
  - `SearchForCheckInAsync` (Registration + Attendee + TicketType + CheckIn)
  - `GetAttendeeByIdAsync` (Attendee + Registrations→Event + Registrations→TicketType)
  - `GetVenueByIdAsync` (Venue + Events→EventCategory)

- **`AnyAsync()` used instead of `Count()` for existence checks.** The skill highlights using `.Any()` over `.Count()` when checking for existence. The code already followed this pattern (e.g., checking for duplicate registrations with `AnyAsync`), which the skill validated as correct.

- **Eager loading via `Include()` used consistently** to prevent N+1 query patterns. All queries that need related data load it eagerly rather than relying on lazy loading.

### 2. `analyzing-dotnet-performance`

**Purpose:** Scan .NET code for ~50 performance anti-patterns across async, memory, strings, collections, LINQ, regex, serialization, and I/O.

**How it influenced the code:**

- **`.ToLower()` in LINQ queries analyzed and validated.** The scan detected `.ToLower()` calls in search queries across `AttendeeService`, `CheckInService`, `EventService`, and `RegistrationService`. Analysis confirmed these are inside EF Core LINQ expressions that translate to SQL `lower()` functions — they do **not** allocate .NET strings. This was correctly identified as a false positive for the EF Core context.

- **Unsealed classes identified as ℹ️ Info level.** All 15 classes (7 services + 8 models) were detected as unsealed. Since these are not on hot paths (services are DI-scoped, models are EF Core entities), and sealing EF Core entity classes can interfere with proxy generation, this was classified as informational only and no changes were made.

- **No critical anti-patterns found.** The scan confirmed no `params` allocations, no per-call `Dictionary`/`List` allocations in service methods, no `Regex` usage, and no `HttpClient` misuse.

## Architecture Decisions

- **Interface + Implementation pattern** for all services, enabling testability and DI
- **Dedicated `InputModel` classes** on all page models to avoid binding directly to entities
- **Post-Redirect-Get (PRG)** pattern for all form submissions
- **Reusable partial views** for status badges (`_EventStatusBadge`, `_RegistrationStatusBadge`) and pagination (`_Pagination`)
- **TempData** for flash messages (success/error) displayed via the shared layout
- **EF Core filtered navigation properties** used in `VenueService.GetVenueByIdAsync` to load only upcoming events
- **Database indexes** configured on frequently queried columns (Status, StartDate, Email, ConfirmationNumber)
