# Plan: Create README.md for webapi-testing

## Problem Statement

This repository lacks a README.md. The repo is a Copilot skill evaluation framework — it uses GitHub Copilot CLI to generate ASP.NET Core Web API apps under different skill configurations, then compares the output quality. Readers need to understand:

1. What this repo does
2. The key files and how they relate
3. How to invoke the Copilot prompts (especially `generate-apps.md`)
4. The end-to-end workflow: edit skills/prompts → generate apps → read analysis

## Research Completed

### Files Analyzed
- **`skills/dotnet-webapi/SKILL.md`** — Custom Copilot skill that guides Web API code generation (sealed types, minimal APIs, CancellationToken, IExceptionHandler, built-in OpenAPI, etc.)
- **`.github/prompts/generate-apps.md`** — The primary orchestration prompt. Tells Copilot to run `generate-apps.ps1`, verify builds/runs, write `generate-all-apps-notes.md`, then follow `analyze.md` to create `analysis.md`.
- **`.github/prompts/analyze.md`** — Template for the comparative analysis report. Defines 15+ dimensions, required sections, style guidelines.
- **`.github/prompts/create-all-apps.md`** — Prompt that builds all 3 apps (Fitness Studio, Library, Vet Clinic) using the individual prompt files, in isolated agents.
- **`.github/prompts/create-app-plan-files.md`** — Planning doc that defines the 3 app scenarios and prompt file structure.
- **`.github/prompts/create-script.md`** — Instructions for creating `generate-apps.ps1`.
- **`.github/prompts/create-fitness-studio-api.prompt.md`** — Detailed spec for Zenith Fitness Studio API.
- **`.github/prompts/create-library-api.prompt.md`** — Detailed spec for Sunrise Community Library API.
- **`.github/prompts/create-vet-clinic-api.prompt.md`** — Detailed spec for Happy Paws Vet Clinic API.
- **`generate-apps.ps1`** — PowerShell script that calls `copilot -p ... --yolo` three times with different skill configs.
- **`analysis.md`** — The generated comparative analysis (16 dimensions, executive summary, per-dimension verdicts).
- **`generate-all-apps-notes.md`** — Build/run status for all 9 generated projects.

### Prior Session History Reviewed
Searched all prior Copilot sessions for this repo. Key invocation patterns discovered:

1. **`generate-apps.md` (primary workflow)** — Invoked as: `Follow the instructions in the file @.github\prompts\generate-apps.md`. This runs the full pipeline: script → verify → notes → analysis. Used in sessions f301e82a, aa7a8690, 78b54b56, cb08c8d2.

2. **`create-all-apps.md` (per-variant generation)** — Invoked with output folder and skill overrides:
   - `Follow the instructions in the file @.github\prompts\create-all-apps.md. Instead of putting the files in 'src' put them in 'src-no-skills'. Do NOT use any skills during this process.`
   - `...put them in 'src-dotnet-webapi'. Use the 'dotnet-webapi' but do NOT use any other skills.`
   - `...put them in 'src-dotnet-artisan'. Use the 'dotnet-artisan' skills but do NOT use the 'dotnet-webapi' skill.`
   Used in 12+ sessions across multiple runs.

3. **Individual app prompts** — Invoked together: `build the three apps using the files @.github\prompts\create-fitness-studio-api.prompt.md @.github\prompts\create-library-api.prompt.md and @.github\prompts\create-vet-clinic-api.prompt.md`

4. **`create-script.md`** — Invoked as: `Read my instructions in @.github\prompts\create-script.md` — produced `generate-apps.ps1`.

5. **`analyze.md`** — Referenced indirectly via `generate-apps.md`, also used directly in session 16b631e0 to regenerate analysis with 3 directories.

## README Structure

1. **Title & Badge** — Project name, short description
2. **Overview** — What this repo does: a framework for evaluating Copilot skill impact on code generation
3. **Prerequisites** — .NET 10 SDK, GitHub Copilot CLI, PowerShell Core
4. **Repository Structure** — Table of key files/folders with descriptions
5. **Quick Start: The Main Workflow** — Step-by-step for `generate-apps.md` (the primary use case)
6. **How It Works** — Explanation of the three skill configurations and what they test
7. **Prompt Reference** — Table of all `.github/prompts/` files with descriptions and example invocations
8. **Customizing & Extending** — How to modify skills/prompts and re-run
9. **Output Files** — What gets generated and where to find it
10. **Skills Reference** — Brief description of `dotnet-webapi` skill

## Todos

1. **create-readme** — Write README.md with all sections above
