# Comparative Analysis: no-skills, dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, dotnet-webapi

## Introduction

This report compares five Copilot skill configurations used to generate a **Veterinary Clinic Management API** (VetClinicApi) — a pet healthcare system with appointments, vaccinations, medical records, and prescription tracking. Each configuration was applied to the same scenario prompt targeting **.NET 10** with **EF Core + SQLite**.

| Configuration | Description | Project Generated? |
|---|---|---|
| **no-skills** | Baseline Copilot (no custom skills) | ✅ Yes |
| **dotnet-artisan** | dotnet-artisan plugin chain | ✅ Yes |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | ✅ Yes |
| **dotnet-webapi** | dotnet-webapi skill | ✅ Yes |
| **managedcode-dotnet-skills** | Community managed-code skills | ❌ **No** (only `Directory.Build.props` generated) |

> **Note:** The `managedcode-dotnet-skills` configuration failed to produce any project code. Only a `Directory.Build.props` file was generated. This configuration receives a score of **1** (Missing) across all dimensions.

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Minimal API Architecture [CRITICAL] | 1 | 5 | 1 | 5 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 4 | 4 | 4 | 1 |
| NuGet & Package Discipline [CRITICAL] | 3 | 4 | 1 | 5 | 1 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 5 | 1 |
| Prefer Built-in over 3rd Party [HIGH] | 2 | 3 | 1 | 5 | 1 |
| Business Logic Correctness [HIGH] | 4 | 4 | 3 | 4 | 1 |
| Modern C# Adoption [HIGH] | 2 | 5 | 2 | 4 | 1 |
| Error Handling & Middleware [HIGH] | 3 | 5 | 3 | 5 | 1 |
| Async Patterns & Cancellation [HIGH] | 2 | 5 | 2 | 5 | 1 |
| EF Core Best Practices [HIGH] | 2 | 4 | 3 | 5 | 1 |
| Service Abstraction & DI [HIGH] | 4 | 4 | 3 | 5 | 1 |
| Security Configuration [HIGH] | 2 | 2 | 2 | 2 | 1 |
| DTO Design [MEDIUM] | 2 | 5 | 2 | 5 | 1 |
| Sealed Types [MEDIUM] | 1 | 5 | 3 | 5 | 1 |
| Data Seeder Design [MEDIUM] | 3 | 3 | 3 | 5 | 1 |
| Structured Logging [MEDIUM] | 3 | 4 | 3 | 4 | 1 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 1 |
| API Documentation [MEDIUM] | 3 | 4 | 3 | 5 | 1 |
| File Organization [MEDIUM] | 3 | 4 | 2 | 5 | 1 |
| HTTP Test File Quality [MEDIUM] | 3 | 1 | 3 | 4 | 1 |
| Type Design & Resource Mgmt [MEDIUM] | 3 | 4 | 3 | 4 | 1 |
| Code Standards Compliance [LOW] | 3 | 4 | 3 | 4 | 1 |

---

## 1. Minimal API Architecture [CRITICAL]

### What each configuration does

**no-skills** and **dotnet-skills** both use traditional **Controllers with `[ApiController]`**:

```csharp
// no-skills: Controllers/OwnersController.cs
[ApiController]
[Route("api/owners")]
[Produces("application/json")]
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _ownerService;
    public OwnersController(IOwnerService ownerService) => _ownerService = ownerService;

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<OwnerSummaryDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _ownerService.GetAllAsync(search, page, pageSize);
        return Ok(result);
    }
}
```

**dotnet-artisan** and **dotnet-webapi** both use **Minimal APIs** with `MapGroup()`, `TypedResults`, and endpoint extension methods:

```csharp
// dotnet-webapi: Endpoints/OwnerEndpoints.cs
public static class OwnerEndpoints
{
    public static void MapOwnerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/owners").WithTags("Owners");

        group.MapGet("/", async Task<Ok<PaginatedResponse<OwnerResponse>>> (
            string? search, int? page, int? pageSize,
            IOwnerService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, page ?? 1, Math.Clamp(pageSize ?? 20, 1, 100), ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetOwners")
        .WithSummary("List all owners")
        .Produces<PaginatedResponse<OwnerResponse>>();
    }
}
```

```csharp
// dotnet-artisan: Endpoints/OwnerEndpoints.cs
public static RouteGroupBuilder MapOwnerEndpoints(this IEndpointRouteBuilder routes)
{
    var group = routes.MapGroup("/api/owners").WithTags("Owners");

    group.MapGet("/", async (string? search, int page, int pageSize, IOwnerService service, CancellationToken ct) =>
        TypedResults.Ok(await service.GetAllAsync(search, page, pageSize, ct)))
        .WithName("GetOwners")
        .WithSummary("List all owners with optional search and pagination");

    return group;
}
```

**dotnet-webapi** goes further with explicit return type annotations on endpoint lambdas:

```csharp
// dotnet-webapi: explicit return type on lambda
group.MapGet("/{id:int}", async Task<Results<Ok<OwnerResponse>, NotFound>> (
    int id, IOwnerService service, CancellationToken ct) =>
{
    var owner = await service.GetByIdAsync(id, ct);
    return owner is null ? TypedResults.NotFound() : TypedResults.Ok(owner);
});
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | Traditional controllers — no MapGroup, no TypedResults, no endpoint extension methods |
| dotnet-artisan | 5 | Full Minimal API stack: MapGroup, TypedResults, endpoint extension methods, WithTags/WithSummary |
| dotnet-skills | 1 | Traditional controllers identical to no-skills pattern |
| dotnet-webapi | 5 | Full Minimal API stack plus explicit `Results<T1, T2>` return types for automatic OpenAPI schema |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** and **dotnet-artisan** tie for the lead. Both implement the modern Minimal API pattern correctly. dotnet-webapi edges ahead slightly with explicit union return types (`Results<Ok<T>, NotFound>`) that enable automatic OpenAPI schema generation without separate `.Produces<T>()` calls.

---

## 2. Input Validation & Guard Clauses [CRITICAL]

### What each configuration does

All four generated projects validate DTOs with data annotations (`[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`).

**dotnet-skills** additionally uses **FluentValidation** (a third-party library):

```csharp
// dotnet-skills: Validators/Validators.cs
public class OwnerCreateValidator : AbstractValidator<OwnerCreateDto>
{
    public OwnerCreateValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.State).MaximumLength(2);
    }
}
```

**dotnet-artisan** and **dotnet-webapi** use validation attributes on sealed record DTOs:

```csharp
// dotnet-webapi: DTOs/OwnerDtos.cs
public sealed record CreateOwnerRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
    [Required, EmailAddress]
    public required string Email { get; init; }
}
```

None of the configurations use explicit guard clauses (`ArgumentNullException.ThrowIfNull()`, `ArgumentException.ThrowIfNullOrEmpty()`) in service constructors or methods.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Data annotations on DTOs, validation in controllers via [ApiController], no guard clauses |
| dotnet-artisan | 4 | Sealed record DTOs with validation attrs + service-level business validation via exceptions |
| dotnet-skills | 4 | FluentValidation provides comprehensive validation, but adds a third-party dependency |
| dotnet-webapi | 4 | Sealed record DTOs with validation attrs + service-level exceptions for business rules |
| managedcode | 1 | No project generated |

**Verdict:** All three skill-based configs provide good validation. **dotnet-artisan** and **dotnet-webapi** are preferred because they achieve strong validation without third-party libraries. None achieve a 5 because explicit guard clauses on constructor parameters are missing across the board.

---

## 3. NuGet & Package Discipline [CRITICAL]

### What each configuration does

```xml
<!-- no-skills: 4 packages, all versions pinned -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />

<!-- dotnet-artisan: 4 packages, all versions pinned, SwaggerUI only -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="10.1.7" />

<!-- dotnet-skills: 5 packages, WILDCARD VERSIONS on EF Core! -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />

<!-- dotnet-webapi: 3 packages, all versions pinned, NO Swashbuckle -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | All pinned but includes unnecessary Swashbuckle (4 packages) |
| dotnet-artisan | 4 | All pinned, only SwaggerUI not full Swashbuckle (4 packages) |
| dotnet-skills | 1 | **Wildcard `10.0.0-*`** on EF Core packages — non-reproducible builds; 5 packages including unnecessary FluentValidation and Swashbuckle |
| dotnet-webapi | 5 | Only 3 packages, all pinned, zero unnecessary dependencies |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** is the clear winner with the minimal possible package set and exact version pinning. **dotnet-skills** is critically flawed with wildcard versions (`10.0.0-*`) that could pull in breaking changes or vulnerable prerelease packages.

---

## 4. EF Migration Usage [CRITICAL]

### What each configuration does

**no-skills**, **dotnet-artisan**, and **dotnet-skills** all use the `EnsureCreated()` anti-pattern:

```csharp
// no-skills: Program.cs
context.Database.EnsureCreated();

// dotnet-artisan: Program.cs
await db.Database.EnsureCreatedAsync();

// dotnet-skills: Program.cs
db.Database.EnsureCreated();
```

**dotnet-webapi** is the only configuration that uses proper **EF Core migrations**:

```csharp
// dotnet-webapi: Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    db.Database.Migrate();
}
```

It includes a complete `Migrations/` folder with `InitialCreate` migration files, and seeds data through `HasData()` in the DbContext (which gets captured in the migration).

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | `EnsureCreated()` bypasses migrations entirely |
| dotnet-artisan | 1 | `EnsureCreatedAsync()` — same anti-pattern, just async |
| dotnet-skills | 1 | `EnsureCreated()` with sync call — worst variant |
| dotnet-webapi | 5 | `Database.Migrate()` with actual migration files + `HasData()` seed data |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** is the only configuration that follows the production-safe migration approach. All others use `EnsureCreated()`, which makes schema evolution impossible and risks data loss.

---

## 5. Prefer Built-in over 3rd Party [HIGH]

### What each configuration does

| Concern | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi |
|---|---|---|---|---|
| OpenAPI | Swashbuckle ❌ | Built-in + SwaggerUI pkg | Both Swashbuckle + built-in ❌ | Built-in only ✅ |
| Validation | Data Annotations ✅ | Data Annotations ✅ | FluentValidation ❌ | Data Annotations ✅ |
| JSON | System.Text.Json ✅ | System.Text.Json ✅ | System.Text.Json ✅ | System.Text.Json ✅ |
| DI | Built-in ✅ | Built-in ✅ | Built-in ✅ | Built-in ✅ |

```csharp
// no-skills: uses Swashbuckle
builder.Services.AddSwaggerGen(c => { ... });
app.UseSwagger();
app.UseSwaggerUI(c => ...);

// dotnet-webapi: uses built-in OpenAPI only
builder.Services.AddOpenApi();
app.MapOpenApi();
// No Swashbuckle at all

// dotnet-skills: redundantly uses BOTH
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c => { ... });
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c => ...);
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Uses Swashbuckle instead of built-in OpenAPI |
| dotnet-artisan | 3 | Uses built-in AddOpenApi/MapOpenApi but still references SwaggerUI package |
| dotnet-skills | 1 | Uses BOTH Swashbuckle AND built-in OpenAPI (redundant), plus third-party FluentValidation |
| dotnet-webapi | 5 | Zero third-party libraries beyond EF Core; fully built-in OpenAPI |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** exemplifies the built-in-first philosophy. **dotnet-skills** is the worst offender, using redundant OpenAPI configurations and a third-party validation library.

---

## 6. Business Logic Correctness [HIGH]

### What each configuration does

The scenario specifies ~30 endpoints across 7 resources with complex business rules (appointment conflict detection, status workflow, cancellation rules, medical record constraints, vaccination tracking, soft delete).

All four generated projects implement the core business rules:
- Appointment scheduling conflict detection
- Status workflow enforcement (Scheduled → CheckedIn → InProgress → Completed)
- Cancellation reason requirement
- Medical record creation constraints (only for Completed/InProgress appointments)
- Vaccination expiry tracking (IsExpired, IsDueSoon computed properties)
- Pet soft-delete with IsActive flag

**dotnet-skills** groups Prescriptions and Vaccinations controllers into `MedicalRecordsController.cs`, reducing clarity but not missing endpoints.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | All endpoints present, business rules correctly implemented |
| dotnet-artisan | 4 | All endpoints present, business rules correctly implemented |
| dotnet-skills | 3 | Endpoints present but grouped in fewer files; some endpoints may be harder to discover |
| dotnet-webapi | 4 | All endpoints present, business rules correctly implemented, clean separation |
| managedcode | 1 | No project generated |

**Verdict:** **no-skills**, **dotnet-artisan**, and **dotnet-webapi** all correctly implement the specification. **dotnet-skills** groups multiple controllers in single files which hinders discoverability.

---

## 7. Modern C# Adoption [HIGH]

### What each configuration does

| Feature | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi |
|---|---|---|---|---|
| Primary constructors | ❌ | ✅ | ❌ | ✅ |
| Collection expressions `[]` | ❌ `new List<T>()` | ✅ `[]` | ❌ `new List<T>()` | ❌ `new List<T>()` |
| `required` modifier | ❌ | ✅ | ❌ | ✅ |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ |
| Implicit usings | ✅ | ✅ | ✅ | ✅ |
| Target-typed `new()` | ✅ | ✅ | ✅ | ✅ |

```csharp
// no-skills: traditional constructor + old collection init
public class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<OwnerService> _logger;
    public OwnerService(VetClinicDbContext context, ILogger<OwnerService> logger)
    {
        _context = context;
        _logger = logger;
    }
}
public ICollection<Pet> Pets { get; set; } = new List<Pet>();

// dotnet-artisan: primary constructor + collection expression
public sealed class OwnerService(VetClinicDbContext db) : IOwnerService { }
public ICollection<Pet> Pets { get; set; } = [];

// dotnet-webapi: primary constructor + required modifier (but new List)
public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService { }
public required string FirstName { get; set; }
public ICollection<Pet> Pets { get; set; } = new List<Pet>();
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Only file-scoped namespaces and implicit usings; no modern constructor or collection patterns |
| dotnet-artisan | 5 | Full modern C# adoption: primary constructors, collection expressions, required modifier |
| dotnet-skills | 2 | File-scoped namespaces and implicit usings only; traditional constructors throughout |
| dotnet-webapi | 4 | Primary constructors and required modifier, but uses `new List<T>()` instead of `[]` |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-artisan** leads with the most comprehensive adoption of modern C# features. **dotnet-webapi** is close behind, missing only collection expressions.

---

## 8. Error Handling & Middleware [HIGH]

### What each configuration does

**no-skills** and **dotnet-skills** use convention-based middleware (`RequestDelegate`):

```csharp
// no-skills: Middleware/GlobalExceptionHandlerMiddleware.cs
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }
}
// Registered as: app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

**dotnet-artisan** and **dotnet-webapi** use the modern **`IExceptionHandler`** pattern:

```csharp
// dotnet-webapi: Middleware/ApiExceptionHandler.cs
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
        if (statusCode == 0) { logger.LogError(exception, "Unhandled exception"); return false; }
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails { ... }, cancellationToken);
        return true;
    }
}
// Registered as: builder.Services.AddExceptionHandler<ApiExceptionHandler>();
//                builder.Services.AddProblemDetails();
//                app.UseExceptionHandler();
```

Key differences:
- `IExceptionHandler` is DI-aware, composable, and the .NET 8+ standard
- **dotnet-webapi** marks it `internal sealed` (correct access level) and returns `false` for unhandled exceptions (letting the framework's default handler produce a proper response)
- **dotnet-artisan** marks it `public sealed` and always returns `true`

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Convention middleware with ProblemDetails — works but outdated pattern |
| dotnet-artisan | 5 | IExceptionHandler, sealed, primary constructor, ProblemDetails, custom exceptions |
| dotnet-skills | 3 | Convention middleware, sealed class, ProblemDetails, custom exceptions |
| dotnet-webapi | 5 | IExceptionHandler, internal sealed, primary constructor, falls through for unhandled |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-artisan** and **dotnet-webapi** both use the modern `IExceptionHandler` pattern. **dotnet-webapi** is slightly more correct by using `internal sealed` and returning `false` for unhandled exceptions rather than swallowing them.

---

## 9. Async Patterns & Cancellation [HIGH]

### What each configuration does

**no-skills** and **dotnet-skills** omit `CancellationToken` from service methods:

```csharp
// no-skills: OwnerService.cs — no CancellationToken
public async Task<PaginatedResponse<OwnerSummaryDto>> GetAllAsync(string? search, int page, int pageSize)
{
    var query = _context.Owners.AsQueryable();
    // ... no cancellation token forwarded to EF Core
    var items = await query.ToListAsync();
}
```

**dotnet-artisan** and **dotnet-webapi** consistently propagate `CancellationToken`:

```csharp
// dotnet-webapi: OwnerService.cs — CancellationToken everywhere
public async Task<PaginatedResponse<OwnerResponse>> GetAllAsync(
    string? search, int page, int pageSize, CancellationToken ct)
{
    var totalCount = await query.CountAsync(ct);
    var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    return PaginatedResponse<OwnerResponse>.Create(items, page, pageSize, totalCount);
}
```

Additionally, **dotnet-skills** uses a **synchronous** `DataSeeder.Seed(db)` method instead of async.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Async methods but no CancellationToken propagation |
| dotnet-artisan | 5 | CancellationToken in every endpoint and service, forwarded to all EF Core calls |
| dotnet-skills | 2 | No CancellationToken, synchronous seeder |
| dotnet-webapi | 5 | CancellationToken in every endpoint and service, forwarded throughout |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-artisan** and **dotnet-webapi** both achieve perfect async patterns. The baseline and dotnet-skills miss the critical `CancellationToken` propagation pattern entirely.

---

## 10. EF Core Best Practices [HIGH]

### What each configuration does

| Practice | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi |
|---|---|---|---|---|
| `AsNoTracking()` for reads | ❌ | ✅ | ✅ | ✅ |
| Fluent API config | ❌ Conventions | ✅ Extensive | ✅ Extensive | ✅ Extensive |
| Unique index on Email | ❌ Convention | ✅ Explicit | ✅ Explicit | ✅ Explicit |
| `AsSplitQuery()` for multi-Include | ❌ | ❌ | ✅ | ❌ |
| HasData() seed in migrations | ❌ | ❌ | ❌ (with EnsureCreated) | ✅ |

```csharp
// no-skills: no AsNoTracking, convention-only configuration
var owners = await _context.Owners.Where(o => ...).ToListAsync();

// dotnet-artisan: AsNoTracking + Fluent API
var query = db.Owners.AsNoTracking().AsQueryable();
// DbContext: entity.HasIndex(e => e.Email).IsUnique();

// dotnet-webapi: AsNoTracking + Fluent API + HasData + Migrations
var query = db.Owners.AsNoTracking().AsQueryable();
// DbContext: modelBuilder.Entity<Owner>().HasData(new Owner { Id = 1, ... });
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | No AsNoTracking, no Fluent API — entirely convention-based |
| dotnet-artisan | 4 | AsNoTracking, Fluent API, but uses EnsureCreated |
| dotnet-skills | 3 | AsNoTracking, Fluent API, AsSplitQuery, but EnsureCreated |
| dotnet-webapi | 5 | AsNoTracking, Fluent API, HasData, migrations, explicit cascade config |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** is the gold standard with migrations, HasData seeding, and all query optimizations. **dotnet-skills** gets credit for `AsSplitQuery()` usage (from the EF query optimization skill) but is undermined by using `EnsureCreated()`.

---

## 11. Service Abstraction & DI [HIGH]

### What each configuration does

All four configurations use interface-based services registered with `AddScoped<IService, Service>()`.

```csharp
// All configs: Program.cs
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IPetService, PetService>();
// ... etc
```

Key differences:

| Pattern | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi |
|---|---|---|---|---|
| Interface files | Separate files | Co-located in service file | All in `IServices.cs` | Separate files |
| Service per domain | ✅ 7 services | ✅ 7 services | 5 files (combined) | ✅ 7 separate |
| Sealed services | ❌ | ✅ | ✅ | ✅ |

```csharp
// dotnet-skills: all interfaces in one file
// Services/Interfaces/IServices.cs
public interface IOwnerService { ... }
public interface IPetService { ... }
// ... all 7 interfaces in one file

// dotnet-webapi: separate interface files
// Services/IOwnerService.cs
public interface IOwnerService { ... }
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | Interface-based, separate files per service, one service per domain |
| dotnet-artisan | 4 | Interface-based, interfaces co-located with implementations |
| dotnet-skills | 3 | All interfaces in single file, multiple services in single files |
| dotnet-webapi | 5 | Clean separation: one interface file and one implementation file per domain area |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** has the cleanest service organization with separate files for each interface and implementation. **dotnet-skills** bundles everything into single files, harming discoverability.

---

## 12. Security Configuration [HIGH]

### What each configuration does

**None** of the four configurations include HSTS or HTTPS redirection:

```csharp
// Expected but missing in ALL configs:
// if (!app.Environment.IsDevelopment()) { app.UseHsts(); }
// app.UseHttpsRedirection();
```

**dotnet-webapi** includes `UseStatusCodePages()` which is a minor improvement:

```csharp
// dotnet-webapi: Program.cs
app.UseExceptionHandler();
app.UseStatusCodePages();  // Produces basic responses for status codes without a body
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | No HSTS/HTTPS, but no security anti-patterns |
| dotnet-artisan | 2 | No HSTS/HTTPS, but no security anti-patterns |
| dotnet-skills | 2 | No HSTS/HTTPS, but no security anti-patterns |
| dotnet-webapi | 2 | No HSTS/HTTPS, but adds UseStatusCodePages |
| managedcode | 1 | No project generated |

**Verdict:** All configurations miss HSTS and HTTPS redirection. This is a universal gap across all skill configurations for this scenario.

---

## 13. DTO Design [MEDIUM]

### What each configuration does

**no-skills** and **dotnet-skills** use mutable classes:

```csharp
// no-skills: DTOs/OwnerDtos.cs — mutable class, not sealed
public class CreateOwnerDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
public class OwnerDto
{
    public int Id { get; set; }
    public List<PetSummaryDto> Pets { get; set; } = [];
}
```

**dotnet-artisan** uses sealed positional records:

```csharp
// dotnet-artisan: DTOs/OwnerDtos.cs — sealed records
public sealed record CreateOwnerDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    string? Address, string? City,
    [MaxLength(2)] string? State, string? ZipCode);

public sealed record OwnerResponse(
    int Id, string FirstName, string LastName, string Email,
    string Phone, string? Address, string? City, string? State,
    string? ZipCode, DateTime CreatedAt, DateTime UpdatedAt);
```

**dotnet-webapi** uses sealed records with `init` properties:

```csharp
// dotnet-webapi: DTOs/OwnerDtos.cs — sealed records with init + required
public sealed record CreateOwnerRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
    [Required, EmailAddress]
    public required string Email { get; init; }
}

public sealed record OwnerResponse(
    int Id, string FirstName, string LastName, string Email,
    string Phone, string? Address, string? City, string? State,
    string? ZipCode, DateTime CreatedAt, DateTime UpdatedAt,
    IReadOnlyList<PetSummaryResponse>? Pets = null);
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Mutable classes, not sealed, *Dto suffix, `List<T>` in responses |
| dotnet-artisan | 5 | Sealed positional records, *Response/*Dto naming, `IReadOnlyList<T>` |
| dotnet-skills | 2 | Mutable classes, not sealed, all DTOs in single file, inheritance (`UpdateDto : CreateDto`) |
| dotnet-webapi | 5 | Sealed records with init/required, *Request/*Response naming, `IReadOnlyList<T>` |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-artisan** and **dotnet-webapi** both achieve excellent DTO design with sealed, immutable records. **dotnet-webapi** uses the *Request/*Response naming convention which is more descriptive than *Dto.

---

## 14. Sealed Types [MEDIUM]

### What each configuration does

```csharp
// no-skills: nothing is sealed
public class Owner { ... }
public class OwnerService : IOwnerService { ... }
public class GlobalExceptionHandlerMiddleware { ... }

// dotnet-artisan: everything is sealed
public sealed class Owner { ... }
public sealed class OwnerService(VetClinicDbContext db) : IOwnerService { ... }
public sealed class GlobalExceptionHandler : IExceptionHandler { ... }

// dotnet-skills: services and controllers sealed, models NOT sealed
public class Owner { ... }  // NOT sealed
public sealed class OwnerService : IOwnerService { ... }
public sealed class OwnersController : ControllerBase { ... }

// dotnet-webapi: everything is sealed
public sealed class Owner { ... }
public sealed class OwnerService(...) : IOwnerService { ... }
internal sealed class ApiExceptionHandler : IExceptionHandler { ... }
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 1 | Nothing sealed — models, services, DTOs, middleware all unsealed |
| dotnet-artisan | 5 | All models, services, DTOs (records), and middleware sealed |
| dotnet-skills | 3 | Services and controllers sealed; models and DTOs not sealed |
| dotnet-webapi | 5 | All models, services, DTOs (records), and middleware sealed |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-artisan** and **dotnet-webapi** consistently seal everything. **dotnet-skills** partially applies sealing (via the performance analysis skill) but only to services and controllers, missing models.

---

## 15. Data Seeder Design [MEDIUM]

### What each configuration does

```csharp
// no-skills, dotnet-artisan: Separate DataSeeder class
public static class DataSeeder
{
    public static async Task SeedAsync(VetClinicDbContext context)
    {
        if (context.Owners.Any()) return;  // Idempotency check
        var owners = new List<Owner> { new() { ... }, ... };
        context.Owners.AddRange(owners);
        await context.SaveChangesAsync();
    }
}

// dotnet-skills: HasData() in OnModelCreating (but with EnsureCreated)
// Data/VetClinicDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... Fluent API config ...
    modelBuilder.Entity<Owner>().HasData(new Owner { Id = 1, ... });
}

// dotnet-webapi: HasData() in OnModelCreating WITH migrations
modelBuilder.Entity<Owner>().HasData(
    new Owner { Id = 1, FirstName = "Sarah", ... });
// Seed data captured in Migrations/20260329032811_InitialCreate.cs
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Separate async seeder, idempotent, realistic data |
| dotnet-artisan | 3 | Separate async seeder, idempotent, realistic data |
| dotnet-skills | 3 | HasData() is good design but undermined by EnsureCreated |
| dotnet-webapi | 5 | HasData() integrated with actual migrations — gold standard |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** uses `HasData()` with migrations, which is the recommended EF Core approach. The seed data becomes part of the migration and is reproducible and version-controlled.

---

## 16. Structured Logging [MEDIUM]

### What each configuration does

All four configurations inject `ILogger<T>` into services:

```csharp
// no-skills: traditional injection + structured templates
private readonly ILogger<OwnerService> _logger;
_logger.LogError(ex, "An unhandled exception occurred");

// dotnet-artisan: primary constructor + structured templates
public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger)
logger.LogError(exception, "Unhandled exception occurred");

// dotnet-webapi: primary constructor + structured templates with placeholders
logger.LogInformation("Owner created: {OwnerId} {FirstName} {LastName}", owner.Id, owner.FirstName, owner.LastName);
logger.LogWarning(exception, "Handled API exception: {Title}", title);
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | ILogger injected, basic structured logging, no LoggerMessage source generators |
| dotnet-artisan | 4 | ILogger via primary constructor, structured templates, consistent usage |
| dotnet-skills | 3 | ILogger injected, basic structured logging |
| dotnet-webapi | 4 | ILogger via primary constructor, structured templates with named placeholders |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-artisan** and **dotnet-webapi** both demonstrate good logging practices with primary constructor injection and structured templates. None use high-performance `[LoggerMessage]` source generators, which would warrant a 5.

---

## 17. Nullable Reference Types [MEDIUM]

All four generated projects enable NRT in their `.csproj`:

```xml
<Nullable>enable</Nullable>
```

All properly use `?` annotations on optional properties and `null!` for required navigation properties. Score: **4** for all generated projects (1 for managedcode). None lose a point for `!` misuse. Not scored 5 because optional navigation properties in entities could be more carefully annotated.

---

## 18. API Documentation [MEDIUM]

### What each configuration does

```csharp
// no-skills: ProducesResponseType attributes on controllers
[ProducesResponseType(typeof(PaginatedResponse<OwnerSummaryDto>), 200)]
[ProducesResponseType(typeof(ValidationProblemDetails), 400)]
public async Task<IActionResult> Create([FromBody] CreateOwnerDto dto) { ... }

// dotnet-artisan: WithName + WithSummary on minimal API endpoints
group.MapGet("/", async (...) => TypedResults.Ok(...))
    .WithName("GetOwners")
    .WithSummary("List all owners with optional search and pagination");

// dotnet-webapi: Full metadata chain
group.MapPost("/", async Task<Created<OwnerResponse>> (...) => { ... })
    .WithName("CreateOwner")
    .WithSummary("Create a new owner")
    .WithDescription("Creates a new pet owner. Email must be unique.")
    .Produces<OwnerResponse>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status409Conflict);
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | ProducesResponseType attributes, Swagger doc configured |
| dotnet-artisan | 4 | WithName, WithSummary, WithTags on all endpoints |
| dotnet-skills | 3 | ProducesResponseType attributes, Swagger doc configured |
| dotnet-webapi | 5 | Full WithName/WithSummary/WithDescription/Produces chain, plus explicit return types |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** provides the richest API documentation with descriptions, summaries, and explicit response type documentation for every possible status code.

---

## 19. File Organization [MEDIUM]

### What each configuration does

| Aspect | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi |
|---|---|---|---|---|
| API Layer | `Controllers/` (7 files) | `Endpoints/` (7 files) | `Controllers/` (5 files) | `Endpoints/` (7 files) |
| DTOs | `DTOs/` (8 files) | `DTOs/` (8 files) | `DTOs/Dtos.cs` (1 file) | `DTOs/` (8 files) |
| Interfaces | Separate files in `Services/` | Co-located | `Services/Interfaces/IServices.cs` (1 file) | Separate files in `Services/` |
| Services | 7 files + 7 interfaces | 7 files (with interfaces) | 5 files (combined) | 7 files + 7 interfaces |
| Migrations | None | None | None | `Migrations/` (3 files) |
| Program.cs size | Clean (delegating) | Clean (~70 lines) | Clean (~40 lines) | Clean (~50 lines) |

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Well-organized controllers structure, separate DTO files, clean |
| dotnet-artisan | 4 | Endpoints/ directory, clean separation, missing .http file |
| dotnet-skills | 2 | All DTOs in one file, all interfaces in one file, combined controllers/services |
| dotnet-webapi | 5 | Best organization: Endpoints/, separate DTOs, separate service interfaces, Migrations/ |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** has the most maintainable file organization. **dotnet-skills** suffers from file consolidation that reduces discoverability.

---

## 20. HTTP Test File Quality [MEDIUM]

### What each configuration does

- **no-skills**: Has `VetClinicApi.http` in `src/VetClinicApi/` with requests covering all endpoints
- **dotnet-artisan**: **No .http file generated** ❌
- **dotnet-skills**: Has `VetClinicApi.http` with requests covering endpoints
- **dotnet-webapi**: Has `VetClinicApi.http` with organized sections, realistic data matching seed IDs

```http
# dotnet-webapi: VetClinicApi.http (excerpt)
@baseUrl = http://localhost:5118

### ===================== Owners =====================

### Get all owners (paginated)
GET {{baseUrl}}/api/owners?page=1&pageSize=10

### Search owners by name
GET {{baseUrl}}/api/owners?search=johnson

### Create a new owner
POST {{baseUrl}}/api/owners
Content-Type: application/json

{
  "firstName": "Robert",
  "lastName": "Taylor",
  "email": "robert.taylor@email.com",
  "phone": "555-0106"
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | .http file present with endpoint coverage |
| dotnet-artisan | 1 | **No .http file generated at all** |
| dotnet-skills | 3 | .http file present with endpoint coverage |
| dotnet-webapi | 4 | Well-organized .http file with realistic data matching seed IDs |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-webapi** has the best .http file. **dotnet-artisan** completely omits it despite generating excellent code otherwise — a significant gap for developer experience.

---

## 21. Type Design & Resource Management [MEDIUM]

All four projects use a proper `AppointmentStatus` enum instead of magic strings:

```csharp
// All configs: Models/AppointmentStatus.cs
public enum AppointmentStatus
{
    Scheduled, CheckedIn, InProgress, Completed, Cancelled, NoShow
}
```

**dotnet-artisan** and **dotnet-webapi** use `IReadOnlyList<T>` for response collections:

```csharp
// dotnet-webapi: PaginatedResponse.cs
public sealed record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
}
```

While **no-skills** uses mutable `List<T>`:
```csharp
public List<PetSummaryDto> Pets { get; set; } = [];
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Proper enums, but `List<T>` instead of `IReadOnlyList<T>` |
| dotnet-artisan | 4 | Proper enums, `IReadOnlyList<T>`, computed properties |
| dotnet-skills | 3 | Proper enums, but `List<T>` in DTOs |
| dotnet-webapi | 4 | Proper enums, `IReadOnlyList<T>`, computed properties |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-artisan** and **dotnet-webapi** both demonstrate precise return types with `IReadOnlyList<T>`.

---

## 22. Code Standards Compliance [LOW]

| Standard | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi |
|---|---|---|---|---|
| PascalCase public members | ✅ | ✅ | ✅ | ✅ |
| camelCase parameters | ✅ | ✅ | ✅ | ✅ |
| Async suffix | ✅ | ✅ | ✅ | ✅ |
| I prefix on interfaces | ✅ | ✅ | ✅ | ✅ |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ |
| Explicit access modifiers | Partial | ✅ | ✅ | ✅ |

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | Follows core conventions, some missing explicit modifiers |
| dotnet-artisan | 4 | Consistent naming, explicit modifiers throughout |
| dotnet-skills | 3 | Follows core conventions, sealed applied after the fact |
| dotnet-webapi | 4 | Consistent naming, `internal sealed` on handler shows deliberate access control |
| managedcode | 1 | No project generated |

**Verdict:** **dotnet-artisan** and **dotnet-webapi** both demonstrate deliberate, consistent code standards. The `internal sealed` modifier on `ApiExceptionHandler` in dotnet-webapi shows particular care.

---

## Weighted Summary

Weighted scoring: **CRITICAL ×3**, **HIGH ×2**, **MEDIUM ×1**, **LOW ×0.5**

| Dimension | Tier | Weight | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|---|---|
| Minimal API Architecture | CRITICAL | ×3 | 3 | 15 | 3 | 15 | 3 |
| Input Validation | CRITICAL | ×3 | 9 | 12 | 12 | 12 | 3 |
| NuGet & Package Discipline | CRITICAL | ×3 | 9 | 12 | 3 | 15 | 3 |
| EF Migration Usage | CRITICAL | ×3 | 3 | 3 | 3 | 15 | 3 |
| Prefer Built-in | HIGH | ×2 | 4 | 6 | 2 | 10 | 2 |
| Business Logic | HIGH | ×2 | 8 | 8 | 6 | 8 | 2 |
| Modern C# | HIGH | ×2 | 4 | 10 | 4 | 8 | 2 |
| Error Handling | HIGH | ×2 | 6 | 10 | 6 | 10 | 2 |
| Async & Cancellation | HIGH | ×2 | 4 | 10 | 4 | 10 | 2 |
| EF Core Practices | HIGH | ×2 | 4 | 8 | 6 | 10 | 2 |
| Service Abstraction | HIGH | ×2 | 8 | 8 | 6 | 10 | 2 |
| Security Config | HIGH | ×2 | 4 | 4 | 4 | 4 | 2 |
| DTO Design | MEDIUM | ×1 | 2 | 5 | 2 | 5 | 1 |
| Sealed Types | MEDIUM | ×1 | 1 | 5 | 3 | 5 | 1 |
| Data Seeder | MEDIUM | ×1 | 3 | 3 | 3 | 5 | 1 |
| Structured Logging | MEDIUM | ×1 | 3 | 4 | 3 | 4 | 1 |
| Nullable Reference Types | MEDIUM | ×1 | 4 | 4 | 4 | 4 | 1 |
| API Documentation | MEDIUM | ×1 | 3 | 4 | 3 | 5 | 1 |
| File Organization | MEDIUM | ×1 | 3 | 4 | 2 | 5 | 1 |
| HTTP Test File | MEDIUM | ×1 | 3 | 1 | 3 | 4 | 1 |
| Type Design | MEDIUM | ×1 | 3 | 4 | 3 | 4 | 1 |
| Code Standards | LOW | ×0.5 | 1.5 | 2 | 1.5 | 2 | 0.5 |
| **TOTAL** | | | **91.5** | **142** | **86.5** | **170** | **37.5** |

### Rankings

| Rank | Configuration | Weighted Score | Percentage |
|---|---|---|---|
| 🥇 1st | **dotnet-webapi** | **170** | 100% |
| 🥈 2nd | **dotnet-artisan** | **142** | 83.5% |
| 🥉 3rd | **no-skills** | **91.5** | 53.8% |
| 4th | **dotnet-skills** | **86.5** | 50.9% |
| 5th | **managedcode-dotnet-skills** | **37.5** | 22.1% |

---

## What All Versions Get Right

Across all four generated projects, these practices are consistently followed:

- **Nullable reference types** enabled via `<Nullable>enable</Nullable>` in `.csproj`
- **Implicit usings** enabled for cleaner files
- **File-scoped namespaces** (`namespace X;`) used consistently
- **AppointmentStatus enum** instead of magic strings for status fields
- **Interface-based service layer** with `AddScoped<IService, Service>()` DI registration
- **Global error handling** with ProblemDetails (RFC 7807) responses
- **JSON enum string serialization** via `JsonStringEnumConverter`
- **ILogger<T>** injection in services and middleware
- **Comprehensive business rules**: appointment conflict detection, status workflow, cancellation rules, medical record constraints, vaccination tracking, pet soft-delete
- **Pagination** with page/pageSize parameters across list endpoints
- **Realistic seed data** with proper entity relationships
- **.NET 10** targeting with modern framework features
- **SQLite** with connection string in `appsettings.json`

---

## Summary: Impact of Skills

### Most Impactful Differences

1. **EF Core Migrations** (dotnet-webapi only): The single biggest differentiator. Only `dotnet-webapi` generates production-safe migration code instead of the `EnsureCreated()` anti-pattern. This alone is a critical quality gate.

2. **Minimal API Architecture** (dotnet-artisan, dotnet-webapi): The two skills that guide toward Minimal APIs produce fundamentally better architecture — `MapGroup()`, `TypedResults`, and endpoint extension methods result in more concise, type-safe, and modern code.

3. **CancellationToken Propagation** (dotnet-artisan, dotnet-webapi): A subtle but critical difference. Cancellation token propagation prevents wasted server resources and is essential for production readiness.

4. **Package Discipline** (dotnet-webapi): Generating code with only 3 NuGet packages (no Swashbuckle) versus 5 with wildcard versions (dotnet-skills) has huge implications for supply-chain security and build reproducibility.

5. **Modern C# Patterns** (dotnet-artisan, dotnet-webapi): Primary constructors, sealed records, and collection expressions produce significantly more concise and idiomatic code.

### Overall Assessment

| Configuration | Assessment |
|---|---|
| **dotnet-webapi** 🥇 | The clear winner across all dimensions. Only configuration to use EF Core migrations, avoid third-party packages entirely, and produce proper OpenAPI documentation. Sets the standard for modern .NET API development. |
| **dotnet-artisan** 🥈 | Strong second place with excellent Minimal API architecture, modern C# adoption (best collection expressions), and proper error handling. Falls short on migrations (EnsureCreated) and missing .http test file. |
| **no-skills** 🥉 | Produces working, conventional code but follows outdated patterns (controllers, no AsNoTracking, no CancellationToken, mutable DTOs). Serves as a useful baseline showing what Copilot generates without skill guidance. |
| **dotnet-skills** 4th | Despite using specialized skills (performance scanner, EF query optimizer), the output has critical issues: wildcard NuGet versions, redundant OpenAPI configurations, third-party FluentValidation, and files crammed together. The skills improve some aspects (AsNoTracking, AsSplitQuery, sealed services) but introduce new problems. |
| **managedcode-dotnet-skills** 5th | Complete failure — no project generated. The configuration was unable to produce any usable output for this scenario. |

### Key Takeaway

The **dotnet-webapi** skill demonstrates that a well-designed, focused skill can dramatically improve code generation quality — achieving a **170-point weighted score** versus the baseline's **91.5 points** (an 86% improvement). The skill's influence is visible in every file: from architecture choices (Minimal APIs) to micro-patterns (sealed types, init properties, CancellationToken). The most impactful skills are those that encode **opinionated best practices** rather than offering generic guidance.
