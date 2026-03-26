# Keystone Properties — Generation Notes

## Skills Used

The following dotnet-artisan skills were invoked during code generation:

### 1. `using-dotnet` (Process Gateway)
- **Role**: Detected .NET intent and enforced routing discipline before any code generation.
- **Influence**: Ensured the routing sequence was followed (detect → advisor → baseline → domain skills) before writing any code. Established the "simplicity first" principle (KISS) that guided architectural decisions — no unnecessary abstractions, no MediatR/CQRS for simple CRUD.

### 2. `dotnet-advisor` (Router)
- **Role**: Routed the request to the correct domain skills based on project signals (ASP.NET Core Razor Pages + EF Core + UI).
- **Influence**: Determined that `dotnet-csharp`, `dotnet-api`, and `dotnet-ui` skills were all needed for this project. Established the loading order: baseline C# standards first, then API/data patterns, then UI patterns.

### 3. `dotnet-csharp` (Baseline — Always Loaded)
- **Role**: Provided C# coding standards, async/await correctness, code smell detection, and .NET 10 version-specific guidance.
- **Influence on generated code**:
  - **Coding standards** (`references/coding-standards.md`): PascalCase naming, file-scoped namespaces, `sealed` classes, explicit access modifiers, `var` where type is obvious, `is not null` pattern.
  - **Async patterns**: All service methods are async with proper `await` chains. No `.Result` or `.Wait()` blocking calls.
  - **Code smells** (`references/code-smells.md`): No empty catch blocks, no swallowed exceptions, proper `using` on disposables, no premature `.ToList()` in LINQ chains.
  - **.NET releases** (`references/dotnet-releases.md`): Targeted `net10.0` TFM. Used primary constructors (C# 12+), collection expressions (`[]`), file-scoped namespaces.
  - **SOLID principles**: Services behind interfaces (DI-friendly), single responsibility per service class, guard clauses for early return.

### 4. `dotnet-api` (ASP.NET Core + EF Core)
- **Role**: Provided EF Core patterns, data access strategy, and agent gotcha avoidance.
- **Influence on generated code**:
  - **EF Core patterns** (`references/efcore-patterns.md`):
    - `DbContext` registered as scoped (one per request).
    - `AsNoTracking()` on all read-only queries for performance.
    - `AsSplitQuery()` used for queries with multiple collection `Include()` calls to avoid Cartesian explosion.
    - Proper relationship configuration with `OnDelete(DeleteBehavior.Restrict)`.
    - Timestamps set via `SaveChanges` override rather than interceptors (simpler for this app's scale).
  - **Agent gotchas** (`references/agent-gotchas.md`):
    - Used correct `Microsoft.EntityFrameworkCore.Sqlite` package name (not the common misspelling).
    - Used `Microsoft.NET.Sdk.Web` SDK (not `Microsoft.NET.Sdk`).
    - Connection string read from configuration, not hardcoded.
    - No shared-framework packages added as explicit `PackageReference`.
    - Service lifetimes match (all scoped — no captive dependency issues).

### 5. `dotnet-ui` (UI Patterns)
- **Role**: Provided guidance on Razor Pages patterns, accessibility, and component architecture.
- **Influence on generated code**:
  - **Accessibility**: `aria-label` on pagination nav, `aria-current="page"` on active nav items, `role="alert"` on TempData flash messages, semantic HTML (`<nav>`, `<main>`, `<section>`, `<table>` with `<thead>`/`<tbody>`).
  - **Reusable components**: `_StatusBadgePartial` (color-coded badges for 7 status types) and `_PaginationPartial` (consistent pagination across all list pages).
  - **Form patterns**: Nested `InputModel` classes with `[BindProperty]`, `asp-validation-for` tag helpers, `asp-validation-summary`, client-side validation via `_ValidationScriptsPartial`.
  - **PRG pattern**: All form submissions use Post-Redirect-Get to prevent duplicate submissions.

## Architecture Decisions Influenced by Skills

| Decision | Rationale | Skill Source |
|----------|-----------|-------------|
| No repository pattern | `DbContext` is the unit of work, `DbSet<T>` is the repository. Framework provides enough. | `using-dotnet` KISS principle |
| Interface + implementation per service | Spec requires it; DI-friendly for future testing | `dotnet-csharp` SOLID |
| `sealed` on all classes | Performance benefit, intent clarity — no inheritance expected | `dotnet-csharp` coding standards |
| Primary constructors on services & page models | Modern C# 12+ pattern, reduces boilerplate | `dotnet-csharp` .NET releases |
| `AsNoTracking()` on reads | Reduces memory/CPU overhead for read-only queries | `dotnet-api` EF Core patterns |
| `AsSplitQuery()` on multi-include queries | Prevents Cartesian explosion | `dotnet-api` EF Core patterns |
| Collection expressions `[]` | Modern C# 12+ syntax for empty collections | `dotnet-csharp` .NET releases |
| Bootstrap 5 CDN | Simpler than bundled, matches spec requirement | `dotnet-ui` |
| TempData for flash messages | PRG-compatible state transfer, spec requirement | `dotnet-api` architecture patterns |

## File Summary

| Directory | File Count | Purpose |
|-----------|-----------|---------|
| `Models/` | 8 | Entity classes and enums |
| `Data/` | 2 | DbContext and data seeder |
| `Services/` | 15 | Business logic (7 interfaces + 7 implementations + PaginatedList) |
| `Pages/` | 40 | Razor Pages (.cshtml + .cshtml.cs) organized by feature |
| Root | 3 | Program.cs, appsettings.json, .csproj |
