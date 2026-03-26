# HorizonHR — Generation Notes

## Skills Used

### 1. `dotnet` (Router Skill)
- **Role**: Primary classification skill that detected the project as an ASP.NET Core Razor Pages web app and routed to the narrowest matching specialized skills.
- **Influence**: Directed the workflow to invoke `dotnet-aspnet-core`, `dotnet-entity-framework-core`, `dotnet-project-setup`, and `dotnet-modern-csharp` in sequence rather than using generic .NET guidance.

### 2. `dotnet-aspnet-core`
- **Role**: Guided ASP.NET Core hosting, middleware, configuration, and Razor Pages patterns.
- **Influence on generated code**:
  - **Middleware ordering** in `Program.cs` follows the recommended pipeline: `UseExceptionHandler` → `UseStaticFiles` → `UseRouting` → endpoints.
  - **Global error handling** via `app.UseExceptionHandler("/Error")` for all environments, not just production.
  - **Service registration** uses scoped lifetime for all services (`AddScoped`), aligning with the DbContext unit-of-work pattern.
  - **Configuration** uses `IConfiguration` with `appsettings.json` connection strings and `ILogger<T>` for structured logging.
  - **Post-Redirect-Get (PRG)** pattern enforced on all form submissions with `TempData` flash messages.
  - **Anti-patterns avoided**: No `new HttpClient()`, no sync-over-async, no `async void`, secrets not committed.

### 3. `dotnet-entity-framework-core`
- **Role**: Guided data access patterns, DbContext design, query optimization, and relationship modeling.
- **Influence on generated code**:
  - **Fluent API configuration** in `ApplicationDbContext.OnModelCreating()` with `IEntityTypeConfiguration`-style inline configuration for all entities, unique indexes, and relationship cascades.
  - **`AsNoTracking()`** used on all read-only queries (list pages, search) to avoid unnecessary change tracking overhead.
  - **`AsSplitQuery()`** used on the Employee detail query to avoid cartesian explosion when loading multiple collection navigations.
  - **Scoped DbContext lifetime** via `AddDbContext<T>()` — aligned with unit-of-work per request.
  - **Decimal column types** explicitly specified (`decimal(18,2)`, `decimal(5,1)`) for monetary and fractional values.
  - **Composite unique indexes** for `(EmployeeId, LeaveTypeId, Year)` and `(EmployeeId, SkillId)`.
  - **Delete behaviors** carefully chosen: `Restrict` for required relationships, `SetNull` for optional manager references, `Cascade` for owned collections.
  - **Automatic timestamp management** via `SaveChangesAsync` override — no manual timestamp assignment in business code.
  - **EnsureCreated + seeder** for development database initialization (appropriate for SQLite; migrations would be used for production SQL Server).

### 4. `dotnet-project-setup`
- **Role**: Guided project structure, SDK choice, and separation of concerns.
- **Influence on generated code**:
  - **`dotnet new webapp`** template used as the starting point for ASP.NET Core Razor Pages with .NET 10.
  - **Clean folder structure**: `Models/`, `Models/Enums/`, `Data/`, `Services/`, `Services/Interfaces/`, `Pages/` organized by feature area.
  - **Interface + implementation pattern** for all services, registered via DI in `Program.cs`.
  - **Single project** structure appropriate for a self-contained HR portal (no unnecessary multi-project overhead).
  - **Target framework**: `net10.0` with nullable reference types and implicit usings enabled.

### 5. `dotnet-modern-csharp`
- **Role**: Ensured modern C# 14 language features are used correctly for .NET 10.
- **Influence on generated code**:
  - **Primary constructors** used throughout (services, page models) — e.g., `public class DepartmentService(ApplicationDbContext db, ILogger<DepartmentService> logger)`.
  - **Collection expressions** (`[]`) for initializing empty collections instead of `new List<T>()`.
  - **File-scoped namespaces** used consistently.
  - **Target-typed `new()`** for object initialization.
  - **Pattern matching** with `is not null`, `is null`, enum pattern matching in Razor `switch` expressions.
  - **`required` keyword not used** on entity properties to maintain EF Core compatibility (EF Core populates via materializer, not constructors).
  - **String interpolation** preferred over concatenation.
  - **`var` keyword** used consistently for local variable declarations where the type is obvious.

## Architecture Decisions

| Decision | Rationale |
|----------|-----------|
| Razor Pages (not MVC Controllers) | Spec requires Razor Pages; page-per-feature model fits CRUD-heavy HR workflows |
| SQLite with EnsureCreated | Spec requires SQLite; no migrations needed for demo/testing seeded data |
| Service layer with interfaces | Separation of concerns; testability; business rule isolation from page models |
| Dedicated InputModel classes | Spec requires `[BindProperty]` on nested input models, not entity binding |
| Reusable `_StatusBadge` partial | Color-coded Bootstrap badges for all status/rating/proficiency displays across the app |
| Reusable `_Pagination` partial | Consistent pagination UI extracted into a single partial view |
| Data seeder as static class | Runs once on startup when DB is empty; creates realistic interconnected demo data |

## File Summary

- **8 entity models** + **6 enums** in `Models/`
- **1 DbContext** with full Fluent API configuration + automatic timestamps in `Data/`
- **1 data seeder** with 13 employees, 5 departments, 4 leave types, 12 skills, and interconnected data in `Data/`
- **6 service interfaces** + **6 service implementations** in `Services/`
- **30+ Razor Pages** (`.cshtml` + `.cshtml.cs`) across 6 feature areas in `Pages/`
- **3 shared partial views** (`_Layout`, `_StatusBadge`, `_Pagination`) in `Pages/Shared/`
