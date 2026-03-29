# Comparative Analysis: dotnet-artisan, dotnet-webapi, managedcode-dotnet-skills, dotnet-skills, no-skills

## Introduction

This report compares **5 Copilot skill configurations**, each generating the same **LibraryApi** application — a community library management system with book loans, reservations, overdue fines, and patron management. Only one scenario (LibraryApi) was present in run-2 across all configurations.

| Configuration | Skill Description | Architecture |
|---|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain (using-dotnet → dotnet-advisor → dotnet-csharp + dotnet-api) | Controllers |
| **dotnet-webapi** | dotnet-webapi skill (single opinionated skill) | Minimal APIs |
| **dotnet-skills** | Official .NET Skills — optimizing-ef-core-queries + analyzing-dotnet-performance | Controllers |
| **managedcode-dotnet-skills** | Community managed-code skills (dotnet router → dotnet-webapi + dotnet-entity-framework-core + dotnet-aspnet-core + dotnet-modern-csharp + dotnet-project-setup) | Minimal APIs |
| **no-skills** | Baseline (default Copilot, no skills) | Controllers |

All five configurations target **.NET 10** with **EF Core + SQLite**, implement **7 resource controllers/endpoint groups** (Authors, Categories, Books, Patrons, Loans, Reservations, Fines), and include comprehensive business logic for checkout, return, renewal, reservation queue, and fine management.

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Minimal API Architecture [CRITICAL] | 2 | 5 | 2 | 5 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 4 | 4 | 4 | 2 |
| NuGet & Package Discipline [CRITICAL] | 5 | 5 | 2 | 5 | 3 |
| EF Migration Usage [CRITICAL] | 1 | 5 | 1 | 5 | 1 |
| Business Logic Correctness [HIGH] | 5 | 5 | 5 | 5 | 5 |
| Prefer Built-in over 3rd Party [HIGH] | 5 | 5 | 2 | 5 | 2 |
| Modern C# Adoption [HIGH] | 5 | 4 | 3 | 5 | 2 |
| Error Handling & Middleware [HIGH] | 3 | 5 | 3 | 5 | 3 |
| Async Patterns & Cancellation [HIGH] | 3 | 5 | 3 | 5 | 2 |
| EF Core Best Practices [HIGH] | 4 | 4 | 4 | 5 | 2 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 4 | 5 | 3 |
| Security Configuration [HIGH] | 1 | 1 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 5 | 5 | 2 | 5 | 2 |
| Sealed Types [MEDIUM] | 5 | 5 | 2 | 5 | 1 |
| Data Seeder Design [MEDIUM] | 3 | 4 | 3 | 5 | 3 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 | 3 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 3 | 5 | 3 | 5 | 2 |
| File Organization [MEDIUM] | 4 | 5 | 4 | 5 | 3 |
| HTTP Test File Quality [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Type Design & Resource Management [MEDIUM] | 5 | 5 | 4 | 5 | 3 |
| Code Standards Compliance [LOW] | 5 | 5 | 4 | 5 | 3 |

---

## 1. Build & Run Success [CRITICAL]

All five configurations compile successfully with **zero errors**.

| Configuration | Errors | Warnings | Build Time |
|---|---|---|---|
| dotnet-artisan | 0 | 220 | 15.2s |
| dotnet-webapi | 0 | 327 | 15.5s |
| dotnet-skills | 0 | 271 | 16.0s |
| managedcode | 0 | 307 | 15.1s |
| no-skills | 0 | 241 | 14.7s |

Warnings are predominantly MA0004 (ConfigureAwait) from the Meziantou analyzer in `Directory.Build.props`, which is a build-time-only analyzer and not a code quality concern for ASP.NET Core apps (SynchronizationContext is null).

**Scores**: All 5 — **Build succeeded** pattern confirmed across every configuration.

**Verdict**: Tie. All configurations produce compilable code.

---

## 2. Security Vulnerability Scan [CRITICAL]

All five configurations report **no vulnerable packages**:

```
The given project `LibraryApi` has no vulnerable packages given the current sources.
```

**Scores**: All 5.

**Verdict**: Tie. All configurations select safe package versions.

---

## 3. Minimal API Architecture [CRITICAL]

This is one of the most impactful differentiators. Only **dotnet-webapi** and **managedcode** use the modern Minimal API pattern.

**dotnet-webapi & managedcode** — Minimal APIs with route groups, TypedResults, and endpoint extension methods:

```csharp
// dotnet-webapi: Endpoints/AuthorEndpoints.cs
var group = app.MapGroup("/api/authors").WithTags("Authors");

group.MapGet("/{id:int}", async Task<Results<Ok<AuthorResponse>, NotFound>> (
    int id, IAuthorService service, CancellationToken ct) =>
{
    var result = await service.GetByIdAsync(id, ct);
    return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
})
.WithName("GetAuthor")
.WithSummary("Get author by ID");
```

```csharp
// Program.cs (both dotnet-webapi and managedcode)
app.MapAuthorEndpoints();
app.MapBookEndpoints();
app.MapLoanEndpoints();
// ... clean Program.cs with no inline endpoint definitions
```

**dotnet-artisan, dotnet-skills, no-skills** — Traditional controllers:

```csharp
// dotnet-artisan: Controllers/AuthorsController.cs
[ApiController]
[Route("api/authors")]
public sealed class AuthorsController(IAuthorService service) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAuthor(int id)
    {
        var author = await service.GetAuthorByIdAsync(id);
        return author is not null ? Ok(author) : NotFound();
    }
}
```

| Configuration | API Style | MapGroup | TypedResults | Results\<T1,T2\> |
|---|---|---|---|---|
| dotnet-artisan | Controllers | ✗ | ✗ | ✗ |
| dotnet-webapi | **Minimal APIs** | **✓** | **✓** | **✓** |
| dotnet-skills | Controllers | ✗ | ✗ | ✗ |
| managedcode | **Minimal APIs** | **✓** | **✓** | **✓** |
| no-skills | Controllers | ✗ | ✗ | ✗ |

**Scores**: dotnet-artisan: 2 · dotnet-webapi: **5** · dotnet-skills: 2 · managedcode: **5** · no-skills: 1

**Verdict**: **dotnet-webapi** and **managedcode** follow the modern .NET standard. Minimal APIs with TypedResults and union return types provide compile-time safety and automatic OpenAPI schema generation. The no-skills baseline scores lowest with no OpenAPI metadata enrichment.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

**dotnet-webapi & managedcode** — Data Annotations on sealed record DTOs:

```csharp
// dotnet-webapi: DTOs/BookDtos.cs
public sealed record CreateBookRequest
{
    [Required, MaxLength(300)]
    public required string Title { get; init; }
    [Required, MaxLength(20)]
    public required string ISBN { get; init; }
    [Range(1, int.MaxValue)]
    public required int TotalCopies { get; init; }
    public required IReadOnlyList<int> AuthorIds { get; init; }
}
```

**dotnet-skills** — FluentValidation with dedicated validator classes:

```csharp
// dotnet-skills: Validators/Validators.cs
public class BookCreateDtoValidator : AbstractValidator<BookCreateDto>
{
    public BookCreateDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ISBN).NotEmpty().MaximumLength(20)
            .Matches(@"^(?:\d{9}[\dXx]|\d{13})$").WithMessage("ISBN must be valid.");
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1);
    }
}
```

**dotnet-artisan** — Manual validation in service layer only:

```csharp
// dotnet-artisan: Services/LoanService.cs
var book = await db.Books.FindAsync(request.BookId)
    ?? throw new KeyNotFoundException($"Book with ID {request.BookId} not found.");
if (!patron.IsActive)
    throw new InvalidOperationException("Patron's membership is not active.");
```

**no-skills** — Manual if-checks in controllers:

```csharp
// no-skills: Controllers/BooksController.cs
if (string.IsNullOrWhiteSpace(dto.Title))
    return BadRequest(new ProblemDetails { Detail = "Title is required." });
if (dto.TotalCopies < 1)
    return BadRequest(new ProblemDetails { Detail = "TotalCopies must be at least 1." });
```

No configuration uses `ArgumentNullException.ThrowIfNull()` for DI constructor parameters.

**Scores**: dotnet-artisan: 3 · dotnet-webapi: **4** · dotnet-skills: **4** · managedcode: **4** · no-skills: 2

**Verdict**: dotnet-webapi and managedcode provide the best balance — built-in Data Annotations on immutable records. dotnet-skills uses FluentValidation with stronger ISBN validation (regex) but adds a 3rd-party dependency. no-skills relies on fragile manual checks.

---

## 5. NuGet & Package Discipline [CRITICAL]

| Configuration | Packages | Versions | Unnecessary Packages |
|---|---|---|---|
| dotnet-artisan | 3 | All exact (10.0.4, 10.0.5) | None |
| dotnet-webapi | 4 | All exact (10.0.4, 10.0.5) | None |
| dotnet-skills | 5 | **2 use `10.*` wildcards** | Swashbuckle + FluentValidation |
| managedcode | 3 | All exact (10.0.4, 10.0.5) | None |
| no-skills | 3+1 | **Meziantou uses `*`** | Swashbuckle |

**dotnet-skills** uses wildcard versions — the worst case for reproducibility:

```xml
<!-- dotnet-skills: WILDCARD VERSIONS -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

**dotnet-artisan & managedcode** — minimal, exact versions:

```xml
<!-- dotnet-artisan & managedcode: EXACT VERSIONS, MINIMAL -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```

**Scores**: dotnet-artisan: **5** · dotnet-webapi: **5** · dotnet-skills: 2 · managedcode: **5** · no-skills: 3

**Verdict**: dotnet-artisan, dotnet-webapi, and managedcode achieve the ideal — minimal packages with exact versions. dotnet-skills is the worst offender with wildcard versions and unnecessary third-party libraries.

---

## 6. EF Migration Usage [CRITICAL]

This is a major differentiator. Only **dotnet-webapi** and **managedcode** use EF Core migrations.

**dotnet-webapi & managedcode** — Migrations:

```csharp
// Program.cs
db.Database.Migrate();
await DataSeeder.SeedAsync(db);
```

Both have a `Migrations/` directory with generated migration files (e.g., `20260329085239_InitialCreate.cs`).

**dotnet-artisan, dotnet-skills, no-skills** — EnsureCreated anti-pattern:

```csharp
// Program.cs (all three)
await db.Database.EnsureCreatedAsync();
await DataSeeder.SeedAsync(db);
```

| Configuration | Strategy | Migrations Dir | Schema Evolution |
|---|---|---|---|
| dotnet-artisan | EnsureCreated | ✗ | Impossible |
| dotnet-webapi | **Migrate()** | **✓** | **Safe** |
| dotnet-skills | EnsureCreated | ✗ | Impossible |
| managedcode | **Migrate()** | **✓** | **Safe** |
| no-skills | EnsureCreated | ✗ | Impossible |

**Scores**: dotnet-artisan: 1 · dotnet-webapi: **5** · dotnet-skills: 1 · managedcode: **5** · no-skills: 1

**Verdict**: **dotnet-webapi** and **managedcode** are the only production-viable configurations. EnsureCreated bypasses the migration pipeline, making schema evolution impossible and causing data loss on model changes.

---

## 7. Business Logic Correctness [HIGH]

All five configurations implement the full specification with remarkable completeness:

| Business Rule | artisan | webapi | skills | managed | no-skills |
|---|---|---|---|---|---|
| Checkout rules (4 validations) | ✓ | ✓ | ✓ | ✓ | ✓ |
| Return + auto-fine ($0.25/day) | ✓ | ✓ | ✓ | ✓ | ✓ |
| Reservation queue promotion | ✓ | ✓ | ✓ | ✓ | ✓ |
| Renewal rules (max 2, no pending) | ✓ | ✓ | ✓ | ✓ | ✓ |
| Fine threshold ($10 block) | ✓ | ✓ | ✓ | ✓ | ✓ |
| Overdue detection endpoint | ✓ | ✓ | ✓ | ✓ | ✓ |
| Membership-based limits | ✓ | ✓ | ✓ | ✓ | ✓ |
| Deletion constraints | ✓ | ✓ | ✓ | ✓ | ✓ |

**Scores**: All **5**.

**Verdict**: Tie. All configurations implement the complete business specification correctly. The underlying model (Copilot) handles business logic consistently regardless of skill configuration.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

| Configuration | OpenAPI | Validation | Logging | JSON |
|---|---|---|---|---|
| dotnet-artisan | **Built-in AddOpenApi()** | Manual | Built-in ILogger | System.Text.Json |
| dotnet-webapi | **Built-in AddOpenApi()** | Data Annotations | Built-in ILogger | System.Text.Json |
| dotnet-skills | AddOpenApi() **+ Swashbuckle** | **FluentValidation** | Built-in ILogger | System.Text.Json |
| managedcode | **Built-in AddOpenApi()** | Data Annotations | Built-in ILogger | System.Text.Json |
| no-skills | **Swashbuckle only** | Manual | Built-in ILogger | System.Text.Json |

**dotnet-skills** uses both Swashbuckle AND built-in OpenAPI — redundant:

```csharp
// dotnet-skills: Program.cs — dual OpenAPI
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c => { ... });
// ...
app.UseSwagger();
app.UseSwaggerUI(c => { ... });
app.MapOpenApi();
```

**no-skills** uses only Swashbuckle with no built-in OpenAPI:

```csharp
// no-skills: Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ...
app.UseSwagger();
app.UseSwaggerUI(options => { ... });
```

**Scores**: dotnet-artisan: **5** · dotnet-webapi: **5** · dotnet-skills: 2 · managedcode: **5** · no-skills: 2

**Verdict**: dotnet-artisan, dotnet-webapi, and managedcode correctly use only built-in capabilities. The .NET team explicitly recommends `AddOpenApi()` over Swashbuckle for .NET 9+ projects.

---

## 9. Modern C# Adoption [HIGH]

| Feature | artisan | webapi | skills | managed | no-skills |
|---|---|---|---|---|---|
| Primary constructors | **✓** | **✓** | ✗ | **✓** | ✗ |
| Collection expressions `[]` | **✓** | ✗ | ✗ | **✓** | ✗ |
| Records for DTOs | **✓** | **✓** | ✗ | **✓** | ✗ |
| File-scoped namespaces | ✓ | ✓ | ✓ | ✓ | ✓ |
| Switch expressions | ✓ | ✓ | ✓ | ✓ | ✓ |
| `required` keyword | ✗ | **✓** | ✗ | **✓** | ✗ |
| Sealed records | **✓** | **✓** | ✗ | **✓** | ✗ |

**dotnet-artisan** — Collection expressions and primary constructors:

```csharp
// Models/Book.cs
public ICollection<BookAuthor> BookAuthors { get; set; } = [];

// Services/LoanService.cs
public sealed class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
```

**dotnet-skills** — Traditional constructors with private readonly fields:

```csharp
// Services/AuthorService.cs
public sealed class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<AuthorService> _logger;
    public AuthorService(LibraryDbContext db, ILogger<AuthorService> logger)
    {
        _db = db;
        _logger = logger;
    }
}
```

**no-skills** — Oldest style with no modern features:

```csharp
// Services/BookService.cs
public class BookService : IBookService
{
    private readonly LibraryDbContext _db;
    public BookService(LibraryDbContext db) => _db = db;
}
```

**Scores**: dotnet-artisan: **5** · dotnet-webapi: 4 · dotnet-skills: 3 · managedcode: **5** · no-skills: 2

**Verdict**: **dotnet-artisan** and **managedcode** showcase the most modern C# features. dotnet-webapi is close but misses collection expressions. no-skills uses the most dated patterns.

---

## 10. Error Handling & Middleware [HIGH]

**dotnet-webapi & managedcode** — Modern `IExceptionHandler`:

```csharp
// Middleware/ApiExceptionHandler.cs (both configs)
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
        // ... ProblemDetails response
    }
}
```

Registered with DI:
```csharp
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
```

**dotnet-artisan** — Inline `UseExceptionHandler` lambda (functional but not composable):

```csharp
// Program.cs
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        // ... switch expression mapping + ProblemDetails
    });
});
```

**dotnet-skills & no-skills** — Custom middleware with `RequestDelegate`:

```csharp
// Middleware/GlobalExceptionHandlerMiddleware.cs
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (BusinessRuleException ex) { /* ... ProblemDetails */ }
        catch (Exception ex) { /* ... ProblemDetails */ }
    }
}
```

**Scores**: dotnet-artisan: 3 · dotnet-webapi: **5** · dotnet-skills: 3 · managedcode: **5** · no-skills: 3

**Verdict**: `IExceptionHandler` (dotnet-webapi, managedcode) is the modern .NET 8+ approach — DI-aware, composable, and testable. The inline lambda (dotnet-artisan) works but isn't reusable. The `RequestDelegate` middleware (dotnet-skills, no-skills) is the pre-.NET 8 pattern.

---

## 11. Async Patterns & Cancellation [HIGH]

**dotnet-webapi & managedcode** — Full `CancellationToken` propagation:

```csharp
// Endpoints → Services → EF Core
group.MapGet("/", async Task<IResult> (
    string? search, int? page, int? pageSize,
    IAuthorService service, CancellationToken ct) =>
{
    var result = await service.GetAllAsync(search, page ?? 1, pageSize ?? 10, ct);
    return TypedResults.Ok(result);
})

// Service layer
public async Task<PaginatedResponse<AuthorListResponse>> GetAllAsync(
    string? search, int page, int pageSize, CancellationToken ct)
{
    var totalCount = await query.CountAsync(ct);
    var items = await query.ToListAsync(ct);
}
```

**dotnet-artisan, dotnet-skills, no-skills** — No `CancellationToken` anywhere:

```csharp
// dotnet-artisan: Services/LoanService.cs
public async Task<LoanResponse> CheckoutBookAsync(CreateLoanRequest request)
{
    var book = await db.Books.FindAsync(request.BookId); // No ct parameter
}
```

**Scores**: dotnet-artisan: 3 · dotnet-webapi: **5** · dotnet-skills: 3 · managedcode: **5** · no-skills: 2

**Verdict**: Only dotnet-webapi and managedcode propagate `CancellationToken`. Without it, cancelled HTTP requests continue executing database queries, wasting server resources.

---

## 12. EF Core Best Practices [HIGH]

**managedcode** — The gold standard with `IEntityTypeConfiguration<T>`:

```csharp
// Data/Configurations/BookConfiguration.cs
public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasIndex(b => b.ISBN).IsUnique();
        builder.Property(b => b.Title).HasMaxLength(300);
        builder.HasMany(b => b.BookAuthors).WithOne(ba => ba.Book);
        builder.HasData(
            new Book { Id = 1, Title = "1984", ISBN = "978-0451524935", ... }
        );
    }
}

// DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
}
```

**dotnet-artisan, dotnet-webapi, dotnet-skills** — Inline Fluent API in `OnModelCreating`:

```csharp
// LibraryDbContext.cs
modelBuilder.Entity<Loan>(e =>
{
    e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
    e.HasOne(l => l.Book).WithMany(b => b.Loans).OnDelete(DeleteBehavior.Restrict);
});
```

**no-skills** — Missing `AsNoTracking()` on reads:

```csharp
// no-skills: Services/LoanService.cs — no AsNoTracking
var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron).AsQueryable();
```

| Feature | artisan | webapi | skills | managed | no-skills |
|---|---|---|---|---|---|
| AsNoTracking on reads | **✓** | **✓** | **✓** | **✓** | ✗ |
| IEntityTypeConfiguration | ✗ | ✗ | ✗ | **✓** | ✗ |
| HasConversion\<string\> | ✓ | ✓ | ✓ | ✓ | ✓ |
| Explicit indexes | ✓ | ✓ | ✓ | ✓ | ✓ |

**Scores**: dotnet-artisan: 4 · dotnet-webapi: 4 · dotnet-skills: 4 · managedcode: **5** · no-skills: 2

**Verdict**: **managedcode** excels with separate configuration classes per entity (`IEntityTypeConfiguration<T>`), which scales better for large models. no-skills misses `AsNoTracking()`, doubling memory usage on read queries.

---

## 13. Service Abstraction & DI [HIGH]

All configurations use interface-based services with `AddScoped<IService, Service>()`:

```csharp
// All configurations (Program.cs)
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();
// ...
```

Key differences:

| Configuration | Sealed Services | Primary Ctors | ILogger in All | Separate Interface Files |
|---|---|---|---|---|
| dotnet-artisan | **✓** | **✓** | ✓ | ✗ (Interfaces.cs) |
| dotnet-webapi | **✓** | **✓** | **✓** | **✓** |
| dotnet-skills | **✓** | ✗ | ✓ | ✗ (Interfaces.cs) |
| managedcode | **✓** | **✓** | **✓** | **✓** |
| no-skills | ✗ | ✗ | Partial | ✗ (Interfaces.cs) |

**Scores**: dotnet-artisan: **5** · dotnet-webapi: **5** · dotnet-skills: 4 · managedcode: **5** · no-skills: 3

**Verdict**: dotnet-artisan, dotnet-webapi, and managedcode all achieve excellence with sealed, primary-constructor services. no-skills has unsealed services and inconsistent ILogger injection.

---

## 14. Security Configuration [HIGH]

**No configuration implements HSTS or HTTPS redirection.**

None of the five configurations include:
- `app.UseHsts()`
- `app.UseHttpsRedirection()`
- CORS configuration
- Authentication/authorization middleware

**Scores**: All 1.

**Verdict**: This is a universal gap. While the spec says "No authentication required," HSTS and HTTPS redirection are baseline security practices for any production API.

---

## 15. DTO Design [MEDIUM]

**dotnet-artisan, dotnet-webapi, managedcode** — Sealed records with immutable patterns:

```csharp
// dotnet-artisan: DTOs/Dtos.cs — positional sealed records
public sealed record AuthorResponse(
    int Id, string FirstName, string LastName,
    string? Biography, DateOnly? BirthDate, string? Country, DateTime CreatedAt);

// dotnet-webapi: DTOs/AuthorDtos.cs — sealed records with init properties
public sealed record CreateAuthorRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
}

// dotnet-webapi: DTOs/PaginatedResponse.cs
public sealed record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public static PaginatedResponse<T> Create(...) => new() { ... };
}
```

**dotnet-skills & no-skills** — Mutable classes with public setters:

```csharp
// dotnet-skills & no-skills: DTOs/Dtos.cs
public class AuthorCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class AuthorDetailDto : AuthorDto  // Inheritance in DTOs
{
    public List<BookSummaryDto> Books { get; set; } = new();
}
```

**Scores**: dotnet-artisan: **5** · dotnet-webapi: **5** · dotnet-skills: 2 · managedcode: **5** · no-skills: 2

**Verdict**: Sealed records with `init` or positional parameters (artisan, webapi, managedcode) produce safer, more expressive API contracts. Mutable class DTOs with inheritance (skills, no-skills) risk over-posting and data leakage.

---

## 16. Sealed Types [MEDIUM]

| Configuration | Sealed Count | Total Types | Sealed % |
|---|---|---|---|
| dotnet-artisan | 46 | 46 | **100%** |
| dotnet-webapi | ~19 | ~19 | **100%** |
| dotnet-skills | 7 | 25+ | 28% |
| managedcode | 60+ | 60+ | **100%** |
| no-skills | 0 | 35 | **0%** |

**Scores**: dotnet-artisan: **5** · dotnet-webapi: **5** · dotnet-skills: 2 · managedcode: **5** · no-skills: 1

**Verdict**: dotnet-artisan, dotnet-webapi, and managedcode seal 100% of types (models, services, DTOs, middleware). no-skills seals nothing. Sealing enables JIT devirtualization and signals design intent (CA1852).

---

## 17. Data Seeder Design [MEDIUM]

**managedcode** — `HasData()` in entity configurations (captured in migrations):

```csharp
// Data/Configurations/AuthorConfiguration.cs
builder.HasData(
    new Author { Id = 1, FirstName = "George", LastName = "Orwell", ... },
    new Author { Id = 2, FirstName = "Jane", LastName = "Austen", ... }
);
```

**dotnet-webapi** — Runtime `DataSeeder` with `Migrate()`:

```csharp
// Data/DataSeeder.cs
public static async Task SeedAsync(LibraryDbContext db)
{
    if (await db.Authors.AnyAsync()) return;
    db.Authors.AddRange(authors);
    await db.SaveChangesAsync();
}
```

**dotnet-artisan, dotnet-skills, no-skills** — Runtime seeder with `EnsureCreated()`:

```csharp
await db.Database.EnsureCreatedAsync();
await DataSeeder.SeedAsync(db);
```

**Scores**: dotnet-artisan: 3 · dotnet-webapi: 4 · dotnet-skills: 3 · managedcode: **5** · no-skills: 3

**Verdict**: `HasData()` (managedcode) is the best approach — seed data is captured in migrations for reproducible state. Runtime seeders work but are separate from the migration pipeline.

---

## 18. Structured Logging [MEDIUM]

All configurations inject `ILogger<T>` and use structured message templates:

```csharp
// Common pattern across all configs
logger.LogInformation("Book {BookId} checked out to patron {PatronId}, loan {LoanId}, due {DueDate}",
    book.Id, patron.Id, loan.Id, loan.DueDate);

logger.LogInformation("Fine of ${Amount:F2} issued for overdue loan {LoanId} ({Days} days late)",
    fineAmount, loanId, overdueDays);
```

**no-skills** injects ILogger in only 4 of 7 services (missing in AuthorService, BookService, CategoryService).

**Scores**: dotnet-artisan: 4 · dotnet-webapi: 4 · dotnet-skills: 4 · managedcode: 4 · no-skills: 3

**Verdict**: All use structured logging correctly. No configuration uses high-performance `[LoggerMessage]` source generators (would require score 5).

---

## 19. Nullable Reference Types [MEDIUM]

All configurations enable NRTs:

```xml
<Nullable>enable</Nullable>
```

All use `?` annotations for optional properties and navigation properties correctly. Minor use of `null!` (null-forgiving operator) exists across all configurations for navigation properties with deferred initialization.

**Scores**: All 4.

**Verdict**: Tie. All configurations leverage NRTs consistently.

---

## 20. API Documentation [MEDIUM]

**dotnet-webapi & managedcode** — Rich OpenAPI metadata on every endpoint:

```csharp
// Both configs
group.MapGet("/", handler)
    .WithName("GetAuthors")
    .WithSummary("List authors")
    .WithDescription("List authors with optional search by name and pagination.")
    .Produces<PaginatedResponse<AuthorListResponse>>()
    .Produces(StatusCodes.Status404NotFound);
```

**dotnet-artisan** — `AddOpenApi()` with document transformer but no per-endpoint metadata:

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Sunrise Community Library API";
        return Task.CompletedTask;
    });
});
```

**no-skills** — Swashbuckle only with minimal metadata:

```csharp
builder.Services.AddSwaggerGen();
```

**Scores**: dotnet-artisan: 3 · dotnet-webapi: **5** · dotnet-skills: 3 · managedcode: **5** · no-skills: 2

**Verdict**: dotnet-webapi and managedcode produce the richest API documentation via per-endpoint metadata. TypedResults with `Results<T1,T2>` automatically generate accurate response schemas.

---

## 21. File Organization [MEDIUM]

**dotnet-webapi & managedcode** — Feature-rich clean structure:

```
Endpoints/          ← Minimal API endpoint groups
Data/               ← DbContext
Data/Configurations/← IEntityTypeConfiguration<T> (managedcode only)
DTOs/               ← One file per resource (AuthorDtos.cs, BookDtos.cs, ...)
Middleware/         ← ApiExceptionHandler
Migrations/         ← EF Core migrations
Models/             ← Entity classes + separate enum files
Services/           ← Separate interface + implementation files
```

**dotnet-artisan** — Clean but consolidated:

```
Controllers/        ← 7 controller files
Data/               ← DbContext + DataSeeder
DTOs/               ← Single Dtos.cs file (all DTOs)
Models/             ← Entity + enum files
Services/           ← Services + single Interfaces.cs
```

**no-skills** — Similar to artisan but less organized:

```
Controllers/        ← 7 controller files
DTOs/               ← Single Dtos.cs
Services/           ← Services + Interfaces.cs + Exceptions.cs
```

**Scores**: dotnet-artisan: 4 · dotnet-webapi: **5** · dotnet-skills: 4 · managedcode: **5** · no-skills: 3

**Verdict**: dotnet-webapi and managedcode have the best organization with dedicated Endpoints/ directories, separate DTO files per resource, and separate interface files. managedcode adds Data/Configurations/ for EF entity configs.

---

## 22. HTTP Test File Quality [MEDIUM]

All configurations produce comprehensive `.http` files:

| Configuration | Test Requests | Business Rule Tests |
|---|---|---|
| dotnet-artisan | 72 | ✓ (checkout failures, renewal failures) |
| dotnet-webapi | 41 | ✓ (fines block, inactive patron, pending reservations) |
| dotnet-skills | 64 | ✓ |
| managedcode | 40 | ✓ |
| no-skills | 73 | ✓ (comprehensive edge cases) |

**Scores**: All 4.

**Verdict**: All configurations produce usable `.http` files covering CRUD and business rules. no-skills and dotnet-artisan have the most test cases but quantity doesn't necessarily mean quality.

---

## 23. Type Design & Resource Management [MEDIUM]

All configurations use enums for status fields and store them as strings:

```csharp
// All configs
public enum LoanStatus { Active, Returned, Overdue }
entity.Property(l => l.Status).HasConversion<string>();
```

Key differences:

| Configuration | Enum Storage | Collection Types in DTOs | JSON Enum Config |
|---|---|---|---|
| dotnet-artisan | String | `IReadOnlyList<T>` | `JsonStringEnumConverter` |
| dotnet-webapi | String | `IReadOnlyList<T>` | `ConfigureHttpJsonOptions` |
| dotnet-skills | String | `List<T>` (mutable) | `JsonStringEnumConverter` |
| managedcode | String | `IReadOnlyList<T>` | `ConfigureHttpJsonOptions` |
| no-skills | String | `List<T>` (mutable) | Not configured |

**Scores**: dotnet-artisan: **5** · dotnet-webapi: **5** · dotnet-skills: 4 · managedcode: **5** · no-skills: 3

**Verdict**: artisan, webapi, and managedcode use `IReadOnlyList<T>` for collection properties, signaling immutability. no-skills doesn't configure JSON enum serialization.

---

## 24. Code Standards Compliance [LOW]

| Convention | artisan | webapi | skills | managed | no-skills |
|---|---|---|---|---|---|
| PascalCase types/methods | ✓ | ✓ | ✓ | ✓ | ✓ |
| Async suffix | ✓ | ✓ | ✓ | ✓ | ✓ |
| I prefix for interfaces | ✓ | ✓ | ✓ | ✓ | ✓ |
| File-scoped namespaces | ✓ | ✓ | ✓ | ✓ | ✓ |
| Explicit access modifiers | ✓ | ✓ | Partial | ✓ | Partial |

**Scores**: dotnet-artisan: **5** · dotnet-webapi: **5** · dotnet-skills: 4 · managedcode: **5** · no-skills: 3

**Verdict**: dotnet-artisan, dotnet-webapi, and managedcode follow .NET conventions fully. no-skills has some types relying on default access modifiers.

---

## Weighted Summary

| Tier | Weight | Dimensions |
|---|---|---|
| CRITICAL | ×3 | Build, Security Scan, Minimal API, Validation, NuGet, EF Migration |
| HIGH | ×2 | Business Logic, Built-in Pref, Modern C#, Error Handling, Async, EF Practices, Service DI, Security |
| MEDIUM | ×1 | DTO Design, Sealed Types, Data Seeder, Logging, NRTs, API Docs, File Org, HTTP File, Type Design |
| LOW | ×0.5 | Code Standards |

### Detailed Weighted Calculation

| Dimension [Tier] | Weight | artisan | webapi | skills | managed | no-skills |
|---|---|---|---|---|---|---|
| **CRITICAL (×3)** | | | | | | |
| Build & Run | 3 | 15 | 15 | 15 | 15 | 15 |
| Security Scan | 3 | 15 | 15 | 15 | 15 | 15 |
| Minimal API Architecture | 3 | 6 | 15 | 6 | 15 | 3 |
| Input Validation | 3 | 9 | 12 | 12 | 12 | 6 |
| NuGet Discipline | 3 | 15 | 15 | 6 | 15 | 9 |
| EF Migration | 3 | 3 | 15 | 3 | 15 | 3 |
| **Critical Subtotal** | | **63** | **87** | **57** | **87** | **51** |
| **HIGH (×2)** | | | | | | |
| Business Logic | 2 | 10 | 10 | 10 | 10 | 10 |
| Built-in Preference | 2 | 10 | 10 | 4 | 10 | 4 |
| Modern C# | 2 | 10 | 8 | 6 | 10 | 4 |
| Error Handling | 2 | 6 | 10 | 6 | 10 | 6 |
| Async/Cancellation | 2 | 6 | 10 | 6 | 10 | 4 |
| EF Core Practices | 2 | 8 | 8 | 8 | 10 | 4 |
| Service DI | 2 | 10 | 10 | 8 | 10 | 6 |
| Security Config | 2 | 2 | 2 | 2 | 2 | 2 |
| **High Subtotal** | | **62** | **68** | **50** | **72** | **40** |
| **MEDIUM (×1)** | | | | | | |
| DTO Design | 1 | 5 | 5 | 2 | 5 | 2 |
| Sealed Types | 1 | 5 | 5 | 2 | 5 | 1 |
| Data Seeder | 1 | 3 | 4 | 3 | 5 | 3 |
| Logging | 1 | 4 | 4 | 4 | 4 | 3 |
| NRTs | 1 | 4 | 4 | 4 | 4 | 4 |
| API Documentation | 1 | 3 | 5 | 3 | 5 | 2 |
| File Organization | 1 | 4 | 5 | 4 | 5 | 3 |
| HTTP File | 1 | 4 | 4 | 4 | 4 | 4 |
| Type Design | 1 | 5 | 5 | 4 | 5 | 3 |
| **Medium Subtotal** | | **37** | **41** | **30** | **42** | **25** |
| **LOW (×0.5)** | | | | | | |
| Code Standards | 0.5 | 2.5 | 2.5 | 2.0 | 2.5 | 1.5 |
| **Low Subtotal** | | **2.5** | **2.5** | **2.0** | **2.5** | **1.5** |
| | | | | | | |
| **TOTAL** | | **164.5** | **198.5** | **139.0** | **203.5** | **117.5** |

### Final Rankings

| Rank | Configuration | Weighted Score | % of Max (244.5) |
|---|---|---|---|
| 🥇 **1st** | **managedcode-dotnet-skills** | **203.5** | 83.2% |
| 🥈 **2nd** | **dotnet-webapi** | **198.5** | 81.2% |
| 🥉 **3rd** | **dotnet-artisan** | **164.5** | 67.3% |
| 4th | **dotnet-skills** | **139.0** | 56.9% |
| 5th | **no-skills** | **117.5** | 48.1% |

---

## What All Versions Get Right

- **Zero build errors** — All five configurations produce compilable .NET 10 projects
- **Zero vulnerabilities** — No known CVEs in any package selection
- **Complete business logic** — All implement checkout rules, return processing, renewal logic, reservation queue management, and fine calculation correctly
- **Interface-based services** — All use `IService`/`Service` pattern with `AddScoped<IService, Service>()`
- **Nullable reference types** — All enable `<Nullable>enable</Nullable>`
- **File-scoped namespaces** — All use `namespace X;` instead of block-scoped
- **EF Core enum handling** — All store enums as strings via `HasConversion<string>()` and serialize as strings in JSON
- **ProblemDetails error responses** — All return RFC 7807 `ProblemDetails` for errors
- **Structured logging** — All use `ILogger<T>` with named message template placeholders
- **Pagination** — All implement consistent pagination with metadata (page, pageSize, totalCount, totalPages)
- **Seed data** — All include realistic, varied seed data (6+ authors, 12+ books, 7+ patrons, loans in multiple states)
- **HTTP test files** — All produce `.http` files covering endpoints with business rule test cases

---

## Summary: Impact of Skills

### Most Impactful Differentiators (Ranked by Score Gap)

1. **EF Core Migrations** (3-way tie at 5 vs 1): The single largest architectural gap. dotnet-webapi and managedcode use `Migrate()`; the other three use the `EnsureCreated()` anti-pattern. This alone makes three configurations unsuitable for production.

2. **Minimal API Architecture** (5 vs 1-2): dotnet-webapi and managedcode use Minimal APIs with TypedResults and route groups. The other three use legacy controllers. This affects type safety, OpenAPI generation quality, and code conciseness.

3. **CancellationToken Propagation** (5 vs 2-3): Only dotnet-webapi and managedcode propagate CancellationToken through the full call chain. Missing propagation wastes resources on cancelled requests.

4. **Package Discipline** (5 vs 2-3): dotnet-skills uses wildcard versions (`10.*`), and both dotnet-skills and no-skills include Swashbuckle unnecessarily.

5. **Modern C# Features** (5 vs 2): Primary constructors, collection expressions, and sealed records are used by the top three configurations but absent in dotnet-skills and no-skills.

### Configuration Assessments

**🥇 managedcode-dotnet-skills (203.5/244.5)** — The most comprehensive configuration. Combines six specialized skills to produce the most modern code: Minimal APIs, IExceptionHandler, migrations, HasData() seeding, IEntityTypeConfiguration, TypedResults, primary constructors, collection expressions, and 100% sealed types. The only configuration that achieves excellence across all tiers.

**🥈 dotnet-webapi (198.5/244.5)** — Very close to managedcode. The single-skill approach produces nearly identical architecture (Minimal APIs, migrations, IExceptionHandler, TypedResults). Minor gaps: no collection expressions, inline DbContext configuration instead of IEntityTypeConfiguration, and runtime seeder instead of HasData(). An excellent result from a single skill.

**🥉 dotnet-artisan (164.5/244.5)** — Strong modern C# adoption (100% sealed, primary constructors, collection expressions, sealed records) but falls behind on architecture choices: uses Controllers instead of Minimal APIs, EnsureCreated instead of migrations, and lacks CancellationToken propagation. The plugin chain's `dotnet-api` skill chose controllers as a "pragmatic fallback" which cost significant points.

**4th: dotnet-skills (139.0/244.5)** — The EF Core query optimization and performance analysis skills improve sealed types and AsNoTracking usage, but don't guide architectural decisions. Uses Controllers, EnsureCreated, Swashbuckle + FluentValidation (unnecessary 3rd-party deps), wildcard package versions, and old-style constructors. The most third-party-dependent configuration.

**5th: no-skills (117.5/244.5)** — The baseline demonstrates Copilot's default output: functional but outdated. Controllers, EnsureCreated, Swashbuckle, no sealed types, no CancellationToken, no AsNoTracking, mutable class DTOs. Produces working code that follows none of the modern .NET best practices.

### Key Takeaway

Skills that provide **opinionated architectural guidance** (dotnet-webapi, managedcode's skill chain) produce dramatically better code than skills focused on **narrow optimizations** (dotnet-skills' EF Core query optimizer). The 72% score gap between managedcode (203.5) and no-skills (117.5) demonstrates that well-designed skills transform Copilot output from "functional but dated" to "production-ready modern .NET."
