# Build & Run Verification Report

**Evaluation:** ASP.NET Core Razor Pages Skill Evaluation
**Date:** 2026-03-26 10:39 UTC
**Configurations:** 5
**Scenarios:** 3
**Total projects:** 15

## Results

| Configuration | Scenario | Build | Run | Notes |
|---|---|---|---|---|
| no-skills | SparkEvents | ✅ Pass | ✅ Pass |  |
| no-skills | KeystoneProperties | ✅ Pass | ✅ Pass |  |
| no-skills | HorizonHR | ✅ Pass | ✅ Pass |  |
| dotnet-webapi | SparkEvents | ✅ Pass | ✅ Pass |  |
| dotnet-webapi | KeystoneProperties | ✅ Pass | ✅ Pass |  |
| dotnet-webapi | HorizonHR | ✅ Pass | ✅ Pass |  |
| dotnet-artisan | SparkEvents | ✅ Pass | ✅ Pass |  |
| dotnet-artisan | KeystoneProperties | ✅ Pass | ✅ Pass |  |
| dotnet-artisan | HorizonHR | ❌ Fail | ⏭️ Skipped |   Determining projects to restore...   Restored C:\data\mycode\webapi-testing\examples\aspnet-razor- |
| managedcode-dotnet-skills | SparkEvents | ✅ Pass | ✅ Pass |  |
| managedcode-dotnet-skills | KeystoneProperties | ✅ Pass | ✅ Pass |  |
| managedcode-dotnet-skills | HorizonHR | ✅ Pass | ✅ Pass |  |
| dotnet-skills | SparkEvents | ✅ Pass | ✅ Pass |  |
| dotnet-skills | KeystoneProperties | ✅ Pass | ✅ Pass |  |
| dotnet-skills | HorizonHR | ✅ Pass | ✅ Pass |  |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (no skills) | None | None |
| dotnet-webapi | dotnet-webapi skill | skills/dotnet-webapi | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
| dotnet-skills | Official .NET Skills (dotnet/skills) | None | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |
