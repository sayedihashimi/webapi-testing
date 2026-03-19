# Generation Summary — All Variants

**Date**: 2026-03-19
**SDK**: .NET 10.0.200
**Generator**: Copilot CLI (`copilot -p ... --yolo`) via `generate-apps.ps1`
**Model**: claude-opus-4.6-1m

## Results Overview

| Directory | App | Build | Run | Swashbuckle | OpenAPI Approach |
|-----------|-----|-------|-----|-------------|-----------------|
| src-no-skills | FitnessStudioApi | ✅ | ✅ | ❌ None | `AddSwaggerGen` (no Swashbuckle pkg) |
| src-no-skills | LibraryApi | ✅ | ✅ | ❌ None | `AddOpenApi` + `MapOpenApi` |
| src-no-skills | VetClinicApi | ✅ | ✅ | ❌ None | `AddOpenApi` + `MapOpenApi` |
| src-dotnet-webapi | FitnessStudioApi | ✅ | ✅ | ❌ None | `AddOpenApi` + `MapOpenApi` |
| src-dotnet-webapi | LibraryApi | ✅ | ✅ | ❌ None | `AddOpenApi` + `MapOpenApi` |
| src-dotnet-webapi | VetClinicApi | ✅ | ✅ | ❌ None | `AddOpenApi` + `MapOpenApi` |
| src-dotnet-artisan | FitnessStudioApi | ✅ | ✅ | `Swashbuckle.AspNetCore` | `AddOpenApi` + `MapOpenApi` |
| src-dotnet-artisan | LibraryApi | ✅ | ✅ | `Swashbuckle.AspNetCore` | `AddOpenApi` + `MapOpenApi` |
| src-dotnet-artisan | VetClinicApi | ✅ | ✅ | ❌ None | `AddOpenApi` + `MapOpenApi` |

**All 9 projects build and run successfully.** ✅

## Skill Configuration Per Variant

| Directory | Skills Used | Mechanism | `gen-notes.md` |
|-----------|------------|-----------|-----------------|
| `src-no-skills` | None | Baseline — no skill guidance | ✅ Present |
| `src-dotnet-webapi` | `dotnet-webapi` (custom skill) | Temporarily registered in config.json via `Add-SkillDirectory` | ❌ Missing |
| `src-dotnet-artisan` | `using-dotnet` → `dotnet-advisor` → `dotnet-csharp` + `dotnet-api` | `--plugin-dir contrib\plugins\dotnet-artisan` | ❌ Missing |

## API Style

| Directory | Pattern | Evidence |
|-----------|---------|----------|
| `src-no-skills` | Controllers | `Controllers/` directory, `[ApiController]` attributes |
| `src-dotnet-webapi` | Minimal APIs | `Endpoints/` directory, `MapGroup()` + extension methods |
| `src-dotnet-artisan` | Minimal APIs | `Endpoints/` directory, `MapGroup()` + extension methods |

## Swashbuckle / OpenAPI Observations

- **`src-dotnet-webapi`**: 0/3 apps have Swashbuckle — all use built-in `AddOpenApi()` + `MapOpenApi()`.
- **`src-no-skills`**: FitnessStudioApi uses `AddSwaggerGen` (without the Swashbuckle package in .csproj). Other 2 use `AddOpenApi` + `MapOpenApi`.
- **`src-dotnet-artisan`**: 2/3 apps (FitnessStudioApi, LibraryApi) reference Swashbuckle in .csproj, but all use `AddOpenApi` + `MapOpenApi` for document generation.

## Script Configuration

This run used `generate-apps.ps1` with self-contained plugin/skill loading:
- **`--plugin-dir`** flag for `dotnet-artisan` (loads plugin from `contrib/plugins/dotnet-artisan`)
- **Temporary config.json edit** for `dotnet-webapi` (registers skill from `contrib/skills/dotnet-webapi`, restores after run)
- No global Copilot plugin/skill installation required
