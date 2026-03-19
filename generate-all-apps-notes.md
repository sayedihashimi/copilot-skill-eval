# Generation Summary — All Variants

**Date**: 2026-03-19
**Generator**: Copilot CLI (`copilot -p ... --yolo`) via `generate-apps.ps1`
**Variants**: 4 (`no-skills`, `dotnet-webapi`, `dotnet-artisan`, `managedcode-dotnet-skills`)
**Apps per variant**: 3 (FitnessStudioApi, LibraryApi, VetClinicApi)
**Total projects**: 12

---

## Results Overview

| Directory | App | Build | Run | Notes |
|-----------|-----|-------|-----|-------|
| src-no-skills | FitnessStudioApi | ✅ | ✅ | |
| src-no-skills | LibraryApi | ✅ | ✅ | |
| src-no-skills | VetClinicApi | ✅ | ✅ | |
| src-dotnet-webapi | FitnessStudioApi | ✅ | ✅ | |
| src-dotnet-webapi | LibraryApi | ✅ | ✅ | |
| src-dotnet-webapi | VetClinicApi | ✅ | ✅ | |
| src-dotnet-artisan | FitnessStudioApi | ✅ | ✅ | |
| src-dotnet-artisan | LibraryApi | ✅ | ✅ | |
| src-dotnet-artisan | VetClinicApi | ✅ | ✅ | |
| src-managedcode-dotnet-skills | FitnessStudioApi | ✅ | ❌ | Nested solution structure (`FitnessStudioApi/FitnessStudioApi/*.csproj`); `dotnet run` cannot find project at root level |
| src-managedcode-dotnet-skills | LibraryApi | ✅ | ❌ | Runtime `TypeLoadException` — Swashbuckle.AspNetCore 7.3.1 incompatible with .NET 10 (`SwaggerGenerator.GetSwagger` method mismatch) |
| src-managedcode-dotnet-skills | VetClinicApi | ✅ | ✅ | |

**Build**: 12/12 ✅
**Run**: 10/12 ✅ — 2 failures in `src-managedcode-dotnet-skills`

---

## Skill Configuration Per Variant

| Directory | Skills Used | Mechanism | `gen-notes.md` |
|-----------|------------|-----------|----------------|
| `src-no-skills` | None | Baseline — no skills | ✅ Present |
| `src-dotnet-webapi` | `dotnet-webapi` | Temporarily registered in `config.json` via `Add-SkillDirectory` | ❌ Missing |
| `src-dotnet-artisan` | `using-dotnet`, `dotnet-advisor`, `dotnet-csharp`, `dotnet-api`, `dotnet-tooling` | `--plugin-dir contrib\plugins\dotnet-artisan` | ✅ Present |
| `src-managedcode-dotnet-skills` | `dotnet-project-setup`, `dotnet-aspnet-core`, `dotnet-entity-framework-core`, `dotnet-microsoft-extensions`, `dotnet-minimal-apis`, `dotnet-modern-csharp` | Temporarily registered in `config.json` via `Add-SkillDirectory` | ✅ Present |

---

## Runtime Issues (not fixed — reported as-is)

### 1. `src-managedcode-dotnet-skills/FitnessStudioApi` — cannot run

The agent created a **nested solution structure** with an `.slnx` file at the top and the actual `.csproj` inside a subdirectory:

```
FitnessStudioApi/
├── FitnessStudioApi.slnx
└── FitnessStudioApi/
    └── FitnessStudioApi.csproj   ← actual project is one level deep
```

`dotnet run` from the top-level `FitnessStudioApi/` directory fails because it expects a `.csproj` at root level. Running `dotnet run --project FitnessStudioApi/FitnessStudioApi` would work.

### 2. `src-managedcode-dotnet-skills/LibraryApi` — crashes at startup

```
System.TypeLoadException: Method 'GetSwagger' in type 'Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenerator'
from assembly 'Swashbuckle.AspNetCore.SwaggerGen, Version=7.3.1.0, ...'
```

The project references `Swashbuckle.AspNetCore 7.3.1` which is not compatible with the .NET 10 runtime. The other variants that work correctly either use no Swashbuckle or use the built-in `AddOpenApi()` / `MapOpenApi()`.

---

## Script Configuration

This run used `generate-apps.ps1` (no `-Apps` parameter — all 4 variants generated):
- **`--plugin-dir`** flag for `dotnet-artisan` (loads plugins from `contrib/plugins/dotnet-artisan`)
- **Temporary `config.json` edit** for `dotnet-webapi` and `managedcode-dotnet-skills` (registers skill dirs, restores after each run)
- Each variant's apps were generated in isolated parallel agents with no shared context
