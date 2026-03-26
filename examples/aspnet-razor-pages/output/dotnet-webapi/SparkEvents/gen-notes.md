# SparkEvents — Generation Notes

## Skills Used

### `dotnet-webapi` Skill

The **dotnet-webapi** skill was invoked during project generation. While the specification called for a Razor Pages application (not a Web API), several principles from the skill directly influenced the generated code:

#### Influence on Generated Code

1. **Sealed types by default** — The skill mandates marking every new class as `sealed` unless designed for inheritance (CA1852). All model classes, service implementations, page models, and middleware classes in SparkEvents follow this pattern:
   - `public sealed class EventCategory`
   - `public sealed class RegistrationService : IRegistrationService`
   - `public sealed class IndexModel : PageModel`
   - `internal sealed class GlobalExceptionHandlerMiddleware`

2. **Service layer with interfaces** — The skill requires a service interface for every service, registered via DI with `AddScoped<IService, Service>()`. SparkEvents implements this throughout:
   - `IEventCategoryService` → `EventCategoryService`
   - `IVenueService` → `VenueService`
   - `IEventService` → `EventService`
   - `IAttendeeService` → `AttendeeService`
   - `ITicketTypeService` → `TicketTypeService`
   - `IRegistrationService` → `RegistrationService`
   - `ICheckInService` → `CheckInService`

3. **No direct DbContext injection into pages** — Following the skill's guidance to never inject `DbContext` directly into controllers/handlers, all data access flows through the service layer.

4. **AsNoTracking() on read queries** — The skill mandates `AsNoTracking()` on all read-only queries. All service methods that return data for display use this optimization.

5. **EF Core Fluent API configuration** — Following skill guidance:
   - Unique indexes on natural keys (`HasIndex(...).IsUnique()` on category names, attendee emails, confirmation numbers)
   - Explicit cascade/restrict delete behaviors on foreign keys
   - Enum stored as strings via `.HasConversion<string>()`
   - Decimal column types specified: `.HasColumnType("decimal(10,2)")`

6. **CancellationToken propagation** — The skill requires `CancellationToken` in every endpoint/handler and forwarded through all async calls. All page handlers accept `CancellationToken ct` and pass it to service methods.

7. **Error handling in Middleware/ folder** — The skill specifies exception handlers belong in a `Middleware/` folder. The `GlobalExceptionHandlerMiddleware` class follows this convention.

8. **Pagination pattern** — The skill's `PaginatedResponse<T>` pattern influenced the `PaginatedList<T>` implementation with `IReadOnlyList<T>` for immutability signaling.

9. **Input model separation** — While the skill recommends `sealed record` DTOs for APIs, the Razor Pages adaptation uses nested `InputModel` classes with `[BindProperty]` — the equivalent pattern for server-rendered forms that maintains the separation between entities and form binding.

#### Patterns Adapted for Razor Pages

Some skill guidance was adapted rather than applied directly since this is a Razor Pages app, not a Web API:

- **DTOs as sealed records** → Adapted to nested `InputModel` classes with data annotations (Razor Pages convention)
- **TypedResults/HTTP status codes** → Adapted to `IActionResult` returns (`Page()`, `RedirectToPage()`, `NotFound()`)
- **OpenAPI/Swagger** → Not applicable for server-rendered app
- **POST 201 Created** → Adapted to PRG (Post-Redirect-Get) pattern with `TempData` flash messages
- **.http test file** → Not applicable for Razor Pages

## Architecture Summary

```
src/SparkEvents/
├── Data/                    # DbContext and data seeder
├── Middleware/               # Global exception handling (per skill)
├── Models/                   # Entity classes + enums (all sealed)
├── Services/                 # Interface + implementation pairs (all sealed)
├── Pages/
│   ├── Shared/              # Layout, _StatusBadge, _Pagination partials
│   ├── Categories/          # CRUD pages
│   ├── Venues/              # CRUD + details pages
│   ├── Events/              # CRUD + registration, roster, waitlist, check-in
│   ├── Attendees/           # CRUD + details pages
│   └── Registrations/       # Details + cancellation pages
└── Program.cs               # DI registration, middleware pipeline, DB seed
```
