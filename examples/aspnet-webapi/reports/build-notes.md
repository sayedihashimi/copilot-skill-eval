# Build & Run Verification Report

**Evaluation:** ASP.NET Core Web API Skill Evaluation
**Date:** 2026-03-30 05:12 UTC
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
| dotnet-artisan | 3 | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 2 vulns |  |
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
| no-skills | FitnessStudioApi | 126 | 0 | 102 | 0 | 0 | 0 | 0 | 24 |
| no-skills | LibraryApi | 148 | 0 | 88 | 0 | 0 | 0 | 0 | 60 |
| no-skills | VetClinicApi | 116 | 2 | 74 | 0 | 0 | 0 | 0 | 40 |
| dotnet-artisan | FitnessStudioApi | 90 | 6 | 60 | 0 | 0 | 0 | 0 | 24 |
| dotnet-artisan | LibraryApi | 138 | 0 | 74 | 0 | 0 | 0 | 0 | 64 |
| dotnet-artisan | VetClinicApi | 116 | 2 | 74 | 0 | 0 | 0 | 0 | 40 |
| managedcode-dotnet-skills | FitnessStudioApi | 128 | 0 | 104 | 0 | 0 | 0 | 0 | 24 |
| managedcode-dotnet-skills | LibraryApi | 158 | 0 | 98 | 0 | 0 | 0 | 0 | 60 |
| managedcode-dotnet-skills | VetClinicApi | 96 | 2 | 54 | 0 | 0 | 0 | 0 | 40 |
| dotnet-skills | FitnessStudioApi | 50 | 0 | 50 | 0 | 0 | 0 | 0 | 0 |
| dotnet-skills | LibraryApi | 132 | 0 | 80 | 0 | 0 | 0 | 0 | 52 |
| dotnet-skills | VetClinicApi | 100 | 2 | 58 | 0 | 0 | 0 | 0 | 40 |

## Asset Usage Per Run

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | 6a98279d…dfcd | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 2 | c5d6d655…f0d6 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 3 | abd996f5…6359 | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-artisan | 1 | 5ee8fd93…7950 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api | dotnet-artisan | ✅ |
| dotnet-artisan | 2 | e7ed9d47…2379 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| dotnet-artisan | 3 | 894060c3…30d6 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | 86dfb05b…7c29 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-project-setup, dotnet-modern-csharp, dotnet-entity-framework-core, dotnet-minimal-apis, dotnet-microsoft-extensions | — | ✅ |
| managedcode-dotnet-skills | 2 | e140e302…53d4 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-modern-csharp, dotnet-project-setup | — | ✅ |
| managedcode-dotnet-skills | 3 | f163762c…d1ca | claude-opus-4.6-1m | dotnet-aspnet-core | — | ✅ |
| dotnet-skills | 1 | 00d36ed0…aadd | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 2 | ee48f009…90dd | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 3 | 52aa5393…f78f | claude-opus-4.6-1m | optimizing-ef-core-queries, analyzing-dotnet-performance | dotnet-data, dotnet-diag | ✅ |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | None | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
| dotnet-skills | Official .NET Skills (dotnet/skills) | None | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |
