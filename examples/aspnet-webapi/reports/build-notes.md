# Build & Run Verification Report

**Evaluation:** ASP.NET Core Web API Skill Evaluation
**Date:** 2026-03-29 16:25 UTC
**Configurations:** 5
**Scenarios:** 3
**Total projects:** 15

## Results

| Configuration | Run | Scenario | Build | Run | Format | Security | Notes |
|---|---|---|---|---|---|---|---|
| no-skills | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| no-skills | 1 | LibraryApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |
| no-skills | 1 | VetClinicApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |
| dotnet-webapi | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | 1 | LibraryApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |
| dotnet-webapi | 1 | VetClinicApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |
| dotnet-artisan | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | 1 | LibraryApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |
| dotnet-artisan | 1 | VetClinicApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |
| managedcode-dotnet-skills | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | 1 | LibraryApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |
| managedcode-dotnet-skills | 1 | VetClinicApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |
| dotnet-skills | 1 | FitnessStudioApi | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | 1 | LibraryApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |
| dotnet-skills | 1 | VetClinicApi | ⚠️ Not found | ⚠️ Not found | ⏭️ Skipped | ⏭️ Skipped | Project directory not found |

## Asset Usage Per Run

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | e8c69826…90de | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-webapi | 1 | 20a62f02…94d8 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-artisan | 1 | beb4965f…ee9c | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | 11281ead…affc | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-project-setup, dotnet-microsoft-extensions, dotnet-modern-csharp | — | ✅ |
| dotnet-skills | 1 | 787f18b5…7aa4 | claude-opus-4.6-1m | optimizing-ef-core-queries, analyzing-dotnet-performance | dotnet-data, dotnet-diag | ✅ |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | None | None |
| dotnet-webapi | dotnet-webapi skill | skills/dotnet-webapi | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
| dotnet-skills | Official .NET Skills (dotnet/skills) | None | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |
