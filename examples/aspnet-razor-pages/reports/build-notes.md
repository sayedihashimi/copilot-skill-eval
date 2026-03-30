# Build & Run Verification Report

**Evaluation:** ASP.NET Core Razor Pages Skill Evaluation
**Date:** 2026-03-30 15:41 UTC
**Configurations:** 4
**Scenarios:** 3
**Total projects:** 4

## Results

| Configuration | Run | Scenario | Build | Run | Format | Security | Notes |
|---|---|---|---|---|---|---|---|
| no-skills | 1 | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | 1 | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 2 vulns |  |
| managedcode-dotnet-skills | 1 | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | 1 | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |

## Automated Metrics

### Build Warnings by Category

| Configuration | Scenario | Total | Naming | Performance | Reliability | Security | Usage | Style | Other |
|---|---|---|---|---|---|---|---|---|---|
| no-skills | SparkEvents | 108 | 10 | 80 | 0 | 0 | 0 | 0 | 18 |
| dotnet-artisan | SparkEvents | 40 | 2 | 36 | 0 | 0 | 0 | 0 | 2 |
| managedcode-dotnet-skills | SparkEvents | 62 | 2 | 60 | 0 | 0 | 0 | 0 | 0 |
| dotnet-skills | SparkEvents | 42 | 2 | 40 | 0 | 0 | 0 | 0 | 0 |

## Asset Usage Per Run

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | 90e289fa…b1e0 | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-artisan | 1 | ae756689…e036 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-tooling, using-dotnet, using-dotnet, using-dotnet, using-dotnet, using-dotnet, dotnet-advisor, using-dotnet, dotnet-advisor, dotnet-advisor, dotnet-advisor, dotnet-advisor, dotnet-api, dotnet-csharp, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-csharp, dotnet-api, dotnet-api, dotnet-csharp, dotnet-csharp, dotnet-csharp | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | fb19e67d…4511 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-modern-csharp, dotnet-project-setup, dotnet-entity-framework-core | — | ✅ |
| dotnet-skills | 1 | 5743e20e…e25f | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | None | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
| dotnet-skills | Official .NET Skills (dotnet/skills) | None | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |
