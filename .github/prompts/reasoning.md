# Why dotnet-webapi Wins Both Evaluations

## The Surprising Result

The `dotnet-webapi` configuration — a single 20KB SKILL.md file — outperforms all other configurations in both the **WebAPI** and **Razor Pages** evaluations, including multi-megabyte plugin chains with dozens of skills.

| Rank | Config | WebAPI Score | Razor Score | Size | Files |
|------|--------|-------------|-------------|------|-------|
| 🥇 | dotnet-webapi | 194.5 | 183.5 | 20 KB | 1 |
| 🥈 | dotnet-artisan | 155.5 | 170.5 | 5.49 MB | 881 |
| 🥉 | managedcode | 138.5 | 147 | 251 KB | 22 |
| 4th | dotnet-skills | 138.5 | 146 | 992 KB | 151 |
| 5th | no-skills | 118.0 | 141 | 0 | 0 |

## Why: Focused Guidance Beats Broad Coverage

### The skill is ~50% general C# / .NET patterns

The `dotnet-webapi` SKILL.md contains a significant amount of **framework-agnostic** guidance that applies equally to Web APIs and Razor Pages:

| Pattern | Where in SKILL.md | Framework-Agnostic? |
|---------|-------------------|---------------------|
| `sealed` on all classes and records | General section | ✅ Yes |
| `sealed record` DTOs with `init` properties | Step 2 | ✅ Yes |
| `IReadOnlyList<T>` over `List<T>` | Step 2 | ✅ Yes |
| `CancellationToken` on every async method | Step 3 | ✅ Yes |
| `IExceptionHandler` with ProblemDetails | Step 5 | ✅ Yes |
| EF Migrations over `EnsureCreated()` | Step 6 | ✅ Yes |
| `AsNoTracking()` on read queries | Step 6 | ✅ Yes |
| Interface-based DI (`AddScoped<IService, Service>()`) | Step 6 | ✅ Yes |
| Primary constructors on service classes | Steps 5-6 (code examples) | ✅ Yes |
| Structured logging with `ILogger<T>` | Step 5 (code examples) | ✅ Yes |
| No Swashbuckle — use built-in OpenAPI | Step 4 | ✅ Yes |
| No `Newtonsoft.Json` — use `System.Text.Json` | Implicit | ✅ Yes |

This general guidance carries over directly to Razor Pages, which is why dotnet-webapi wins there despite being a Web API-specific skill.

### The other configs have dilution problems

| Config | Problem |
|--------|---------|
| **dotnet-artisan** (5.49 MB, 881 files) | Covers the entire .NET ecosystem: MAUI, WPF, Blazor, DevOps, debugging. Most guidance is irrelevant to a Web API or Razor Pages task. The sheer volume may prevent Copilot from focusing on the patterns that matter. |
| **managedcode-dotnet-skills** (251 KB, 22 files) | Uses a routing-based approach (classify repo → route to narrowest skill). Good coverage but routing overhead may mean some guidance doesn't get applied. |
| **dotnet-skills** (992 KB, 151 files) | Heavily focused on MSBuild (14 skills), testing (8 skills), diagnostics (6 skills), and upgrades (6 skills). Only 3 of 46 skills are core .NET. Zero skills specifically address Minimal API patterns, EF Core best practices, or Web API design. |
| **no-skills** (baseline) | No guidance at all — Copilot relies on its training data only. |

### The biggest score differentiators

Three dimensions create the largest gaps in the **WebAPI** evaluation:

| Dimension | Weight | dotnet-webapi | Others (avg) | Why |
|-----------|--------|--------------|--------------|-----|
| **EF Migration Usage** | 3× (critical) | 5 | 1 | Skill explicitly says "Use migrations, not EnsureCreated()" — no other config teaches this |
| **Minimal API Architecture** | 3× (critical) | 5 | 2 | Skill defaults to Minimal APIs with route groups and TypedResults — others often produce controllers |
| **Prefer Built-in over 3rd Party** | 2× (high) | 5 | 2.5 | Skill explicitly prohibits Swashbuckle — others frequently include it |

EF Migration alone accounts for **12 weighted points** of difference (5×3 vs 1×3). This is a binary signal — the skill either teaches migrations or it doesn't.

In the **Razor Pages** evaluation, the gap is narrower. The main differentiators are:

| Dimension | Weight | dotnet-webapi | Others (avg) | Why |
|-----------|--------|--------------|--------------|-----|
| **Build & Run Success** | 3× (critical) | 5 | 4 | dotnet-artisan had a HorizonHR build failure |
| **Error Handling Strategy** | 2× (high) | 5 | 3 | Skill teaches IExceptionHandler — applies to Razor Pages too |
| **Async Patterns & Cancellation** | 2× (high) | 5 | 3 | Skill mandates CancellationToken everywhere |
| **Modern C# Adoption** | 2× (high) | 5 | 3.5 | Skill examples use primary constructors consistently |

## Caveats and Considerations

### 1. Single-run evaluation noise

These are single-run LLM-as-judge evaluations. Running multiple times would give more reliable scores. A single build failure (dotnet-artisan's HorizonHR) costs 6 weighted points — enough to change rankings.

### 2. Possible dimension bias toward dotnet-webapi

The `dotnet-webapi` skill was likely authored with awareness of the quality signals the .NET team cares about. The dimensions in our eval.yaml align closely with what the skill teaches. Other packages weren't designed to optimize for *our* scoring rubric.

### 3. Razor Pages evaluation gap

The Razor Pages example uses the **exact same** `dotnet-webapi` SKILL.md (identical file hash). There is no Razor Pages-specific skill in any configuration. This means we're really measuring "which general .NET guidance is best" rather than "which Razor Pages skill is best."

A dedicated Razor Pages skill — covering page model design, form handling, tag helpers, ViewComponents, and named handlers — might outperform dotnet-webapi on Razor-specific dimensions.

### 4. Practical implication

**For the .NET team:** A small, focused skill that explicitly teaches the right patterns is more effective than a large skill library that covers many domains but doesn't specifically address the target scenario's best practices. When creating skills, prioritize **explicit, prescriptive guidance** for the exact patterns you want to see in generated code.
