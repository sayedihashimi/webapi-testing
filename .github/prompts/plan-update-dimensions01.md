# Plan: Revise Evaluation Dimensions for .NET Skills Evaluator

## Problem Statement

The current eval.yaml files have too many dimensions (44 for WebAPI, 39+ for Razor Pages) with significant overlap, a library-specific dimension (FluentValidation), and missing dimensions critical for .NET team quality evaluation. The analysis prompt becomes too large, degrading LLM-as-judge scoring reliability.

**Target audience:** .NET team and related teams evaluating Copilot custom skills that generate C# code. The generated code must build, run, and serve as exemplary .NET code following current best practices.

## Key Decisions

| Decision | Rationale |
|----------|-----------|
| Consolidate to ~20–25 dimensions per example | LLM scoring reliability degrades with 40+ dimensions; focused evaluation is more actionable |
| Remove FluentValidation dimension | Evaluate validation quality regardless of library choice |
| Add "Business Logic Correctness" | Generated code must actually implement the specified business rules |
| Add "Prefer Built-in over 3rd Party" | .NET team cannot ship examples using Swashbuckle, Newtonsoft.Json, etc. when built-in alternatives exist |
| Keep NuGet Version Pinning as critical | AI code gen commonly produces floating versions — a very bad practice |
| Keep Minimal API as critical | .NET team standard for new API development |
| Modern C# Adoption at high tier | Emitting modern syntax is important for .NET team examples |
| Exclude test generation for now | Scenario prompts explicitly exclude tests |

---

## Current → Proposed Dimension Mapping

### WebAPI: 44 → 24 dimensions

#### Consolidation Map

| Old Dimensions | New Dimension | Action |
|---------------|---------------|--------|
| API Architecture + TypedResults Usage | **Minimal API Architecture** | Merged — both evaluate Minimal API patterns |
| Guard Clauses + Input Validation Coverage + FluentValidation Usage | **Input Validation & Guard Clauses** | Merged — one dimension for all validation |
| Middleware Style + Exception Handling Strategy | **Error Handling & Middleware** | Merged — both evaluate error handling pipeline |
| Async/Await Best Practices + CancellationToken Propagation | **Async Patterns & Cancellation** | Merged — both evaluate async correctness |
| EF Core Relationship Configuration + AsNoTracking Usage | **EF Core Best Practices** | Merged — both evaluate EF Core usage |
| HSTS & HTTPS Redirection | **Security Configuration** | Expanded — add CORS, auth-related headers |
| Built-in OpenAPI over Swashbuckle | Absorbed into **Prefer Built-in over 3rd Party** | Merged with new broader dimension |
| OpenAPI Metadata | **API Documentation** | Renamed for clarity |
| Package Discipline | Absorbed into **NuGet Version Pinning** → rename to **NuGet & Package Discipline** | Merged — both about package management |
| Enum Design + Dispose & Resource Management | **Type Design & Resource Management** | Merged — both about type-level best practices |
| FluentValidation Usage | **REMOVED** | Library-specific; covered by Input Validation |
| — | **Business Logic Correctness** | NEW |
| — | **Prefer Built-in over 3rd Party** | NEW |

#### Proposed WebAPI Dimensions (24)

**CRITICAL (3×) — 6 dimensions:**

| # | Dimension | What It Covers | Eval Method |
|---|-----------|---------------|-------------|
| 1 | **Build & Run Success** | Compilation and app startup verification | automated |
| 2 | **Security Vulnerability Scan** | Known CVEs, deprecated packages | automated |
| 3 | **Minimal API Architecture** | Minimal APIs with route groups, endpoint extension methods, TypedResults | llm |
| 4 | **Input Validation & Guard Clauses** | All user inputs validated, guard clauses on public APIs, proper error responses for invalid input | llm |
| 5 | **NuGet & Package Discipline** | Exact version pinning, minimal purposeful packages, no unnecessary dependencies | hybrid |
| 6 | **EF Migration Usage** | EF Migrations vs EnsureCreated anti-pattern | llm |

**HIGH (2×) — 8 dimensions:**

| # | Dimension | What It Covers | Eval Method |
|---|-----------|---------------|-------------|
| 7 | **Business Logic Correctness** | Business rules from scenario prompts are actually implemented (e.g., waitlist promotion, fine calculation, booking limits) | llm |
| 8 | **Prefer Built-in over 3rd Party** | System.Text.Json over Newtonsoft, built-in OpenAPI over Swashbuckle, built-in DI over Autofac, built-in logging over Serilog/NLog | llm |
| 9 | **Modern C# Adoption** | Primary constructors, collection expressions, global usings, file-scoped namespaces | llm |
| 10 | **Error Handling & Middleware** | IExceptionHandler, ProblemDetails (RFC 7807), custom exception types, no swallowed exceptions | llm |
| 11 | **Async Patterns & Cancellation** | Async/await naming, no async void, no sync-over-async, CancellationToken through all layers | llm |
| 12 | **EF Core Best Practices** | Fluent API relationships, AsNoTracking for reads, proper cascade behavior, DbContext lifetime | llm |
| 13 | **Service Abstraction & DI** | Interface-based services, proper DI registration, single responsibility | llm |
| 14 | **Security Configuration** | HSTS, HTTPS redirection, security headers, proper CORS policy | llm |

**MEDIUM (1×) — 9 dimensions:**

| # | Dimension | What It Covers | Eval Method |
|---|-----------|---------------|-------------|
| 15 | **DTO Design** | Records vs classes, immutability, separation from entities, naming conventions | llm |
| 16 | **Sealed Types** | Classes and records declared sealed for JIT optimization and design intent | llm |
| 17 | **Data Seeder Design** | HasData vs injectable services, realistic seed data, state variety | llm |
| 18 | **Structured Logging** | ILogger<T>, structured message templates (not string interpolation), log levels | llm |
| 19 | **Nullable Reference Types** | Project-level NRT enforcement, no suppression operators, proper null handling | llm |
| 20 | **API Documentation** | OpenAPI endpoint descriptions, response types, parameter documentation | llm |
| 21 | **File Organization** | Feature-based or per-entity folder layout, clean project structure | llm |
| 22 | **HTTP Test File Quality** | .http file completeness, correct seed data IDs, all endpoints covered | llm |
| 23 | **Type Design & Resource Management** | Proper enum usage, IDisposable/IAsyncDisposable patterns, return type precision | llm |

**LOW (0.5×) — 1 dimension:**

| # | Dimension | What It Covers | Eval Method |
|---|-----------|---------------|-------------|
| 24 | **Code Standards Compliance** | Naming conventions, explicit access modifiers, file-scoped namespaces, formatting | hybrid |

---

### Razor Pages: 39+ → 23 dimensions

#### Consolidation Map

| Old Dimensions | New Dimension | Action |
|---------------|---------------|--------|
| Form Handling & Validation + Input Model Separation + Named Handler Methods | **Form Handling & Validation** | Merged — all about form patterns |
| Semantic HTML + Accessibility & ARIA + Bootstrap Integration + CSS Organization | **UI Quality & Accessibility** | Merged — all about frontend quality |
| View Components + Custom Tag Helpers + Layout & Partial Views | **Reusable UI Components** | Merged — all about component patterns |
| Async/Await Best Practices + CancellationToken Propagation | **Async Patterns & Cancellation** | Merged |
| EF Core Relationship Configuration + AsNoTracking Usage | **EF Core Best Practices** | Merged |
| HSTS & HTTPS Redirection | **Security Configuration** | Expanded |
| Guard Clauses + Input Validation Coverage + FluentValidation Usage | **Input Validation & Guard Clauses** | Merged |
| Enum Design + Dispose & Resource Management | **Type Design & Resource Management** | Merged |
| Built-in OpenAPI over Swashbuckle | Absorbed into **Prefer Built-in over 3rd Party** | Merged |
| FluentValidation Usage | **REMOVED** | Library-specific |
| — | **Business Logic Correctness** | NEW |
| — | **Prefer Built-in over 3rd Party** | NEW |

#### Proposed Razor Pages Dimensions (23)

**CRITICAL (3×) — 5 dimensions:**

| # | Dimension | What It Covers | Eval Method |
|---|-----------|---------------|-------------|
| 1 | **Build & Run Success** | Compilation and app startup | automated |
| 2 | **Security Vulnerability Scan** | Known CVEs, deprecated packages | automated |
| 3 | **Input Validation & Guard Clauses** | Form validation, model binding validation, guard clauses | llm |
| 4 | **NuGet & Package Discipline** | Exact version pinning, minimal packages | hybrid |
| 5 | **EF Migration Usage** | Migrations vs EnsureCreated | llm |

**HIGH (2×) — 9 dimensions:**

| # | Dimension | What It Covers | Eval Method |
|---|-----------|---------------|-------------|
| 6 | **Business Logic Correctness** | Business rules from scenario prompts actually implemented | llm |
| 7 | **Prefer Built-in over 3rd Party** | System.Text.Json, built-in OpenAPI, built-in DI, built-in logging | llm |
| 8 | **Modern C# Adoption** | Primary constructors, collection expressions, global usings | llm |
| 9 | **Page Model Design** | Thin page models, proper service injection, BindProperty usage | llm |
| 10 | **Form Handling & Validation** | Anti-forgery, PRG pattern, named handlers, input model separation | llm |
| 11 | **Error Handling Strategy** | Custom exceptions, IExceptionHandler, ProblemDetails, error pages | llm |
| 12 | **Async Patterns & Cancellation** | Async naming, CancellationToken propagation, no sync-over-async | llm |
| 13 | **EF Core Best Practices** | Fluent API, AsNoTracking, cascade behavior | llm |
| 14 | **Security Configuration** | HSTS, HTTPS, anti-forgery tokens, security headers | llm |

**MEDIUM (1×) — 8 dimensions:**

| # | Dimension | What It Covers | Eval Method |
|---|-----------|---------------|-------------|
| 15 | **Service Abstraction & DI** | Interface-based services, DI registration | llm |
| 16 | **UI Quality & Accessibility** | Semantic HTML, ARIA attributes, Bootstrap grid, responsive design, keyboard nav | llm |
| 17 | **Reusable UI Components** | ViewComponents, tag helpers, partial views, _Layout hierarchy | llm |
| 18 | **Data Seeder Design** | Seeding strategy, realistic data, state variety | llm |
| 19 | **Structured Logging** | ILogger<T>, structured templates, log levels | llm |
| 20 | **Nullable Reference Types** | NRT enforcement, no suppression operators | llm |
| 21 | **File Organization** | Feature-based layout, proper Areas/Pages structure | llm |
| 22 | **Type Design & Resource Management** | Enums, IDisposable patterns, TempData usage, sealed types | llm |

**LOW (0.5×) — 1 dimension:**

| # | Dimension | What It Covers | Eval Method |
|---|-----------|---------------|-------------|
| 23 | **Code Standards Compliance** | Naming, access modifiers, file-scoped namespaces | hybrid |

---

## Removed Dimensions (with justification)

| Removed Dimension | Reason |
|-------------------|--------|
| FluentValidation Usage | Library-specific; validation quality now evaluated in "Input Validation & Guard Clauses" |
| Pagination | Too granular; implementation varies by app requirements |
| Return Type Precision | Absorbed into "Type Design & Resource Management" |
| TempData & Flash Messages (Razor) | Absorbed into "Type Design & Resource Management" |
| Sealed Types (Razor) | Absorbed into "Type Design & Resource Management" for Razor; kept standalone for WebAPI |

---

## Implementation Steps

### Step 1: Create this plan document
- File: `.github/prompts/plan-update-dimensions01.md`

### Step 2: Update WebAPI eval.yaml
- Rewrite the `dimensions:` section of `examples/aspnet-webapi/eval.yaml`
- Keep all other sections (name, scenarios, configurations, verification) unchanged
- Write complete `what_to_look_for` and `why_it_matters` for each new/merged dimension

### Step 3: Update Razor Pages eval.yaml
- Rewrite the `dimensions:` section of `examples/aspnet-razor-pages/eval.yaml`
- Keep all other sections unchanged
- Write complete `what_to_look_for` and `why_it_matters` for each new/merged dimension

### Step 4: Validate
- Run `skill-eval validate-config` on both examples to ensure YAML is valid

### Step 5: Commit
- Commit all changes with descriptive message
