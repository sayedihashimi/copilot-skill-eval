# Build & Run Verification Report

**Evaluation:** ASP.NET Core Web API Skill Evaluation
**Date:** 2026-03-27 06:36 UTC
**Configurations:** 5
**Scenarios:** 3
**Total projects:** 15

## Results

| Configuration | Scenario | Build | Run | Format | Security | Notes |
|---|---|---|---|---|---|---|
| no-skills | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| no-skills | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| no-skills | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | VetClinicApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | LibraryApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | VetClinicApi | ❌ Fail | ⏭️ Skipped | ⏭️ Skipped | ⏭️ Skipped |   Determining projects to restore...   All projects are up-to-date for restore. C:\Program Files\dot |

## Automated Metrics

### Build Warnings by Category

| Configuration | Scenario | Total | Naming | Performance | Reliability | Security | Usage | Style | Other |
|---|---|---|---|---|---|---|---|---|---|
| managedcode-dotnet-skills | LibraryApi | 176 | 0 | 112 | 0 | 0 | 0 | 0 | 64 |
| managedcode-dotnet-skills | VetClinicApi | 96 | 2 | 54 | 0 | 0 | 0 | 0 | 40 |
| dotnet-skills | FitnessStudioApi | 94 | 0 | 94 | 0 | 0 | 0 | 0 | 0 |
| dotnet-skills | LibraryApi | 176 | 0 | 120 | 0 | 0 | 0 | 0 | 56 |
| dotnet-skills | VetClinicApi | 122 | 2 | 80 | 0 | 0 | 0 | 0 | 40 |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | None | None |
| dotnet-webapi | dotnet-webapi skill | skills/dotnet-webapi | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
| dotnet-skills | Official .NET Skills (dotnet/skills) | None | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |
