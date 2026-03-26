# KeystoneProperties — Generation Notes

## Skills Used

Four Copilot skills were invoked during the generation of this application. Each provided domain-specific patterns and anti-patterns that directly shaped the code.

### 1. `dotnet-aspnet-core`

**How it influenced the code:**

- **Middleware ordering** — `Program.cs` follows the prescribed middleware order: `UseExceptionHandler` → `UseStaticFiles` → `UseRouting` → `UseAuthorization` → `MapRazorPages`.
- **Error handling** — Global exception handling via `UseExceptionHandler("/Error")` with a custom Error page, per the skill's error handling patterns.
- **Configuration** — Connection string configured in `appsettings.json` and accessed via `builder.Configuration.GetConnectionString()`, following the skill's configuration patterns.
- **DI registration** — All services registered as `AddScoped<>` in `Program.cs`, aligning with the scoped-lifetime best practice for request-bound DbContext usage.
- **Logging** — `ILogger<T>` injected into all services via primary constructors; key business operations logged at Information level.
- **Anti-patterns avoided** — No sync-over-async calls; all handlers use `async Task`; no `async void`.

### 2. `dotnet-entity-framework-core`

**How it influenced the code:**

- **DbContext lifetime** — Registered as scoped via `AddDbContext<>`, matching the unit-of-work pattern recommended by the skill.
- **AsNoTracking** — All read-only queries in services use `.AsNoTracking()` to reduce memory overhead, per the skill's query tracking strategy guidance.
- **Fluent API configuration** — Entity relationships, indexes (unique constraint on `Unit(PropertyId, UnitNumber)`, `Tenant.Email`), decimal precision, and FK delete behaviors all configured in `OnModelCreating` using Fluent API rather than data annotations for complex constraints.
- **Pagination pattern** — `PaginatedList<T>` implements offset-based pagination with `Skip/Take`, following the skill's pagination patterns.
- **Eager loading** — Service queries use `.Include()` and `.ThenInclude()` to avoid N+1 queries, following the skill's loading strategy guidance.
- **Anti-patterns avoided** — No lazy loading, no `ToList()` before filtering, no generic repository wrapper, `SaveChanges` not called in loops.

### 3. `dotnet-project-setup`

**How it influenced the code:**

- **Project structure** — Clean separation of concerns with `Models/`, `Data/`, `Services/`, and `Pages/` directories, each with explicit responsibility boundaries.
- **Service layer pattern** — Interface + implementation pattern (`IPropertyService`/`PropertyService`) for all business logic, enabling testability and clean DI.
- **Project conventions** — Single `webapp` project with `Microsoft.NET.Sdk.Web`, targeting `net10.0` with nullable enabled and implicit usings.
- **NuGet packages** — Only essential packages added: `Microsoft.EntityFrameworkCore.Sqlite` and `Microsoft.EntityFrameworkCore.Design`.

### 4. `dotnet-modern-csharp`

**How it influenced the code:**

- **Primary constructors** — All service classes and page models use C# 12+ primary constructors for dependency injection (e.g., `public class PropertyService(ApplicationDbContext context, ILogger<PropertyService> logger) : IPropertyService`).
- **File-scoped namespaces** — Every `.cs` file uses file-scoped namespace declarations to reduce nesting.
- **Collection expressions** — Empty collections initialized with `[]` syntax (C# 12) instead of `new List<T>()`.
- **Pattern matching** — `is not null`, `is null`, and `switch` expressions used throughout for cleaner control flow.
- **Target-typed new** — `new()` used where the type is clear from context.
- **Nullable reference types** — Properly annotated nullable properties (`string?`, `int?`, `Lease?`) with `= null!` for required navigations and `= default!` for properties set during initialization.

## Architecture Summary

| Layer | Contents | Key Patterns |
|-------|----------|-------------|
| **Models** | 7 entities, 12 enums | Data Annotations, navigation properties, computed properties |
| **Data** | `ApplicationDbContext`, `DataSeeder` | Fluent API, auto-timestamps, idempotent seeding |
| **Services** | 8 service interfaces + implementations | Primary constructors, AsNoTracking reads, business rule enforcement, structured logging |
| **Pages** | 30+ Razor Pages across 7 feature areas | PRG pattern, InputModel binding, TempData flash messages, ViewComponents |
| **Shared** | Layout, StatusBadge ViewComponent, Pagination partial | Bootstrap 5, reusable UI components, semantic HTML, accessibility attributes |

## Business Rules Implemented

- No overlapping leases validation
- Unit status sync with lease lifecycle
- Lease status workflow enforcement (Pending → Active → Expired/Renewed/Terminated)
- Late fee auto-calculation ($50 base + $5/day, capped at $200)
- Lease renewal chain tracking
- Maintenance request workflow with status transitions
- Emergency maintenance handling (auto-assign, unit status change)
- Tenant deactivation guard (active leases check)
- Property deactivation guard (active leases check)
- Deposit tracking and return payment generation
