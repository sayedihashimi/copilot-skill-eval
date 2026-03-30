# Build & Run Verification Report

**Evaluation:** ASP.NET Core Web API Skill Evaluation
**Date:** 2026-03-30 08:34 UTC
**Configurations:** 4
**Scenarios:** 3
**Total projects:** 12

## Results

| Configuration | Run | Scenario | Build | Run | Format | Security | Notes |
|---|---|---|---|---|---|---|---|
| no-skills | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| no-skills | 2 | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| no-skills | 3 | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
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
| no-skills | FitnessStudioApi | 94 | 6 | 64 | 0 | 0 | 0 | 0 | 24 |
| no-skills | LibraryApi | 150 | 0 | 90 | 0 | 0 | 0 | 0 | 60 |
| no-skills | VetClinicApi | 100 | 2 | 58 | 0 | 0 | 0 | 0 | 40 |
| dotnet-artisan | FitnessStudioApi | 92 | 0 | 76 | 0 | 0 | 0 | 0 | 16 |
| dotnet-artisan | LibraryApi | 78 | 0 | 78 | 0 | 0 | 0 | 0 | 0 |
| dotnet-artisan | VetClinicApi | 96 | 2 | 54 | 0 | 0 | 0 | 0 | 40 |
| managedcode-dotnet-skills | FitnessStudioApi | 126 | 0 | 102 | 0 | 0 | 0 | 0 | 24 |
| managedcode-dotnet-skills | LibraryApi | 146 | 0 | 90 | 0 | 0 | 0 | 0 | 56 |
| managedcode-dotnet-skills | VetClinicApi | 116 | 2 | 74 | 0 | 0 | 0 | 0 | 40 |
| dotnet-skills | FitnessStudioApi | 112 | 6 | 82 | 0 | 0 | 0 | 0 | 24 |
| dotnet-skills | LibraryApi | 126 | 0 | 70 | 0 | 0 | 0 | 0 | 56 |
| dotnet-skills | VetClinicApi | 116 | 2 | 74 | 0 | 0 | 0 | 0 | 40 |

## Asset Usage Per Run

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | 0379df85…2d34 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 2 | 69d851ec…050d | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 3 | 2de28baf…57aa | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-artisan | 1 | 4fb2147b…5391 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| dotnet-artisan | 2 | 71bade27…0a8f | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-tooling | dotnet-artisan | ✅ |
| dotnet-artisan | 3 | 19f2d38a…036c | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-tooling | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | 4e90ce72…ca1f | claude-opus-4.6-1m | dotnet, dotnet-project-setup, dotnet-entity-framework-core, dotnet-minimal-apis, dotnet-aspnet-core, dotnet-microsoft-extensions, dotnet-modern-csharp | — | ✅ |
| managedcode-dotnet-skills | 2 | 0e8a5b3f…78c0 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-project-setup, dotnet-microsoft-extensions, dotnet-entity-framework-core, dotnet-modern-csharp | — | ✅ |
| managedcode-dotnet-skills | 3 | 04ba91ac…bb25 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-project-setup, dotnet-modern-csharp, dotnet-minimal-apis, dotnet-microsoft-extensions | — | ✅ |
| dotnet-skills | 1 | 5756516f…f454 | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 2 | aeafeccc…f55c | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 3 | 726c8812…7ed7 | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | None | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
| dotnet-skills | Official .NET Skills (dotnet/skills) | None | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |
