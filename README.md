# Copilot Skill Evaluation Framework for ASP.NET Core Web APIs

A framework for evaluating how **GitHub Copilot custom skills** impact the quality of generated ASP.NET Core Web API code. It generates the same set of apps under different Copilot skill configurations, then produces a detailed comparative analysis.

## Overview

This repo answers the question: **"Do custom Copilot skills produce better code?"**

It works by asking GitHub Copilot CLI to build three realistic ASP.NET Core Web API applications — a fitness studio booking system, a community library, and a veterinary clinic — under four different configurations:

| Configuration | Directory | What It Tests |
|---|---|---|
| **No skills** | `src-no-skills/` | Baseline — Copilot with no custom skill guidance |
| **dotnet-webapi skill** | `src-dotnet-webapi/` | A single custom skill focused on Web API patterns |
| **dotnet-artisan skills** | `src-dotnet-artisan/` | A full skill chain (`using-dotnet` → `dotnet-advisor` → `dotnet-csharp` + `dotnet-api`) |
| **managedcode-dotnet-skills** | `src-managedcode-dotnet-skills/` | Community-maintained skills covering project setup, ASP.NET Core, EF Core, minimal APIs, and modern C# |

After generation, the framework analyzes the output across 16 dimensions (API style, sealed types, CancellationToken propagation, OpenAPI wiring, etc.) and produces a structured comparison report.

## Prerequisites

- [**.NET 10 SDK**](https://dotnet.microsoft.com/download) (or later)
- [**GitHub Copilot CLI**](https://docs.github.com/en/copilot/using-github-copilot/using-github-copilot-in-the-command-line) (`copilot` command available in your terminal)
- **PowerShell Core** (pwsh) — for running the generation script

## Repository Structure

```
webapi-testing/
├── .github/
│   └── prompts/                          # Copilot prompt files
│       ├── generate-apps.md              # ⭐ Primary workflow orchestrator
│       ├── analyze.md                    # Analysis report template (16 dimensions)
│       ├── create-all-apps.md            # Builds all 3 apps using individual prompts
│       ├── create-app-plan-files.md      # Planning doc for the 3 app scenarios
│       ├── create-script.md              # Instructions for creating generate-apps.ps1
│       ├── create-fitness-studio-api.prompt.md   # Spec: Zenith Fitness Studio API
│       ├── create-library-api.prompt.md          # Spec: Sunrise Community Library API
│       └── create-vet-clinic-api.prompt.md       # Spec: Happy Paws Vet Clinic API
├── contrib/
│   ├── skills/
│   │   ├── dotnet-webapi/                # Custom Copilot skill for Web API generation
│   │   └── managedcode-dotnet-skills/    # Community skills (project-setup, aspnet-core, EF Core, etc.)
│   └── plugins/
│       └── dotnet-artisan/               # Artisan plugin chain (using-dotnet → advisor → csharp + api)
├── generate-apps.ps1                     # PowerShell script that invokes Copilot CLI 4x
├── analysis.md                           # Generated comparative analysis report
├── generate-all-apps-notes.md            # Generated build/run status for all 12 projects
├── src-no-skills/                        # Generated apps (no skills)
├── src-dotnet-webapi/                    # Generated apps (dotnet-webapi skill)
├── src-dotnet-artisan/                   # Generated apps (dotnet-artisan skills)
└── src-managedcode-dotnet-skills/        # Generated apps (managedcode-dotnet-skills)
```

## Quick Start: The Main Workflow

The primary workflow is driven by **`generate-apps.md`** — a prompt file that orchestrates the entire pipeline. Here's how to use it:

### 1. Open Copilot CLI in the repo root

```bash
cd webapi-testing
```

### 2. Ask Copilot to follow the generate-apps instructions

```
Follow the instructions in the file @.github\prompts\generate-apps.md
```

This single command triggers the full pipeline:

1. **Runs `generate-apps.ps1`** — deletes existing `src-*` folders and calls `copilot -p ... --yolo` four times with different skill configurations
2. **Verifies all 12 projects** — runs `dotnet build` and `dotnet run` on each
3. **Checks `gen-notes.md`** — confirms each variant used the correct skill configuration
4. **Writes `generate-all-apps-notes.md`** — a summary of build/run results and skill configs
5. **Follows `analyze.md`** — produces `analysis.md` with a 16-dimension comparative report

> **⏱ Note:** The full pipeline takes a significant amount of time (60–120+ minutes) since it generates 12 complete ASP.NET Core apps across 4 configurations.

### 3. Review the output

After completion, read:

- **`generate-all-apps-notes.md`** — Quick summary: did everything build and run?
- **`analysis.md`** — The detailed comparison across 16 dimensions with code examples, verdicts, and rankings

## How It Works

### The Generation Script

`generate-apps.ps1` is a PowerShell script that calls the Copilot CLI four times, each with a different prompt that controls which skills are active:

```powershell
# No skills (baseline)
copilot -p "Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in 'src' put them in 'src-no-skills'. Do NOT use any skills during this process." --yolo

# dotnet-webapi skill only
copilot -p "Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in 'src' put them in 'src-dotnet-webapi'. Use the 'dotnet-webapi' but do NOT use any other skills." --yolo

# dotnet-artisan skills (full chain)
copilot -p "Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in 'src' put them in 'src-dotnet-artisan'. Use the 'dotnet-artisan' skills but do NOT use the 'dotnet-webapi' skill." --yolo

# managedcode-dotnet-skills (community skills)
copilot -p "Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in 'src' put them in 'src-managedcode-dotnet-skills'. Use the 'managedcode-dotnet-skills' skills but do NOT use any other skills." --yolo
```

The script supports an optional `-Apps` parameter to generate only specific variants:

```powershell
.\generate-apps.ps1                              # All four variants
.\generate-apps.ps1 -Apps no-skills              # Only the baseline
.\generate-apps.ps1 -Apps dotnet-webapi, dotnet-artisan  # Two variants
.\generate-apps.ps1 -Apps managedcode-dotnet-skills      # Only managedcode variant
```

### The Three Apps

Each variant generates three ASP.NET Core Web API applications from detailed prompt specifications:

| App | Domain | Key Features |
|---|---|---|
| **FitnessStudioApi** | Booking/membership | Class capacity, waitlist, membership tiers, booking windows, instructor scheduling |
| **LibraryApi** | Resource management | Borrowing limits, overdue fines, reservation queues, book availability tracking |
| **VetClinicApi** | Healthcare/service | Appointment scheduling, vaccination tracking, medical records, prescription management |

Each app is a standalone project with EF Core + SQLite, seed data, an `.http` test file, and full CRUD + specialized endpoints.

### The Analysis

The analysis compares all variants across dimensions defined in `.github/prompts/analyze.md`:

- API style (Controllers vs Minimal APIs)
- Sealed types, primary constructors, DTO design
- CancellationToken propagation, AsNoTracking usage
- Exception handling strategy, middleware patterns
- OpenAPI metadata richness, Swashbuckle usage
- File organization, pagination design, collection initialization

Each dimension includes inline code examples from the actual generated code, a verdict on which approach is best, and references to .NET best practices.

## Prompt Reference

### Primary Prompts

| Prompt File | Purpose | How to Invoke |
|---|---|---|
| `generate-apps.md` | **Full pipeline**: generate → verify → analyze | `Follow the instructions in the file @.github\prompts\generate-apps.md` |
| `analyze.md` | Generate only the analysis report | Referenced automatically by `generate-apps.md`. Can also be used standalone to regenerate `analysis.md` without re-generating apps. |
| `create-all-apps.md` | Build all 3 apps into a target directory | `Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in 'src' put them in 'src-no-skills'. Do NOT use any skills during this process.` |

### App Specification Prompts

These define _what_ each app should do. They are referenced by `create-all-apps.md` and are not typically invoked directly.

| Prompt File | App | Domain |
|---|---|---|
| `create-fitness-studio-api.prompt.md` | Zenith Fitness Studio | Booking & membership management |
| `create-library-api.prompt.md` | Sunrise Community Library | Book loans, reservations & fines |
| `create-vet-clinic-api.prompt.md` | Happy Paws Vet Clinic | Appointments, medical records & vaccinations |

You _can_ invoke these individually to build a single app:

```
build the three apps using the files @.github\prompts\create-fitness-studio-api.prompt.md @.github\prompts\create-library-api.prompt.md and @.github\prompts\create-vet-clinic-api.prompt.md
```

### Utility Prompts

| Prompt File | Purpose |
|---|---|
| `create-script.md` | Instructions that were used to create `generate-apps.ps1` |
| `create-app-plan-files.md` | Planning document that defines the 3 app scenarios and prompt structure |

## Customizing & Extending

The typical workflow for iterating on skill quality:

### 1. Edit the skill

Modify `contrib/skills/dotnet-webapi/SKILL.md` to change what guidance Copilot follows when generating Web API code. The skill covers:

- API style (minimal APIs by default)
- Sealed types, primary constructors
- OpenAPI wiring (built-in `AddOpenApi()` + `MapOpenApi()`, no Swashbuckle)
- CancellationToken propagation
- Exception handling with `IExceptionHandler`
- DTO design with sealed records
- And more (~500 lines of guidance)

### 2. Regenerate the apps

Ask Copilot to run the full pipeline:

```
Follow the instructions in the file @.github\prompts\generate-apps.md
```

Or run just the script directly if you want more control:

```powershell
.\generate-apps.ps1 -Apps dotnet-webapi
```

Then ask Copilot to do the verification and analysis steps manually.

### 3. Review the analysis

Open `analysis.md` to see the updated comparison. The executive summary table at the top gives a quick overview, and each dimension section has detailed code examples and verdicts.

### Adding New Skills or Configurations

To test a new skill configuration:

1. Create a new skill in `contrib/skills/<your-skill>/SKILL.md`
2. Add a new entry to the `$allRuns` hashtable in `generate-apps.ps1`
3. Update `generate-apps.md` if needed
4. Run the pipeline

### Adding New App Scenarios

To add a fourth app:

1. Create a new prompt file: `.github/prompts/create-<your-app>.prompt.md`
2. Add it to `create-all-apps.md`
3. Run the pipeline — the analysis will automatically discover and compare the new app

## Output Files

| File | What It Contains | Generated By |
|---|---|---|
| `generate-all-apps-notes.md` | Build/run status, skill configs, runtime issue details for all 12 projects | Copilot following `generate-apps.md` |
| `analysis.md` | 16-dimension comparative analysis with code examples, verdicts, and rankings | Copilot following `analyze.md` |
| `src-*/gen-notes.md` | Per-variant notes on which skills were used during generation | Copilot during app generation |
| `src-*/FitnessStudioApi/` | Complete ASP.NET Core Web API project | Copilot following app spec prompts |
| `src-*/LibraryApi/` | Complete ASP.NET Core Web API project | Copilot following app spec prompts |
| `src-*/VetClinicApi/` | Complete ASP.NET Core Web API project | Copilot following app spec prompts |

## The dotnet-webapi Skill

The custom skill at `contrib/skills/dotnet-webapi/SKILL.md` provides ~500 lines of guidance covering:

- **Minimal APIs by default** — only use controllers if the project already has them
- **Sealed types everywhere** — classes, records, middleware, services
- **Primary constructors** for DI injection
- **Built-in OpenAPI** — `AddOpenApi()` + `MapOpenApi()`, never install Swashbuckle
- **CancellationToken propagation** through all layers
- **`IExceptionHandler`** for global error handling (not convention-based middleware)
- **Sealed records** for DTOs with `*Request`/`*Response` naming
- **`AsNoTracking()`** for read-only queries
- **`IReadOnlyList<T>`** for collection return types
- Validation, pagination, `.http` file generation, and more

## The managedcode-dotnet-skills

Community-maintained skills at `contrib/skills/managedcode-dotnet-skills/` covering six focused areas:

- **dotnet-project-setup** — .NET solution/project scaffolding, folder structure, SDK settings
- **dotnet-aspnet-core** — ASP.NET Core hosting, middleware, configuration, logging
- **dotnet-entity-framework-core** — EF Core data access, modeling, migrations, query patterns
- **dotnet-microsoft-extensions** — Dependency injection, configuration, logging, options
- **dotnet-minimal-apis** — Minimal API endpoint design with route groups, filters, TypedResults
- **dotnet-modern-csharp** — Modern C# language features (C# 13/14) compatible with .NET 10

## License

See [LICENSE](LICENSE) for details.
