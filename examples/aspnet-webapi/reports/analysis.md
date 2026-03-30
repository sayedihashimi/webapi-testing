# Aggregated Analysis: ASP.NET Core Web API Skill Evaluation

**Runs:** 3 | **Configurations:** 4 | **Scenarios:** 3 | **Dimensions:** 24
**Date:** 2026-03-30 05:22 UTC

---

## Scoring Methodology

Each dimension is scored on a **1–5 scale**:

| Score | Meaning |
|:---:|---|
| 5 | Excellent — follows all best practices |
| 4 | Good — minor gaps only |
| 3 | Acceptable — some issues present |
| 2 | Below average — significant gaps |
| 1 | Poor — missing or fundamentally wrong |

Dimensions are grouped into **tiers** that determine their weight in the final weighted score:

| Tier | Weight | Dimensions |
|---|:---:|:---:|
| CRITICAL | ×3 | 6 |
| HIGH | ×2 | 8 |
| MEDIUM | ×1 | 9 |
| LOW | ×0.5 | 1 |

**Maximum possible weighted score: 217.5** (all dimensions scoring 5).
Scores shown as **mean ± standard deviation** across runs.

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 5.0 | 4.7 ± 0.6 | 5.0 | 5.0 |
| Security Vulnerability Scan [CRITICAL] | 4.3 ± 0.6 | 4.0 ± 1.0 | 4.7 ± 0.6 | 4.7 ± 0.6 |
| Minimal API Architecture [CRITICAL] | 1.0 | 5.0 | 1.0 | 1.7 ± 1.2 |
| Input Validation & Guard Clauses [CRITICAL] | 3.0 | 4.0 | 3.3 ± 0.6 | 3.7 ± 0.6 |
| NuGet & Package Discipline [CRITICAL] | 2.3 ± 0.6 | 4.0 ± 1.0 | 3.7 ± 0.6 | 3.7 ± 0.6 |
| EF Migration Usage [CRITICAL] | 1.7 ± 0.6 | 1.7 ± 0.6 | 1.7 ± 0.6 | 1.7 ± 0.6 |
| Business Logic Correctness [HIGH] | 5.0 | 5.0 | 5.0 | 5.0 |
| Prefer Built-in over 3rd Party [HIGH] | 1.7 ± 0.6 | 4.3 ± 0.6 | 2.3 ± 0.6 | 2.3 ± 0.6 |
| Modern C# Adoption [HIGH] | 1.7 ± 0.6 | 5.0 | 3.7 ± 0.6 | 2.3 ± 0.6 |
| Error Handling & Middleware [HIGH] | 3.0 ± 1.0 | 4.3 ± 0.6 | 4.7 ± 0.6 | 3.7 ± 0.6 |
| Async Patterns & Cancellation [HIGH] | 2.3 ± 0.6 | 5.0 | 3.3 ± 1.5 | 2.3 ± 0.6 |
| EF Core Best Practices [HIGH] | 2.7 ± 0.6 | 4.7 ± 0.6 | 4.3 ± 1.2 | 4.3 ± 0.6 |
| Service Abstraction & DI [HIGH] | 4.3 ± 0.6 | 5.0 | 4.3 ± 0.6 | 4.7 ± 0.6 |
| Security Configuration [HIGH] | 2.0 | 2.0 | 2.0 | 2.0 |
| DTO Design [MEDIUM] | 2.7 ± 1.2 | 5.0 | 3.0 ± 1.0 | 2.7 ± 1.2 |
| Sealed Types [MEDIUM] | 1.0 | 5.0 | 1.0 | 3.7 ± 1.2 |
| Data Seeder Design [MEDIUM] | 4.0 | 4.3 ± 0.6 | 4.0 | 4.0 |
| Structured Logging [MEDIUM] | 4.0 | 4.0 | 4.0 | 4.0 |
| Nullable Reference Types [MEDIUM] | 4.3 ± 0.6 | 4.3 ± 0.6 | 4.3 ± 0.6 | 4.3 ± 0.6 |
| API Documentation [MEDIUM] | 3.3 ± 0.6 | 5.0 | 3.3 ± 0.6 | 3.3 ± 0.6 |
| File Organization [MEDIUM] | 3.0 ± 1.0 | 5.0 | 4.0 ± 1.0 | 3.3 ± 1.2 |
| HTTP Test File Quality [MEDIUM] | 4.0 | 4.7 ± 0.6 | 4.3 ± 0.6 | 4.0 |
| Type Design & Resource Management [MEDIUM] | 3.3 ± 0.6 | 4.3 ± 0.6 | 3.7 ± 0.6 | 3.7 ± 0.6 |
| Code Standards Compliance [LOW] | 3.7 ± 0.6 | 5.0 | 4.0 | 4.0 ± 1.0 |

---

## Final Rankings

| Rank | Configuration | Mean Score | % of Max (217.5) | Std Dev | Min | Max |
|---|---|---|---|---|---|---|
| 🥇 | dotnet-artisan | 184.8 | 85% | 5.5 | 179.5 | 190.5 |
| 🥈 | managedcode-dotnet-skills | 151.0 | 69% | 7.0 | 143.0 | 156.0 |
| 🥉 | dotnet-skills | 149.3 | 69% | 13.2 | 137.5 | 163.5 |
| 4th | no-skills | 128.8 | 59% | 10.1 | 117.5 | 137.0 |

---

## Weighted Score per Run

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 137.0 | 190.5 | 154.0 | 163.5 |
| 2 | 117.5 | 179.5 | 156.0 | 137.5 |
| 3 | 132.0 | 184.5 | 143.0 | 147.0 |
| **Mean** | **128.8** | **184.8** | **151.0** | **149.3** |

---

## Verification Summary (All Runs)

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |
|---|---|---|---|
| no-skills | 3/3 (100%) | 3/3 (100%) | 130.0 |
| dotnet-artisan | 3/3 (100%) | 3/3 (100%) | 114.7 |
| managedcode-dotnet-skills | 3/3 (100%) | 3/3 (100%) | 127.3 |
| dotnet-skills | 3/3 (100%) | 3/3 (100%) | 94.0 |

---

## Asset Usage Summary

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | 6a98279d…dfcd | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 2 | c5d6d655…f0d6 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 3 | abd996f5…6359 | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-artisan | 1 | 5ee8fd93…7950 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api | dotnet-artisan | ✅ |
| dotnet-artisan | 2 | e7ed9d47…2379 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| dotnet-artisan | 3 | 894060c3…30d6 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | 86dfb05b…7c29 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-project-setup, dotnet-modern-csharp, dotnet-entity-framework-core, dotnet-minimal-apis, dotnet-microsoft-extensions | — | ✅ |
| managedcode-dotnet-skills | 2 | e140e302…53d4 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-modern-csharp, dotnet-project-setup | — | ✅ |
| managedcode-dotnet-skills | 3 | f163762c…d1ca | claude-opus-4.6-1m | dotnet-aspnet-core | — | ✅ |
| dotnet-skills | 1 | 00d36ed0…aadd | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 2 | ee48f009…90dd | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 3 | 52aa5393…f78f | claude-opus-4.6-1m | optimizing-ef-core-queries, analyzing-dotnet-performance | dotnet-data, dotnet-diag | ✅ |

---

## Consistency Analysis

| Configuration | Score σ | Most Consistent Dim (σ) | Most Variable Dim (σ) |
|---|---|---|---|
| no-skills | 10.1 | Build & Run Success (0.0) | DTO Design (1.2) |
| dotnet-artisan | 5.5 | Minimal API Architecture (0.0) | Security Vulnerability Scan (1.0) |
| managedcode-dotnet-skills | 7.0 | Build & Run Success (0.0) | Async Patterns & Cancellation (1.5) |
| dotnet-skills | 13.2 | Build & Run Success (0.0) | Minimal API Architecture (1.2) |

---

## Per-Dimension Analysis

### 1. Build & Run Success [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 |
| 2 | 5 | 5 | 5 | 5 |
| 3 | 5 | 4 | 5 | 5 |
| **Mean** | **5.0** | **4.7** | **5.0** | **5.0** |

#### Analysis

All four configurations produce projects that compile with zero errors.

| Config | Errors | Warnings | Build Result |
|---|---|---|---|
| no-skills | 0 | 0 | ✅ Build succeeded |
| dotnet-artisan | 0 | 1 (NU1903 transitive vuln) | ✅ Build succeeded |
| dotnet-skills | 0 | 0 | ✅ Build succeeded |
| managedcode | 0 | 0 | ✅ Build succeeded |

The dotnet-artisan build emits one warning due to a transitive dependency (`Microsoft.Build.Tasks.Core 17.7.2` via `Microsoft.EntityFrameworkCore.Design` preview) with a known vulnerability (GHSA-h4j7-5rxr-p4wc).

**Scores**: no-skills: **5** | dotnet-artisan: **4** (transitive vuln warning) | dotnet-skills: **5** | managedcode: **5**

**Verdict**: All configs produce compilable projects. dotnet-artisan loses a point for the NU1903 warning from preview packages.

---

### 2. Security Vulnerability Scan [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 |
| 2 | 4 | 4 | 4 | 4 |
| 3 | 4 | 3 | 5 | 5 |
| **Mean** | **4.3** | **4.0** | **4.7** | **4.7** |

#### Analysis

`dotnet list package --vulnerable` was run against all projects.

| Config | Vulnerable Packages | Floating Versions | Notes |
|---|---|---|---|
| no-skills | 0 direct | ⚠️ `10.*` on 2 packages | Floating versions risk pulling vulnerable releases |
| dotnet-artisan | 0 direct, 1 transitive | None | NU1903: `Microsoft.Build.Tasks.Core` 17.7.2 (preview dep) |
| dotnet-skills | 0 | None | Clean |
| managedcode | 0 | None | Clean |

```xml
<!-- no-skills: floating versions -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />

<!-- dotnet-artisan: pinned preview versions -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-preview.3.25171.5" />
```

**Scores**: no-skills: **4** (floating versions risk) | dotnet-artisan: **3** (transitive vuln) | dotnet-skills: **5** | managedcode: **5**

**Verdict**: dotnet-skills and managedcode are cleanest. no-skills uses floating `10.*` versions which could pull vulnerable patches. dotnet-artisan's preview packages introduce a transitive vulnerability.

---

### 3. Minimal API Architecture [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 1 | 5 | 1 | 3 |
| 2 | 1 | 5 | 1 | 1 |
| 3 | 1 | 5 | 1 | 1 |
| **Mean** | **1.0** | **5.0** | **1.0** | **1.7** |

#### Analysis

This is the single largest architectural divergence across all configurations.

**dotnet-artisan** uses Minimal APIs with MapGroup, TypedResults, and endpoint extension methods:

```csharp
// dotnet-artisan: Program.cs — clean endpoint registration
app.MapOwnerEndpoints();
app.MapPetEndpoints();
app.MapVeterinarianEndpoints();
app.MapAppointmentEndpoints();
```

```csharp
// dotnet-artisan: OwnerEndpoints.cs — MapGroup with metadata
public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/owners").WithTags("Owners");
        group.MapGet("/", GetAll).WithSummary("List all owners with search and pagination");
        group.MapGet("/{id:int}", GetById).WithSummary("Get owner by ID with pets");
        group.MapPost("/", Create).WithSummary("Create a new owner");
        // ...
        return group;
    }

    private static async Task<Results<Ok<OwnerResponse>, NotFound>> GetById(
        int id, IOwnerService service, CancellationToken ct = default)
    {
        var owner = await service.GetByIdAsync(id, ct);
        return owner is not null ? TypedResults.Ok(owner) : TypedResults.NotFound();
    }
}
```

All three other configs use traditional **Controllers**:

```csharp
// no-skills, dotnet-skills, managedcode: Controller pattern
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OwnersController : ControllerBase
{
    private readonly IOwnerService _service;
    public OwnersController(IOwnerService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OwnerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, ...)
        => Ok(await _service.GetAllAsync(search, page, pageSize));
}
```

**Scores**: no-skills: **1** | dotnet-artisan: **5** | dotnet-skills: **1** | managedcode: **1**

**Verdict**: dotnet-artisan is the only config using Minimal APIs — the modern .NET standard. It demonstrates MapGroup, TypedResults, `Results<T1, T2>` union return types, and extension methods, producing compile-time type-safe OpenAPI schemas. The controller approach used by all others is functional but outdated for new .NET projects.

---

### 4. Input Validation & Guard Clauses [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 4 | 3 | 4 |
| 2 | 3 | 4 | 4 | 3 |
| 3 | 3 | 4 | 3 | 4 |
| **Mean** | **3.0** | **4.0** | **3.3** | **3.7** |

#### Analysis

All configurations use Data Annotations on DTOs for input validation. dotnet-skills additionally uses FluentValidation.

```csharp
// no-skills, managedcode: Data Annotations on DTO classes
public class CreatePetDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue, ErrorMessage = "Weight must be positive")]
    public decimal? Weight { get; set; }
}
```

```csharp
// dotnet-artisan: Data Annotations on sealed record DTOs
public sealed record CreatePetRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;
    [Range(0.01, double.MaxValue)]
    public decimal? Weight { get; init; }
}
```

```csharp
// dotnet-skills: FluentValidation in addition to Data Annotations
public class CreatePetValidator : AbstractValidator<CreatePetDto>
{
    public CreatePetValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Weight).GreaterThan(0).When(x => x.Weight.HasValue);
    }
}
```

For guard clauses, only dotnet-artisan consistently validates at both endpoint and service boundaries. None of the configs use `ArgumentNullException.ThrowIfNull()` — they all rely on custom exceptions instead.

**Scores**: no-skills: **3** | dotnet-artisan: **4** | dotnet-skills: **4** (FluentValidation) | managedcode: **3**

**Verdict**: dotnet-skills earns extra credit for FluentValidation. dotnet-artisan validates comprehensively via tuple return patterns. None use modern `ThrowIfNull` guard clauses on constructor parameters.

---

### 5. NuGet & Package Discipline [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 4 | 3 | 4 |
| 2 | 3 | 3 | 4 | 4 |
| 3 | 2 | 5 | 4 | 3 |
| **Mean** | **2.3** | **4.0** | **3.7** | **3.7** |

#### Analysis

```xml
<!-- no-skills: 4 packages, 2 floating -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />

<!-- dotnet-artisan: 4 packages, all pinned, NO Swashbuckle -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0-preview.3.25172.1" />
<PackageReference Include="Scalar.AspNetCore" Version="2.0.36" />

<!-- dotnet-skills: 5 packages, all pinned, includes FluentValidation + Swashbuckle -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />

<!-- managedcode: 4 packages, all pinned, includes Swashbuckle -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

| Config | Total Packages | Floating Versions | Swashbuckle | Unnecessary Packages |
|---|---|---|---|---|
| no-skills | 4 | 2 (`10.*`) | Yes | Redundant (OpenApi + Swashbuckle) |
| dotnet-artisan | 4 | 0 | No | None |
| dotnet-skills | 5 | 0 | Yes | FluentValidation (3rd party) |
| managedcode | 4 | 0 | Yes | Redundant (OpenApi + Swashbuckle) |

**Scores**: no-skills: **2** (floating versions) | dotnet-artisan: **5** (minimal, pinned) | dotnet-skills: **3** (extra deps) | managedcode: **4** (pinned, slight redundancy)

**Verdict**: dotnet-artisan is exemplary — minimal packages, all pinned, no Swashbuckle. no-skills is worst with floating `10.*` versions that can break reproducibility.

---

### 6. EF Migration Usage [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 2 |
| 2 | 1 | 1 | 1 | 1 |
| 3 | 2 | 2 | 2 | 2 |
| **Mean** | **1.7** | **1.7** | **1.7** | **1.7** |

#### Analysis

**All four configurations use `EnsureCreated()`** — the anti-pattern that bypasses EF Core migrations.

```csharp
// All configs: Program.cs
var db = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
await db.Database.EnsureCreatedAsync();  // ❌ Anti-pattern
await DataSeeder.SeedAsync(db);
```

None call `db.Database.MigrateAsync()`. While all include `Microsoft.EntityFrameworkCore.Design` (suggesting awareness of migrations), no migration files exist in any project.

**Scores**: no-skills: **2** | dotnet-artisan: **2** | dotnet-skills: **2** | managedcode: **2**

**Verdict**: Universal failure. All configs use `EnsureCreated()`, making schema evolution impossible without data loss. This is a significant gap across all Copilot configurations.

---

### 7. Business Logic Correctness [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 |
| 2 | 5 | 5 | 5 | 5 |
| 3 | 5 | 5 | 5 | 5 |
| **Mean** | **5.0** | **5.0** | **5.0** | **5.0** |

#### Analysis

All configurations implement the spec's business rules comprehensively:

| Business Rule | no-skills | dotnet-artisan | dotnet-skills | managedcode |
|---|---|---|---|---|
| Scheduling conflict detection | ✅ | ✅ | ✅ | ✅ |
| Appointment status workflow | ✅ | ✅ | ✅ | ✅ |
| Cancellation rules (reason + future only) | ✅ | ✅ | ✅ | ✅ |
| Medical record state enforcement | ✅ | ✅ | ✅ | ✅ |
| Prescription date calculation | ✅ | ✅ | ✅ | ✅ |
| Vaccination due/overdue tracking | ✅ | ✅ | ✅ | ✅ |
| Pet soft delete | ✅ | ✅ | ✅ | ✅ |
| Owner delete guard (active pets) | ✅ | ✅ | ✅ | ✅ |
| Email/license uniqueness | ✅ | ✅ | ✅ | ✅ |

All configs implement the same scheduling conflict detection logic:

```csharp
// Shared pattern across all configs
var hasConflict = await query.AnyAsync(a =>
    proposedStart < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
    proposedEnd > a.AppointmentDate);
```

And the same status workflow validation:

```csharp
// Shared transition map
Scheduled → [CheckedIn, Cancelled, NoShow]
CheckedIn → [InProgress, Cancelled]
InProgress → [Completed]
Completed, Cancelled, NoShow → [] (terminal)
```

**Scores**: no-skills: **5** | dotnet-artisan: **5** | dotnet-skills: **5** | managedcode: **5**

**Verdict**: All configurations fully implement the specification. Business logic correctness is independent of the skill configuration — the underlying LLM handles domain logic well regardless.

---

### 8. Prefer Built-in over 3rd Party [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 4 | 2 | 2 |
| 2 | 1 | 4 | 3 | 3 |
| 3 | 2 | 5 | 2 | 2 |
| **Mean** | **1.7** | **4.3** | **2.3** | **2.3** |

#### Analysis

| Config | Swashbuckle | FluentValidation | Other 3rd Party | Uses Built-in OpenAPI |
|---|---|---|---|---|
| no-skills | ✅ Present | No | None | Yes (+ Swashbuckle) |
| dotnet-artisan | ❌ Not present | No | Scalar.AspNetCore | Yes (AddOpenApi/MapOpenApi) |
| dotnet-skills | ✅ Present | ✅ Present | None | Yes (+ Swashbuckle) |
| managedcode | ✅ Present | No | None | Yes (+ Swashbuckle) |

```csharp
// dotnet-artisan: Pure built-in OpenAPI
builder.Services.AddOpenApi("v1", options => { /* document transformer */ });
app.MapOpenApi();
app.MapScalarApiReference();  // Scalar for UI only

// no-skills, dotnet-skills, managedcode: Swashbuckle
builder.Services.AddSwaggerGen();
app.UseSwagger();
app.UseSwaggerUI();
```

**Scores**: no-skills: **2** | dotnet-artisan: **5** | dotnet-skills: **2** | managedcode: **2**

**Verdict**: dotnet-artisan exclusively uses built-in `AddOpenApi()`/`MapOpenApi()` with Scalar for interactive docs — the modern approach. All others redundantly include both Swashbuckle and OpenApi. dotnet-skills additionally adds FluentValidation when Data Annotations suffice.

---

### 9. Modern C# Adoption [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 5 | 4 | 3 |
| 2 | 1 | 5 | 4 | 2 |
| 3 | 2 | 5 | 3 | 2 |
| **Mean** | **1.7** | **5.0** | **3.7** | **2.3** |

#### Analysis

| Feature | no-skills | dotnet-artisan | dotnet-skills | managedcode |
|---|---|---|---|---|
| Primary constructors | ❌ | ✅ | ❌ | ❌ |
| Records for DTOs | ❌ | ✅ (sealed records) | ❌ | ❌ |
| Collection expressions `[]` | ❌ (`new List<T>()`) | ✅ | ❌ (`new List<T>()`) | ✅ |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ |
| Init accessors | ❌ | ✅ | ❌ | ❌ |
| Target-typed new | ✅ | ✅ | ✅ | ✅ |
| ImplicitUsings | ✅ | ✅ | ✅ | ✅ |
| Positional records | ❌ | ✅ | ❌ | ❌ |
| Tuple deconstruction | ❌ | ✅ | ❌ | ❌ |

```csharp
// dotnet-artisan: Primary constructor + sealed
public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PagedResult<OwnerResponse>> GetAllAsync(..., CancellationToken ct = default)
    {
        var query = db.Owners.AsNoTracking().AsQueryable();
        // ...
    }
}

// no-skills, dotnet-skills, managedcode: Traditional constructor
public class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<OwnerService> _logger;

    public OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger)
    {
        _db = db;
        _logger = logger;
    }
}
```

```csharp
// dotnet-artisan: Collection expressions in models
public ICollection<Pet> Pets { get; set; } = [];

// no-skills, dotnet-skills: Old-style initialization
public ICollection<Pet> Pets { get; set; } = new List<Pet>();
```

**Scores**: no-skills: **2** | dotnet-artisan: **5** | dotnet-skills: **2** | managedcode: **3** (collection expressions)

**Verdict**: dotnet-artisan comprehensively adopts C# 12+ features — primary constructors, records, collection expressions, init accessors, and tuple deconstruction. The other configs use traditional patterns. managedcode earns a slight edge for collection expression adoption.

---

### 10. Error Handling & Middleware [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 4 |
| 2 | 2 | 4 | 5 | 4 |
| 3 | 4 | 4 | 4 | 3 |
| **Mean** | **3.0** | **4.3** | **4.7** | **3.7** |

#### Analysis

| Config | Pattern | IExceptionHandler | ProblemDetails | Custom Exceptions |
|---|---|---|---|---|
| no-skills | IExceptionHandler | ✅ (2 handlers) | ✅ | BusinessRuleException, NotFoundException, ConflictException |
| dotnet-artisan | IExceptionHandler + tuple returns | ✅ (1 handler) | ✅ | None (uses tuple returns) |
| dotnet-skills | Convention middleware | ❌ | ✅ | BusinessRuleException, NotFoundException |
| managedcode | IExceptionHandler | ✅ (2 handlers, ordered) | ✅ | BusinessRuleException |

```csharp
// dotnet-artisan: IExceptionHandler with primary constructor
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        var problemDetails = new ProblemDetails { /* ... */ };
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}

// dotnet-skills: Convention-based middleware (older pattern)
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (BusinessRuleException ex) { await WriteErrorResponse(context, ...); }
        catch (NotFoundException ex) { await WriteErrorResponse(context, ...); }
        catch (Exception ex) { await WriteErrorResponse(context, ...); }
    }
}
```

```csharp
// managedcode: Properly ordered IExceptionHandler registration
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();  // specific first
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();        // catch-all second
builder.Services.AddProblemDetails();
```

**Scores**: no-skills: **4** | dotnet-artisan: **4** | dotnet-skills: **3** (convention middleware) | managedcode: **4**

**Verdict**: no-skills, dotnet-artisan, and managedcode all use the modern `IExceptionHandler` pattern. managedcode demonstrates best practice with explicit handler ordering. dotnet-skills falls behind by using convention-based middleware instead of the composable, DI-aware `IExceptionHandler`.

---

### 11. Async Patterns & Cancellation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 5 | 2 | 2 |
| 2 | 2 | 5 | 5 | 2 |
| 3 | 3 | 5 | 3 | 3 |
| **Mean** | **2.3** | **5.0** | **3.3** | **2.3** |

#### Analysis

| Config | Async/Await | Async Suffix | CancellationToken in Endpoints | CancellationToken in Services |
|---|---|---|---|---|
| no-skills | ✅ | ✅ | ❌ | ❌ |
| dotnet-artisan | ✅ | ✅ | ✅ | ✅ |
| dotnet-skills | ✅ | ✅ | ❌ | ❌ |
| managedcode | ✅ | ✅ | ❌ | ❌ |

```csharp
// dotnet-artisan: Full CancellationToken propagation
// Endpoint handler:
private static async Task<Ok<PagedResult<OwnerResponse>>> GetAll(
    IOwnerService service, [FromQuery] string? search,
    [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
    CancellationToken ct = default)
{
    var result = await service.GetAllAsync(search, page, pageSize, ct);
    return TypedResults.Ok(result);
}

// Service method:
public async Task<PagedResult<OwnerResponse>> GetAllAsync(
    string? search, int page, int pageSize, CancellationToken ct = default)
{
    var totalCount = await query.CountAsync(ct);
    var items = await query.Skip(...).Take(...).ToListAsync(ct);
    // ...
}
```

```csharp
// no-skills, dotnet-skills, managedcode: No CancellationToken
public async Task<IActionResult> GetAll([FromQuery] string? search, ...)
    => Ok(await _service.GetAllAsync(search, page, pageSize));
// Service: no ct parameter, EF calls without token
```

**Scores**: no-skills: **3** | dotnet-artisan: **5** | dotnet-skills: **3** | managedcode: **3**

**Verdict**: dotnet-artisan is the only configuration that propagates CancellationToken from endpoint handlers through services to EF Core calls. This prevents wasted database work on cancelled requests — critical for production scalability.

---

### 12. EF Core Best Practices [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 4 | 5 | 4 |
| 2 | 2 | 5 | 5 | 4 |
| 3 | 3 | 5 | 3 | 5 |
| **Mean** | **2.7** | **4.7** | **4.3** | **4.3** |

#### Analysis

| Config | AsNoTracking | Fluent API | Enum Conversion | Delete Behaviors | Timestamps Override |
|---|---|---|---|---|---|
| no-skills | ❌ | ✅ | ✅ `HasConversion<string>()` | ✅ Restrict | ✅ SaveChanges override |
| dotnet-artisan | ✅ | ✅ | ✅ `HasConversion<string>()` | ✅ Restrict | ✅ SaveChanges override |
| dotnet-skills | ✅ + AsSplitQuery | ✅ | ✅ `HasConversion<string>()` | ✅ Restrict | ✅ SaveChanges override |
| managedcode | ❌ | ✅ | ✅ `HasConversion<string>()` | ✅ Restrict | ✅ SaveChanges override |

```csharp
// dotnet-artisan, dotnet-skills: AsNoTracking on reads
var query = db.Owners.AsNoTracking().AsQueryable();

// no-skills, managedcode: Missing AsNoTracking — unnecessary tracking on read queries
var query = _db.Owners.AsQueryable();
```

```csharp
// dotnet-skills: AsSplitQuery for complex includes
var appointment = await _db.Appointments
    .AsNoTracking()
    .AsSplitQuery()
    .Include(a => a.Pet)
    .Include(a => a.Veterinarian)
    .Include(a => a.MedicalRecord)
        .ThenInclude(m => m.Prescriptions)
    .FirstOrDefaultAsync(a => a.Id == id);
```

All configs properly configure relationships via Fluent API:

```csharp
// Shared pattern: Restrict deletes, unique indexes, computed property exclusion
modelBuilder.Entity<Pet>(entity =>
{
    entity.HasIndex(e => e.MicrochipNumber).IsUnique()
          .HasFilter("MicrochipNumber IS NOT NULL");
    entity.HasOne(e => e.Owner).WithMany(o => o.Pets)
          .HasForeignKey(e => e.OwnerId)
          .OnDelete(DeleteBehavior.Restrict);
});
```

**Scores**: no-skills: **3** | dotnet-artisan: **5** | dotnet-skills: **5** (AsSplitQuery bonus) | managedcode: **3**

**Verdict**: dotnet-artisan and dotnet-skills tie. dotnet-skills adds `AsSplitQuery()` for complex includes (optimizing-ef-core-queries skill influence). no-skills and managedcode lack `AsNoTracking()`, doubling memory usage on read queries.

---

### 13. Service Abstraction & DI [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 |
| 2 | 4 | 5 | 4 | 4 |
| 3 | 4 | 5 | 4 | 5 |
| **Mean** | **4.3** | **5.0** | **4.3** | **4.7** |

#### Analysis

All configurations use `AddScoped<IService, Service>()` with proper interface segregation:

```csharp
// All configs: Interface-based DI registration
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IPetService, PetService>();
// ... 7 services total
```

| Config | Interface-Based | Sealed Services | Single Responsibility | Files per Service |
|---|---|---|---|---|
| no-skills | ✅ | ❌ | ✅ (7 services) | 2 (IService + Service) |
| dotnet-artisan | ✅ | ✅ | ✅ (7 services) | 2 (IService + Service) |
| dotnet-skills | ✅ | ✅ | ✅ (7 services) | 2 (IService + Service) |
| managedcode | ✅ | ❌ | ✅ (7 services) | Combined IServices.cs |

managedcode groups all interfaces into a single `IServices.cs` file, while others use one file per interface.

**Scores**: no-skills: **4** | dotnet-artisan: **5** (sealed) | dotnet-skills: **5** (sealed) | managedcode: **4**

**Verdict**: dotnet-artisan and dotnet-skills mark services as `sealed` for JIT devirtualization. All configs follow proper DI patterns with one service per domain entity.

---

### 14. Security Configuration [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 2 |
| 2 | 2 | 2 | 2 | 2 |
| 3 | 2 | 2 | 2 | 2 |
| **Mean** | **2.0** | **2.0** | **2.0** | **2.0** |

#### Analysis

**No configuration implements HSTS or HTTPS redirection.**

```csharp
// Expected but absent from ALL configs:
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
```

Additionally, dotnet-skills exposes Swagger unconditionally (not guarded by `IsDevelopment()`):

```csharp
// dotnet-skills: Swagger always exposed
app.UseSwagger();
app.UseSwaggerUI();  // ⚠️ Not behind IsDevelopment() check

// no-skills, managedcode: Properly guarded
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**Scores**: no-skills: **2** | dotnet-artisan: **2** | dotnet-skills: **2** | managedcode: **2**

**Verdict**: Universal gap. No config adds HSTS or HTTPS redirection. dotnet-skills has an additional issue with unconditional Swagger exposure.

---

### 15. DTO Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 5 | 3 | 4 |
| 2 | 2 | 5 | 4 | 2 |
| 3 | 2 | 5 | 2 | 2 |
| **Mean** | **2.7** | **5.0** | **3.0** | **2.7** |

#### Analysis

| Config | Type | Sealed | Naming | Immutability |
|---|---|---|---|---|
| no-skills | Classes | ❌ | `*Dto` | Mutable `{ get; set; }` |
| dotnet-artisan | Records | ✅ | `*Request`/`*Response` | Positional + `init` |
| dotnet-skills | Classes | ❌ | `*Dto` | Mutable `{ get; set; }` |
| managedcode | Classes | ❌ | `*Dto` / `*ResponseDto` | Mutable `{ get; set; }` |

```csharp
// dotnet-artisan: Sealed positional records (immutable responses)
public sealed record OwnerResponse(
    int Id, string FirstName, string LastName, string Email, string Phone,
    string? Address, string? City, string? State, string? ZipCode,
    DateTime CreatedAt, DateTime UpdatedAt,
    List<PetSummaryResponse>? Pets = null);

// dotnet-artisan: Sealed records with init for requests
public sealed record CreateOwnerRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;
}

// All others: Mutable class DTOs
public class OwnerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
}
```

**Scores**: no-skills: **2** | dotnet-artisan: **5** | dotnet-skills: **2** | managedcode: **2**

**Verdict**: dotnet-artisan demonstrates best-practice DTO design — sealed records with positional parameters for responses and `init` accessors for requests, producing immutable, type-safe API contracts.

---

### 16. Sealed Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 1 | 5 | 1 | 5 |
| 2 | 1 | 5 | 1 | 3 |
| 3 | 1 | 5 | 1 | 3 |
| **Mean** | **1.0** | **5.0** | **1.0** | **3.7** |

#### Analysis

| Config | Models Sealed | DTOs Sealed | Services Sealed | Total Sealed % |
|---|---|---|---|---|
| no-skills | 0/7 | 0/~20 | 0/7 | **0%** |
| dotnet-artisan | 7/7 | 24/24 | 7/7 | **~98%** |
| dotnet-skills | 0/7 | 0/~20 | 7/7 | **~16%** |
| managedcode | 0/7 | 0/~20 | 0/7 | **0%** |

```csharp
// dotnet-artisan: All types sealed
public sealed class Owner { /* ... */ }
public sealed class VetClinicDbContext(DbContextOptions<VetClinicDbContext> options)
    : DbContext(options) { /* ... */ }
public sealed record OwnerResponse(/* ... */);

// no-skills, managedcode: Nothing sealed
public class Owner { /* ... */ }
public class VetClinicDbContext : DbContext { /* ... */ }
```

**Scores**: no-skills: **1** | dotnet-artisan: **5** | dotnet-skills: **3** (services only) | managedcode: **1**

**Verdict**: dotnet-artisan applies `sealed` universally (41/42 types — only the enum excluded). This enables JIT devirtualization and signals explicit design intent. dotnet-skills seals only services (via the performance analyzer skill).

---

### 17. Data Seeder Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 |
| 2 | 4 | 5 | 4 | 4 |
| 3 | 4 | 4 | 4 | 4 |
| **Mean** | **4.0** | **4.3** | **4.0** | **4.0** |

#### Analysis

All configurations use a runtime seeder service called from `Program.cs` with idempotency guards:

```csharp
// Shared pattern across all configs
public static class DataSeeder
{
    public static async Task SeedAsync(VetClinicDbContext context)
    {
        if (context.Owners.Any()) return;  // Idempotency guard
        // 5 owners, 8 pets, 3 vets, appointments with varied statuses...
    }
}
```

All seed realistic, varied data covering different entity states (active/inactive pets, scheduled/completed/cancelled appointments, expired/due-soon vaccinations).

**Scores**: no-skills: **4** | dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **4**

**Verdict**: Tie. All configs produce comprehensive seed data with idempotency. None use `HasData()` in migrations (which would integrate with the migration pipeline).

---

### 18. Structured Logging [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 |
| 2 | 4 | 4 | 4 | 4 |
| 3 | 4 | 4 | 4 | 4 |
| **Mean** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

All configurations use `ILogger<T>` with structured message templates:

```csharp
// Shared pattern — all configs use named placeholders
_logger.LogInformation("Owner created: {OwnerId}", owner.Id);
_logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, status);
_logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
```

No config uses string interpolation for logging. None use `[LoggerMessage]` source generators.

**Scores**: no-skills: **4** | dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **4**

**Verdict**: Tie. All configs follow structured logging best practices with `ILogger<T>`. None reach 5 because none adopt the high-performance `[LoggerMessage]` source generator.

---

### 19. Nullable Reference Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 |
| 2 | 4 | 4 | 4 | 4 |
| 3 | 4 | 4 | 4 | 4 |
| **Mean** | **4.3** | **4.3** | **4.3** | **4.3** |

#### Analysis

All configurations enable NRT via `.csproj`:

```xml
<Nullable>enable</Nullable>
```

All properly annotate optional properties with `?` and use `= null!` for required navigation properties:

```csharp
// Shared pattern
public string? Address { get; set; }          // Optional field
public Owner Owner { get; set; } = null!;     // Required FK navigation
```

**Scores**: no-skills: **4** | dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **4**

**Verdict**: Tie. All configs properly enable and use NRT annotations.

---

### 20. API Documentation [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 4 |
| 2 | 3 | 5 | 3 | 3 |
| 3 | 3 | 5 | 3 | 3 |
| **Mean** | **3.3** | **5.0** | **3.3** | **3.3** |

#### Analysis

```csharp
// dotnet-artisan: Rich Minimal API metadata
group.MapGet("/", GetAll)
    .WithSummary("List all owners with search and pagination")
    .WithTags("Owners");
// TypedResults + Results<T1,T2> auto-generate OpenAPI schemas

// no-skills, dotnet-skills, managedcode: Controller attributes
/// <summary>List all owners with optional search and pagination</summary>
[HttpGet]
[ProducesResponseType(typeof(PagedResult<OwnerDto>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetAll(...)
```

dotnet-artisan's `Results<Created<T>, BadRequest<ProblemDetails>, Conflict<ProblemDetails>>` union types automatically generate precise OpenAPI response schemas without explicit `[ProducesResponseType]` annotations.

**Scores**: no-skills: **3** | dotnet-artisan: **5** | dotnet-skills: **3** | managedcode: **3**

**Verdict**: dotnet-artisan produces superior API documentation through TypedResults and union types that auto-generate accurate OpenAPI schemas, plus explicit `.WithSummary()` metadata.

---

### 21. File Organization [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 4 |
| 2 | 2 | 5 | 3 | 2 |
| 3 | 4 | 5 | 4 | 4 |
| **Mean** | **3.0** | **5.0** | **4.0** | **3.3** |

#### Analysis

```
// dotnet-artisan: Endpoints/ directory with extension methods
src/VetClinicApi/
├── Endpoints/          ← Clean endpoint separation
│   ├── OwnerEndpoints.cs
│   ├── PetEndpoints.cs
│   └── ...
├── Services/ (14 files: 7 interfaces + 7 implementations)
├── Models/ (8 files)
├── DTOs/ (8 files)
├── Data/ (VetClinicDbContext.cs, DataSeeder.cs)
├── Middleware/ (GlobalExceptionHandler.cs)
└── Program.cs

// All others: Controllers/ directory
src/VetClinicApi/
├── Controllers/        ← Traditional controller layout
│   ├── OwnersController.cs
│   └── ...
├── Services/
├── Models/
├── DTOs/
├── Data/
├── Middleware/
└── Program.cs
```

**Scores**: no-skills: **4** | dotnet-artisan: **5** | dotnet-skills: **4** | managedcode: **4**

**Verdict**: All have clean separation of concerns. dotnet-artisan's `Endpoints/` directory with extension methods keeps Program.cs cleaner (7 one-line calls vs inline controller registration).

---

### 22. HTTP Test File Quality [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 4 |
| 2 | 4 | 4 | 4 | 4 |
| 3 | 4 | 5 | 4 | 4 |
| **Mean** | **4.0** | **4.7** | **4.3** | **4.0** |

#### Analysis

All configurations include comprehensive `.http` files covering all CRUD operations, with FK-consistent payloads referencing seeded data IDs.

dotnet-artisan's `.http` file (327 lines) is the most thorough, explicitly testing:
- Full appointment status workflow transitions (Scheduled → CheckedIn → InProgress → Completed)
- Conflict detection scenarios
- Cancellation with and without reasons
- Delete owner with active pets (409 Conflict)
- Invalid status transitions

**Scores**: no-skills: **4** | dotnet-artisan: **5** | dotnet-skills: **4** | managedcode: **4**

**Verdict**: dotnet-artisan's HTTP test file is most comprehensive with explicit error case coverage. All others provide adequate coverage.

---

### 23. Type Design & Resource Management [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 4 |
| 2 | 3 | 4 | 4 | 4 |
| 3 | 3 | 4 | 3 | 3 |
| **Mean** | **3.3** | **4.3** | **3.7** | **3.7** |

#### Analysis

All configurations use `AppointmentStatus` enum instead of magic strings. All use `DateOnly` for date-only fields.

```csharp
// dotnet-artisan: Static readonly transition dictionary (optimized)
private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> s_validTransitions = new()
{
    [AppointmentStatus.Scheduled] = [AppointmentStatus.CheckedIn, ...],
    // ...
};

// no-skills: Creates new dictionary per call
var validTransitions = new Dictionary<AppointmentStatus, AppointmentStatus[]>
{
    { AppointmentStatus.Scheduled, new[] { AppointmentStatus.CheckedIn, ... } },
};
```

dotnet-artisan uses `IReadOnlyList<T>` for read-only returns and `static readonly` for immutable data.

**Scores**: no-skills: **3** | dotnet-artisan: **4** | dotnet-skills: **3** | managedcode: **3**

**Verdict**: dotnet-artisan demonstrates better type discipline with static readonly collections and more precise return types.

---

### 24. Code Standards Compliance [LOW × 0]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 5 |
| 2 | 3 | 5 | 4 | 3 |
| 3 | 4 | 5 | 4 | 4 |
| **Mean** | **3.7** | **5.0** | **4.0** | **4.0** |

#### Analysis

All configurations follow .NET naming conventions (PascalCase public members, camelCase parameters, explicit access modifiers, Async suffix).

dotnet-artisan additionally uses `s_` prefix for static fields (following .NET runtime coding guidelines):

```csharp
// dotnet-artisan: s_ prefix for static readonly
private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> s_validTransitions = ...;
```

**Scores**: no-skills: **4** | dotnet-artisan: **5** | dotnet-skills: **4** | managedcode: **4**

**Verdict**: All follow conventions well. dotnet-artisan earns the edge for runtime-style static field naming.

---

---

## Raw Data References

- Per-run analysis: `reports/analysis-run-1.md`
- Per-run analysis: `reports/analysis-run-2.md`
- Per-run analysis: `reports/analysis-run-3.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
