# Keystone Properties — Generation Notes

## Skills Used

### `dotnet-webapi` Skill

The **dotnet-webapi** skill was loaded and its guidance was adapted for this Razor Pages application. While the skill is primarily designed for Web API endpoints, many of its architectural patterns and best practices apply equally to server-rendered Razor Pages apps. Here's how the skill influenced the generated code:

#### Patterns Applied from the Skill

1. **Sealed Types by Default (CA1852)**
   All classes — entities, services, page models, middleware — are marked `sealed` unless designed for inheritance. This follows the skill's guidance to "seal all types by default" for design clarity and JIT devirtualization.

2. **Service Layer with Interfaces (Dependency Inversion)**
   Every service has a corresponding interface (`IPropertyService` / `PropertyService`, etc.) registered via `AddScoped<IService, Service>()` in DI. The skill explicitly requires: "Define an interface for every service — this enables unit testing with mocks."

3. **EF Core Fluent API Configuration**
   Following the skill's Step 6 guidance:
   - Enums stored as strings: `.HasConversion<string>()`
   - Decimal column types specified: `.HasColumnType("decimal(10,2)")`
   - Unique indexes on natural keys: `.HasIndex(...).IsUnique()`
   - Explicit cascade/restrict delete behaviors on foreign keys
   - `AsNoTracking()` on all read-only queries

4. **Error Handling Middleware**
   The skill's Step 5 guidance on global exception handling was adapted. Instead of `IExceptionHandler` (API-focused), a `GlobalExceptionHandlerMiddleware` was placed in the `Middleware/` folder as the skill requires, redirecting to a custom `/Error` page appropriate for Razor Pages.

5. **Pagination Pattern**
   The skill's paginated response pattern was adapted into `PaginatedList<T>` with `IReadOnlyList<T>` items (per the skill's preference for immutable collections), reused across all list pages with a shared `_Pagination.cshtml` partial.

6. **Primary Constructor Injection**
   Services and page models use primary constructors for DI injection, following the skill's modern C# patterns (e.g., `public sealed class PropertyService(ApplicationDbContext db, ILogger<PropertyService> logger)`).

7. **CancellationToken Propagation**
   Every service method and page handler accepts and forwards `CancellationToken`, following the skill's requirement to "accept CancellationToken in every endpoint signature and forward it through all async calls."

#### Deviations from Skill (Razor Pages vs Web API)

- **No DTOs/Request-Response records**: The skill mandates separate `sealed record` DTOs for API input/output. For Razor Pages, entity models are used directly in views with dedicated `InputModel` classes for form binding — the standard Razor Pages pattern.
- **No OpenAPI/Swagger**: Not applicable to a server-rendered app.
- **No `.http` test file**: Not applicable; pages are tested via browser.
- **`EnsureCreated()` used instead of migrations**: For simplicity in this self-contained demo app with seed data, `EnsureCreated()` is used. The skill recommends migrations for production projects.
- **Data seeding via `DataSeeder` class**: The skill recommends `HasData()` in Fluent API. A runtime seeder was used instead to handle complex relational seed data more cleanly.

## Architecture Summary

```
KeystoneProperties/
├── Models/          — Entity classes and enums (all sealed)
├── Data/            — DbContext with Fluent API config + DataSeeder
├── Services/        — Interface + sealed implementation pairs
├── Middleware/       — Global exception handler
├── Pages/
│   ├── Shared/      — Layout, pagination partial, status badge partial
│   ├── Properties/  — CRUD + Deactivate (5 pages)
│   ├── Units/       — CRUD (4 pages)
│   ├── Tenants/     — CRUD + Deactivate (5 pages)
│   ├── Leases/      — CRUD + Terminate + Renew (6 pages)
│   ├── Payments/    — List + Details + Create + Overdue (4 pages)
│   ├── Maintenance/ — List + Details + Create + UpdateStatus (4 pages)
│   └── Inspections/ — List + Details + Create + Complete (4 pages)
└── wwwroot/         — Static assets (CSS)
```
