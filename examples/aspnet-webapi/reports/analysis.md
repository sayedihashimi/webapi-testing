# Comparative Analysis: managedcode-dotnet-skills, no-skills, dotnet-skills, dotnet-artisan, dotnet-webapi

## Introduction

This report compares **5 Copilot configurations**, each generating **3 ASP.NET Core Web API applications** (.NET 10, EF Core + SQLite). The configurations are:

| Directory | Configuration | Description |
|---|---|---|
| `managedcode-dotnet-skills` | Community managed-code skills | Community-maintained .NET skill set |
| `no-skills` | Baseline (default Copilot) | No custom skills — raw Copilot output |
| `dotnet-skills` | Official .NET Skills (dotnet/skills) | Microsoft-maintained dotnet/skills |
| `dotnet-artisan` | dotnet-artisan plugin chain | 9 skills + 14 specialist agents |
| `dotnet-webapi` | dotnet-webapi skill | Dedicated ASP.NET Web API skill |

Each configuration generated three identical scenarios:
- **FitnessStudioApi** — Booking/membership system with class scheduling, waitlists, and instructor management
- **LibraryApi** — Book loans, reservations, overdue fines, and availability tracking
- **VetClinicApi** — Pet healthcare with appointments, vaccinations, and medical records

All scores use a **1–5 scale** averaged across the 3 scenarios per configuration.

---

## Executive Summary

| Dimension [Tier] | no-skills | managedcode | dotnet-skills | dotnet-artisan | dotnet-webapi |
|---|---|---|---|---|---|
| API Architecture [HIGH] | 2 | 2 | 2 | 5 | 5 |
| DTO Design [HIGH] | 3 | 4 | 3 | 5 | 5 |
| Service Abstraction [HIGH] | 4 | 4 | 4 | 5 | 5 |
| CancellationToken Propagation [HIGH] | 1 | 3 | 3 | 5 | 5 |
| Middleware Style [HIGH] | 2 | 4 | 3 | 5 | 5 |
| Exception Handling Strategy [HIGH] | 3 | 3 | 4 | 4 | 4 |
| EF Core Relationship Configuration [HIGH] | 4 | 4 | 4 | 5 | 5 |
| Async/Await Best Practices [HIGH] | 3 | 3 | 3 | 5 | 5 |
| Error Response Conformance [HIGH] | 3 | 3 | 4 | 4 | 4 |
| Endpoint Completeness [CRITICAL] | 4 | 4 | 4 | 5 | 5 |
| Business Rule Implementation [CRITICAL] | 4 | 4 | 4 | 5 | 5 |
| Input Validation Coverage [CRITICAL] | 3 | 3 | 4 | 4 | 4 |
| Security Vulnerability Scan [CRITICAL] | 3 | 3 | 3 | 4 | 4 |
| Sealed Types [MEDIUM] | 1 | 1 | 4 | 5 | 5 |
| AsNoTracking Usage [MEDIUM] | 1 | 4 | 4 | 5 | 5 |
| Return Type Precision [MEDIUM] | 2 | 3 | 2 | 5 | 5 |
| Data Seeder Design [MEDIUM] | 3 | 3 | 3 | 4 | 4 |
| File Organization [MEDIUM] | 3 | 3 | 4 | 4 | 4 |
| Pagination [MEDIUM] | 3 | 3 | 3 | 5 | 5 |
| OpenAPI Metadata [MEDIUM] | 3 | 3 | 3 | 5 | 5 |
| Structured Logging [MEDIUM] | 4 | 3 | 4 | 5 | 5 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Package Discipline [MEDIUM] | 3 | 3 | 3 | 4 | 4 |
| TypedResults Usage [MEDIUM] | 1 | 1 | 1 | 5 | 5 |
| HTTP Test File Quality [MEDIUM] | 3 | 4 | 4 | 4 | 4 |
| Enum Design [MEDIUM] | 3 | 4 | 4 | 4 | 4 |
| Guard Clauses & Argument Validation [MEDIUM] | 3 | 3 | 3 | 3 | 3 |
| Dispose & Resource Management [MEDIUM] | 3 | 3 | 3 | 3 | 3 |
| Code Standards Compliance [LOW] | 3 | 3 | 4 | 5 | 5 |
| Modern C# Adoption [LOW] | 2 | 3 | 3 | 5 | 5 |

---

## 1. API Architecture [HIGH]

### no-skills
All three apps use **traditional Controllers** with `[ApiController]` and `MapControllers()`. No minimal APIs, no route groups.

```csharp
// no-skills: MembersController.cs
[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;
    public MembersController(IMemberService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<MemberListDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, ...)
        => Ok(await _service.GetAllAsync(page, pageSize, search, isActive));
}
```

### managedcode-dotnet-skills
Also uses **Controllers** exclusively. Identical pattern to no-skills.

```csharp
// managedcode: MembersController.cs
[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController(IMemberService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginatedResponse<MemberListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, ..., CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
        return Ok(result);
    }
}
```

### dotnet-skills
Also uses **Controllers**. Mixed consistency — FitnessStudioApi uses controllers, LibraryApi uses controllers, VetClinicApi uses controllers in some apps but minimal APIs with MapGroup in others.

```csharp
// dotnet-skills: MembersController.cs
[ApiController]
[Route("api/members")]
[Produces("application/json")]
public sealed class MembersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<MemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(...) { }
}
```

### dotnet-artisan
Uses **Minimal APIs with MapGroup()** and extension methods that keep Program.cs clean. FitnessStudioApi and VetClinicApi use minimal APIs; LibraryApi uses Controllers with clean separation.

```csharp
// dotnet-artisan: BookingEndpoints.cs
public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings")
            .WithTags("Bookings");

        group.MapPost("/", async (CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        }).WithSummary("Book a class (enforces all booking rules)");

        return group;
    }
}

// Program.cs stays clean:
app.MapMembershipPlanEndpoints();
app.MapMemberEndpoints();
app.MapBookingEndpoints();
```

### dotnet-webapi
Consistently uses **Minimal APIs with MapGroup()**, union return types (`Results<T1, T2>`), and rich metadata. All three apps follow the same pattern.

```csharp
// dotnet-webapi: MemberEndpoints.cs
public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/members").WithTags("Members");

        group.MapGet("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.GetByIdAsync(id, ct);
            return member is null ? TypedResults.NotFound() : TypedResults.Ok(member);
        })
        .WithName("GetMemberById")
        .Produces<MemberResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
```

**Score**: no-skills: **2** | managedcode: **2** | dotnet-skills: **2** | dotnet-artisan: **5** | dotnet-webapi: **5**

**Verdict**: dotnet-artisan and dotnet-webapi produce modern minimal API architectures with MapGroup, extension methods, and clean Program.cs files. The other three configurations default to traditional Controllers, which have higher overhead and lack the modern route-group organization.

---

## 2. DTO Design [HIGH]

### no-skills
FitnessStudioApi uses positional records but most DTOs are classes with mutable setters. Not sealed. Naming uses `Dto` suffix.

```csharp
// no-skills: FitnessStudioApi uses records
public record MembershipPlanDto(int Id, string Name, string? Description, ...);
public record CreateMembershipPlanDto([Required] string Name, ...);

// no-skills: LibraryApi uses mutable classes
public class BookCreateDto
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;
}
```

### managedcode-dotnet-skills
Records used throughout. Not sealed. Naming is `Dto`/`CreateXxxDto`. Init-only properties.

```csharp
// managedcode: Positional records for responses
public record MemberDto(int Id, string FirstName, string LastName, ...);

// Property-based for create
public record CreateMemberDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;
}
```

### dotnet-skills
Sealed classes (not records) for DTOs. FluentValidation used. Per-entity DTO folders.

```csharp
// dotnet-skills: Sealed classes, not records
public sealed class CreateMemberDto {
    public string FirstName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
```

### dotnet-artisan
**Sealed records** with `Request`/`Response` naming, init-only properties, and data annotations.

```csharp
// dotnet-artisan: Sealed immutable records
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}

public sealed record MemberResponse(
    int Id, string FirstName, string LastName, string Email, ...);
```

### dotnet-webapi
**Sealed records** with `required` keyword, `init` properties, and `Request`/`Response` naming.

```csharp
// dotnet-webapi: Sealed records with required keyword
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }
}

public sealed record MemberResponse(
    int Id, string FirstName, string LastName, ...);
```

**Score**: no-skills: **3** | managedcode: **4** | dotnet-skills: **3** | dotnet-artisan: **5** | dotnet-webapi: **5**

**Verdict**: dotnet-artisan and dotnet-webapi produce the best DTO design — sealed records with Request/Response naming, init-only properties for immutability, and proper data annotations. The `required` keyword in dotnet-webapi adds compile-time initialization enforcement.

---

## 3. Service Abstraction [HIGH]

All configurations use interface + implementation registered with scoped DI. The key differentiators are sealed implementations, primary constructors, and interface organization.

### no-skills
```csharp
// Traditional constructor, not sealed
public class MemberService : IMemberService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MemberService> _logger;
    public MemberService(FitnessDbContext db, ILogger<MemberService> logger) { _db = db; _logger = logger; }
}
```

### dotnet-artisan / dotnet-webapi
```csharp
// Sealed + primary constructor
public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    // No field declarations needed
}
```

**Score**: no-skills: **4** | managedcode: **4** | dotnet-skills: **4** | dotnet-artisan: **5** | dotnet-webapi: **5**

**Verdict**: All configurations implement interface-based DI. dotnet-artisan and dotnet-webapi add sealed implementations with primary constructors, reducing boilerplate.

---

## 4. CancellationToken Propagation [HIGH]

### no-skills
**No CancellationToken parameters anywhere** — zero matches across all 3 apps.

```csharp
// no-skills: MISSING cancellation token
public async Task<PaginatedResult<MemberListDto>> GetAllAsync(int page, int pageSize, string? search, bool? isActive)
{
    var total = await query.CountAsync();  // No ct
    var items = await query.ToListAsync();  // No ct
}
```

### managedcode-dotnet-skills
FitnessStudioApi passes CancellationToken consistently. LibraryApi **omits** it from service methods.

```csharp
// managedcode FitnessStudioApi: ✅ Token propagated
public async Task<PaginatedResponse<MemberListDto>> GetAllAsync(
    string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
{
    var totalCount = await query.CountAsync(ct);
    var items = await query.ToListAsync(ct);
}

// managedcode LibraryApi: ❌ No token
public async Task<PaginatedResponse<AuthorResponse>> GetAuthorsAsync(string? search, int page, int pageSize)
```

### dotnet-skills
Present in LibraryApi and VetClinicApi, **missing in FitnessStudioApi**.

### dotnet-artisan / dotnet-webapi
**Full propagation** from endpoint → service → EF Core in all three apps, with `ct = default` parameter defaults.

```csharp
// dotnet-artisan/dotnet-webapi: Full chain
group.MapGet("/", async (..., CancellationToken ct) =>
{
    var result = await service.GetAllAsync(search, isActive, p, ps, ct);  // ✅
    return TypedResults.Ok(result);
})

public async Task<PaginatedResponse<MemberResponse>> GetAllAsync(..., CancellationToken ct)
{
    var totalCount = await query.CountAsync(ct);      // ✅ to EF Core
    var members = await query.ToListAsync(ct);         // ✅ to EF Core
    await db.SaveChangesAsync(ct);                     // ✅ to EF Core
}
```

**Score**: no-skills: **1** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **5** | dotnet-webapi: **5**

**Verdict**: CancellationToken is critical for production — it prevents wasted server resources. Only dotnet-artisan and dotnet-webapi consistently propagate tokens through every layer.

---

## 5. Middleware Style [HIGH]

### no-skills
Convention-based `RequestDelegate` middleware with try/catch — the older pattern.

```csharp
// no-skills: Traditional middleware
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (BusinessRuleException ex) { ... }
        catch (Exception ex) { ... }
    }
}
// Registered: app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

### managedcode-dotnet-skills
Uses **IExceptionHandler** (modern .NET 8+ pattern) consistently.

```csharp
// managedcode: Modern IExceptionHandler
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title) = exception switch { ... };
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
        return true;
    }
}
// Registered: builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

### dotnet-skills
Mixed — FitnessStudioApi uses IExceptionHandler; LibraryApi and VetClinicApi use traditional middleware.

### dotnet-artisan / dotnet-webapi
**IExceptionHandler** everywhere with `AddProblemDetails()` and `UseStatusCodePages()`.

```csharp
// dotnet-artisan: Sealed IExceptionHandler
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (0, (string?)null)
        };
        // ...
    }
}
```

**Score**: no-skills: **2** | managedcode: **4** | dotnet-skills: **3** | dotnet-artisan: **5** | dotnet-webapi: **5**

**Verdict**: IExceptionHandler is composable, DI-aware, and the modern .NET recommended approach. dotnet-artisan and dotnet-webapi consistently use it.

---

## 6. Exception Handling Strategy [HIGH]

All configurations use custom exception classes. The differentiator is richness and semantic precision.

### no-skills
`BusinessRuleException` with StatusCode + Title properties, plus `NotFoundException`.

### managedcode-dotnet-skills
Catches `KeyNotFoundException`, `InvalidOperationException`, `FluentValidation.ValidationException`. Some apps define `BusinessRuleException`, others rely on built-in types.

### dotnet-skills
`BusinessRuleException` with configurable StatusCode and Title — the richest exception design.

```csharp
// dotnet-skills: Rich exception
public sealed class BusinessRuleException : Exception {
    public int StatusCode { get; }
    public string Title { get; }
    public BusinessRuleException(string message, int statusCode = 400, string title = "Business Rule Violation")
        : base(message) { StatusCode = statusCode; Title = title; }
}
```

### dotnet-artisan / dotnet-webapi
Map built-in exception types to status codes via pattern matching. Uses `InvalidOperationException` for business rules, `KeyNotFoundException` for 404s, `ArgumentException` for validation.

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **4** | dotnet-artisan: **4** | dotnet-webapi: **4**

**Verdict**: dotnet-skills, dotnet-artisan, and dotnet-webapi all produce good exception strategies. dotnet-skills' rich BusinessRuleException with embedded status codes is particularly elegant.

---

## 7. EF Core Relationship Configuration [HIGH]

All configurations use Fluent API in `OnModelCreating`. The key differentiators are explicit cascade behavior, enum conversions, and unique indexes.

### no-skills
Fluent API with explicit cascade behavior but inconsistent — some relationships use Cascade, some rely on defaults.

### dotnet-artisan / dotnet-webapi
Comprehensive Fluent API with explicit `OnDelete`, `HasConversion<string>()` for enums, unique indexes, decimal precision, and filtered indexes.

```csharp
// dotnet-webapi: Comprehensive EF configuration
modelBuilder.Entity<Membership>(entity =>
{
    entity.Property(e => e.Status).HasConversion<string>();
    entity.HasOne(e => e.Member).WithMany(m => m.Memberships)
        .HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(e => e.MembershipPlan).WithMany(p => p.Memberships)
        .HasForeignKey(e => e.MembershipPlanId).OnDelete(DeleteBehavior.Restrict);
});
```

**Score**: no-skills: **4** | managedcode: **4** | dotnet-skills: **4** | dotnet-artisan: **5** | dotnet-webapi: **5**

**Verdict**: dotnet-artisan and dotnet-webapi consistently configure explicit delete behavior, enum conversions, and unique indexes for all entities.

---

## 8. Async/Await Best Practices [HIGH]

### no-skills
Proper async/await but **synchronous DataSeeder** in some apps. No CancellationToken. Async suffix used inconsistently.

### dotnet-artisan / dotnet-webapi
All methods use proper async/await with `Async` suffix. No sync-over-async. DataSeeder is async.

```csharp
// dotnet-webapi: Correct async pattern
public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct)
{
    var emailExists = await db.Members.AnyAsync(m => m.Email == request.Email, ct);
    db.Members.Add(member);
    await db.SaveChangesAsync(ct);
    return MapToResponse(member, null);
}
```

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **5** | dotnet-webapi: **5**

**Verdict**: dotnet-artisan and dotnet-webapi follow all async best practices including async seeding, consistent Async suffix, and token propagation.

---

## 9. Error Response Conformance [HIGH]

All configurations return ProblemDetails. The differentiators are status code variety and field richness.

### no-skills
ProblemDetails with RFC 7807 `type` field. Status codes: 400, 404, 409, 500.

### dotnet-artisan / dotnet-webapi
ProblemDetails with `traceId` extension, `Instance` path, and proper status mapping.

```csharp
// dotnet-artisan: ProblemDetails with traceId
var problemDetails = new ProblemDetails
{
    Status = statusCode, Title = title, Detail = exception.Message,
    Instance = httpContext.Request.Path
};
problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
```

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **4** | dotnet-artisan: **4** | dotnet-webapi: **4**

---

## 10. Endpoint Completeness [CRITICAL]

All configurations implement the vast majority of specified endpoints. The FitnessStudioApi specification requires ~35 endpoints, LibraryApi ~33, VetClinicApi ~28.

### no-skills
Implements ~41 FitnessStudioApi endpoints, ~40 LibraryApi, ~35 VetClinicApi. Some sub-resource endpoints may be combined.

### dotnet-artisan / dotnet-webapi
All specified endpoints implemented with comprehensive sub-resource routes and correct HTTP method mapping.

**Score**: no-skills: **4** | managedcode: **4** | dotnet-skills: **4** | dotnet-artisan: **5** | dotnet-webapi: **5**

---

## 11. Business Rule Implementation [CRITICAL]

### no-skills
Enforces core rules (booking window, membership tier, capacity, waitlist promotion, cancellation cascade). All 12 FitnessStudioApi rules present.

### dotnet-artisan / dotnet-webapi
All business rules implemented with ISO week calculations, overlap detection, and check-in/no-show windows.

```csharp
// dotnet-artisan: ISO week calculation for weekly booking limits
var weekStart = GetIsoWeekStart(now);
var weekEnd = weekStart.AddDays(7);
var weeklyBookings = await db.Bookings.CountAsync(b =>
    b.MemberId == request.MemberId &&
    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
    b.ClassSchedule.StartTime >= weekStart &&
    b.ClassSchedule.StartTime < weekEnd, ct);
```

**Score**: no-skills: **4** | managedcode: **4** | dotnet-skills: **4** | dotnet-artisan: **5** | dotnet-webapi: **5**

---

## 12. Input Validation Coverage [CRITICAL]

### no-skills / managedcode
Data annotations on DTOs. No FluentValidation.

### dotnet-skills
FluentValidation with auto-validation middleware and 13+ validators including custom business rules (age validation).

```csharp
// dotnet-skills: FluentValidation with business rules
public sealed class CreateMemberValidator : AbstractValidator<CreateMemberDto> {
    public CreateMemberValidator() {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).Must(dob => { ... return age >= 16; })
            .WithMessage("Member must be at least 16 years old.");
    }
}
```

### dotnet-artisan / dotnet-webapi
Data annotations on sealed record DTOs. Validation integrated via ASP.NET Core model binding.

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **4** | dotnet-artisan: **4** | dotnet-webapi: **4**

---

## 13. Security Vulnerability Scan [CRITICAL]

All configurations use similar minimal NuGet package sets. No known vulnerable packages detected. dotnet-artisan and dotnet-webapi avoid Swashbuckle (which has had historical vulnerabilities) by using built-in `Microsoft.AspNetCore.OpenApi`.

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **4** | dotnet-webapi: **4**

---

## 14. Sealed Types [MEDIUM]

### no-skills
**Zero sealed classes or records** across all 3 apps.

### managedcode-dotnet-skills
**Not sealed** — records used but `sealed` modifier absent.

### dotnet-skills
Sealed classes on controllers, services, exceptions, and some DTOs. Partial coverage.

### dotnet-artisan / dotnet-webapi
**All types sealed** — entities, services, DTOs, middleware, even DbContext.

```csharp
// dotnet-webapi: Everything sealed
public sealed class Member { ... }
public sealed record CreateMemberRequest { ... }
public sealed class MemberService(...) : IMemberService { ... }
internal sealed class ApiExceptionHandler(...) : IExceptionHandler { ... }
public sealed class FitnessDbContext(...) : DbContext(...) { ... }
```

**Score**: no-skills: **1** | managedcode: **1** | dotnet-skills: **4** | dotnet-artisan: **5** | dotnet-webapi: **5**

**Verdict**: Sealed types enable JIT devirtualization and signal design intent. Only dotnet-artisan and dotnet-webapi consistently apply the modifier everywhere.

---

## 15. AsNoTracking Usage [MEDIUM]

### no-skills
**Zero AsNoTracking calls** — all queries track entities unnecessarily.

### managedcode-dotnet-skills
Used consistently in FitnessStudioApi read queries but inconsistent across apps.

### dotnet-skills
Used consistently in all read-only queries across apps.

### dotnet-artisan / dotnet-webapi
**Consistent AsNoTracking** on every read-only query, omitted correctly on write operations.

```csharp
// dotnet-artisan: Consistent pattern
var query = db.Members.AsNoTracking().AsQueryable();  // Read-only
var member = await db.Members.FindAsync([id], ct);     // Write path - no AsNoTracking
```

**Score**: no-skills: **1** | managedcode: **4** | dotnet-skills: **4** | dotnet-artisan: **5** | dotnet-webapi: **5**

---

## 16. Return Type Precision [MEDIUM]

### no-skills
`IEnumerable<T>` and `List<T>` — never `IReadOnlyList<T>`.

### managedcode-dotnet-skills
`IReadOnlyList<T>` in FitnessStudioApi's PaginatedResponse. LibraryApi uses `List<T>`.

### dotnet-skills
Mixed: LibraryApi uses `IReadOnlyList<T>`, FitnessStudioApi uses `List<T>`, VetClinicApi uses `IEnumerable<T>`.

### dotnet-artisan / dotnet-webapi
**Consistent `IReadOnlyList<T>`** in PaginatedResponse and service return types. `ICollection<T>` for EF entity navigation properties.

```csharp
// dotnet-webapi: IReadOnlyList in DTOs, ICollection in entities
public sealed record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
}

public sealed class Member
{
    public ICollection<Booking> Bookings { get; set; } = [];
}
```

**Score**: no-skills: **2** | managedcode: **3** | dotnet-skills: **2** | dotnet-artisan: **5** | dotnet-webapi: **5**

---

## 17. Data Seeder Design [MEDIUM]

### no-skills
Mixed: FitnessStudioApi uses injectable async seeder; LibraryApi uses static synchronous seeder.

### dotnet-artisan
Static async seeder with idempotent guard clause.

### dotnet-webapi
**Injectable DI-registered seeder** with async operations.

```csharp
// dotnet-webapi: Injectable seeder
builder.Services.AddScoped<DataSeeder>();
var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
await seeder.SeedAsync();
```

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **4** | dotnet-webapi: **4**

---

## 18. File Organization [MEDIUM]

### no-skills
Per-concern folders. FitnessStudioApi puts **all DTOs in one file** (Dtos.cs) and all interfaces in one file (Interfaces.cs).

### dotnet-skills
Per-entity DTO folders, per-entity enum files, interfaces in separate `Interfaces/` directory.

### dotnet-artisan / dotnet-webapi
Per-concern folders (Endpoints/, Services/, DTOs/, Models/) with per-entity files within each folder.

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **4** | dotnet-artisan: **4** | dotnet-webapi: **4**

---

## 19. Pagination [MEDIUM]

### no-skills
Positional record with pre-computed TotalPages (not a computed property).

```csharp
// no-skills: TotalPages passed as parameter
public record PaginatedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
```

### dotnet-artisan / dotnet-webapi
**Sealed record** with computed properties, factory method, and IReadOnlyList.

```csharp
// dotnet-webapi: Sealed with factory + computed properties
public sealed record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int TotalPages { get; init; }
    public required bool HasNextPage { get; init; }
    public required bool HasPreviousPage { get; init; }

    public static PaginatedResponse<T> Create(
        IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<T> { Items = items, TotalPages = totalPages, ... };
    }
}
```

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **5** | dotnet-webapi: **5**

---

## 20. OpenAPI Metadata [MEDIUM]

### no-skills
XML `<summary>` docs + `[ProducesResponseType]` attributes. Swashbuckle configuration.

### dotnet-artisan / dotnet-webapi
`WithName()`, `WithSummary()`, `WithDescription()`, `Produces<T>()`, `WithTags()` on minimal API endpoints. Automatic schema inference from TypedResults.

```csharp
// dotnet-webapi: Rich OpenAPI metadata
group.MapGet("/", async (...) => { ... })
    .WithName("GetMembers")
    .WithSummary("List members")
    .WithDescription("List members with optional search by name/email, filter by active status, and pagination.")
    .Produces<PaginatedResponse<MemberResponse>>();
```

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **5** | dotnet-webapi: **5**

---

## 21. Structured Logging [MEDIUM]

All configurations use `ILogger<T>` with structured message templates. No configuration uses `[LoggerMessage]` source generators.

### no-skills
Structured templates used. ILogger<T> injected via traditional constructors.

### dotnet-artisan / dotnet-webapi
Structured templates with named placeholders, proper log levels, exception context.

```csharp
// dotnet-artisan: Structured logging
logger.LogInformation("Booking {BookingId} created for member {MemberId} in class {ClassId} - Status: {Status}",
    booking.Id, member.Id, schedule.Id, booking.Status);
```

**Score**: no-skills: **4** | managedcode: **3** | dotnet-skills: **4** | dotnet-artisan: **5** | dotnet-webapi: **5**

---

## 22. Nullable Reference Types [MEDIUM]

All configurations enable `<Nullable>enable</Nullable>` and use proper annotations (`string?`, `= null!` for EF navigation properties).

**Score**: no-skills: **4** | managedcode: **4** | dotnet-skills: **4** | dotnet-artisan: **4** | dotnet-webapi: **4**

---

## 23. Package Discipline [MEDIUM]

### no-skills
3–4 packages. Uses Swashbuckle instead of built-in OpenAPI.

### dotnet-artisan / dotnet-webapi
3–4 packages. Uses `Microsoft.AspNetCore.OpenApi` (built-in) instead of Swashbuckle. EF Design package properly scoped with `PrivateAssets=all`.

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **4** | dotnet-webapi: **4**

---

## 24. TypedResults Usage [MEDIUM]

### no-skills / managedcode / dotnet-skills
Controllers return `IActionResult` — no TypedResults.

### dotnet-artisan / dotnet-webapi
**TypedResults consistently** with `Results<T1, T2>` union return types for compile-time safety.

```csharp
// dotnet-webapi: Union return types
group.MapGet("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (
    int id, IMemberService service, CancellationToken ct) =>
{
    var member = await service.GetByIdAsync(id, ct);
    return member is null ? TypedResults.NotFound() : TypedResults.Ok(member);
})
```

**Score**: no-skills: **1** | managedcode: **1** | dotnet-skills: **1** | dotnet-artisan: **5** | dotnet-webapi: **5**

**Verdict**: TypedResults provide compile-time safety and automatic OpenAPI schema generation. Only available with minimal APIs, which only dotnet-artisan and dotnet-webapi use.

---

## 25. HTTP Test File Quality [MEDIUM]

All configurations produce `.http` files with base URL variables, section headers, and endpoint coverage. dotnet-artisan and dotnet-webapi include business rule test scenarios.

**Score**: no-skills: **3** | managedcode: **4** | dotnet-skills: **4** | dotnet-artisan: **4** | dotnet-webapi: **4**

---

## 26. Enum Design [MEDIUM]

All use singular enum names. Differentiator is `HasConversion<string>()` usage.

### no-skills
Enums defined but no `HasConversion<string>()` — stored as integers in SQLite.

### dotnet-skills / dotnet-artisan / dotnet-webapi
`HasConversion<string>()` configured in Fluent API + `JsonStringEnumConverter` for API serialization.

**Score**: no-skills: **3** | managedcode: **4** | dotnet-skills: **4** | dotnet-artisan: **4** | dotnet-webapi: **4**

---

## 27. Guard Clauses & Argument Validation [MEDIUM]

All configurations use `?? throw` pattern for null checks. None use `ArgumentNullException.ThrowIfNull()` or other modern guard APIs on constructor parameters.

```csharp
// All configurations use this pattern:
var member = await db.Members.FindAsync([id], ct)
    ?? throw new KeyNotFoundException($"Member with ID {id} not found.");
```

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **3** | dotnet-webapi: **3**

---

## 28. Dispose & Resource Management [MEDIUM]

All configurations use `using` blocks for service scope creation. DbContext is DI-managed. No manual disposal needed or implemented.

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **3** | dotnet-webapi: **3**

---

## 29. Code Standards Compliance [LOW]

### no-skills
PascalCase used. File-scoped namespaces. Some services not sealed. No explicit access modifiers on some members.

### dotnet-artisan / dotnet-webapi
Consistent PascalCase, file-scoped namespaces, `Async` suffix on all async methods, explicit access modifiers, `internal sealed` on middleware handlers.

**Score**: no-skills: **3** | managedcode: **3** | dotnet-skills: **4** | dotnet-artisan: **5** | dotnet-webapi: **5**

---

## 30. Modern C# Adoption [LOW]

| Feature | no-skills | managedcode | dotnet-skills | dotnet-artisan | dotnet-webapi |
|---|---|---|---|---|---|
| Primary constructors | ❌ Partial | ✅ Services | ⚠️ Mixed | ✅ All | ✅ All |
| Collection expressions `[]` | ❌ `new List<T>()` | ✅ Some | ❌ Partial | ✅ All | ✅ All |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ | ✅ |
| Records for DTOs | ⚠️ Partial | ✅ | ❌ Classes | ✅ Sealed | ✅ Sealed |
| `required` keyword | ❌ | ❌ | ❌ | ❌ | ✅ |
| `init` properties | ❌ | ✅ | ❌ | ✅ | ✅ |
| ImplicitUsings | ✅ | ✅ | ✅ | ✅ | ✅ |

**Score**: no-skills: **2** | managedcode: **3** | dotnet-skills: **3** | dotnet-artisan: **5** | dotnet-webapi: **5**

---

## Weighted Summary

Weights: **Critical × 3**, **High × 2**, **Medium × 1**, **Low × 0.5**

| Dimension | Tier | Weight | no-skills | managedcode | dotnet-skills | dotnet-artisan | dotnet-webapi |
|---|---|---|---|---|---|---|---|
| Endpoint Completeness | CRITICAL | 3 | 12 | 12 | 12 | 15 | 15 |
| Business Rule Implementation | CRITICAL | 3 | 12 | 12 | 12 | 15 | 15 |
| Input Validation Coverage | CRITICAL | 3 | 9 | 9 | 12 | 12 | 12 |
| Security Vulnerability Scan | CRITICAL | 3 | 9 | 9 | 9 | 12 | 12 |
| API Architecture | HIGH | 2 | 4 | 4 | 4 | 10 | 10 |
| DTO Design | HIGH | 2 | 6 | 8 | 6 | 10 | 10 |
| Service Abstraction | HIGH | 2 | 8 | 8 | 8 | 10 | 10 |
| CancellationToken Propagation | HIGH | 2 | 2 | 6 | 6 | 10 | 10 |
| Middleware Style | HIGH | 2 | 4 | 8 | 6 | 10 | 10 |
| Exception Handling Strategy | HIGH | 2 | 6 | 6 | 8 | 8 | 8 |
| EF Core Relationships | HIGH | 2 | 8 | 8 | 8 | 10 | 10 |
| Async/Await Best Practices | HIGH | 2 | 6 | 6 | 6 | 10 | 10 |
| Error Response Conformance | HIGH | 2 | 6 | 6 | 8 | 8 | 8 |
| Sealed Types | MEDIUM | 1 | 1 | 1 | 4 | 5 | 5 |
| AsNoTracking Usage | MEDIUM | 1 | 1 | 4 | 4 | 5 | 5 |
| Return Type Precision | MEDIUM | 1 | 2 | 3 | 2 | 5 | 5 |
| Data Seeder Design | MEDIUM | 1 | 3 | 3 | 3 | 4 | 4 |
| File Organization | MEDIUM | 1 | 3 | 3 | 4 | 4 | 4 |
| Pagination | MEDIUM | 1 | 3 | 3 | 3 | 5 | 5 |
| OpenAPI Metadata | MEDIUM | 1 | 3 | 3 | 3 | 5 | 5 |
| Structured Logging | MEDIUM | 1 | 4 | 3 | 4 | 5 | 5 |
| Nullable Reference Types | MEDIUM | 1 | 4 | 4 | 4 | 4 | 4 |
| Package Discipline | MEDIUM | 1 | 3 | 3 | 3 | 4 | 4 |
| TypedResults Usage | MEDIUM | 1 | 1 | 1 | 1 | 5 | 5 |
| HTTP Test File Quality | MEDIUM | 1 | 3 | 4 | 4 | 4 | 4 |
| Enum Design | MEDIUM | 1 | 3 | 4 | 4 | 4 | 4 |
| Guard Clauses | MEDIUM | 1 | 3 | 3 | 3 | 3 | 3 |
| Dispose & Resource Mgmt | MEDIUM | 1 | 3 | 3 | 3 | 3 | 3 |
| Code Standards Compliance | LOW | 0.5 | 1.5 | 1.5 | 2 | 2.5 | 2.5 |
| Modern C# Adoption | LOW | 0.5 | 1 | 1.5 | 1.5 | 2.5 | 2.5 |
| | | | | | | | |
| **TOTAL** | | | **134.5** | **150.0** | **162.5** | **213.0** | **213.0** |

### Rankings

| Rank | Configuration | Weighted Score | % of Maximum (245) |
|---|---|---|---|
| 🥇 1 (tie) | **dotnet-artisan** | **213.0** | 86.9% |
| 🥇 1 (tie) | **dotnet-webapi** | **213.0** | 86.9% |
| 🥉 3 | **dotnet-skills** | **162.5** | 66.3% |
| 4 | **managedcode-dotnet-skills** | **150.0** | 61.2% |
| 5 | **no-skills** | **134.5** | 54.9% |

---

## What All Versions Get Right

- ✅ **Interface-based service abstraction** with scoped DI lifetime — all 5 configurations use `AddScoped<IService, Service>()`
- ✅ **EF Core Fluent API** for relationship configuration with explicit foreign keys and unique indexes
- ✅ **ProblemDetails error responses** — all produce RFC 7807-compliant error payloads
- ✅ **Structured logging** with `ILogger<T>` and named message template placeholders
- ✅ **Nullable reference types enabled** (`<Nullable>enable</Nullable>`) with proper annotations
- ✅ **Comprehensive seed data** — all generate idempotent seeders with realistic, relationally consistent demo data
- ✅ **OpenAPI documentation** — all configure Swagger UI accessible at root or `/swagger`
- ✅ **Enum types for domain concepts** — all define enums for status fields like `BookingStatus`, `LoanStatus`, `AppointmentStatus`
- ✅ **File-scoped namespaces** — universal adoption of `namespace X;` syntax
- ✅ **Business rule enforcement** — all implement the core business rules (capacity management, booking windows, membership tiers, waitlist promotion)
- ✅ **.http test files** — all generate endpoint test files with base URL variables and section organization
- ✅ **Pagination** — all implement Skip/Take with total count metadata

---

## Summary: Impact of Skills

### Most Impactful Differences (ranked by weighted score delta)

1. **API Architecture** (+6 weighted points for artisan/webapi) — Minimal APIs with MapGroup produce cleaner, more maintainable code than Controllers. This is the single most visible architectural difference.

2. **CancellationToken Propagation** (+8 weighted points) — The no-skills baseline completely lacks CancellationToken support, which is a critical production concern. This is the largest gap in the baseline.

3. **TypedResults & Sealed Types** (+4 and +4 weighted points) — Exclusive to configurations using minimal APIs. Provide compile-time safety, JIT optimization, and automatic OpenAPI schema generation.

4. **AsNoTracking Usage** (+4 weighted points) — The baseline never calls AsNoTracking, doubling memory usage on every read query. All skill-enhanced configurations fix this.

5. **Modern C# Adoption** (+1.5 weighted points) — Primary constructors, collection expressions, sealed records, init-only properties, and the `required` keyword are consistently used only by dotnet-artisan and dotnet-webapi.

### Overall Assessment

| Configuration | Character | Summary |
|---|---|---|
| **no-skills** | Competent baseline | Produces working, well-structured code but misses modern patterns: no CancellationToken, no AsNoTracking, no sealed types, Controllers-only. Suitable for prototypes. |
| **managedcode-dotnet-skills** | Incremental improvement | Adds IExceptionHandler, AsNoTracking, partial CancellationToken, and records. Still uses Controllers. A modest upgrade over baseline. |
| **dotnet-skills** | Quality-focused | Adds FluentValidation, sealed classes, richer exception types, per-entity file organization. Still Controller-based. Best exception design of all configs. |
| **dotnet-artisan** | Production-grade | Minimal APIs, sealed types everywhere, full CancellationToken chain, TypedResults, IReadOnlyList, rich OpenAPI metadata. Tied for best overall quality. |
| **dotnet-webapi** | Production-grade | Nearly identical to dotnet-artisan. Adds `required` keyword on DTOs. Uses injectable seeders. Tied for best overall quality. |

**The dotnet-artisan and dotnet-webapi skills produce code that is approximately 58% higher quality than the baseline** (213 vs 134.5 weighted points), with the most impactful improvements in API architecture, async patterns, and type safety. The official dotnet-skills configuration provides a meaningful 21% improvement over baseline but remains Controller-based.
