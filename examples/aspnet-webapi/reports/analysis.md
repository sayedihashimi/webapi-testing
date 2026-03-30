# Aggregated Analysis: ASP.NET Core Web API Skill Evaluation

**Runs:** 3 | **Configurations:** 4 | **Scenarios:** 3 | **Dimensions:** 26
**Date:** 2026-03-30 09:10 UTC

---

## Overview

Evaluate how custom Copilot skills impact the quality of generated ASP.NET Core Web API code across three realistic application scenarios.

---

## What Was Tested

### Scenarios

Each run generates one of the following application scenarios (randomly selected per run):

| Scenario | Description |
|---|---|
| FitnessStudioApi | Booking/membership system with class scheduling, waitlists, and instructor management |
| LibraryApi | Book loans, reservations, overdue fines, and availability tracking |
| VetClinicApi | Pet healthcare with appointments, vaccinations, and medical records |

### Configurations

Each configuration gives Copilot different custom skills or plugins. The **no-skills** baseline uses default Copilot with no custom instructions.

| Configuration | Description | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | — | — |
| dotnet-artisan | dotnet-artisan plugin chain | — | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | — |
| dotnet-skills | Official .NET Skills (dotnet/skills) | — | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |

### How It Works

1. **Generate** — For each configuration, Copilot CLI (`copilot --yolo`) is given a scenario prompt and generates a complete project from scratch. One scenario is randomly selected per run.
2. **Verify** — Each generated project is built (`dotnet build`), run, format-checked, and scanned for NuGet vulnerabilities.
3. **Analyze** — An AI judge reviews the source code of all configurations side-by-side and scores each across 26 quality dimensions.

Generation model: **claude-opus-4.6-1m**
Analysis model: **gpt-5.3-codex**

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
| Scenario Coverage [MEDIUM] | 2.0 | 2.0 | 2.0 | 2.0 |
| Build & Run Success [CRITICAL] | 4.0 ± 1.7 | 4.0 ± 1.7 | 4.0 ± 1.7 | 4.0 ± 1.7 |
| Security Vulnerability Scan [CRITICAL] | 4.0 ± 1.7 | 4.0 ± 1.7 | 4.0 ± 1.7 | 4.0 ± 1.7 |
| Minimal API Architecture [CRITICAL] | 1.3 ± 0.6 | 5.0 | 1.7 ± 0.6 | 1.7 ± 0.6 |
| Input Validation & Guard Clauses [CRITICAL] | 3.3 ± 0.6 | 3.3 ± 0.6 | 3.7 ± 0.6 | 4.0 |
| NuGet & Package Discipline [CRITICAL] | 2.7 ± 1.5 | 3.3 ± 1.2 | 1.7 ± 0.6 | 2.3 ± 1.5 |
| EF Migration Usage [CRITICAL] | 1.0 | 1.0 | 1.0 | 1.0 |
| Business Logic Correctness [HIGH] | 2.3 ± 0.6 | 2.7 ± 1.2 | 2.7 ± 1.2 | 2.7 ± 1.2 |
| Prefer Built-in over 3rd Party [HIGH] | 1.7 ± 0.6 | 3.0 | 2.0 | 2.0 |
| Modern C# Adoption [HIGH] | 2.7 ± 0.6 | 5.0 | 4.0 | 3.7 ± 0.6 |
| Error Handling & Middleware [HIGH] | 3.0 | 4.0 | 3.7 ± 0.6 | 3.7 ± 0.6 |
| Async Patterns & Cancellation [HIGH] | 1.7 ± 0.6 | 3.7 ± 1.2 | 2.7 ± 0.6 | 2.7 ± 0.6 |
| EF Core Best Practices [HIGH] | 2.0 | 4.3 ± 0.6 | 4.0 | 4.0 |
| Service Abstraction & DI [HIGH] | 4.0 | 4.7 ± 0.6 | 4.3 ± 0.6 | 4.3 ± 0.6 |
| Security Configuration [HIGH] | 1.0 | 1.3 ± 0.6 | 2.3 ± 1.2 | 1.0 |
| DTO Design [MEDIUM] | 2.7 ± 0.6 | 5.0 | 3.0 ± 1.0 | 3.7 ± 0.6 |
| Sealed Types [MEDIUM] | 1.3 ± 0.6 | 5.0 | 2.0 ± 1.0 | 3.3 ± 1.5 |
| Data Seeder Design [MEDIUM] | 3.3 ± 1.2 | 4.0 | 4.0 | 4.0 |
| Structured Logging [MEDIUM] | 3.7 ± 0.6 | 4.3 ± 0.6 | 4.0 | 4.0 |
| Nullable Reference Types [MEDIUM] | 4.0 | 4.0 | 4.0 | 4.0 |
| API Documentation [MEDIUM] | 3.3 ± 0.6 | 5.0 | 3.7 ± 0.6 | 3.7 ± 0.6 |
| File Organization [MEDIUM] | 3.7 ± 0.6 | 5.0 | 4.0 | 4.0 |
| HTTP Test File Quality [MEDIUM] | 3.3 ± 0.6 | 4.3 ± 0.6 | 3.7 ± 0.6 | 3.7 ± 0.6 |
| Type Design & Resource Management [MEDIUM] | 3.0 | 4.0 ± 1.0 | 3.7 ± 0.6 | 3.7 ± 0.6 |
| Code Standards Compliance [LOW] | 3.7 ± 0.6 | 5.0 | 3.7 ± 0.6 | 4.0 |
| Scenario Coverage Across Apps [MEDIUM] | 2.0 | 2.0 | 2.0 | 2.0 |

---

## Final Rankings

| Rank | Configuration | Mean Score | % of Max (217.5) | Std Dev | Min | Max |
|---|---|---|---|---|---|---|
| 🥇 | dotnet-artisan | 163.8 | 75% | 16.8 | 144.5 | 174.5 |
| 🥈 | dotnet-skills | 136.3 | 63% | 14.6 | 120.0 | 148.0 |
| 🥉 | managedcode-dotnet-skills | 134.5 | 62% | 10.2 | 123.0 | 142.5 |
| 4th | no-skills | 117.2 | 54% | 9.3 | 108.0 | 126.5 |

---

## Weighted Score per Run

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 108.0 | 144.5 | 123.0 | 120.0 |
| 2 | 126.5 | 174.5 | 142.5 | 148.0 |
| 3 | 117.0 | 172.5 | 138.0 | 141.0 |
| **Mean** | **117.2** | **163.8** | **134.5** | **136.3** |

---

## Verification Summary (All Runs)

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |
|---|---|---|---|
| no-skills | 3/3 (100%) | 3/3 (100%) | 114.7 |
| dotnet-artisan | 3/3 (100%) | 3/3 (100%) | 88.7 |
| managedcode-dotnet-skills | 3/3 (100%) | 3/3 (100%) | 129.3 |
| dotnet-skills | 3/3 (100%) | 3/3 (100%) | 118.0 |

---

## Consistency Analysis

| Configuration | Score σ | Most Consistent Dim (σ) | Most Variable Dim (σ) |
|---|---|---|---|
| no-skills | 9.3 | Scenario Coverage (0.0) | Build & Run Success (1.7) |
| dotnet-artisan | 16.8 | Scenario Coverage (0.0) | Build & Run Success (1.7) |
| managedcode-dotnet-skills | 10.2 | Scenario Coverage (0.0) | Build & Run Success (1.7) |
| dotnet-skills | 14.6 | Scenario Coverage (0.0) | Build & Run Success (1.7) |

---

## Per-Dimension Analysis

### 1. Scenario Coverage [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 2 |
| 2 | — | — | — | — |
| 3 | — | — | — | — |
| **Mean** | **2.0** | **2.0** | **2.0** | **2.0** |

### 2. Build & Run Success [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 2 |
| 2 | 5 | 5 | 5 | 5 |
| 3 | 5 | 5 | 5 | 5 |
| **Mean** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

All 4 available configs built successfully (`Build succeeded`) and ran long enough for a smoke window (~12s).

```bash
# all available run-3 projects
BuildSucceeded=true, Warnings=0, Errors=0, RunStarted=true, RunStayedAlive12s=true
```



### 3. Security Vulnerability Scan [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 2 |
| 2 | 5 | 5 | 5 | 5 |
| 3 | 5 | 5 | 5 | 5 |
| **Mean** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

`dotnet list package --vulnerable --include-transitive` reported no vulnerable packages for all present projects.


**Verdict:** Tie on known vulnerability surface for available outputs.

### 4. Minimal API Architecture [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 1 | 5 | 2 | 2 |
| 2 | 2 | 5 | 2 | 2 |
| 3 | 1 | 5 | 1 | 1 |
| **Mean** | **1.3** | **5.0** | **1.7** | **1.7** |

#### Analysis

Only `dotnet-artisan` uses modern Minimal API route groups + TypedResults.

```csharp
// dotnet-artisan: Endpoints/OwnerEndpoints.cs
var group = routes.MapGroup("/api/owners").WithTags("Owners");
group.MapGet("/{id:int}", async Task<Results<Ok<OwnerDetailResponse>, NotFound>> (...) => ...
    ? TypedResults.Ok(owner)
    : TypedResults.NotFound());
```

```csharp
// no-skills / managedcode / dotnet-skills: Controllers
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
```


**Verdict:** `dotnet-artisan` clearly best; it matches current .NET Minimal API guidance.

### 5. Input Validation & Guard Clauses [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 3 | 4 | 4 |
| 2 | 3 | 3 | 3 | 4 |
| 3 | 3 | 4 | 4 | 4 |
| **Mean** | **3.3** | **3.3** | **3.7** | **4.0** |

#### Analysis

All available versions validate request models; style differs.

```csharp
// dotnet-artisan DTOs (DataAnnotations)
[Required, MaxLength(500)]
public required string Reason { get; init; }
[Range(15, 120)]
public int DurationMinutes { get; init; } = 30;
```

```csharp
// managedcode / dotnet-skills Validators.cs
public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    RuleFor(x => x.AppointmentDate).GreaterThan(DateTime.UtcNow);
}
```

Guard-clause primitives (`ArgumentNullException.ThrowIfNull`) are generally absent.


**Verdict:** FluentValidation variants and artisan are stronger than baseline, but all miss consistent guard-API usage.

### 6. NuGet & Package Discipline [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 2 | 1 | 1 |
| 2 | 3 | 4 | 2 | 2 |
| 3 | 1 | 4 | 2 | 4 |
| **Mean** | **2.7** | **3.3** | **1.7** | **2.3** |

#### Analysis

`no-skills` and `managedcode` use floating versions; others mostly pin exact versions.

```xml
<!-- no-skills -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.*-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

```xml
<!-- dotnet-artisan -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```


**Verdict:** `dotnet-artisan` and `dotnet-skills` are materially better on reproducibility.

### 7. EF Migration Usage [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 1 | 1 | 1 | 1 |
| 2 | 1 | 1 | 1 | 1 |
| 3 | 1 | 1 | 1 | 1 |
| **Mean** | **1.0** | **1.0** | **1.0** | **1.0** |

#### Analysis

All present configurations use `EnsureCreated(Async)` instead of migrations.

```csharp
// all available configs, Program.cs pattern
await context.Database.EnsureCreatedAsync();
```


**Verdict:** Universal critical weakness; production-safe migration workflow is missing.

### 8. Business Logic Correctness [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 2 |
| 2 | 3 | 4 | 4 | 4 |
| 3 | 2 | 2 | 2 | 2 |
| **Mean** | **2.3** | **2.7** | **2.7** | **2.7** |

#### Analysis

Within `VetClinicApi`, business logic is substantial (status transitions, scheduling conflicts, cancellation constraints). But expected run-3 app set is incomplete (missing `FitnessStudioApi` and `LibraryApi` in every available config).

```csharp
// dotnet-artisan AppointmentService
if (!_validTransitions.TryGetValue(appointment.Status, out var validNext) || !validNext.Contains(newStatus))
    throw new InvalidOperationException(...);
```


**Verdict:** Functional depth exists in VetClinic, but scenario completeness penalty dominates.

### 9. Prefer Built-in over 3rd Party [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 1 | 3 | 2 | 2 |
| 2 | 2 | 3 | 2 | 2 |
| 3 | 2 | 3 | 2 | 2 |
| **Mean** | **1.7** | **3.0** | **2.0** | **2.0** |

#### Analysis

All available versions include Swashbuckle; two also include FluentValidation packages.

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
```


**Verdict:** `dotnet-artisan` slightly better package minimalism, but none fully prefer built-ins.

### 10. Modern C# Adoption [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 5 | 4 | 4 |
| 2 | 2 | 5 | 4 | 4 |
| 3 | 3 | 5 | 4 | 3 |
| **Mean** | **2.7** | **5.0** | **4.0** | **3.7** |

#### Analysis

`dotnet-artisan` uses primary constructors, sealed records, collection expressions broadly.

```csharp
// dotnet-artisan
public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> _validTransitions = new() { ... };
```


**Verdict:** `dotnet-artisan` is strongest and most idiomatic for current C#.

### 11. Error Handling & Middleware [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 4 | 3 | 4 |
| 2 | 3 | 4 | 4 | 3 |
| 3 | 3 | 4 | 4 | 4 |
| **Mean** | **3.0** | **4.0** | **3.7** | **3.7** |

#### Analysis

`dotnet-artisan`, `managedcode`, and `dotnet-skills` use `IExceptionHandler` + ProblemDetails; `no-skills` uses custom middleware.

```csharp
// managedcode Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
```


**Verdict:** The `IExceptionHandler` implementations are preferable to manual middleware.

### 12. Async Patterns & Cancellation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 1 | 5 | 2 | 2 |
| 2 | 2 | 3 | 3 | 3 |
| 3 | 2 | 3 | 3 | 3 |
| **Mean** | **1.7** | **3.7** | **2.7** | **2.7** |

#### Analysis

Async is broadly correct (`Task`, `await`, no `async void`), but cancellation token propagation is sparse.


**Verdict:** Good async baseline, incomplete cancellation flow through layers.

### 13. EF Core Best Practices [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 2 | 4 | 4 | 4 |
| 2 | 2 | 4 | 4 | 4 |
| 3 | 2 | 5 | 4 | 4 |
| **Mean** | **2.0** | **4.3** | **4.0** | **4.0** |

#### Analysis

`dotnet-artisan` is strongest: frequent `AsNoTracking()`, includes, and clean service query patterns.

```csharp
// dotnet-artisan
var query = db.Appointments
    .AsNoTracking()
    .Include(a => a.Pet)
    .Include(a => a.Veterinarian);
```


**Verdict:** `dotnet-artisan` best aligns with performant EF read patterns.

### 14. Service Abstraction & DI [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 |
| 2 | 4 | 5 | 5 | 5 |
| 3 | 4 | 5 | 4 | 4 |
| **Mean** | **4.0** | **4.7** | **4.3** | **4.3** |

#### Analysis

All available outputs use interface-based service registration and scoped DI.

```csharp
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
```


**Verdict:** Strong across available configs; artisan is most consistent and cleanly organized.

### 15. Security Configuration [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 1 | 1 | 3 | 1 |
| 2 | 1 | 1 | 1 | 1 |
| 3 | 1 | 2 | 3 | 1 |
| **Mean** | **1.0** | **1.3** | **2.3** | **1.0** |

#### Analysis

`managedcode` is best of available set (uses HTTPS redirection). None use non-dev HSTS pattern.

```csharp
// managedcode Program.cs
app.UseHttpsRedirection();
```


**Verdict:** Security middleware is incomplete in all generated variants.

### 16. DTO Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 5 | 4 | 4 |
| 2 | 2 | 5 | 2 | 4 |
| 3 | 3 | 5 | 3 | 3 |
| **Mean** | **2.7** | **5.0** | **3.0** | **3.7** |

#### Analysis

`dotnet-artisan` uses immutable sealed records extensively; others mix mutable classes/records.

```csharp
// dotnet-artisan
public sealed record CreateAppointmentRequest { public required int PetId { get; init; } }
```

```csharp
// managedcode
public sealed class AppointmentCreateDto { public int PetId { get; set; } }
```


**Verdict:** `dotnet-artisan` has best API contract immutability and intent.

### 17. Sealed Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 1 | 5 | 3 | 2 |
| 2 | 1 | 5 | 1 | 3 |
| 3 | 2 | 5 | 2 | 5 |
| **Mean** | **1.3** | **5.0** | **2.0** | **3.3** |

#### Analysis

`dotnet-skills` and `dotnet-artisan` heavily use sealing; others are mixed.


**Verdict:** `dotnet-artisan` and `dotnet-skills` best convey non-inheritance intent/perf posture.

### 18. Data Seeder Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 |
| 2 | 2 | 4 | 4 | 4 |
| 3 | 4 | 4 | 4 | 4 |
| **Mean** | **3.3** | **4.0** | **4.0** | **4.0** |

#### Analysis

All available configs include seed data and startup seeding paths.

```csharp
await DataSeeder.SeedAsync(context);
```


**Verdict:** Similar quality across available outputs.

### 19. Structured Logging [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 |
| 2 | 4 | 5 | 4 | 4 |
| 3 | 3 | 4 | 4 | 4 |
| **Mean** | **3.7** | **4.3** | **4.0** | **4.0** |

#### Analysis

All available outputs use `ILogger<T>` and named placeholders.

```csharp
logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId}", appointment.Id, appointment.PetId);
```


**Verdict:** Good baseline; artisan/managed/dotnet-skills are more consistently structured.

### 20. Nullable Reference Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 |
| 2 | 4 | 4 | 4 | 4 |
| 3 | 4 | 4 | 4 | 4 |
| **Mean** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

All available `.csproj` files enable nullable context.

```xml
<Nullable>enable</Nullable>
```


**Verdict:** Strong consistency among available projects.

### 21. API Documentation [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 5 | 4 | 4 |
| 2 | 4 | 5 | 4 | 4 |
| 3 | 3 | 5 | 3 | 3 |
| **Mean** | **3.3** | **5.0** | **3.7** | **3.7** |

#### Analysis

`dotnet-artisan` is strongest due endpoint-level summaries/tags with Minimal APIs.

```csharp
// dotnet-artisan
group.MapGet("/", ...).WithSummary("List all owners with optional search and pagination");
```


**Verdict:** `dotnet-artisan` provides richest OpenAPI metadata shape.

### 22. File Organization [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 5 | 4 | 4 |
| 2 | 4 | 5 | 4 | 4 |
| 3 | 4 | 5 | 4 | 4 |
| **Mean** | **3.7** | **5.0** | **4.0** | **4.0** |

#### Analysis

All available projects are organized by concern; `dotnet-artisan` is notably clean with endpoint extensions.


**Verdict:** `dotnet-artisan` best separation in Program.cs + endpoint modules.

### 23. HTTP Test File Quality [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 |
| 2 | 4 | 5 | 4 | 4 |
| 3 | 3 | 4 | 3 | 3 |
| **Mean** | **3.3** | **4.3** | **3.7** | **3.7** |

#### Analysis

All available `.http` files are substantial and cover many happy paths; explicit negative-path tests are sparse.


**Verdict:** `dotnet-artisan` has strongest practical API walkthrough, but still mostly success-path heavy.

### 24. Type Design & Resource Management [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 3 | 3 | 4 | 4 |
| 2 | 3 | 5 | 4 | 4 |
| 3 | 3 | 4 | 3 | 3 |
| **Mean** | **3.0** | **4.0** | **3.7** | **3.7** |

#### Analysis

Enums and domain types are generally good; precision choices vary (`List<T>` vs `IReadOnlyList<T>`), and cancellation/resource flow can be tighter.


**Verdict:** `dotnet-artisan` leads on type precision and modern signatures.

### 25. Code Standards Compliance [LOW × 0]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 4 |
| 2 | 3 | 5 | 3 | 4 |
| 3 | 4 | 5 | 4 | 4 |
| **Mean** | **3.7** | **5.0** | **3.7** | **4.0** |

#### Analysis

Naming and access patterns are generally compliant across available outputs; `dotnet-artisan` is most consistent.


**Verdict:** All available variants are readable and convention-friendly; artisan is most polished.

### 26. Scenario Coverage Across Apps [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|
| 1 | — | — | — | — |
| 2 | 2 | 2 | 2 | 2 |
| 3 | — | — | — | — |
| **Mean** | **2.0** | **2.0** | **2.0** | **2.0** |

---

## Asset Usage Summary

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | 0379df85…2d34 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 2 | 69d851ec…050d | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 3 | 2de28baf…57aa | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-artisan | 1 | 4fb2147b…5391 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| dotnet-artisan | 2 | 71bade27…0a8f | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-tooling | dotnet-artisan | ✅ |
| dotnet-artisan | 3 | 19f2d38a…036c | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-tooling | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | 4e90ce72…ca1f | claude-opus-4.6-1m | dotnet, dotnet-project-setup, dotnet-entity-framework-core, dotnet-minimal-apis, dotnet-aspnet-core, dotnet-microsoft-extensions, dotnet-modern-csharp | — | ✅ |
| managedcode-dotnet-skills | 2 | 0e8a5b3f…78c0 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-project-setup, dotnet-microsoft-extensions, dotnet-entity-framework-core, dotnet-modern-csharp | — | ✅ |
| managedcode-dotnet-skills | 3 | 04ba91ac…bb25 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-project-setup, dotnet-modern-csharp, dotnet-minimal-apis, dotnet-microsoft-extensions | — | ✅ |
| dotnet-skills | 1 | 5756516f…f454 | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 2 | aeafeccc…f55c | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 3 | 726c8812…7ed7 | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |

---

## Raw Data References

- Per-run analysis: `reports/analysis-run-1.md`
- Per-run analysis: `reports/analysis-run-2.md`
- Per-run analysis: `reports/analysis-run-3.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
