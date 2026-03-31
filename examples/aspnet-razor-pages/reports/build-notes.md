# Build & Run Verification Report

**Evaluation:** ASP.NET Core Razor Pages Skill Evaluation
**Date:** 2026-03-31 12:12 UTC
**Configurations:** 5
**Scenarios:** 3
**Total projects:** 5

## Results

| Configuration | Run | Scenario | Build | Run | Format | Security | Notes |
|---|---|---|---|---|---|---|---|
| no-skills | 1 | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | 1 | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | 1 | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | 1 | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | 1 | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |

## Automated Metrics

### Build Warnings by Category

| Configuration | Scenario | Total | Naming | Performance | Reliability | Security | Usage | Style | Other |
|---|---|---|---|---|---|---|---|---|---|
| no-skills | SparkEvents | 98 | 4 | 54 | 0 | 0 | 0 | 0 | 40 |
| dotnet-webapi | SparkEvents | 40 | 4 | 34 | 0 | 0 | 0 | 0 | 2 |
| dotnet-artisan | SparkEvents | 38 | 4 | 32 | 0 | 0 | 0 | 0 | 2 |
| managedcode-dotnet-skills | SparkEvents | 66 | 4 | 48 | 0 | 0 | 0 | 0 | 14 |
| dotnet-skills | SparkEvents | 46 | 2 | 30 | 0 | 0 | 0 | 0 | 14 |

## Asset Usage Per Run

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | a89cd48f…169c | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-webapi | 1 | 9a779929…f5c6 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-artisan | 1 | 1f049787…31d1 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp, dotnet-tooling, using-dotnet, using-dotnet, dotnet-advisor, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | ca8efda4…6094 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-project-setup, dotnet-modern-csharp, dotnet-entity-framework-core | — | ✅ |
| dotnet-skills | 1 | a8ff7968…a1ee | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | None | None |
| dotnet-webapi | dotnet-webapi skill | skills/dotnet-webapi | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
| dotnet-skills | Official .NET Skills (dotnet/skills) | None | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |
