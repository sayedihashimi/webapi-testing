# Aggregated Analysis: ASP.NET Core Web API Skill Evaluation

**Runs:** 3 | **Configurations:** 5 | **Scenarios:** 3 | **Dimensions:** 25
**Date:** 2026-03-29 22:04 UTC

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 |
| Security Vulnerability Scan [CRITICAL] | 4.3 ± 1.2 | 4.3 ± 1.2 | 4.3 ± 1.2 | 4.3 ± 1.2 | 4.3 ± 1.2 |
| Minimal API Architecture [CRITICAL] | 1.3 ± 0.6 | 5.0 | 4.7 ± 0.6 | 1.3 ± 0.6 | 1.3 ± 0.6 |
| Input Validation & Guard Clauses [CRITICAL] | 3.3 ± 0.6 | 4.0 ± 1.0 | 4.0 ± 1.0 | 4.0 ± 1.0 | 4.0 |
| NuGet & Package Discipline [CRITICAL] | 3.7 ± 1.5 | 4.0 ± 1.0 | 3.0 ± 1.0 | 3.7 ± 1.2 | 2.7 ± 1.2 |
| EF Migration Usage [CRITICAL] | 1.3 ± 0.6 | 3.7 ± 2.3 | 1.3 ± 0.6 | 1.3 ± 0.6 | 1.3 ± 0.6 |
| Business Logic Correctness [HIGH] | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 |
| Prefer Built-in over 3rd Party [HIGH] | 2.7 ± 1.2 | 5.0 | 3.0 | 2.7 ± 0.6 | 2.3 ± 0.6 |
| Modern C# Adoption [HIGH] | 2.7 ± 0.6 | 5.0 | 5.0 | 4.0 | 3.0 ± 1.0 |
| Error Handling & Middleware [HIGH] | 4.0 ± 1.0 | 5.0 | 4.3 ± 1.2 | 4.7 ± 0.6 | 4.3 ± 0.6 |
| Async Patterns & Cancellation [HIGH] | 2.7 ± 0.6 | 5.0 | 5.0 | 2.7 ± 0.6 | 2.7 ± 0.6 |
| EF Core Best Practices [HIGH] | 3.3 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.3 ± 0.6 | 4.7 ± 0.6 |
| Service Abstraction & DI [HIGH] | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 |
| Security Configuration [HIGH] | 1.7 ± 1.2 | 1.7 ± 1.2 | 1.7 ± 1.2 | 2.3 ± 1.2 | 1.7 ± 1.2 |
| DTO Design [MEDIUM] | 3.0 ± 1.7 | 5.0 | 4.3 ± 1.2 | 4.3 ± 0.6 | 3.3 ± 1.5 |
| Sealed Types [MEDIUM] | 1.3 ± 0.6 | 5.0 | 5.0 | 1.7 ± 0.6 | 3.3 ± 1.5 |
| Data Seeder Design [MEDIUM] | 4.0 ± 1.0 | 4.7 ± 0.6 | 4.0 ± 1.0 | 4.0 ± 1.0 | 4.0 ± 1.0 |
| Structured Logging [MEDIUM] | 4.3 ± 0.6 | 4.7 ± 0.6 | 4.3 ± 0.6 | 4.3 ± 0.6 | 4.0 ± 1.0 |
| Nullable Reference Types [MEDIUM] | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 |
| API Documentation [MEDIUM] | 3.0 | 5.0 | 4.3 ± 0.6 | 3.0 | 3.7 ± 0.6 |
| File Organization [MEDIUM] | 3.3 ± 0.6 | 5.0 | 5.0 | 4.3 ± 0.6 | 4.0 ± 1.0 |
| HTTP Test File Quality [MEDIUM] | 4.3 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 4.3 ± 0.6 | 4.3 ± 0.6 |
| Type Design & Resource Management [MEDIUM] | 5.0 | 5.0 | 5.0 | 5.0 | 5.0 |
| Code Standards Compliance [LOW] | 3.7 ± 1.2 | 5.0 | 5.0 | 4.0 | 3.7 ± 1.2 |
| Type Design & Resource Mgmt [MEDIUM] | 3.5 ± 0.7 | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.0 | 3.5 ± 0.7 |

---

## Final Rankings

| Rank | Configuration | Mean Weighted Score | Std Dev | Min | Max |
|---|---|---|---|---|---|
| 🥇 | dotnet-webapi | 194.2 | 13.0 | 181.5 | 207.5 |
| 🥈 | dotnet-artisan | 175.5 | 22.1 | 152.5 | 196.5 |
| 🥉 | managedcode-dotnet-skills | 155.0 | 20.5 | 134.0 | 175.0 |
| 4th | dotnet-skills | 148.2 | 25.1 | 124.5 | 174.5 |
| 5th | no-skills | 142.5 | 29.1 | 112.5 | 170.5 |

---

## Weighted Score per Run

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 170.5 | 207.5 | 196.5 | 175.0 | 174.5 |
| 2 | 144.5 | 193.5 | 177.5 | 156.0 | 145.5 |
| 3 | 112.5 | 181.5 | 152.5 | 134.0 | 124.5 |
| **Mean** | **142.5** | **194.2** | **175.5** | **155.0** | **148.2** |

---

## Verification Summary (All Runs)

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |
|---|---|---|---|
| no-skills | 3/9 (33%) | 3/9 (33%) | 41.1 |
| dotnet-webapi | 3/9 (33%) | 3/9 (33%) | 50.2 |
| dotnet-artisan | 3/9 (33%) | 3/9 (33%) | 36.0 |
| managedcode-dotnet-skills | 3/9 (33%) | 3/9 (33%) | 41.1 |
| dotnet-skills | 3/9 (33%) | 3/9 (33%) | 30.2 |

---

## Asset Usage Summary

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | 779cecba…14c0 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 2 | 0a4d4a83…0ad1 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 3 | 235f8ac0…1fd3 | None | — | — | ✅ |
| dotnet-webapi | 1 | 000aa748…d9fb | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-webapi | 2 | e407e0bc…d6d4 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-webapi | 3 | 45d42b13…7009 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-artisan | 1 | 67413dba…a58f | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api | dotnet-artisan | ✅ |
| dotnet-artisan | 2 | ec0cf5cb…0537 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api | dotnet-artisan | ✅ |
| dotnet-artisan | 3 | dada0431…66f6 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | 428c6d5a…aa04 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-project-setup | — | ✅ |
| managedcode-dotnet-skills | 3 | 2f9a449e…13da | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-project-setup, dotnet-modern-csharp, dotnet-entity-framework-core, dotnet-modern-csharp | — | ✅ |
| dotnet-skills | 1 | 08a1d1d7…2f57 | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 2 | 9583f8dd…150a | claude-opus-4.6-1m | optimizing-ef-core-queries, analyzing-dotnet-performance | dotnet-data, dotnet-diag | ✅ |
| dotnet-skills | 3 | f1af7ace…75c3 | claude-opus-4.6-1m | optimizing-ef-core-queries, analyzing-dotnet-performance | dotnet-data, dotnet-diag | ✅ |

---

## Consistency Analysis

| Configuration | Score σ | Most Consistent Dim (σ) | Most Variable Dim (σ) |
|---|---|---|---|
| no-skills | 29.1 | API Documentation (0.0) | DTO Design (1.7) |
| dotnet-webapi | 13.0 | Minimal API Architecture (0.0) | EF Migration Usage (2.3) |
| dotnet-artisan | 22.1 | Prefer Built-in over 3rd Party (0.0) | Security Vulnerability Scan (1.2) |
| managedcode-dotnet-skills | 20.5 | Modern C# Adoption (0.0) | Security Vulnerability Scan (1.2) |
| dotnet-skills | 25.1 | Input Validation & Guard Clauses (0.0) | DTO Design (1.5) |

---

## Per-Dimension Analysis

### 1. Build & Run Success [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| 3 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.7** | **4.7** | **4.7** | **4.7** | **4.7** |

#### Analysis

All five configurations produce projects that compile successfully with **zero errors and zero warnings**.

| Configuration | Build Result | Warnings | Errors |
|---|---|---|---|
| dotnet-artisan | ✅ Succeeded | 0 | 0 |
| dotnet-webapi | ✅ Succeeded | 0 | 0 |
| managedcode | ✅ Succeeded | 0 | 0 |
| dotnet-skills | ✅ Succeeded | 0 | 0 |
| no-skills | ✅ Succeeded | 0 | 0 |

**Scores:** dotnet-artisan: **5** | dotnet-webapi: **5** | managedcode: **5** | dotnet-skills: **5** | no-skills: **5**

**Verdict:** All configurations produce clean, building projects. This is the baseline expectation.

---

### 2. Security Vulnerability Scan [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| 3 | 3 | 3 | 3 | 3 | 3 |
| **Mean** | **4.3** | **4.3** | **4.3** | **4.3** | **4.3** |

#### Analysis

`dotnet list package --vulnerable` reports **no vulnerable packages** across all configurations.

| Configuration | Vulnerable Packages |
|---|---|
| dotnet-artisan | None |
| dotnet-webapi | None |
| managedcode | None |
| dotnet-skills | None |
| no-skills | None |

**Scores:** dotnet-artisan: **5** | dotnet-webapi: **5** | managedcode: **5** | dotnet-skills: **5** | no-skills: **5**

**Verdict:** All configurations are clean. Note that wildcard versions (dotnet-artisan, dotnet-skills) may pull in vulnerable packages in the future — see NuGet Package Discipline for version pinning analysis.

---

### 3. Minimal API Architecture [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 2 | 2 |
| 2 | 1 | 5 | 5 | 1 | 1 |
| 3 | 1 | 5 | 4 | 1 | 1 |
| **Mean** | **1.3** | **5.0** | **4.7** | **1.3** | **1.3** |

#### Analysis

This dimension evaluates whether the project uses Minimal APIs with route groups, TypedResults, and endpoint extension methods — the modern .NET standard.

### dotnet-artisan (Score: 5)
Uses **Minimal APIs** with full best-practice patterns:

```csharp
// Endpoints/AuthorEndpoints.cs — dotnet-artisan
public static RouteGroupBuilder MapAuthorEndpoints(this IEndpointRouteBuilder routes)
{
    var group = routes.MapGroup("/api/authors").WithTags("Authors");

    group.MapGet("/{id:int}", async Task<Results<Ok<AuthorDetailResponse>, NotFound>>
        (int id, IAuthorService service, CancellationToken ct) =>
    {
        var author = await service.GetAuthorByIdAsync(id, ct);
        return author is not null ? TypedResults.Ok(author) : TypedResults.NotFound();
    })
    .WithName("GetAuthor")
    .WithSummary("Get author details including their books");
    // ...
}
```

- ✅ `MapGroup()` with `WithTags()`
- ✅ Endpoint extension methods (`app.MapAuthorEndpoints()`)
- ✅ `TypedResults.Ok()`, `TypedResults.Created()`, `TypedResults.NotFound()`
- ✅ `Results<Ok<T>, NotFound>` union return types

### dotnet-webapi (Score: 5)
Identical pattern to dotnet-artisan:

```csharp
// Endpoints/AuthorEndpoints.cs — dotnet-webapi
public static void MapAuthorEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/authors").WithTags("Authors");

    group.MapGet("/", async Task<Ok<PaginatedResponse<AuthorListResponse>>> (
        string? search, int? page, int? pageSize,
        IAuthorService service, CancellationToken ct) =>
    {
        var result = await service.GetAllAsync(search, page ?? 1, Math.Clamp(pageSize ?? 10, 1, 100), ct);
        return TypedResults.Ok(result);
    })
    .WithName("GetAuthors")
    .WithSummary("List authors")
    .WithDescription("List authors with optional search by name and pagination.");
}
```

- ✅ All the same patterns as dotnet-artisan
- ✅ Additionally includes `Math.Clamp()` for input clamping

### managedcode-dotnet-skills (Score: 1)
Uses **Controllers** — the legacy pattern:

```csharp
// Controllers/AuthorsController.cs — managedcode
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthorsController(IAuthorService authorService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, ...)
        => Ok(await authorService.GetAllAsync(search, page, pageSize));
}
```

- ❌ `[ApiController]` controller pattern
- ❌ No `MapGroup()`, no `TypedResults`, no union return types
- ❌ Returns `IActionResult` (no compile-time type safety)

### dotnet-skills (Score: 1) and no-skills (Score: 1)
Both use the same **Controller** pattern as managedcode:

```csharp
// Controllers/AuthorsController.cs — dotnet-skills
[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _service;
    public AuthorsController(IAuthorService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAuthors([FromQuery] string? search, ...)
        => Ok(await _service.GetAuthorsAsync(search, page, pageSize));
}
```

**Verdict:** Only **dotnet-artisan** and **dotnet-webapi** use modern Minimal APIs. The other three fall back to the legacy controller pattern. Minimal APIs with `TypedResults` and `Results<>` union types provide compile-time safety, lower overhead, and automatic OpenAPI schema generation.

---

### 4. Input Validation & Guard Clauses [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 4 | 4 |
| 2 | 3 | 4 | 4 | 5 | 4 |
| 3 | 3 | 3 | 3 | 3 | 4 |
| **Mean** | **3.3** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

### managedcode-dotnet-skills (Score: 5)
The strongest validation — uses **FluentValidation** with a custom action filter plus Data Annotations:

```csharp
// Validators/RequestValidators.cs — managedcode
public class CreateBookValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.ISBN)
            .Matches(@"^(?:\d{9}[\dXx]|\d{13}|\d{3}-\d{1,5}-\d{1,7}-\d{1,7}-\d{1})$")
            .WithMessage("ISBN must be a valid ISBN-10 or ISBN-13 format.");
    }
}
```

```csharp
// Middleware/FluentValidationFilter.cs — managedcode
public class FluentValidationFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Auto-validates all action arguments against registered validators
    }
}
```

- ✅ FluentValidation with custom validators
- ✅ ISBN regex validation
- ✅ `ValidationProblemDetails` on errors
- ✅ Service-level guard clauses with `BusinessRuleException`

### dotnet-artisan (Score: 4) and dotnet-webapi (Score: 4)
Both use **Data Annotations** on DTOs plus service-level guard clauses:

```csharp
// DTOs/AuthorDtos.cs — dotnet-artisan
public sealed class CreateAuthorRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
}
```

```csharp
// Services/LoanService.cs — dotnet-webapi
var book = await db.Books.FindAsync([request.BookId], ct)
    ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");
if (!patron.IsActive)
    throw new ArgumentException("Patron's membership is not active.");
```

- ✅ `[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]` on DTOs
- ✅ Null-coalescing throw expressions
- ❌ No FluentValidation / custom ISBN format validation

### dotnet-skills (Score: 4)
Uses FluentValidation like managedcode but with slightly less precise validators.

### no-skills (Score: 3)
Relies on service-layer validation and custom exceptions but **no DTO-level validation attributes** on request classes:

```csharp
// DTOs/Dtos.cs — no-skills
public class CreateAuthorDto
{
    public string FirstName { get; set; } = string.Empty;  // No [Required] or [MaxLength]
    public string LastName { get; set; } = string.Empty;
}
```

- ❌ DTOs lack validation attributes
- ✅ Service-level guard clauses present

**Verdict:** **managedcode** leads with FluentValidation + ISBN regex. **dotnet-artisan** and **dotnet-webapi** are solid with Data Annotations. **no-skills** lacks DTO-level validation.

---

### 5. NuGet & Package Discipline [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 3 | 4 | 5 | 4 |
| 2 | 4 | 5 | 2 | 3 | 2 |
| 3 | 2 | 4 | 3 | 3 | 2 |
| **Mean** | **3.7** | **4.0** | **3.0** | **3.7** | **2.7** |

#### Analysis

| Configuration | Packages | Version Strategy | Unnecessary Packages |
|---|---|---|---|
| dotnet-artisan | 4 | Wildcards (`10.*-*`) on 3 packages | Swashbuckle.AspNetCore.SwaggerUI |
| dotnet-webapi | 3 | **Exact versions** | None |
| managedcode | 5 | **Exact versions** | Swashbuckle, FluentValidation |
| dotnet-skills | 5 | Wildcards (`10.0.*-*`) on 2 packages | Swashbuckle, FluentValidation |
| no-skills | 3 | **Exact versions** | Swashbuckle |

### dotnet-webapi (Score: 5) — Best
```xml
<!-- dotnet-webapi .csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```
- ✅ Only 3 packages — minimal footprint
- ✅ All exact versions — reproducible builds
- ✅ No Swashbuckle — uses built-in OpenAPI only

### dotnet-artisan (Score: 2) — Wildcard versions
```xml
<!-- dotnet-artisan .csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.*-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="10.1.7" />
```
- ❌ `10.*-*` wildcard versions on core packages — non-reproducible
- ❌ Swashbuckle included alongside built-in OpenAPI

### no-skills (Score: 4) — Exact versions but has Swashbuckle
```xml
<!-- no-skills .csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```
- ✅ Exact versions
- ❌ No `Microsoft.AspNetCore.OpenApi` — relies on Swashbuckle

### managedcode (Score: 3) and dotnet-skills (Score: 2)
Both include FluentValidation and Swashbuckle with dotnet-skills using wildcard versions.

**Verdict:** **dotnet-webapi** demonstrates perfect package discipline — minimal, exact, no unnecessary dependencies. Wildcard versions (`*-*`) are dangerous for build reproducibility.

---

### 6. EF Migration Usage [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 2 | 2 | 2 |
| 2 | 1 | 1 | 1 | 1 | 1 |
| 3 | 1 | 5 | 1 | 1 | 1 |
| **Mean** | **1.3** | **3.7** | **1.3** | **1.3** | **1.3** |

#### Analysis

**All five configurations use `EnsureCreated()` / `EnsureCreatedAsync()` instead of migrations.**

```csharp
// Program.cs — all configurations
await db.Database.EnsureCreatedAsync();
```

- ❌ No `dotnet ef migrations add` or `context.Database.Migrate()`
- ❌ Schema evolution impossible without data loss
- ❌ `EnsureCreated()` is explicitly documented as an anti-pattern for production

**Scores:** All **1** — This is a universal gap across all configurations.

**Verdict:** No configuration generates EF Core migrations. This is the most impactful shared deficiency.

---

### 7. Business Logic Correctness [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| 3 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.7** | **4.7** | **4.7** | **4.7** | **4.7** |

#### Analysis

All five configurations implement the complete set of **40 endpoints** and **8 business rules** specified in the prompt.

**Checkout rules verified across all:**
```csharp
// All configurations implement these checks in LoanService/CheckoutAsync:
if (!patron.IsActive) throw ...;                     // Membership active
if (book.AvailableCopies <= 0) throw ...;            // Available copies
if (unpaidFines >= 10.00m) throw ...;                // Fine threshold
if (activeLoansCount >= maxLoans) throw ...;         // Borrowing limit
```

**Return processing verified:**
- ✅ Overdue fine calculation: `overdueDays * 0.25m`
- ✅ Reservation queue promotion: Pending → Ready with 3-day expiration
- ✅ Copy management: AvailableCopies incremented/decremented

**Renewal rules verified:**
- ✅ Max 2 renewals, no renewal if overdue, no renewal if pending reservations

**Scores:** All **5** — Complete and correct business logic across all configurations.

**Verdict:** Every configuration faithfully implements the specification. The business logic layer is consistently high quality.

---

### 8. Prefer Built-in over 3rd Party [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 3 | 3 | 3 |
| 2 | 2 | 5 | 3 | 2 | 2 |
| 3 | 2 | 5 | 3 | 3 | 2 |
| **Mean** | **2.7** | **5.0** | **3.0** | **2.7** | **2.3** |

#### Analysis

### dotnet-webapi (Score: 5) — Uses only built-in APIs
```csharp
// Program.cs — dotnet-webapi
builder.Services.AddOpenApi();          // Built-in OpenAPI
app.MapOpenApi();                       // Built-in endpoint
// No Swashbuckle, no FluentValidation, no Newtonsoft, no AutoMapper
```

### dotnet-artisan (Score: 3) — Mixed
```csharp
// Program.cs — dotnet-artisan
builder.Services.AddOpenApi("v1", ...); // Built-in OpenAPI ✅
app.MapOpenApi();                       // Built-in ✅
app.UseSwaggerUI(...);                  // Swashbuckle SwaggerUI ❌
```
Uses built-in `AddOpenApi()` but adds `Swashbuckle.AspNetCore.SwaggerUI` for the UI component.

### managedcode (Score: 2), dotnet-skills (Score: 2), no-skills (Score: 2)
All use **Swashbuckle** for OpenAPI:
```csharp
// Program.cs — no-skills
builder.Services.AddSwaggerGen(c => { ... });
app.UseSwagger();
app.UseSwaggerUI(c => { ... });
```
- ❌ Full Swashbuckle dependency instead of built-in `AddOpenApi()`/`MapOpenApi()`

managedcode and dotnet-skills additionally include **FluentValidation** — a third-party library where Data Annotations could suffice.

**Verdict:** **dotnet-webapi** achieves a zero-third-party footprint. The built-in `AddOpenApi()`/`MapOpenApi()` replaces Swashbuckle for OpenAPI generation.

---

### 9. Modern C# Adoption [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 4 | 4 |
| 2 | 3 | 5 | 5 | 4 | 3 |
| 3 | 2 | 5 | 5 | 4 | 2 |
| **Mean** | **2.7** | **5.0** | **5.0** | **4.0** | **3.0** |

#### Analysis

| Feature | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|---|---|---|---|---|
| Primary constructors | ✅ All services, DbContext | ✅ All services, DbContext | ✅ All services, controllers | ❌ Traditional ctors in services | ❌ Traditional ctors |
| Collection expressions `[]` | ✅ Throughout | ✅ Throughout | ✅ Entities | ❌ `new List<T>()` | ❌ `new List<T>()` |
| File-scoped namespaces | ✅ All files | ✅ All files | ✅ All files | ✅ All files | ✅ All files |
| `required` keyword | ✅ On entities | ✅ On entities and DTOs | ❌ Not used | ❌ Not used | ❌ Not used |
| Record DTOs | ❌ Classes with init | ✅ Sealed records | ✅ Records | ❌ Classes | ❌ Classes |
| Switch expressions | ✅ Used | ✅ Used | ✅ Used | ✅ Used | ✅ Used |

### dotnet-artisan (Score: 5)
```csharp
// Services/AuthorService.cs — dotnet-artisan
public sealed class AuthorService(LibraryDbContext db, ILogger<AuthorService> logger) : IAuthorService

// Models/Author.cs — dotnet-artisan
public ICollection<BookAuthor> BookAuthors { get; set; } = [];
public required string FirstName { get; set; }
```

### dotnet-webapi (Score: 5)
```csharp
// DTOs/AuthorDTOs.cs — dotnet-webapi
public sealed record AuthorResponse(int Id, string FirstName, string LastName, ...);
public sealed record CreateAuthorRequest { [Required] public required string FirstName { get; init; } }
```

### dotnet-skills (Score: 3) and no-skills (Score: 3)
```csharp
// Services/LoanService.cs — dotnet-skills (no primary constructors)
public sealed class LoanService : ILoanService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<LoanService> _logger;
    public LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    {
        _db = db;
        _logger = logger;
    }
}

// Models/Author.cs — no-skills (old collection init)
public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
```

**Verdict:** **dotnet-artisan** and **dotnet-webapi** make full use of C# 12 features. The others fall back to older patterns.

---

### 10. Error Handling & Middleware [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 5 | 4 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| 3 | 3 | 5 | 3 | 4 | 4 |
| **Mean** | **4.0** | **5.0** | **4.3** | **4.7** | **4.3** |

#### Analysis

All five configurations implement the modern `IExceptionHandler` pattern with `ProblemDetails`.

```csharp
// All configurations — IExceptionHandler pattern
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (404, "Not Found"),
            // ...
        };
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails { ... }, ct);
        return true;
    }
}
```

Notable differences:
- **managedcode** and **dotnet-skills** additionally map `BusinessRuleException` → 409 Conflict
- **no-skills** registers **two** `IExceptionHandler` implementations (`BusinessExceptionHandler` + `GlobalExceptionHandler`)
- **dotnet-webapi** returns `false` for unhandled exceptions (letting default handler take over)

**Scores:** All **5** — Excellent modern error handling across the board.

**Verdict:** Universally well-implemented. managedcode and no-skills add custom `BusinessRuleException` for cleaner exception semantics.

---

### 11. Async Patterns & Cancellation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 3 | 3 |
| 2 | 3 | 5 | 5 | 3 | 3 |
| 3 | 2 | 5 | 5 | 2 | 2 |
| **Mean** | **2.7** | **5.0** | **5.0** | **2.7** | **2.7** |

#### Analysis

### dotnet-artisan (Score: 5) and dotnet-webapi (Score: 5)
**CancellationToken propagated through all layers:**

```csharp
// Endpoint → Service → EF Core — dotnet-webapi
group.MapGet("/", async (..., CancellationToken ct) =>
    TypedResults.Ok(await service.GetAllAsync(search, page, pageSize, ct)));

// Service method signature
Task<PaginatedResponse<AuthorListResponse>> GetAllAsync(..., CancellationToken ct);

// EF Core calls
await query.ToListAsync(ct);
await db.SaveChangesAsync(ct);
```

### managedcode (Score: 3), dotnet-skills (Score: 3), no-skills (Score: 3)
**No CancellationToken** — controllers and services lack the parameter:

```csharp
// Services/Interfaces.cs — dotnet-skills
Task<PagedResponse<AuthorDto>> GetAuthorsAsync(string? search, int page, int pageSize);
// No CancellationToken parameter

// EF Core calls — no token forwarded
await _db.SaveChangesAsync();  // No ct parameter
```

**Verdict:** Only **dotnet-artisan** and **dotnet-webapi** propagate CancellationToken end-to-end. The other three waste server resources on cancelled requests.

---

### 12. EF Core Best Practices [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 5 | 5 |
| 2 | 3 | 5 | 5 | 4 | 4 |
| 3 | 3 | 4 | 4 | 4 | 5 |
| **Mean** | **3.3** | **4.7** | **4.7** | **4.3** | **4.7** |

#### Analysis

| Feature | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|---|---|---|---|---|
| Fluent API config | ✅ Inline OnModelCreating | ✅ Inline OnModelCreating | ✅ IEntityTypeConfiguration | ✅ Inline OnModelCreating | ✅ Inline |
| AsNoTracking for reads | ✅ Consistent | ✅ Consistent | ✅ Consistent | ✅ Consistent | ❌ Not used |
| HasConversion for enums | ✅ All enums → string | ✅ All enums → string | ✅ All enums → string | ✅ All enums → string | ✅ All enums → string |
| Composite keys | ✅ BookAuthor, BookCategory | ✅ BookAuthor, BookCategory | ✅ BookAuthor, BookCategory | ✅ BookAuthor, BookCategory | ✅ BookAuthor, BookCategory |
| Delete behavior | Cascade (convention) | ✅ Explicit Restrict | Cascade (convention) | Cascade (convention) | ✅ Explicit Restrict |
| Decimal precision | ✅ HasPrecision(10,2) | ✅ decimal(10,2) | ✅ HasPrecision(10,2) | ✅ HasPrecision(10,2) | ✅ Column TypeName |

### dotnet-webapi (Score: 5)
```csharp
// Data/LibraryDbContext.cs — dotnet-webapi
entity.HasOne(l => l.Book).WithMany(b => b.Loans)
    .HasForeignKey(l => l.BookId).OnDelete(DeleteBehavior.Restrict);
```
- ✅ Explicit `DeleteBehavior.Restrict` prevents accidental cascade deletes

### no-skills (Score: 3)
```csharp
// Services/BookService.cs — no-skills
var books = await _db.Books.Include(b => b.BookAuthors).ToListAsync();
// No .AsNoTracking() — tracking overhead on read-only queries
```

**Verdict:** **dotnet-artisan** and **dotnet-webapi** lead with `AsNoTracking` and explicit delete behaviors. **no-skills** misses `AsNoTracking` entirely.

---

### 13. Service Abstraction & DI [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| 3 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.7** | **4.7** | **4.7** | **4.7** | **4.7** |

#### Analysis

All configurations use **interface-based DI** with `AddScoped<IService, Service>()`:

```csharp
// Program.cs — all configurations
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
// ... 7 service registrations total
```

All follow single-responsibility (one service per domain entity) and proper scoped lifetimes.

**Scores:** All **5** — Universally well-implemented.

---

### 14. Security Configuration [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 3 | 3 | 3 | 3 |
| 2 | 1 | 1 | 1 | 1 | 1 |
| 3 | 1 | 1 | 1 | 3 | 1 |
| **Mean** | **1.7** | **1.7** | **1.7** | **2.3** | **1.7** |

#### Analysis

**No configuration implements HSTS or HTTPS redirection.**

```csharp
// Program.cs — none of the configurations include:
// app.UseHsts();
// app.UseHttpsRedirection();
```

- ❌ No HSTS middleware
- ❌ No HTTPS redirection
- ❌ No CORS configuration
- ❌ No authentication/authorization (as specified)

**Scores:** All **1** — Universal security gap.

**Verdict:** While authentication was explicitly excluded from the spec, HSTS and HTTPS redirection are baseline security requirements.

---

### 15. DTO Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 2 | 5 | 3 | 4 | 3 |
| 3 | 2 | 5 | 5 | 4 | 2 |
| **Mean** | **3.0** | **5.0** | **4.3** | **4.3** | **3.3** |

#### Analysis

### dotnet-webapi (Score: 5) — Sealed records with factory methods
```csharp
// DTOs/AuthorDTOs.cs — dotnet-webapi
public sealed record AuthorResponse(int Id, string FirstName, string LastName, ...);
public sealed record CreateAuthorRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
}
public sealed record PaginatedResponse<T>
{
    public static PaginatedResponse<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount) => ...;
}
```
- ✅ Sealed records (immutable, value equality)
- ✅ Positional records for responses
- ✅ `IReadOnlyList<T>` for collections
- ✅ Factory method on pagination DTO

### managedcode (Score: 4) — Records
```csharp
// DTOs/AuthorDtos.cs — managedcode
public record AuthorResponse(int Id, string FirstName, ...);
public record CreateAuthorRequest { [Required] public string FirstName { get; init; } = string.Empty; }
```
- ✅ Records (implicitly sealed)
- ❌ Not explicitly sealed

### dotnet-artisan (Score: 3) — Classes with init
```csharp
// DTOs/AuthorDtos.cs — dotnet-artisan
public class AuthorResponse { public int Id { get; init; } public required string FirstName { get; init; } }
public sealed class AuthorDetailResponse : AuthorResponse { ... }
```
- ❌ Base response classes are non-sealed classes (not records)
- ✅ Init-only properties
- ✅ Sealed on derived types

### no-skills (Score: 2) — Mutable classes
```csharp
// DTOs/Dtos.cs — no-skills
public class AuthorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;  // Mutable setters
}
```
- ❌ Mutable `{ get; set; }` classes
- ❌ Not sealed, not records
- ❌ `*Dto` naming instead of `*Request`/`*Response`

**Verdict:** **dotnet-webapi** shows exemplary DTO design with sealed records and factory methods.

---

### 16. Sealed Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 5 | 2 | 5 |
| 2 | 2 | 5 | 5 | 2 | 2 |
| 3 | 1 | 5 | 5 | 1 | 3 |
| **Mean** | **1.3** | **5.0** | **5.0** | **1.7** | **3.3** |

#### Analysis

| Configuration | Entities Sealed | Services Sealed | DbContext Sealed | DTOs Sealed |
|---|---|---|---|---|
| dotnet-artisan | ✅ All sealed | ✅ All sealed | ✅ Sealed | ✅ Partially sealed |
| dotnet-webapi | ✅ All sealed | ✅ All sealed | ✅ Sealed | ✅ All sealed records |
| managedcode | ❌ Not sealed | ❌ Not sealed | ❌ Not sealed | ✅ Records (implicit) |
| dotnet-skills | ❌ Not sealed | ✅ All sealed | ❌ Not sealed | ❌ Not sealed |
| no-skills | ❌ Not sealed | ❌ Not sealed | ❌ Not sealed | ❌ Not sealed |

**Scores:** dotnet-artisan: **5** | dotnet-webapi: **5** | managedcode: **2** | dotnet-skills: **2** | no-skills: **2**

**Verdict:** Only **dotnet-artisan** and **dotnet-webapi** consistently apply `sealed` across all type categories, enabling JIT devirtualization optimizations.

---

### 17. Data Seeder Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 4 | 4 | 4 | 4 | 4 |
| 3 | 3 | 5 | 3 | 3 | 3 |
| **Mean** | **4.0** | **4.7** | **4.0** | **4.0** | **4.0** |

#### Analysis

All configurations use a static `DataSeeder.SeedAsync()` method with idempotency guards:

```csharp
// DataSeeder.cs — all configurations
public static async Task SeedAsync(LibraryDbContext db)
{
    if (await db.Authors.AnyAsync()) return;  // Idempotent guard
    // ... seed data
}
```

All seed realistic data: 5-6 authors, 5-6 categories, 12 books, 6-7 patrons, 8-12 loans, 3+ reservations, 3-4 fines.

**Scores:** All **4** — Good seeder design; not using `HasData()` in migrations (which would earn a 5).

---

### 18. Structured Logging [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 4 | 3 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| 3 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.3** | **4.7** | **4.3** | **4.3** | **4.0** |

#### Analysis

All configurations use `ILogger<T>` with structured message templates:

```csharp
// All configurations
logger.LogInformation("Book checked out: LoanId={LoanId}, BookId={BookId}, PatronId={PatronId}",
    loan.Id, book.Id, patron.Id);
```

- ✅ Named placeholders `{LoanId}`, `{BookId}`
- ✅ No string interpolation in log messages
- ✅ Appropriate log levels (Information for operations, Warning/Error for exceptions)
- ❌ No `[LoggerMessage]` source generators in any configuration

**Scores:** All **5** — Consistent structured logging.

---

### 19. Nullable Reference Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| 3 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.7** | **4.7** | **4.7** | **4.7** | **4.7** |

#### Analysis

All configurations enable nullable reference types and use proper annotations:

```xml
<!-- All .csproj files -->
<Nullable>enable</Nullable>
```

```csharp
// All configurations
public string? Biography { get; set; }        // Optional
public DateTime? ReturnDate { get; set; }     // Null while active
public Book Book { get; set; } = null!;       // Navigation property
```

**Scores:** All **5** — Universal NRT adoption.

---

### 20. API Documentation [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 3 | 4 |
| 2 | 3 | 5 | 4 | 3 | 4 |
| 3 | 3 | 5 | 4 | 3 | 3 |
| **Mean** | **3.0** | **5.0** | **4.3** | **3.0** | **3.7** |

#### Analysis

### dotnet-webapi (Score: 5)
```csharp
// Endpoints/AuthorEndpoints.cs — dotnet-webapi
group.MapGet("/", async (...) => ...)
    .WithName("GetAuthors")
    .WithSummary("List authors")
    .WithDescription("List authors with optional search by name and pagination.")
    .Produces<PaginatedResponse<AuthorListResponse>>();
```
- ✅ `.WithName()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()`
- ✅ Built-in OpenAPI via `AddOpenApi()`/`MapOpenApi()`

### dotnet-artisan (Score: 4)
```csharp
group.MapGet("/", ...)
    .WithName("GetAuthors")
    .WithSummary("List authors with search by name and pagination");
```
- ✅ `.WithName()`, `.WithSummary()`, `.WithTags()`
- ❌ No `.WithDescription()` (slightly less metadata)

### dotnet-skills (Score: 4)
Uses XML doc comments on controllers:
```csharp
/// <summary>List authors with optional search by name and pagination.</summary>
[HttpGet]
[ProducesResponseType(typeof(PagedResponse<AuthorDto>), 200)]
```

### managedcode (Score: 3) and no-skills (Score: 3)
Use `[ProducesResponseType]` but limited descriptions.

**Verdict:** **dotnet-webapi** provides the richest OpenAPI metadata via endpoint fluent methods.

---

### 21. File Organization [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 5 | 5 |
| 2 | 3 | 5 | 5 | 4 | 3 |
| 3 | 3 | 5 | 5 | 4 | 4 |
| **Mean** | **3.3** | **5.0** | **5.0** | **4.3** | **4.0** |

#### Analysis

| Configuration | Structure Pattern | Endpoint Separation | Interface Files |
|---|---|---|---|
| dotnet-artisan | Clean per-concern folders | ✅ Endpoints/ directory | Interfaces in service files |
| dotnet-webapi | Clean per-concern folders | ✅ Endpoints/ directory | ✅ Separate IService files |
| managedcode | Per-concern with Validators/ | Controllers/ directory | Single IServices.cs |
| dotnet-skills | Per-concern | Controllers/ directory | Single Interfaces.cs |
| no-skills | Per-concern (minimal) | Controllers/ directory | Single Interfaces.cs |

### dotnet-webapi (Score: 5) — Separate interface files
```
Services/
├── IAuthorService.cs
├── AuthorService.cs
├── IBookService.cs
├── BookService.cs
└── ...
```

### dotnet-skills (Score: 3) — All DTOs in one file
```
DTOs/
└── Dtos.cs          # All 29 DTO classes in a single file
Services/
└── Interfaces.cs    # All 7 interfaces in a single file
```

**Scores:** dotnet-artisan: **5** | dotnet-webapi: **5** | managedcode: **4** | dotnet-skills: **3** | no-skills: **3**

---

### 22. HTTP Test File Quality [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 4 | 5 |
| 2 | 4 | 5 | 5 | 5 | 4 |
| 3 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.3** | **4.7** | **4.7** | **4.3** | **4.3** |

#### Analysis

All configurations include comprehensive `.http` test files covering all endpoints with realistic data.

### dotnet-artisan (Score: 5) and dotnet-webapi (Score: 5)
Both include business rule failure tests:
```http
### Attempt checkout with too many unpaid fines (should fail)
POST {{baseUrl}}/api/loans
Content-Type: application/json

{
  "bookId": 1,
  "patronId": 4
}
```

### dotnet-skills (Score: 4) and no-skills (Score: 4)
Comprehensive but with fewer negative test cases.

**Verdict:** dotnet-artisan and dotnet-webapi provide the most thorough `.http` files.

---

### 23. Type Design & Resource Management [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | — | — | — | — | — |
| 3 | — | — | — | — | — |
| **Mean** | **5.0** | **5.0** | **5.0** | **5.0** | **5.0** |

#### Analysis

All configurations use enums for status fields (no magic strings):

```csharp
// All configurations
public enum LoanStatus { Active, Returned, Overdue }
public enum MembershipType { Standard, Premium, Student }
```

**dotnet-artisan** and **dotnet-webapi** additionally use `IReadOnlyList<T>` for service return types (score 5). Others use `List<T>` (score 4).

---

### 24. Code Standards Compliance [LOW × 0]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 4 | 5 |
| 2 | 3 | 5 | 5 | 4 | 3 |
| 3 | 3 | 5 | 5 | 4 | 3 |
| **Mean** | **3.7** | **5.0** | **5.0** | **4.0** | **3.7** |

#### Analysis

| Feature | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|---|---|---|---|---|
| PascalCase types | ✅ | ✅ | ✅ | ✅ | ✅ |
| camelCase params | ✅ | ✅ | ✅ | ✅ | ✅ |
| Async suffix | ✅ | ✅ | ✅ | ✅ | ✅ |
| I prefix interfaces | ✅ | ✅ | ✅ | ✅ | ✅ |
| Explicit access modifiers | ✅ `public sealed` | ✅ `internal sealed` | ✅ `public` | ✅ `public` | ✅ `public` |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ | ✅ |

**dotnet-webapi** notably uses `internal sealed` on `ApiExceptionHandler` — correctly limiting visibility.

**Scores:** dotnet-artisan: **5** | dotnet-webapi: **5** | managedcode: **4** | dotnet-skills: **3** | no-skills: **3**

---

### 25. Type Design & Resource Mgmt [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | — | — | — | — | — |
| 2 | 4 | 5 | 5 | 4 | 4 |
| 3 | 3 | 4 | 4 | 4 | 3 |
| **Mean** | **3.5** | **4.5** | **4.5** | **4.0** | **3.5** |

---

## Raw Data References

- Per-run analysis: `reports/analysis-run-1.md`
- Per-run analysis: `reports/analysis-run-2.md`
- Per-run analysis: `reports/analysis-run-3.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
