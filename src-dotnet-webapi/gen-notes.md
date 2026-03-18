# Code Generation Notes

## Skills Used

All three applications were generated using the **`dotnet-webapi`** skill. This skill provides specialized guidance for creating and modifying ASP.NET Core Web API endpoints with correct HTTP semantics, OpenAPI metadata, error handling, and data access wiring.

### Skill: `dotnet-webapi`

**Description**: Guides creation and modification of ASP.NET Core Web API endpoints with correct HTTP semantics, OpenAPI metadata, error handling, and data access wiring.

**Used for**: Adding new API endpoints (minimal APIs), wiring up OpenAPI/Swagger, creating `.http` test files, connecting endpoints to EF Core, adding pagination/filtering/sorting to list endpoints, and setting up global error handling middleware.

**Key conventions enforced by this skill across all three apps**:

| Convention | Detail |
|---|---|
| API Style | Minimal APIs (`app.MapGet`, `app.MapPost`, etc.) — no controllers |
| Request/Response Types | Dedicated `Create{Entity}Request`, `Update{Entity}Request`, `{Entity}Response` types — never expose EF Core entities directly |
| HTTP Status Codes | `201 Created` with Location header for POST creates, `204 No Content` for deletes, `200 OK` / `404 Not Found` for GETs |
| CancellationToken | Accepted in every endpoint and forwarded to all async calls |
| OpenAPI Metadata | `.WithSummary()`, `.WithDescription()`, `.Produces<>()` on every endpoint; Swagger UI enabled |
| Enum Serialization | `JsonStringEnumConverter` so enums appear as strings in JSON |
| Error Handling | `IExceptionHandler` with `AddProblemDetails()` returning RFC 7807 ProblemDetails |
| Service Layer | Service interfaces + implementations between endpoints and DbContext |
| Read-Only Queries | `AsNoTracking()` on all read-only EF Core queries |
| EF Core Migrations | Used `dotnet ef migrations add` — not `EnsureCreated()` |
| Pagination | `page`/`pageSize` query params returning `{ items, totalCount, totalPages, hasNextPage, hasPreviousPage }` |
| Validation | `System.ComponentModel.DataAnnotations` on DTOs |
| .http Files | Comprehensive test files covering all endpoints with realistic request data |

## Applications Generated

### 1. FitnessStudioApi (Zenith Fitness Studio)

- **Location**: `./FitnessStudioApi/`
- **Skill Used**: `dotnet-webapi`
- **Description**: Fitness and wellness studio booking API managing members, membership plans, memberships, instructors, class types, class schedules, bookings, and waitlists.
- **Build Status**: ✅ 0 errors, 0 warnings

### 2. LibraryApi (Sunrise Community Library)

- **Location**: `./LibraryApi/`
- **Skill Used**: `dotnet-webapi`
- **Description**: Community library management API managing books, authors, categories, patrons, loans, reservations, and fines.
- **Build Status**: ✅ 0 errors, 0 warnings

### 3. VetClinicApi (Happy Paws Veterinary Clinic)

- **Location**: `./VetClinicApi/`
- **Skill Used**: `dotnet-webapi`
- **Description**: Veterinary clinic management API managing pet owners, pets, veterinarians, appointments, medical records, prescriptions, and vaccinations.
- **Build Status**: ✅ 0 errors, 0 warnings

## Isolation

Each application was built by a separate, isolated agent with no shared knowledge or context between them. This ensures each project is fully independent and self-contained.
