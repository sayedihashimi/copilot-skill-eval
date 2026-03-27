# Build & Run Verification Report

**Evaluation:** ASP.NET Core Razor Pages Skill Evaluation
**Date:** 2026-03-27 06:56 UTC
**Configurations:** 5
**Scenarios:** 3
**Total projects:** 15

## Results

| Configuration | Scenario | Build | Run | Format | Security | Notes |
|---|---|---|---|---|---|---|
| no-skills | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| no-skills | KeystoneProperties | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| no-skills | HorizonHR | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | KeystoneProperties | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-webapi | HorizonHR | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | KeystoneProperties | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-artisan | HorizonHR | ❌ Fail | ⏭️ Skipped | ⏭️ Skipped | ⏭️ Skipped |   Determining projects to restore...   All projects are up-to-date for restore. C:\Program Files\dot |
| managedcode-dotnet-skills | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | KeystoneProperties | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| managedcode-dotnet-skills | HorizonHR | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | SparkEvents | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | KeystoneProperties | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |
| dotnet-skills | HorizonHR | ✅ Pass | ✅ Pass | ❌ 0 issues | ⚠️ 1 vulns |  |

## Automated Metrics

### Build Warnings by Category

| Configuration | Scenario | Total | Naming | Performance | Reliability | Security | Usage | Style | Other |
|---|---|---|---|---|---|---|---|---|---|
| no-skills | SparkEvents | 50 | 2 | 36 | 0 | 0 | 0 | 0 | 12 |
| no-skills | KeystoneProperties | 108 | 6 | 54 | 0 | 0 | 0 | 0 | 48 |
| no-skills | HorizonHR | 118 | 0 | 56 | 0 | 0 | 0 | 0 | 62 |
| dotnet-webapi | SparkEvents | 100 | 2 | 82 | 0 | 0 | 0 | 0 | 16 |
| dotnet-webapi | KeystoneProperties | 138 | 6 | 82 | 0 | 0 | 0 | 0 | 50 |
| dotnet-webapi | HorizonHR | 112 | 0 | 62 | 0 | 0 | 0 | 0 | 50 |
| dotnet-artisan | SparkEvents | 38 | 2 | 36 | 0 | 0 | 0 | 0 | 0 |
| dotnet-artisan | KeystoneProperties | 60 | 6 | 44 | 0 | 0 | 0 | 0 | 10 |
| dotnet-artisan | HorizonHR | 126 | 2 | 48 | 0 | 0 | 0 | 0 | 76 |
| managedcode-dotnet-skills | SparkEvents | 178 | 6 | 106 | 0 | 0 | 0 | 0 | 66 |
| managedcode-dotnet-skills | KeystoneProperties | 86 | 8 | 72 | 0 | 0 | 0 | 0 | 6 |
| managedcode-dotnet-skills | HorizonHR | 108 | 2 | 48 | 0 | 0 | 0 | 0 | 58 |
| dotnet-skills | SparkEvents | 138 | 2 | 78 | 0 | 0 | 0 | 0 | 58 |
| dotnet-skills | KeystoneProperties | 98 | 6 | 88 | 0 | 0 | 0 | 0 | 4 |
| dotnet-skills | HorizonHR | 80 | 0 | 80 | 0 | 0 | 0 | 0 | 0 |

## Skill Configurations

| Configuration | Label | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | None | None |
| dotnet-webapi | dotnet-webapi skill | skills/dotnet-webapi | None |
| dotnet-artisan | dotnet-artisan plugin chain | None | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | None |
| dotnet-skills | Official .NET Skills (dotnet/skills) | None | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |
