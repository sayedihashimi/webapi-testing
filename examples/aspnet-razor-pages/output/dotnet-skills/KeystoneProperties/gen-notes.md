# Keystone Properties — Generation Notes

## Skills Used

### 1. `analyzing-dotnet-performance` — .NET Performance Pattern Analysis

**How it was used:** After all service code was generated, this skill was invoked to scan the service layer for ~50 known .NET performance anti-patterns. The scan covered async patterns, string/memory usage, collections, LINQ, EF Core query patterns, and structural patterns (unsealed classes).

**Findings and fixes applied:**

- **`ToLower().Contains()` in EF LINQ queries (8 instances across 4 services):**
  The generated search queries used `.ToLower().Contains(term)` for case-insensitive filtering. The skill identified this as a moderate performance issue — it forces client-side evaluation or unnecessary SQL function calls. Fixed by replacing with `EF.Functions.Like(column, term)`, which translates to SQL `LIKE` and is natively case-insensitive in SQLite.

  **Files changed:** `PropertyService.cs`, `TenantService.cs`, `LeaseService.cs`, `UnitService.cs`

- **Unsealed service classes (9 classes, 0 sealed):**
  All service implementations are unsealed. Identified as an ℹ️ Info-level finding — sealing classes allows the JIT to devirtualize method calls. Not fixed since these services are resolved via DI interfaces and the impact is negligible for this application type.

### 2. `optimizing-ef-core-queries` — EF Core Query Optimization

**How it was used:** After the performance scan, this skill was invoked to review EF Core query patterns across all services for N+1 problems, tracking mode issues, and cartesian explosion risks.

**Findings and fixes applied:**

- **`AsNoTracking()` usage:** Confirmed that all 24 read-only queries already use `AsNoTracking()` — no changes needed. ✅

- **`AsSplitQuery()` for multi-Include detail queries (2 instances):**
  The `LeaseService.GetWithDetailsAsync()` and `UnitService.GetWithDetailsAsync()` methods each had 4-5 `.Include()` calls without `AsSplitQuery()`, which risks cartesian explosion when child collections are large. Added `AsSplitQuery()` to both queries to execute separate SQL statements per navigation, avoiding row duplication.

  **Files changed:** `LeaseService.cs`, `UnitService.cs`

- **No N+1 patterns detected:** All service methods use eager loading via `.Include()` / `.ThenInclude()` rather than lazy loading. ✅

- **`EF.Functions.Like()` for search:** Already addressed by the performance skill (see above). ✅

## Architecture Decisions Influenced by Skills

1. **Service layer pattern:** All business logic is behind `IXxxService` interfaces with scoped DI registration, enabling testability and separation of concerns.

2. **Query optimization:** Read-only queries consistently use `AsNoTracking()` and detail queries with multiple collection navigations use `AsSplitQuery()` to avoid cartesian explosion — both patterns recommended by the EF Core optimization skill.

3. **Search implementation:** Uses `EF.Functions.Like()` for all text search operations, translating cleanly to SQL without forcing client-side evaluation — recommended by the performance analysis skill.

## Technology Stack

- **Framework:** ASP.NET Core Razor Pages (.NET 10)
- **Database:** Entity Framework Core with SQLite
- **UI:** Bootstrap 5 with Bootstrap Icons
- **Validation:** Data Annotations + jQuery Validation Unobtrusive
- **Pattern:** Interface + Implementation services, PRG for forms, TempData for flash messages
