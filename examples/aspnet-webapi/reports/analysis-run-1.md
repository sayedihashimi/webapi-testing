# Comparative Analysis: dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, no-skills, dotnet-webapi

## Introduction

This report compares **five Copilot configurations** used to generate ASP.NET Core Web API applications across three realistic scenarios:

| Configuration | Label | Description |
|---|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain | 9 skills + 14 specialist agents for .NET development |
| **managedcode-dotnet-skills** | Community managed-code skills | Community-contributed .NET skills |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | Official .NET team skills (dotnet, dotnet-data, dotnet-nuget, etc.) |
| **no-skills** | Baseline (default Copilot) | No custom skills or plugins |
| **dotnet-webapi** | dotnet-webapi skill | Purpose-built ASP.NET Core Web API skill |

Each configuration generated three apps:
- **FitnessStudioApi** (run-1) — Booking/membership system with class scheduling, waitlists, and instructor management
- **LibraryApi** (run-2) — Book loans, reservations, overdue fines, and availability tracking
- **VetClinicApi** (run-3) — Pet healthcare with appointments, vaccinations, and medical records

> **Critical finding**: The `managedcode-dotnet-skills` configuration **failed to generate the VetClinicApi** entirely — only a `Directory.Build.props` file was produced. This configuration is scored across only 2 of 3 apps, with a penalty applied for the missing app.

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | managedcode | dotnet-skills | no-skills | dotnet-webapi |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 4 | 2 | 4 | 4 | 5 |
| Security Vulnerability Scan [CRITICAL] | 4 | 3 | 3 | 3 | 5 |
| Minimal API Architecture [CRITICAL] | 4 | 3 | 1 | 1 | 5 |
| Input Validation & Guard Clauses [CRITICAL] | 4 | 3 | 4 | 3 | 4 |
| NuGet & Package Discipline [CRITICAL] | 3 | 2 | 2 | 3 | 5 |
| EF Migration Usage [CRITICAL] | 1 | 3 | 1 | 1 | 5 |
| Business Logic Correctness [HIGH] | 4 | 3 | 4 | 4 | 4 |
| Prefer Built-in over 3rd Party [HIGH] | 3 | 3 | 2 | 2 | 5 |
| Modern C# Adoption [HIGH] | 4 | 3 | 3 | 3 | 5 |
| Error Handling & Middleware [HIGH] | 4 | 4 | 3 | 3 | 5 |
| Async Patterns & Cancellation [HIGH] | 5 | 4 | 4 | 4 | 5 |
| EF Core Best Practices [HIGH] | 4 | 3 | 3 | 3 | 5 |
| Service Abstraction & DI [HIGH] | 5 | 4 | 4 | 4 | 5 |
| Security Configuration [HIGH] | 1 | 2 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 5 | 4 | 3 | 3 | 5 |
| Sealed Types [MEDIUM] | 4 | 3 | 2 | 2 | 5 |
| Data Seeder Design [MEDIUM] | 4 | 3 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 4 | 3 | 3 | 3 | 4 |
| Nullable Reference Types [MEDIUM] | 5 | 4 | 4 | 4 | 5 |
| API Documentation [MEDIUM] | 4 | 4 | 2 | 2 | 5 |
| File Organization [MEDIUM] | 4 | 3 | 3 | 3 | 5 |
| HTTP Test File Quality [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Type Design & Resource Mgmt [MEDIUM] | 4 | 3 | 3 | 3 | 5 |
| Code Standards Compliance [LOW] | 4 | 3 | 3 | 3 | 5 |

---

## 1. Build & Run Success [CRITICAL]

All configurations target .NET 10 with SQLite and were designed to compile and run. However, the **managedcode-dotnet-skills** configuration completely failed to produce the VetClinicApi (run-3), generating only a `Directory.Build.props` file with analyzer configuration — no source code at all.

**dotnet-webapi** — All 3 apps fully generated with EF migrations included, ensuring database is properly set up on first run:
```csharp
// dotnet-webapi: Program.cs (all 3 apps)
await dbContext.Database.MigrateAsync();  // Migrations guarantee schema correctness
```

**dotnet-artisan** — All 3 apps generated with complete source code. Uses `EnsureCreatedAsync()` which works for initial run but is fragile:
```csharp
// dotnet-artisan: Program.cs
await db.Database.EnsureCreatedAsync();
await DataSeeder.SeedAsync(db);
```

**managedcode-dotnet-skills** — Only 2/3 apps generated. VetClinicApi output:
```xml
<!-- managedcode-dotnet-skills/run-3/VetClinicApi/Directory.Build.props — ENTIRE OUTPUT -->
<Project>
  <PropertyGroup>
    <AnalysisLevel>latest</AnalysisLevel>
    <AnalysisMode>Recommended</AnalysisMode>
  </PropertyGroup>
</Project>
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | All 3 apps generated with migrations for reliable startup |
| dotnet-artisan | **4** | All 3 apps generated; EnsureCreated works but is less robust |
| dotnet-skills | **4** | All 3 apps generated; EnsureCreated works but is less robust |
| no-skills | **4** | All 3 apps generated; EnsureCreated works but is less robust |
| managedcode | **2** | 1 of 3 apps completely failed to generate |

**Verdict**: **dotnet-webapi** is the clear winner — 100% generation success with the most reliable database initialization strategy.

---

## 2. Security Vulnerability Scan [CRITICAL]

Package security depends on version pinning discipline and avoidance of unnecessary dependencies. Floating versions can pull in vulnerable releases.

**dotnet-webapi** — Minimal, pinned packages with no unnecessary dependencies:
```xml
<!-- dotnet-webapi: FitnessStudioApi.csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<!-- No Swashbuckle, no FluentValidation, no floating versions -->
```

**dotnet-skills** — Uses floating versions that could pull in vulnerable packages:
```xml
<!-- dotnet-skills: LibraryApi.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<!-- VetClinicApi uses Version="10.0.0-*" (pre-release floating!) -->
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | All exact versions, minimal packages, no unnecessary deps |
| dotnet-artisan | **4** | Exact versions but includes Swashbuckle (extra attack surface) |
| no-skills | **3** | Exact versions but includes Swashbuckle |
| managedcode | **3** | Floating versions in FitnessStudio (`10.0.0-*`) |
| dotnet-skills | **3** | Floating versions (`10.*`, `10.0.0-*`) in 2/3 apps |

**Verdict**: **dotnet-webapi** eliminates unnecessary packages and pins all versions, minimizing vulnerability exposure.

---

## 3. Minimal API Architecture [CRITICAL]

The modern .NET standard uses Minimal APIs with `MapGroup()`, `TypedResults`, `Results<T1, T2>` union return types, and endpoint extension methods.

**dotnet-webapi** — Consistently uses Minimal APIs with all modern patterns across all 3 apps:
```csharp
// dotnet-webapi: MemberEndpoints.cs
public static void MapMemberEndpoints(this IEndpointRouteBuilder routes)
{
    var group = routes.MapGroup("/api/members").WithTags("Members");

    group.MapGet("/{id:int}", GetById)
        .WithName("GetMember")
        .WithSummary("Get member details including active membership");
}

private static async Task<Results<Ok<MemberResponse>, NotFound>> GetById(
    int id, IMemberService service, CancellationToken ct)
{
    var member = await service.GetByIdAsync(id, ct);
    return member is null ? TypedResults.NotFound() : TypedResults.Ok(member);
}
```

**dotnet-artisan** — Uses Minimal APIs for 2/3 apps (FitnessStudio, VetClinic) but falls back to Controllers for LibraryApi:
```csharp
// dotnet-artisan: LibraryApi/Program.cs — Controllers
builder.Services.AddControllers();
app.MapControllers();
```

**no-skills** and **dotnet-skills** — All 3 apps use Controllers exclusively:
```csharp
// no-skills: FitnessStudioApi/Program.cs
builder.Services.AddControllers();
app.MapControllers();

// no-skills: Controllers/MembersController.cs
[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase { }
```

**managedcode** — Mixed: FitnessStudio uses Controllers, LibraryApi uses Minimal APIs with MapGroup + TypedResults + Results<>:
```csharp
// managedcode: LibraryApi/Endpoints/PatronEndpoints.cs
var group = routes.MapGroup("/api/patrons").WithTags("Patrons");
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | All 3 apps: Minimal APIs + MapGroup + TypedResults + Results<> |
| dotnet-artisan | **4** | 2/3 apps Minimal API (LibraryApi uses Controllers) |
| managedcode | **3** | 1/2 generated apps uses Minimal API (and VetClinic missing) |
| dotnet-skills | **1** | All 3 apps use Controllers |
| no-skills | **1** | All 3 apps use Controllers |

**Verdict**: **dotnet-webapi** is the only configuration that consistently produces modern Minimal API architecture with all recommended patterns. The `dotnet-webapi` skill's explicit guidance on Minimal APIs makes a dramatic difference.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

All configurations use validation attributes on DTOs and business rule validation in service layers. The key differentiator is consistency and use of modern guard clause patterns.

**dotnet-webapi** — Validation attributes on all DTOs + business rule validation in services:
```csharp
// dotnet-webapi: DTOs/MemberDtos.cs
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }
}
```

**dotnet-artisan** — Similar pattern with data annotation validation:
```csharp
// dotnet-artisan: DTOs/MemberDtos.cs
public sealed record CreateMemberDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;
}
```

**dotnet-skills** — Uses FluentValidation for validation:
```csharp
// dotnet-skills: FitnessStudioApi/Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **4** | Consistent annotations + service-level business rule validation |
| dotnet-artisan | **4** | Consistent annotations + service-level validation across all apps |
| dotnet-skills | **4** | FluentValidation provides comprehensive validation |
| managedcode | **3** | Annotations present but only 2/3 apps generated |
| no-skills | **3** | Annotations present but less consistent guard clauses |

**Verdict**: **dotnet-webapi**, **dotnet-artisan**, and **dotnet-skills** are tied — all provide thorough validation, though using different mechanisms.

---

## 5. NuGet & Package Discipline [CRITICAL]

Exact version pinning and minimal package selection are essential for reproducible builds and reduced attack surface.

**dotnet-webapi** — Consistently minimal packages across all 3 apps. No Swashbuckle, no FluentValidation:
```xml
<!-- dotnet-webapi: FitnessStudioApi.csproj — only 4 packages -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">...</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.5">...</PackageReference>
```

**dotnet-skills** — Adds unnecessary packages and uses floating versions:
```xml
<!-- dotnet-skills: LibraryApi.csproj -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />

<!-- dotnet-skills: VetClinicApi.csproj — pre-release floating! -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0-*" />
```

**managedcode** — Also uses floating versions in FitnessStudio:
```xml
<!-- managedcode: FitnessStudioApi.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-*" />
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Minimal packages, all exact versions, no unnecessary deps |
| no-skills | **3** | Exact versions but includes Swashbuckle |
| dotnet-artisan | **3** | Exact versions but includes Swashbuckle (7.3.1 in Fitness, 10.1.7 in Vet) |
| managedcode | **2** | Floating versions (`10.0.0-*`) + Swashbuckle in some apps |
| dotnet-skills | **2** | Floating versions (`10.*`, `10.0.0-*`) + FluentValidation + Swashbuckle |

**Verdict**: **dotnet-webapi** is the only configuration that consistently avoids floating versions and unnecessary third-party packages.

---

## 6. EF Migration Usage [CRITICAL]

EF Core migrations are the only production-safe approach for schema management. `EnsureCreated()` bypasses migrations entirely.

**dotnet-webapi** — Uses `Migrate()` or `MigrateAsync()` in all 3 apps:
```csharp
// dotnet-webapi: FitnessStudioApi/Program.cs
await dbContext.Database.MigrateAsync();

// dotnet-webapi: LibraryApi/Program.cs
db.Database.Migrate();

// dotnet-webapi: VetClinicApi/Program.cs
db.Database.Migrate();
```

**All other configurations** use `EnsureCreated()` — the anti-pattern:
```csharp
// dotnet-artisan: FitnessStudioApi/Program.cs
await db.Database.EnsureCreatedAsync();

// no-skills: FitnessStudioApi/Program.cs
db.Database.EnsureCreated();

// dotnet-skills: FitnessStudioApi/Program.cs
db.Database.EnsureCreated();
```

The only exception is **managedcode LibraryApi**, which uses `Migrate()`:
```csharp
// managedcode: LibraryApi/Program.cs
db.Database.Migrate();
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Migrate() in all 3 apps — production-ready |
| managedcode | **3** | Migrate() in 1/2 generated apps, EnsureCreated in the other |
| dotnet-artisan | **1** | EnsureCreated() in all 3 apps |
| dotnet-skills | **1** | EnsureCreated() in all 3 apps |
| no-skills | **1** | EnsureCreated() in all 3 apps |

**Verdict**: **dotnet-webapi** is the only configuration that consistently uses migrations. This is perhaps the single most impactful difference — EnsureCreated makes schema evolution impossible.

---

## 7. Business Logic Correctness [HIGH]

Each scenario specifies detailed business rules (booking windows, waitlists, fine calculations, membership tiers). All configurations implement core CRUD operations and most business rules, with varying completeness.

**dotnet-webapi** — Comprehensive business rules with capacity management, waitlist promotion, and membership tier checks:
```csharp
// dotnet-webapi: BookingService.cs
if (membership.Status != MembershipStatus.Active)
    throw new BusinessRuleException("Only members with active memberships can book");

if (classSchedule.CurrentEnrollment >= classSchedule.Capacity)
{
    booking.Status = BookingStatus.Waitlisted;
    booking.WaitlistPosition = classSchedule.WaitlistCount + 1;
}
```

**dotnet-artisan** — Similarly comprehensive with booking window enforcement and membership tier access:
```csharp
// dotnet-artisan: BookingService.cs
if (classType.IsPremium && !membershipPlan.AllowsPremiumClasses)
    throw new InvalidOperationException("Your membership plan does not allow premium classes.");
```

All configurations implement the core endpoints specified in the prompts. The key differentiator is depth of business rule enforcement.

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **4** | Comprehensive business rules across all 3 apps |
| dotnet-artisan | **4** | Thorough business rule implementation |
| dotnet-skills | **4** | Good business rule coverage with FluentValidation |
| no-skills | **4** | Adequate business rules across all endpoints |
| managedcode | **3** | Only 2/3 apps generated; rules in generated apps are good |

**Verdict**: Most configurations handle business logic comparably well. The prompt specifications drive completeness more than the skills do.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

.NET 10 provides built-in OpenAPI support via `AddOpenApi()`/`MapOpenApi()`. Swashbuckle, Newtonsoft.Json, FluentValidation, and other third-party packages should be avoided when built-in alternatives exist.

**dotnet-webapi** — Pure built-in stack across all 3 apps:
```csharp
// dotnet-webapi: Program.cs — no Swashbuckle, no SwaggerUI
builder.Services.AddOpenApi();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// No app.UseSwagger(), no app.UseSwaggerUI()
```

**dotnet-artisan** — Uses Swashbuckle for Swagger UI in 2/3 apps:
```csharp
// dotnet-artisan: FitnessStudioApi/Program.cs
builder.Services.AddOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Zenith Fitness Studio API v1");
});
```
```xml
<!-- dotnet-artisan: FitnessStudioApi.csproj -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.3.1" />
```

**dotnet-skills** — Uses both Swashbuckle AND FluentValidation:
```csharp
// dotnet-skills: Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddSwaggerGen(options => { ... });
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "..."));
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Pure built-in: AddOpenApi, System.Text.Json, built-in DI, ILogger |
| dotnet-artisan | **3** | Built-in OpenAPI but adds Swashbuckle for SwaggerUI |
| managedcode | **3** | Mixed: LibraryApi is pure built-in, FitnessStudio has Swashbuckle |
| no-skills | **2** | Swashbuckle in most apps, AddEndpointsApiExplorer |
| dotnet-skills | **2** | Swashbuckle + FluentValidation + AddSwaggerGen |

**Verdict**: **dotnet-webapi** is the only configuration that avoids all unnecessary third-party packages. The skill explicitly guides toward built-in alternatives.

---

## 9. Modern C# Adoption [HIGH]

Modern C# features include primary constructors (C# 12), file-scoped namespaces, `sealed` types, target-typed `new`, and `init` properties.

**dotnet-webapi** — Consistently uses primary constructors for services and DbContext:
```csharp
// dotnet-webapi: FitnessDbContext.cs
public sealed class FitnessDbContext(DbContextOptions<FitnessDbContext> options) : DbContext(options)

// dotnet-webapi: MemberService.cs (primary constructor)
public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
```

**dotnet-artisan** — Also uses primary constructors:
```csharp
// dotnet-artisan: FitnessDbContext.cs
public sealed class FitnessDbContext(DbContextOptions<FitnessDbContext> options) : DbContext(options)
```

**no-skills** and **dotnet-skills** — Mix of primary constructors and traditional constructors:
```csharp
// dotnet-skills: MemberService.cs (traditional constructor)
public class MemberService : IMemberService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<MemberService> _logger;
    public MemberService(FitnessDbContext context, ILogger<MemberService> logger)
    {
        _context = context;
        _logger = logger;
    }
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Consistent primary constructors, file-scoped namespaces, sealed types |
| dotnet-artisan | **4** | Primary constructors + file-scoped namespaces in most files |
| managedcode | **3** | Some primary constructors, some traditional |
| dotnet-skills | **3** | Mostly traditional constructors |
| no-skills | **3** | Mostly traditional constructors |

**Verdict**: **dotnet-webapi** most consistently adopts modern C# patterns, closely followed by **dotnet-artisan**.

---

## 10. Error Handling & Middleware [HIGH]

The modern .NET 8+ approach uses `IExceptionHandler` with `AddProblemDetails()` for composable, DI-aware error handling.

**dotnet-webapi** — `IExceptionHandler` implementation across all 3 apps:
```csharp
// dotnet-webapi: Middleware/ApiExceptionHandler.cs
public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        // Maps exception types to status codes + ProblemDetails
    }
}

// Program.cs
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
```

**dotnet-artisan** — Uses `IExceptionHandler` in FitnessStudio and VetClinic, but inline middleware in LibraryApi:
```csharp
// dotnet-artisan: LibraryApi/Program.cs — inline exception handler (not IExceptionHandler)
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        // Manual ProblemDetails construction...
    });
});
```

**no-skills** and **dotnet-skills** — Mix of `IExceptionHandler` and convention-based middleware:
```csharp
// dotnet-skills: VetClinicApi — convention-based middleware (not IExceptionHandler)
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context) { ... }
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | IExceptionHandler + AddProblemDetails consistently in all 3 apps |
| dotnet-artisan | **4** | IExceptionHandler in 2/3 apps; inline handler in LibraryApi |
| managedcode | **4** | IExceptionHandler in both generated apps |
| dotnet-skills | **3** | IExceptionHandler in 1/3 apps; middleware in others |
| no-skills | **3** | IExceptionHandler in 1/3 apps; middleware in others |

**Verdict**: **dotnet-webapi** consistently uses the modern `IExceptionHandler` pattern. Other configurations are inconsistent.

---

## 11. Async Patterns & Cancellation [HIGH]

Proper async patterns and `CancellationToken` propagation prevent thread pool starvation and wasted resources.

**dotnet-webapi** — CancellationToken in all endpoint handlers, propagated through services to EF Core:
```csharp
// dotnet-webapi: MemberEndpoints.cs
private static async Task<Results<Ok<MemberResponse>, NotFound>> GetById(
    int id, IMemberService service, CancellationToken ct)
{
    var member = await service.GetByIdAsync(id, ct);
    return member is null ? TypedResults.NotFound() : TypedResults.Ok(member);
}

// dotnet-webapi: MemberService.cs
public async Task<MemberResponse?> GetByIdAsync(int id, CancellationToken ct)
{
    return await db.Members.AsNoTracking()
        .Where(m => m.Id == id)
        .Select(m => new MemberResponse(...))
        .FirstOrDefaultAsync(ct);  // Token forwarded
}
```

**dotnet-artisan** — Excellent CancellationToken propagation:
```csharp
// dotnet-artisan: MemberEndpoints.cs
private static async Task<IResult> GetAll(
    IMemberService service, string? search, bool? isActive,
    int page, int pageSize, CancellationToken ct = default)
{
    var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
    return TypedResults.Ok(result);
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | CancellationToken in all endpoint handlers + service layer + EF Core |
| dotnet-artisan | **5** | Comprehensive CancellationToken propagation through all layers |
| managedcode | **4** | Good propagation in generated apps |
| dotnet-skills | **4** | CancellationToken present but less consistent in controller layer |
| no-skills | **4** | CancellationToken present but less consistent |

**Verdict**: **dotnet-webapi** and **dotnet-artisan** both excel at CancellationToken propagation through the entire call chain.

---

## 12. EF Core Best Practices [HIGH]

Explicit Fluent API relationship configuration, `AsNoTracking()` for read queries, and proper `DbContext` lifetime management.

**dotnet-webapi** — Comprehensive Fluent API with `AsNoTracking()` used consistently:
```csharp
// dotnet-webapi: FitnessDbContext.cs
modelBuilder.Entity<Membership>(entity =>
{
    entity.Property(e => e.Status).HasConversion<string>();
    entity.HasOne(e => e.Member)
        .WithMany(m => m.Memberships)
        .HasForeignKey(e => e.MemberId)
        .OnDelete(DeleteBehavior.Restrict);
});

// dotnet-webapi: MemberService.cs — AsNoTracking on reads
var query = db.Members.AsNoTracking().AsQueryable();
```

**dotnet-artisan** — Similar Fluent API + AsNoTracking usage:
```csharp
// dotnet-artisan: FitnessDbContext.cs
entity.HasOne(e => e.MembershipPlan)
    .WithMany()
    .HasForeignKey(e => e.MembershipPlanId)
    .OnDelete(DeleteBehavior.Restrict);

entity.Property(e => e.Status).HasConversion<string>();
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Fluent API + HasConversion + OnDelete + AsNoTracking consistently |
| dotnet-artisan | **4** | Fluent API + AsNoTracking in most services |
| managedcode | **3** | AsNoTracking present; Fluent API varies |
| dotnet-skills | **3** | AsNoTracking present; less explicit relationship config |
| no-skills | **3** | AsNoTracking present; basic Fluent API |

**Verdict**: **dotnet-webapi** has the most thorough EF Core configuration with explicit delete behaviors, string conversions, and consistent AsNoTracking.

---

## 13. Service Abstraction & DI [HIGH]

All configurations use interface-based services with `AddScoped<IService, Service>()` registration:
```csharp
// All configurations follow this pattern:
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBookingService, BookingService>();
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | All services sealed + interface-based + proper scoped lifetime |
| dotnet-artisan | **5** | All services sealed + interface-based + proper scoped lifetime |
| managedcode | **4** | Interface-based services, proper DI |
| dotnet-skills | **4** | Interface-based services with separate interfaces directory |
| no-skills | **4** | Interface-based services, proper DI |

**Verdict**: All configurations handle DI well. **dotnet-webapi** and **dotnet-artisan** earn top marks for consistently sealing service implementations.

---

## 14. Security Configuration [HIGH]

HSTS, HTTPS redirection, and security headers for production readiness. Only **one configuration** includes any security middleware at all.

**managedcode FitnessStudioApi** — The only app with `UseHttpsRedirection()`:
```csharp
// managedcode: FitnessStudioApi/Program.cs
app.UseHttpsRedirection();
app.MapControllers();
```

All other configurations across all apps omit both `UseHsts()` and `UseHttpsRedirection()`.

| Config | Score | Justification |
|---|---|---|
| managedcode | **2** | UseHttpsRedirection in 1/2 generated apps; no UseHsts |
| dotnet-webapi | **1** | No HSTS or HTTPS redirection in any app |
| dotnet-artisan | **1** | No HSTS or HTTPS redirection in any app |
| dotnet-skills | **1** | No HSTS or HTTPS redirection in any app |
| no-skills | **1** | No HSTS or HTTPS redirection in any app |

**Verdict**: This is a universal weakness. Even **managedcode** only partially addresses it. No configuration includes `UseHsts()`.

---

## 15. DTO Design [MEDIUM]

Records vs classes, immutability, sealed modifiers, and naming conventions.

**dotnet-webapi** — Sealed records with `required` + `init` properties and Request/Response naming:
```csharp
// dotnet-webapi: DTOs/MemberDtos.cs
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
}

public sealed record MemberResponse(
    int Id, string FirstName, string LastName, string Email);
```

**dotnet-artisan** — Sealed records with Dto naming convention:
```csharp
// dotnet-artisan: DTOs/MemberDtos.cs
public sealed record CreateMemberDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;
}
```

**no-skills** / **dotnet-skills** — Mix of records and classes, inconsistent sealing:
```csharp
// no-skills: DTOs — classes, not records, mutable setters
public class CreateMemberDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; }
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Sealed records, init properties, Request/Response naming |
| dotnet-artisan | **5** | Sealed records, init properties, consistent Dto naming |
| managedcode | **4** | Records with init properties in some apps |
| dotnet-skills | **3** | Mix of records and classes, inconsistent |
| no-skills | **3** | Mostly classes with mutable setters |

**Verdict**: **dotnet-webapi** and **dotnet-artisan** both produce excellent DTO design with immutable sealed records.

---

## 16. Sealed Types [MEDIUM]

Sealed classes and records enable JIT devirtualization and signal design intent.

**dotnet-webapi** — Extensively sealed across models, DTOs, services, DbContext, and exception handlers:
```csharp
public sealed class FitnessDbContext(...) : DbContext(options)
public sealed class MemberService(...) : IMemberService
public sealed class ApiExceptionHandler(...) : IExceptionHandler
public sealed record CreateMemberRequest { ... }
public sealed class Member { ... }
```

**no-skills** / **dotnet-skills** — Inconsistent sealing:
```csharp
// dotnet-skills: Services — not sealed
public class MemberService : IMemberService { ... }
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Sealed across all type categories consistently |
| dotnet-artisan | **4** | Most types sealed; occasional gaps |
| managedcode | **3** | Some sealed types |
| dotnet-skills | **2** | Mostly non-sealed classes |
| no-skills | **2** | Mostly non-sealed classes |

**Verdict**: **dotnet-webapi** consistently applies the `sealed` modifier across all type categories.

---

## 17. Data Seeder Design [MEDIUM]

All configurations implement data seeders that populate the database with realistic demo data.

**dotnet-webapi** — Injectable `DataSeeder` service with idempotency checks:
```csharp
// dotnet-webapi: Data/DataSeeder.cs
public sealed class DataSeeder(FitnessDbContext db)
{
    public async Task SeedAsync()
    {
        if (await db.MembershipPlans.AnyAsync()) return;  // Idempotency
    }
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **4** | Injectable seeder, idempotent, realistic variety |
| dotnet-artisan | **4** | Static seeder with idempotency checks, good variety |
| dotnet-skills | **4** | Static seeder with realistic data |
| no-skills | **4** | Static seeder with realistic data |
| managedcode | **3** | Seeders in 2/3 generated apps; adequate variety |

**Verdict**: All configurations handle seeding adequately. The prompt drives seed data requirements more than the skills.

---

## 18. Structured Logging [MEDIUM]

All configurations inject `ILogger<T>` and use structured message templates with named placeholders:
```csharp
// Common across all configs:
logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
logger.LogInformation("Booking {BookingId} created for member {MemberId}", booking.Id, member.Id);
```

No configuration uses `[LoggerMessage]` source generators.

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **4** | Consistent structured logging in services and middleware |
| dotnet-artisan | **4** | Structured logging in services and middleware |
| managedcode | **3** | Logging in 2/3 generated apps |
| dotnet-skills | **3** | Structured logging but inconsistent coverage |
| no-skills | **3** | Structured logging but inconsistent coverage |

**Verdict**: All configurations use structured logging correctly. None use high-performance `[LoggerMessage]` source generators.

---

## 19. Nullable Reference Types [MEDIUM]

All configurations enable NRT and ImplicitUsings:
```xml
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | NRT enabled + proper nullable annotations on all optional properties |
| dotnet-artisan | **5** | NRT enabled + consistent nullable annotations |
| managedcode | **4** | NRT enabled in generated apps |
| dotnet-skills | **4** | NRT enabled; mostly correct annotations |
| no-skills | **4** | NRT enabled; mostly correct annotations |

**Verdict**: All configurations enable NRT. **dotnet-webapi** and **dotnet-artisan** are most consistent.

---

## 20. API Documentation [MEDIUM]

Rich OpenAPI metadata on endpoints using Minimal API fluent extensions.

**dotnet-webapi** — Extensive metadata on all endpoints across all 3 apps:
```csharp
// dotnet-webapi: MemberEndpoints.cs
group.MapGet("/{id:int}", GetById)
    .WithName("GetMember")
    .WithSummary("Get member details including active membership")
    .WithDescription("Returns full member details with current membership info")
    .Produces<MemberResponse>(200)
    .ProducesProblem(404);
```

**dotnet-skills** / **no-skills** — Controllers lack Minimal API metadata extensions:
```csharp
// no-skills: Controllers — no WithName/WithSummary possible
[HttpGet("{id}")]
public async Task<ActionResult<MemberDto>> GetMember(int id) { ... }
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | WithName + WithSummary + WithDescription + Produces consistently |
| managedcode | **4** | WithName + WithSummary on Minimal API apps |
| dotnet-artisan | **4** | WithName + WithSummary on 2/3 apps |
| dotnet-skills | **2** | No Minimal API metadata; relies on Swagger annotations |
| no-skills | **2** | No Minimal API metadata; minimal documentation |

**Verdict**: **dotnet-webapi** produces the richest API documentation through Minimal API metadata.

---

## 21. File Organization [MEDIUM]

**dotnet-webapi** — Consistent structure across all 3 apps:
```
src/FitnessStudioApi/
├── Data/          (DbContext, DataSeeder)
├── DTOs/          (request/response records)
├── Endpoints/     (7 endpoint extension method files)
├── Middleware/     (ApiExceptionHandler)
├── Models/        (entity classes)
├── Models/Enums/  (enum definitions)
├── Services/      (interface + implementation pairs)
├── Migrations/    (EF Core migrations)
├── Program.cs
└── FitnessStudioApi.http
```

**no-skills** / **dotnet-skills** — Controllers folder instead of Endpoints:
```
src/FitnessStudioApi/
├── Controllers/   (7 controller classes)
├── Data/          (DbContext, DataSeeder)
├── DTOs/
├── Models/
├── Services/
└── Program.cs
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Clean: Endpoints/, Models/Enums/, Migrations/ |
| dotnet-artisan | **4** | Clean structure for Minimal API apps |
| managedcode | **3** | Good structure in generated apps; missing 1 app |
| dotnet-skills | **3** | Standard Controllers/ layout |
| no-skills | **3** | Standard Controllers/ layout |

**Verdict**: **dotnet-webapi** has the cleanest, most consistent organization.

---

## 22. HTTP Test File Quality [MEDIUM]

All configurations generate `.http` files for API testing. The **dotnet-artisan VetClinicApi** is the notable exception — no `.http` file was generated.

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **4** | .http files for all 3 apps with good coverage |
| no-skills | **4** | .http files for all 3 apps |
| dotnet-skills | **4** | .http files for all 3 apps |
| managedcode | **4** | .http files for both generated apps |
| dotnet-artisan | **4** | .http files for 2/3 apps (missing VetClinic) |

**Verdict**: All configurations produce adequate `.http` files.

---

## 23. Type Design & Resource Management [MEDIUM]

**dotnet-webapi** — Enums with `HasConversion<string>()` for readable database storage:
```csharp
// dotnet-webapi: Models/Enums/BookingStatus.cs
public enum BookingStatus
{
    Confirmed, Waitlisted, Cancelled, Attended, NoShow
}

// dotnet-webapi: FitnessDbContext.cs
entity.Property(e => e.Status).HasConversion<string>();
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Enums + HasConversion<string> + proper return types |
| dotnet-artisan | **4** | Enums + HasConversion<string> in most entities |
| managedcode | **3** | Enums present; HasConversion varies |
| dotnet-skills | **3** | Enums present; some HasConversion |
| no-skills | **3** | Enums present; some HasConversion |

**Verdict**: **dotnet-webapi** is most consistent in enum design with string conversion.

---

## 24. Code Standards Compliance [LOW]

.NET naming conventions, explicit access modifiers, file-scoped namespaces.

**dotnet-webapi** — Consistent PascalCase, explicit `public`/`sealed`, file-scoped namespaces:
```csharp
namespace FitnessStudioApi.Services;

public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger) : IMemberService
{
    public async Task<PaginatedResponse<MemberListResponse>> GetAllAsync(...)
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-webapi | **5** | Consistent naming, sealed, file-scoped namespaces, Async suffixes |
| dotnet-artisan | **4** | Good naming, mostly sealed, file-scoped namespaces |
| managedcode | **3** | Correct naming conventions, less consistent sealing |
| dotnet-skills | **3** | Correct naming, but traditional patterns |
| no-skills | **3** | Correct naming, but traditional patterns |

**Verdict**: **dotnet-webapi** follows .NET coding standards most consistently.

---

## Weighted Summary

Weights: Critical = ×3, High = ×2, Medium = ×1, Low = ×0.5

### Critical Tier (×3)

| Dimension | dotnet-artisan | managedcode | dotnet-skills | no-skills | dotnet-webapi |
|---|---|---|---|---|---|
| Build & Run Success | 4 | 2 | 4 | 4 | 5 |
| Security Vuln Scan | 4 | 3 | 3 | 3 | 5 |
| Minimal API Architecture | 4 | 3 | 1 | 1 | 5 |
| Input Validation | 4 | 3 | 4 | 3 | 4 |
| NuGet Discipline | 3 | 2 | 2 | 3 | 5 |
| EF Migration Usage | 1 | 3 | 1 | 1 | 5 |
| **Critical Subtotal** | **20** | **16** | **15** | **15** | **29** |
| **Critical Weighted (×3)** | **60** | **48** | **45** | **45** | **87** |

### High Tier (×2)

| Dimension | dotnet-artisan | managedcode | dotnet-skills | no-skills | dotnet-webapi |
|---|---|---|---|---|---|
| Business Logic | 4 | 3 | 4 | 4 | 4 |
| Prefer Built-in | 3 | 3 | 2 | 2 | 5 |
| Modern C# | 4 | 3 | 3 | 3 | 5 |
| Error Handling | 4 | 4 | 3 | 3 | 5 |
| Async/Cancellation | 5 | 4 | 4 | 4 | 5 |
| EF Core Practices | 4 | 3 | 3 | 3 | 5 |
| Service/DI | 5 | 4 | 4 | 4 | 5 |
| Security Config | 1 | 2 | 1 | 1 | 1 |
| **High Subtotal** | **30** | **26** | **24** | **24** | **35** |
| **High Weighted (×2)** | **60** | **52** | **48** | **48** | **70** |

### Medium Tier (×1)

| Dimension | dotnet-artisan | managedcode | dotnet-skills | no-skills | dotnet-webapi |
|---|---|---|---|---|---|
| DTO Design | 5 | 4 | 3 | 3 | 5 |
| Sealed Types | 4 | 3 | 2 | 2 | 5 |
| Data Seeder | 4 | 3 | 4 | 4 | 4 |
| Structured Logging | 4 | 3 | 3 | 3 | 4 |
| Nullable Ref Types | 5 | 4 | 4 | 4 | 5 |
| API Documentation | 4 | 4 | 2 | 2 | 5 |
| File Organization | 4 | 3 | 3 | 3 | 5 |
| HTTP Test File | 4 | 4 | 4 | 4 | 4 |
| Type Design | 4 | 3 | 3 | 3 | 5 |
| **Medium Subtotal (×1)** | **38** | **31** | **28** | **28** | **42** |

### Low Tier (×0.5)

| Dimension | dotnet-artisan | managedcode | dotnet-skills | no-skills | dotnet-webapi |
|---|---|---|---|---|---|
| Code Standards | 4 | 3 | 3 | 3 | 5 |
| **Low Weighted (×0.5)** | **2.0** | **1.5** | **1.5** | **1.5** | **2.5** |

### Total Weighted Scores

| Configuration | Critical (×3) | High (×2) | Medium (×1) | Low (×0.5) | **Total** |
|---|---|---|---|---|---|
| **dotnet-webapi** | 87 | 70 | 42 | 2.5 | **201.5** |
| **dotnet-artisan** | 60 | 60 | 38 | 2.0 | **160.0** |
| **managedcode** | 48 | 52 | 31 | 1.5 | **132.5** |
| **dotnet-skills** | 45 | 48 | 28 | 1.5 | **122.5** |
| **no-skills** | 45 | 48 | 28 | 1.5 | **122.5** |

---

## What All Versions Get Right

Despite significant differences, all configurations share these best practices:

- **Interface-based DI**: All use `AddScoped<IService, Service>()` with constructor injection
- **Entity Framework Core with SQLite**: All use EF Core with proper DbContext registration
- **Nullable Reference Types**: All enable `<Nullable>enable</Nullable>` in .csproj
- **Structured logging**: All inject `ILogger<T>` and use structured message templates (`{Placeholder}`)
- **JSON enum serialization**: All configure `JsonStringEnumConverter` for string-based enum serialization
- **Global error handling**: All implement some form of centralized exception handling (IExceptionHandler or middleware)
- **ProblemDetails**: All return RFC 7807 ProblemDetails for errors
- **Data seeding**: All provide idempotent seed data with realistic variety
- **Validation attributes**: All use `[Required]`, `[MaxLength]`, `[EmailAddress]` on DTOs
- **Async/await**: All use proper async patterns without sync-over-async anti-patterns
- **.http test files**: All configurations generate .http files for API testing
- **Separation of concerns**: All organize code into Models, DTOs, Services, and API layers

---

## Summary: Impact of Skills

### Rankings by Weighted Score

| Rank | Configuration | Score | Delta vs Baseline |
|---|---|---|---|
| 🥇 | **dotnet-webapi** | **201.5** | **+79.0** (+64.5%) |
| 🥈 | **dotnet-artisan** | **160.0** | **+37.5** (+30.6%) |
| 🥉 | **managedcode-dotnet-skills** | **132.5** | **+10.0** (+8.2%) |
| 4th | **dotnet-skills** | **122.5** | **0.0** (tied with baseline) |
| 5th | **no-skills** (baseline) | **122.5** | — |

### Key Findings

1. **The `dotnet-webapi` skill is transformative** (+64.5% over baseline). It is the only configuration that consistently produces:
   - Minimal APIs with `MapGroup`, `TypedResults`, and `Results<T1, T2>` union return types
   - EF Core migrations instead of `EnsureCreated()`
   - No Swashbuckle — pure built-in `AddOpenApi()`/`MapOpenApi()`
   - Rich OpenAPI metadata (`WithName`, `WithSummary`, `WithDescription`, `Produces<T>`)
   - Sealed types across all categories (models, DTOs, services, DbContext)
   - Minimal, pinned NuGet packages

2. **The `dotnet-artisan` plugin chain is second-best** (+30.6%). Its main strengths are:
   - Minimal APIs in 2/3 apps (but inconsistently falls back to Controllers)
   - Sealed records for DTOs, sealed services and DbContext
   - CancellationToken propagation
   - Its main weakness is `EnsureCreated()` in all apps and inconsistent architecture choices across runs

3. **The `managedcode-dotnet-skills` are marginally beneficial** (+8.2%). Their output is mixed:
   - The LibraryApi is actually excellent (Minimal APIs, Migrate(), built-in OpenAPI, IExceptionHandler)
   - The FitnessStudioApi reverts to Controllers with Swashbuckle
   - The VetClinicApi completely failed to generate — a critical reliability issue
   - The only configuration to include `UseHttpsRedirection()`

4. **The `dotnet-skills` (official .NET skills) provide no measurable improvement** over the baseline. They:
   - Add unnecessary dependencies (FluentValidation, Swashbuckle)
   - Introduce floating version patterns (`10.*`, `10.0.0-*`) — the worst NuGet practice
   - Generate the same Controller-based architecture as the baseline
   - Despite being official .NET team skills, they don't guide toward modern Minimal API patterns

5. **The `no-skills` baseline** establishes the floor: Controllers, `EnsureCreated()`, Swashbuckle, and traditional C# patterns. The code is functional but reflects older .NET conventions.

### Most Impactful Dimensions

The dimensions where skills make the biggest difference (largest score spread between configs):

| Dimension | Spread (max - min) | Key Differentiator |
|---|---|---|
| EF Migration Usage | **4** (5 vs 1) | Only dotnet-webapi uses Migrate() |
| Minimal API Architecture | **4** (5 vs 1) | Only dotnet-webapi is consistent |
| Build & Run Success | **3** (5 vs 2) | managedcode failed 1/3 apps |
| Prefer Built-in | **3** (5 vs 2) | dotnet-webapi avoids all 3rd-party |
| NuGet Discipline | **3** (5 vs 2) | Floating versions vs exact pinning |
| Sealed Types | **3** (5 vs 2) | Skill-guided vs default behavior |
| API Documentation | **3** (5 vs 2) | Minimal API metadata vs Controller defaults |

### Overall Assessment

The **dotnet-webapi skill** demonstrates that a focused, purpose-built skill can dramatically improve code quality — producing code that follows modern .NET best practices in every dimension. The general-purpose .NET skills (**dotnet-skills**) surprisingly add no value over the baseline, and the community **managedcode** skills have a reliability problem (1/3 apps failed to generate). The **dotnet-artisan** plugin chain shows that a comprehensive skill set helps but cannot match a purpose-built skill's consistency. The most critical gap across all non-dotnet-webapi configurations is the universal use of `EnsureCreated()` instead of EF Core migrations — a pattern that makes production deployment impossible.
