# Comparative Analysis: dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, no-skills, dotnet-webapi

## Introduction

This report compares **5 Copilot skill configurations** that each generated the same **FitnessStudioApi** — a booking/membership system with class scheduling, waitlists, and instructor management targeting .NET 10 with EF Core and SQLite. Each configuration produced a complete API at `output/{config}/run-1/FitnessStudioApi/src/FitnessStudioApi/`.

| Configuration | Label | API Style | Packages |
|---|---|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain | Minimal APIs | 5 |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | Controllers | 5 |
| **dotnet-webapi** | dotnet-webapi skill | Minimal APIs | 3 |
| **managedcode-dotnet-skills** | Community managed-code skills | Controllers | 4 |
| **no-skills** | Baseline (default Copilot) | Controllers | 4 |

All five configurations target `net10.0`, use SQLite via EF Core, implement all 12 business rules from the specification, and produce zero build errors and zero vulnerable packages.

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode | no-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Minimal API Architecture [CRITICAL] | 5 | 2 | 5 | 2 | 2 |
| Input Validation & Guard Clauses [CRITICAL] | 5 | 4 | 5 | 4 | 4 |
| NuGet & Package Discipline [CRITICAL] | 4 | 4 | 3 | 5 | 5 |
| EF Migration Usage [CRITICAL] | 2 | 2 | 5 | 2 | 2 |
| Business Logic Correctness [HIGH] | 5 | 5 | 5 | 5 | 5 |
| Prefer Built-in over 3rd Party [HIGH] | 3 | 3 | 5 | 3 | 4 |
| Modern C# Adoption [HIGH] | 5 | 4 | 5 | 4 | 3 |
| Error Handling & Middleware [HIGH] | 5 | 4 | 5 | 5 | 4 |
| Async Patterns & Cancellation [HIGH] | 5 | 3 | 5 | 3 | 3 |
| EF Core Best Practices [HIGH] | 5 | 5 | 5 | 5 | 4 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 5 | 5 | 5 |
| Security Configuration [HIGH] | 3 | 3 | 3 | 3 | 3 |
| DTO Design [MEDIUM] | 5 | 5 | 5 | 5 | 5 |
| Sealed Types [MEDIUM] | 5 | 5 | 5 | 2 | 1 |
| Data Seeder Design [MEDIUM] | 5 | 5 | 5 | 5 | 5 |
| Structured Logging [MEDIUM] | 4 | 3 | 5 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 5 | 5 | 5 | 5 | 5 |
| API Documentation [MEDIUM] | 5 | 4 | 5 | 3 | 3 |
| File Organization [MEDIUM] | 5 | 5 | 5 | 5 | 4 |
| HTTP Test File Quality [MEDIUM] | 5 | 5 | 5 | 4 | 5 |
| Type Design & Resource Management [MEDIUM] | 5 | 5 | 5 | 5 | 5 |
| Code Standards Compliance [LOW] | 5 | 5 | 5 | 4 | 5 |

---

## 1. Build & Run Success [CRITICAL]

All five configurations produce projects that compile with **zero errors and zero warnings**:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

| Config | Build Result | Warnings | Errors |
|---|---|---|---|
| dotnet-artisan | ✅ Build succeeded | 0 | 0 |
| dotnet-skills | ✅ Build succeeded | 0 | 0 |
| dotnet-webapi | ✅ Build succeeded | 0 | 0 |
| managedcode | ✅ Build succeeded | 0 | 0 |
| no-skills | ✅ Build succeeded | 0 | 0 |

**Score**: All configurations — **5/5**

**Verdict**: No differentiation. All configurations produce clean-building projects on the first attempt.

---

## 2. Security Vulnerability Scan [CRITICAL]

All five configurations report **no vulnerable packages** when scanned with `dotnet list package --vulnerable`:

```
The given project `FitnessStudioApi` has no vulnerable packages given the current sources.
```

All packages use current .NET 10 versions (EF Core 10.0.5, OpenApi 10.0.4) with no known CVEs.

**Score**: All configurations — **5/5**

**Verdict**: No differentiation. All configurations select modern, maintained packages.

---

## 3. Minimal API Architecture [CRITICAL]

This is the **single most differentiating dimension** across configurations.

**dotnet-artisan (5/5)** — Full Minimal API with static method handlers and route groups:
```csharp
// Endpoints/MemberEndpoints.cs (dotnet-artisan)
public static RouteGroupBuilder MapMemberEndpoints(this IEndpointRouteBuilder routes)
{
    var group = routes.MapGroup("/api/members")
        .WithTags("Members");
    group.MapGet("/", GetAll).WithSummary("List members with search and filters");
    group.MapGet("/{id:int}", GetById).WithSummary("Get member details");
    group.MapPost("/", Create).WithSummary("Register a new member");
    return group;
}
```

**dotnet-webapi (5/5)** — Full Minimal API with `TypedResults` and `Results<T1, T2>` union return types:
```csharp
// Endpoints/BookingEndpoints.cs (dotnet-webapi)
group.MapGet("/{id:int}", async Task<Results<Ok<BookingResponse>, NotFound>> (
    int id, IBookingService service, CancellationToken ct) =>
{
    var booking = await service.GetByIdAsync(id, ct);
    return booking is null ? TypedResults.NotFound() : TypedResults.Ok(booking);
});
```

**dotnet-skills, managedcode, no-skills (2/5)** — All use traditional `[ApiController]` pattern:
```csharp
// Controllers/BookingsController.cs (no-skills, managedcode, dotnet-skills)
[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    public BookingsController(IBookingService service) => _service = service;
}
```

| Feature | dotnet-artisan | dotnet-webapi | Others |
|---|---|---|---|
| MapGroup() | ✅ | ✅ | ❌ |
| Extension methods | ✅ | ✅ | ❌ |
| TypedResults | ✅ | ✅ | ❌ |
| Results<T1,T2> unions | ❌ | ✅ | ❌ |
| WithSummary/WithTags | ✅ | ✅ | ❌ |

**Verdict**: **dotnet-webapi** leads with full TypedResults + union return types. **dotnet-artisan** is close behind with static method handlers. The three controller-based configurations use the older pattern that cannot leverage compile-time OpenAPI schema generation.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

**dotnet-artisan (5/5)** and **dotnet-webapi (5/5)** — Both combine DataAnnotation attributes on DTOs with exception-based validation in services:
```csharp
// DTOs/MemberDtos.cs (dotnet-artisan)
public sealed record CreateMemberDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    DateOnly DateOfBirth,
    [Required, MaxLength(200)] string EmergencyContactName,
    [Required] string EmergencyContactPhone);
```

```csharp
// BookingService.cs (dotnet-webapi)
var member = await db.Members.FindAsync([request.MemberId], ct)
    ?? throw new KeyNotFoundException($"Member with Id {request.MemberId} not found.");
if (!member.IsActive)
    throw new ArgumentException("Member account is inactive.");
```

**dotnet-skills, managedcode, no-skills (4/5)** — Good DTO validation but no modern guard clause APIs (`ArgumentNullException.ThrowIfNull`, `ArgumentException.ThrowIfNullOrEmpty`):
```csharp
// Services/MemberService.cs (no-skills)
public MemberService(FitnessDbContext db, ILogger<MemberService> _logger)
{
    _db = db;       // No ArgumentNullException.ThrowIfNull(db)
    _logger = logger;
}
```

**Verdict**: The Minimal API configurations produce cleaner validation patterns. All configurations validate business rules but the controller-based ones lack modern guard clause APIs in constructors.

---

## 5. NuGet & Package Discipline [CRITICAL]

**managedcode (5/5)** and **no-skills (5/5)** — Minimal packages, all with exact pinned versions:
```xml
<!-- FitnessStudioApi.csproj (managedcode / no-skills) -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

**dotnet-artisan (4/5)** — Pinned versions but includes unused `FluentValidation.DependencyInjectionExtensions`:
```xml
<!-- FitnessStudioApi.csproj (dotnet-artisan) — 5 packages -->
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
<!-- ↑ Imported but validation uses DataAnnotations, not FluentValidation -->
```

**dotnet-skills (4/5)** — Pinned versions, FluentValidation actually used (validators exist), but 5 total packages:
```xml
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
```

**dotnet-webapi (3/5)** — Only 3 packages (minimal!) but uses **floating version ranges**:
```xml
<!-- FitnessStudioApi.csproj (dotnet-webapi) -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
<!-- ↑ Floating versions — non-reproducible builds, may pull prereleases -->
```

**Verdict**: **managedcode** and **no-skills** have the cleanest package discipline. **dotnet-webapi** has the most minimal package set (3 vs 4-5) but the floating `10.*-*` versions violate reproducible build practices.

---

## 6. EF Migration Usage [CRITICAL]

This is the **second most differentiating dimension**.

**dotnet-webapi (5/5)** — The only configuration using proper EF Core migrations:
```csharp
// Program.cs (dotnet-webapi)
await db.Database.MigrateAsync();  // ← Production-safe migrations
```
```
Migrations/
├── 20260329184143_InitialCreate.cs
├── 20260329184143_InitialCreate.Designer.cs
└── FitnessDbContextModelSnapshot.cs
```

**All others (2/5)** — Use `EnsureCreated()` / `EnsureCreatedAsync()` which bypasses migrations entirely:
```csharp
// Program.cs (dotnet-artisan, dotnet-skills)
await db.Database.EnsureCreatedAsync();  // ← Anti-pattern: no migration tracking

// Program.cs (managedcode, no-skills)
db.Database.EnsureCreated();  // ← Same anti-pattern, synchronous variant
```

**Verdict**: **dotnet-webapi** is the clear winner — it is the only configuration that uses the production-safe `MigrateAsync()` approach with a proper Migrations folder. All other configurations use `EnsureCreated()`, which makes schema evolution impossible.

---

## 7. Business Logic Correctness [HIGH]

All five configurations implement **all 12 business rules** from the specification. The implementation patterns are remarkably consistent:

```csharp
// Booking window validation — identical pattern across all configs
if (schedule.StartTime > now.AddDays(7))
    throw new BusinessRuleException("Cannot book a class more than 7 days in advance.");
if (schedule.StartTime <= now.AddMinutes(30))
    throw new BusinessRuleException("Cannot book a class less than 30 minutes before start time.");
```

All configurations implement: booking window, capacity/waitlist management, cancellation policy, premium tier access, weekly limits, active membership enforcement, no double-booking, instructor conflicts, membership freeze/unfreeze, class cancellation cascade, check-in window, and no-show flagging.

**Score**: All configurations — **5/5**

**Verdict**: No differentiation. This confirms that all configurations received and correctly interpreted the specification.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

**dotnet-webapi (5/5)** — Zero third-party libraries where built-in alternatives exist:
```csharp
// Program.cs (dotnet-webapi) — pure built-in OpenAPI
builder.Services.AddOpenApi();
// ...
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();  // No Swashbuckle at all
}
```

**no-skills (4/5)** — Uses Swashbuckle for UI only, with built-in OpenAPI for spec generation:
```csharp
// Program.cs (no-skills)
builder.Services.AddOpenApi();
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Zenith Fitness Studio API v1");
});
```

**dotnet-artisan, dotnet-skills, managedcode (3/5)** — Use `AddSwaggerGen()` for document generation alongside built-in OpenAPI (redundant):
```csharp
// Program.cs (dotnet-artisan, managedcode)
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();  // ← Redundant with AddOpenApi()
```

| Feature | dotnet-webapi | no-skills | Others |
|---|---|---|---|
| AddOpenApi() (built-in) | ✅ | ✅ | ✅ |
| Swashbuckle package | ❌ | ✅ (UI only) | ✅ (full) |
| AddSwaggerGen() | ❌ | ❌ | ✅ |
| System.Text.Json | ✅ | ✅ | ✅ |
| Built-in ILogger | ✅ | ✅ | ✅ |

**Verdict**: **dotnet-webapi** is exemplary — no Swashbuckle dependency at all. The other configurations include Swashbuckle unnecessarily when built-in OpenAPI via `AddOpenApi()` / `MapOpenApi()` suffices.

---

## 9. Modern C# Adoption [HIGH]

**dotnet-artisan (5/5)** and **dotnet-webapi (5/5)** — Fully embrace C# 12+ features:
```csharp
// Primary constructors (dotnet-artisan, dotnet-webapi)
public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService

// Collection expressions (dotnet-artisan, dotnet-webapi)
public ICollection<Membership> Memberships { get; set; } = [];

// Required keyword (dotnet-artisan, dotnet-webapi)
public required string FirstName { get; set; }
```

**dotnet-skills (4/5)** and **managedcode (4/5)** — Records and file-scoped namespaces, but traditional constructors:
```csharp
// Traditional constructor (dotnet-skills, managedcode)
public BookingService(FitnessDbContext context, ILogger<BookingService> logger)
{
    _context = context;
    _logger = logger;
}
```

**no-skills (3/5)** — Minimal modern C# adoption:
```csharp
// Old collection initialization (no-skills)
public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
// Should be: = [];

// No primary constructors, no collection expressions, no global usings
```

| Feature | artisan | webapi | skills | managedcode | no-skills |
|---|---|---|---|---|---|
| Primary constructors | ✅ | ✅ | ❌ | ❌ | ❌ |
| Collection expressions `[]` | ✅ | ✅ | ❌ | ❌ | ❌ |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ | ✅ |
| Records for DTOs | ✅ | ✅ | ✅ | ✅ | ✅ |
| `required` keyword | ✅ | ✅ | ❌ | ❌ | ❌ |

**Verdict**: **dotnet-artisan** and **dotnet-webapi** generate code that fully leverages C# 12-14 features. The controller-based configurations stick to C# 10/11 patterns.

---

## 10. Error Handling & Middleware [HIGH]

**dotnet-artisan (5/5)**, **dotnet-webapi (5/5)**, and **managedcode (5/5)** — Use modern `IExceptionHandler`:
```csharp
// Middleware/GlobalExceptionHandler.cs (dotnet-artisan)
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (statusCode, title) = exception switch
        {
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Business Rule Violation"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
        // Returns ProblemDetails...
    }
}
```

Registration:
```csharp
// Program.cs (dotnet-artisan, dotnet-webapi, managedcode)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
```

**dotnet-skills (4/5)** and **no-skills (4/5)** — Use legacy convention-based middleware:
```csharp
// Middleware/ExceptionHandlingMiddleware.cs (dotnet-skills)
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }
}
```

Registration:
```csharp
// Program.cs (dotnet-skills, no-skills)
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

**Verdict**: The `IExceptionHandler` pattern (used by artisan, webapi, managedcode) is composable, DI-aware, and the modern .NET 8+ approach. The middleware pattern works but is less maintainable.

---

## 11. Async Patterns & Cancellation [HIGH]

**dotnet-artisan (5/5)** and **dotnet-webapi (5/5)** — Full `CancellationToken` propagation through the entire call chain:
```csharp
// Endpoint → Service → EF Core (dotnet-webapi)
group.MapPost("/", async (CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
{
    var booking = await service.CreateAsync(request, ct);
    return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
});

// Service method (dotnet-webapi)
public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
{
    var member = await db.Members.FindAsync([request.MemberId], ct);  // ct forwarded to EF
}
```

**dotnet-skills (3/5)**, **managedcode (3/5)**, and **no-skills (3/5)** — No `CancellationToken` propagation:
```csharp
// Controller (no-skills) — no CancellationToken parameter
public async Task<IActionResult> GetAll(
    [FromQuery] string? search, [FromQuery] bool? isActive,
    [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    => Ok(await _service.GetAllAsync(search, isActive, page, pageSize));

// Service (no-skills) — no CancellationToken parameter
public async Task<BookingDto> GetByIdAsync(int id)  // Missing CancellationToken
```

**Verdict**: The Minimal API configurations propagate `CancellationToken` from endpoint to EF Core. The controller-based configurations drop the token entirely, wasting server resources on cancelled requests.

---

## 12. EF Core Best Practices [HIGH]

**dotnet-artisan (5/5)**, **dotnet-skills (5/5)**, **dotnet-webapi (5/5)**, and **managedcode (5/5)** — Comprehensive EF Core patterns:
```csharp
// Fluent API config (all skilled configurations)
modelBuilder.Entity<MembershipPlan>(entity =>
{
    entity.HasIndex(e => e.Name).IsUnique();
    entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
});

// AsNoTracking for reads (dotnet-artisan, dotnet-skills, dotnet-webapi, managedcode)
var query = db.Members.AsNoTracking().AsQueryable();

// Enum conversion (all)
entity.Property(e => e.Status).HasConversion<string>();

// Restrict delete (all)
entity.HasOne(e => e.Member).WithMany(m => m.Memberships)
    .HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Restrict);
```

**no-skills (4/5)** — Same Fluent API patterns but **missing `AsNoTracking()`** on read queries:
```csharp
// Services/ClassScheduleService.cs (no-skills)
var query = _db.ClassSchedules
    .Include(cs => cs.ClassType)
    .Include(cs => cs.Instructor)
    .AsQueryable();  // ❌ Missing .AsNoTracking()
```

**Verdict**: Four of five configurations achieve excellent EF Core patterns. **no-skills** omits `AsNoTracking()`, doubling memory usage on read queries.

---

## 13. Service Abstraction & DI [HIGH]

All five configurations use identical service abstraction patterns:

```csharp
// Program.cs (all configurations)
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<IClassTypeService, ClassTypeService>();
builder.Services.AddScoped<IClassScheduleService, ClassScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
```

All use interface-based registration (`AddScoped<IService, Service>()`), scoped lifetime (matching DbContext), and constructor injection. Seven services cover all business domains.

**Score**: All configurations — **5/5**

**Verdict**: No differentiation. Interface-based service abstraction with scoped DI is a well-understood pattern that all configurations implement correctly.

---

## 14. Security Configuration [HIGH]

**All configurations (3/5)** — None implement HSTS or HTTPS redirection. All properly restrict Swagger/OpenAPI to development:

```csharp
// Missing from ALL configurations:
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();              // ❌ Not present
    app.UseHttpsRedirection();  // ❌ Not present
}
```

All do correctly gate Swagger/OpenAPI behind the development environment check:
```csharp
// Program.cs (all configurations)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Swagger UI configuration...
}
```

**Score**: All configurations — **3/5**

**Verdict**: No differentiation. All configurations share the same security gap — missing HSTS and HTTPS redirection for production deployments.

---

## 15. DTO Design [MEDIUM]

All configurations use **record types** for DTOs with consistent naming. The Minimal API configurations additionally use `sealed record`:

```csharp
// dotnet-artisan, dotnet-webapi
public sealed record CreateMemberDto(
    [Required, MaxLength(100)] string FirstName, ...);

// dotnet-webapi — Request/Response naming
public sealed record CreateBookingRequest { ... }
public sealed record BookingResponse(...);

// dotnet-skills, managedcode, no-skills — Dto suffix naming
public record CreateBookingDto(int ClassScheduleId, int MemberId);
public record BookingDto(...);
```

**Score**: All configurations — **5/5**

**Verdict**: All configurations use records for DTOs. **dotnet-webapi** stands out with the `Request`/`Response` naming convention and `init` properties with `required` keyword for request types.

---

## 16. Sealed Types [MEDIUM]

**dotnet-artisan (5/5)**, **dotnet-skills (5/5)**, and **dotnet-webapi (5/5)** — All classes and records are sealed:
```csharp
// dotnet-artisan, dotnet-webapi
public sealed class BookingService(FitnessDbContext db, ...) : IBookingService
public sealed class Member { ... }
public sealed record BookingResponse(...)

// dotnet-skills
public sealed class BookingService : IBookingService
public sealed class ExceptionHandlingMiddleware
```

**managedcode (2/5)** — Some classes sealed, but inconsistent:
```csharp
// managedcode — Controllers and DTOs unsealed
public class BookingsController : ControllerBase  // ❌ Not sealed
public record MembershipPlanDto(...)               // ❌ Not sealed
```

**no-skills (1/5)** — No sealed types at all:
```csharp
// no-skills — nothing is sealed
public class MemberService : IMemberService  // ❌
public class Member                          // ❌
public record MembershipPlanDto(...)         // ❌
```

**Verdict**: **dotnet-artisan**, **dotnet-skills**, and **dotnet-webapi** all enforce sealing for JIT devirtualization and design intent. **no-skills** misses this entirely.

---

## 17. Data Seeder Design [MEDIUM]

All five configurations implement nearly identical runtime seeders with realistic variety:

```csharp
// Data/DataSeeder.cs (all configurations)
public static async Task SeedAsync(FitnessDbContext db)
{
    if (await db.MembershipPlans.AnyAsync()) return;  // Idempotent

    // 3 plans (Basic $29.99, Premium $49.99, Elite $79.99)
    // 8 members with varied profiles
    // 6-8 memberships (Active, Expired, Frozen, Cancelled)
    // 4 instructors, 6 class types (2 premium)
    // 12 schedules, 15-22 bookings in various states
}
```

**Score**: All configurations — **5/5**

**Verdict**: No meaningful differentiation. All configurations produce comprehensive, idempotent seed data.

---

## 18. Structured Logging [MEDIUM]

**dotnet-webapi (5/5)** — Structured templates with consistent usage across all services and the seeder:
```csharp
// BookingService.cs (dotnet-webapi)
logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ClassId} with status {Status}",
    booking.Id, request.MemberId, request.ClassScheduleId, booking.Status);

// DataSeeder.cs (dotnet-webapi)
logger.LogInformation("Seeding database with initial data");
```

**dotnet-artisan (4/5)**, **managedcode (4/5)**, **no-skills (4/5)** — Good structured logging but no `[LoggerMessage]` source generators:
```csharp
// BookingService.cs (dotnet-artisan)
logger.LogInformation("Booking {BookingId} created for member {MemberId} in class {ClassId} with status {Status}",
    booking.Id, dto.MemberId, dto.ClassScheduleId, booking.Status);
```

**dotnet-skills (3/5)** — Logging present but less consistent across services:
```csharp
// BookingService.cs (dotnet-skills)
_logger.LogInformation("Booking created: Member {MemberId} -> Class {ClassId}, Status: {Status}",
    dto.MemberId, dto.ClassScheduleId, booking.Status);
```

**Verdict**: All configurations use `ILogger<T>` with structured message templates. **dotnet-webapi** has the most thorough logging. None use `[LoggerMessage]` source generators.

---

## 19. Nullable Reference Types [MEDIUM]

All configurations enable NRT in the project file and use proper `?` annotations:

```xml
<!-- All .csproj files -->
<Nullable>enable</Nullable>
```

```csharp
// All configurations use proper nullable annotations
public int? WaitlistPosition { get; set; }
public DateTime? CheckInTime { get; set; }
public string? CancellationReason { get; set; }
public ClassSchedule ClassSchedule { get; set; } = null!;  // EF navigation
```

**Score**: All configurations — **5/5**

**Verdict**: No differentiation.

---

## 20. API Documentation [MEDIUM]

**dotnet-artisan (5/5)** and **dotnet-webapi (5/5)** — Full OpenAPI metadata on every endpoint:
```csharp
// Endpoints/BookingEndpoints.cs (dotnet-webapi)
group.MapPost("/", handler)
    .WithName("CreateBooking")
    .WithSummary("Book a class")
    .WithDescription("Books a class for a member. Enforces all booking rules...")
    .WithTags("Bookings")
    .Produces<BookingResponse>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);
```

**dotnet-skills (4/5)** — XML doc comments and `[ProducesResponseType]` attributes:
```csharp
// Controllers/BookingsController.cs (dotnet-skills)
/// <summary>Book a class (enforces all booking rules)</summary>
[HttpPost]
[ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
```

**managedcode (3/5)** and **no-skills (3/5)** — XML comments present but less metadata:
```csharp
// Controllers/MembersController.cs (no-skills)
/// <summary>List members with search, filter, and pagination</summary>
[HttpGet]
[ProducesResponseType(typeof(PaginatedResult<MemberListDto>), 200)]
public async Task<IActionResult> GetAll(...)
```

**Verdict**: Minimal API configurations produce richer OpenAPI metadata with `.WithName()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()`. Controller-based configs rely on XML comments and `[ProducesResponseType]` which generates less detailed documentation.

---

## 21. File Organization [MEDIUM]

**dotnet-artisan (5/5)** and **dotnet-webapi (5/5)** — Clean Minimal API structure with dedicated `Endpoints/` directory:
```
src/FitnessStudioApi/
├── Data/           # DbContext + DataSeeder
├── DTOs/           # 7-8 DTO files (one per entity)
├── Endpoints/      # 7 endpoint groups (extension methods)
├── Middleware/      # IExceptionHandler
├── Models/         # Entity classes
├── Services/       # Interface + implementation pairs
├── Program.cs      # 67-72 lines — clean composition root
```

**dotnet-skills (5/5)** and **managedcode (5/5)** — Well-organized controller structure:
```
src/FitnessStudioApi/
├── Controllers/    # 7 controller files
├── Services/
│   ├── Interfaces/ # 7 service interfaces
│   └── [implementations]
├── Models/         # Entity models
├── DTOs/           # DTO files
├── Data/           # DbContext + DataSeeder
├── Middleware/      # Exception handling
├── Program.cs      # 57-73 lines
```

**no-skills (4/5)** — Same structure but DTOs are in a single monolithic file (`Dtos.cs`, 180+ lines):
```
DTOs/
└── Dtos.cs  # ❌ All DTOs in one file — should be split per entity
```

**Verdict**: Most configurations organize well. **no-skills** loses a point for monolithic DTO file.

---

## 22. HTTP Test File Quality [MEDIUM]

All configurations provide comprehensive `.http` files covering all endpoints (35-40+ requests):

```http
@baseUrl = http://localhost:5223

### List all active membership plans
GET {{baseUrl}}/api/membership-plans

### Book a class (Alice books Day 4 Spin)
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{
  "classScheduleId": 9,
  "memberId": 1
}

### Test: Basic member tries to book premium class (should fail)
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{
  "classScheduleId": 5,
  "memberId": 3
}
```

| Config | Requests | Business Rule Tests | FK References |
|---|---|---|---|
| dotnet-artisan | 35+ | ✅ 4 error cases | ✅ Correct |
| dotnet-skills | 36+ | ✅ 4 error cases | ✅ Correct |
| dotnet-webapi | 38+ | ✅ 5 error cases | ✅ Correct |
| managedcode | 32+ | ✅ 3 error cases | ✅ Correct |
| no-skills | 35+ | ✅ 3 error cases | ✅ Correct |

**Score**: dotnet-artisan, dotnet-skills, dotnet-webapi, no-skills — **5/5**; managedcode — **4/5** (fewer test cases)

**Verdict**: All configurations provide high-quality `.http` files. **dotnet-webapi** has the most thorough coverage with 5 business rule test cases.

---

## 23. Type Design & Resource Management [MEDIUM]

All configurations use enums instead of magic strings and store them with `HasConversion<string>()`:

```csharp
// Models/Enums.cs (all configurations)
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }
public enum MembershipStatus { Active, Expired, Cancelled, Frozen }
public enum DifficultyLevel { Beginner, Intermediate, Advanced, AllLevels }

// FitnessDbContext.cs (all configurations)
entity.Property(e => e.Status).HasConversion<string>();
```

All configurations use `DateOnly` for dates without time, `decimal` for monetary values with `HasColumnType("decimal(10,2)")`, and `DateTime` for timestamps.

**Score**: All configurations — **5/5**

**Verdict**: No differentiation. All configurations demonstrate proper enum design and type precision.

---

## 24. Code Standards Compliance [LOW]

**dotnet-artisan (5/5)**, **dotnet-skills (5/5)**, **dotnet-webapi (5/5)**, and **no-skills (5/5)**:
```csharp
// Consistent across these configurations:
namespace FitnessStudioApi.Services;           // File-scoped
public sealed class BookingService : IBookingService  // Explicit access, sealed
public async Task<BookingDto> GetByIdAsync(...)       // Async suffix, PascalCase
public interface IBookingService { ... }              // I prefix
```

**managedcode (4/5)** — Good but constructor patterns use arrow-bodied syntax inconsistently:
```csharp
// managedcode
public BookingsController(IBookingService service) => _service = service;
// vs traditional constructors elsewhere
```

**Verdict**: All configurations follow .NET naming guidelines well.

---

## Weighted Summary

| Tier | Weight | Max Score |
|---|---|---|
| Critical (6 dims) | ×3 | 90 |
| High (8 dims) | ×2 | 80 |
| Medium (9 dims) | ×1 | 45 |
| Low (1 dim) | ×0.5 | 2.5 |
| **Total** | | **217.5** |

### Weighted Scores by Configuration

| Tier | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode | no-skills |
|---|---|---|---|---|---|
| Critical (×3) | 26 × 3 = **78** | 22 × 3 = **66** | 28 × 3 = **84** | 23 × 3 = **69** | 23 × 3 = **69** |
| High (×2) | 36 × 2 = **72** | 32 × 2 = **64** | 38 × 2 = **76** | 33 × 2 = **66** | 31 × 2 = **62** |
| Medium (×1) | **44** | **42** | **45** | **38** | **37** |
| Low (×0.5) | 5 × 0.5 = **2.5** | 5 × 0.5 = **2.5** | 5 × 0.5 = **2.5** | 4 × 0.5 = **2** | 5 × 0.5 = **2.5** |
| **Total** | **196.5** | **174.5** | **207.5** | **175** | **170.5** |
| **Percentage** | **90.3%** | **80.2%** | **95.4%** | **80.5%** | **78.4%** |

### Final Rankings

| Rank | Configuration | Weighted Score | Percentage |
|---|---|---|---|
| 🥇 1st | **dotnet-webapi** | 207.5 / 217.5 | **95.4%** |
| 🥈 2nd | **dotnet-artisan** | 196.5 / 217.5 | **90.3%** |
| 🥉 3rd | **managedcode-dotnet-skills** | 175.0 / 217.5 | **80.5%** |
| 4th | **dotnet-skills** | 174.5 / 217.5 | **80.2%** |
| 5th | **no-skills** | 170.5 / 217.5 | **78.4%** |

---

## What All Versions Get Right

- ✅ **Zero build errors, zero warnings** — all configurations compile cleanly on .NET 10
- ✅ **No vulnerable packages** — all use current, maintained NuGet packages
- ✅ **All 12 business rules implemented** — booking window, capacity/waitlist, cancellation policy, premium tier access, weekly limits, active membership, no double-booking, instructor conflicts, membership freeze, class cancellation cascade, check-in window, no-show flagging
- ✅ **Interface-based service layer** with scoped DI registration (`AddScoped<IService, Service>()`)
- ✅ **Record types for DTOs** — immutable, value-equality semantics
- ✅ **System.Text.Json** with `JsonStringEnumConverter` — no Newtonsoft.Json
- ✅ **Fluent API configuration** in `OnModelCreating` with unique indexes, relationship config, and `HasConversion<string>()` for enums
- ✅ **Nullable reference types** enabled project-wide with proper `?` annotations
- ✅ **File-scoped namespaces** across all source files
- ✅ **Comprehensive `.http` test files** covering all endpoints with realistic data
- ✅ **RFC 7807 ProblemDetails** for error responses
- ✅ **Realistic seed data** with idempotency checks (3 plans, 8 members, multiple membership states, 12+ schedules, 15+ bookings)
- ✅ **`Directory.Build.props`** with `.NET analyzers` and `Meziantou.Analyzer` for code quality enforcement
- ✅ **Type-safe enums** for all status fields (no magic strings or integers)
- ✅ **Structured logging** with `ILogger<T>` and named message template parameters

---

## Summary: Impact of Skills

### Most Impactful Differences (ranked by weighted score impact)

1. **Minimal API Architecture** (Critical, ×3): The single largest differentiator. Only **dotnet-webapi** and **dotnet-artisan** use Minimal APIs with route groups and TypedResults. This 3-point gap (5 vs 2) translates to **9 weighted points** — the largest single-dimension swing.

2. **EF Migration Usage** (Critical, ×3): Only **dotnet-webapi** uses `MigrateAsync()` with a proper Migrations folder. All others use `EnsureCreated()`. This 3-point gap accounts for **9 weighted points**.

3. **Async/CancellationToken Propagation** (High, ×2): Only **dotnet-webapi** and **dotnet-artisan** propagate `CancellationToken` through the full endpoint → service → EF Core chain. This 2-point gap is worth **4 weighted points**.

4. **Prefer Built-in over 3rd Party** (High, ×2): **dotnet-webapi** completely avoids Swashbuckle; others include it unnecessarily. A 2-point gap worth **4 weighted points**.

5. **Modern C# Adoption** (High, ×2): Primary constructors, collection expressions, and the `required` keyword appear only in **dotnet-webapi** and **dotnet-artisan**. The 2-point gap vs no-skills is worth **4 weighted points**.

6. **Sealed Types** (Medium, ×1): Three configurations consistently seal all types. **no-skills** seals nothing (4-point gap = **4 weighted points**).

### Overall Assessment

| Configuration | Assessment |
|---|---|
| **dotnet-webapi** (95.4%) | 🥇 **Reference-quality code.** The only configuration that uses EF migrations, avoids all unnecessary 3rd-party packages, and combines Minimal APIs with full TypedResults, Results<T1,T2> union types, and CancellationToken propagation. The sole weakness is floating NuGet version ranges (`10.*-*`). |
| **dotnet-artisan** (90.3%) | 🥈 **Excellent.** Matches dotnet-webapi on Minimal APIs, modern C#, async patterns, and sealed types. Falls behind only on EF migrations (uses EnsureCreated) and includes Swashbuckle + unused FluentValidation. The "simplicity first" philosophy produces very clean code. |
| **managedcode-dotnet-skills** (80.5%) | 🥉 **Good.** Uses controllers (not Minimal APIs) but gets IExceptionHandler correct, has the cleanest NuGet discipline (4 packages, all pinned), and implements all business rules. Weak on sealed types and CancellationToken propagation. |
| **dotnet-skills** (80.2%) | **Good.** Very similar to managedcode but uses FluentValidation (actually integrated), has sealed types, and uses EF.Functions.Like(). Uses legacy exception middleware rather than IExceptionHandler. |
| **no-skills** (78.4%) | **Adequate baseline.** Produces working, well-structured code but uses older patterns throughout: controllers, legacy middleware, no CancellationToken, no sealed types, no primary constructors, no AsNoTracking, EnsureCreated(). Serves as the quality floor that skills measurably improve upon. |

### Key Takeaway

The **dotnet-webapi** skill produces the highest-quality code by a significant margin (+11 points over dotnet-artisan, +37 points over no-skills). The two Minimal API configurations (dotnet-webapi and dotnet-artisan) form a clear top tier, while the three controller-based configurations cluster together at 78-81%. Skills make the biggest impact on **architectural decisions** (Minimal APIs, EF migrations, CancellationToken propagation) rather than business logic correctness, which all configurations handle well.
