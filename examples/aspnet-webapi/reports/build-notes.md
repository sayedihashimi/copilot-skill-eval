# Build & Run Verification Report

**Evaluation:** ASP.NET Core Web API Skill Evaluation
**Date:** 2026-03-31 09:11 UTC
**Configurations:** 5
**Scenarios:** 3
**Total projects:** 15

## Results

| Configuration | Run | Scenario | Build | Run | Format | Security | Notes |
|---|---|---|---|---|---|---|---|
| no-skills | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| no-skills | 2 | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| no-skills | 3 | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | 2 | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 2 vulns |  |
| dotnet-webapi | 3 | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | 2 | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | 3 | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | 2 | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | 3 | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | 2 | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | 3 | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |

## Automated Metrics

### Build Warnings by Category

| Configuration | Scenario | Total | Naming | Performance | Reliability | Security | Usage | Style | Other |
|---|---|---|---|---|---|---|---|---|---|
| no-skills | FitnessStudioApi | 106 | 0 | 82 | 0 | 0 | 0 | 0 | 24 |
| no-skills | LibraryApi | 140 | 0 | 72 | 0 | 0 | 0 | 0 | 68 |
| no-skills | VetClinicApi | 102 | 2 | 58 | 0 | 0 | 0 | 0 | 42 |
| dotnet-webapi | FitnessStudioApi | 122 | 0 | 98 | 0 | 0 | 0 | 0 | 24 |
| dotnet-webapi | LibraryApi | 178 | 0 | 114 | 0 | 0 | 0 | 0 | 64 |
| dotnet-webapi | VetClinicApi | 96 | 2 | 54 | 0 | 0 | 0 | 0 | 40 |
| dotnet-artisan | FitnessStudioApi | 132 | 6 | 102 | 0 | 0 | 0 | 0 | 24 |
| dotnet-artisan | LibraryApi | 82 | 0 | 82 | 0 | 0 | 0 | 0 | 0 |
| dotnet-artisan | VetClinicApi | 64 | 2 | 62 | 0 | 0 | 0 | 0 | 0 |
| managedcode-dotnet-skills | FitnessStudioApi | 128 | 0 | 104 | 0 | 0 | 0 | 0 | 24 |
| managedcode-dotnet-skills | LibraryApi | 156 | 0 | 96 | 0 | 0 | 0 | 0 | 60 |
| managedcode-dotnet-skills | VetClinicApi | 118 | 2 | 76 | 0 | 0 | 0 | 0 | 40 |
| dotnet-skills | FitnessStudioApi | 68 | 0 | 68 | 0 | 0 | 0 | 0 | 0 |
| dotnet-skills | LibraryApi | 140 | 0 | 88 | 0 | 0 | 0 | 0 | 52 |
| dotnet-skills | VetClinicApi | 118 | 2 | 76 | 0 | 0 | 0 | 0 | 40 |

## Asset Usage Per Run

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | 3d221ee7…b060 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 2 | 8d159e82…5d92 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 3 | a6d566ce…f64f | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-webapi | 1 | 88ec51d6…8163 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-webapi | 2 | 4ea81de9…c8db | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-webapi | 3 | c5890f1b…66b1 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-artisan | 1 | 8a16012c…24ab | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api | dotnet-artisan | ✅ |
| dotnet-artisan | 2 | d3493166…4f8a | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-tooling | dotnet-artisan | ✅ |
| dotnet-artisan | 3 | 7898f8bb…ab89 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | 406ec9a8…34ed | claude-opus-4.6-1m | dotnet, dotnet-modern-csharp, dotnet-project-setup, dotnet-entity-framework-core, dotnet-microsoft-extensions, dotnet-aspnet-core, dotnet-minimal-apis | — | ✅ |
| managedcode-dotnet-skills | 2 | fe7444c2…1ec8 | None | — | — | ✅ |
| managedcode-dotnet-skills | 3 | 75f70326…8f8d | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-modern-csharp, dotnet-entity-framework-core, dotnet-project-setup | — | ✅ |
| dotnet-skills | 1 | c6aebf53…20c6 | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 2 | f896f3c2…291c | claude-opus-4.6-1m | optimizing-ef-core-queries, analyzing-dotnet-performance | dotnet-data, dotnet-diag | ✅ |
| dotnet-skills | 3 | ba79ee8f…57ff | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | None | None |
| dotnet-webapi | dotnet-webapi skill | skills/dotnet-webapi | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
| dotnet-skills | Official .NET Skills (dotnet/skills) | None | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |
