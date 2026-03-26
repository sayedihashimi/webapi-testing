# Comparative Analysis: no-skills, dotnet-webapi, dotnet-artisan, managedcode-dotnet-skills, dotnet-skills

## Introduction

This report compares five Copilot skill configurations, each used to generate the same three ASP.NET Web API applications:

| Configuration | Description | Apps Generated |
|---|---|---|
| **no-skills** | Baseline — no skills attached | FitnessStudioApi, LibraryApi, VetClinicApi |
| **dotnet-webapi** | Single `dotnet-webapi` skill | FitnessStudioApi, LibraryApi, VetClinicApi |
| **dotnet-artisan** | Multi-skill plugin chain (`using-dotnet`, `dotnet-advisor`, `dotnet-csharp`, `dotnet-api`) | FitnessStudioApi, LibraryApi, VetClinicApi |
| **managedcode-dotnet-skills** | Community managed-code skills (`dotnet`, `dotnet-aspnet-core`, `dotnet-entity-framework-core`, `dotnet-modern-csharp`) | FitnessStudioApi, LibraryApi, VetClinicApi |
| **dotnet-skills** | Official .NET skills (`analyzing-dotnet-performance`, `optimizing-ef-core-queries`) | FitnessStudioApi, LibraryApi, VetClinicApi |

All projects target **.NET 10** with **C# 13/14**, use **EF Core with SQLite**, and implement domain-rich business rules (booking capacity, membership tiers, waitlist management, overdue fines, vaccination tracking).

---

## Executive Summary

| Dimension | no-skills | dotnet-webapi | dotnet-artisan | managedcode | dotnet-skills |
|---|---|---|---|---|---|
| **API Style** | ❌ Controllers | ✅ Minimal APIs | ✅/mixed Minimal + Controllers | ❌ Controllers | ❌ Controllers |
| **Sealed Types** | ❌ None | ✅ Everywhere | ✅ Partial (Fitness only) | ✅/mixed Some apps | ✅ Most classes |
| **Primary Constructors** | ❌ Traditional | ✅ Everywhere | ✅ Partial | ✅ Everywhere | ❌ Traditional |
| **DTO Design** | ❌ Mutable classes | ✅ Sealed records | ✅ Sealed records | ✅/mixed Records (not sealed) | ❌ Sealed classes (mutable) |
| **Service Abstraction** | ✅ Interface+Impl | ✅ Interface+Impl | ✅ Interface+Impl | ✅ Interface+Impl | ✅ Interface+Impl |
| **CancellationToken** | ❌ None | ✅/mixed Partial | ✅ Full propagation | ✅ Full propagation | ✅ Full propagation |
| **AsNoTracking** | ❌ None | ✅/mixed Inconsistent | ✅ Read queries | ✅ Read queries | ✅ Comprehensive |
| **Return Type Precision** | ✅/mixed IReadOnlyList partial | ✅ IReadOnlyList | ✅ IReadOnlyList | ✅ IReadOnlyList | ❌ List\<T\> |
| **Data Seeder** | ❌ Basic | ✅ HasData() | ✅ Async SeedAsync | ✅/mixed Sync seeder | ✅/mixed Sync seeder |
| **Middleware Style** | ❌ RequestDelegate | ✅ IExceptionHandler | ✅ IExceptionHandler | ✅ IExceptionHandler | ✅ IExceptionHandler |
| **Exception Handling** | ✅/mixed Custom classes + middleware | ✅ Built-in types | ✅ Custom BusinessRuleException | ✅ Custom + primary ctors | ✅ Custom + switch |
| **File Organization** | ❌ Single Dtos.cs | ✅ Per-entity files | ✅/mixed Mixed | ✅/mixed Per-entity files | ✅ Per-entity folders |
| **Pagination** | ❌ Mutable class | ✅ Sealed record + factory | ✅ Sealed record + computed | ✅/mixed Record | ❌ Mutable class |
| **OpenAPI Metadata** | ❌ Minimal | ✅ Full (Name+Summary+Desc+Produces) | ✅/mixed WithTags+WithSummary | ❌ Controller-only | ❌ Controller-only |
| **Collection Init** | ❌ new List\<T\>() | ✅ [] syntax | ✅ [] syntax | ✅ [] syntax | ❌ new List\<T\>() |
| **Structured Logging** | ✅ Templates | ✅ Templates | ✅ Templates | ✅ Templates | ✅ Templates |
| **Nullable Refs** | ✅ Enabled | ✅ Enabled | ✅ Enabled | ✅ Enabled | ✅ Enabled |
| **Global Usings** | ❌ None | ✅ GlobalUsings.cs | ❌ Implicit only | ❌ Implicit only | ❌ Implicit only |
| **Package Discipline** | ❌ Swashbuckle redundancy | ✅ Clean (built-in OpenAPI) | ❌ Swashbuckle + OpenAPI | ❌ Swashbuckle + OpenAPI | ✅/mixed Mostly clean |
| **EF Core Config** | ✅ Explicit Restrict | ✅/mixed Partial HasConversion | ⚠️ Cascade deletes | ✅ Explicit Restrict | ✅ Restrict + timestamps |
| **TypedResults** | N/A (Controllers) | ✅ Full + union types | ✅/mixed Mixed TypedResults/Results | N/A (Controllers) | ❌ Results only |
| **Route Groups** | N/A | ✅ MapGroup + extensions | ✅ MapGroup + extensions | N/A | ❌ Inline in Program.cs |
| **HTTP Test Files** | ✅ Comprehensive | ✅ Comprehensive + business tests | ✅ Comprehensive | ✅ Comprehensive | ✅ Comprehensive |
| **Naming Conventions** | ✅ Consistent | ✅ Request/Response naming | ✅ Request/Response naming | ✅ Dto naming | ✅ Dto naming |
| **Enum Design** | ✅ Enums (int storage) | ✅ Enums + HasConversion\<string\> | ✅ Enums (int storage) | ✅ Enums (int storage) | ✅ Enums (separate files) |
| **Guard Clauses** | ❌ No modern guards | ❌ No modern guards | ❌ No modern guards | ❌ No modern guards | ❌ No modern guards |
| **Async/Await** | ✅ Correct | ✅ Correct | ✅ Correct | ✅ Correct | ✅ Correct |
| **Access Modifiers** | ❌ Missing on some | ✅ Explicit everywhere | ✅ Explicit + sealed | ✅ Explicit + sealed | ✅ Explicit + sealed |
| **File-scoped NS** | ✅ Everywhere | ✅ Everywhere | ✅ Everywhere | ✅ Everywhere | ✅ Everywhere |
| **Dispose/Resources** | ✅ Scoped DI | ✅ Scoped DI | ✅ Scoped DI | ✅ Scoped DI | ✅ Scoped DI |

---

## 1. API Style

The most fundamental architectural difference across configurations.

**no-skills** and **managedcode-dotnet-skills** — all apps use traditional MVC controllers:

```csharp
// no-skills: FitnessStudioApi/Controllers/MembersController.cs
[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _service;
    public MembersController(IMemberService service) { _service = service; }
}
```

**dotnet-webapi** — all apps use Minimal APIs with route groups and extension methods:

```csharp
// dotnet-webapi: FitnessStudioApi/Endpoints/MemberEndpoints.cs
public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/members").WithTags("Members");

        group.MapGet("/", async (string? search, bool? isActive, int? page, int? pageSize,
            IMemberService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAllAsync(search, isActive, p, ps, ct)))
            .WithName("GetMembers")
            .WithSummary("List members");
    }
}
```

**dotnet-artisan** — mixed: FitnessStudioApi and VetClinicApi use Minimal APIs; LibraryApi uses controllers.

**dotnet-skills** — mostly controllers, except VetClinicApi which inlines endpoints directly in Program.cs (an anti-pattern for maintainability).

**Verdict**: **dotnet-webapi** wins. Minimal APIs with MapGroup extension methods produce the cleanest, most modern code. They are the recommended approach since .NET 6 and provide better OpenAPI integration, lower overhead, and more concise endpoint definitions.

---

## 2. Sealed Types

Sealing classes signals design intent and enables JIT devirtualization optimizations.

| Configuration | Services Sealed | Controllers Sealed | DTOs Sealed |
|---|---|---|---|
| no-skills | ❌ No | ❌ No | ❌ No |
| dotnet-webapi | ✅ All 7 | N/A (Minimal APIs) | ✅ All sealed records |
| dotnet-artisan | ✅ FitnessStudio | ❌ LibraryApi | ✅ FitnessStudio |
| managedcode-dotnet-skills | ✅/mixed Some | ❌ Not consistent | ❌ Records not sealed |
| dotnet-skills | ✅ All | ✅ All controllers | ✅ Sealed classes |

```csharp
// dotnet-webapi: Services/MemberService.cs — sealed + primary constructor
public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    : IMemberService

// no-skills: Services/MemberService.cs — not sealed, traditional constructor
public class MemberService : IMemberService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<MemberService> _logger;
```

**Verdict**: **dotnet-webapi** and **dotnet-skills** are best. Both consistently seal service classes, which is important for concrete DI-registered types that should never be subclassed.

---

## 3. Primary Constructors

C# 12 primary constructors eliminate constructor boilerplate for dependency injection.

```csharp
// dotnet-webapi: 3 lines — primary constructor
public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    : IMemberService
{
    public async Task<PaginatedResponse<MemberResponse>> GetAllAsync(...) =>
        // db and logger are directly accessible
}

// no-skills / dotnet-skills: 10 lines — traditional constructor
public sealed class MemberService : IMemberService
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

| Configuration | Primary Constructors |
|---|---|
| no-skills | ❌ Traditional everywhere |
| dotnet-webapi | ✅ Everywhere |
| dotnet-artisan | ✅ FitnessStudio + VetClinic |
| managedcode-dotnet-skills | ✅ Everywhere |
| dotnet-skills | ❌ Traditional everywhere |

**Verdict**: **dotnet-webapi** and **managedcode-dotnet-skills** are best. Primary constructors save ~7 lines per class with no functional trade-off. **dotnet-skills** surprisingly regresses to traditional constructors despite targeting the latest C#.

---

## 4. DTO Design

DTOs vary significantly in type choice, immutability, and naming.

```csharp
// dotnet-webapi: Sealed record with init — best practice
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
}
public sealed record MemberResponse(
    int Id, string FirstName, string LastName, string Email,
    DateOnly JoinDate, bool IsActive, DateTime CreatedAt);

// no-skills: Mutable class with setters — worst practice
public class MemberCreateDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
}

// dotnet-skills: Sealed class with mutable setters — mixed
public sealed class CreateMemberDto
{
    public string FirstName { get; set; } = string.Empty;
}
```

| Config | Type | Sealed | Immutable | Naming |
|---|---|---|---|---|
| no-skills | Class | ❌ | ❌ Mutable setters | `*Dto` |
| dotnet-webapi | Record | ✅ | ✅ init/positional | `*Request/*Response` |
| dotnet-artisan | Record | ✅ | ✅ init/positional | `*Request/*Response` |
| managedcode | Record | ❌ | ✅ init/positional | `*Dto` |
| dotnet-skills | Class | ✅ | ❌ Mutable setters | `*Dto` |

**Verdict**: **dotnet-webapi** and **dotnet-artisan** are best. Sealed records with `init` properties provide compile-time immutability, value equality, and clear API contract naming (`Request`/`Response` > `Dto`).

---

## 5. Service Abstraction

All five configurations use the interface + implementation pattern with scoped DI registration:

```csharp
// All configurations register services identically
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IClassScheduleService, ClassScheduleService>();
```

**Verdict**: Tie — all configurations correctly follow dependency inversion with `AddScoped<IService, Implementation>()`.

---

## 6. CancellationToken Propagation

Critical for production APIs — prevents wasted server resources on cancelled requests.

```csharp
// dotnet-artisan: Full chain — endpoint → service → EF Core
group.MapGet("/", async (IMemberService service, CancellationToken ct, ...) =>
    TypedResults.Ok(await service.GetAllAsync(search, isActive, page, pageSize, ct)))

// Service layer propagates CT to every EF Core call
public async Task<PaginatedResponse<MemberListResponse>> GetAllAsync(
    string? search, bool? isActive, int page, int pageSize, CancellationToken ct = default)
{
    var totalCount = await query.CountAsync(ct);       // ✅ CT propagated
    var items = await query.ToListAsync(ct);            // ✅ CT propagated
}

// no-skills: No CancellationToken anywhere
public async Task<PagedResult<MemberListDto>> GetAllAsync(
    string? search, bool? isActive, int page, int pageSize)
{
    var totalCount = await query.CountAsync();          // ❌ No CT
    var items = await query.ToListAsync();              // ❌ No CT
}
```

| Configuration | Propagation |
|---|---|
| no-skills | ❌ None — no CT in any layer |
| dotnet-webapi | ✅/mixed — FitnessStudio + LibraryApi have CT; VetClinic partial |
| dotnet-artisan | ✅ Full — endpoint through EF Core |
| managedcode-dotnet-skills | ✅ Full — endpoint through EF Core |
| dotnet-skills | ✅ Full — endpoint through EF Core |

**Verdict**: **dotnet-artisan**, **managedcode-dotnet-skills**, and **dotnet-skills** are best with complete end-to-end propagation. **no-skills** is the worst — every cancelled HTTP request wastes database resources.

---

## 7. AsNoTracking Usage

Disabling change tracking on read-only queries prevents unnecessary memory allocation.

```csharp
// dotnet-skills: Consistent AsNoTracking on all reads (14 occurrences)
public async Task<PagedResult<BookListResponse>> GetBooksAsync(...)
{
    var query = context.Books
        .AsNoTracking()                    // ✅ Read-only — no tracking needed
        .AsQueryable();
    var items = await query.Select(b => new BookListResponse { ... }).ToListAsync();
}

// no-skills: No AsNoTracking anywhere
var query = _context.Members.AsQueryable();  // ❌ Tracking enabled by default
```

| Configuration | AsNoTracking Count | Consistency |
|---|---|---|
| no-skills | 0 | ❌ Never used |
| dotnet-webapi | ~6 | ✅/mixed Inconsistent across apps |
| dotnet-artisan | ~4 | ✅ Read queries only |
| managedcode-dotnet-skills | ~4 | ✅ Read queries |
| dotnet-skills | ~14 | ✅ Most comprehensive |

**Verdict**: **dotnet-skills** is best with 14 consistent uses across all read paths. This is one area where the performance-focused skills clearly outperform.

---

## 8. Return Type Precision

`IReadOnlyList<T>` prevents accidental mutation by consumers while maintaining indexing.

```csharp
// dotnet-webapi: IReadOnlyList<T> on pagination response
public sealed record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
}

// no-skills: Mutable List<T>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
}
```

| Configuration | Collection Return Type |
|---|---|
| no-skills | `List<T>` (mutable) |
| dotnet-webapi | `IReadOnlyList<T>` |
| dotnet-artisan | `IReadOnlyList<T>` |
| managedcode-dotnet-skills | `IReadOnlyList<T>` |
| dotnet-skills | `List<T>` (mutable) |

**Verdict**: **dotnet-webapi**, **dotnet-artisan**, and **managedcode-dotnet-skills** correctly use `IReadOnlyList<T>`. **dotnet-skills** surprisingly uses mutable `List<T>` despite its performance focus.

---

## 9. Data Seeder Design

How seed data is created affects migration compatibility and startup behavior.

```csharp
// dotnet-artisan: Async seeder with idempotency check
public static async Task SeedAsync(FitnessDbContext db)
{
    if (await db.MembershipPlans.AnyAsync()) return;  // ✅ Idempotent
    db.MembershipPlans.AddRange(basic, premium);
    await db.SaveChangesAsync();                       // ✅ Single async save
}

// dotnet-webapi (VetClinicApi): HasData in OnModelCreating — migration-integrated
private static void SeedData(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Owner>().HasData(
        new Owner { Id = 1, FirstName = "Sarah", ... });
}
```

| Configuration | Approach | Async | Migration-safe |
|---|---|---|---|
| no-skills | Static/HasData mix | ❌ | ✅/mixed |
| dotnet-webapi | HasData() + runtime seeders | N/A | ✅ |
| dotnet-artisan | Static async method | ✅ | ❌ |
| managedcode-dotnet-skills | Static sync method | ❌ | ❌ |
| dotnet-skills | Static sync method | ❌ | ❌ |

**Verdict**: **dotnet-webapi**'s `HasData()` approach integrates with EF Core migrations for reproducible seeding. **dotnet-artisan**'s async runtime seeder is best for flexibility and non-blocking startup.

---

## 10. Middleware Style

Exception handling middleware architecture.

```csharp
// dotnet-webapi: Modern IExceptionHandler (sealed, primary constructor, pattern matching)
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
        // ...
    }
}

// no-skills: Legacy RequestDelegate middleware
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (BusinessRuleException ex) { /* manual ProblemDetails */ }
    }
}
```

**Verdict**: **dotnet-webapi** is best — `IExceptionHandler` is composable, DI-aware, and the recommended .NET 8+ approach. The `internal sealed` access modifier and pattern matching are additional quality signals. **no-skills** uses the legacy `RequestDelegate` pattern.

---

## 11. Exception Handling Strategy

```csharp
// dotnet-webapi: Uses built-in .NET exception types — no custom classes needed
throw new KeyNotFoundException($"Member {id} not found.");        // → 404
throw new ArgumentException("Inactive members cannot book.");     // → 400
throw new InvalidOperationException("Booking overlaps.");         // → 409

// dotnet-artisan: Custom exception with embedded HTTP semantics
public sealed class BusinessRuleException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }
    public BusinessRuleException(string message, int statusCode = 400,
        string title = "Business Rule Violation") : base(message) { ... }
}

// managedcode-dotnet-skills: Concise primary-constructor exceptions
public class BusinessRuleException(string message) : Exception(message);
public class ConflictException(string message) : Exception(message);
public class NotFoundException(string message) : Exception(message);
```

| Configuration | Strategy | Custom Types |
|---|---|---|
| no-skills | Custom exceptions + manual middleware | 3 classes (non-sealed) |
| dotnet-webapi | Built-in .NET types only | None |
| dotnet-artisan | BusinessRuleException with StatusCode property | 1 sealed class |
| managedcode-dotnet-skills | Custom exceptions with primary constructors | 3 classes |
| dotnet-skills | BusinessRuleException + KeyNotFoundException | 1 custom + built-in |

**Verdict**: **dotnet-artisan**'s approach is most sophisticated — a single `BusinessRuleException` with embedded `StatusCode` and `Title` properties centralizes HTTP mapping. **dotnet-webapi**'s built-in types are simpler but less expressive for domain-specific errors.

---

## 12. File Organization

```
// dotnet-webapi: Per-entity DTO files, Endpoints/ folder
FitnessStudioApi/
├── Endpoints/     (MemberEndpoints.cs, BookingEndpoints.cs, ...)
├── DTOs/          (MemberDtos.cs, BookingDtos.cs, ...)
├── Models/        (Member.cs, Enums.cs)
├── Services/      (IMemberService.cs, MemberService.cs)
├── Data/          (FitnessDbContext.cs, DataSeeder.cs)
└── Middleware/     (ApiExceptionHandler.cs)

// no-skills: Single mega-file DTOs
DTOs/
└── Dtos.cs        (ALL DTOs in one file — hard to navigate)

// dotnet-skills: Per-entity DTO folders (most organized)
DTOs/
├── Booking/       (BookingDtos.cs)
├── Member/        (MemberDtos.cs)
├── Membership/    (MembershipDtos.cs)
└── ...
```

**Verdict**: **dotnet-skills** has the most organized structure with per-entity DTO folders. **dotnet-webapi** balances organization with simplicity using per-entity files. **no-skills** stuffing all DTOs into one file is the worst approach.

---

## 13. Pagination

```csharp
// dotnet-webapi: Sealed record with factory method — best practice
public sealed record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int TotalPages { get; init; }
    public required bool HasNextPage { get; init; }
    public required bool HasPreviousPage { get; init; }

    public static PaginatedResponse<T> Create(
        IReadOnlyList<T> items, int page, int pageSize, int totalCount) { ... }
}

// dotnet-artisan: Sealed record with computed properties
public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

// no-skills: Mutable class — worst practice
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
```

**Verdict**: **dotnet-artisan** is best — positional sealed record with computed properties is the most concise and correct. **dotnet-webapi** is close with its factory method. **no-skills** uses a mutable class with no computed properties.

---

## 14. OpenAPI Metadata

```csharp
// dotnet-webapi: Full endpoint metadata — all four decorators
group.MapGet("/", handler)
    .WithName("GetMembers")
    .WithSummary("List members")
    .WithDescription("List members with optional search, filter, and pagination.")
    .Produces<PaginatedResponse<MemberResponse>>();

group.MapDelete("/{id:int}", handler)
    .WithName("DeleteBook")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status409Conflict);

// dotnet-artisan: Minimal metadata — only summary
group.MapGet("/", GetAll).WithSummary("List all veterinarians with filters");

// Controller configs: Rely on [ProducesResponseType] attributes
[ProducesResponseType<PaginatedResponse<MemberListDto>>(StatusCodes.Status200OK)]
public async Task<IActionResult> GetAll(...) { ... }
```

**Verdict**: **dotnet-webapi** is best with comprehensive `WithName`, `WithSummary`, `WithDescription`, and `Produces<T>` on every endpoint. This generates the richest OpenAPI documentation and enables better client generation.

---

## 15. Collection Initialization

```csharp
// dotnet-webapi / dotnet-artisan / managedcode: Modern C# 12 collection expressions
public ICollection<Membership> Memberships { get; set; } = [];
public ICollection<Booking> Bookings { get; set; } = [];

// no-skills / dotnet-skills: Legacy pattern
public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
```

**Verdict**: **dotnet-webapi**, **dotnet-artisan**, and **managedcode-dotnet-skills** correctly use `[]` syntax. **no-skills** and **dotnet-skills** use the verbose `new List<T>()` pattern.

---

## 16. Structured Logging

All five configurations use structured message templates with `ILogger<T>`:

```csharp
// All configs: Structured templates with named placeholders
logger.LogInformation("Membership {MembershipId} created for member {MemberId}", membership.Id, member.Id);
_logger.LogWarning("Business rule violation: {Message}", ex.Message);
_logger.LogError(ex, "An unhandled exception occurred");
```

No configuration uses `[LoggerMessage]` source generators for high-performance logging. String interpolation in log calls is avoided across all configurations.

**Verdict**: Tie — all configurations follow structured logging best practices. None achieves the highest tier with `[LoggerMessage]` source generators.

---

## 17. Nullable Reference Types

All 15 projects enable NRT in their `.csproj`:

```xml
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

All configurations properly annotate optional properties with `?`:

```csharp
public string? Specialization { get; set; }     // Optional field
public DateTime? CancellationDate { get; set; }  // Nullable datetime
public string? CancellationReason { get; set; }  // Nullable string
```

**Verdict**: Tie — all configurations correctly enable and use nullable reference types.

---

## 18. Global Using Directives

Only **dotnet-webapi** creates explicit `GlobalUsings.cs` files:

```csharp
// dotnet-webapi/LibraryApi/GlobalUsings.cs
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Mvc;

// dotnet-webapi/VetClinicApi/GlobalUsings.cs
global using Microsoft.AspNetCore.Http.HttpResults;
```

All configurations rely on `<ImplicitUsings>enable</ImplicitUsings>` for framework namespaces.

**Verdict**: **dotnet-webapi** is best — explicit `GlobalUsings.cs` for frequently-used namespaces like `HttpResults` reduces per-file boilerplate.

---

## 19. Package Discipline

```xml
<!-- dotnet-webapi: Clean — built-in OpenAPI only -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />

<!-- no-skills / dotnet-artisan / managedcode: Redundant Swashbuckle -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.6" />  <!-- ❌ Redundant -->
```

| Configuration | Swashbuckle Redundancy | Extra Packages |
|---|---|---|
| no-skills | ❌ All 3 apps | None |
| dotnet-webapi | ✅ None | None |
| dotnet-artisan | ❌ All 3 apps | SwaggerUI |
| managedcode-dotnet-skills | ❌ All 3 apps | FluentValidation |
| dotnet-skills | ✅/mixed 1 app has it | FluentValidation |

**Verdict**: **dotnet-webapi** is best — no unnecessary packages. .NET 9+ has built-in OpenAPI via `Microsoft.AspNetCore.OpenApi`, making Swashbuckle redundant.

---

## 20. EF Core Relationship Configuration

```csharp
// no-skills: Explicit Restrict everywhere — safe
entity.HasOne(e => e.Member)
    .WithMany(m => m.Memberships)
    .HasForeignKey(e => e.MemberId)
    .OnDelete(DeleteBehavior.Restrict);  // ✅ Must cancel memberships first

// dotnet-artisan: Cascade deletes — potentially dangerous
entity.HasOne(b => b.Member)
    .WithMany(m => m.Bookings)
    .HasForeignKey(b => b.MemberId)
    .OnDelete(DeleteBehavior.Cascade);   // ⚠️ Silent data loss

// dotnet-skills: Restrict + automatic timestamp management
public override Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    UpdateTimestamps();  // ✅ Auto-set UpdatedAt via ChangeTracker
    return base.SaveChangesAsync(ct);
}
```

**Verdict**: **dotnet-skills** is best overall with explicit `Restrict` behavior plus automatic timestamp management via `SaveChangesAsync` override. **dotnet-artisan** is concerning with cascade deletes that bypass business logic. **dotnet-webapi** is notable for `HasConversion<string>()` on enums.

---

## 21. TypedResults Usage

```csharp
// dotnet-webapi: TypedResults with union return types — compile-time safety
group.MapGet("/{id:int}",
    async Task<Results<Ok<MemberResponse>, NotFound>> (int id, IMemberService service, CancellationToken ct) =>
{
    var member = await service.GetByIdAsync(id, ct);
    return member is null ? TypedResults.NotFound() : TypedResults.Ok(member);
});

// dotnet-artisan: Mixed — TypedResults for some, Results for others
group.MapGet("/", async (...) =>
    TypedResults.Ok(await service.GetAllAsync(...)));  // ✅ TypedResults
group.MapGet("/{id:int}", async (...) =>
    member is not null ? Results.Ok(member) : Results.NotFound());  // ❌ Results

// dotnet-skills (VetClinic inline): Results only
return Results.Ok(await service.GetAllAsync(search, pagination));
```

**Verdict**: **dotnet-webapi** is best — `TypedResults` with `Results<T1, T2>` union return types provide compile-time safety and automatic OpenAPI schema generation without manual `Produces<T>` annotations.

---

## 22. Route Group Organization

```csharp
// dotnet-webapi: Extension methods keep Program.cs clean
// Program.cs
app.MapMemberEndpoints();
app.MapBookingEndpoints();

// MemberEndpoints.cs — isolated, testable
public static void MapMemberEndpoints(this WebApplication app)
{
    var group = app.MapGroup("/api/members").WithTags("Members");
    group.MapGet("/", handler);
    group.MapPost("/", handler);
}

// dotnet-skills (VetClinic): Inline in Program.cs — anti-pattern
var owners = app.MapGroup("/api/owners").WithTags("Owners");
owners.MapGet("/", async (IOwnerService service, ...) => { ... });
owners.MapPost("/", async (IOwnerService service, ...) => { ... });
// 100+ lines of endpoint definitions in Program.cs
```

**Verdict**: **dotnet-webapi** and **dotnet-artisan** use extension methods properly, keeping `Program.cs` under 70 lines. **dotnet-skills** VetClinicApi inlines all endpoints in Program.cs — manageable for small APIs but doesn't scale.

---

## 23. HTTP Test File Quality

All configurations generate comprehensive `.http` files with proper request ordering:

```http
# dotnet-webapi/FitnessStudioApi.http — 348 lines, full coverage
@baseUrl = http://localhost:5176

### List all active membership plans
GET {{baseUrl}}/api/membership-plans

### Create a new membership plan
POST {{baseUrl}}/api/membership-plans
Content-Type: application/json

{ "name": "Student", "durationMonths": 1, "price": 19.99, ... }

### Business Rule Test: Basic member booking premium class (should fail 400)
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{ "classScheduleId": 9, "memberId": 3 }
```

All configurations maintain correct FK ordering (plans → members → memberships → bookings) and include negative test cases.

**Verdict**: Tie — all configurations produce high-quality `.http` files. **dotnet-webapi** is slightly ahead with explicit business rule test labels and error case documentation.

---

## 24. Naming Conventions

All configurations follow .NET naming guidelines consistently:

| Aspect | All Configurations |
|---|---|
| Async suffix | ✅ `GetAllAsync`, `CreateAsync`, `DeleteAsync` |
| Interface prefix | ✅ `IMemberService`, `IBookingService` |
| Method verbs | ✅ `Create`, `Update`, `Delete`, `CheckIn`, `MarkNoShow` |
| PascalCase types | ✅ `MemberService`, `BookingResponse` |
| camelCase params | ✅ `int page`, `string? search` |

The only naming difference is DTO conventions: **dotnet-webapi** and **dotnet-artisan** use `*Request`/`*Response`, while the others use `*Dto`.

**Verdict**: **dotnet-webapi** and **dotnet-artisan** — `Request`/`Response` naming better communicates the direction of data flow than the generic `Dto` suffix.

---

## 25. Enum Design

All configurations use proper enums for domain status fields (no magic strings):

```csharp
// dotnet-webapi: Single-line compact enums + HasConversion<string>
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }

// In DbContext:
entity.Property(e => e.Status).HasConversion<string>();  // ✅ Human-readable DB values

// dotnet-skills: Separate enum files in Enums/ folder
// Models/Enums/BookingStatus.cs
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }
```

Only **dotnet-webapi** configures `HasConversion<string>()` for human-readable enum storage. All others store enums as integers by default.

**Verdict**: **dotnet-webapi** — string enum storage makes database debugging and reporting easier. **dotnet-skills** has the best file organization with per-enum files.

---

## 26. Guard Clauses & Argument Validation

No configuration uses modern .NET guard helpers:

```csharp
// What all configurations do — null-coalescing throw pattern
var member = await _context.Members.FindAsync(dto.MemberId)
    ?? throw new NotFoundException($"Member with ID {dto.MemberId} not found.");

// What none do — modern guard clauses
ArgumentNullException.ThrowIfNull(request);
ArgumentException.ThrowIfNullOrEmpty(request.FirstName);
```

All configurations use `?? throw` for entity lookup validation and `if` checks for business rule validation. No configuration uses `ArgumentNullException.ThrowIfNull()` or similar modern guard APIs.

**Verdict**: All configurations miss an opportunity to use modern guard clause APIs on constructor parameters and public method arguments.

---

## 27. Async/Await Best Practices

All configurations follow async best practices:

- ✅ `Async` suffix on all async methods
- ✅ No `async void` methods found
- ✅ No sync-over-async anti-patterns (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`)
- ✅ Consistent `await` through the call chain

**Verdict**: Tie — all configurations handle async patterns correctly.

---

## 28. Access Modifier Explicitness

```csharp
// dotnet-webapi: Explicit sealed + internal for handler
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler

// dotnet-webapi: Explicit public sealed for services
public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    : IMemberService

// no-skills: Sometimes missing access modifier
class MemberService : IMemberService  // ❌ Implicit internal
```

All configurations use file-scoped namespaces (`namespace X;`). **no-skills** occasionally omits access modifiers; all skill-enhanced configurations are explicit.

**Verdict**: **dotnet-webapi** is best — `internal sealed` on exception handlers and `public sealed` on services demonstrate intentional access control.

---

## 29. Dispose & Resource Management

All configurations properly manage resources through DI:

```csharp
// All configs: Scoped DbContext + scoped services
builder.Services.AddDbContext<FitnessDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<IMemberService, MemberService>();

// Startup scope for seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
    await context.Database.EnsureCreatedAsync();
}
```

No manual `DbContext.Dispose()` calls, no `IHttpClientFactory` usage needed, no file operations requiring explicit disposal.

**Verdict**: Tie — all configurations correctly rely on DI-managed lifetimes.

---

## What All Versions Get Right

- ✅ **Interface-based service abstraction** with scoped DI registration
- ✅ **Structured logging** with `ILogger<T>` and named template placeholders
- ✅ **Nullable reference types** enabled project-wide
- ✅ **File-scoped namespaces** everywhere
- ✅ **Correct async/await patterns** — no sync-over-async, proper suffixes
- ✅ **Enum types for domain status fields** — no magic strings or integers
- ✅ **Consistent .NET naming conventions** — PascalCase types, camelCase params, Async suffixes, I-prefixed interfaces
- ✅ **Comprehensive seed data** covering multiple entity states (Active, Expired, Cancelled, Waitlisted)
- ✅ **High-quality `.http` test files** with correct FK ordering and business rule test cases
- ✅ **Scoped DbContext lifetime** matching service lifetimes
- ✅ **ProblemDetails (RFC 7807)** responses for error handling
- ✅ **Implicit usings** enabled via `.csproj`

---

## Summary: Impact of Skills

### Ranking by Overall Code Quality

1. **🥇 dotnet-webapi** — The most consistently modern and well-crafted output across all dimensions. Minimal APIs, sealed records, TypedResults with union return types, rich OpenAPI metadata, clean package discipline, and primary constructors. The single-skill approach produces remarkably focused results.

2. **🥈 dotnet-artisan** — Strong runner-up with excellent CancellationToken propagation, sealed record DTOs, computed pagination properties, and async data seeding. Slightly inconsistent (mixed Minimal API/Controller across apps, mixed TypedResults/Results, cascade delete concerns). The multi-skill orchestration works well but introduces variability.

3. **🥉 managedcode-dotnet-skills** — Good on fundamentals: primary constructors, records for DTOs, CancellationToken propagation, IExceptionHandler. Falls short on sealing types, uses Swashbuckle redundantly, and sticks with controllers. A solid middle ground.

4. **4th: dotnet-skills** — Excels at performance-specific patterns (most AsNoTracking usage, sealed classes, SaveChangesAsync timestamp automation) but regresses on modern C# patterns: no primary constructors, mutable DTO classes, `new List<T>()` syntax, and inlined VetClinicApi endpoints. The performance-focused skills improve specific dimensions while missing broader code quality.

5. **5th: no-skills** — Baseline demonstrates what unguided Copilot produces: functional but outdated patterns. No sealed types, no CancellationToken, no AsNoTracking, mutable class DTOs, legacy RequestDelegate middleware, all DTOs in one file, and `new List<T>()` initialization. Every skill configuration measurably improves upon this baseline.

### Most Impactful Differences

| Rank | Dimension | Impact | Biggest Gap |
|---|---|---|---|
| 1 | **CancellationToken Propagation** | 🔴 Production reliability | no-skills (none) vs skills (full chain) |
| 2 | **AsNoTracking Usage** | 🔴 Memory & performance | no-skills (0) vs dotnet-skills (14 uses) |
| 3 | **IExceptionHandler vs RequestDelegate** | 🟡 Architecture modernity | no-skills (legacy) vs all skills (modern) |
| 4 | **Sealed Types** | 🟡 JIT optimization & design intent | no-skills (none) vs dotnet-webapi/skills (all) |
| 5 | **DTO Immutability** | 🟡 API safety | no-skills (mutable) vs dotnet-webapi (sealed records) |
| 6 | **TypedResults + Union Types** | 🟡 Type safety & OpenAPI | Controllers vs dotnet-webapi (TypedResults) |
| 7 | **Package Discipline** | 🟢 Maintenance burden | 11/15 apps have Swashbuckle redundancy |
| 8 | **Primary Constructors** | 🟢 Code conciseness | ~7 lines saved per class |

### Key Takeaway

**Skills consistently improve code quality**, with each skill configuration targeting different strengths. The `dotnet-webapi` skill produces the most holistically modern code. For production APIs, the combination of CancellationToken propagation (from dotnet-artisan/managedcode/dotnet-skills) with the modern API patterns (from dotnet-webapi) would represent the ideal output.
