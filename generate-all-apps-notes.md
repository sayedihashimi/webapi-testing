# Generation Summary — All Variants

**Date**: 2026-03-18
**SDK**: .NET 10.0.200
**Generator**: Copilot CLI (`copilot -p ... --yolo`) via `generate-apps.ps1`

## Results Overview

| Directory | App | Build | Run | Swashbuckle | OpenAPI Approach |
|-----------|-----|-------|-----|-------------|-----------------|
| src-no-skills | FitnessStudioApi | ✅ | ✅ | `Swashbuckle.AspNetCore` 7.* | `AddSwaggerGen` + `UseSwaggerUI` (old) |
| src-no-skills | LibraryApi | ✅ | ✅ | `Swashbuckle.AspNetCore` 10.1.5 | `AddSwaggerGen` + `UseSwaggerUI` (old) |
| src-no-skills | VetClinicApi | ✅ | ✅ | `Swashbuckle.AspNetCore` 7.*-* | `AddSwaggerGen` + `UseSwaggerUI` (old) |
| src-dotnet-webapi | FitnessStudioApi | ✅ | ✅ | ❌ None | Built-in `AddOpenApi` + `MapOpenApi` |
| src-dotnet-webapi | LibraryApi | ✅ | ✅ | ❌ None | Built-in `AddOpenApi` + `MapOpenApi` |
| src-dotnet-webapi | VetClinicApi | ✅ | ✅ | ❌ None | Built-in `AddOpenApi` + `MapOpenApi` |
| src-dotnet-artisan | FitnessStudioApi | ✅ | ✅ | `Swashbuckle.AspNetCore` 10.1.5 | Built-in `AddOpenApi` + `MapOpenApi` + `UseSwaggerUI` |
| src-dotnet-artisan | LibraryApi | ✅ | ✅ | ❌ None | Built-in `AddOpenApi` + `MapOpenApi` |
| src-dotnet-artisan | VetClinicApi | ✅ | ✅ | ❌ None | Built-in `AddOpenApi` + `MapOpenApi` |

**All 9 projects build and run successfully.** ✅
**All 9 apps are fully implemented** (39-46 .cs files each, with Controllers/Endpoints, Services, DTOs, Models, Middleware, Data).

## Skill Configuration Per Variant

| Directory | Skills Used | Notes |
|-----------|------------|-------|
| `src-no-skills` | None | Baseline — no skill guidance. `gen-notes.md` present ✅ |
| `src-dotnet-webapi` | `dotnet-webapi` (custom skill) | Single skill for Web API patterns. `gen-notes.md` **missing** ❌ |
| `src-dotnet-artisan` | `using-dotnet` → `dotnet-advisor` → `dotnet-csharp` + `dotnet-api` | Full skill chain. `gen-notes.md` **missing** ❌ |

## Swashbuckle Observations

- **`src-dotnet-webapi`: 0/3 apps have any Swashbuckle package** ✅ — The updated SKILL.md successfully prevented all Swashbuckle installation. All 3 use only the built-in `AddOpenApi()` + `MapOpenApi()`.
- **`src-no-skills`**: 3/3 apps use the old `AddSwaggerGen` + `UseSwaggerUI` pattern with the full `Swashbuckle.AspNetCore` package. Version inconsistency: two apps use 7.* wildcards, one uses 10.1.5.
- **`src-dotnet-artisan`**: 1/3 apps (FitnessStudioApi) added `Swashbuckle.AspNetCore` 10.1.5 for `UseSwaggerUI`, but still uses built-in `AddOpenApi` + `MapOpenApi` for document generation (not `AddSwaggerGen`). The other 2 have no Swashbuckle.
- No `AddSwaggerGen()` calls exist in any dotnet-webapi or dotnet-artisan app.

## API Style

| Directory | Pattern | Evidence |
|-----------|---------|----------|
| `src-no-skills` | Controllers | `Controllers/` directory, `[ApiController]` attributes |
| `src-dotnet-webapi` | Minimal APIs | `Endpoints/` directory, `MapGroup()` + extension methods |
| `src-dotnet-artisan` | Minimal APIs | `Endpoints/` directory, `MapGroup()` + extension methods |
