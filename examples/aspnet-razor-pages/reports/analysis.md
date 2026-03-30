# Aggregated Analysis: ASP.NET Core Razor Pages Skill Evaluation

**Runs:** 1 | **Configurations:** 5 | **Scenarios:** 3 | **Dimensions:** 23
**Date:** 2026-03-30 15:56 UTC

---

## Overview

Evaluate how custom Copilot skills impact the quality of generated ASP.NET Core Razor Pages code across three realistic application scenarios.

---

## What Was Tested

### Scenarios

Each run generates one of the following application scenarios (randomly selected per run):

| Scenario | Description |
|---|---|
| SparkEvents | Event registration portal with ticket types, capacity management, waitlists, and check-in |
| KeystoneProperties | Property management with leases, tenants, maintenance requests, and rent tracking |
| HorizonHR | Employee directory and HR portal with leave management, reviews, and skills |

### Configurations

Each configuration gives Copilot different custom skills or plugins. The **no-skills** baseline uses default Copilot with no custom instructions.

| Configuration | Description | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | — | — |
| dotnet-webapi | dotnet-webapi skill | skills/dotnet-webapi | — |
| dotnet-artisan | dotnet-artisan plugin chain | — | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | — |
| dotnet-skills | Official .NET Skills (dotnet/skills) | — | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |

### How It Works

1. **Generate** — For each configuration, Copilot CLI (`copilot --yolo`) is given a scenario prompt and generates a complete project from scratch. One scenario is randomly selected per run.
2. **Verify** — Each generated project is built (`dotnet build`), run, format-checked, and scanned for NuGet vulnerabilities.
3. **Analyze** — An AI judge reviews the source code of all configurations side-by-side and scores each across 23 quality dimensions.

Generation model: **claude-opus-4.6-1m**
Analysis model: **gpt-5.3-codex**

---

## Scoring Methodology

Each dimension is scored on a **1–5 scale**:

| Score | Meaning |
|:---:|---|
| 5 | Excellent — follows all best practices |
| 4 | Good — minor gaps only |
| 3 | Acceptable — some issues present |
| 2 | Below average — significant gaps |
| 1 | Poor — missing or fundamentally wrong |

Dimensions are grouped into **tiers** that determine their weight in the final weighted score:

| Tier | Weight | Dimensions |
|---|:---:|:---:|
| CRITICAL | ×3 | 5 |
| HIGH | ×2 | 9 |
| MEDIUM | ×1 | 8 |
| LOW | ×0.5 | 1 |

**Maximum possible weighted score: 207.5** (all dimensions scoring 5).
Scores shown as **mean ± standard deviation** across runs.

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 2.0 | 4.0 | 2.0 | 2.0 | 2.0 |
| Security Vulnerability Scan [CRITICAL] | 4.0 | 4.0 | 2.0 | 4.0 | 4.0 |
| Input Validation & Guard Clauses [CRITICAL] | 4.0 | 4.0 | 3.0 | 3.0 | 3.0 |
| NuGet & Package Discipline [CRITICAL] | 4.0 | 1.0 | 3.0 | 5.0 | 5.0 |
| EF Migration Usage [CRITICAL] | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 |
| Business Logic Correctness [HIGH] | 2.0 | 4.0 | 2.0 | 2.0 | 2.0 |
| Prefer Built-in over 3rd Party [HIGH] | 4.0 | 5.0 | 4.0 | 5.0 | 5.0 |
| Modern C# Adoption [HIGH] | 2.0 | 5.0 | 5.0 | 4.0 | 3.0 |
| Page Model Design [HIGH] | 3.0 | 4.0 | 4.0 | 4.0 | 4.0 |
| Form Handling & Validation [HIGH] | 4.0 | 4.0 | 4.0 | 3.0 | 3.0 |
| Error Handling Strategy [HIGH] | 3.0 | 3.0 | 3.0 | 3.0 | 3.0 |
| Async Patterns & Cancellation [HIGH] | 2.0 | 5.0 | 2.0 | 2.0 | 2.0 |
| EF Core Best Practices [HIGH] | 2.0 | 5.0 | 4.0 | 3.0 | 3.0 |
| Security Configuration [HIGH] | 1.0 | 2.0 | 4.0 | 1.0 | 1.0 |
| Service Abstraction & DI [MEDIUM] | 4.0 | 5.0 | 3.0 | 4.0 | 4.0 |
| UI Quality & Accessibility [MEDIUM] | 2.0 | 4.0 | 4.0 | 2.0 | 3.0 |
| Reusable UI Components [MEDIUM] | 4.0 | 4.0 | 4.0 | 3.0 | 4.0 |
| Data Seeder Design [MEDIUM] | 4.0 | 4.0 | 4.0 | 4.0 | 4.0 |
| Structured Logging [MEDIUM] | 4.0 | 4.0 | 3.0 | 4.0 | 3.0 |
| Nullable Reference Types [MEDIUM] | 5.0 | 5.0 | 5.0 | 5.0 | 5.0 |
| File Organization [MEDIUM] | 4.0 | 5.0 | 4.0 | 4.0 | 4.0 |
| Type Design & Resource Management [MEDIUM] | 3.0 | 5.0 | 3.0 | 3.0 | 3.0 |
| Code Standards Compliance [LOW] | 3.0 | 4.0 | 4.0 | 3.0 | 4.0 |

---

## Final Rankings

| Rank | Configuration | Mean Score | % of Max (207.5) | Std Dev | Min | Max |
|---|---|---|---|---|---|---|
| 🥇 | dotnet-webapi | 154.0 | 74% | 0.0 | 154.0 | 154.0 |
| 🥈 | managedcode-dotnet-skills | 129.5 | 62% | 0.0 | 129.5 | 129.5 |
| 🥉 | dotnet-artisan | 129.0 | 62% | 0.0 | 129.0 | 129.0 |
| 4th | dotnet-skills | 129.0 | 62% | 0.0 | 129.0 | 129.0 |
| 5th | no-skills | 122.5 | 59% | 0.0 | 122.5 | 122.5 |

---

## Weighted Score per Run

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 122.5 | 154.0 | 129.0 | 129.5 | 129.0 |
| **Mean** | **122.5** | **154.0** | **129.0** | **129.5** | **129.0** |

---

## Verification Summary (All Runs)

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |
|---|---|---|---|
| no-skills | 1/1 (100%) | 1/1 (100%) | 108.0 |
| dotnet-artisan | 1/1 (100%) | 1/1 (100%) | 40.0 |
| managedcode-dotnet-skills | 1/1 (100%) | 1/1 (100%) | 62.0 |
| dotnet-skills | 1/1 (100%) | 1/1 (100%) | 42.0 |

---

## Consistency Analysis

| Configuration | Score σ | Most Consistent Dim (σ) | Most Variable Dim (σ) |
|---|---|---|---|

---

## Per-Dimension Analysis

### 1. Build & Run Success [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 4 | 2 | 2 | 2 |
| **Mean** | **2.0** | **4.0** | **2.0** | **2.0** | **2.0** |

#### Analysis

```csharp
// dotnet-artisan Program.cs
await app.RunAsync();
// managedcode / dotnet-skills / no-skills Program.cs
app.Run();
// dotnet-webapi Program.cs (all 3 apps)
app.Run();
```

All discovered projects build and start. `dotnet-webapi` is strongest because it produced and successfully ran all 3 scenarios, while others only produced SparkEvents.

**Scores:** artisan 2, managedcode 2, dotnet-skills 2, no-skills 2, dotnet-webapi 4.

**Verdict:** `dotnet-webapi` wins on completeness of runnable outputs.

### 2. Security Vulnerability Scan [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 2 | 4 | 4 |
| **Mean** | **4.0** | **4.0** | **2.0** | **4.0** | **4.0** |

#### Analysis

```text
# dotnet-artisan/SparkEvents (dotnet list package --vulnerable)
> Microsoft.Build.Tasks.Core 17.7.2 High GHSA-h4j7-5rxr-p4wc
```

`dotnet-artisan` showed a transitive vulnerability; all other generated apps reported no vulnerable packages.

**Scores:** 2, 4, 4, 4, 4.

**Verdict:** Managed, official skills, baseline, and webapi tie ahead of artisan.

### 3. Input Validation & Guard Clauses [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 3 | 3 | 3 |
| **Mean** | **4.0** | **4.0** | **3.0** | **3.0** | **3.0** |

#### Analysis

```csharp
// no-skills Pages/Events/Create.cshtml.cs
[Required] public string Title { get; set; } = string.Empty;
if (!ModelState.IsValid) return Page();

// dotnet-webapi Pages/Events/Create.cshtml.cs
[Required] public string Title { get; set; } = string.Empty;
if (!ModelState.IsValid) { ... return Page(); }
```

All configs use DataAnnotations and `ModelState.IsValid` checks. Guard-clause APIs like `ArgumentNullException.ThrowIfNull()` were generally absent.

**Scores:** 3, 3, 3, 4, 4.

**Verdict:** `no-skills` and `dotnet-webapi` are slightly more comprehensive in form-model validation coverage.

### 4. NuGet & Package Discipline [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 1 | 3 | 5 | 5 |
| **Mean** | **4.0** | **1.0** | **3.0** | **5.0** | **5.0** |

#### Analysis

```xml
<!-- managedcode/dotnet-skills -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />

<!-- dotnet-webapi/SparkEvents -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="*" />
```

Managed and official skills pin exact versions cleanly. `dotnet-webapi` uses floating/wildcard versions (`*`, `10.*-*`), the biggest packaging risk.

**Scores:** 3, 5, 5, 4, 1.

**Verdict:** `managedcode-dotnet-skills` and `dotnet-skills` are best.

### 5. EF Migration Usage [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 1 | 1 | 1 | 1 |
| **Mean** | **1.0** | **1.0** | **1.0** | **1.0** | **1.0** |

#### Analysis

```csharp
// all configurations (Program.cs pattern)
await db.Database.EnsureCreatedAsync();
// or
db.Database.EnsureCreated();
```

No generated app used `Database.Migrate()` or migration workflow.

**Scores:** 1, 1, 1, 1, 1.

**Verdict:** All fail this production-critical criterion.

### 6. Business Logic Correctness [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 4 | 2 | 2 | 2 |
| **Mean** | **2.0** | **4.0** | **2.0** | **2.0** | **2.0** |

#### Analysis

```text
# prompt requires SparkEvents + KeystoneProperties + HorizonHR per configuration
# actual: only dotnet-webapi has all 3 apps; others only SparkEvents
```

SparkEvents implementations are generally broad, but 4/5 configs miss two whole scenarios.

**Scores:** 2, 2, 2, 2, 4.

**Verdict:** `dotnet-webapi` clearly best for scenario completeness.

### 7. Prefer Built-in over 3rd Party [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 5 | 5 |
| **Mean** | **4.0** | **5.0** | **4.0** | **5.0** | **5.0** |

#### Analysis

```xml
<!-- all .csproj samples -->
<Project Sdk="Microsoft.NET.Sdk.Web">
<!-- no Swashbuckle / Newtonsoft / Autofac / AutoMapper / Serilog -->
```

All outputs stay close to built-in platform dependencies; no unnecessary third-party architectural framework choices.

**Scores:** 4, 5, 5, 4, 5.

**Verdict:** Managed, official skills, and webapi are strongest.

### 8. Modern C# Adoption [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 4 | 3 |
| **Mean** | **2.0** | **5.0** | **5.0** | **4.0** | **3.0** |

#### Analysis

```csharp
// dotnet-artisan EventService.cs
public sealed class EventService(SparkEventsDbContext db, ILogger<EventService> logger)

// dotnet-webapi/HorizonHR Program-wide patterns
public sealed class DepartmentService(...)
```

`dotnet-artisan` and `dotnet-webapi` use primary constructors, collection expressions, sealed classes heavily. `no-skills` is mostly traditional constructors.

**Scores:** 5, 4, 3, 2, 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` lead modern language usage.

### 9. Page Model Design [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | 4 |
| **Mean** | **3.0** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

```csharp
// no-skills Pages/Events/Create.cshtml.cs
[BindProperty] public InputModel Input { get; set; } = new();

// managedcode Pages/Events/Create.cshtml.cs
[BindProperty] public InputModel Input { get; set; } = new();
```

All configs generally keep page models service-oriented with `InputModel`-based binding; no widespread entity over-posting anti-pattern was observed.

**Scores:** 4, 4, 4, 3, 4.

**Verdict:** Mostly good across the board; `no-skills` slightly less consistent.

### 10. Form Handling & Validation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 3 | 3 |
| **Mean** | **4.0** | **4.0** | **4.0** | **3.0** | **3.0** |

#### Analysis

```csharp
// no-skills Create handler
if (!ModelState.IsValid) return Page();
return RedirectToPage("Details", new { id = created.Id });

// shared form pages
<partial name="_ValidationScriptsPartial" />
```

PRG and validation display are present in most outputs. Explicit antiforgery attributes are rare (Razor default form tokening still applies).

**Scores:** 4, 3, 3, 4, 4.

**Verdict:** `dotnet-artisan`, `no-skills`, and `dotnet-webapi` are strongest.

### 11. Error Handling Strategy [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 3 | 3 | 3 | 3 |
| **Mean** | **3.0** | **3.0** | **3.0** | **3.0** | **3.0** |

#### Analysis

```csharp
// dotnet-webapi/HorizonHR Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

// others
app.UseExceptionHandler("/Error");
```

Error handling is present everywhere but uneven in depth.

**Scores:** 3, 3, 3, 3, 3.

**Verdict:** Tie at adequate level; none is exceptional end-to-end.

### 12. Async Patterns & Cancellation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 2 | 2 | 2 |
| **Mean** | **2.0** | **5.0** | **2.0** | **2.0** | **2.0** |

#### Analysis

```csharp
// dotnet-webapi EventService
public async Task<Event> CreateAsync(Event evt, CancellationToken ct = default)
await db.SaveChangesAsync(ct);

// managedcode EventService
public async Task<Event> CreateAsync(Event evt)
await db.SaveChangesAsync();
```

CancellationToken propagation is comprehensive in `dotnet-webapi` and largely absent in others.

**Scores:** 2, 2, 2, 2, 5.

**Verdict:** `dotnet-webapi` is clearly best practice here.

### 13. EF Core Best Practices [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 4 | 3 | 3 |
| **Mean** | **2.0** | **5.0** | **4.0** | **3.0** | **3.0** |

#### Analysis

```csharp
// dotnet-webapi EventService
.Include(e => e.EventCategory).AsNoTracking()

// no-skills EventService
_context.Events.Include(e => e.Category).AsQueryable();
```

`dotnet-webapi` consistently uses `AsNoTracking` and explicit configuration patterns; `no-skills` underuses no-tracking for reads.

**Scores:** 4, 3, 3, 2, 5.

**Verdict:** `dotnet-webapi` wins.

### 14. Security Configuration [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 2 | 4 | 1 | 1 |
| **Mean** | **1.0** | **2.0** | **4.0** | **1.0** | **1.0** |

#### Analysis

```csharp
// dotnet-artisan Program.cs
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseHttpsRedirection();

// managedcode/dotnet-skills/no-skills
app.UseExceptionHandler("/Error"); // no HSTS/HTTPS pattern in Program.cs
```

Security middleware is strongest in artisan and SparkEvents-webapi; missing in several other outputs, including Keystone/Horizon webapi apps.

**Scores:** 4, 1, 1, 1, 2.

**Verdict:** `dotnet-artisan` is best in this dimension.

### 15. Service Abstraction & DI [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 3 | 4 | 4 |
| **Mean** | **4.0** | **5.0** | **3.0** | **4.0** | **4.0** |

#### Analysis

```csharp
// Program.cs
builder.Services.AddScoped<IEventService, EventService>();
```

All configs use interface-based DI; `dotnet-webapi` has broader, consistent interface-service coverage across three apps.

**Scores:** 3, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` strongest, then managed/official/baseline.

### 16. UI Quality & Accessibility [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 4 | 4 | 2 | 3 |
| **Mean** | **2.0** | **4.0** | **4.0** | **2.0** | **3.0** |

#### Analysis

```html
<!-- dotnet-artisan _Layout.cshtml -->
<nav ... aria-label="Main navigation"> ... aria-current="page" ... </nav>
<div class="alert ..." role="alert">...</div>
```

Artisan and webapi variants include richer ARIA and semantic usage. Managed and baseline are more basic.

**Scores:** 4, 2, 3, 2, 4.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` lead accessibility detail.

### 17. Reusable UI Components [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 3 | 4 |
| **Mean** | **4.0** | **4.0** | **4.0** | **3.0** | **4.0** |

#### Analysis

```html
<!-- shared partial patterns -->
<partial name="_Pagination" />
<partial name="_StatusBadge" />
```

All variants use reusable partials; baseline also includes a ViewComponent (`StatusBadge`).

**Scores:** 4, 3, 4, 4, 4.

**Verdict:** Good overall; managedcode slightly behind in component richness.

### 18. Data Seeder Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.0** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

```csharp
// Program.cs seeding pattern (all)
await DataSeeder.SeedAsync(db);
```

All generated apps include startup seeding with practical domain samples.

**Scores:** 4, 4, 4, 4, 4.

**Verdict:** Broad tie.

### 19. Structured Logging [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 3 | 4 | 3 |
| **Mean** | **4.0** | **4.0** | **3.0** | **4.0** | **3.0** |

#### Analysis

```csharp
// managedcode EventService
logger.LogInformation("Created event {EventTitle} (ID: {EventId})", evt.Title, evt.Id);
```

All configs use `ILogger<T>` and structured templates to varying depth.

**Scores:** 3, 4, 3, 4, 4.

**Verdict:** Managed, no-skills, and webapi are strongest.

### 20. Nullable Reference Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **5.0** | **5.0** | **5.0** | **5.0** | **5.0** |

#### Analysis

```xml
<Nullable>enable</Nullable>
```

Enabled in all examined `.csproj` files.

**Scores:** 5, 5, 5, 5, 5.

**Verdict:** Full tie at best score.

### 21. File Organization [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 4 | 4 |
| **Mean** | **4.0** | **5.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

```text
Pages/{Feature}/..., Services/, Models/, Data/
```

All follow clean Razor feature folders; `dotnet-webapi` shows strongest consistency across all three scenarios.

**Scores:** 4, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` best overall structure consistency.

### 22. Type Design & Resource Management [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 3 | 3 | 3 |
| **Mean** | **3.0** | **5.0** | **3.0** | **3.0** | **3.0** |

#### Analysis

```csharp
// dotnet-webapi
public sealed class EventService(...)
modelBuilder.Entity<Event>().Property(e => e.Status).HasConversion<string>();
```

`dotnet-webapi` stands out with pervasive sealed types, enum string conversions, and strong resource-safe async usage.

**Scores:** 3, 3, 3, 3, 5.

**Verdict:** `dotnet-webapi` best.

### 23. Code Standards Compliance [LOW × 0]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 3 | 4 |
| **Mean** | **3.0** | **4.0** | **4.0** | **3.0** | **4.0** |

#### Analysis

```csharp
namespace SparkEvents.Pages.Events;
public async Task<IActionResult> OnPostAsync() { ... }
```

Naming and file-scoped namespaces are generally good; consistency is a bit lower in managedcode/no-skills.

**Scores:** 4, 3, 4, 3, 4.

**Verdict:** `dotnet-artisan`, `dotnet-skills`, and `dotnet-webapi` are strongest.

---

## Asset Usage Summary

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | 90e289fa…b1e0 | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-artisan | 1 | ae756689…e036 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-tooling, using-dotnet, using-dotnet, using-dotnet, using-dotnet, using-dotnet, dotnet-advisor, using-dotnet, dotnet-advisor, dotnet-advisor, dotnet-advisor, dotnet-advisor, dotnet-api, dotnet-csharp, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-csharp, dotnet-api, dotnet-api, dotnet-csharp, dotnet-csharp, dotnet-csharp | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | fb19e67d…4511 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-modern-csharp, dotnet-project-setup, dotnet-entity-framework-core | — | ✅ |
| dotnet-skills | 1 | 5743e20e…e25f | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |

---

## Raw Data References

- Per-run analysis: `reports/analysis-run-1.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
