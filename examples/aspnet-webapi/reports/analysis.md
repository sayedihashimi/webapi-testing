# Comparative Analysis: dotnet-skills, managedcode-dotnet-skills, dotnet-artisan, dotnet-webapi, no-skills

## Introduction

This analysis compares five Copilot skill configurations used to generate three ASP.NET Core Web API applications targeting .NET 10. Each configuration produced identical scenarios:

| Configuration | Label | Description |
|---|---|---|
| **no-skills** | Baseline (default Copilot) | No custom skills or plugins |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | 11 official .NET plugins (dotnet, dotnet-ai, dotnet-data, etc.) |
| **managedcode-dotnet-skills** | Community managed-code skills | Community-contributed .NET skills |
| **dotnet-artisan** | dotnet-artisan plugin chain | 9 skills + 14 specialist agents for .NET development |
| **dotnet-webapi** | dotnet-webapi skill | Focused Web API skill |

**Scenarios evaluated:**
- **FitnessStudioApi** — Booking/membership system with class scheduling, waitlists, and instructor management
- **LibraryApi** — Book loans, reservations, overdue fines, and availability tracking
- **VetClinicApi** — Pet healthcare with appointments, vaccinations, and medical records

All scores are averaged across the three scenarios per configuration unless noted otherwise.

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-skills | managedcode | dotnet-artisan | dotnet-webapi |
|---|---|---|---|---|---|
| API Architecture [CRITICAL] | 2 | 3 | 2 | 4 | 5 |
| Sealed Types [MEDIUM] | 1 | 3 | 1 | 5 | 5 |
| Modern C# Adoption [LOW] | 3 | 3 | 4 | 5 | 5 |
| DTO Design [HIGH] | 3 | 2 | 4 | 5 | 5 |
| Service Abstraction [HIGH] | 4 | 4 | 4 | 4 | 4 |
| CancellationToken Propagation [HIGH] | 1 | 1 | 4 | 4 | 5 |
| AsNoTracking Usage [MEDIUM] | 1 | 3 | 4 | 4 | 5 |
| Return Type Precision [MEDIUM] | 2 | 2 | 4 | 4 | 5 |
| Data Seeder Design [MEDIUM] | 3 | 3 | 3 | 3 | 4 |
| Middleware Style [HIGH] | 2 | 3 | 4 | 4 | 5 |
| Exception Handling Strategy [HIGH] | 4 | 3 | 4 | 3 | 3 |
| File Organization [MEDIUM] | 4 | 4 | 4 | 5 | 5 |
| Pagination [MEDIUM] | 3 | 3 | 4 | 4 | 5 |
| OpenAPI Metadata [MEDIUM] | 3 | 3 | 3 | 4 | 5 |
| Structured Logging [MEDIUM] | 4 | 3 | 4 | 3 | 4 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Package Discipline [MEDIUM] | 3 | 3 | 3 | 3 | 4 |
| EF Core Relationships [HIGH] | 4 | 4 | 4 | 4 | 4 |
| TypedResults Usage [MEDIUM] | 1 | 1 | 1 | 4 | 5 |
| HTTP Test File Quality [MEDIUM] | 3 | 3 | 3 | 3 | 4 |
| Code Standards [LOW] | 4 | 4 | 4 | 4 | 4 |
| Enum Design [MEDIUM] | 4 | 3 | 4 | 4 | 4 |
| Guard Clauses [MEDIUM] | 3 | 3 | 3 | 3 | 3 |
| Async/Await Practices [HIGH] | 4 | 4 | 4 | 4 | 4 |
| Dispose & Resource Mgmt [MEDIUM] | 3 | 3 | 3 | 3 | 3 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 4 |
| HSTS & HTTPS Redirection [HIGH] | 2 | 1 | 1 | 1 | 1 |
| NuGet Version Pinning [CRITICAL] | 2 | 2 | 2 | 2 | 2 |
| FluentValidation Usage [MEDIUM] | 1 | 2 | 2 | 1 | 1 |
| Built-in OpenAPI over Swashbuckle [CRITICAL] | 2 | 3 | 2 | 3 | 4 |
| Build & Run Success [CRITICAL] | 4 | 4 | 4 | 4 | 4 |
| Input Validation Coverage [CRITICAL] | 3 | 4 | 4 | 4 | 4 |
| Endpoint Completeness [CRITICAL] | 4 | 4 | 4 | 4 | 4 |
| Business Rule Implementation [CRITICAL] | 4 | 4 | 4 | 4 | 4 |
| Error Response Conformance [HIGH] | 3 | 4 | 4 | 4 | 4 |
| Security Vulnerability Scan [CRITICAL] | 3 | 3 | 3 | 3 | 3 |

---

## 1. API Architecture [CRITICAL]

All three specs request modern ASP.NET Core Web API. The critical question is whether generated code uses **minimal APIs with MapGroup** (modern, recommended) or **traditional controllers**.

**no-skills:** All three projects use traditional `[ApiController]` controllers (21 `[ApiController]` attributes, 0 `MapGroup` calls). Program.cs simply calls `app.MapControllers()`.

```csharp
// no-skills: Controllers/MembersController.cs
[ApiController]
[Route("api/members")]
public class MembersController(IMemberService service) : ControllerBase
```

**dotnet-skills:** Mixed — FitnessStudioApi and LibraryApi use controllers (14 `[ApiController]`), but VetClinicApi uses minimal APIs with `MapGroup` (7 calls). Inconsistent across projects.

**managedcode-dotnet-skills:** All three projects use controllers exclusively (21 `[ApiController]`, 0 `MapGroup`). No minimal API usage.

**dotnet-artisan:** Mostly minimal APIs with MapGroup (14 `MapGroup` calls), but LibraryApi uses controllers for some resources (7 `[ApiController]`). Clean endpoint extension methods.

```csharp
// dotnet-artisan: Endpoints/MemberEndpoints.cs
public static void MapMemberEndpoints(this WebApplication app)
{
    var group = app.MapGroup("/api/members").WithTags("Members");
    group.MapGet("/", async (...) => TypedResults.Ok(await service.GetAllAsync(...)));
}
```

**dotnet-webapi:** All three projects consistently use minimal APIs with MapGroup (21 `MapGroup` calls, 0 controllers). Clean endpoint extension methods keep Program.cs lean. Global usings file present.

```csharp
// dotnet-webapi: Program.cs
app.MapMemberEndpoints();
app.MapBookingEndpoints();

// dotnet-webapi: Endpoints/MemberEndpoints.cs
public static void MapMemberEndpoints(this WebApplication app)
{
    var group = app.MapGroup("/api/members").WithTags("Members");
    group.MapGet("/", async (..., CancellationToken ct) => TypedResults.Ok(await service.GetAllAsync(..., ct)))
        .WithName("GetMembers")
        .WithSummary("List members")
        .Produces<PaginatedResponse<MemberResponse>>();
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Controllers only — functional but outdated pattern |
| dotnet-skills | 3 | Mixed: 2/3 controllers, 1/3 minimal APIs |
| managedcode | 2 | Controllers only — no modern API patterns |
| dotnet-artisan | 4 | Mostly minimal APIs with MapGroup and endpoint extensions |
| dotnet-webapi | 5 | 100% minimal APIs with MapGroup, rich metadata, and endpoint extensions |

**Verdict:** dotnet-webapi is the clear winner — fully minimal APIs with organized route groups, endpoint extensions, and rich OpenAPI metadata per endpoint.

---

## 2. Sealed Types [MEDIUM]

Sealed types enable JIT devirtualization and communicate "not designed for inheritance."

| Config | Sealed Count | Total Types | Sealed % |
|---|---|---|---|
| no-skills | 0 | 159 | 0% |
| dotnet-skills | 68 | 162 | 42% |
| managedcode | 0 | 160 | 0% |
| dotnet-artisan | 132 | 135 | 98% |
| dotnet-webapi | 126 | 129 | 98% |

```csharp
// dotnet-webapi/dotnet-artisan: Models/Member.cs
public sealed class Member { ... }

// dotnet-webapi/dotnet-artisan: DTOs/MemberDtos.cs
public sealed record MemberResponse(...);

// no-skills/managedcode: Models/Member.cs
public class Member { ... }   // not sealed
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | Zero sealed types |
| dotnet-skills | 3 | ~42% sealed — models in Fitness sealed, others not |
| managedcode | 1 | Zero sealed types |
| dotnet-artisan | 5 | ~98% sealed — models, DTOs, services, handlers |
| dotnet-webapi | 5 | ~98% sealed — comprehensive and consistent |

**Verdict:** dotnet-artisan and dotnet-webapi apply `sealed` universally. This is a clear signal that these skills encode the best practice.

---

## 3. Modern C# Adoption [LOW]

Evaluating primary constructors, collection expressions (`= []`), global usings, and file-scoped namespaces.

| Config | Primary Ctors | Collection Expr `[]` | File-Scoped NS | Global Usings |
|---|---|---|---|---|
| no-skills | Partial | 0 | ✅ | Implicit only |
| dotnet-skills | Partial | 20 | ✅ | Implicit only |
| managedcode | ✅ All | 21 | ✅ | Implicit only |
| dotnet-artisan | ✅ All | 24 | ✅ | Implicit only |
| dotnet-webapi | ✅ All | 16 | ✅ | ✅ Explicit file |

```csharp
// dotnet-webapi: Primary constructor + collection expression
public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    // no private readonly fields needed
}

// no-skills: Traditional constructor
public class MemberService
{
    private readonly FitnessDbContext _db;
    public MemberService(FitnessDbContext db) { _db = db; }
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | File-scoped NS, some primary ctors, no collection expressions |
| dotnet-skills | 3 | Mixed: primary ctors in some projects, collection expressions |
| managedcode | 4 | Consistent primary ctors and collection expressions |
| dotnet-artisan | 5 | All modern features consistently applied |
| dotnet-webapi | 5 | All modern features + explicit GlobalUsings.cs |

**Verdict:** dotnet-artisan and dotnet-webapi consistently use C# 12 features throughout.

---

## 4. DTO Design [HIGH]

Evaluating records vs classes, sealed modifiers, naming conventions, and immutability.

**no-skills:** Mixed — FitnessStudioApi uses positional records, but LibraryApi and VetClinicApi use mutable classes with `{ get; set; }`. Naming uses `*Dto` suffix.

```csharp
// no-skills FitnessStudioApi: Records (good)
public record MemberDto(int Id, string FirstName, ...);

// no-skills LibraryApi: Mutable class (poor)
public class AuthorCreateDto { public string FirstName { get; set; } = string.Empty; }
```

**dotnet-skills:** All classes, not records. `*Dto` naming. Mutable with `{ get; set; }`. FitnessStudioApi has some sealed DTOs.

**managedcode:** Records used consistently across all three projects. `*Dto` naming. Some use `init` properties.

**dotnet-artisan:** Sealed records with `*Request/*Response` naming. Full immutability via `init` and `required` modifiers.

**dotnet-webapi:** Sealed records with `*Request/*Response` naming. Full immutability via `init` and positional records. `required` modifier used.

```csharp
// dotnet-webapi: Sealed record with init and required
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
    [Required, EmailAddress]
    public required string Email { get; init; }
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Inconsistent — records in one project, mutable classes in others |
| dotnet-skills | 2 | All classes, mutable, partial sealed |
| managedcode | 4 | Records, `init`, but not sealed, `*Dto` naming |
| dotnet-artisan | 5 | Sealed records, `*Request/*Response`, immutable |
| dotnet-webapi | 5 | Sealed records, `*Request/*Response`, `required init`, immutable |

**Verdict:** dotnet-artisan and dotnet-webapi produce the most modern DTO patterns with sealed records and `*Request/*Response` naming.

---

## 5. Service Abstraction [HIGH]

All five configurations use the interface + implementation pattern with `AddScoped<IService, Service>()` registration. Each generates 7 service pairs per project.

```csharp
// All configs: Program.cs
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBookingService, BookingService>();
```

| Config | Score | Justification |
|---|---|---|
| All configs | 4 | Proper interface-based DI with scoped lifetime |

**Verdict:** All configurations handle this correctly. This is a baseline Copilot capability.

---

## 6. CancellationToken Propagation [HIGH]

Critical for production — prevents wasted server resources on cancelled HTTP requests.

| Config | CT Usages (total) | Endpoints → Services → EF Core |
|---|---|---|
| no-skills | 0 | ❌ → ❌ → ❌ |
| dotnet-skills | 6 | Partial in VetClinic only |
| managedcode | 237 | ✅ → ✅ → ✅ |
| dotnet-artisan | 237 | ✅ → ✅ → ✅ |
| dotnet-webapi | 359 | ✅ → ✅ → ✅ |

```csharp
// dotnet-webapi: Full chain
// Endpoint
group.MapGet("/", async (..., CancellationToken ct) =>
    TypedResults.Ok(await service.GetAllAsync(..., ct)));

// Service
public async Task<PaginatedResponse<MemberResponse>> GetAllAsync(
    ..., CancellationToken ct)
{
    var count = await query.CountAsync(ct);
    var items = await query.Skip(...).Take(...).ToListAsync(ct);
}

// no-skills: No cancellation at all
public async Task<IActionResult> GetAll(...) // no CT parameter
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | Zero CancellationToken usage anywhere |
| dotnet-skills | 1 | Only 6 usages (VetClinic only, not propagated properly) |
| managedcode | 4 | Consistent across all projects and layers |
| dotnet-artisan | 4 | Consistent across all projects and layers |
| dotnet-webapi | 5 | Most comprehensive: 359 usages, full chain |

**Verdict:** dotnet-webapi has the most thorough CancellationToken propagation. no-skills completely ignores it.

---

## 7. AsNoTracking Usage [MEDIUM]

Read-only EF Core queries should use `AsNoTracking()` to avoid unnecessary change-tracker overhead.

| Config | AsNoTracking Count |
|---|---|
| no-skills | 0 |
| dotnet-skills | 39 |
| managedcode | 60 |
| dotnet-artisan | 59 |
| dotnet-webapi | 75 |

```csharp
// dotnet-webapi: Consistent AsNoTracking on reads
var query = db.Members.AsNoTracking().AsQueryable();

// no-skills: Tracks all queries (wasteful)
var query = _db.Members.AsQueryable(); // no AsNoTracking
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | Zero AsNoTracking — all reads tracked |
| dotnet-skills | 3 | Used in 2/3 projects (missing in VetClinic) |
| managedcode | 4 | Consistent across all projects |
| dotnet-artisan | 4 | Consistent across all projects |
| dotnet-webapi | 5 | Most comprehensive with 75 usages |

**Verdict:** All skills configurations add AsNoTracking. Only the baseline (no-skills) misses this important optimization.

---

## 8. Return Type Precision [MEDIUM]

`IReadOnlyList<T>` prevents accidental mutation of returned collections and maintains indexing capability.

| Config | IReadOnlyList Count |
|---|---|
| no-skills | 0 |
| dotnet-skills | 1 |
| managedcode | 30 |
| dotnet-artisan | 26 |
| dotnet-webapi | 70 |

```csharp
// dotnet-webapi: IReadOnlyList in service interfaces and pagination
public sealed record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
}
Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(int memberId, CancellationToken ct);

// no-skills: Mutable List<T> everywhere
public List<T> Items { get; set; } = new();
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | IEnumerable used occasionally, mostly List&lt;T&gt; |
| dotnet-skills | 2 | Only 1 IReadOnlyList usage (LibraryApi) |
| managedcode | 4 | Consistent IReadOnlyList usage |
| dotnet-artisan | 4 | Good IReadOnlyList usage in DTOs and services |
| dotnet-webapi | 5 | Most comprehensive — 70 usages across all layers |

**Verdict:** dotnet-webapi leads with systematic use of `IReadOnlyList<T>` for all read-only collection returns.

---

## 9. Data Seeder Design [MEDIUM]

Seed data initialization strategy: `HasData()` (migration-friendly), injectable seeders, or static methods.

All configurations predominantly use static seeder methods called from `Program.cs`, with `Any()` checks to avoid duplicate seeding. dotnet-webapi's VetClinicApi uniquely uses `HasData()` in `OnModelCreating` for migration-integrated seeding.

```csharp
// Most configs: Static seeder
public static async Task SeedAsync(FitnessDbContext db) {
    if (await db.MembershipPlans.AnyAsync()) return;
    // ... add seed data
}

// dotnet-webapi VetClinicApi: HasData (migration-integrated)
modelBuilder.Entity<Owner>().HasData(
    new Owner { Id = 1, FirstName = "John", ... });
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Mixed: FitnessStudio uses injectable service, others static |
| dotnet-skills | 3 | Static async seeders, adequate |
| managedcode | 3 | Static async seeders |
| dotnet-artisan | 3 | Static async seeders |
| dotnet-webapi | 4 | VetClinic uses HasData(); FitnessStudio uses injectable seeder |

**Verdict:** dotnet-webapi edges ahead with its mix of `HasData()` and injectable seeders.

---

## 10. Middleware Style [HIGH]

The modern approach is `IExceptionHandler` (.NET 8+), which is DI-aware and composable. Convention-based middleware with `RequestDelegate` is the traditional approach.

| Config | IExceptionHandler Usages | Pattern |
|---|---|---|
| no-skills | 0 | Convention-based middleware (try/catch) |
| dotnet-skills | 1 | Fitness: IExceptionHandler; Library/Vet: Convention middleware |
| managedcode | 3 | IExceptionHandler in all three projects |
| dotnet-artisan | 3 | IExceptionHandler in all three projects |
| dotnet-webapi | 8 | IExceptionHandler, registered with AddExceptionHandler&lt;T&gt;() |

```csharp
// dotnet-webapi: Modern IExceptionHandler
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
        // ... write ProblemDetails
    }
}

// no-skills: Convention-based middleware
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context) {
        try { await _next(context); }
        catch (BusinessRuleException ex) { /* handle */ }
    }
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Convention middleware — works but outdated pattern |
| dotnet-skills | 3 | Mixed: IExceptionHandler in 1/3, convention in 2/3 |
| managedcode | 4 | IExceptionHandler in all projects |
| dotnet-artisan | 4 | IExceptionHandler with primary constructors |
| dotnet-webapi | 5 | IExceptionHandler with sealed class, primary ctors, switch expressions |

**Verdict:** dotnet-webapi uses the most modern pattern consistently.

---

## 11. Exception Handling Strategy [HIGH]

Custom domain exceptions vs built-in types vs result patterns.

**no-skills:** Defines custom exceptions like `BusinessRuleException`, `NotFoundException`, `ConflictException` — good semantic clarity.

```csharp
// no-skills: Custom domain exceptions
public class BusinessRuleException(string message, int statusCode = 400) : Exception(message);
public class NotFoundException(string message) : Exception(message);
```

**dotnet-skills/dotnet-artisan/dotnet-webapi:** Use built-in exception types (`KeyNotFoundException`, `InvalidOperationException`, `ArgumentException`) mapped via switch expressions.

**managedcode:** Defines custom exceptions similar to no-skills (`BusinessRuleException`, `NotFoundException`).

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | Rich custom exceptions with HTTP status codes |
| dotnet-skills | 3 | Built-in types only — adequate but less semantic |
| managedcode | 4 | Custom domain exceptions with clear semantics |
| dotnet-artisan | 3 | Built-in types — functional but could be more expressive |
| dotnet-webapi | 3 | Built-in types mapped via switch — clean but generic |

**Verdict:** no-skills and managedcode produce richer exception hierarchies. Skills configurations favor simplicity with built-in types.

---

## 12. File Organization [MEDIUM]

Consistent folder structure with clear separation of concerns.

**dotnet-webapi and dotnet-artisan** add an `Endpoints/` folder (for minimal APIs) and sometimes a `GlobalUsings.cs` file:

```
// dotnet-webapi / dotnet-artisan structure:
├── Data/         (DbContext, DataSeeder)
├── DTOs/         (per-entity DTO files)
├── Endpoints/    (per-entity endpoint extensions)
├── Models/       (entity models, enums)
├── Services/     (interfaces + implementations)
├── Middleware/    (IExceptionHandler)
└── Program.cs

// no-skills / managedcode / dotnet-skills structure:
├── Controllers/  (per-entity controllers)
├── Data/         (DbContext, DataSeeder)
├── DTOs/         (per-entity DTO files)
├── Models/       (entity models, enums)
├── Services/     (interfaces + implementations)
├── Middleware/    (exception handler)
└── Program.cs
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | Clean traditional structure, consistent |
| dotnet-skills | 4 | Clean structure, separate Interfaces/ in some projects |
| managedcode | 4 | Clean structure, consistent |
| dotnet-artisan | 5 | Endpoints/ folder, clean separation, consistent |
| dotnet-webapi | 5 | Endpoints/, GlobalUsings.cs, most consistent |

**Verdict:** All configurations produce reasonable structure. dotnet-webapi and dotnet-artisan add Endpoints/ for better organization.

---

## 13. Pagination [MEDIUM]

Pagination type design: sealed records vs mutable classes, computed properties, and defaults.

```csharp
// dotnet-webapi: Sealed record with factory method
public sealed record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalPages { get; init; }
    public required bool HasNextPage { get; init; }
    public required bool HasPreviousPage { get; init; }
}

// no-skills LibraryApi: Mutable class
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Mixed: record in Fitness, mutable class in Library/Vet |
| dotnet-skills | 3 | Non-sealed class with computed properties |
| managedcode | 4 | Record with IReadOnlyList, computed properties |
| dotnet-artisan | 4 | Sealed record with computed properties |
| dotnet-webapi | 5 | Sealed record, IReadOnlyList, required + init, factory method |

**Verdict:** dotnet-webapi produces the most robust pagination type with full immutability.

---

## 14. OpenAPI Metadata [MEDIUM]

Richness of endpoint documentation — WithName(), WithSummary(), Produces&lt;T&gt;(), etc.

**dotnet-webapi** decorates every endpoint with full metadata:

```csharp
// dotnet-webapi: Rich endpoint metadata
group.MapGet("/", async (...) => ...)
    .WithName("GetMembers")
    .WithSummary("List members")
    .WithDescription("List members with optional search and filtering")
    .Produces<PaginatedResponse<MemberResponse>>();
```

**Controller-based configs** use `[ProducesResponseType]` attributes and XML documentation comments — functional but less expressive.

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | ProducesResponseType attributes, XML summaries |
| dotnet-skills | 3 | ProducesResponseType, some projects better than others |
| managedcode | 3 | ProducesResponseType on controllers |
| dotnet-artisan | 4 | WithSummary/WithName on minimal API endpoints |
| dotnet-webapi | 5 | Full metadata: WithName, WithSummary, WithDescription, Produces, union types |

**Verdict:** dotnet-webapi produces the richest OpenAPI metadata thanks to minimal API fluent decorators.

---

## 15. Structured Logging [MEDIUM]

All configurations use `ILogger<T>` with structured message templates. None use `[LoggerMessage]` source generators.

```csharp
// All skill configs: Structured templates
logger.LogInformation("Registered new member {MemberName} with ID {MemberId}",
    $"{member.FirstName} {member.LastName}", member.Id);
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | Consistent structured templates across all projects |
| dotnet-skills | 3 | Inconsistent: some projects log more than others |
| managedcode | 4 | Consistent structured templates |
| dotnet-artisan | 3 | Less logging overall, but properly structured |
| dotnet-webapi | 4 | Consistent templates in services and middleware |

**Verdict:** All use proper structured logging. No configuration uses `[LoggerMessage]` for high-performance logging.

---

## 16. Nullable Reference Types [MEDIUM]

All configurations enable `<Nullable>enable</Nullable>` and use proper `?` annotations on optional properties and navigation properties.

| Config | Score | Justification |
|---|---|---|
| All configs | 4 | Enabled with proper annotations; some `= null!` usage for navigation props |

**Verdict:** Parity across all configurations. This is a template default.

---

## 17. Package Discipline [MEDIUM]

Evaluating unnecessary dependencies and Swashbuckle usage in .NET 10.

| Config | Swashbuckle | FluentValidation | Extra Packages |
|---|---|---|---|
| no-skills | 2/3 projects | No | None |
| dotnet-skills | 1/3 projects | 1/3 (Fitness) | None |
| managedcode | 2/3 projects | 1/3 (Library) | None |
| dotnet-artisan | 3/3 projects | No | SwaggerUI in Fitness |
| dotnet-webapi | 0/3 projects | No | None |

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Swashbuckle in 2/3, no unnecessary extras |
| dotnet-skills | 3 | Swashbuckle in Library, FluentValidation in Fitness |
| managedcode | 3 | Swashbuckle in 2/3, FluentValidation in Library |
| dotnet-artisan | 3 | Swashbuckle in all 3 alongside AddOpenApi |
| dotnet-webapi | 4 | No Swashbuckle, minimal packages (only 3) |

**Verdict:** dotnet-webapi is the leanest with only essential packages.

---

## 18. EF Core Relationship Configuration [HIGH]

All configurations use Fluent API in `OnModelCreating` with proper relationship configuration including `HasForeignKey`, `OnDelete`, `HasIndex`, and `HasConversion<string>()` for enums.

```csharp
// Common pattern across all configs:
modelBuilder.Entity<Membership>(entity =>
{
    entity.HasOne(e => e.Member)
        .WithMany(m => m.Memberships)
        .HasForeignKey(e => e.MemberId)
        .OnDelete(DeleteBehavior.Cascade);
    entity.Property(e => e.Status).HasConversion<string>();
});
```

| Config | Score | Justification |
|---|---|---|
| All configs | 4 | Comprehensive Fluent API; no IEntityTypeConfiguration&lt;T&gt; separation |

**Verdict:** All configurations produce adequate Fluent API configuration. None use `IEntityTypeConfiguration<T>` for cleaner separation.

---

## 19. TypedResults Usage [MEDIUM]

TypedResults provide compile-time type safety and automatic OpenAPI schema generation for minimal APIs.

| Config | TypedResults Count |
|---|---|
| no-skills | 0 |
| dotnet-skills | 0 |
| managedcode | 0 |
| dotnet-artisan | 88 |
| dotnet-webapi | 116 |

```csharp
// dotnet-webapi: TypedResults with union return types
group.MapGet("/{id}", async (int id, IMemberService service, CancellationToken ct)
    => await service.GetByIdAsync(id, ct) is { } member
        ? TypedResults.Ok(member)
        : TypedResults.NotFound())
    .WithName("GetMember")
    .Produces<MemberResponse>()
    .ProducesProblem(404);

// no-skills/managedcode: IActionResult (no type safety)
return Ok(result);  // runtime-only type checking
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | Not applicable — uses controllers |
| dotnet-skills | 1 | Not applicable — uses controllers (mostly) |
| managedcode | 1 | Not applicable — uses controllers |
| dotnet-artisan | 4 | 88 TypedResults usages across minimal API endpoints |
| dotnet-webapi | 5 | 116 TypedResults, union types, comprehensive |

**Verdict:** Only minimal API configurations use TypedResults. dotnet-webapi leads with 116 usages and `Results<Ok<T>, NotFound>` union types.

---

## 20. HTTP Test File Quality [MEDIUM]

All configurations generate `.http` files with request examples. dotnet-webapi includes business rule test cases.

```http
### dotnet-webapi: Business rule test cases
### Test: Book a premium class as a Basic member
### Expected: 400 — plan does not include premium classes
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{
  "classScheduleId": 5,
  "memberId": 3
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Basic CRUD requests, adequate |
| dotnet-skills | 3 | Present, covers endpoints |
| managedcode | 3 | Present, covers endpoints |
| dotnet-artisan | 3 | Present with structured groups |
| dotnet-webapi | 4 | Includes business rule test scenarios with expected outcomes |

**Verdict:** dotnet-webapi includes more comprehensive test scenarios.

---

## 21. Code Standards Compliance [LOW]

All configurations follow .NET naming conventions: PascalCasing, Async suffix, explicit access modifiers, and file-scoped namespaces.

| Config | Score | Justification |
|---|---|---|
| All configs | 4 | Consistent adherence to .NET naming guidelines |

**Verdict:** Parity — this is a Copilot baseline capability.

---

## 22. Enum Design [MEDIUM]

All configurations use enums for domain status fields with singular names (e.g., `MembershipStatus`, `BookingStatus`).

```csharp
// Common across all configs:
public enum MembershipStatus { Active, Expired, Cancelled, Frozen }
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }
```

**dotnet-webapi, dotnet-artisan, no-skills, managedcode:** Use `HasConversion<string>()` for string storage.
**dotnet-skills:** Inconsistent — some projects convert, some don't.

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | Proper enums with string conversion |
| dotnet-skills | 3 | Inconsistent string conversion |
| managedcode | 4 | Proper enums with string conversion |
| dotnet-artisan | 4 | Proper enums, some missing string conversion |
| dotnet-webapi | 4 | Proper enums with consistent string conversion |

**Verdict:** All configurations handle enums well. Minor inconsistencies in dotnet-skills.

---

## 23. Guard Clauses & Argument Validation [MEDIUM]

No configuration uses the modern `ArgumentNullException.ThrowIfNull()` pattern. All use null-coalescing throws or traditional if-null-throw patterns.

```csharp
// All configs: Null-coalescing throw (adequate)
var member = await db.Members.FindAsync(id)
    ?? throw new KeyNotFoundException($"Member with ID {id} not found.");
```

| Config | Score | Justification |
|---|---|---|
| All configs | 3 | Adequate null checking, but no ThrowIfNull() |

**Verdict:** All configurations miss the modern `ArgumentNullException.ThrowIfNull()` pattern.

---

## 24. Async/Await Best Practices [HIGH]

All configurations properly use `async Task` return types, Async suffixes, and avoid sync-over-async anti-patterns.

| Config | Score | Justification |
|---|---|---|
| All configs | 4 | Proper async patterns throughout, no async void or sync-over-async |

**Verdict:** All configurations handle async/await correctly.

---

## 25. Dispose & Resource Management [MEDIUM]

All configurations use `using` blocks for DI scopes in `Program.cs` seeding. No special `IAsyncDisposable` implementations needed.

| Config | Score | Justification |
|---|---|---|
| All configs | 3 | Adequate — using blocks for scopes, DI handles DbContext |

**Verdict:** Parity across all configurations.

---

## 26. EF Migration Usage [CRITICAL]

`EnsureCreated()` bypasses migrations — it cannot evolve schemas safely.

| Config | Pattern |
|---|---|
| no-skills | `EnsureCreated()` / `EnsureCreatedAsync()` in all 3 projects |
| dotnet-skills | `EnsureCreated()` / `EnsureCreatedAsync()` in all 3 projects |
| managedcode | `EnsureCreated()` / `EnsureCreatedAsync()` in all 3 projects |
| dotnet-artisan | `EnsureCreatedAsync()` in all 3 projects |
| dotnet-webapi | `MigrateAsync()` / `Migrate()` in all 3 projects ✅ |

```csharp
// dotnet-webapi: Proper migration usage
await context.Database.MigrateAsync();

// All others: EnsureCreated (anti-pattern)
await db.Database.EnsureCreatedAsync();
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | EnsureCreated — cannot evolve schema |
| dotnet-skills | 1 | EnsureCreated — cannot evolve schema |
| managedcode | 1 | EnsureCreated — cannot evolve schema |
| dotnet-artisan | 1 | EnsureCreated — cannot evolve schema |
| dotnet-webapi | 4 | Uses Migrate() / MigrateAsync() — production-safe |

**Verdict:** Only dotnet-webapi uses proper EF Core migrations. This is a **critical** differentiator.

---

## 27. HSTS & HTTPS Redirection [HIGH]

Only one project across all configurations includes HTTPS redirection:

```csharp
// no-skills FitnessStudioApi only:
app.UseHttpsRedirection();
```

No configuration uses `app.UseHsts()`.

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | HttpsRedirection in 1/3 projects — partial |
| dotnet-skills | 1 | Missing entirely |
| managedcode | 1 | Missing entirely |
| dotnet-artisan | 1 | Missing entirely |
| dotnet-webapi | 1 | Missing entirely |

**Verdict:** Universally missing — all configurations should add HSTS for non-development environments.

---

## 28. NuGet Version Pinning [CRITICAL]

Wildcard versions like `10.*-*` or `10.0.0-*` can pull in breaking pre-release packages.

| Config | Wildcard Packages | Pinned Packages |
|---|---|---|
| no-skills | EF Core in Fitness/Vet (10.0.0-\*, 10.\*-\*) | Library fully pinned |
| dotnet-skills | EF Core in Fitness/Vet (10.0.0-\*, 10.\*-\*) | Library fully pinned |
| managedcode | EF Core in Library (10.0.\*-\*) | Fitness/Vet pinned |
| dotnet-artisan | EF Core in all (10.\*-\*) | Swashbuckle pinned |
| dotnet-webapi | EF Core in all (10.\*-\*, 10.0.\*-\*) | OpenApi pinned in some |

| Config | Score | Justification |
|---|---|---|
| All configs | 2 | All use wildcards for some packages — non-reproducible builds |

**Verdict:** All configurations suffer from wildcard NuGet versioning. This is a universal gap.

---

## 29. FluentValidation Usage [MEDIUM]

| Config | FluentValidation |
|---|---|
| no-skills | ❌ Not used |
| dotnet-skills | Partial (FitnessStudioApi only) |
| managedcode | Partial (LibraryApi only) |
| dotnet-artisan | ❌ Not used |
| dotnet-webapi | ❌ Not used |

```csharp
// dotnet-skills FitnessStudioApi:
public sealed class CreateMemberValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | Data annotations only |
| dotnet-skills | 2 | FluentValidation in 1/3 projects |
| managedcode | 2 | FluentValidation in 1/3 projects |
| dotnet-artisan | 1 | Data annotations only |
| dotnet-webapi | 1 | Data annotations only |

**Verdict:** FluentValidation usage is sporadic. Most configs rely on Data Annotations, which is adequate but less expressive.

---

## 30. Built-in OpenAPI over Swashbuckle [CRITICAL]

Swashbuckle is deprecated in .NET 9+ — the built-in `AddOpenApi()` is the official replacement.

| Config | Uses Swashbuckle | Uses AddOpenApi() |
|---|---|---|
| no-skills | 2/3 projects | 1/3 (VetClinic) |
| dotnet-skills | 1/3 (Library) | 2/3 (Fitness, VetClinic) |
| managedcode | 2/3 projects | 1/3 (VetClinic) |
| dotnet-artisan | 3/3 projects | Also uses AddOpenApi() |
| dotnet-webapi | 0/3 projects ✅ | 3/3 projects ✅ |

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Swashbuckle in 2/3 — deprecated dependency |
| dotnet-skills | 3 | Mixed: Swashbuckle in Library, built-in elsewhere |
| managedcode | 2 | Swashbuckle in 2/3 — deprecated dependency |
| dotnet-artisan | 3 | Swashbuckle alongside AddOpenApi — redundant |
| dotnet-webapi | 4 | Pure built-in OpenAPI — no Swashbuckle |

**Verdict:** Only dotnet-webapi consistently avoids the deprecated Swashbuckle dependency.

---

## 31. Build & Run Success [CRITICAL]

All 15 projects (5 configs × 3 scenarios) successfully compile with `dotnet build`. Minor warnings exist (NETSDK1057 for preview .NET 10) but no blocking errors.

| Config | Score | Justification |
|---|---|---|
| All configs | 4 | All projects build successfully with zero errors |

**Verdict:** Parity — all configurations produce compilable projects. Score 4 instead of 5 due to informational warnings.

---

## 32. Input Validation Coverage [CRITICAL]

All configurations validate inputs via Data Annotations (`[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`). Services add business rule validation.

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Data annotations, inconsistent across projects |
| dotnet-skills | 4 | FluentValidation in Fitness adds depth |
| managedcode | 4 | FluentValidation in Library; annotations elsewhere |
| dotnet-artisan | 4 | Thorough annotations with service-level validation |
| dotnet-webapi | 4 | Thorough annotations with service-level validation |

**Verdict:** Skill configurations produce more thorough validation. FluentValidation adds extra coverage in some projects.

---

## 33. Endpoint Completeness [CRITICAL]

All configurations implement the full set of CRUD endpoints plus business-logic endpoints specified in each scenario prompt. Each project implements 30–50 endpoints covering all resources.

| Config | Score | Justification |
|---|---|---|
| All configs | 4 | Comprehensive endpoint coverage matching specs |

**Verdict:** Parity — all configurations implement the required endpoints.

---

## 34. Business Rule Implementation [CRITICAL]

All configurations implement core business rules: booking window validation, capacity management, membership tier access, cancellation policies, and domain constraints.

```csharp
// Common across skill configs:
if (schedule.CurrentEnrollment >= schedule.Capacity)
    booking.Status = BookingStatus.Waitlisted;

if (age < 16)
    throw new BusinessRuleException("Member must be at least 16 years old.");
```

| Config | Score | Justification |
|---|---|---|
| All configs | 4 | Core business rules implemented; some edge cases may be missing |

**Verdict:** All configurations implement the major business rules effectively.

---

## 35. Error Response Conformance [HIGH]

All configurations return ProblemDetails for error conditions, following RFC 7807.

```csharp
// All configs produce:
{
  "status": 404,
  "title": "Not Found",
  "detail": "Member with ID 999 not found.",
  "instance": "/api/members/999"
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | ProblemDetails but inconsistent structure |
| dotnet-skills | 4 | Consistent ProblemDetails with Type field |
| managedcode | 4 | Consistent ProblemDetails |
| dotnet-artisan | 4 | Consistent ProblemDetails with Instance field |
| dotnet-webapi | 4 | Most consistent ProblemDetails with all fields |

**Verdict:** Skill configurations produce slightly more consistent error responses.

---

## 36. Security Vulnerability Scan [CRITICAL]

No configuration includes obviously vulnerable patterns. All use parameterized EF Core queries (preventing SQL injection). No hardcoded secrets detected.

| Config | Score | Justification |
|---|---|---|
| All configs | 3 | Safe patterns, but EnsureCreated (except webapi) and wildcards are risks |

**Verdict:** All are adequate. dotnet-webapi's use of Migrate() and no Swashbuckle gives it a slight edge.

---

## Weighted Summary

Weight multipliers: Critical = ×3, High = ×2, Medium = ×1, Low = ×0.5

| Dimension | Tier | Weight | no-skills | dotnet-skills | managedcode | dotnet-artisan | dotnet-webapi |
|---|---|---|---|---|---|---|---|
| API Architecture | CRITICAL | ×3 | 6 | 9 | 6 | 12 | 15 |
| EF Migration Usage | CRITICAL | ×3 | 3 | 3 | 3 | 3 | 12 |
| NuGet Version Pinning | CRITICAL | ×3 | 6 | 6 | 6 | 6 | 6 |
| Built-in OpenAPI | CRITICAL | ×3 | 6 | 9 | 6 | 9 | 12 |
| Build & Run Success | CRITICAL | ×3 | 12 | 12 | 12 | 12 | 12 |
| Input Validation | CRITICAL | ×3 | 9 | 12 | 12 | 12 | 12 |
| Endpoint Completeness | CRITICAL | ×3 | 12 | 12 | 12 | 12 | 12 |
| Business Rules | CRITICAL | ×3 | 12 | 12 | 12 | 12 | 12 |
| Security Scan | CRITICAL | ×3 | 9 | 9 | 9 | 9 | 9 |
| DTO Design | HIGH | ×2 | 6 | 4 | 8 | 10 | 10 |
| Service Abstraction | HIGH | ×2 | 8 | 8 | 8 | 8 | 8 |
| CancellationToken | HIGH | ×2 | 2 | 2 | 8 | 8 | 10 |
| Middleware Style | HIGH | ×2 | 4 | 6 | 8 | 8 | 10 |
| Exception Strategy | HIGH | ×2 | 8 | 6 | 8 | 6 | 6 |
| EF Core Relationships | HIGH | ×2 | 8 | 8 | 8 | 8 | 8 |
| Async/Await | HIGH | ×2 | 8 | 8 | 8 | 8 | 8 |
| HSTS & HTTPS | HIGH | ×2 | 4 | 2 | 2 | 2 | 2 |
| Error Conformance | HIGH | ×2 | 6 | 8 | 8 | 8 | 8 |
| Sealed Types | MEDIUM | ×1 | 1 | 3 | 1 | 5 | 5 |
| AsNoTracking | MEDIUM | ×1 | 1 | 3 | 4 | 4 | 5 |
| Return Type Precision | MEDIUM | ×1 | 2 | 2 | 4 | 4 | 5 |
| Data Seeder | MEDIUM | ×1 | 3 | 3 | 3 | 3 | 4 |
| File Organization | MEDIUM | ×1 | 4 | 4 | 4 | 5 | 5 |
| Pagination | MEDIUM | ×1 | 3 | 3 | 4 | 4 | 5 |
| OpenAPI Metadata | MEDIUM | ×1 | 3 | 3 | 3 | 4 | 5 |
| Structured Logging | MEDIUM | ×1 | 4 | 3 | 4 | 3 | 4 |
| Nullable Refs | MEDIUM | ×1 | 4 | 4 | 4 | 4 | 4 |
| Package Discipline | MEDIUM | ×1 | 3 | 3 | 3 | 3 | 4 |
| TypedResults | MEDIUM | ×1 | 1 | 1 | 1 | 4 | 5 |
| HTTP Test Quality | MEDIUM | ×1 | 3 | 3 | 3 | 3 | 4 |
| Enum Design | MEDIUM | ×1 | 4 | 3 | 4 | 4 | 4 |
| Guard Clauses | MEDIUM | ×1 | 3 | 3 | 3 | 3 | 3 |
| Dispose & Resources | MEDIUM | ×1 | 3 | 3 | 3 | 3 | 3 |
| FluentValidation | MEDIUM | ×1 | 1 | 2 | 2 | 1 | 1 |
| Modern C# Adoption | LOW | ×0.5 | 1.5 | 1.5 | 2 | 2.5 | 2.5 |
| Code Standards | LOW | ×0.5 | 2 | 2 | 2 | 2 | 2 |
| **TOTAL** | | | **175.5** | **187.5** | **204** | **222.5** | **252.5** |

### Final Ranking

| Rank | Configuration | Weighted Score | % of Maximum |
|---|---|---|---|
| 🥇 1st | **dotnet-webapi** | **252.5** | **77%** |
| 🥈 2nd | **dotnet-artisan** | **222.5** | **68%** |
| 🥉 3rd | **managedcode-dotnet-skills** | **204** | **62%** |
| 4th | **dotnet-skills** | **187.5** | **57%** |
| 5th | **no-skills** | **175.5** | **54%** |

---

## What All Versions Get Right

- ✅ **Service Abstraction:** All use interface + implementation with `AddScoped<IService, Service>()` DI registration
- ✅ **File-scoped Namespaces:** Universal adoption of `namespace X;` syntax
- ✅ **Nullable Reference Types:** All enable `<Nullable>enable</Nullable>` with proper annotations
- ✅ **Async/Await Patterns:** No `async void`, no sync-over-async anti-patterns, proper `Task<T>` returns
- ✅ **EF Core Fluent API:** All use OnModelCreating with proper relationship configuration
- ✅ **ProblemDetails Errors:** All return RFC 7807 ProblemDetails for error responses
- ✅ **Code Standards:** PascalCasing, Async suffix, explicit access modifiers throughout
- ✅ **Endpoint Completeness:** All implement the full set of specified endpoints
- ✅ **Business Rule Implementation:** Core domain rules enforced in services
- ✅ **Build Success:** All 15 projects compile without errors
- ✅ **Enum Design:** Domain status fields use enums with singular names
- ✅ **Structured Logging:** `ILogger<T>` with message templates (not string interpolation)

---

## Summary: Impact of Skills

### Most Impactful Differences (ranked by weighted score impact)

1. **EF Migration Usage (+9 for dotnet-webapi):** Only dotnet-webapi uses `Migrate()` instead of `EnsureCreated()`. This is the single most critical improvement — EnsureCreated cannot evolve schemas, making it unusable in production. This alone is worth 9 weighted points.

2. **API Architecture (+9 for dotnet-webapi, +6 for dotnet-artisan):** Minimal APIs with MapGroup produce cleaner, more maintainable code with better OpenAPI integration. Controller-based configs miss built-in features like TypedResults and fluent endpoint metadata.

3. **CancellationToken Propagation (+8 for dotnet-webapi, +6 for artisan/managedcode):** The no-skills baseline has zero CancellationToken usage. dotnet-webapi has 359 usages — a fundamental production safety mechanism.

4. **Sealed Types (+4 for dotnet-artisan/dotnet-webapi):** 98% sealed types in these configurations versus 0% in no-skills and managedcode. Enables JIT devirtualization and communicates design intent.

5. **DTO Design (+4 for dotnet-artisan/dotnet-webapi):** Sealed records with `required init` properties and `*Request/*Response` naming produce safer, more expressive API contracts than mutable classes with `*Dto` naming.

### Overall Assessment

**dotnet-webapi** (252.5 points) is the clear winner, scoring highest on 18 of 36 dimensions and leading in every critical category except HSTS and NuGet pinning. Its consistent use of minimal APIs, TypedResults, EF migrations, CancellationToken propagation, and sealed types makes it the most production-ready option.

**dotnet-artisan** (222.5 points) is the strong second place, sharing many of dotnet-webapi's strengths (sealed types, minimal APIs, TypedResults) but falling short on EF migrations and having slightly less comprehensive CancellationToken usage.

**managedcode-dotnet-skills** (204 points) provides meaningful improvements over the baseline — notably CancellationToken propagation, AsNoTracking, and IReadOnlyList usage — but stays with the controller pattern and EnsureCreated.

**dotnet-skills** (187.5 points) shows inconsistency across projects, sometimes generating controllers and sometimes minimal APIs, with mixed sealed type usage and limited CancellationToken support.

**no-skills** (175.5 points) represents the baseline Copilot output. While it produces functional, compilable code with proper service abstractions and business rules, it misses numerous production best practices: no CancellationToken, no AsNoTracking, no sealed types, no TypedResults, and no EF migrations.

**Bottom line:** Custom skills improve generated code quality by **14–44%** (weighted score increase), with the focused dotnet-webapi skill providing the most consistent and comprehensive improvements. The gap between no-skills and dotnet-webapi is substantial and spans both architectural decisions (minimal APIs, migrations) and implementation details (sealed types, CancellationTokens, TypedResults).
