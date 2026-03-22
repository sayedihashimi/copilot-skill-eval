# Build & Run Verification Report

**Evaluation:** ASP.NET Core Razor Pages Skill Evaluation (Round 2 — with Razor-specific dimensions)
**Date:** 2026-03-22
**Configurations:** 4
**Scenarios:** 3
**Total projects:** 12
**Dimensions:** 24 (8 new Razor-specific + 16 sharpened)

## Results

| Configuration | Scenario | Build | Run | Notes |
|---|---|---|---|---|
| no-skills | SparkEvents | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| no-skills | KeystoneProperties | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| no-skills | HorizonHR | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| dotnet-webapi | SparkEvents | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| dotnet-webapi | KeystoneProperties | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| dotnet-webapi | HorizonHR | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| dotnet-artisan | SparkEvents | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| dotnet-artisan | KeystoneProperties | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| dotnet-artisan | HorizonHR | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| managedcode-dotnet-skills | SparkEvents | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| managedcode-dotnet-skills | KeystoneProperties | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |
| managedcode-dotnet-skills | HorizonHR | ✅ Pass | ✅ Pass | 0 errors, 0 warnings |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (no skills) | None | None |
| dotnet-webapi | dotnet-webapi skill | skills/dotnet-webapi | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
