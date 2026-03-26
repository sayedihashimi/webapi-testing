# SparkEvents — Generation Notes

## Skills Used

### 1. `using-dotnet` (Process Gateway)
- **How it influenced the code:** Established the .NET routing discipline before any implementation. Detected .NET intent from the prompt (ASP.NET Core, Razor Pages, EF Core keywords) and enforced the skill loading sequence. Applied the "Simplicity First (KISS)" principle — the app uses `DbContext` directly as the unit of work without unnecessary repository abstractions, and services are kept lean with single implementations behind interfaces as requested by the spec.

### 2. `dotnet-advisor` (Router)
- **How it influenced the code:** Routed the request to the correct domain skills. Identified this as a "build me an app" request spanning API/data access and tooling domains, loading `dotnet-csharp` → `dotnet-tooling` → `dotnet-api` in sequence. Enforced .NET-first defaults (ASP.NET Core Razor Pages instead of a JavaScript framework).

### 3. `dotnet-csharp` (Baseline C# Patterns)
- **How it influenced the code:**
  - **Coding Standards** (`references/coding-standards.md`): PascalCase for types/methods/properties, `_camelCase` for private fields, file-scoped namespaces throughout, explicit access modifiers, `sealed` classes for page models and services.
  - **Async Patterns** (`references/async-patterns.md`): All database operations use `async`/`await` correctly. No blocking calls (`.Result`, `.Wait()`). `CancellationToken` not needed in Razor Pages handlers (framework handles request cancellation).
  - **Code Smells** (`references/code-smells.md`): No empty catch blocks — business rule violations throw `InvalidOperationException` which are caught and displayed via TempData. No `async void`. Proper exception re-throwing with `throw;` not `throw ex;`.
  - **.NET Releases** (`references/dotnet-releases.md`): Targeted `net10.0` with C# 14 features. Used primary constructors for DI injection in services and page models. Used collection expressions (`[]`) for empty collection initializers. Used pattern matching (`is not null`, `is not`) for null checks.

### 4. `dotnet-api` (ASP.NET Core & EF Core)
- **How it influenced the code:**
  - **EF Core Patterns** (`references/efcore-patterns.md`): `DbContext` registered as scoped (one per request) via `AddDbContext`. `AsNoTracking()` used for all read-only queries (list pages, details pages). Proper `Include()`/`ThenInclude()` for eager loading related entities. Entity relationships configured with `OnDelete` behaviors (Restrict for critical FKs, Cascade for owned entities). Enum-to-string conversion for `EventStatus` and `RegistrationStatus` columns.
  - **Agent Gotchas** (`references/agent-gotchas.md`): Avoided injecting `DbContext` into singletons — all services are scoped. Connection string read from configuration, not hardcoded. Used `EnsureCreatedAsync()` only for initial development (appropriate for this non-production demo app with seed data).

### 5. `dotnet-tooling` (Project Setup)
- **How it influenced the code:**
  - **Scaffold Project** (`references/scaffold-project.md`): Used `dotnet new webapp` template with `Microsoft.NET.Sdk.Web` SDK. Proper project structure with separation of concerns (Models, Data, Services, Pages). NuGet packages added with correct names (`Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design`).

## Architecture Decisions

| Decision | Rationale | Skill Influence |
|----------|-----------|-----------------|
| Interface + implementation for services | Spec requires it; enables testability | `using-dotnet` KISS principle — kept to 3 service interfaces, no over-abstraction |
| `DbContext` used directly in simple pages | Categories/Venues CRUD is simple enough; services reserved for business logic | `dotnet-csharp` SOLID principles — services own complex business rules |
| `sealed` on all page models and services | Performance (JIT devirtualization) and intent clarity | `dotnet-csharp` coding standards |
| Primary constructors for DI | Modern C# 14 pattern, reduces boilerplate | `dotnet-csharp` .NET releases reference |
| `PaginatedList<T>` with `IPaginatedList` interface | Enables reusable `_Pagination` partial across different entity types | `dotnet-csharp` type design |
| `AsNoTracking()` on all read queries | EF Core best practice for read-only pages | `dotnet-api` EF Core patterns |
| Reusable `_StatusBadge` and `_RegistrationStatusBadge` partials | DRY principle — status badges used across 10+ pages | `dotnet-csharp` SOLID/DRY principles |
| TempData for flash messages with PRG pattern | Spec requirement; prevents duplicate form submissions | `dotnet-api` middleware patterns |

## File Statistics

- **Total source files:** 80 (`.cs` + `.cshtml`)
- **Models:** 9 files (7 entities + 2 enums)
- **Data layer:** 2 files (DbContext + DataSeeder)
- **Services:** 5 files (3 implementations + 1 interface file + PaginatedList)
- **Pages:** 64 files (32 page pairs + shared partials + layout)
- **Seed data:** 4 categories, 3 venues, 6 events, 12 attendees, 20+ registrations, 6 check-ins

## Routes Verified

All 23 routes return HTTP 200:
- Dashboard: `/`
- Events: `/Events`, `/Events/Details/{id}`, `/Events/Create`, `/Events/Edit/{id}`, `/Events/Cancel/{id}`
- Registration: `/Events/{id}/Register`, `/Events/{id}/Roster`, `/Events/{id}/Waitlist`
- Ticket Types: `/Events/{id}/TicketTypes`
- Check-In: `/Events/{id}/CheckIn`
- Attendees: `/Attendees`, `/Attendees/Details/{id}`, `/Attendees/Create`, `/Attendees/Edit/{id}`
- Venues: `/Venues`, `/Venues/Details/{id}`, `/Venues/Create`, `/Venues/Edit/{id}`
- Categories: `/Categories`, `/Categories/Create`
- Registrations: `/Registrations/Details/{id}`, `/Registrations/Cancel/{id}`
