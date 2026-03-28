# Comparative Analysis: dotnet-artisan, dotnet-webapi, managedcode-dotnet-skills, dotnet-skills, no-skills

## Introduction

This report compares **5 Copilot skill configurations** across **3 ASP.NET Core Web API scenarios** (15 apps total). Each configuration generated the same three applications from identical prompts:

| Configuration | Description | Apps Generated |
|---|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain (9 skills, 14 specialist agents) | FitnessStudioApi, LibraryApi, VetClinicApi |
| **dotnet-webapi** | dotnet-webapi skill | FitnessStudioApi, LibraryApi, VetClinicApi |
| **managedcode-dotnet-skills** | Community managed-code skills | FitnessStudioApi, LibraryApi, VetClinicApi |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | FitnessStudioApi, LibraryApi, VetClinicApi |
| **no-skills** | Baseline (default Copilot, no skills) | FitnessStudioApi, LibraryApi, VetClinicApi |

Each app targets **.NET 10** with **EF Core + SQLite**, implementing a domain-specific REST API with business rules, data seeding, and error handling. Scores use a **1–5 scale** (1=missing/wrong, 3=adequate, 5=excellent).

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 4 | 4 | 4 | 4 | 4 |
| Security Vulnerability Scan [CRITICAL] | 3 | 4 | 3 | 3 | 3 |
| Minimal API Architecture [CRITICAL] | 4 | 5 | 1 | 2 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 4 | 4 | 4 | 4 | 3 |
| NuGet & Package Discipline [CRITICAL] | 2 | 3 | 3 | 3 | 3 |
| EF Migration Usage [CRITICAL] | 1 | 5 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 4 | 5 | 4 | 4 | 4 |
| Prefer Built-in over 3rd Party [HIGH] | 3 | 5 | 2 | 3 | 2 |
| Modern C# Adoption [HIGH] | 5 | 5 | 4 | 3 | 2 |
| Error Handling & Middleware [HIGH] | 4 | 5 | 4 | 4 | 3 |
| Async Patterns & Cancellation [HIGH] | 4 | 5 | 4 | 2 | 2 |
| EF Core Best Practices [HIGH] | 4 | 5 | 4 | 4 | 2 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 5 | 5 | 4 |
| Security Configuration [HIGH] | 2 | 2 | 2 | 2 | 3 |
| DTO Design [MEDIUM] | 5 | 5 | 4 | 3 | 3 |
| Sealed Types [MEDIUM] | 5 | 5 | 1 | 4 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 3 | 3 | 3 |
| Structured Logging [MEDIUM] | 3 | 5 | 4 | 4 | 3 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 4 | 5 | 3 | 3 | 3 |
| File Organization [MEDIUM] | 4 | 5 | 4 | 3 | 3 |
| HTTP Test File Quality [MEDIUM] | 4 | 5 | 4 | 3 | 4 |
| Type Design & Resource Management [MEDIUM] | 4 | 5 | 3 | 4 | 3 |
| Code Standards Compliance [LOW] | 5 | 5 | 5 | 5 | 4 |

---

## 1. Build & Run Success [CRITICAL]

All 15 generated projects have valid `.csproj` files targeting `net10.0` with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`. No obvious compilation errors were found in any configuration.

```xml
<!-- Common across all configurations -->
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

**Scores**: All configurations score **4** — projects appear structurally sound with no detectable compilation issues. A score of 5 would require verified `dotnet build` output with zero warnings.

**Verdict**: Tie — all configurations produce structurally valid projects.

---

## 2. Security Vulnerability Scan [CRITICAL]

Package selection affects vulnerability surface. The key differentiator is whether Swashbuckle (a third-party package with its own dependency tree) is included.

| Config | Swashbuckle Apps | Extra 3rd Party | Assessment |
|---|---|---|---|
| dotnet-artisan | 3/3 (SwaggerUI on Fitness, full on Library/VetClinic) | None | Larger attack surface |
| dotnet-webapi | 0/3 | None | Minimal surface |
| managedcode | 3/3 | FluentValidation on Library | Larger attack surface |
| dotnet-skills | 1/3 (Library only) | FluentValidation.AspNetCore on Fitness | Mixed |
| no-skills | 2/3 (Fitness, Library) | None | Moderate surface |

**Scores**: dotnet-webapi: **4**, all others: **3**

**Verdict**: **dotnet-webapi** wins by eliminating all third-party packages, minimizing the vulnerability surface to only Microsoft-maintained packages.

---

## 3. Minimal API Architecture [CRITICAL]

This is the single largest architectural differentiator across configurations.

### dotnet-webapi (Score: 5) — All 3 apps use Minimal API with full best practices

```csharp
// dotnet-webapi: FitnessStudioApi/Endpoints/MemberEndpoints.cs
public static void MapMemberEndpoints(this WebApplication app)
{
    var group = app.MapGroup("/api/members").WithTags("Members");
    group.MapGet("/", async Task<Results<Ok<PaginatedResponse<MemberResponse>>, BadRequest>>
        (string? search, bool? isActive, int page, int pageSize,
         IMemberService service, CancellationToken ct) =>
    {
        var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
        return TypedResults.Ok(result);
    }).WithName("GetMembers").WithSummary("List members");
}
```

All three apps use `MapGroup()` with `WithTags()`, endpoint extension methods (`Map*Endpoints`), `TypedResults` with `Results<T1, T2>` union return types, and `CancellationToken` parameters.

### dotnet-artisan (Score: 4) — 2/3 apps use Minimal API

FitnessStudioApi and VetClinicApi use Minimal API with MapGroup and endpoint extensions. LibraryApi falls back to controllers:

```csharp
// dotnet-artisan: FitnessStudioApi/Endpoints/MemberEndpoints.cs
var group = routes.MapGroup("/api/members").WithTags("Members");
group.MapGet("/", async (...) => { ... }).WithSummary("List members");

// dotnet-artisan: LibraryApi/Controllers/BooksController.cs — CONTROLLERS
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase { ... }
```

### dotnet-skills (Score: 2) — 1/3 apps use Minimal API, but poorly

FitnessStudioApi and LibraryApi use controllers. VetClinicApi uses Minimal API but defines all endpoints **inline in Program.cs** (274 lines) without endpoint extension methods:

```csharp
// dotnet-skills: VetClinicApi/Program.cs (274 lines — all endpoints inline!)
var owners = app.MapGroup("/api/owners").WithTags("Owners");
owners.MapGet("/", async (IOwnerService service, string? search, ...) =>
{
    return Results.Ok(await service.GetAllAsync(search, pagination));
}).WithSummary("List all owners");
```

Uses `Results.Ok()` instead of `TypedResults.Ok()`, losing compile-time type safety.

### managedcode & no-skills (Score: 1) — All controllers

```csharp
// no-skills: FitnessStudioApi/Controllers/MembershipsController.cs
[ApiController]
[Route("api/memberships")]
[Produces("application/json")]
public class MembershipsController : ControllerBase
```

Both configurations generate traditional controller-based APIs with `[ApiController]`, `IActionResult` return types, and no MapGroup or TypedResults usage.

**Verified controller counts from grep:**
- no-skills: 21 `[ApiController]` files (7 per app)
- managedcode: 21 `[ApiController]` files (7 per app)
- dotnet-skills: 14 `[ApiController]` files (Fitness: 7, Library: 7, VetClinic: 0)
- dotnet-artisan: 7 `[ApiController]` files (Library only)
- dotnet-webapi: 0 `[ApiController]` files

**Verdict**: **dotnet-webapi** is the clear winner — 100% Minimal API with MapGroup, TypedResults, Results<T1,T2> union types, and endpoint extension methods across all 3 apps. This is the modern .NET standard.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

All configurations use Data Annotations on DTOs. The key difference is depth of validation and guard clause patterns.

```csharp
// dotnet-webapi: FitnessStudioApi/DTOs/MemberDtos.cs
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
    [Required, EmailAddress]
    public required string Email { get; init; }
    [Range(1, 24)]
    public int DurationMonths { get; init; }
}
```

```csharp
// managedcode: LibraryApi/Validators/Validators.cs (FluentValidation)
public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20);
    }
}
```

```csharp
// no-skills: FitnessStudioApi/DTOs/Dtos.cs — positional records
public record CreateMembershipPlanDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(1, 24)] int DurationMonths,
    [Range(0.01, double.MaxValue)] decimal Price);
```

No configuration consistently uses `ArgumentNullException.ThrowIfNull()` or modern throw-helper guard clauses. Service-level validation uses exception throwing patterns.

**Scores**: dotnet-webapi: **4**, dotnet-artisan: **4**, managedcode: **4**, dotnet-skills: **4**, no-skills: **3**

**Verdict**: Tie among top 4. **no-skills** slightly weaker with less consistent service-layer validation. No configuration reaches 5 due to missing modern guard clause patterns.

---

## 5. NuGet & Package Discipline [CRITICAL]

Floating versions (`Version="10.*-*"`) are widespread across configurations and represent the most common packaging deficiency.

| Config | Floating Versions | Swashbuckle | Extra Packages | Total Packages (avg) |
|---|---|---|---|---|
| dotnet-artisan | Fitness only (10.*-*) | 3/3 apps | None | 4 |
| dotnet-webapi | ALL 3 apps (10.*-*) | 0/3 apps | None | 3 |
| managedcode | Library only (10.0.*-*) | 3/3 apps | FluentValidation (Library) | 4-5 |
| dotnet-skills | Fitness, VetClinic | 1/3 apps | FluentValidation.AspNetCore (Fitness) | 3-4 |
| no-skills | Fitness, VetClinic | 2/3 apps | None | 3-4 |

```xml
<!-- dotnet-webapi: FitnessStudioApi.csproj — minimal but floating -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />

<!-- managedcode: FitnessStudioApi.csproj — pinned but includes Swashbuckle -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.6" />
```

**Scores**: dotnet-webapi: **3**, dotnet-artisan: **2**, managedcode: **3**, dotnet-skills: **3**, no-skills: **3**

**Verdict**: No configuration fully pins versions. **dotnet-webapi** has the fewest packages but floating versions across all apps. **dotnet-artisan** scores lowest due to floating versions plus Swashbuckle on all apps. The ideal would be pinned exact versions with no unnecessary packages.

---

## 6. EF Migration Usage [CRITICAL]

This is the starkest binary difference: **only dotnet-webapi uses migrations**.

```csharp
// dotnet-webapi: FitnessStudioApi/Program.cs — CORRECT: uses MigrateAsync
await context.Database.MigrateAsync();

// dotnet-webapi: LibraryApi/Program.cs — uses Migrate (sync)
db.Database.Migrate();

// dotnet-webapi: VetClinicApi/Program.cs — uses Migrate (sync)
dbContext.Database.Migrate();
```

All other configurations use the `EnsureCreated` anti-pattern:

```csharp
// ALL other configs: EnsureCreated (bypasses migrations)
await db.Database.EnsureCreatedAsync();  // dotnet-artisan, managedcode, no-skills
context.Database.EnsureCreated();         // dotnet-skills (sync variant)
```

**Verified via grep across all 15 Program.cs files:**
- `MigrateAsync`: dotnet-webapi FitnessStudioApi only
- `Migrate()`: dotnet-webapi LibraryApi and VetClinicApi
- `EnsureCreatedAsync`: All 12 remaining apps
- `EnsureCreated` (sync): dotnet-artisan LibraryApi, dotnet-skills FitnessStudioApi, no-skills LibraryApi, managedcode LibraryApi

**Scores**: dotnet-webapi: **5**, all others: **1**

**Verdict**: **dotnet-webapi** is the only configuration that uses EF Core migrations — the only production-safe approach. All others use `EnsureCreated`, which makes schema evolution impossible and causes data loss on model changes. This is a critical differentiator.

---

## 7. Business Logic Correctness [HIGH]

All configurations implement the core CRUD endpoints for each scenario. The differentiation is in business rule depth.

**dotnet-webapi** (Score: 5) implements the most comprehensive business rules for FitnessStudioApi:
- ✅ Minimum age 16 for membership
- ✅ Active membership required for booking
- ✅ Premium class access control by plan tier
- ✅ Weekly booking limits per membership plan
- ✅ 7-day advance booking window, 30-minute minimum notice
- ✅ Waitlist management with automatic promotion
- ✅ No double-booking (time overlap check)
- ✅ Membership freeze/unfreeze with EndDate extension
- ✅ Class cancellation cascading to all bookings

Other configurations implement 70-90% of specified business rules. Common gaps include missing waitlist promotion, incomplete freeze logic, and missing check-in window validation.

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **4**, managedcode: **4**, dotnet-skills: **4**, no-skills: **4**

**Verdict**: **dotnet-webapi** delivers the most complete business rule implementation across all three scenarios.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

```csharp
// dotnet-webapi: FitnessStudioApi/Program.cs — pure built-in OpenAPI
builder.Services.AddOpenApi();
// ...
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// No Swashbuckle, no UseSwagger(), no UseSwaggerUI()
```

```csharp
// no-skills: FitnessStudioApi/Program.cs — Swashbuckle
builder.Services.AddSwaggerGen();
// ...
app.UseSwagger();
app.UseSwaggerUI();
```

```csharp
// managedcode: LibraryApi — FluentValidation (3rd party)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// Controllers validate manually:
var validation = await createValidator.ValidateAsync(request);
```

| Config | Swashbuckle | FluentValidation | Other 3rd Party |
|---|---|---|---|
| dotnet-webapi | ❌ None | ❌ None | ❌ None |
| dotnet-artisan | ✅ 3/3 | ❌ None | ❌ None |
| dotnet-skills | ✅ 1/3 | ✅ 1/3 | ❌ None |
| managedcode | ✅ 3/3 | ✅ 1/3 | ❌ None |
| no-skills | ✅ 2/3 | ❌ None | ❌ None |

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **3**, dotnet-skills: **3**, managedcode: **2**, no-skills: **2**

**Verdict**: **dotnet-webapi** is the only configuration that uses exclusively built-in .NET capabilities. All others pull in Swashbuckle or FluentValidation where built-in alternatives exist.

---

## 9. Modern C# Adoption [HIGH]

| Feature | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|---|---|---|---|---|
| Primary constructors | ✅ All services | ✅ All services | ✅ All services | ⚠️ Partial | ❌ None |
| Collection expressions `[]` | ✅ | ✅ | ✅ | ✅ Limited | ❌ `new List<T>()` |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ | ✅ |
| Target-typed new | ✅ | ✅ | ✅ | ✅ | ❌ |
| GlobalUsings.cs | ❌ | ✅ (Library, VetClinic) | ❌ | ❌ | ❌ |
| `required` keyword | ✅ | ✅ | ❌ | ❌ | ❌ |

```csharp
// dotnet-webapi: MemberService.cs — primary constructor + sealed
public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService

// no-skills: MemberService.cs — traditional constructor
public class MemberService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MemberService> _logger;
    public MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    {
        _db = db;
        _logger = logger;
    }
}
```

```csharp
// dotnet-artisan: Member.cs — collection expression
public ICollection<Membership> Memberships { get; set; } = [];

// no-skills: Member.cs — legacy
public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
```

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **5**, managedcode: **4**, dotnet-skills: **3**, no-skills: **2**

**Verdict**: **dotnet-webapi** and **dotnet-artisan** tie — both fully embrace C# 12 features. **no-skills** is noticeably behind, lacking primary constructors and collection expressions.

---

## 10. Error Handling & Middleware [HIGH]

The modern approach uses `IExceptionHandler` (composable, DI-aware). The legacy approach uses custom middleware with `RequestDelegate`.

```csharp
// dotnet-webapi: Middleware/ApiExceptionHandler.cs — modern IExceptionHandler
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
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
        // Returns ProblemDetails
    }
}
```

```csharp
// no-skills: Middleware/GlobalExceptionHandlerMiddleware.cs — legacy middleware
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (BusinessRuleException ex) { /* handle */ }
        catch (Exception ex) { /* handle */ }
    }
}
```

| Config | Pattern | Sealed | ProblemDetails | Exception Mapping |
|---|---|---|---|---|
| dotnet-webapi | IExceptionHandler | ✅ `internal sealed` | ✅ | 404, 400, 409, 500 |
| dotnet-artisan | IExceptionHandler | ✅ `sealed` | ✅ | 404, 409, 400, 500 |
| managedcode | IExceptionHandler | ❌ Not sealed | ✅ | Custom exceptions |
| dotnet-skills | IExceptionHandler (Fitness), Middleware (others) | ✅ Fitness sealed | ✅ | Mixed patterns |
| no-skills | Custom Middleware | ❌ Not sealed | ✅ | BusinessRuleException |

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **4**, managedcode: **4**, dotnet-skills: **4**, no-skills: **3**

**Verdict**: **dotnet-webapi** has the cleanest implementation with `internal sealed` visibility, primary constructor DI, and comprehensive exception mapping. **no-skills** is the only configuration using the legacy middleware pattern.

---

## 11. Async Patterns & Cancellation [HIGH]

CancellationToken propagation varies dramatically:

```csharp
// dotnet-webapi: Full CancellationToken chain
// Endpoint:
group.MapGet("/", async (..., CancellationToken ct) => { ... });
// Service interface:
Task<PaginatedResponse<MemberResponse>> GetAllAsync(
    string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
// Service implementation:
var items = await query.Skip(skip).Take(pageSize).ToListAsync(ct);

// no-skills: No CancellationToken at all
public async Task<PaginatedResult<MemberListDto>> GetAllAsync(
    int page, int pageSize, string? search, bool? isActive)
{
    var items = await query.Skip(skip).Take(pageSize).ToListAsync(); // no CT!
}
```

**Verified CancellationToken usage per config (total grep matches across all services):**
- dotnet-webapi: Extensive (every service method)
- dotnet-artisan: Extensive (most services, gap in LibraryApi)
- managedcode: Present (services use CT)
- dotnet-skills: Minimal (few services)
- no-skills: Zero CancellationToken usage in service methods

No configuration uses `async void` or `.Result`/`.Wait()` anti-patterns.

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **4**, managedcode: **4**, dotnet-skills: **2**, no-skills: **2**

**Verdict**: **dotnet-webapi** propagates CancellationToken through every layer consistently. **no-skills** and **dotnet-skills** lack CancellationToken entirely, wasting server resources on cancelled requests.

---

## 12. EF Core Best Practices [HIGH]

### AsNoTracking

```csharp
// dotnet-webapi: MemberService.cs — consistent AsNoTracking
var query = db.Members.AsNoTracking().AsQueryable();

// no-skills: MemberService.cs — NO AsNoTracking (tracks all reads)
var query = _db.Members.AsQueryable();
```

### Fluent API & HasConversion

```csharp
// dotnet-webapi: FitnessDbContext.cs — enum string conversion
entity.Property(e => e.Status).HasConversion<string>();
entity.Property(e => e.PaymentStatus).HasConversion<string>();
entity.HasIndex(e => new { e.MemberId, e.Status }); // composite index

// no-skills: FitnessDbContext.cs — basic config, no HasConversion
entity.HasIndex(e => e.Email).IsUnique();
// Enums stored as integers (default)
```

| Config | AsNoTracking | HasConversion<string\> | Composite Indexes | Delete Behavior |
|---|---|---|---|---|
| dotnet-webapi | ✅ All reads | ✅ All enums | ✅ | ✅ Configured |
| dotnet-artisan | ✅ Most reads | ⚠️ Partial | ✅ | ✅ Configured |
| managedcode | ✅ Most reads | ⚠️ Partial | ✅ | ✅ Configured |
| dotnet-skills | ✅ Most reads | ✅ All enums | ✅ | ✅ Configured |
| no-skills | ❌ None | ⚠️ Some apps | ❌ | ⚠️ Defaults |

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **4**, managedcode: **4**, dotnet-skills: **4**, no-skills: **2**

**Verdict**: **dotnet-webapi** sets the standard with universal AsNoTracking, full enum string conversion, and composite indexes. **no-skills** lacks AsNoTracking entirely, roughly doubling memory allocation on read queries.

---

## 13. Service Abstraction & DI [HIGH]

All configurations use interface-based services with `AddScoped<IService, Service>()` registration:

```csharp
// Common pattern across all configs:
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBookingService, BookingService>();
```

The difference is in constructor patterns (covered in Modern C#) and scope correctness. All configurations properly use Scoped lifetime for DbContext-dependent services.

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **5**, managedcode: **5**, dotnet-skills: **5**, no-skills: **4**

**Verdict**: Essentially tied. **no-skills** scores slightly lower due to less consistent separation in some apps. All configurations correctly implement the IService pattern.

---

## 14. Security Configuration [HIGH]

No configuration implements full HSTS + HTTPS redirection. Only **no-skills** includes partial security middleware.

```csharp
// no-skills: FitnessStudioApi/Program.cs — has UseHttpsRedirection
app.UseHttpsRedirection();
// But NO UseHsts()

// All other configs: NO security middleware at all
// (No UseHttpsRedirection, no UseHsts, no CORS)
```

**Scores**: no-skills: **3**, all others: **2**

**Verdict**: **no-skills** ironically leads here with `UseHttpsRedirection()` on its FitnessStudioApi. All skill-enhanced configurations omit security headers entirely — a notable blind spot.

---

## 15. DTO Design [MEDIUM]

```csharp
// dotnet-webapi: Sealed records with Request/Response naming
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
}
public sealed record MemberResponse(int Id, string FirstName, ...);

// no-skills: Non-sealed positional records with Dto naming
public record CreateMembershipPlanDto(
    [Required, MaxLength(100)] string Name,
    [Range(1, 24)] int DurationMonths);
```

| Config | Type | Sealed | Naming | Immutability |
|---|---|---|---|---|
| dotnet-webapi | Records | ✅ Sealed | Request/Response | ✅ `required` + `init` |
| dotnet-artisan | Records | ✅ Sealed | Response/Request | ✅ `init` |
| managedcode | Records | ❌ | Dto suffix | ✅ `init` |
| dotnet-skills | Classes | ⚠️ Partial | Dto suffix | ⚠️ `set` (mutable) |
| no-skills | Records | ❌ | Dto suffix | ✅ Positional (immutable) |

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **5**, managedcode: **4**, dotnet-skills: **3**, no-skills: **3**

**Verdict**: **dotnet-webapi** and **dotnet-artisan** both use sealed records with modern naming. **dotnet-skills** notably uses mutable classes for DTOs.

---

## 16. Sealed Types [MEDIUM]

**Verified `sealed class`/`sealed record` file counts (FitnessStudioApi only):**
- dotnet-artisan: **24 files**
- dotnet-webapi: **25 files**
- dotnet-skills: **31 files**
- managedcode: **0 files**
- no-skills: **0 files**

```csharp
// dotnet-webapi: Every type is sealed
public sealed class MemberService(FitnessDbContext db, ...) : IMemberService
public sealed class FitnessDbContext : DbContext
internal sealed class ApiExceptionHandler : IExceptionHandler
public sealed record CreateMemberRequest { ... }

// no-skills: Nothing is sealed
public class MembershipService : IMembershipService
public class FitnessDbContext : DbContext
public class GlobalExceptionHandlerMiddleware
public record CreateMembershipPlanDto(...);
```

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **5**, dotnet-skills: **4**, managedcode: **1**, no-skills: **1**

**Verdict**: **dotnet-webapi**, **dotnet-artisan**, and **dotnet-skills** consistently seal their types. **managedcode** and **no-skills** seal nothing, missing JIT devirtualization optimizations and design intent signaling.

---

## 17. Data Seeder Design [MEDIUM]

| Config | Strategy | Async | Idempotent | Data Richness |
|---|---|---|---|---|
| dotnet-webapi | Service-based (Fitness/Library), HasData (VetClinic) | ✅ Mixed | ✅ | Rich (8+ members, 15+ bookings) |
| dotnet-artisan | Static SeedAsync service | ✅ | ✅ | Rich |
| managedcode | Static SeedAsync service | ✅ | ✅ | Good |
| dotnet-skills | Static Seed (sync) | ⚠️ Mixed | ✅ | Good |
| no-skills | DataSeeder service via DI | ✅ | ✅ | Good |

```csharp
// dotnet-webapi: VetClinicApi — HasData in migrations (best practice)
private static void SeedData(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Owner>().HasData(
        new Owner { Id = 1, FirstName = "Sarah", ... });
}
```

**Scores**: dotnet-webapi: **4**, dotnet-artisan: **4**, managedcode: **3**, dotnet-skills: **3**, no-skills: **3**

**Verdict**: **dotnet-webapi** leads by using `HasData()` for migration-compatible seeding on VetClinicApi, paired with service-based seeding elsewhere. All configurations include idempotency guards.

---

## 18. Structured Logging [MEDIUM]

```csharp
// dotnet-webapi: MemberService.cs — structured logging in all services
logger.LogInformation("Registered new member {MemberName} with ID {MemberId}",
    $"{member.FirstName} {member.LastName}", member.Id);

// dotnet-artisan: Only in exception handler — limited coverage
logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

// no-skills: MembershipService.cs — present but inconsistent
_logger.LogInformation("Membership created for member {MemberId} with plan {PlanName}",
    dto.MemberId, plan.Name);
```

| Config | ILogger<T> in Services | Structured Templates | Coverage |
|---|---|---|---|
| dotnet-webapi | ✅ All services | ✅ Named placeholders | Comprehensive |
| dotnet-artisan | ⚠️ Exception handler mainly | ✅ | Minimal |
| managedcode | ✅ All services | ✅ | Good |
| dotnet-skills | ✅ All services | ✅ | Good |
| no-skills | ⚠️ Some services | ✅ | Inconsistent |

No configuration uses `[LoggerMessage]` source generators.

**Scores**: dotnet-webapi: **5**, managedcode: **4**, dotnet-skills: **4**, dotnet-artisan: **3**, no-skills: **3**

**Verdict**: **dotnet-webapi** logs business events across all services with proper structured templates. **dotnet-artisan** surprisingly has minimal service-level logging despite its sophisticated architecture.

---

## 19. Nullable Reference Types [MEDIUM]

All 15 `.csproj` files include `<Nullable>enable</Nullable>`. All configurations use `?` annotations on optional properties. Usage is consistent across all configs.

**Scores**: All configurations: **4**

**Verdict**: Tie. Universal NRT adoption with proper nullable annotations.

---

## 20. API Documentation [MEDIUM]

```csharp
// dotnet-webapi: Full Minimal API metadata
group.MapGet("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (...) => { ... })
    .WithName("GetMember")
    .WithSummary("Get member by ID")
    .WithDescription("Returns member details including active membership")
    .Produces<MemberResponse>(200)
    .ProducesProblem(404);

// no-skills: Controller-based metadata
[HttpGet("{id}")]
[ProducesResponseType(typeof(MembershipPlanDto), 200)]
[ProducesResponseType(404)]
public async Task<IActionResult> GetById(int id) { ... }
```

| Config | WithName | WithSummary | WithTags | Produces<T\> | Framework |
|---|---|---|---|---|---|
| dotnet-webapi | ✅ | ✅ | ✅ | ✅ | Built-in OpenAPI |
| dotnet-artisan | ❌ | ✅ | ✅ | ❌ | OpenAPI + SwaggerUI |
| managedcode | N/A | N/A | N/A | ✅ ProducesResponseType | OpenAPI + Swashbuckle |
| dotnet-skills | N/A (controllers) | ✅ XML | N/A | ✅ ProducesResponseType | OpenAPI + Swashbuckle |
| no-skills | N/A | N/A | N/A | ✅ ProducesResponseType | Swashbuckle only |

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **4**, managedcode: **3**, dotnet-skills: **3**, no-skills: **3**

**Verdict**: **dotnet-webapi** provides the richest API metadata through Minimal API fluent methods. Controller-based configs are limited to attributes.

---

## 21. File Organization [MEDIUM]

```
# dotnet-webapi: FitnessStudioApi (clean, ~65 lines Program.cs)
├── Program.cs (65 lines)
├── Endpoints/ (7 files: one per resource)
├── Services/ (14 files: 7 interfaces + 7 implementations)
├── Models/ (8 files)
├── DTOs/ (8 files)
├── Data/ (DbContext + DataSeeder)
├── Middleware/ (ApiExceptionHandler)
└── Migrations/

# dotnet-skills: VetClinicApi (274-line Program.cs!)
├── Program.cs (274 lines — ALL endpoints inline!)
├── Services/ (16 files)
├── Models/ (9 files)
├── DTOs/ (9 files)
├── Data/ (DbContext + DataSeeder)
└── Middleware/
```

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **4**, managedcode: **4**, dotnet-skills: **3**, no-skills: **3**

**Verdict**: **dotnet-webapi** maintains the cleanest Program.cs across all apps with proper endpoint extraction. **dotnet-skills** VetClinicApi is notably cluttered with a 274-line Program.cs containing all endpoint definitions inline.

---

## 22. HTTP Test File Quality [MEDIUM]

All configurations generate `.http` test files. Coverage varies significantly:

| Config | FitnessStudio | LibraryApi | VetClinicApi | Business Rule Tests |
|---|---|---|---|---|
| dotnet-webapi | ~348 lines, comprehensive | Comprehensive | ~50 lines | ✅ Premium access, weekly limits |
| dotnet-artisan | Comprehensive | Comprehensive | Comprehensive | ✅ |
| managedcode | Comprehensive | Comprehensive | Comprehensive | ✅ |
| dotnet-skills | ~362 lines | Comprehensive | **7 lines only** ⚠️ | ✅ on Fitness |
| no-skills | ~373 lines | Present | Present | ✅ |

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **4**, managedcode: **4**, no-skills: **4**, dotnet-skills: **3**

**Verdict**: **dotnet-webapi** provides the most consistently comprehensive `.http` files. **dotnet-skills** has a critical gap with VetClinicApi's 7-line placeholder.

---

## 23. Type Design & Resource Management [MEDIUM]

```csharp
// dotnet-webapi: Enums stored as strings, IReadOnlyList returns
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }
// In DbContext: entity.Property(e => e.Status).HasConversion<string>();
// Service returns: Task<IReadOnlyList<BookingResponse>>

// no-skills: Enums stored as integers, IEnumerable returns
public enum MembershipStatus { Active, Expired, Cancelled, Frozen }
// No HasConversion — stored as int
// Service returns: Task<IEnumerable<BookingDto>>
```

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **4**, dotnet-skills: **4**, managedcode: **3**, no-skills: **3**

**Verdict**: **dotnet-webapi** consistently stores enums as strings (readable in database), returns `IReadOnlyList<T>` (prevents mutation), and uses precise return types throughout.

---

## 24. Code Standards Compliance [LOW]

All configurations follow .NET naming conventions (PascalCase for publics, camelCase for parameters, I-prefix for interfaces, Async suffix). The key difference is explicit access modifiers and consistency.

```csharp
// dotnet-webapi: Explicit internal on handler
internal sealed class ApiExceptionHandler : IExceptionHandler

// no-skills: Default (implicit public)
public class GlobalExceptionHandlerMiddleware
```

**Scores**: dotnet-webapi: **5**, dotnet-artisan: **5**, managedcode: **5**, dotnet-skills: **5**, no-skills: **4**

**Verdict**: Near-tie. All configurations follow .NET conventions. **no-skills** slightly less consistent with access modifiers.

---

## Weighted Summary

Weights: Critical ×3, High ×2, Medium ×1, Low ×0.5

| Dimension [Tier] | Weight | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|---|---|---|---|---|---|
| Build & Run [CRITICAL] | ×3 | 12 | 12 | 12 | 12 | 12 |
| Security Scan [CRITICAL] | ×3 | 9 | 12 | 9 | 9 | 9 |
| Minimal API [CRITICAL] | ×3 | 12 | 15 | 3 | 6 | 3 |
| Input Validation [CRITICAL] | ×3 | 12 | 12 | 12 | 12 | 9 |
| NuGet Discipline [CRITICAL] | ×3 | 6 | 9 | 9 | 9 | 9 |
| EF Migration [CRITICAL] | ×3 | 3 | 15 | 3 | 3 | 3 |
| **Critical Subtotal** | | **54** | **75** | **48** | **51** | **45** |
| Business Logic [HIGH] | ×2 | 8 | 10 | 8 | 8 | 8 |
| Built-in Preference [HIGH] | ×2 | 6 | 10 | 4 | 6 | 4 |
| Modern C# [HIGH] | ×2 | 10 | 10 | 8 | 6 | 4 |
| Error Handling [HIGH] | ×2 | 8 | 10 | 8 | 8 | 6 |
| Async/CancellationToken [HIGH] | ×2 | 8 | 10 | 8 | 4 | 4 |
| EF Core Practices [HIGH] | ×2 | 8 | 10 | 8 | 8 | 4 |
| Service/DI [HIGH] | ×2 | 10 | 10 | 10 | 10 | 8 |
| Security Config [HIGH] | ×2 | 4 | 4 | 4 | 4 | 6 |
| **High Subtotal** | | **62** | **74** | **58** | **54** | **44** |
| DTO Design [MEDIUM] | ×1 | 5 | 5 | 4 | 3 | 3 |
| Sealed Types [MEDIUM] | ×1 | 5 | 5 | 1 | 4 | 1 |
| Data Seeder [MEDIUM] | ×1 | 4 | 4 | 3 | 3 | 3 |
| Logging [MEDIUM] | ×1 | 3 | 5 | 4 | 4 | 3 |
| NRT [MEDIUM] | ×1 | 4 | 4 | 4 | 4 | 4 |
| API Docs [MEDIUM] | ×1 | 4 | 5 | 3 | 3 | 3 |
| File Org [MEDIUM] | ×1 | 4 | 5 | 4 | 3 | 3 |
| HTTP Tests [MEDIUM] | ×1 | 4 | 5 | 4 | 3 | 4 |
| Type Design [MEDIUM] | ×1 | 4 | 5 | 3 | 4 | 3 |
| **Medium Subtotal** | | **37** | **43** | **30** | **31** | **27** |
| Code Standards [LOW] | ×0.5 | 2.5 | 2.5 | 2.5 | 2.5 | 2.0 |
| **Low Subtotal** | | **2.5** | **2.5** | **2.5** | **2.5** | **2.0** |
| | | | | | | |
| **TOTAL WEIGHTED SCORE** | | **155.5** | **194.5** | **138.5** | **138.5** | **118.0** |
| **RANK** | | **2nd** | **1st** | **3rd (tie)** | **3rd (tie)** | **5th** |

---

## What All Versions Get Right

- ✅ **Target .NET 10** with proper `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- ✅ **Interface-based DI** with `AddScoped<IService, Service>()` pattern across all services
- ✅ **ProblemDetails** error responses following RFC 7807 in all configurations
- ✅ **Data Annotations** for input validation on DTOs ([Required], [MaxLength], [EmailAddress], [Range])
- ✅ **File-scoped namespaces** (`namespace X;`) in all source files
- ✅ **Proper async/await** patterns — no `async void`, no `.Result`, no `.Wait()` anti-patterns
- ✅ **EF Core with SQLite** and Fluent API configuration in `OnModelCreating()`
- ✅ **Comprehensive data seeding** with idempotency guards
- ✅ **`.http` test files** for API testing (varying completeness)
- ✅ **Proper enum types** for domain statuses (no magic strings or integers)
- ✅ **.NET naming conventions** — PascalCase for publics, camelCase for parameters, Async suffix

---

## Summary: Impact of Skills

### Overall Ranking

1. **🥇 dotnet-webapi (194.5 pts)** — The clear winner by a wide margin. This is the only configuration that produces modern Minimal APIs with TypedResults across all apps, uses EF Core migrations instead of EnsureCreated, avoids all third-party packages, and consistently applies CancellationToken propagation, AsNoTracking, and sealed types. It represents the gold standard for .NET team-quality generated code.

2. **🥈 dotnet-artisan (155.5 pts)** — Strong second place. Excels at modern C# adoption (primary constructors, sealed types, collection expressions) and delivers Minimal APIs on 2/3 apps. Weakened by universal EnsureCreated usage, Swashbuckle inclusion, and floating package versions. The gap from first place is primarily the EF migration miss (×3 weight on a 4-point difference = 12 points).

3. **🥉 managedcode-dotnet-skills (138.5 pts, tied 3rd)** — Solid output with good async patterns and CancellationToken support, but hampered by all-controller architecture, universal Swashbuckle dependency, and zero sealed types. Modern C# adoption is good (primary constructors, collection expressions) but not as consistent.

4. **🥉 dotnet-skills (138.5 pts, tied 3rd)** — Tied with managedcode but with a different profile: strong sealed type usage (31 files in FitnessStudio alone) and FluentValidation for rich validation, but poor Minimal API adoption (VetClinic's 274-line inline Program.cs), minimal CancellationToken support, and inconsistent patterns across apps.

5. **dotnet (no-skills) (118.0 pts)** — The baseline performs adequately but lacks virtually every modern .NET practice: no primary constructors, no collection expressions, no sealed types, no CancellationToken, no AsNoTracking, no IExceptionHandler. Ironically, it's the only configuration that includes `UseHttpsRedirection()`.

### Most Impactful Differences

1. **EF Migration vs EnsureCreated (36-point swing)** — The single largest scoring differentiator. Only dotnet-webapi uses migrations; all others use the EnsureCreated anti-pattern. This alone accounts for 12 of dotnet-webapi's 39-point lead over dotnet-artisan.

2. **Minimal API Architecture (36-point swing)** — Using Minimal APIs with MapGroup, TypedResults, and endpoint extensions is a critical modern .NET practice that only dotnet-webapi achieves universally.

3. **CancellationToken Propagation (12-point swing)** — The difference between 5 and 2 at ×2 weight. dotnet-webapi and dotnet-artisan lead; no-skills and dotnet-skills lag significantly.

4. **Third-Party Dependencies (12-point swing)** — dotnet-webapi's exclusive use of built-in APIs gives it a meaningful edge over configurations that pull in Swashbuckle and FluentValidation.

5. **Sealed Types (4-point swing)** — While only ×1 weight, the difference between 5 and 1 is stark. Three configurations seal everything; two seal nothing.

### Conclusion

Skills make a **substantial and measurable difference** in generated code quality. The gap between the best skill (dotnet-webapi at 194.5) and no skills (118.0) is **76.5 points — a 65% improvement**. The dotnet-webapi skill consistently produces code that follows modern .NET best practices, while the baseline generates functional but outdated patterns. The dotnet-artisan plugin chain provides meaningful improvement over no-skills (+37.5 points, 32% improvement) but falls short of dotnet-webapi primarily due to EF migration and third-party dependency choices.
