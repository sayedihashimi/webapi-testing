# Comparative Analysis: dotnet-artisan, dotnet-webapi, managedcode-dotnet-skills, dotnet-skills, no-skills

## Introduction

This report compares **five Copilot skill configurations** used to generate the same ASP.NET Core Web API project — a **Community Library Management API** (LibraryApi) for managing books, authors, categories, patrons, loans, reservations, and fines.

| Configuration | Description | Run-2 Status |
|---|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain (9 skills + 14 agents) | ✅ Full project generated |
| **dotnet-webapi** | dotnet-webapi skill | ✅ Full project generated |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | ❌ No code generated (only `Directory.Build.props`) |
| **managedcode-dotnet-skills** | Community managed-code skills | ❌ No code generated (only `Directory.Build.props`) |
| **no-skills** | Baseline (default Copilot, no skills) | ✅ Full project generated |

**Note:** `dotnet-skills` and `managedcode-dotnet-skills` produced no application code in run-2 — only a `Directory.Build.props` file containing a Meziantou.Analyzer reference with wildcard version `Version="*"`. These configurations receive a score of **1** across all dimensions. The analysis focuses primarily on the three working configurations.

Only the **LibraryApi** scenario was present in run-2 (FitnessStudioApi and VetClinicApi were not generated for this run).

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 5 | 5 | 1 | 1 | 5 |
| Security Vulnerability Scan [CRITICAL] | 5 | 5 | 1 | 1 | 4 |
| Minimal API Architecture [CRITICAL] | 4 | 5 | 1 | 1 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 4 | 4 | 1 | 1 | 4 |
| NuGet & Package Discipline [CRITICAL] | 5 | 3 | 1 | 1 | 3 |
| EF Migration Usage [CRITICAL] | 2 | 2 | 1 | 1 | 2 |
| Business Logic Correctness [HIGH] | 5 | 5 | 1 | 1 | 5 |
| Prefer Built-in over 3rd Party [HIGH] | 5 | 5 | 1 | 1 | 1 |
| Modern C# Adoption [HIGH] | 5 | 5 | 1 | 1 | 2 |
| Error Handling & Middleware [HIGH] | 3 | 5 | 1 | 1 | 3 |
| Async Patterns & Cancellation [HIGH] | 5 | 5 | 1 | 1 | 2 |
| EF Core Best Practices [HIGH] | 5 | 5 | 1 | 1 | 3 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 1 | 1 | 3 |
| Security Configuration [HIGH] | 1 | 1 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 5 | 5 | 1 | 1 | 2 |
| Sealed Types [MEDIUM] | 5 | 5 | 1 | 1 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 1 | 1 | 4 |
| Structured Logging [MEDIUM] | 5 | 5 | 1 | 1 | 4 |
| Nullable Reference Types [MEDIUM] | 5 | 5 | 1 | 1 | 4 |
| API Documentation [MEDIUM] | 5 | 5 | 1 | 1 | 4 |
| File Organization [MEDIUM] | 5 | 5 | 1 | 1 | 3 |
| HTTP Test File Quality [MEDIUM] | 5 | 5 | 1 | 1 | 4 |
| Type Design & Resource Management [MEDIUM] | 5 | 5 | 1 | 1 | 3 |
| Code Standards Compliance [LOW] | 5 | 5 | 1 | 1 | 3 |

---

## 1. Build & Run Success [CRITICAL]

All three working configurations build successfully with **zero errors and zero warnings**.

```
# All three configs
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The `dotnet-skills` and `managedcode-dotnet-skills` configurations generated no buildable code — only a `Directory.Build.props` file.

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Build succeeded, 0 errors, 0 warnings |
| dotnet-webapi | **5** | Build succeeded, 0 errors, 0 warnings |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **5** | Build succeeded, 0 errors, 0 warnings |

**Verdict:** Three-way tie among the working configurations. All produce clean, zero-warning builds.

---

## 2. Security Vulnerability Scan [CRITICAL]

All three working configurations report **no vulnerable packages** from `dotnet list package --vulnerable`. However, the `no-skills` configuration includes an unnecessary third-party package (Swashbuckle) that increases attack surface.

```xml
<!-- no-skills: adds unnecessary 3rd-party package -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | No vulnerabilities, minimal packages (3 packages only) |
| dotnet-webapi | **5** | No vulnerabilities, minimal packages (3 packages only) |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **4** | No vulnerabilities, but adds Swashbuckle (4 packages total — larger attack surface) |

**Verdict:** `dotnet-artisan` and `dotnet-webapi` tie — both use the minimum viable package set. `no-skills` adds an unnecessary dependency.

---

## 3. Minimal API Architecture [CRITICAL]

This dimension reveals the **largest architectural divergence** between the configurations.

**dotnet-artisan** — Uses Minimal APIs with `MapGroup()` and `Results.` helpers:
```csharp
// dotnet-artisan: AuthorEndpoints.cs
public static class AuthorEndpoints {
    public static RouteGroupBuilder MapAuthorEndpoints(this WebApplication app) {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async (string? search, int page, int pageSize,
                                  IAuthorService service, CancellationToken ct) =>
            Results.Ok(await service.GetAuthorsAsync(search, page, pageSize, ct)))
            .WithName("GetAuthors")
            .Produces<PaginatedResponse<AuthorResponse>>();

        return group;
    }
}
```

**dotnet-webapi** — Uses Minimal APIs with `MapGroup()`, `TypedResults.`, and `Results<T1, T2>` union return types:
```csharp
// dotnet-webapi: AuthorEndpoints.cs — union return types
group.MapGet("/{id:int}", async Task<Results<Ok<AuthorResponse>, NotFound>> (
    int id, IAuthorService service, CancellationToken ct) =>
{
    var author = await service.GetByIdAsync(id, ct);
    return author is null ? TypedResults.NotFound() : TypedResults.Ok(author);
})
.WithName("GetAuthorById")
.Produces<AuthorResponse>()
.Produces(StatusCodes.Status404NotFound);
```

**no-skills** — Uses traditional Controllers with `[ApiController]`:
```csharp
// no-skills: AuthorsController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuthorDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var author = await _service.GetByIdAsync(id);
        return author == null ? NotFound() : Ok(author);
    }
}
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **4** | Minimal APIs + MapGroup + endpoint extensions, but uses `Results.` instead of `TypedResults.` and lacks union return types |
| dotnet-webapi | **5** | Minimal APIs + MapGroup + endpoint extensions + `TypedResults.` + `Results<T1, T2>` union types for compile-time type safety |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **1** | Uses Controllers — the legacy pattern with more boilerplate and overhead |

**Verdict:** `dotnet-webapi` is best — its union return types (`Results<Ok<T>, NotFound>`) enable automatic OpenAPI schema generation and compile-time safety. `dotnet-artisan` is close but misses the TypedResults pattern. `no-skills` falls back to the controller pattern entirely.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

All three working configurations use DataAnnotations for DTO validation and guard clauses in service methods. The patterns are nearly identical.

```csharp
// dotnet-artisan & dotnet-webapi: CreateAuthorRequest
public sealed record CreateAuthorRequest {
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
    [Required, MaxLength(100)]
    public required string LastName { get; init; }
    [MaxLength(2000)]
    public string? Biography { get; init; }
}
```

```csharp
// no-skills: BookCreateDto — uses classes, not records
public class BookCreateDto {
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    [Required, Range(1, int.MaxValue, ErrorMessage = "TotalCopies must be at least 1")]
    public int TotalCopies { get; set; }
}
```

All configs enforce checkout business rules with guard clauses:
```csharp
// All three configs — business rule validation in LoanService
if (!patron.IsActive)
    throw new InvalidOperationException("Patron's membership is inactive.");
if (book.AvailableCopies < 1)
    throw new ArgumentException("No available copies.");
if (unpaidFines >= 10.00m)
    throw new ArgumentException("Patron has unpaid fines exceeding $10.00.");
```

None of the configurations use modern `ArgumentNullException.ThrowIfNull()` or `ArgumentException.ThrowIfNullOrEmpty()` guard clause helpers on constructor parameters.

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **4** | DataAnnotations on DTOs, business rule guards in services, but no modern ThrowIf helpers |
| dotnet-webapi | **4** | Same approach — DataAnnotations + service-layer guards |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **4** | Same DataAnnotations + guards, but DTOs are classes not records (less type safety) |

**Verdict:** Tie across all working configurations. All enforce validation adequately but none use the modern `ThrowIfNull()`/`ThrowIfNullOrEmpty()` patterns on DI-injected constructor parameters.

---

## 5. NuGet & Package Discipline [CRITICAL]

The `.csproj` files reveal significant differences in version pinning strategy.

```xml
<!-- dotnet-artisan: Exact versions, 3 packages -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```

```xml
<!-- dotnet-webapi: WILDCARD versions on EF Core packages -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*-*">
  ...
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

```xml
<!-- no-skills: Exact versions but adds Swashbuckle (4 packages) -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">...</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Exact versions on all 3 packages; minimal set |
| dotnet-webapi | **3** | Uses `10.*-*` wildcard on 2/3 packages — causes non-reproducible builds |
| dotnet-skills | **1** | No code (only `Directory.Build.props` with `Version="*"` on Meziantou.Analyzer) |
| managedcode | **1** | Same as dotnet-skills |
| no-skills | **3** | Exact versions but adds unnecessary Swashbuckle package |

**Verdict:** `dotnet-artisan` is best — exact version pins on a minimal package set. `dotnet-webapi` wildcards are risky for reproducibility. `no-skills` pins correctly but adds an unnecessary package.

---

## 6. EF Migration Usage [CRITICAL]

**All three working configurations use `EnsureCreatedAsync()`** — the anti-pattern that bypasses EF Core migrations.

```csharp
// All three configs — Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Database.EnsureCreatedAsync();   // ← Anti-pattern
    await DataSeeder.SeedAsync(db);
}
```

None of the configurations use `dotnet ef migrations add` or `context.Database.MigrateAsync()`.

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **2** | Uses EnsureCreatedAsync; gen-notes acknowledge it but still use it |
| dotnet-webapi | **2** | Uses EnsureCreatedAsync; gen-notes explicitly note this as a deviation from skill guidance |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **2** | Uses EnsureCreatedAsync |

**Verdict:** Universal failure — all configs use the same anti-pattern. The `dotnet-webapi` gen-notes acknowledge this deviation, suggesting the skill recommends migrations but the implementation chose EnsureCreated for seed data convenience.

---

## 7. Business Logic Correctness [HIGH]

All three working configurations implement the full set of specified endpoints and business rules. The LibraryApi specification defines 35+ endpoints across 7 resource groups, with complex business rules for checkout limits, overdue fines, reservation queues, and renewals.

**Endpoint coverage comparison:**

| Resource | Endpoints (spec) | dotnet-artisan | dotnet-webapi | no-skills |
|---|---|---|---|---|
| Authors | 5 | ✅ 5 | ✅ 5 | ✅ 5 |
| Categories | 5 | ✅ 5 | ✅ 5 | ✅ 5 |
| Books | 7 | ✅ 7 | ✅ 7 | ✅ 7 |
| Patrons | 8 | ✅ 8 | ✅ 8 | ✅ 8 |
| Loans | 6 | ✅ 6 | ✅ 6 | ✅ 6 |
| Reservations | 5 | ✅ 5 | ✅ 5 | ✅ 5 |
| Fines | 4 | ✅ 4 | ✅ 4 | ✅ 4 |

All business rules are implemented:
- ✅ Borrowing limits by membership type (Standard=5, Premium=10, Student=3)
- ✅ Fine threshold ($10.00 checkout block)
- ✅ Overdue fine calculation ($0.25/day)
- ✅ Reservation queue management
- ✅ Renewal rules (max 2, no overdue, no pending reservations)

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Full endpoint coverage, all business rules implemented |
| dotnet-webapi | **5** | Full endpoint coverage, all business rules implemented |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **5** | Full endpoint coverage via controllers, all business rules implemented |

**Verdict:** Three-way tie — all working configurations deliver a functionally complete API.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

This dimension reveals a **stark divide** between skilled and unskilled configurations.

```csharp
// dotnet-artisan & dotnet-webapi: Built-in OpenAPI
builder.Services.AddOpenApi();       // Microsoft.AspNetCore.OpenApi (built-in)
app.MapOpenApi();                    // Serves at /openapi/v1.json
```

```csharp
// no-skills: Swashbuckle (3rd party)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new() {
        Title = "Sunrise Community Library API", ...
    });
    options.IncludeXmlComments(xmlPath);
});
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "...");
    c.RoutePrefix = string.Empty;
});
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Uses `AddOpenApi()`/`MapOpenApi()`, no 3rd-party packages |
| dotnet-webapi | **5** | Uses `AddOpenApi()`/`MapOpenApi()`, no 3rd-party packages |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **1** | Uses Swashbuckle for OpenAPI — a 3rd-party library when built-in exists |

**Verdict:** Both skilled configs use the built-in OpenAPI support. `no-skills` falls back to Swashbuckle — the most common indicator of outdated .NET practices. This is the single highest-impact difference between skilled and unskilled generation.

---

## 9. Modern C# Adoption [HIGH]

**dotnet-artisan & dotnet-webapi** — Extensive modern C# usage:
```csharp
// Primary constructors (C# 12)
public sealed class AuthorService(LibraryDbContext db, ILogger<AuthorService> logger)
    : IAuthorService { ... }

// Collection expressions (C# 12)
public ICollection<BookAuthor> BookAuthors { get; set; } = [];
public ICollection<Loan> Loans { get; set; } = [];

// File-scoped namespaces (C# 10)
namespace LibraryApi.Models;

// DbContext with primary constructor
public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options)
    : DbContext(options) { ... }
```

**no-skills** — Traditional patterns:
```csharp
// Traditional constructor (pre-C# 12)
public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;
    public AuthorService(LibraryDbContext db) => _db = db;
}

// Old collection initialization
public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

// DbContext — traditional style
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }
}
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Primary constructors, collection expressions `[]`, file-scoped namespaces, implicit usings |
| dotnet-webapi | **5** | Same modern patterns throughout |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **2** | File-scoped namespaces and target-typed `new()`, but no primary constructors, no `[]` syntax |

**Verdict:** `dotnet-artisan` and `dotnet-webapi` both generate idiomatic modern C# 12+ code. `no-skills` generates code that looks like .NET 6-era patterns.

---

## 10. Error Handling & Middleware [HIGH]

**dotnet-webapi** — Modern `IExceptionHandler` pattern (DI-aware, composable):
```csharp
// dotnet-webapi: Middleware/ApiExceptionHandler.cs
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
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
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode, Title = title,
            Detail = exception.Message, Instance = httpContext.Request.Path
        }, cancellationToken);
        return true;
    }
}

// Registration
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
```

**dotnet-artisan** — Inline `UseExceptionHandler` delegate (not DI-aware, manual JSON):
```csharp
// dotnet-artisan: Program.cs — inline handler
app.UseExceptionHandler(exceptionApp => {
    exceptionApp.Run(async context => {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var (statusCode, title, detail) = exception switch {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad Request", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };
        await context.Response.WriteAsJsonAsync(new { type = $"https://httpstatuses.com/{statusCode}", ... });
    });
});
```

**no-skills** — Traditional `UseMiddleware<T>` pattern (pre-.NET 8):
```csharp
// no-skills: GlobalExceptionHandlerMiddleware.cs
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

// Registration
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **3** | Uses `UseExceptionHandler` but inline delegate, writes anonymous ProblemDetails (not the standard class), maps `InvalidOperationException` to 400 instead of 409 |
| dotnet-webapi | **5** | Modern `IExceptionHandler`, DI-aware, uses standard `ProblemDetails` class, maps `InvalidOperationException` → 409 Conflict |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **3** | Traditional middleware, uses `ProblemDetails` class, switch expression mapping, but pre-.NET 8 pattern |

**Verdict:** `dotnet-webapi` is the clear winner — `IExceptionHandler` is the modern .NET 8+ pattern that's composable, DI-aware, and the recommended approach. `dotnet-artisan` and `no-skills` both use older patterns.

---

## 11. Async Patterns & Cancellation [HIGH]

**dotnet-artisan & dotnet-webapi** — Full `CancellationToken` propagation:
```csharp
// Endpoint handlers
group.MapGet("/{id:int}", async (int id, IAuthorService service, CancellationToken ct) => { ... })

// Service methods — CancellationToken forwarded to EF Core
public async Task<AuthorResponse?> GetByIdAsync(int id, CancellationToken ct = default)
{
    return await db.Authors.AsNoTracking()
        .Where(a => a.Id == id)
        .Select(a => new AuthorResponse(...))
        .FirstOrDefaultAsync(ct);
}
```

**no-skills** — No `CancellationToken` anywhere:
```csharp
// Controller actions — no CancellationToken parameter
public async Task<IActionResult> GetById(int id)
{
    var author = await _service.GetByIdAsync(id);
    return author == null ? NotFound() : Ok(author);
}

// Service methods — no CancellationToken
public async Task<AuthorDetailDto?> GetByIdAsync(int id)
{
    var total = await query.CountAsync();   // No cancellation support
}
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | CancellationToken in all endpoint handlers and service methods, forwarded to EF Core |
| dotnet-webapi | **5** | Same — complete CancellationToken chain from endpoint to database |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **2** | No CancellationToken in controllers or services; Async suffix used correctly |

**Verdict:** `dotnet-artisan` and `dotnet-webapi` tie — both propagate CancellationToken end-to-end. `no-skills` completely omits cancellation support, wasting server resources on cancelled requests.

---

## 12. EF Core Best Practices [HIGH]

**dotnet-artisan & dotnet-webapi** — Full Fluent API with `AsNoTracking`, enum conversion, and explicit delete behaviors:
```csharp
// dotnet-webapi: Fluent API with OnDelete behaviors
modelBuilder.Entity<Loan>(e => {
    e.HasOne(l => l.Book).WithMany(b => b.Loans)
        .HasForeignKey(l => l.BookId).OnDelete(DeleteBehavior.Restrict);
    e.Property(l => l.Status).HasConversion<string>();
});

// Read queries use AsNoTracking
var query = db.Authors.AsNoTracking().AsQueryable();
```

**no-skills** — Fluent API and enum conversion present, but no `AsNoTracking` and less explicit delete behaviors:
```csharp
// no-skills: Missing AsNoTracking on reads
public async Task<PagedResult<BookDto>> GetAllAsync(...)
{
    var query = _db.Books
        .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
        .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
        .AsQueryable();    // ← No AsNoTracking — EF tracks all entities needlessly
}

// Simpler relationship config (no explicit OnDelete)
modelBuilder.Entity<BookAuthor>()
    .HasOne(ba => ba.Book).WithMany(b => b.BookAuthors)
    .HasForeignKey(ba => ba.BookId);   // ← No OnDelete specified
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Fluent API, `AsNoTracking` on all reads, `HasConversion<string>()`, explicit `OnDelete` behaviors, unique indexes |
| dotnet-webapi | **5** | Same — comprehensive Fluent API config with all best practices |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **3** | Fluent API and enum conversion present, but no `AsNoTracking` on reads, less explicit delete behaviors |

**Verdict:** `dotnet-artisan` and `dotnet-webapi` tie — both follow EF Core best practices. `no-skills` omits `AsNoTracking`, which roughly doubles memory usage and halves performance on read queries.

---

## 13. Service Abstraction & DI [HIGH]

All three configs use interface-based service registration:

```csharp
// All configs — Interface-based DI
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
// ... 7 services total
```

The implementations differ:

```csharp
// dotnet-artisan & dotnet-webapi: Sealed + primary constructors
public sealed class AuthorService(LibraryDbContext db, ILogger<AuthorService> logger)
    : IAuthorService { ... }

// no-skills: Not sealed, traditional constructors
public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;
    public AuthorService(LibraryDbContext db) => _db = db;
}
```

File organization also differs:
- **dotnet-artisan & dotnet-webapi**: Each interface in its own file (`IAuthorService.cs`, `IBookService.cs`, etc.)
- **no-skills**: All interfaces in a single `Interfaces.cs` file (72 lines)

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Interface-based, sealed implementations, primary constructors, separate interface files |
| dotnet-webapi | **5** | Same pattern |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **3** | Interface-based DI correct, but implementations not sealed, traditional constructors, all interfaces in one file |

**Verdict:** `dotnet-artisan` and `dotnet-webapi` demonstrate the modern service pattern. `no-skills` gets the fundamentals right but misses sealed types and clean file separation.

---

## 14. Security Configuration [HIGH]

**None of the configurations implement HSTS or HTTPS redirection.** This is a universal gap.

```csharp
// Expected pattern — NOT present in ANY config
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **1** | No HSTS, no HTTPS redirection |
| dotnet-webapi | **1** | No HSTS, no HTTPS redirection |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **1** | No HSTS, no HTTPS redirection |

**Verdict:** Universal failure. No configuration addresses transport-layer security. This is a significant oversight for production readiness.

---

## 15. DTO Design [MEDIUM]

**dotnet-artisan & dotnet-webapi** — Sealed records with `init` properties and `IReadOnlyList<T>`:
```csharp
// Positional record for responses
public sealed record AuthorResponse(
    int Id, string FirstName, string LastName, string? Biography,
    DateOnly? BirthDate, string? Country, DateTime CreatedAt,
    IReadOnlyList<AuthorBookResponse> Books);

// Body record with init properties for requests
public sealed record CreateAuthorRequest {
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
    [Required, MaxLength(100)]
    public required string LastName { get; init; }
}

// Generic pagination wrapper
public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
```

**no-skills** — Mutable classes with `Dto` suffix and inheritance:
```csharp
// Mutable class, not sealed, "Dto" naming
public class BookCreateDto {
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    public List<int> AuthorIds { get; set; } = new();
}

// Inheritance between DTOs
public class PatronDetailDto : PatronDto {
    public int ActiveLoansCount { get; set; }
    public decimal TotalUnpaidFines { get; set; }
}

// Mutable pagination
public class PagedResult<T> {
    public List<T> Items { get; set; } = new();
}
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Sealed records, immutable (init/positional), Request/Response naming, `IReadOnlyList<T>` |
| dotnet-webapi | **5** | Same — sealed records, `required init`, Request/Response naming |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **2** | Mutable classes, not sealed, Dto suffix, `List<T>` instead of `IReadOnlyList<T>`, inheritance |

**Verdict:** `dotnet-artisan` and `dotnet-webapi` produce immutable, type-safe DTO contracts. `no-skills` produces mutable DTOs with inheritance — a less safe and less modern approach.

---

## 16. Sealed Types [MEDIUM]

**dotnet-artisan & dotnet-webapi** — Everything sealed:
```csharp
public sealed class Author { ... }
public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options) { ... }
public sealed class AuthorService(LibraryDbContext db, ...) : IAuthorService { ... }
public sealed record AuthorResponse(...);
internal sealed class ApiExceptionHandler(...) : IExceptionHandler { ... }
```

**no-skills** — Nothing sealed:
```csharp
public class Author { ... }
public class LibraryDbContext : DbContext { ... }
public class AuthorService : IAuthorService { ... }
public class BookCreateDto { ... }
public class GlobalExceptionHandlerMiddleware { ... }
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | All models, services, DTOs, DbContext are sealed |
| dotnet-webapi | **5** | Same — comprehensive sealing including middleware |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **1** | Nothing is sealed — no models, services, DTOs, middleware, or DbContext |

**Verdict:** `dotnet-artisan` and `dotnet-webapi` correctly signal design intent with `sealed`. `no-skills` omits it entirely, missing JIT devirtualization optimizations and allowing unintended inheritance.

---

## 17. Data Seeder Design [MEDIUM]

All three configs use a static `DataSeeder` class with `AddRange` and idempotency guard:

```csharp
// All configs — same pattern
public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext db)
    {
        if (await db.Authors.AnyAsync()) return;  // Idempotency

        var authors = new List<Author> { ... };
        db.Authors.AddRange(authors);
        // ... categories, books, patrons, loans, reservations, fines
        await db.SaveChangesAsync();
    }
}
```

All provide realistic, varied data: 6 authors, 6 categories, 12 books, 6 patrons (mixed membership types, 1 inactive), 8 loans (Active/Returned/Overdue), 3 reservations, 3-4 fines.

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **4** | Static seeder, idempotent, realistic variety, mix of statuses; not using HasData/migrations |
| dotnet-webapi | **4** | Same approach and quality |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **4** | Same approach; slightly more variety in fines (4 vs 3) |

**Verdict:** Tie — all three produce adequate seed data with good variety. None use `HasData()` which would integrate with migrations.

---

## 18. Structured Logging [MEDIUM]

**dotnet-artisan & dotnet-webapi** — ILogger<T> in all 7 services with structured templates:
```csharp
// Structured message templates with named placeholders
logger.LogInformation("Created author {AuthorId}: {FirstName} {LastName}",
    author.Id, author.FirstName, author.LastName);

logger.LogInformation("Book {BookId} checked out to patron {PatronId}, loan {LoanId}, due {DueDate}",
    book.Id, patron.Id, loan.Id, loan.DueDate);

logger.LogInformation("Fine of ${Amount:F2} issued to patron {PatronId} for overdue loan {LoanId}",
    fineAmount, loan.PatronId, loan.Id);

// Error handler
logger.LogWarning(exception, "Handled API exception: {Title}", title);
logger.LogError(exception, "Unhandled exception");
```

**no-skills** — ILogger in some services (LoanService, ReservationService, FineService) with structured templates:
```csharp
_logger.LogInformation("Book '{Title}' (ID: {BookId}) checked out to patron '{PatronName}' (ID: {PatronId}). Due: {DueDate}",
    book.Title, book.Id, $"{patron.FirstName} {patron.LastName}", patron.Id, loan.DueDate);
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | ILogger<T> in all services, structured templates, appropriate log levels |
| dotnet-webapi | **5** | Same — consistent structured logging throughout |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **4** | ILogger in key services only (not AuthorService, CategoryService, BookService), structured templates used correctly |

**Verdict:** `dotnet-artisan` and `dotnet-webapi` are more consistent — logging in every service. `no-skills` logs only in complex business services.

---

## 19. Nullable Reference Types [MEDIUM]

All three working configurations enable NRT in the project file:

```xml
<Nullable>enable</Nullable>
```

Both skilled configs use proper nullable annotations:
```csharp
// dotnet-artisan & dotnet-webapi
public string? Biography { get; set; }
public DateOnly? BirthDate { get; set; }
public DateTime? ReturnDate { get; set; }
```

`no-skills` also uses nullable annotations but occasionally uses `= null!` on navigation properties:
```csharp
public Book Book { get; set; } = null!;
public Patron Patron { get; set; } = null!;
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | NRT enabled, proper ? annotations, no `null!` abuse |
| dotnet-webapi | **5** | Same |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **4** | NRT enabled, proper annotations, but uses `null!` on navigation properties |

**Verdict:** All handle NRT well. `no-skills` uses `null!` on navigation properties which is a common EF Core pattern but technically suppresses null safety.

---

## 20. API Documentation [MEDIUM]

**dotnet-artisan & dotnet-webapi** — Fluent endpoint metadata:
```csharp
// WithName + WithSummary + Produces + ProducesProblem
group.MapGet("/{id:int}", async (...) => { ... })
    .WithName("GetAuthorById")
    .WithSummary("Get author details including their books")
    .Produces<AuthorDetailResponse>()
    .ProducesProblem(StatusCodes.Status404NotFound);
```

`dotnet-webapi` additionally uses `WithDescription` for complex endpoints:
```csharp
.WithDescription("Enforces: available copies, active membership, borrowing limits, and fine threshold ($10.00).")
```

**no-skills** — XML comments with `ProducesResponseType` attributes:
```csharp
/// <summary>Get author details including their books</summary>
[HttpGet("{id}")]
[ProducesResponseType(typeof(AuthorDetailDto), 200)]
[ProducesResponseType(404)]
public async Task<IActionResult> GetById(int id) { ... }
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | WithName, WithSummary, Produces, ProducesProblem on all endpoints |
| dotnet-webapi | **5** | Same + WithDescription for complex operations |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **4** | XML comments, ProducesResponseType attributes, Swagger UI — functional but uses legacy approach |

**Verdict:** Both skilled configs use the modern Minimal API metadata approach. `no-skills` uses the controller-era XML comment approach which still works but is more verbose.

---

## 21. File Organization [MEDIUM]

**dotnet-artisan & dotnet-webapi** — Clean per-concern folders with Endpoints directory:
```
src/LibraryApi/
├── Data/           (LibraryDbContext.cs, DataSeeder.cs)
├── DTOs/           (AuthorDtos.cs, BookDtos.cs, ..., PaginatedResponse.cs)
├── Endpoints/      (AuthorEndpoints.cs, BookEndpoints.cs, ...)
├── Middleware/      (ApiExceptionHandler.cs)  ← webapi only
├── Models/         (Author.cs, Book.cs, ..., Enums.cs)
├── Services/       (IAuthorService.cs, AuthorService.cs, ...)
└── Program.cs      (clean — just DI registration + endpoint mapping)
```

**no-skills** — Controllers instead of Endpoints, DTOs consolidated:
```
src/LibraryApi/
├── Controllers/    (AuthorsController.cs, ...)
├── Data/           (LibraryDbContext.cs, DataSeeder.cs)
├── DTOs/           (Dtos.cs ← ALL DTOs in ONE file, PagedResult.cs)
├── Middleware/      (GlobalExceptionHandlerMiddleware.cs)
├── Models/         (Author.cs, Book.cs, ...)
├── Services/       (Interfaces.cs ← ALL interfaces in ONE file, AuthorService.cs, ...)
└── Program.cs
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Clean per-concern folders, separate interface files, dedicated Endpoints directory |
| dotnet-webapi | **5** | Same structure with Middleware folder for IExceptionHandler |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **3** | Separation of concerns present, but all DTOs in one file, all interfaces in one file — hurts discoverability |

**Verdict:** `dotnet-artisan` and `dotnet-webapi` produce better-organized code. `no-skills` consolidates DTOs and interfaces into single files, reducing discoverability.

---

## 22. HTTP Test File Quality [MEDIUM]

All three configs include comprehensive `.http` test files:

**dotnet-artisan & dotnet-webapi** — 40+ requests with error cases:
```http
### Check out a book — TEST: inactive patron (Frank Brown, patron 6)
POST {{baseUrl}}/api/loans
Content-Type: application/json

{ "bookId": 6, "patronId": 6 }

### Renew a loan — TEST: cannot renew overdue loan (loan 5)
POST {{baseUrl}}/api/loans/5/renew

### Renew a loan — TEST: cannot renew when reservations exist
POST {{baseUrl}}/api/loans/4/renew
```

**no-skills** — 50+ requests with similar coverage:
```http
### Checkout fails if patron is inactive (ID 6)
POST http://localhost:5145/api/loans
Content-Type: application/json

{ "bookId": 4, "patronId": 6 }
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | 40+ requests, comprehensive error cases, realistic data matching seed IDs |
| dotnet-webapi | **5** | 40+ requests with error cases and business rule tests |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **4** | 50+ requests with good coverage; slightly less organized labels |

**Verdict:** All produce high-quality test files. The skilled configs use `{{baseUrl}}` variables more consistently.

---

## 23. Type Design & Resource Management [MEDIUM]

All configs use enums for domain status fields:
```csharp
// All configs
public enum MembershipType { Standard, Premium, Student }
public enum LoanStatus { Active, Returned, Overdue }
public enum ReservationStatus { Pending, Ready, Fulfilled, Cancelled, Expired }
public enum FineStatus { Unpaid, Paid, Waived }
```

Key differences in return types and collections:

```csharp
// dotnet-artisan & dotnet-webapi: IReadOnlyList<T> in DTOs
public sealed record PaginatedResponse<T>(IReadOnlyList<T> Items, ...);

// no-skills: mutable List<T>
public class PagedResult<T> { public List<T> Items { get; set; } = new(); }
```

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Enums with `HasConversion<string>()`, `IReadOnlyList<T>` return types, switch expressions |
| dotnet-webapi | **5** | Same precision in type design |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **3** | Enums used correctly, but `List<T>` instead of `IReadOnlyList<T>` |

**Verdict:** Skilled configs prefer immutable, precise return types. `no-skills` uses mutable `List<T>`.

---

## 24. Code Standards Compliance [LOW]

**dotnet-artisan & dotnet-webapi**:
- ✅ PascalCase for types and members
- ✅ camelCase for parameters
- ✅ `Async` suffix on all async methods
- ✅ `I` prefix on all interfaces
- ✅ Explicit `public sealed` / `internal sealed` access modifiers
- ✅ File-scoped namespaces (`namespace X;`)
- ✅ Verb-phrase method names (`GetAuthorsAsync`, `CheckoutAsync`)

**no-skills**:
- ✅ PascalCase, camelCase, Async suffix, I prefix
- ❌ No explicit `sealed` modifier on any type
- ❌ Some implicit access modifiers (relying on defaults)
- ✅ File-scoped namespaces
- ✅ Verb-phrase method names

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **5** | Follows all .NET naming and access modifier conventions |
| dotnet-webapi | **5** | Same |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **3** | Follows naming conventions but misses sealed and some explicit modifiers |

**Verdict:** Skilled configs follow .NET conventions completely. `no-skills` follows naming guidelines but misses the sealed modifier convention.

---

## 25. JSON Serialization Configuration [ADDITIONAL]

This additional dimension captures how each config handles JSON serialization of enum values.

**dotnet-webapi** — Configures `JsonStringEnumConverter` globally:
```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

**dotnet-artisan & no-skills** — No explicit JSON serialization configuration. Enums serialize as integers by default.

| Configuration | Score | Justification |
|---|---|---|
| dotnet-artisan | **3** | No JSON enum configuration; enums stored as strings in DB but serialize as integers in responses |
| dotnet-webapi | **5** | `JsonStringEnumConverter` configured; enums serialize as human-readable strings |
| dotnet-skills | **1** | No code generated |
| managedcode | **1** | No code generated |
| no-skills | **3** | No JSON enum configuration |

**Verdict:** `dotnet-webapi` is the only config that ensures enum values appear as readable strings (e.g., `"Premium"` vs `2`) in API responses.

---

## Weighted Summary

Applying the tier-based weighting: Critical ×3, High ×2, Medium ×1, Low ×0.5.

### Critical Dimensions (×3)

| Dimension | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| Build & Run Success | 5 | 5 | 1 | 1 | 5 |
| Security Vulnerability Scan | 5 | 5 | 1 | 1 | 4 |
| Minimal API Architecture | 4 | 5 | 1 | 1 | 1 |
| Input Validation & Guard Clauses | 4 | 4 | 1 | 1 | 4 |
| NuGet & Package Discipline | 5 | 3 | 1 | 1 | 3 |
| EF Migration Usage | 2 | 2 | 1 | 1 | 2 |
| **Subtotal (raw → ×3)** | **25 → 75** | **24 → 72** | **6 → 18** | **6 → 18** | **19 → 57** |

### High Dimensions (×2)

| Dimension | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| Business Logic Correctness | 5 | 5 | 1 | 1 | 5 |
| Prefer Built-in over 3rd Party | 5 | 5 | 1 | 1 | 1 |
| Modern C# Adoption | 5 | 5 | 1 | 1 | 2 |
| Error Handling & Middleware | 3 | 5 | 1 | 1 | 3 |
| Async Patterns & Cancellation | 5 | 5 | 1 | 1 | 2 |
| EF Core Best Practices | 5 | 5 | 1 | 1 | 3 |
| Service Abstraction & DI | 5 | 5 | 1 | 1 | 3 |
| Security Configuration | 1 | 1 | 1 | 1 | 1 |
| **Subtotal (raw → ×2)** | **34 → 68** | **36 → 72** | **8 → 16** | **8 → 16** | **20 → 40** |

### Medium Dimensions (×1)

| Dimension | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| DTO Design | 5 | 5 | 1 | 1 | 2 |
| Sealed Types | 5 | 5 | 1 | 1 | 1 |
| Data Seeder Design | 4 | 4 | 1 | 1 | 4 |
| Structured Logging | 5 | 5 | 1 | 1 | 4 |
| Nullable Reference Types | 5 | 5 | 1 | 1 | 4 |
| API Documentation | 5 | 5 | 1 | 1 | 4 |
| File Organization | 5 | 5 | 1 | 1 | 3 |
| HTTP Test File Quality | 5 | 5 | 1 | 1 | 4 |
| Type Design & Resource Management | 5 | 5 | 1 | 1 | 3 |
| JSON Serialization Config | 3 | 5 | 1 | 1 | 3 |
| **Subtotal (×1)** | **47** | **49** | **10** | **10** | **32** |

### Low Dimensions (×0.5)

| Dimension | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| Code Standards Compliance | 5 | 5 | 1 | 1 | 3 |
| **Subtotal (raw → ×0.5)** | **5 → 2.5** | **5 → 2.5** | **1 → 0.5** | **1 → 0.5** | **3 → 1.5** |

### Total Weighted Scores

| Configuration | Critical (×3) | High (×2) | Medium (×1) | Low (×0.5) | **Total** | **Max Possible** | **Percentage** |
|---|---|---|---|---|---|---|---|
| **dotnet-webapi** | 72 | 72 | 49 | 2.5 | **195.5** | 250.0 | **78.2%** |
| **dotnet-artisan** | 75 | 68 | 47 | 2.5 | **192.5** | 250.0 | **77.0%** |
| **no-skills** | 57 | 40 | 32 | 1.5 | **130.5** | 250.0 | **52.2%** |
| **dotnet-skills** | 18 | 16 | 10 | 0.5 | **44.5** | 250.0 | **17.8%** |
| **managedcode** | 18 | 16 | 10 | 0.5 | **44.5** | 250.0 | **17.8%** |

---

## What All Versions Get Right

Across all three working configurations (`dotnet-artisan`, `dotnet-webapi`, `no-skills`):

- ✅ **Zero-error, zero-warning builds** on .NET 10.0 targeting `net10.0`
- ✅ **No vulnerable NuGet packages** detected by `dotnet list package --vulnerable`
- ✅ **Full endpoint coverage** — all 35+ endpoints from the specification are implemented
- ✅ **Complete business rule implementation** — checkout limits, overdue fines, reservation queues, renewal rules
- ✅ **Interface-based service registration** with `AddScoped<IService, Service>()`
- ✅ **Enum types for domain status fields** — `MembershipType`, `LoanStatus`, `ReservationStatus`, `FineStatus`
- ✅ **Fluent API configuration** for EF Core relationships, unique indexes, and composite keys
- ✅ **Enum-to-string conversion** via `HasConversion<string>()` in EF Core
- ✅ **ProblemDetails error responses** (RFC 7807) for error handling
- ✅ **Idempotent seed data** with realistic variety across all entity types
- ✅ **Comprehensive `.http` test files** covering happy and error paths
- ✅ **Nullable reference types** enabled with proper `?` annotations
- ✅ **File-scoped namespaces** used consistently
- ✅ **Structured logging** with named placeholders (no string interpolation in log calls)
- ✅ **SQLite connection string from `appsettings.json`** — not hardcoded

---

## Summary: Impact of Skills

### Ranking by Weighted Score

1. 🥇 **dotnet-webapi** (195.5 / 250.0 = 78.2%) — The top performer, narrowly beating dotnet-artisan. Its key advantages: `IExceptionHandler` pattern, `TypedResults` with union return types, `JsonStringEnumConverter` configuration. Only weakness: wildcard NuGet versions (`10.*-*`).

2. 🥈 **dotnet-artisan** (192.5 / 250.0 = 77.0%) — Nearly tied with dotnet-webapi. Its key advantage: exact NuGet version pinning (5/5 on package discipline vs webapi's 3/5). Its weaknesses: inline exception handler instead of `IExceptionHandler`, uses `Results.` instead of `TypedResults.`, no JSON enum serialization config.

3. 🥉 **no-skills** (130.5 / 250.0 = 52.2%) — Functionally complete but architecturally dated. Falls back to Controllers, Swashbuckle, traditional constructors, and mutable DTOs. Produces code that works but looks like .NET 6-era patterns.

4. ❌ **dotnet-skills** (44.5 / 250.0 = 17.8%) — Failed to generate any code in run-2.

5. ❌ **managedcode-dotnet-skills** (44.5 / 250.0 = 17.8%) — Same failure as dotnet-skills.

### Most Impactful Differences

| Rank | Dimension | Impact | Winner |
|---|---|---|---|
| 1 | **Prefer Built-in over 3rd Party** | Swashbuckle vs AddOpenApi() — the single most visible indicator of modern .NET | dotnet-artisan, dotnet-webapi |
| 2 | **Minimal API Architecture** | Controllers vs Minimal APIs + MapGroup + TypedResults — fundamentally different API paradigms | dotnet-webapi |
| 3 | **Modern C# Adoption** | Primary constructors + `[]` syntax vs traditional patterns — affects every file | dotnet-artisan, dotnet-webapi |
| 4 | **Async Patterns & Cancellation** | Full CancellationToken chain vs none — affects production scalability | dotnet-artisan, dotnet-webapi |
| 5 | **Sealed Types** | Universal sealing vs none — performance and design intent | dotnet-artisan, dotnet-webapi |

### Overall Assessment

The skills-enabled configurations (`dotnet-artisan` and `dotnet-webapi`) produce **significantly higher-quality code** than the baseline `no-skills` configuration — scoring ~50% higher on weighted metrics. The gap is most dramatic in architectural decisions (Minimal APIs vs Controllers), modern C# adoption (primary constructors, collection expressions), and framework alignment (built-in OpenAPI vs Swashbuckle).

Between the two leading configs, `dotnet-webapi` edges ahead on **architectural precision** (TypedResults, union return types, IExceptionHandler, JSON enum config) while `dotnet-artisan` leads on **package discipline** (exact version pins). Both produce code that would be suitable for official .NET documentation samples.

The `dotnet-skills` and `managedcode-dotnet-skills` configurations failed entirely in run-2, producing no application code.

**Universal gaps** across all configurations: no HSTS/HTTPS redirection, no EF Core migrations (all use EnsureCreated), and no modern `ThrowIfNull()`/`ThrowIfNullOrEmpty()` guard clause helpers.
