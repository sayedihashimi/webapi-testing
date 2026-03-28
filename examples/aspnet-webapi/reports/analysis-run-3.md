# Comparative Analysis: no-skills, dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, dotnet-webapi

## Introduction

This report compares **5 Copilot skill configurations**, each generating the same **VetClinicApi** application — a veterinary clinic management system with pet owners, pets, veterinarians, appointments, medical records, prescriptions, and vaccination tracking. All projects target .NET 10 with Entity Framework Core and SQLite.

| Configuration | Description | API Style | Key Packages |
|---|---|---|---|
| **no-skills** | Baseline (default Copilot) | Controllers | Swashbuckle, EF Core (wildcards) |
| **dotnet-artisan** | dotnet-artisan plugin chain | Minimal APIs | EF Core (wildcards), Meziantou Analyzer |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | Controllers | Swashbuckle, FluentValidation, EF Core (wildcards) |
| **dotnet-webapi** | dotnet-webapi skill | Minimal APIs | EF Core (pinned) |
| **managedcode-dotnet-skills** | Community managed-code skills | Minimal APIs | EF Core (pinned) |

The analysis covers the **VetClinicApi** scenario from `run-3` across all configurations. Each dimension is scored on a **1–5 scale** (1=missing/broken, 5=excellent).

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 4 | 4 | 4 | 2 | 4 |
| Security Vulnerability Scan [CRITICAL] | 3 | 3 | 2 | 4 | 4 |
| Minimal API Architecture [CRITICAL] | 1 | 5 | 1 | 5 | 5 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 3 | 4 | 3 | 3 |
| NuGet & Package Discipline [CRITICAL] | 2 | 2 | 1 | 5 | 5 |
| EF Migration Usage [CRITICAL] | 2 | 2 | 2 | 3 | 2 |
| Business Logic Correctness [HIGH] | 4 | 5 | 4 | 3 | 4 |
| Prefer Built-in over 3rd Party [HIGH] | 2 | 5 | 1 | 5 | 5 |
| Modern C# Adoption [HIGH] | 1 | 5 | 2 | 5 | 5 |
| Error Handling & Middleware [HIGH] | 3 | 5 | 3 | 5 | 5 |
| Async Patterns & Cancellation [HIGH] | 2 | 5 | 2 | 5 | 5 |
| EF Core Best Practices [HIGH] | 2 | 5 | 4 | 5 | 5 |
| Service Abstraction & DI [HIGH] | 4 | 4 | 4 | 4 | 4 |
| Security Configuration [HIGH] | 1 | 1 | 2 | 1 | 1 |
| DTO Design [MEDIUM] | 2 | 5 | 3 | 5 | 5 |
| Sealed Types [MEDIUM] | 1 | 5 | 2 | 5 | 5 |
| Data Seeder Design [MEDIUM] | 3 | 3 | 3 | 1 | 3 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 3 | 5 | 3 | 5 | 5 |
| File Organization [MEDIUM] | 3 | 5 | 3 | 4 | 5 |
| HTTP Test File Quality [MEDIUM] | 4 | 4 | 4 | 1 | 4 |
| Type Design & Resource Management [MEDIUM] | 3 | 5 | 3 | 5 | 5 |
| Code Standards Compliance [LOW] | 3 | 5 | 3 | 5 | 5 |

---

## 1. Build & Run Success [CRITICAL]

### What each configuration does

**no-skills, dotnet-artisan, dotnet-skills, managedcode** all use `EnsureCreatedAsync()` followed by a data seeder, which creates the database schema and populates it with demo data:

```csharp
// no-skills, dotnet-artisan, managedcode — Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(db);
}
```

**dotnet-webapi** calls `Database.Migrate()` without any Migrations directory and without a data seeder:

```csharp
// dotnet-webapi — Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    db.Database.Migrate();
}
// No DataSeeder call. No Migrations/ directory exists.
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | Schema created via EnsureCreated, seeder populates data. Wildcard package versions are a minor risk. |
| dotnet-artisan | 4 | Same pattern; app starts and responds with seeded data. |
| dotnet-skills | 4 | Same pattern; FluentValidation wired correctly. |
| dotnet-webapi | **2** | `Migrate()` with zero migrations creates an empty database — no tables, no data. Endpoints would throw at runtime. |
| managedcode | 4 | Same EnsureCreated + seeder pattern; functional on startup. |

**Verdict**: dotnet-webapi's use of `Migrate()` is the correct *concept* but without a Migrations directory or seeder, the app is non-functional. All others work out of the box.

---

## 2. Security Vulnerability Scan [CRITICAL]

### What each configuration does

**dotnet-webapi** and **managedcode** use only essential framework packages with pinned versions:

```xml
<!-- dotnet-webapi — VetClinicApi.csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```

**dotnet-skills** includes the **deprecated** `FluentValidation.AspNetCore` package plus Swashbuckle:

```xml
<!-- dotnet-skills — VetClinicApi.csproj -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

**no-skills** includes Swashbuckle with wildcard EF Core versions. **dotnet-artisan** uses wildcards for all packages and adds `Meziantou.Analyzer Version="*"` in `Directory.Build.props`.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Swashbuckle adds attack surface; wildcard versions risk pulling vulnerable releases. |
| dotnet-artisan | 3 | Wildcard versions are risky; Meziantou is dev-only (PrivateAssets) so mitigated. |
| dotnet-skills | **2** | `FluentValidation.AspNetCore` is deprecated; Swashbuckle unnecessary; wildcards compound risk. |
| dotnet-webapi | **4** | Minimal packages, all pinned. Lowest attack surface. |
| managedcode | **4** | Same minimal, pinned approach. |

**Verdict**: dotnet-webapi and managedcode are safest with minimal pinned packages. dotnet-skills is weakest due to the deprecated FluentValidation.AspNetCore package.

---

## 3. Minimal API Architecture [CRITICAL]

### What each configuration does

**no-skills** and **dotnet-skills** use traditional `[ApiController]` controllers:

```csharp
// no-skills — OwnersController.cs
[ApiController]
[Route("api/owners")]
[Produces("application/json")]
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _ownerService;
    public OwnersController(IOwnerService ownerService) => _ownerService = ownerService;

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<OwnerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, ...) => Ok(await ...);
}
```

**dotnet-artisan, dotnet-webapi, managedcode** use Minimal APIs with `MapGroup()`, `TypedResults`, and `Results<T1, T2>` union return types:

```csharp
// dotnet-artisan — OwnerEndpoints.cs
public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/owners").WithTags("Owners");

        group.MapGet("/{id:int}", async Task<Results<Ok<OwnerResponse>, NotFound>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.GetByIdAsync(id, ct);
            return owner is null ? TypedResults.NotFound() : TypedResults.Ok(owner);
        })
        .WithName("GetOwnerById")
        .WithSummary("Get owner by ID");

        return group;
    }
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **1** | Controllers only — no MapGroup, no TypedResults, no route constraints. |
| dotnet-artisan | **5** | Full Minimal API with MapGroup, TypedResults, Results<> unions, route constraints. |
| dotnet-skills | **1** | Controllers only — same legacy pattern as no-skills. |
| dotnet-webapi | **5** | Full Minimal API with MapGroup, TypedResults, Results<> unions. |
| managedcode | **5** | Full Minimal API, identical modern pattern. |

**Verdict**: dotnet-artisan, dotnet-webapi, and managedcode all produce modern Minimal APIs. no-skills and dotnet-skills generate the legacy controller pattern.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

### What each configuration does

All configurations use **Data Annotations** on DTOs (`[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`). **dotnet-skills** adds **FluentValidation** with auto-validation:

```csharp
// dotnet-skills — Validators.cs
public class CreateOwnerDtoValidator : AbstractValidator<CreateOwnerDto>
{
    public CreateOwnerDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
    }
}

// dotnet-skills — Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
```

All configurations validate business rules in the service layer (e.g., appointment scheduling conflicts, status transitions). None use modern `ArgumentNullException.ThrowIfNull()` guard clauses on constructor parameters — the skill-based configs use primary constructors which make explicit guards unnecessary for DI parameters.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Data Annotations on DTOs; service-level business validation; no guard clauses. |
| dotnet-artisan | 3 | Data Annotations; service-level validation; primary constructors eliminate need for DI guards. |
| dotnet-skills | **4** | FluentValidation pipeline adds automatic validation; dual validation (annotations + fluent). |
| dotnet-webapi | 3 | Data Annotations; service-level validation; primary constructors. |
| managedcode | 3 | Same pattern as dotnet-webapi. |

**Verdict**: dotnet-skills edges ahead with FluentValidation auto-validation, though using a deprecated package. All others are adequate with Data Annotations.

---

## 5. NuGet & Package Discipline [CRITICAL]

### What each configuration does

**dotnet-webapi** and **managedcode** pin every package to exact versions:

```xml
<!-- managedcode — VetClinicApi.csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.4">
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.4" />
<!-- Only 3 packages, all pinned, no unnecessary dependencies -->
```

**no-skills** and **dotnet-artisan** use floating wildcard versions `10.*-*`:

```xml
<!-- no-skills — VetClinicApi.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

**dotnet-artisan** additionally uses `Version="*"` (worst-case wildcard) for an analyzer in `Directory.Build.props`:

```xml
<!-- dotnet-artisan — Directory.Build.props -->
<PackageReference Include="Meziantou.Analyzer" Version="*">
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

**dotnet-skills** is worst — wildcards, Swashbuckle, and the deprecated `FluentValidation.AspNetCore`:

```xml
<!-- dotnet-skills — adds unnecessary/deprecated packages + wildcards -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Two wildcard versions; unnecessary Swashbuckle package. |
| dotnet-artisan | 2 | Three wildcard versions in csproj; `Version="*"` for analyzer (mitigated by PrivateAssets). |
| dotnet-skills | **1** | Wildcards + deprecated package + unnecessary Swashbuckle = worst discipline. |
| dotnet-webapi | **5** | All exact versions; only essential packages. |
| managedcode | **5** | All exact versions; only essential packages. |

**Verdict**: dotnet-webapi and managedcode demonstrate ideal package discipline. dotnet-skills is the worst with deprecated packages and wildcards.

---

## 6. EF Migration Usage [CRITICAL]

### What each configuration does

**no-skills, dotnet-artisan, dotnet-skills, managedcode** all use the `EnsureCreated` anti-pattern:

```csharp
// All four configs — Program.cs
await context.Database.EnsureCreatedAsync();
```

**dotnet-webapi** uses the correct `Migrate()` approach but lacks a Migrations directory:

```csharp
// dotnet-webapi — Program.cs
db.Database.Migrate();
// No Migrations/ directory exists — no migrations to apply
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | EnsureCreated bypasses migrations; works but schema cannot evolve. |
| dotnet-artisan | 2 | Same EnsureCreated anti-pattern. |
| dotnet-skills | 2 | Same anti-pattern. |
| dotnet-webapi | **3** | Correct approach (`Migrate`), but incomplete (no migrations generated). |
| managedcode | 2 | Same EnsureCreated anti-pattern. |

**Verdict**: dotnet-webapi shows awareness of the correct migration-based approach but fails to follow through. All others default to the EnsureCreated anti-pattern. No configuration achieves a production-ready migration workflow.

---

## 7. Business Logic Correctness [HIGH]

### What each configuration does

All five configurations implement the core business rules: appointment conflict detection, status workflow transitions, cancellation rules, medical record creation constraints, soft-delete for pets, and vaccination tracking. The spec defines **30+ endpoints** across 7 resource groups.

**dotnet-artisan** implements all endpoints with comprehensive validation including checking for active pets during appointment creation:

```csharp
// dotnet-artisan — AppointmentService.cs
if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
    throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

if (request.AppointmentDate <= DateTime.UtcNow)
    throw new ArgumentException("Appointment date must be in the future.");

await CheckSchedulingConflictAsync(request.VeterinarianId, request.AppointmentDate,
    request.DurationMinutes, excludeAppointmentId: null, ct);
```

**dotnet-webapi** is missing critical runtime components: no `DataSeeder`, no Migrations directory, and no `.http` file. While endpoint definitions are correct, the app has no demo data and a broken database initialization:

```csharp
// dotnet-webapi — Program.cs (entire database init)
db.Database.Migrate();
// No seeder. No .http file. No seed data.
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | All endpoints implemented; all business rules enforced; seed data present. |
| dotnet-artisan | **5** | Complete endpoint coverage; thorough validation; active pet check on appointment creation. |
| dotnet-skills | 4 | All endpoints; all business rules; FluentValidation adds robustness. |
| dotnet-webapi | **3** | Correct endpoint definitions but no seed data, no seeder, broken database init. |
| managedcode | 4 | All endpoints and business rules implemented with seed data. |

**Verdict**: dotnet-artisan is most complete. dotnet-webapi's endpoint code is sound but the missing seeder and broken initialization reduce functional completeness.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

### What each configuration does

**dotnet-artisan, dotnet-webapi, managedcode** use only built-in OpenAPI:

```csharp
// dotnet-artisan — Program.cs
builder.Services.AddOpenApi();
// No Swashbuckle, no app.UseSwagger()
```

**no-skills** uses both built-in OpenAPI AND Swashbuckle:

```csharp
// no-skills — Program.cs
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { ... });
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint(...));
```

**dotnet-skills** uses Swashbuckle plus the deprecated `FluentValidation.AspNetCore`:

```csharp
// dotnet-skills — Program.cs
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation(); // deprecated package
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
app.UseSwagger();
app.UseSwaggerUI();
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Dual OpenAPI + Swashbuckle; redundant configuration. |
| dotnet-artisan | **5** | Built-in OpenAPI only; no 3rd-party packages. |
| dotnet-skills | **1** | Swashbuckle + deprecated FluentValidation.AspNetCore; maximum 3rd-party dependency. |
| dotnet-webapi | **5** | Built-in OpenAPI only. |
| managedcode | **5** | Built-in OpenAPI only. |

**Verdict**: Three skill-based configs correctly use built-in OpenAPI. dotnet-skills is worst with deprecated 3rd-party packages.

---

## 9. Modern C# Adoption [HIGH]

### What each configuration does

**dotnet-artisan, dotnet-webapi, managedcode** use modern C# throughout — primary constructors, collection expressions, `required` modifier:

```csharp
// dotnet-artisan — OwnerService.cs (primary constructor)
public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService

// dotnet-artisan — Owner.cs (collection expression, required modifier)
public sealed class Owner
{
    public required string FirstName { get; set; }
    public ICollection<Pet> Pets { get; set; } = [];  // collection expression
}
```

**no-skills** uses traditional patterns throughout:

```csharp
// no-skills — Owner.cs (no sealed, no required, old collection init)
public class Owner
{
    public string FirstName { get; set; } = string.Empty;
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}

// no-skills — OwnersController.cs (traditional constructor)
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _ownerService;
    public OwnersController(IOwnerService ownerService) => _ownerService = ownerService;
}
```

**dotnet-skills** is mixed — records for DTOs but traditional constructors and `new List<T>()`:

```csharp
// dotnet-skills — Owner.cs (no sealed, no required, old collection init)
public class Owner
{
    public string FirstName { get; set; } = string.Empty;
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
```

| Feature | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Primary constructors | 0 | 17 | 1 | 19 | 19 |
| Sealed types | 0 | 38 | 8 | 40 | 47 |
| Collection expressions `[]` | 0 | 6 | 0 | 6 | 6 |
| `required` modifier | 0 | ✅ | 0 | ✅ | ✅ |

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **1** | No modern C# features; traditional patterns throughout. |
| dotnet-artisan | **5** | Full adoption of primary constructors, sealed, collection expressions, required. |
| dotnet-skills | 2 | Minimal modern adoption (1 primary ctor, 8 sealed); mostly traditional. |
| dotnet-webapi | **5** | Same full modern adoption as dotnet-artisan. |
| managedcode | **5** | Highest sealed count (47); full modern adoption. |

**Verdict**: dotnet-artisan, dotnet-webapi, and managedcode all demonstrate exemplary modern C# adoption. no-skills shows none.

---

## 10. Error Handling & Middleware [HIGH]

### What each configuration does

**dotnet-artisan, dotnet-webapi, managedcode** use the modern `IExceptionHandler` interface (introduced in .NET 8):

```csharp
// dotnet-artisan — ApiExceptionHandler.cs
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (0, (string?)null)
        };
        // ProblemDetails response...
    }
}

// Program.cs
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
```

**no-skills** and **dotnet-skills** use convention-based middleware with `RequestDelegate`:

```csharp
// dotnet-skills — GlobalExceptionHandlerMiddleware.cs
public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<...> logger)
    {
        _next = next; _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (InvalidOperationException ex) { await WriteProblemDetailsAsync(...); }
        catch (KeyNotFoundException ex) { await WriteProblemDetailsAsync(...); }
    }
}
// Program.cs
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

**managedcode**'s handler additionally catches unhandled exceptions as 500:

```csharp
// managedcode — handles ALL exceptions including unhandled
if (statusCode == 0)
{
    logger.LogError(exception, "Unhandled exception occurred");
    statusCode = StatusCodes.Status500InternalServerError;
    title = "Internal Server Error";
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Custom middleware works but is the old pattern; ProblemDetails returned. |
| dotnet-artisan | **5** | Modern IExceptionHandler; DI-aware; pattern matching; ProblemDetails. |
| dotnet-skills | 3 | Same old middleware pattern as no-skills. |
| dotnet-webapi | **5** | Same modern IExceptionHandler pattern. |
| managedcode | **5** | IExceptionHandler with comprehensive 500 handling. |

**Verdict**: IExceptionHandler (dotnet-artisan, dotnet-webapi, managedcode) is composable, DI-aware, and the modern .NET standard.

---

## 11. Async Patterns & Cancellation [HIGH]

### What each configuration does

**dotnet-artisan, dotnet-webapi, managedcode** propagate `CancellationToken` through every layer — endpoints, services, and EF Core queries:

```csharp
// dotnet-artisan — OwnerEndpoints.cs (CancellationToken in endpoint)
group.MapGet("/", async Task<Ok<PaginatedResponse<OwnerResponse>>> (
    string? search, int page = 1, int pageSize = 20,
    IOwnerService service = default!, CancellationToken ct = default) =>
{
    return TypedResults.Ok(await service.GetAllAsync(search, page, pageSize, ct));
})

// dotnet-artisan — OwnerService.cs (CancellationToken forwarded to EF Core)
public async Task<PaginatedResponse<OwnerResponse>> GetAllAsync(
    string? search, int page, int pageSize, CancellationToken ct = default)
{
    var totalCount = await query.CountAsync(ct);
    var items = await query.Skip(...).Take(...).ToListAsync(ct);
}
```

**no-skills** and **dotnet-skills** have **zero** meaningful CancellationToken usage:

```csharp
// no-skills — OwnersController.cs (no CancellationToken)
public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, ...)
    => Ok(await _ownerService.GetAllAsync(search, page, pageSize));
// CancellationToken not accepted, not forwarded
```

| Config | CancellationToken references |
|---|---|
| no-skills | 0 |
| dotnet-artisan | 110 |
| dotnet-skills | 2 |
| dotnet-webapi | 110 |
| managedcode | 110 |

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **2** | Async/await correct but zero CancellationToken propagation. |
| dotnet-artisan | **5** | Full cancellation propagation through all layers. |
| dotnet-skills | **2** | Only 2 CancellationToken references; effectively none in practice. |
| dotnet-webapi | **5** | Full cancellation propagation. |
| managedcode | **5** | Full cancellation propagation. |

**Verdict**: Massive gap — skill-based Minimal API configs propagate CancellationToken everywhere, while no-skills and dotnet-skills ignore it entirely.

---

## 12. EF Core Best Practices [HIGH]

### What each configuration does

**managedcode** uses `IEntityTypeConfiguration<T>` for clean separation of entity configuration:

```csharp
// managedcode — Data/Configurations/EntityConfigurations.cs
public sealed class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.FirstName).IsRequired().HasMaxLength(100);
        builder.HasIndex(o => o.Email).IsUnique();
        builder.HasMany(o => o.Pets).WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Restrict);
    }
}
// VetClinicDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(VetClinicDbContext).Assembly);
}
```

**dotnet-artisan** and **dotnet-webapi** configure entities inline in `OnModelCreating` with explicit `OnDelete` behavior:

```csharp
// dotnet-artisan — VetClinicDbContext.cs
modelBuilder.Entity<Owner>(entity =>
{
    entity.HasIndex(o => o.Email).IsUnique();
    entity.HasMany(o => o.Pets).WithOne(p => p.Owner)
        .HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Restrict);
});
```

**no-skills** lacks `AsNoTracking()` entirely — every read query uses change tracking:

| Config | AsNoTracking calls | OnDelete configured | DbContext style |
|---|---|---|---|
| no-skills | 0 | Partial | Traditional constructor |
| dotnet-artisan | 20 | All Restrict | Primary constructor |
| dotnet-skills | 28 | Partial | Traditional constructor |
| dotnet-webapi | 24 | All Restrict | Primary constructor |
| managedcode | 21 | All Restrict | Primary constructor + IEntityTypeConfiguration |

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **2** | Zero AsNoTracking; partial OnDelete; traditional DbContext. |
| dotnet-artisan | **5** | Consistent AsNoTracking; explicit OnDelete; primary ctor DbContext; SaveChanges timestamp override. |
| dotnet-skills | 4 | Good AsNoTracking usage; partial OnDelete; traditional DbContext. |
| dotnet-webapi | **5** | Consistent AsNoTracking; explicit OnDelete; primary ctor DbContext. |
| managedcode | **5** | IEntityTypeConfiguration for clean separation; AsNoTracking; SaveChanges timestamps; ApplyConfigurationsFromAssembly. |

**Verdict**: managedcode slightly edges ahead with `IEntityTypeConfiguration` + `ApplyConfigurationsFromAssembly`. no-skills is weakest with zero AsNoTracking.

---

## 13. Service Abstraction & DI [HIGH]

### What each configuration does

All five configurations follow the same interface-based service pattern with `AddScoped<IService, Service>()`:

```csharp
// All configs — Program.cs
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IVeterinarianService, VeterinarianService>();
// ... 7 services total
```

All have one service per domain entity with proper single responsibility. Interface definitions match implementation across all configs.

### Scores

| Config | Score | Justification |
|---|---|---|
| All configs | 4 | Consistent interface-based DI; one service per domain area; proper scoped lifetime. |

**Verdict**: All configurations follow identical DI patterns. This is a baseline that Copilot gets right regardless of skills.

---

## 14. Security Configuration [HIGH]

### What each configuration does

**dotnet-skills** is the only configuration with any HTTPS configuration:

```csharp
// dotnet-skills — Program.cs
app.UseHttpsRedirection();
```

All other configurations have **no** HSTS or HTTPS redirection. No configuration includes conditional HSTS for non-development environments.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | No HSTS, no HTTPS redirection. |
| dotnet-artisan | 1 | No HSTS, no HTTPS redirection. |
| dotnet-skills | **2** | Has UseHttpsRedirection but no conditional HSTS. |
| dotnet-webapi | 1 | No HSTS, no HTTPS redirection. |
| managedcode | 1 | No HSTS, no HTTPS redirection. |

**Verdict**: Uniformly weak. Only dotnet-skills includes basic HTTPS redirection. None implement the recommended `if (!app.Environment.IsDevelopment()) { app.UseHsts(); }` pattern.

---

## 15. DTO Design [MEDIUM]

### What each configuration does

**dotnet-artisan, dotnet-webapi, managedcode** use sealed records with `init` properties, `required` modifier, and *Request/*Response naming:

```csharp
// dotnet-artisan — OwnerDtos.cs
public sealed record CreateOwnerRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
}

public sealed record OwnerResponse(
    int Id, string FirstName, string LastName, string Email, string Phone,
    string? Address, string? City, string? State, string? ZipCode,
    DateTime CreatedAt, DateTime UpdatedAt,
    IReadOnlyList<OwnerPetResponse>? Pets = null);
```

**no-skills** uses mutable classes with `*Dto` naming:

```csharp
// no-skills — OwnerDtos.cs
public class CreateOwnerDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
}

public class OwnerDto
{
    public int Id { get; set; }
    public List<PetSummaryDto> Pets { get; set; } = new();
}
```

**dotnet-skills** uses records (not sealed) with `*Dto` naming:

```csharp
// dotnet-skills — OwnerDtos.cs
public record CreateOwnerDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **2** | Mutable classes; `string.Empty` defaults; *Dto naming; `List<T>` in responses. |
| dotnet-artisan | **5** | Sealed records; init+required; *Request/*Response; positional records for responses; `IReadOnlyList<T>`. |
| dotnet-skills | 3 | Records with init; but not sealed; *Dto naming; missing required modifier. |
| dotnet-webapi | **5** | Same quality as dotnet-artisan. |
| managedcode | **5** | Same quality as dotnet-artisan. |

**Verdict**: Sealed records with `required` + `init` and *Request/*Response naming (dotnet-artisan, dotnet-webapi, managedcode) produce the safest, most expressive API contracts.

---

## 16. Sealed Types [MEDIUM]

### What each configuration does

| Config | Sealed class/record count | Coverage |
|---|---|---|
| no-skills | 0 | No sealed types anywhere |
| dotnet-artisan | 38 | Models, DTOs, Services, Middleware, DbContext all sealed |
| dotnet-skills | 8 | Middleware class sealed; a few services sealed |
| dotnet-webapi | 40 | Comprehensive sealing |
| managedcode | **47** | Most comprehensive — all types sealed |

```csharp
// managedcode — all models sealed
public sealed class Owner { ... }
public sealed class Pet { ... }

// managedcode — all services sealed with primary constructors
public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService

// no-skills — nothing sealed
public class Owner { ... }
public class OwnerService { ... }
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **1** | Zero sealed types. |
| dotnet-artisan | **5** | 38 sealed types across all layers. |
| dotnet-skills | 2 | Only 8 sealed types; inconsistent application. |
| dotnet-webapi | **5** | 40 sealed types. |
| managedcode | **5** | 47 sealed types — most comprehensive. |

**Verdict**: managedcode leads with 47 sealed types. dotnet-artisan and dotnet-webapi are close behind. no-skills seals nothing.

---

## 17. Data Seeder Design [MEDIUM]

### What each configuration does

**no-skills, dotnet-artisan, dotnet-skills, managedcode** all use a static `DataSeeder.SeedAsync()` method called from `Program.cs` with idempotency checks:

```csharp
// dotnet-artisan — DataSeeder.cs (pattern shared by 4 configs)
public static class DataSeeder
{
    public static async Task SeedAsync(VetClinicDbContext db)
    {
        if (await db.Owners.AnyAsync()) return;  // idempotency guard
        // 5 owners, 8 pets, 3 vets, 10 appointments, 4 medical records, 5 prescriptions, 6 vaccinations
    }
}
```

**dotnet-webapi** has **no data seeder** at all — no `DataSeeder` class exists in the project.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Runtime seeder with idempotency; realistic variety. |
| dotnet-artisan | 3 | Same pattern with comprehensive seed data. |
| dotnet-skills | 3 | Same pattern. |
| dotnet-webapi | **1** | No seeder exists; database starts completely empty. |
| managedcode | 3 | Same seeder pattern. |

**Verdict**: Four configs share the same adequate seeder pattern. dotnet-webapi's missing seeder is a significant gap for developer experience.

---

## 18. Structured Logging [MEDIUM]

### What each configuration does

All configurations inject `ILogger<T>` and use structured message templates:

```csharp
// dotnet-artisan — OwnerService.cs (structured logging with named placeholders)
logger.LogInformation("Created owner {OwnerId}: {FirstName} {LastName}",
    owner.Id, owner.FirstName, owner.LastName);
logger.LogInformation("Deleted owner {OwnerId}", id);
```

| Config | Structured log calls |
|---|---|
| no-skills | 17 |
| dotnet-artisan | 17 |
| dotnet-skills | 18 |
| dotnet-webapi | 17 |
| managedcode | 12 |

### Scores

All configs score **4** — proper ILogger<T> with structured templates. None use `[LoggerMessage]` source generators (which would earn a 5).

**Verdict**: Uniform quality. All use structured logging correctly but none adopt high-performance source generators.

---

## 19. Nullable Reference Types [MEDIUM]

All configurations enable NRTs in their `.csproj`:

```xml
<Nullable>enable</Nullable>
```

All use proper `?` annotations on optional properties and `null!` for required navigation properties. All score **4**.

---

## 20. API Documentation [MEDIUM]

### What each configuration does

**dotnet-artisan, dotnet-webapi, managedcode** use rich endpoint metadata:

```csharp
// dotnet-webapi — OwnerEndpoints.cs
group.MapGet("/{id:int}", async Task<Results<Ok<OwnerDetailResponse>, NotFound>> (...) => { ... })
    .WithName("GetOwnerById")
    .WithSummary("Get owner by ID")
    .WithDescription("Returns the owner details including their pets.")
    .Produces<OwnerDetailResponse>()
    .Produces(StatusCodes.Status404NotFound);
```

**no-skills** and **dotnet-skills** rely on XML doc comments and `[ProducesResponseType]`:

```csharp
// no-skills — OwnersController.cs
/// <summary>Get owner by ID including their pets</summary>
[HttpGet("{id}")]
[ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetById(int id) => ...
```

| Config | WithName/WithSummary/WithDescription references |
|---|---|
| no-skills | 0 |
| dotnet-artisan | 86 |
| dotnet-skills | 0 |
| dotnet-webapi | **105** |
| managedcode | 85 |

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | XML doc comments + ProducesResponseType provide basic documentation. |
| dotnet-artisan | **5** | Rich WithName/WithSummary/WithDescription on every endpoint. |
| dotnet-skills | 3 | Same controller-based documentation as no-skills. |
| dotnet-webapi | **5** | Most metadata (105 refs); includes .Produces<T>() alongside TypedResults. |
| managedcode | **5** | Rich metadata on all endpoints. |

**Verdict**: dotnet-webapi leads quantitatively with 105 metadata references. All three Minimal API configs far exceed the controller-based ones.

---

## 21. File Organization [MEDIUM]

### What each configuration does

| Config | Structure | Notable |
|---|---|---|
| no-skills | `Controllers/ Data/ DTOs/ Middleware/ Models/ Services/Interfaces/` | Interfaces in sub-folder |
| dotnet-artisan | `Data/ DTOs/ Endpoints/ Middleware/ Models/ Properties/ Services/` | `Directory.Build.props` for analyzers |
| dotnet-skills | `Controllers/ Data/ DTOs/ Middleware/ Models/ Services/ Validators/` | Dedicated Validators folder |
| dotnet-webapi | `Data/ DTOs/ Endpoints/ Middleware/ Models/ Services/` | Clean Minimal API layout |
| managedcode | `Data/Configurations/ DTOs/ Endpoints/ Middleware/ Models/ Services/` | Entity configs in sub-folder |

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Standard controller layout; nested Interfaces folder adds depth. |
| dotnet-artisan | **5** | Clean Endpoints-based layout; Directory.Build.props; Properties folder. |
| dotnet-skills | 3 | Standard layout; Validators add value but Controllers are legacy. |
| dotnet-webapi | 4 | Clean layout but missing .http file. |
| managedcode | **5** | Best separation with Data/Configurations sub-folder for entity configs. |

**Verdict**: managedcode's `Data/Configurations/` separation is cleanest. dotnet-artisan's `Directory.Build.props` shows production awareness.

---

## 22. HTTP Test File Quality [MEDIUM]

### What each configuration does

Four configs include `.http` files with **~300 lines** covering all endpoints:

```http
### Owners
@baseUrl = http://localhost:5055

### List all owners
GET {{baseUrl}}/api/owners?page=1&pageSize=10

### Create a new owner
POST {{baseUrl}}/api/owners
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  ...
}
```

**dotnet-webapi** has **no .http file** at all.

| Config | .http file | Lines |
|---|---|---|
| no-skills | ✅ | 310 |
| dotnet-artisan | ✅ | 295 |
| dotnet-skills | ✅ | 311 |
| dotnet-webapi | ❌ | — |
| managedcode | ✅ | 294 |

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | Comprehensive .http file with all endpoints. |
| dotnet-artisan | 4 | Same quality. |
| dotnet-skills | 4 | Same quality. |
| dotnet-webapi | **1** | No .http file generated. |
| managedcode | 4 | Same quality. |

**Verdict**: dotnet-webapi's missing .http file is a notable gap. All others provide equivalent quality.

---

## 23. Type Design & Resource Management [MEDIUM]

### What each configuration does

**dotnet-artisan, dotnet-webapi, managedcode** use `IReadOnlyList<T>` return types in service interfaces:

```csharp
// dotnet-artisan — IPetService.cs
Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct);
Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct);
```

**no-skills** and **dotnet-skills** use mutable `List<T>`:

```csharp
// no-skills — IPetService.cs
Task<List<MedicalRecordDto>> GetMedicalRecordsAsync(int petId);
Task<List<VaccinationDto>> GetVaccinationsAsync(int petId);
```

All configs use `AppointmentStatus` enum with `HasConversion<string>()` in EF Core configuration.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Enums correct; mutable List<T> return types. |
| dotnet-artisan | **5** | IReadOnlyList<T>; proper enum design; precise return types. |
| dotnet-skills | 3 | Same as no-skills. |
| dotnet-webapi | **5** | Same quality as dotnet-artisan. |
| managedcode | **5** | Same quality as dotnet-artisan. |

**Verdict**: `IReadOnlyList<T>` in skill-based Minimal API configs prevents accidental mutation of returned collections.

---

## 24. Code Standards Compliance [LOW]

### What each configuration does

All follow .NET naming conventions (PascalCase types/members, camelCase parameters, `I` prefix for interfaces, file-scoped namespaces). Key differences in access modifier explicitness:

```csharp
// dotnet-artisan — explicit internal sealed
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler

// no-skills — relies on defaults
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Follows conventions but relies on default access modifiers. |
| dotnet-artisan | **5** | Explicit modifiers everywhere; internal where appropriate; file-scoped namespaces. |
| dotnet-skills | 3 | Same as no-skills. |
| dotnet-webapi | **5** | Same quality as dotnet-artisan. |
| managedcode | **5** | Same quality as dotnet-artisan. |

---

## Weighted Summary

Weighted scoring: **Critical × 3, High × 2, Medium × 1, Low × 0.5**

### Critical Dimensions (×3)

| Dimension | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Build & Run Success | 4 | 4 | 4 | 2 | 4 |
| Security Vulnerability Scan | 3 | 3 | 2 | 4 | 4 |
| Minimal API Architecture | 1 | 5 | 1 | 5 | 5 |
| Input Validation | 3 | 3 | 4 | 3 | 3 |
| NuGet & Package Discipline | 2 | 2 | 1 | 5 | 5 |
| EF Migration Usage | 2 | 2 | 2 | 3 | 2 |
| **Subtotal (×3)** | **45** | **57** | **42** | **66** | **69** |

### High Dimensions (×2)

| Dimension | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Business Logic Correctness | 4 | 5 | 4 | 3 | 4 |
| Prefer Built-in over 3rd Party | 2 | 5 | 1 | 5 | 5 |
| Modern C# Adoption | 1 | 5 | 2 | 5 | 5 |
| Error Handling & Middleware | 3 | 5 | 3 | 5 | 5 |
| Async Patterns & Cancellation | 2 | 5 | 2 | 5 | 5 |
| EF Core Best Practices | 2 | 5 | 4 | 5 | 5 |
| Service Abstraction & DI | 4 | 4 | 4 | 4 | 4 |
| Security Configuration | 1 | 1 | 2 | 1 | 1 |
| **Subtotal (×2)** | **38** | **70** | **44** | **66** | **68** |

### Medium Dimensions (×1)

| Dimension | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| DTO Design | 2 | 5 | 3 | 5 | 5 |
| Sealed Types | 1 | 5 | 2 | 5 | 5 |
| Data Seeder Design | 3 | 3 | 3 | 1 | 3 |
| Structured Logging | 4 | 4 | 4 | 4 | 4 |
| Nullable Reference Types | 4 | 4 | 4 | 4 | 4 |
| API Documentation | 3 | 5 | 3 | 5 | 5 |
| File Organization | 3 | 5 | 3 | 4 | 5 |
| HTTP Test File Quality | 4 | 4 | 4 | 1 | 4 |
| Type Design & Resource Mgmt | 3 | 5 | 3 | 5 | 5 |
| **Subtotal (×1)** | **27** | **40** | **29** | **34** | **40** |

### Low Dimensions (×0.5)

| Dimension | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Code Standards Compliance | 3 | 5 | 3 | 5 | 5 |
| **Subtotal (×0.5)** | **1.5** | **2.5** | **1.5** | **2.5** | **2.5** |

### Total Weighted Scores

| Configuration | Critical (×3) | High (×2) | Medium (×1) | Low (×0.5) | **Total** |
|---|---|---|---|---|---|
| **managedcode-dotnet-skills** | 69 | 68 | 40 | 2.5 | **179.5** |
| **dotnet-artisan** | 57 | 70 | 40 | 2.5 | **169.5** |
| **dotnet-webapi** | 66 | 66 | 34 | 2.5 | **168.5** |
| **dotnet-skills** | 42 | 44 | 29 | 1.5 | **116.5** |
| **no-skills** | 45 | 38 | 27 | 1.5 | **111.5** |

---

## What All Versions Get Right

- **Service abstraction**: All use interface-based services with `AddScoped<IService, Service>()` — proper dependency inversion
- **Structured logging**: All inject `ILogger<T>` and use structured message templates with named placeholders
- **Nullable reference types**: All enable `<Nullable>enable</Nullable>` with proper `?` annotations
- **Enum design**: All use `AppointmentStatus` enum with `HasConversion<string>()` for readable database storage
- **Business rule enforcement**: Appointment conflict detection, status workflow transitions, medical record creation constraints, and soft-delete patterns are implemented across all configurations
- **Pagination**: All implement consistent pagination with metadata (total count, total pages)
- **File-scoped namespaces**: All use `namespace X;` syntax
- **Implicit usings**: All enable `<ImplicitUsings>enable</ImplicitUsings>`

---

## Summary: Impact of Skills

### Rankings

1. **🥇 managedcode-dotnet-skills (179.5)** — The highest overall score. Combines all modern patterns (Minimal APIs, TypedResults, sealed types, primary constructors, collection expressions) with the best NuGet discipline (pinned versions, no unnecessary packages) and the only configuration using `IEntityTypeConfiguration<T>` for clean EF Core configuration. Its only weakness is the `EnsureCreated` anti-pattern shared by most configs.

2. **🥈 dotnet-artisan (169.5)** — Very close to managedcode on code quality dimensions, scoring identically on modern C#, error handling, and async patterns. Loses ground on NuGet discipline due to wildcard package versions (`10.*-*` and `Version="*"` for Meziantou). Uniquely includes `Directory.Build.props` with code analysis settings showing production awareness.

3. **🥉 dotnet-webapi (168.5)** — Scores highest on critical dimensions (66/69) thanks to pinned packages and Minimal APIs, but is severely penalized by a broken database initialization (`Migrate()` with no migrations), missing data seeder, and missing `.http` file. The code quality is excellent — it's the *completeness* that holds it back.

4. **4th: dotnet-skills (116.5)** — Despite adding FluentValidation for richer validation, it makes several poor choices: controllers instead of Minimal APIs, Swashbuckle instead of built-in OpenAPI, the deprecated `FluentValidation.AspNetCore` package, wildcard versions, and minimal modern C# adoption. The official .NET Skills surprisingly produce one of the weakest outputs.

5. **5th: no-skills (111.5)** — The baseline produces functional but dated code. Zero CancellationToken usage, zero sealed types, zero AsNoTracking calls, controllers instead of Minimal APIs, Swashbuckle instead of built-in OpenAPI, and mutable class DTOs. This establishes the floor that skills can improve upon.

### Most Impactful Differences

1. **Minimal APIs vs Controllers** (+12 weighted points) — The single largest differentiator, worth 12 points in critical weighting. Skills that enforce Minimal APIs (dotnet-artisan, dotnet-webapi, managedcode) produce fundamentally more modern code.

2. **CancellationToken propagation** (+6 weighted points) — Complete presence (110 refs) vs. complete absence (0 refs) across the entire codebase. This directly impacts server resource utilization under load.

3. **NuGet discipline** (+9 weighted points) — Pinned versions vs. wildcards is a critical security and reproducibility concern. managedcode and dotnet-webapi get this right; others don't.

4. **Modern C# features** (+8 weighted points) — Primary constructors, sealed types, collection expressions, and required modifiers are either fully adopted or completely absent. No configuration shows partial adoption — it's all or nothing.

5. **IExceptionHandler vs middleware** (+4 weighted points) — The modern .NET 8+ pattern is consistently produced by the same three configs (dotnet-artisan, dotnet-webapi, managedcode).

### Overall Assessment

Skills make a **dramatic difference** — the gap between the best (179.5) and worst (111.5) is 68 points (61% improvement). The three Minimal API-producing skills (managedcode, dotnet-artisan, dotnet-webapi) cluster tightly together at the top, while the two controller-producing approaches (no-skills, dotnet-skills) cluster at the bottom. The dotnet-webapi skill produces the highest-quality *code* but the least complete *project*, demonstrating that code quality and completeness are independent dimensions that skills must address holistically.
