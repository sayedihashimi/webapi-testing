# Comparative Analysis: no-skills, managedcode-dotnet-skills, dotnet-artisan, dotnet-webapi, dotnet-skills

## Introduction

This report compares **five Copilot skill configurations** used to generate identical ASP.NET Core Razor Pages applications. Each configuration produced three apps — **SparkEvents** (event registration portal), **KeystoneProperties** (property management), and **HorizonHR** (HR/employee directory) — targeting .NET 10 with EF Core SQLite.

| Configuration | Description | Apps Generated |
|---|---|---|
| **no-skills** | Baseline default Copilot (no custom skills) | 3 (62/76/60 .cs files) |
| **dotnet-artisan** | dotnet-artisan plugin chain | 3 (51/64/56 .cs files) |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | 3 (54/67/65 .cs files) |
| **dotnet-webapi** | dotnet-webapi skill | 3 (61/67/64 .cs files) |
| **managedcode-dotnet-skills** | Community managed-code skills | 3 (62/80/57 .cs files) |

All configurations produced complete, feature-organized applications with Models, Services, Pages, and Data folders. The differences lie in code quality patterns, modern C# adoption, and adherence to .NET best practices.

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Page Model Design [HIGH] | 4 | 5 | 4 | 5 | 4 |
| Form Handling & Validation [HIGH] | 4 | 4 | 4 | 4 | 4 |
| Input Model Separation [HIGH] | 5 | 5 | 5 | 5 | 5 |
| Service Abstraction [HIGH] | 5 | 4 | 5 | 5 | 5 |
| CancellationToken Propagation [HIGH] | 1 | 1 | 1 | 5 | 1 |
| Exception Handling Strategy [HIGH] | 2 | 2 | 2 | 4 | 2 |
| EF Core Relationship Config [HIGH] | 4 | 5 | 4 | 5 | 4 |
| Async/Await Best Practices [HIGH] | 4 | 4 | 4 | 4 | 4 |
| HSTS & HTTPS Redirection [HIGH] | 2 | 1 | 2 | 2 | 1 |
| Error Response Conformance [HIGH] | 3 | 3 | 3 | 4 | 3 |
| Named Handler Methods [MEDIUM] | 3 | 3 | 4 | 4 | 3 |
| Semantic HTML [MEDIUM] | 3 | 4 | 3 | 3 | 3 |
| Accessibility & ARIA [MEDIUM] | 2 | 4 | 3 | 3 | 4 |
| View Components & Reusable UI [MEDIUM] | 3 | 3 | 4 | 3 | 3 |
| Null & Empty State Handling [MEDIUM] | 3 | 4 | 4 | 4 | 4 |
| CSS Organization [MEDIUM] | 2 | 3 | 3 | 3 | 3 |
| Tag Helper Usage [MEDIUM] | 4 | 5 | 5 | 5 | 4 |
| Layout & Partial Views [MEDIUM] | 3 | 4 | 4 | 4 | 3 |
| Bootstrap Integration [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Sealed Types [MEDIUM] | 1 | 5 | 1 | 5 | 1 |
| AsNoTracking Usage [MEDIUM] | 1 | 5 | 5 | 4 | 5 |
| TempData & Flash Messages [MEDIUM] | 4 | 4 | 5 | 4 | 4 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Pagination [MEDIUM] | 3 | 4 | 4 | 5 | 3 |
| Custom Tag Helpers [MEDIUM] | 1 | 1 | 1 | 1 | 1 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Package Discipline [MEDIUM] | 3 | 3 | 3 | 3 | 3 |
| Enum Design [MEDIUM] | 4 | 5 | 4 | 4 | 4 |
| Guard Clauses [MEDIUM] | 1 | 2 | 1 | 1 | 1 |
| Dispose & Resource Management [MEDIUM] | 3 | 3 | 3 | 3 | 3 |
| File Organization [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Modern C# Adoption [LOW] | 2 | 5 | 2 | 5 | 5 |
| Code Standards Compliance [LOW] | 4 | 4 | 4 | 4 | 4 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 1 |
| NuGet Version Pinning [CRITICAL] | 1 | 4 | 1 | 1 | 1 |
| Build & Run Success [CRITICAL] | 4 | 4 | 4 | 4 | 4 |
| Security Vulnerability Scan [CRITICAL] | 3 | 3 | 3 | 3 | 3 |
| Input Validation Coverage [CRITICAL] | 4 | 5 | 4 | 4 | 4 |
| Page Completeness [CRITICAL] | 5 | 4 | 4 | 4 | 4 |
| Business Rule Implementation [CRITICAL] | 4 | 4 | 4 | 4 | 4 |
| Anti-Forgery Token Coverage [CRITICAL] | 3 | 3 | 3 | 3 | 3 |
| Built-in OpenAPI over Swashbuckle [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Minimal API for Endpoints [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| FluentValidation Usage [MEDIUM] | 1 | 1 | 1 | 1 | 1 |

---

## 1. Page Model Design [HIGH]

All configurations produce **thin page models** that delegate business logic to injected services. The key differentiators are the use of primary constructors and sealed class modifiers.

**no-skills** — Traditional constructor injection without primary constructors:
```csharp
// no-skills: Pages/Events/Create.cshtml.cs
public class CreateModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;
    public CreateModel(IEventService eventService, ICategoryService categoryService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
    }
}
```

**dotnet-webapi / dotnet-artisan** — Primary constructors with sealed classes:
```csharp
// dotnet-webapi: Pages/Events/Create.cshtml.cs
public sealed class CreateModel(
    IEventService eventService,
    IEventCategoryService categoryService,
    IVenueService venueService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | Thin models, delegates to services, but traditional constructors add boilerplate |
| dotnet-artisan | 5 | Thin + primary constructors + sealed page models + IValidatableObject |
| dotnet-skills | 4 | Thin models with traditional constructors, good separation |
| dotnet-webapi | 5 | Thin + primary constructors + sealed + excellent InputModel design |
| managedcode | 4 | Thin with primary constructors but not sealed |

**Verdict:** dotnet-webapi and dotnet-artisan lead with primary constructors and sealed page models. The sealed modifier prevents unintended inheritance and enables JIT devirtualization.

---

## 2. Form Handling & Validation [HIGH]

All configurations implement comprehensive form handling with `[BindProperty]`, `asp-validation-for`, `asp-validation-summary`, and `ModelState.IsValid` checks. The PRG (Post-Redirect-Get) pattern is consistently applied.

**Common pattern across all:**
```html
<!-- Shared pattern across all configs -->
<form method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>
    <div class="mb-3">
        <label asp-for="Input.Title" class="form-label"></label>
        <input asp-for="Input.Title" class="form-control" />
        <span asp-validation-for="Input.Title" class="text-danger"></span>
    </div>
</form>
@section Scripts { <partial name="_ValidationScriptsPartial" /> }
```

**dotnet-artisan** adds `IValidatableObject` for cross-field validation:
```csharp
// dotnet-artisan: Events/Create InputModel
public sealed class InputModel : IValidatableObject
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult("End date must be after start date.", [nameof(EndDate)]);
    }
}
```

No configuration uses explicit `[ValidateAntiForgeryToken]` — all rely on Razor Pages' built-in automatic anti-forgery validation (which is on by default for POST handlers).

| Config | Score | Justification |
|---|---|---|
| no-skills | 4 | Complete form handling, PRG pattern, validation scripts included |
| dotnet-artisan | 4 | Same plus IValidatableObject for cross-field validation |
| dotnet-skills | 4 | Standard pattern with validation scripts, ModelState checks |
| dotnet-webapi | 4 | Standard pattern, consistent across all apps |
| managedcode | 4 | Standard pattern, good coverage |

**Verdict:** All configurations implement form handling equally well. dotnet-artisan and dotnet-skills add IValidatableObject for richer validation, giving them a slight edge in complex scenarios.

---

## 3. Input Model Separation [HIGH]

All five configurations use **nested InputModel classes** for form binding, preventing over-posting attacks. No configuration binds directly to entity classes.

```csharp
// Universal pattern across all configs
[BindProperty]
public InputModel Input { get; set; } = new();

public class InputModel
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;
    [Required, Range(1, int.MaxValue)]
    public int TotalCapacity { get; set; }
}
```

| Config | Score | Justification |
|---|---|---|
| All | 5 | All use nested InputModel classes with validation attributes |

**Verdict:** Perfect score across the board. This is a critical security pattern and all configurations get it right.

---

## 4. Service Abstraction [HIGH]

All configurations implement the interface-based service pattern (`AddScoped<IService, Service>()`). The only difference is in coverage and number of services.

**Typical Program.cs registration:**
```csharp
// Common pattern
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
```

| Config | Interface Registrations | Notes |
|---|---|---|
| no-skills | 20 | All interface-based |
| dotnet-artisan | 15 (+ 1 concrete) | One concrete DI registration found |
| dotnet-skills | 21 | All interface-based |
| dotnet-webapi | 21 | All interface-based |
| managedcode | 21 | All interface-based |

| Config | Score | Justification |
|---|---|---|
| no-skills | 5 | 20 interface-based registrations, zero concrete |
| dotnet-artisan | 4 | 15 interface-based but 1 concrete registration found |
| dotnet-skills | 5 | 21 fully interface-based |
| dotnet-webapi | 5 | 21 fully interface-based |
| managedcode | 5 | 21 fully interface-based |

**Verdict:** All configurations implement excellent dependency inversion. dotnet-artisan has a minor inconsistency with one concrete registration.

---

## 5. CancellationToken Propagation [HIGH]

This is the **most dramatic differentiator** across all configurations. dotnet-webapi is the only configuration that consistently propagates CancellationToken from page handlers through services to EF Core calls.

**dotnet-webapi** — Full propagation (420 mentions across all apps):
```csharp
// dotnet-webapi: Page handler accepts CancellationToken
public async Task OnGetAsync(CancellationToken ct)
{
    await LoadDropdownsAsync(ct);
}

public async Task<IActionResult> OnPostAsync(CancellationToken ct)
{
    await _eventService.CreateAsync(evt, ct);
    return RedirectToPage("Details", new { id = evt.Id });
}

// dotnet-webapi: Service propagates to EF Core
public async Task<Event> CreateAsync(Event evt, CancellationToken ct = default)
{
    db.Events.Add(evt);
    await db.SaveChangesAsync(ct);  // ← Propagated to EF Core
    return evt;
}
```

**All other configs** — CancellationToken absent from handlers and services:
```csharp
// no-skills / dotnet-artisan / dotnet-skills / managedcode
public async Task<IActionResult> OnPostAsync()  // ← No CancellationToken
{
    await _eventService.CreateEventAsync(evt);   // ← No token passed
}
```

| Config | CancellationToken Mentions | Score | Justification |
|---|---|---|---|
| no-skills | 4 | 1 | Only in DbContext overrides, not propagated |
| dotnet-artisan | 101 | 1 | Present in some services but NOT in page handlers |
| dotnet-skills | 4 | 1 | Only in DbContext, not propagated |
| dotnet-webapi | 420 | 5 | Full propagation: handlers → services → EF Core |
| managedcode | 6 | 1 | Minimal, not meaningfully propagated |

**Verdict:** dotnet-webapi is the clear winner. CancellationToken propagation prevents wasted server resources on cancelled HTTP requests — a critical production concern. Note: dotnet-artisan has partial coverage (101 mentions, mainly in services) but fails to accept tokens in page handlers, making the propagation incomplete.

---

## 6. Exception Handling Strategy [HIGH]

**dotnet-webapi** implements the modern `IExceptionHandler` interface with `ProblemDetails`, while other configs use basic `UseExceptionHandler("/Error")`.

**dotnet-webapi** — Modern IExceptionHandler:
```csharp
// dotnet-webapi: Middleware/GlobalExceptionHandler.cs
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        httpContext.Response.StatusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        return false;
    }
}

// Program.cs registration
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
```

**Other configs** — Standard error page only:
```csharp
// no-skills / dotnet-artisan / dotnet-skills / managedcode
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
```

| Config | Score | Justification |
|---|---|---|
| no-skills | 2 | Standard error page, no custom exception mapping |
| dotnet-artisan | 2 | Standard error page, uses InvalidOperationException for business rules |
| dotnet-skills | 2 | Standard error page only |
| dotnet-webapi | 4 | IExceptionHandler + ProblemDetails + status code mapping per exception type |
| managedcode | 2 | Standard error page only |

**Verdict:** dotnet-webapi leads with the modern .NET 8+ `IExceptionHandler` pattern. Other configs lack any structured exception-to-HTTP-status mapping.

---

## 7. EF Core Relationship Configuration [HIGH]

All configurations use Fluent API in `OnModelCreating`, but the depth varies significantly.

**dotnet-webapi / dotnet-artisan** — Comprehensive with HasConversion and decimal precision:
```csharp
// dotnet-webapi: SparkEventsDbContext.cs
modelBuilder.Entity<Event>(entity =>
{
    entity.HasOne(e => e.EventCategory)
        .WithMany(c => c.Events)
        .HasForeignKey(e => e.EventCategoryId)
        .OnDelete(DeleteBehavior.Restrict);

    entity.Property(e => e.Status)
        .HasConversion<string>();  // Enum stored as string

    entity.Property(t => t.Price)
        .HasColumnType("decimal(10,2)");
});
```

**no-skills** — Good Fluent API but no HasConversion:
```csharp
// no-skills: SparkEventsDbContext.cs
modelBuilder.Entity<Event>(entity =>
{
    entity.HasOne(e => e.EventCategory)
        .WithMany(c => c.Events)
        .HasForeignKey(e => e.EventCategoryId)
        .OnDelete(DeleteBehavior.Restrict);
    // No HasConversion for enums
});
```

| Config | HasConversion | Decimal Precision | Unique Indexes | Score |
|---|---|---|---|---|
| no-skills | 0 | Yes | Yes | 4 |
| dotnet-artisan | 2 | Yes | Yes | 5 |
| dotnet-skills | 0 | Yes | Yes | 4 |
| dotnet-webapi | 20 | Yes | Yes | 5 |
| managedcode | 2 | Yes | Yes | 4 |

**Verdict:** dotnet-webapi leads with 20 `HasConversion` calls ensuring enums are stored as readable strings. dotnet-artisan also uses this pattern effectively.

---

## 8. Async/Await Best Practices [HIGH]

All configurations follow the `Async` suffix convention consistently. No sync-over-async anti-patterns were detected.

```csharp
// Universal pattern
public async Task<Event> CreateEventAsync(Event evt) { ... }
public async Task<IActionResult> OnPostAsync() { ... }
public async Task OnGetAsync() { ... }
```

| Config | Score | Justification |
|---|---|---|
| All | 4 | Consistent Async suffix, no sync-over-async. Not 5 because none use ConfigureAwait or return Task directly for simple pass-through |

**Verdict:** Tie. All configurations implement async/await correctly.

---

## 9. HSTS & HTTPS Redirection [HIGH]

HSTS and HTTPS enforcement is inconsistent across all configurations — no config applies it to all 3 apps.

| Config | Apps with HSTS | Apps with HTTPS Redirect | Score |
|---|---|---|---|
| no-skills | 2/3 (HR, Keystone) | 2/3 | 2 |
| dotnet-artisan | 0/3 | 0/3 | 1 |
| dotnet-skills | 1/3 (SparkEvents) | 1/3 | 2 |
| dotnet-webapi | 1/3 (SparkEvents) | 1/3 | 2 |
| managedcode | 0/3 | 0/3 | 1 |

**Verdict:** All configurations fail to consistently apply HSTS/HTTPS. no-skills covers 2/3 apps, making it the relative leader in this weak area. dotnet-artisan and managedcode omit HSTS entirely.

---

## 10. Error Response Conformance [HIGH]

**dotnet-webapi** provides the most consistent error handling with its `IExceptionHandler` middleware plus `StatusCodePagesWithReExecute` in some apps.

| Config | Score | Justification |
|---|---|---|
| no-skills | 3 | UseExceptionHandler("/Error"), TempData for user messages |
| dotnet-artisan | 3 | Same pattern, InvalidOperationException for business errors |
| dotnet-skills | 3 | StatusCodePagesWithReExecute in 1 app |
| dotnet-webapi | 4 | IExceptionHandler + StatusCodePages + consistent error mapping |
| managedcode | 3 | Standard error handling |

**Verdict:** dotnet-webapi leads with the most structured error pipeline.

---

## 11. Named Handler Methods [MEDIUM]

Pages with multiple actions (e.g., publish + cancel on an event details page) should use named handlers like `OnPostPublishAsync` instead of a single `OnPost`.

```csharp
// Good pattern (dotnet-webapi, dotnet-skills)
public async Task<IActionResult> OnPostPublishAsync(int id, CancellationToken ct) { ... }
public async Task<IActionResult> OnPostCancelAsync(int id, CancellationToken ct) { ... }
public async Task<IActionResult> OnPostToggleActiveAsync(int eventId, int ticketTypeId) { ... }
```

| Config | Named Handlers Found | Score | Justification |
|---|---|---|---|
| no-skills | 7 | 3 | Some named handlers, mostly action-specific pages |
| dotnet-artisan | 6 | 3 | Named handlers for publish/toggle actions |
| dotnet-skills | 8 | 4 | Good coverage with multiple named handlers per page |
| dotnet-webapi | 8 | 4 | Consistent named handler usage |
| managedcode | 6 | 3 | Basic named handlers |

**Verdict:** dotnet-skills and dotnet-webapi have the best named handler coverage.

---

## 12. Semantic HTML [MEDIUM]

```html
<!-- dotnet-artisan: _Layout.cshtml -->
<header>
    <nav class="navbar navbar-expand-sm" aria-label="Main navigation">
        ...
    </nav>
</header>
<main role="main" class="container my-4">
    @RenderBody()
</main>
<footer class="border-top footer text-muted">
    &copy; 2026 Spark Events
</footer>
```

| Config | Semantic Element Count | Score | Justification |
|---|---|---|---|
| no-skills | 102 | 3 | Good `<section>`, `<nav>` usage but inconsistent |
| dotnet-artisan | 75 | 4 | Fewer elements but more semantically correct usage |
| dotnet-skills | 85 | 3 | Good baseline semantic structure |
| dotnet-webapi | 61 | 3 | Adequate semantic HTML |
| managedcode | 90 | 3 | Good semantic coverage |

**Verdict:** All configs use semantic HTML to a reasonable degree. dotnet-artisan uses elements more purposefully despite a lower raw count.

---

## 13. Accessibility & ARIA [MEDIUM]

ARIA attribute usage varies significantly. managedcode and dotnet-artisan lead.

```html
<!-- dotnet-artisan: _Layout.cshtml -->
<nav aria-label="Main navigation">
<a class="nav-link" aria-current="page">Dashboard</a>
<button aria-label="Toggle navigation" aria-controls="navbarNav" aria-expanded="false">
<div class="alert alert-success" role="alert">
    <button class="btn-close" aria-label="Close"></button>
</div>
```

| Config | aria-* Count | role="alert" Count | Score |
|---|---|---|---|
| no-skills | 15 | 19 | 2 |
| dotnet-artisan | 83 | 11 | 4 |
| dotnet-skills | 70 | 20 | 3 |
| dotnet-webapi | 66 | 13 | 3 |
| managedcode | 102 | 22 | 4 |

**Verdict:** managedcode (102 ARIA attributes) and dotnet-artisan (83) lead accessibility. no-skills has the fewest ARIA attributes (15), relying on implicit accessibility.

---

## 14. View Components & Reusable UI [MEDIUM]

No configuration uses formal `ViewComponent` classes. All use partial views for status badges and pagination.

| Config | Custom Partials | Notable Partials | Score |
|---|---|---|---|
| no-skills | 7 | _Pagination, _StatusBadge, _RegistrationStatusBadge | 3 |
| dotnet-artisan | 8 | _Pagination, _StatusBadge, _DepartmentRow | 3 |
| dotnet-skills | 8 | _FlashMessages, _Pagination, _StatusBadge, _EventStatusBadge | 4 |
| dotnet-webapi | 6 | _Pagination, _StatusBadge | 3 |
| managedcode | 5 | _Pagination, _StatusBadge | 3 |

**Verdict:** dotnet-skills edges ahead with a dedicated `_FlashMessages` partial and separate status badge partials per entity type.

---

## 15. Null & Empty State Handling [MEDIUM]

Most configs provide "No items found" messages for empty collections, but coverage varies.

```html
<!-- Common pattern -->
@if (!Model.Events.Items.Any())
{
    <div class="alert alert-info">No events found matching your criteria.</div>
}
```

| Config | Empty State Messages | Score |
|---|---|---|
| no-skills | 41 | 3 |
| dotnet-artisan | 32 | 4 |
| dotnet-skills | 35 | 4 |
| dotnet-webapi | 44 | 4 |
| managedcode | 36 | 4 |

**Verdict:** All skill-enhanced configs score higher than baseline due to more consistent null checks and guard patterns in services.

---

## 16. CSS Organization [MEDIUM]

No configuration uses inline styles excessively. All rely primarily on Bootstrap with a `site.css` for customization.

| Config | Inline style="" | @Html.* (legacy) | Score |
|---|---|---|---|
| no-skills | 5 | 10 | 2 |
| dotnet-artisan | 17 | 0 | 3 |
| dotnet-skills | 21 | 0 | 3 |
| dotnet-webapi | 21 | 0 | 3 |
| managedcode | 11 | 21 | 3 |

**Verdict:** no-skills uses some legacy `@Html.*` helpers (10 occurrences) and managedcode uses 21. dotnet-artisan, dotnet-skills, and dotnet-webapi use zero legacy helpers.

---

## 17. Tag Helper Usage [MEDIUM]

Modern `asp-*` tag helpers are the standard across all configurations.

| Config | asp-for | asp-page | asp-items | @Html.* (legacy) | Score |
|---|---|---|---|---|---|
| no-skills | 497 | 244 | 46 | 10 | 4 |
| dotnet-artisan | 562 | 247 | 69 | 0 | 5 |
| dotnet-skills | 546 | 235 | 45 | 0 | 5 |
| dotnet-webapi | 522 | 261 | 47 | 0 | 5 |
| managedcode | 523 | 229 | 46 | 21 | 4 |

**Verdict:** dotnet-artisan, dotnet-skills, and dotnet-webapi use zero legacy `@Html.*` helpers, earning a perfect score.

---

## 18. Layout & Partial Views [MEDIUM]

All configs use `_Layout.cshtml`, `_ViewStart.cshtml`, `_ViewImports.cshtml`, and `_ValidationScriptsPartial`. dotnet-artisan and dotnet-skills consistently include `@section Scripts` for page-specific JavaScript.

| Config | @section Scripts | Partial Views | _ValidationScriptsPartial | Score |
|---|---|---|---|---|
| no-skills | 15 | 82 | 16 | 3 |
| dotnet-artisan | 29 | 89 | 29 | 4 |
| dotnet-skills | 40 | 91 | 41 | 4 |
| dotnet-webapi | 28 | 62 | 28 | 4 |
| managedcode | 29 | 72 | 29 | 3 |

**Verdict:** dotnet-skills leads with the most consistent validation script inclusion (41 instances matching 40 script sections, indicating near-perfect coverage).

---

## 19. Bootstrap Integration [MEDIUM]

All configurations use Bootstrap 5 effectively with cards, forms, tables, badges, and responsive grid.

```html
<!-- Universal high-quality Bootstrap usage -->
<div class="card mb-4">
    <div class="card-header"><h5 class="mb-0">Basic Information</h5></div>
    <div class="card-body">
        <div class="row g-3">
            <div class="col-md-6 mb-3">
                <input asp-for="Input.Title" class="form-control" />
            </div>
        </div>
    </div>
</div>
<div class="d-flex gap-2">
    <button type="submit" class="btn btn-primary">Save</button>
    <a asp-page="Index" class="btn btn-outline-secondary">Cancel</a>
</div>
```

| Config | Score | Justification |
|---|---|---|
| All | 4 | Consistent, professional Bootstrap 5 usage across all apps |

**Verdict:** Tie. All configurations demonstrate solid Bootstrap integration.

---

## 20. Sealed Types [MEDIUM]

The most binary differentiator. dotnet-webapi and dotnet-artisan seal nearly everything; others seal almost nothing.

```csharp
// dotnet-webapi / dotnet-artisan pattern
public sealed class EventService(SparkEventsDbContext db, ILogger<EventService> logger)
    : IEventService { }
public sealed class CreateModel : PageModel { }
public sealed class SparkEventsDbContext : DbContext { }
public sealed class DataSeeder { }
```

| Config | Sealed Count | Score | Justification |
|---|---|---|---|
| no-skills | 0 | 1 | No sealed types anywhere |
| dotnet-artisan | 136 | 5 | All models, services, page models, DbContext sealed |
| dotnet-skills | 6 | 1 | Only a handful of sealed types |
| dotnet-webapi | 187 | 5 | Comprehensive sealing of all non-inherited types |
| managedcode | 0 | 1 | No sealed types |

**Verdict:** dotnet-webapi (187) and dotnet-artisan (136) lead dramatically. Sealed types enable JIT devirtualization optimizations and explicitly communicate design intent.

---

## 21. AsNoTracking Usage [MEDIUM]

A major performance differentiator. All skill-enhanced configs except no-skills use `AsNoTracking()` for read-only queries.

```csharp
// dotnet-artisan / dotnet-skills / managedcode pattern
var query = db.Events
    .Include(e => e.EventCategory)
    .Include(e => e.Venue)
    .AsNoTracking()  // ← Skips change tracking for reads
    .AsQueryable();
```

| Config | AsNoTracking Count | Score | Justification |
|---|---|---|---|
| no-skills | 0 | 1 | Never used — all queries track entities |
| dotnet-artisan | 69 | 5 | Consistently applied to all read-only queries |
| dotnet-skills | 70 | 5 | Consistently applied |
| dotnet-webapi | 53 | 4 | Broadly applied but slightly less consistent |
| managedcode | 72 | 5 | Most consistent application |

**Verdict:** All skill-enhanced configurations massively outperform the baseline. `AsNoTracking()` reduces memory usage and improves query performance for read paths.

---

## 22. TempData & Flash Messages [MEDIUM]

All configurations implement TempData-based flash messaging with Bootstrap alerts.

```csharp
// Common pattern
TempData["SuccessMessage"] = "Event created successfully.";
return RedirectToPage("Details", new { id = evt.Id });
```

```html
<!-- _Layout.cshtml -->
@if (TempData["SuccessMessage"] is string successMsg)
{
    <div class="alert alert-success alert-dismissible fade show" role="alert">
        @successMsg
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>
}
```

dotnet-skills extracts this into a dedicated `_FlashMessages.cshtml` partial.

| Config | TempData Count | Score | Justification |
|---|---|---|---|
| no-skills | 72 | 4 | Good coverage, inline in layout |
| dotnet-artisan | 70 | 4 | Good coverage, inline in layout |
| dotnet-skills | 91 | 5 | Highest count + dedicated _FlashMessages partial |
| dotnet-webapi | 90 | 4 | High count, inline in layout |
| managedcode | 88 | 4 | Good coverage |

**Verdict:** dotnet-skills leads by extracting flash messages into a reusable partial view.

---

## 23. Data Seeder Design [MEDIUM]

All configurations use a static `DataSeeder` class called from `Program.cs` with idempotency checks.

```csharp
// Universal pattern
public static async Task SeedAsync(SparkEventsDbContext db)
{
    if (await db.EventCategories.AnyAsync()) return;  // Idempotency guard
    var categories = new List<EventCategory> { ... };
    db.EventCategories.AddRange(categories);
    await db.SaveChangesAsync();
}
```

| Config | Score | Justification |
|---|---|---|
| All | 4 | Async seeding, idempotency guards, called from Program.cs. Not 5 because none use injectable services |

**Verdict:** Tie. All configurations implement adequate data seeding.

---

## 24. Pagination [MEDIUM]

All configs implement pagination, but the quality of the reusable component varies.

**dotnet-webapi** — Most comprehensive with 79 pagination-related mentions:
```csharp
// dotnet-webapi: Models/PaginatedList.cs
public sealed class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static async Task<PaginatedList<T>> CreateAsync(
        IQueryable<T> source, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var count = await source.CountAsync(ct);
        var items = await source.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize).ToListAsync(ct);
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
```

| Config | Pagination References | Score | Justification |
|---|---|---|---|
| no-skills | 16 | 3 | PaginatedList exists but limited reuse |
| dotnet-artisan | 60 | 4 | Good reuse across apps with _Pagination partial |
| dotnet-skills | 54 | 4 | Reusable PaginatedList&lt;T&gt; + partial view |
| dotnet-webapi | 79 | 5 | Most extensive, sealed class, IReadOnlyList, CancellationToken |
| managedcode | 32 | 3 | Basic pagination |

**Verdict:** dotnet-webapi leads with the most robust `PaginatedList<T>` implementation including `IReadOnlyList`, sealed class, and CancellationToken.

---

## 25. Custom Tag Helpers [MEDIUM]

No configuration creates custom `TagHelper` classes. All rely on built-in `asp-*` tag helpers and partial views.

| Config | Score | Justification |
|---|---|---|
| All | 1 | No custom TagHelper classes found in any configuration |

**Verdict:** Tie at the lowest score. A missed opportunity across all configurations.

---

## 26. Structured Logging [MEDIUM]

All configurations inject `ILogger<T>` and use structured message templates.

```csharp
// Universal pattern
logger.LogInformation("Created event {EventTitle} (ID: {EventId})", evt.Title, evt.Id);
logger.LogInformation("Cancelled event {EventId}. {Count} registrations affected.",
    id, activeRegistrations.Count);
```

No configuration uses `[LoggerMessage]` source generators for high-performance logging.

| Config | ILogger&lt;T&gt; Count | Score | Justification |
|---|---|---|---|
| no-skills | 24 | 4 | Good structured templates |
| dotnet-artisan | 12 | 4 | Fewer but properly structured |
| dotnet-skills | 42 | 4 | Most logging calls, all structured |
| dotnet-webapi | 22 | 4 | Properly structured |
| managedcode | 18 | 4 | Properly structured |

**Verdict:** Tie. All use proper structured logging. dotnet-skills has the most logging instrumentation.

---

## 27. Nullable Reference Types [MEDIUM]

All configurations enable `<Nullable>enable</Nullable>` in their `.csproj` files and use nullable annotations (`?`) on optional properties.

```xml
<!-- Universal -->
<Nullable>enable</Nullable>
```

```csharp
// Proper nullable annotation usage
public Employee? Manager { get; set; }
public Department Department { get; set; } = null!;
```

| Config | Score | Justification |
|---|---|---|
| All | 4 | Enabled in all .csproj files, proper annotations on models |

**Verdict:** Tie. All configurations correctly enable nullable reference types.

---

## 28. Package Discipline [MEDIUM]

All configurations use minimal package sets — only EF Core SQLite and Design packages. None include Swashbuckle or unnecessary dependencies.

| Config | Packages per App | Unnecessary Packages | Score |
|---|---|---|---|
| no-skills | 2 | None | 3 |
| dotnet-artisan | 2 | None | 3 |
| dotnet-skills | 2-3 | EF Tools in some | 3 |
| dotnet-webapi | 1-3 | EF Tools in some | 3 |
| managedcode | 1-2 | None | 3 |

**Verdict:** Tie. All configurations maintain lean dependencies. Some include `Microsoft.EntityFrameworkCore.Tools` unnecessarily in web projects, but this is minor.

---

## 29. Enum Design [MEDIUM]

All configurations use enums for status fields with proper singular naming.

```csharp
// Common enum pattern
public enum EventStatus { Draft, Published, SoldOut, Completed, Cancelled }
public enum LeaseStatus { Pending, Active, Expired, Renewed, Terminated }
public enum EmploymentType { FullTime, PartTime, Contract, Intern }
```

| Config | HasConversion&lt;string&gt;() | Score | Justification |
|---|---|---|---|
| no-skills | 0 | 4 | Proper enums, stored as integers (convention) |
| dotnet-artisan | 2 | 5 | Proper enums + HasConversion for readable DB values |
| dotnet-skills | 0 | 4 | Proper enums, stored as integers |
| dotnet-webapi | 20 | 4 | Extensive HasConversion but same naming quality |
| managedcode | 2 | 4 | Proper enums with some HasConversion |

**Verdict:** dotnet-artisan edges ahead by combining proper naming with string enum storage for debuggability.

---

## 30. Guard Clauses & Argument Validation [MEDIUM]

No configuration uses modern `ArgumentNullException.ThrowIfNull()` patterns. dotnet-artisan uses null-coalescing throw for entity lookups.

```csharp
// dotnet-artisan pattern (partial guard)
var evt = await db.Events.FirstOrDefaultAsync(e => e.Id == eventId)
    ?? throw new InvalidOperationException("Event not found.");
```

| Config | ThrowIfNull Count | Score | Justification |
|---|---|---|---|
| no-skills | 0 | 1 | No guard clauses |
| dotnet-artisan | 0 | 2 | Uses ?? throw for entity lookups |
| dotnet-skills | 0 | 1 | No guard clauses |
| dotnet-webapi | 0 | 1 | No guard clauses |
| managedcode | 0 | 1 | No guard clauses |

**Verdict:** All configurations lack formal guard clause patterns. dotnet-artisan partially compensates with `?? throw new InvalidOperationException()`.

---

## 31. Dispose & Resource Management [MEDIUM]

All configurations rely on DI-managed lifetimes for `DbContext`. `using` blocks appear only in `Program.cs` for the seeding scope.

| Config | Score | Justification |
|---|---|---|
| All | 3 | Correct DI lifetime management, using blocks for scoped operations |

**Verdict:** Tie. All configurations handle resource management adequately via DI.

---

## 32. File Organization [MEDIUM]

All configurations use feature-based page organization under `Pages/` with domain-specific subfolders.

```
Pages/
├── Events/      (Create, Edit, Details, Index, Cancel, Complete, Register, etc.)
├── Attendees/   (Create, Edit, Details, Index)
├── Categories/  (Create, Edit, Delete, Index)
├── Venues/      (Create, Edit, Delete, Details, Index)
├── Registrations/ (Cancel, Details)
└── CheckIn/     (Index, Process)
```

dotnet-webapi additionally creates a `Middleware/` folder for exception handling.

| Config | Score | Justification |
|---|---|---|
| All | 4 | Feature-organized, clear folder structure |

**Verdict:** Tie. All configurations follow the same organizational pattern.

---

## 33. Modern C# Adoption [LOW]

The most visible stylistic differentiator.

**dotnet-artisan / dotnet-webapi / managedcode** — Primary constructors and collection expressions:
```csharp
// Primary constructor (C# 12)
public sealed class EventService(SparkEventsDbContext db, ILogger<EventService> logger)
    : IEventService { }

// Collection expression (C# 12)
public ICollection<TicketType> TicketTypes { get; set; } = [];
public List<SelectListItem> Categories { get; set; } = [];
```

**no-skills / dotnet-skills** — Traditional constructors:
```csharp
// Traditional pattern
public class EventService : IEventService
{
    private readonly SparkEventsDbContext _db;
    private readonly ILogger<EventService> _logger;
    public EventService(SparkEventsDbContext db, ILogger<EventService> logger)
    {
        _db = db;
        _logger = logger;
    }
}
```

| Config | Primary Constructors | Collection Expressions | Score |
|---|---|---|---|
| no-skills | 0 | 0 | 2 |
| dotnet-artisan | 109 | 76 | 5 |
| dotnet-skills | 0 | 14 | 2 |
| dotnet-webapi | 88 | 71 | 5 |
| managedcode | 115 | 95 | 5 |

**Verdict:** dotnet-artisan, dotnet-webapi, and managedcode fully embrace modern C# 12 features. no-skills and dotnet-skills stick with traditional patterns.

---

## 34. Code Standards Compliance [LOW]

All configurations follow .NET naming guidelines: PascalCase for public members, camelCase for parameters, Async suffix on async methods, I prefix on interfaces.

| Config | Score | Justification |
|---|---|---|
| All | 4 | Consistent naming, proper suffixes |

**Verdict:** Tie. No significant naming standard violations detected.

---

## 35. EF Migration Usage [CRITICAL]

**All configurations use `EnsureCreated()` instead of EF Core migrations.** This is the single most critical shared failure.

```csharp
// Universal anti-pattern
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SparkEventsDbContext>();
    await db.Database.EnsureCreatedAsync();  // ← Bypasses migrations
    await DataSeeder.SeedAsync(db);
}
```

| Config | Score | Justification |
|---|---|---|
| All | 1 | EnsureCreated bypasses migrations. No MigrateAsync() anywhere |

**Verdict:** All configurations fail equally. `EnsureCreated()` makes schema evolution impossible and would cause data loss on model changes in production.

---

## 36. NuGet Version Pinning [CRITICAL]

dotnet-artisan is the only configuration that pins specific package versions in some apps.

```xml
<!-- dotnet-artisan: Pinned versions (SparkEvents, HorizonHR) -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />

<!-- All others: Wildcard versions -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

| Config | Wildcard Refs | Pinned Refs | Score |
|---|---|---|---|
| no-skills | 6 | 0 | 1 |
| dotnet-artisan | 2 | 4 | 4 |
| dotnet-skills | 7 | 0 | 1 |
| dotnet-webapi | 6 | 0 | 1 |
| managedcode | 5 | 0 | 1 |

**Verdict:** dotnet-artisan is the clear winner with pinned versions (`10.0.5`) in 2 of 3 apps. All other configs use wildcard versions that risk non-reproducible builds.

---

## 37. Build & Run Success [CRITICAL]

All configurations generated apps that produced SQLite database files (`.db`), indicating successful builds and runs during generation.

| Config | Score | Justification |
|---|---|---|
| All | 4 | All apps have .db files indicating successful startup |

**Verdict:** Tie. All configurations produce buildable applications.

---

## 38. Security Vulnerability Scan [CRITICAL]

Without running `dotnet list package --vulnerable`, we assess baseline risk from wildcard versions and package choices.

| Config | Score | Justification |
|---|---|---|
| All | 3 | Minimal, mainstream packages. Wildcard versions increase theoretical risk |

**Verdict:** Tie. All configurations use minimal packages. dotnet-artisan's pinned versions slightly reduce risk.

---

## 39. Input Validation Coverage [CRITICAL]

All configurations implement validation attributes and `ModelState.IsValid` checks. dotnet-artisan adds `IValidatableObject` for cross-field validation.

| Config | ModelState Checks | IValidatableObject | Score |
|---|---|---|---|
| no-skills | 40 | 0 | 4 |
| dotnet-artisan | 43 | 2 | 5 |
| dotnet-skills | 41 | 2 | 4 |
| dotnet-webapi | 40 | 0 | 4 |
| managedcode | 39 | 0 | 4 |

**Verdict:** dotnet-artisan leads with both data annotations and `IValidatableObject` for complex validation scenarios.

---

## 40. Page Completeness [CRITICAL]

All configurations generate a comprehensive set of CRUD pages for each scenario. Page counts are comparable.

| Config | SparkEvents | KeystoneProperties | HorizonHR | Total | Score |
|---|---|---|---|---|---|
| no-skills | 34 | 36 | 31 | 101 | 5 |
| dotnet-artisan | 33 | 36 | 32 | 101 | 4 |
| dotnet-skills | 32 | 36 | 33 | 101 | 4 |
| dotnet-webapi | 31 | 36 | 31 | 98 | 4 |
| managedcode | 33 | 35 | 31 | 99 | 4 |

**Verdict:** All configurations generate near-complete page sets. no-skills has the highest total page count.

---

## 41. Business Rule Implementation [CRITICAL]

All configurations implement core business rules such as capacity checks, waitlist management, late fee calculations, and leave balance enforcement.

| Config | Score | Justification |
|---|---|---|
| All | 4 | Core business rules implemented. Some edge cases may be inconsistently enforced |

**Verdict:** Tie. All configurations implement the core business rules from the specifications.

---

## 42. Anti-Forgery Token Coverage [CRITICAL]

All Razor Pages applications get automatic anti-forgery validation on POST handlers by default. No configuration adds explicit `[ValidateAntiForgeryToken]` attributes — but this is acceptable because Razor Pages includes anti-forgery by convention.

| Config | Score | Justification |
|---|---|---|
| All | 3 | Relies on Razor Pages default anti-forgery. Functional but not explicit |

**Verdict:** Tie. All configurations benefit from Razor Pages' built-in CSRF protection.

---

## 43. Built-in OpenAPI over Swashbuckle [CRITICAL]

No configuration includes Swashbuckle or any OpenAPI packages, which is correct for server-rendered Razor Pages apps.

| Config | Score | Justification |
|---|---|---|
| All | 5 | No Swashbuckle found. Correct for Razor Pages apps |

---

## 44. Minimal API for Endpoints [CRITICAL]

No configuration uses API controllers. All interactions are through Razor Pages.

| Config | Score | Justification |
|---|---|---|
| All | 5 | Pure Razor Pages architecture, no controllers |

---

## 45. FluentValidation Usage [MEDIUM]

No configuration uses FluentValidation. All rely on Data Annotations and (in some cases) `IValidatableObject`.

| Config | Score | Justification |
|---|---|---|
| All | 1 | FluentValidation not used. Data Annotations used instead |

---

## Weighted Summary

Weights: **Critical × 3**, **High × 2**, **Medium × 1**, **Low × 0.5**

### Critical Dimensions (×3)

| Dimension | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| EF Migration Usage | 1 | 1 | 1 | 1 | 1 |
| NuGet Version Pinning | 1 | 4 | 1 | 1 | 1 |
| Build & Run Success | 4 | 4 | 4 | 4 | 4 |
| Security Vulnerability Scan | 3 | 3 | 3 | 3 | 3 |
| Input Validation Coverage | 4 | 5 | 4 | 4 | 4 |
| Page Completeness | 5 | 4 | 4 | 4 | 4 |
| Business Rule Implementation | 4 | 4 | 4 | 4 | 4 |
| Anti-Forgery Token Coverage | 3 | 3 | 3 | 3 | 3 |
| Built-in OpenAPI over Swashbuckle | 5 | 5 | 5 | 5 | 5 |
| Minimal API for Endpoints | 5 | 5 | 5 | 5 | 5 |
| **Critical Subtotal (×3)** | **105** | **114** | **102** | **102** | **102** |

### High Dimensions (×2)

| Dimension | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Page Model Design | 4 | 5 | 4 | 5 | 4 |
| Form Handling & Validation | 4 | 4 | 4 | 4 | 4 |
| Input Model Separation | 5 | 5 | 5 | 5 | 5 |
| Service Abstraction | 5 | 4 | 5 | 5 | 5 |
| CancellationToken Propagation | 1 | 1 | 1 | 5 | 1 |
| Exception Handling Strategy | 2 | 2 | 2 | 4 | 2 |
| EF Core Relationship Config | 4 | 5 | 4 | 5 | 4 |
| Async/Await Best Practices | 4 | 4 | 4 | 4 | 4 |
| HSTS & HTTPS Redirection | 2 | 1 | 2 | 2 | 1 |
| Error Response Conformance | 3 | 3 | 3 | 4 | 3 |
| **High Subtotal (×2)** | **68** | **68** | **68** | **86** | **66** |

### Medium Dimensions (×1)

| Dimension | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Named Handler Methods | 3 | 3 | 4 | 4 | 3 |
| Semantic HTML | 3 | 4 | 3 | 3 | 3 |
| Accessibility & ARIA | 2 | 4 | 3 | 3 | 4 |
| View Components & Reusable UI | 3 | 3 | 4 | 3 | 3 |
| Null & Empty State Handling | 3 | 4 | 4 | 4 | 4 |
| CSS Organization | 2 | 3 | 3 | 3 | 3 |
| Tag Helper Usage | 4 | 5 | 5 | 5 | 4 |
| Layout & Partial Views | 3 | 4 | 4 | 4 | 3 |
| Bootstrap Integration | 4 | 4 | 4 | 4 | 4 |
| Sealed Types | 1 | 5 | 1 | 5 | 1 |
| AsNoTracking Usage | 1 | 5 | 5 | 4 | 5 |
| TempData & Flash Messages | 4 | 4 | 5 | 4 | 4 |
| Data Seeder Design | 4 | 4 | 4 | 4 | 4 |
| Pagination | 3 | 4 | 4 | 5 | 3 |
| Custom Tag Helpers | 1 | 1 | 1 | 1 | 1 |
| Structured Logging | 4 | 4 | 4 | 4 | 4 |
| Nullable Reference Types | 4 | 4 | 4 | 4 | 4 |
| Package Discipline | 3 | 3 | 3 | 3 | 3 |
| Enum Design | 4 | 5 | 4 | 4 | 4 |
| Guard Clauses | 1 | 2 | 1 | 1 | 1 |
| Dispose & Resource Management | 3 | 3 | 3 | 3 | 3 |
| File Organization | 4 | 4 | 4 | 4 | 4 |
| FluentValidation Usage | 1 | 1 | 1 | 1 | 1 |
| **Medium Subtotal (×1)** | **65** | **83** | **79** | **81** | **74** |

### Low Dimensions (×0.5)

| Dimension | no-skills | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode |
|---|---|---|---|---|---|
| Modern C# Adoption | 2 | 5 | 2 | 5 | 5 |
| Code Standards Compliance | 4 | 4 | 4 | 4 | 4 |
| **Low Subtotal (×0.5)** | **3** | **4.5** | **3** | **4.5** | **4.5** |

### Total Weighted Scores

| Configuration | Critical (×3) | High (×2) | Medium (×1) | Low (×0.5) | **Total** |
|---|---|---|---|---|---|
| **🥇 dotnet-webapi** | 102 | 86 | 81 | 4.5 | **273.5** |
| **🥈 dotnet-artisan** | 114 | 68 | 83 | 4.5 | **269.5** |
| **🥉 dotnet-skills** | 102 | 68 | 79 | 3 | **252** |
| **4th: managedcode** | 102 | 66 | 74 | 4.5 | **246.5** |
| **5th: no-skills** | 105 | 68 | 65 | 3 | **241** |

---

## What All Versions Get Right

- **Feature-organized folder structure** — All configurations create Pages/ subfolders by domain entity (Events, Attendees, Venues, etc.)
- **Interface-based DI registration** — `AddScoped<IService, Service>()` pattern used universally
- **Nested InputModel classes** — All prevent over-posting by binding to dedicated input models, never directly to entities
- **Data annotation validation** — `[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]` applied comprehensively
- **ModelState.IsValid checks** — All POST handlers validate model state before processing
- **PRG pattern** — Post-Redirect-Get with TempData messaging after successful form submissions
- **Bootstrap 5 integration** — Professional use of responsive grid, form controls, cards, badges, and alerts
- **Async/await conventions** — Consistent `Async` suffix, no sync-over-async anti-patterns
- **EF Core Fluent API** — Relationship configuration with explicit delete behaviors and unique indexes
- **Proper enum design** — Singular-named enums for all status fields
- **Nullable reference types** — `<Nullable>enable</Nullable>` in all `.csproj` files
- **Structured logging** — `ILogger<T>` injection with structured message templates
- **SQLite with EF Core** — Correct database configuration in `appsettings.json`
- **Standard Razor layout hierarchy** — `_Layout`, `_ViewStart`, `_ViewImports` files present
- **No Swashbuckle** — Correctly omits API documentation for server-rendered apps

---

## Summary: Impact of Skills

### Ranking by Weighted Score

1. **🥇 dotnet-webapi — 273.5 points**
   The strongest overall configuration. Its decisive advantages are **CancellationToken propagation** (the only config to implement it fully), **IExceptionHandler** middleware with ProblemDetails, **sealed types** (187), and the most robust **PaginatedList&lt;T&gt;** component. It also excels at modern C# adoption with primary constructors and collection expressions. Its main weakness is wildcard NuGet versions.

2. **🥈 dotnet-artisan — 269.5 points**
   The runner-up with the best **NuGet version pinning** (pinned `10.0.5` in 2/3 apps), excellent **AsNoTracking** usage, **sealed types** (136), **IValidatableObject** for cross-field validation, and strong accessibility (83 ARIA attributes). It also uses primary constructors and collection expressions extensively. Its weakness is missing CancellationToken propagation and HSTS/HTTPS.

3. **🥉 dotnet-skills — 252 points**
   A solid middle-ground configuration with the best **TempData** implementation (dedicated `_FlashMessages` partial), the most **ILogger&lt;T&gt;** instrumentation (42 instances), and the most consistent **_ValidationScriptsPartial** inclusion. It lacks modern C# features (no primary constructors) and has all wildcard versions.

4. **4th: managedcode-dotnet-skills — 246.5 points**
   Strong modern C# adoption (115 primary constructors, 95 collection expressions) and the best **accessibility** (102 ARIA attributes). However, it lacks CancellationToken, sealed types, HSTS, and uses wildcard versions throughout.

5. **5th: no-skills — 241 points**
   The baseline performs respectably — it generates complete, working applications with good structure and validation. However, it misses performance optimizations (no AsNoTracking), modern C# features (no primary constructors), and has the weakest accessibility (15 ARIA attributes). It does produce the most pages (101 total).

### Most Impactful Differences

1. **CancellationToken propagation** (dotnet-webapi only): The single biggest technical differentiator. Critical for production server resource management.
2. **Sealed types** (dotnet-webapi, dotnet-artisan): 187 and 136 sealed types respectively enable JIT optimizations and communicate design intent. Other configs have 0-6.
3. **AsNoTracking** (all skill configs vs no-skills): Every skill-enhanced configuration adds this critical performance optimization. The baseline completely misses it.
4. **NuGet version pinning** (dotnet-artisan only): The only configuration to pin specific versions, ensuring reproducible builds.
5. **Modern C# features** (dotnet-artisan, dotnet-webapi, managedcode): Primary constructors eliminate 3-5 lines of boilerplate per class. Collection expressions (`= []`) are cleaner than `= new List<T>()`.
6. **IExceptionHandler** (dotnet-webapi only): Modern structured exception handling with HTTP status code mapping.
7. **ARIA accessibility** (managedcode, dotnet-artisan): 5-7x more ARIA attributes than the baseline, significantly improving screen reader support.

### Overall Assessment

Skills demonstrably improve code quality. The **no-skills baseline** generates functional but architecturally basic code. Each skill configuration brings specific strengths:

- **dotnet-webapi** excels at **runtime correctness** (CancellationToken, exception handling, sealed types)
- **dotnet-artisan** excels at **build reproducibility** (pinned versions) and **code quality** (sealed types, AsNoTracking, validation)
- **managedcode** excels at **accessibility** and **modern syntax**
- **dotnet-skills** excels at **instrumentation** (logging, validation scripts, flash messages)

The ideal configuration would combine dotnet-webapi's runtime patterns with dotnet-artisan's version pinning and managedcode's accessibility — a composite that would score approximately 300+ points.
