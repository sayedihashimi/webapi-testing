---
name: dotnet-webapi
description: >
  Guides creation and modification of ASP.NET Core Web API endpoints with
  correct HTTP semantics, OpenAPI metadata, error handling, and data access
  wiring.
  USE FOR: adding new API endpoints (controllers or minimal APIs), wiring up
  OpenAPI/Swagger, creating .http test files, connecting endpoints to EF Core,
  adding pagination/filtering/sorting to list endpoints, setting up global
  error handling middleware.
  DO NOT USE FOR: general C# coding style (use dotnet-csharp), EF Core query
  optimization (use optimizing-ef-core-queries), frontend/Blazor work,
  gRPC services, or SignalR hubs.
---

# ASP.NET Core Web API

Produce well-structured ASP.NET Core Web API endpoints with proper HTTP
semantics, OpenAPI documentation, error handling, and data access patterns.

## Workflow

### General: Seal all types by default

Mark every new class and record as `sealed` unless it is explicitly designed
for inheritance. This applies to:

- **Model / entity classes** — `public sealed class Product { ... }`
- **Service implementations** — `public sealed class ProductService(...) : IProductService`
- **DTO records** — `public sealed record ProductResponse(...)`
- **Middleware and handlers** — `internal sealed class ApiExceptionHandler(...) : IExceptionHandler`

Sealing types prevents unintended inheritance, communicates design intent, and
enables JIT devirtualization for better performance (CA1852 analyzer rule).

### Step 1: Determine the API style

Scan the project for existing endpoint patterns before writing any code.

1. Search for classes inheriting `ControllerBase` or decorated with `[ApiController]`.
2. Search `Program.cs` or endpoint files for `app.MapGet`, `app.MapPost`, etc.
3. If the project already uses **controllers**, continue with controllers.
4. If the project already uses **minimal APIs**, continue with minimal APIs.
5. If neither exists (new project), **default to minimal APIs** unless the user
   explicitly requests controllers.

Do not mix styles in the same project.

### Step 2: Define request and response types

Create dedicated types for API input and output. Never expose EF Core entities
directly in request or response bodies.

**Use `sealed record` for all DTOs.** Records enforce immutability, provide
value-based equality, and produce concise code. Seal them to prevent unintended
inheritance and enable JIT devirtualization (CA1852).

**Naming convention:**

| Role | Convention | Example |
|------|-----------|---------|
| Input (create) | `Create{Entity}Request` | `CreateProductRequest` |
| Input (update) | `Update{Entity}Request` | `UpdateProductRequest` |
| Output (single) | `{Entity}Response` | `ProductResponse` |
| Output (list) | `{Entity}ListResponse` or paginated wrapper | `ProductListResponse` |

**Response DTOs** — use positional sealed records for concise, immutable output:

```csharp
public sealed record ProductResponse(
    int Id,
    string Name,
    decimal Price,
    string CategoryName,
    bool IsAvailable);
```

**Request DTOs** — use sealed records with `init` properties so data annotations
work naturally:

```csharp
public sealed record CreateProductRequest
{
    [Required, MaxLength(200)]
    public required string Name { get; init; }

    [Range(0.01, 999999.99)]
    public required decimal Price { get; init; }

    public required int CategoryId { get; init; }
}
```

Follow the same pattern for `Update{Entity}Request` records, adding any
additional properties the update requires (e.g., `IsAvailable`).

**Do not** use mutable classes (`{ get; set; }`) for DTOs. Mutable DTOs allow
accidental modification after construction and lose the self-documenting
immutability that records provide.

### Step 3: Implement the endpoints

Whether using controllers or minimal APIs, follow these HTTP conventions
consistently.

**Minimal API return types — prefer `TypedResults`:**

Always prefer `TypedResults` over the `Results` factory. `TypedResults` embeds
response type information in the method signature, giving the OpenAPI generator
richer metadata automatically.

When a handler returns **multiple result types** (e.g., `Ok` or `NotFound`),
annotate the lambda with an explicit `Results<T1, T2>` return type. This
lets you use `TypedResults` while still giving the compiler a common type:

```csharp
async Task<Results<Ok<ProductResponse>, NotFound>> (int id, ...) => ...
```

**Do not** use `TypedResults.Ok(x)` and `TypedResults.NotFound()` in a bare
ternary without an explicit return type annotation. `Ok<T>` and `NotFound` are
different types with no common base the compiler can infer, which causes
`CS1593: Delegate 'RequestDelegate' does not take N arguments` because the
compiler falls back to matching `RequestDelegate(HttpContext)`.

**Fallback — `Results` factory:** If a handler has many conditional branches
(3+ result types), you may use the `Results` factory (`Results.Ok()`,
`Results.NotFound()`) which returns `IResult`, sacrificing compile-time OpenAPI
inference for simpler signatures.

**Status codes:**

| Operation | Success | Common errors |
|-----------|---------|---------------|
| GET (single) | `200 OK` | `404 Not Found` |
| GET (list) | `200 OK` | — |
| POST (create) | `201 Created` with `Location` header | `400 Bad Request`, `409 Conflict` |
| PUT (full update) | `200 OK` | `400 Bad Request`, `404 Not Found` |
| PATCH (partial/action) | `200 OK` | `400 Bad Request`, `404 Not Found` |
| DELETE | `204 No Content` | `404 Not Found`, `409 Conflict` |

**POST 201 responses:** Always return a `Location` header pointing to the
newly created resource.

- Controllers: use `CreatedAtAction(nameof(GetById), new { id = ... }, response)`
- Minimal APIs: use `TypedResults.Created($"/api/products/{id}", response)`

**CancellationToken:** Accept `CancellationToken` in every endpoint signature
and forward it through to all async calls (service methods, EF Core queries,
`HttpClient` calls). This allows the server to stop work when a client
disconnects.

```csharp
// Controller example
[HttpGet("{id}")]
public async Task<ActionResult<ProductResponse>> GetById(
    int id, CancellationToken cancellationToken)
{
    var product = await _productService.GetByIdAsync(id, cancellationToken);
    return product is null ? NotFound() : Ok(product);
}

// Minimal API example — TypedResults with explicit return type (recommended)
app.MapGet("/api/products/{id}", async Task<Results<Ok<ProductResponse>, NotFound>> (
    int id, IProductService service, CancellationToken cancellationToken) =>
{
    var product = await service.GetByIdAsync(id, cancellationToken);
    return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
});
```

**Pagination:** For any endpoint that returns a list of items, support
pagination with query parameters. Return metadata so the client knows
how to navigate.

Use a `sealed record` for the paginated wrapper with `IReadOnlyList<T>` to
signal immutability:

```csharp
public sealed record PaginatedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public required int TotalPages { get; init; }
    public required bool HasNextPage { get; init; }
    public required bool HasPreviousPage { get; init; }

    public static PaginatedResponse<T> Create(
        IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResponse<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
```

Enforce a sensible default page size (e.g., 20) and a maximum (e.g., 100)
to prevent clients from requesting unbounded result sets.

### Step 4: Wire up OpenAPI

Every ASP.NET Core Web API should have OpenAPI documentation. Check whether
the project already has OpenAPI configured before adding it.

**For .NET 9+ projects**, use the built-in ASP.NET Core OpenAPI support
(`builder.Services.AddOpenApi()` + `app.MapOpenApi()` in development).
This is all that is needed — no additional packages required.

**Do NOT add any `Swashbuckle.*` NuGet package** (`Swashbuckle.AspNetCore`,
`Swashbuckle.AspNetCore.SwaggerUI`, `Swashbuckle.AspNetCore.SwaggerGen`,
etc.) to projects that don't already use it. Swashbuckle has known
compatibility issues with .NET 9+ and .NET 10 OpenAPI types. If the project
already has Swashbuckle installed, keep it unless the user asks to remove it.

Reference: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview

**OpenAPI metadata on endpoints:** Add descriptive metadata so the generated
documentation is useful, not just a list of routes. For minimal APIs, chain
the metadata methods:

```csharp
app.MapGet("/api/products/{id}", handler)
    .WithName("GetProductById")
    .WithSummary("Get a product by ID")
    .WithDescription("Returns the full product details including category.")
    .Produces<ProductResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);
```

**Enum serialization:** Configure JSON serialization so enums appear as
readable strings in both API responses and OpenAPI schemas:

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

### Step 5: Set up error handling

Use a global exception handler so that individual endpoints do not need
try-catch blocks. Return RFC 7807 Problem Details for all error responses.

**For .NET 8+ projects**, prefer the built-in exception handler middleware:

```csharp
builder.Services.AddProblemDetails();

app.UseExceptionHandler();
app.UseStatusCodePages();
```

If the project needs custom exception-to-status-code mapping (e.g., a
`NotFoundException` should return 404), implement `IExceptionHandler`:

```csharp
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (0, (string?)null)
        };

        if (statusCode == 0)
            return false; // Let the default handler deal with it

        logger.LogWarning(exception, "Handled API exception: {Title}", title);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}
```

Register it:

```csharp
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

app.UseExceptionHandler();
```

**File placement:** Always place exception handler classes in a `Middleware/`
folder to maintain consistent project organization. Do not place them at the
project root.

### Step 6: Wire up data access

If the endpoints need database storage, follow these steps.

**1. Search for an existing DbContext.**

Look for classes that inherit `DbContext` in the project. If one exists,
add the new `DbSet<T>` properties to it. If multiple DbContexts exist,
ask the user which one to extend, or create a new one if appropriate.

**2. Use EF Core migrations — not `EnsureCreated()`.**

`EnsureCreated()` does not track schema changes and silently skips if
the database already exists. Always use migrations for projects that will
evolve:

```bash
# Install the EF Core tools if not already installed
dotnet tool install --global dotnet-ef

# Create the migration
dotnet ef migrations add Add{Entity}Table --project <project-path>

# Apply it
dotnet ef database update --project <project-path>
```

**Do not** seed data in `Program.cs`. If seed data is needed, use EF Core's
`HasData()` in `OnModelCreating` so it is captured in a migration:

```csharp
modelBuilder.Entity<Category>().HasData(
    new Category { Id = 1, Name = "Electronics" },
    new Category { Id = 2, Name = "Books" }
);
```

**3. DbContext Fluent API configuration:**

- Define unique indexes on natural keys (`HasIndex(...).IsUnique()`)
- Set cascade/restrict delete behaviors explicitly on foreign keys
- Store enums as strings: `.HasConversion<string>()`
- Specify column types for decimals: `.HasColumnType("decimal(10,2)")`

**4. Service layer between endpoints and DbContext.**

Do not inject `DbContext` directly into controllers or endpoint handlers.
Create a service interface and a sealed implementation class that owns the
data access logic and mapping between entities and request/response types.

Always define an interface for every service — this enables unit testing with
mocks and follows the Dependency Inversion Principle:

```csharp
// Services/IProductService.cs
public interface IProductService
{
    Task<PaginatedResponse<ProductResponse>> GetAllAsync(
        string? search, int page, int pageSize, CancellationToken ct);
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}

// Services/ProductService.cs
public sealed class ProductService(AppDbContext db, ILogger<ProductService> logger)
    : IProductService
{
    public async Task<PaginatedResponse<ProductResponse>> GetAllAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking().AsQueryable();
        // ... filter, order, paginate, project to DTOs
        return PaginatedResponse<ProductResponse>.Create(items, page, pageSize, totalCount);
    }

    private static ProductResponse MapToResponse(Product p) =>
        new(p.Id, p.Name, p.Price, p.Category.Name, p.IsAvailable);
}
```

Register with the interface, not the concrete type:

```csharp
// In Program.cs
builder.Services.AddScoped<IProductService, ProductService>();
```

Use `AsNoTracking()` on all read-only queries to avoid unnecessary change
tracking overhead.

### Step 7: Create a .http test file

After implementing endpoints, create a `.http` file in the project root that
demonstrates how to call every new endpoint. This serves as living
documentation and a quick manual test harness.

```http
@baseUrl = http://localhost:5000

### Get all products (paginated)
GET {{baseUrl}}/api/products?page=1&pageSize=10

### Get product by ID
GET {{baseUrl}}/api/products/1

### Create a product
POST {{baseUrl}}/api/products
Content-Type: application/json

{
  "name": "Wireless Mouse",
  "price": 29.99,
  "categoryId": 1
}

### Delete a product
DELETE {{baseUrl}}/api/products/1
```

Include at least one request per endpoint with realistic bodies. Show error
paths (e.g., non-existent IDs). Match the port to `launchSettings.json`.

### Step 8: Build and verify

1. Run `dotnet build` — confirm zero errors and zero warnings.
2. Start the app and verify the OpenAPI document loads at `/openapi/v1.json`.
3. Run the requests in the `.http` file and confirm correct status codes.
4. If migrations were created, verify the database schema matches expectations.

## Validation

- [ ] All endpoints return correct HTTP status codes per the table in Step 3
- [ ] POST endpoints return `201 Created` with a `Location` header
- [ ] DELETE endpoints return `204 No Content`
- [ ] Every endpoint signature includes `CancellationToken`
- [ ] `CancellationToken` is forwarded to all downstream async calls
- [ ] OpenAPI document is generated and includes all new endpoints
- [ ] Endpoints have summary/description metadata for OpenAPI
- [ ] Enum values appear as strings in JSON responses and OpenAPI schemas
- [ ] Error responses use RFC 7807 Problem Details format
- [ ] List endpoints support pagination with metadata in the response
- [ ] EF Core entities are not exposed directly in request/response bodies
- [ ] Read-only queries use `AsNoTracking()`
- [ ] Database changes use EF Core migrations, not `EnsureCreated()`
- [ ] A `.http` file exists with a request for every new endpoint
- [ ] `dotnet build` passes with zero errors and zero warnings
- [ ] All DTOs are `sealed record` types (not mutable classes)
- [ ] All classes are `sealed` unless explicitly designed for inheritance
- [ ] Minimal API handlers use `TypedResults` with explicit `Results<T1, T2>` return types
- [ ] Collection properties in response types use `IReadOnlyList<T>`, not `List<T>`
- [ ] Every service has a corresponding interface registered in DI
- [ ] Exception handlers are placed in the `Middleware/` folder

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| Exposing EF Core entities as API responses | Create separate `sealed record` request/response types. Entities leak navigation properties and internal fields. |
| Forgetting `CancellationToken` | Add to every endpoint and forward through the entire async call chain. |
| Using `EnsureCreated()` instead of migrations | Use `dotnet ef migrations add` / `dotnet ef database update`. `EnsureCreated()` silently ignores schema changes. |
| Seeding data in `Program.cs` | Use `HasData()` in Fluent API so seeds are versioned in migrations. |
| Returning `200 OK` from POST create | Return `201 Created` with a `Location` header. |
| Missing OpenAPI metadata | Chain `.WithName()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()` on every endpoint. |
| Injecting DbContext into controllers/handlers | Use a service layer with an interface for separation and testability. |
| Returning unbounded lists | Always paginate. Default 20, max 100. |
| Mixing controller and minimal API styles | Pick one per project and be consistent. |
| `TypedResults` in ternary without explicit return type | `Ok<T>` and `NotFound` have no common base — annotate with `Task<Results<Ok<T>, NotFound>>` or fall back to `Results` factory. |
| Using mutable classes for DTOs | Use `sealed record` with positional syntax (responses) or `init` properties (requests). |
| Not sealing types | Seal all types by default (CA1852). Enables JIT devirtualization. |
| Returning `List<T>` in responses | Use `IReadOnlyList<T>` to signal immutability. |
| Registering services without interfaces | Define `IService` and register with `AddScoped<IService, Service>()`. |
| Adding any `Swashbuckle.*` package to new .NET 9+ projects | Use built-in `AddOpenApi()` + `MapOpenApi()`. Do not add `Swashbuckle.AspNetCore`, `Swashbuckle.AspNetCore.SwaggerUI`, or any other Swashbuckle package. |

## More Info

- [ASP.NET Core Web API overview](https://learn.microsoft.com/en-us/aspnet/core/web-api/) — fundamental concepts for building Web APIs
- [OpenAPI in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview) — built-in OpenAPI support in .NET 9+
- [Minimal APIs overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview) — routing, parameter binding, and response types
- [Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors) — Problem Details and exception handling
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/) — managing schema changes with migrations
