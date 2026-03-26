# HorizonHR — Generation Notes

## Skills Used

### `dotnet-webapi` Skill

The **dotnet-webapi** skill was invoked during project generation. While the specification called for a Razor Pages application (not a Web API), many of the skill's architectural guidelines are framework-agnostic ASP.NET Core best practices that directly influenced the generated code:

#### How the Skill Influenced the Code

1. **Seal all types by default** — All model classes, service implementations, page models, middleware, and input model classes are marked `sealed` to prevent unintended inheritance, communicate design intent, and enable JIT devirtualization (per CA1852). Examples:
   - `public sealed class Department { ... }`
   - `public sealed class EmployeeService(...) : IEmployeeService`
   - `public sealed class IndexModel(...) : PageModel`
   - `internal sealed class GlobalExceptionHandler(...) : IExceptionHandler`

2. **Service layer with interfaces** — The skill mandates that endpoints (or in this case, Razor Page models) should not inject `DbContext` directly. Instead, every service has a corresponding interface registered in DI:
   - `builder.Services.AddScoped<IDepartmentService, DepartmentService>();`
   - `builder.Services.AddScoped<IEmployeeService, EmployeeService>();`
   - etc.

3. **Dedicated input model classes** — Following the skill's guidance to never bind directly to entity classes, all forms use nested `InputModel` classes with `[BindProperty]` for form binding. Examples:
   - `CreateModel.InputModel` for department creation
   - `EditModel.InputModel` for employee editing

4. **EF Core Fluent API configuration** — Per the skill's guidance:
   - Unique indexes on natural keys: `HasIndex(d => d.Name).IsUnique()`
   - Explicit cascade/restrict delete behaviors on all foreign keys
   - Enum stored as strings: `.HasConversion<string>()`
   - Decimal column types specified: `.HasColumnType("decimal(10,2)")`

5. **`AsNoTracking()` on read-only queries** — All service methods that only read data use `AsNoTracking()` to avoid unnecessary change tracking overhead.

6. **Error handling middleware** — The skill's `IExceptionHandler` pattern was adapted. A `GlobalExceptionHandler` class is placed in the `Middleware/` folder (as the skill requires) and registered via `builder.Services.AddExceptionHandler<>()`.

7. **Pagination pattern** — The skill's paginated response pattern influenced the `PaginatedList<T>` helper class used across all list pages, with consistent `pageNumber`/`pageSize` query parameters.

8. **CancellationToken propagation** — Every service method and page handler accepts and forwards `CancellationToken` through the entire async call chain, following the skill's guidance.

9. **`IReadOnlyList<T>` for collections** — The `PaginatedList<T>.Items` property and `DashboardStats.HeadcountByDepartment` use `IReadOnlyList<T>` to signal immutability, per the skill's preference.

## Architecture Summary

- **Framework**: ASP.NET Core Razor Pages on .NET 10
- **Database**: EF Core with SQLite (`HorizonHR.db`)
- **Styling**: Bootstrap 5 CDN
- **Project Structure**: Models → Data (DbContext, Seeder) → Services (interface + sealed impl) → Pages → Middleware
- **Entities**: Department, Employee, LeaveType, LeaveBalance, LeaveRequest, PerformanceReview, Skill, EmployeeSkill
- **Seed Data**: 5 departments, 4 leave types, 13 employees, leave balances, 8 leave requests, 6 reviews, 12 skills, 23 employee-skill records
- **Reusable Components**: `_StatusBadge` partial view for color-coded status badges, `_PaginationPartial` for pagination controls
