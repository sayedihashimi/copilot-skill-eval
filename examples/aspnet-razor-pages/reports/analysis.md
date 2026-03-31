# Aggregated Analysis: ASP.NET Core Razor Pages Skill Evaluation

**Runs:** 1 | **Configurations:** 5 | **Scenarios:** 3 | **Dimensions:** 24
**Date:** 2026-03-31 12:28 UTC

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
3. **Analyze** — An AI judge reviews the source code of all configurations side-by-side and scores each across 24 quality dimensions.

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
| Build & Run Success [CRITICAL] | 5.0 | 5.0 | 5.0 | 5.0 | 5.0 |
| Security Vulnerability Scan [CRITICAL] | 5.0 | 5.0 | 5.0 | 5.0 | 5.0 |
| Input Validation & Guard Clauses [CRITICAL] | 3.0 | 4.0 | 4.0 | 4.0 | 3.0 |
| NuGet & Package Discipline [CRITICAL] | 5.0 | 2.0 | 5.0 | 5.0 | 2.0 |
| EF Migration Usage [CRITICAL] | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 |
| Business Logic Correctness [HIGH] | 3.0 | 4.0 | 4.0 | 4.0 | 3.0 |
| Prefer Built-in over 3rd Party [HIGH] | 5.0 | 5.0 | 5.0 | 5.0 | 5.0 |
| Modern C# Adoption [HIGH] | 3.0 | 5.0 | 4.0 | 4.0 | 3.0 |
| Page Model Design [HIGH] | 2.0 | 4.0 | 4.0 | 4.0 | 2.0 |
| Form Handling & Validation [HIGH] | 3.0 | 4.0 | 4.0 | 4.0 | 3.0 |
| Error Handling Strategy [HIGH] | 3.0 | 4.0 | 3.0 | 4.0 | 3.0 |
| Async Patterns & Cancellation [HIGH] | 2.0 | 5.0 | 3.0 | 3.0 | 2.0 |
| EF Core Best Practices [HIGH] | 3.0 | 4.0 | 4.0 | 5.0 | 3.0 |
| Security Configuration [HIGH] | 2.0 | 2.0 | 2.0 | 3.0 | 2.0 |
| Service Abstraction & DI [MEDIUM] | 3.0 | 5.0 | 4.0 | 4.0 | 3.0 |
| UI Quality & Accessibility [MEDIUM] | 3.0 | 3.0 | 3.0 | 4.0 | 3.0 |
| Reusable UI Components [MEDIUM] | 2.0 | 2.0 | 2.0 | 4.0 | 2.0 |
| Data Seeder Design [MEDIUM] | 3.0 | 4.0 | 4.0 | 4.0 | 3.0 |
| Structured Logging [MEDIUM] | 3.0 | 4.0 | 4.0 | 4.0 | 3.0 |
| Nullable Reference Types [MEDIUM] | 5.0 | 5.0 | 5.0 | 5.0 | 5.0 |
| File Organization [MEDIUM] | 4.0 | 4.0 | 4.0 | 4.0 | 4.0 |
| Type Design & Resource Management [MEDIUM] | 3.0 | 4.0 | 4.0 | 4.0 | 3.0 |
| Code Standards Compliance [LOW] | 3.0 | 4.0 | 4.0 | 4.0 | 3.0 |
| Scenario Coverage (3 apps expected) [MEDIUM] | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 |

---

## Final Rankings

| Rank | Configuration | Mean Score | % of Max (207.5) | Std Dev | Min | Max |
|---|---|---|---|---|---|---|
| 🥇 | managedcode-dotnet-skills | 168.0 | 81% | 0.0 | 168.0 | 168.0 |
| 🥈 | dotnet-webapi | 159.0 | 77% | 0.0 | 159.0 | 159.0 |
| 🥉 | dotnet-artisan | 159.0 | 77% | 0.0 | 159.0 | 159.0 |
| 4th | no-skills | 137.5 | 66% | 0.0 | 137.5 | 137.5 |
| 5th | dotnet-skills | 128.5 | 62% | 0.0 | 128.5 | 128.5 |

---

## Weighted Score per Run

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 137.5 | 159.0 | 159.0 | 168.0 | 128.5 |
| **Mean** | **137.5** | **159.0** | **159.0** | **168.0** | **128.5** |

---

## Verification Summary (All Runs)

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |
|---|---|---|---|
| no-skills | 1/1 (100%) | 1/1 (100%) | 98.0 |
| dotnet-webapi | 1/1 (100%) | 1/1 (100%) | 40.0 |
| dotnet-artisan | 1/1 (100%) | 1/1 (100%) | 38.0 |
| managedcode-dotnet-skills | 1/1 (100%) | 1/1 (100%) | 66.0 |
| dotnet-skills | 1/1 (100%) | 1/1 (100%) | 46.0 |

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
| 1 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **5.0** | **5.0** | **5.0** | **5.0** | **5.0** |

#### Analysis

All five `SparkEvents` projects compile and start (`BuildSucceeded=true`, `WarningCount=0`, `RunAlive8s=true`).

```csharp
// dotnet-artisan: Program.cs
await app.RunAsync();

// dotnet-skills/no-skills/dotnet-webapi: Program.cs
app.Run();
```

Scores: artisan **5**, managedcode **5**, dotnet-skills **5**, no-skills **5**, webapi **5**.

Verdict: Tie; all are runnable at baseline.

### 2. Security Vulnerability Scan [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **5.0** | **5.0** | **5.0** | **5.0** | **5.0** |

#### Analysis

`dotnet list package --vulnerable` returned no vulnerable packages across all five.

```text
# measured for each config under run-1/SparkEvents/src/SparkEvents
HasVulnerabilities=false
```

Scores: all **5**.

Verdict: Tie.

### 3. Input Validation & Guard Clauses [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | 3 |
| **Mean** | **3.0** | **4.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

All configs use DataAnnotations + `ModelState.IsValid`; stronger variants enforce domain guards in services.

```csharp
// dotnet-artisan: Services/RegistrationService.cs
if (now < evt.RegistrationOpenDate || now > evt.RegistrationCloseDate)
    throw new InvalidOperationException("Registration is not open for this event.");
if (hasDuplicate)
    throw new InvalidOperationException("This attendee is already registered for this event.");
```

```csharp
// dotnet-webapi: Pages/Events/Register.cshtml.cs
if (!ModelState.IsValid)
{
    Attendees = await _attendeeService.GetAllListAsync(cancellationToken);
    return Page();
}
```

Scores: **4, 4, 3, 3, 4**.

Verdict: artisan/managedcode/webapi are best on service-boundary validation.

### 4. NuGet & Package Discipline [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 2 | 5 | 5 | 2 |
| **Mean** | **5.0** | **2.0** | **5.0** | **5.0** | **2.0** |

#### Analysis

Pinned versions in artisan/managedcode/no-skills; floating versions in dotnet-skills and webapi.

```xml
<!-- dotnet-artisan: SparkEvents.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />

<!-- dotnet-webapi: SparkEvents.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

Scores: **5, 5, 2, 5, 2**.

Verdict: exact pinning is the clear winner.

### 5. EF Migration Usage [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 1 | 1 | 1 | 1 |
| **Mean** | **1.0** | **1.0** | **1.0** | **1.0** | **1.0** |

#### Analysis

All projects use `EnsureCreated` and do not use migrations startup.

```csharp
// all Program.cs variants
await context.Database.EnsureCreatedAsync(); // or EnsureCreated()
```

Scores: all **1**.

Verdict: shared production-readiness gap.

### 6. Business Logic Correctness [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | 3 |
| **Mean** | **3.0** | **4.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

SparkEvents rules (capacity, waitlist, cancellation windows, status transitions) are implemented well in service-heavy variants.

```csharp
// dotnet-artisan: Services/RegistrationService.cs (waitlist promotion)
var firstWaitlisted = await db.Registrations
    .Where(r => r.EventId == reg.EventId && r.Status == RegistrationStatus.Waitlisted)
    .OrderBy(r => r.WaitlistPosition)
    .FirstOrDefaultAsync();
```

Scores: **4, 4, 3, 3, 4**.

Verdict: webapi/managedcode/artisan lead inside SparkEvents logic.

### 7. Prefer Built-in over 3rd Party [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **5.0** | **5.0** | **5.0** | **5.0** | **5.0** |

#### Analysis

All use built-in ASP.NET Core + EF Core only (no Swashbuckle/Newtonsoft/Autofac/Serilog).

```xml
<!-- representative across configs -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" ... />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" ... />
```

Scores: all **5**.

Verdict: excellent shared dependency discipline.

### 8. Modern C# Adoption [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 4 | 4 | 3 |
| **Mean** | **3.0** | **5.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

webapi is most modern; dotnet-skills/no-skills are more traditional constructor style.

```csharp
// dotnet-webapi: Services/EventService.cs
public sealed class EventService(SparkEventsDbContext context, ILogger<EventService> logger) : IEventService

// dotnet-skills: Services/EventService.cs
public sealed class EventService : IEventService
{
    private readonly SparkEventsDbContext _db;
}
```

Scores: **4, 4, 3, 3, 5**.

Verdict: dotnet-webapi best modern C# profile.

### 9. Page Model Design [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 4 | 4 | 4 | 2 |
| **Mean** | **2.0** | **4.0** | **4.0** | **4.0** | **2.0** |

#### Analysis

dotnet-skills/no-skills frequently inject `DbContext` in pages; webapi keeps stronger service-only page boundaries.

```csharp
// no-skills: Pages/Events/Register.cshtml.cs
private readonly SparkEventsDbContext _db;

// dotnet-webapi: Pages/Events/Register.cshtml.cs
public RegisterModel(IRegistrationService registrationService, IEventService eventService,
    IAttendeeService attendeeService, ITicketTypeService ticketTypeService)
```

Scores: **4, 4, 2, 2, 4**.

Verdict: avoid direct page-level DbContext for cleaner testability/security.

### 10. Form Handling & Validation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | 3 |
| **Mean** | **3.0** | **4.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

All include validation helpers and scripts partial; PRG pattern is generally used.

```html
<!-- representative across configs -->
<div asp-validation-summary="ModelOnly"></div>
<span asp-validation-for="Input.AttendeeId"></span>
<partial name="_ValidationScriptsPartial" />
```

Scores: **4, 4, 3, 3, 4**.

Verdict: strong baseline; webapi/artisan/managedcode are most consistent.

### 11. Error Handling Strategy [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 3 | 4 | 3 |
| **Mean** | **3.0** | **4.0** | **3.0** | **4.0** | **3.0** |

#### Analysis

managedcode adds HSTS; webapi adds centralized exception handler service and ProblemDetails registration.

```csharp
// managedcode: Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// dotnet-webapi: Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseStatusCodePages();
```

Scores: **3, 4, 3, 3, 4**.

Verdict: managedcode and webapi are strongest.

### 12. Async Patterns & Cancellation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 3 | 3 | 2 |
| **Mean** | **2.0** | **5.0** | **3.0** | **3.0** | **2.0** |

#### Analysis

Only webapi consistently propagates `CancellationToken` through handlers and services.

```csharp
// dotnet-webapi: Services/EventService.cs
public async Task<Event?> GetByIdAsync(int id, CancellationToken ct = default)
    => await context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

// dotnet-skills: Data/DataSeeder.cs
public static void Seed(SparkEventsDbContext db)
{
    db.SaveChanges();
}
```

Scores: **3, 3, 2, 2, 5**.

Verdict: dotnet-webapi clearly best.

### 13. EF Core Best Practices [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 5 | 3 |
| **Mean** | **3.0** | **4.0** | **4.0** | **5.0** | **3.0** |

#### Analysis

All use fluent mapping and `AsNoTracking` in reads; managedcode adds save timestamp automation.

```csharp
// managedcode: Data/SparkEventsDbContext.cs
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    UpdateTimestamps();
    return base.SaveChangesAsync(cancellationToken);
}
```

Scores: **4, 5, 3, 3, 4**.

Verdict: managedcode has best EF operational polish.

### 14. Security Configuration [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 3 | 2 |
| **Mean** | **2.0** | **2.0** | **2.0** | **3.0** | **2.0** |

#### Analysis

No config enables `UseHttpsRedirection`; only managedcode includes `UseHsts`.

```csharp
// managedcode Program.cs
app.UseHsts();
```

Scores: **2, 3, 2, 2, 2**.

Verdict: important shared gap despite otherwise solid app quality.

### 15. Service Abstraction & DI [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 4 | 4 | 3 |
| **Mean** | **3.0** | **5.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

webapi has broadest service surface; others are narrower.

```csharp
// dotnet-webapi Program.cs
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IAttendeeService, AttendeeService>();
```

Scores: **4, 4, 3, 3, 5**.

Verdict: webapi strongest architecture layering.

### 16. UI Quality & Accessibility [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 3 | 3 | 4 | 3 |
| **Mean** | **3.0** | **3.0** | **3.0** | **4.0** | **3.0** |

#### Analysis

All use Bootstrap + tag helpers; managedcode stands out with reusable status badge component.

```csharp
// managedcode: Pages/Shared/Components/StatusBadge/StatusBadgeViewComponent.cs
public IViewComponentResult Invoke(string statusText, string? displayText = null)
```

Scores: **3, 4, 3, 3, 3**.

Verdict: managedcode best UX reuse signal.

### 17. Reusable UI Components [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 4 | 2 |
| **Mean** | **2.0** | **2.0** | **2.0** | **4.0** | **2.0** |

#### Analysis

Only managedcode includes explicit ViewComponent composition.

```csharp
// managedcode
public class StatusBadgeViewComponent : ViewComponent
```

Scores: **2, 4, 2, 2, 2**.

Verdict: managedcode clearly leads DRY Razor composition.

### 18. Data Seeder Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | 3 |
| **Mean** | **3.0** | **4.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

artisan/managedcode/webapi use async seeding; dotnet-skills uses synchronous seeding.

```csharp
// dotnet-artisan Program.cs
await DataSeeder.SeedAsync(context);

// dotnet-skills DataSeeder.cs
public static void Seed(SparkEventsDbContext db)
```

Scores: **4, 4, 3, 3, 4**.

Verdict: async seeding patterns are preferable.

### 19. Structured Logging [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | 3 |
| **Mean** | **3.0** | **4.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

Structured `ILogger<T>` templates are present in service layers.

```csharp
// dotnet-artisan: Services/RegistrationService.cs
logger.LogInformation("Registration created: {ConfirmationNumber} for event {EventId}, status: {Status}",
    confirmationNumber, eventId, registration.Status);
```

Scores: **4, 4, 3, 3, 4**.

Verdict: good baseline across variants.

### 20. Nullable Reference Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **5.0** | **5.0** | **5.0** | **5.0** | **5.0** |

#### Analysis

All enable NRT.

```xml
<!-- all SparkEvents.csproj -->
<Nullable>enable</Nullable>
```

Scores: all **5**.

Verdict: tie.

### 21. File Organization [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.0** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

All follow clean Razor layout by concern.

```text
src/SparkEvents/{Data,Models,Pages,Services,Program.cs}
```

Scores: all **4**.

Verdict: tie.

### 22. Type Design & Resource Management [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | 3 |
| **Mean** | **3.0** | **4.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

webapi/artisan/managedcode have stronger modern type style; no manual DbContext disposal anti-pattern detected.

```csharp
// dotnet-webapi
public sealed class EventService(...)
```

Scores: **4, 4, 3, 3, 4**.

Verdict: modern sealed/service patterns improve consistency.

### 23. Code Standards Compliance [LOW × 0]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | 3 |
| **Mean** | **3.0** | **4.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

Naming and async suffix usage are generally aligned; webapi/artisan/managedcode slightly tighter.

```csharp
// representative
public async Task<IActionResult> OnPostAsync(...)
```

Scores: **4, 4, 3, 3, 4**.

Verdict: no severe standards issues.

### 24. Scenario Coverage (3 apps expected) [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 1 | 1 | 1 | 1 |
| **Mean** | **1.0** | **1.0** | **1.0** | **1.0** | **1.0** |

#### Analysis

Every config’s `run-1` includes only `SparkEvents`; no `KeystoneProperties` or `HorizonHR`.

```text
# output/{config}/run-1
Run1Apps: SparkEvents
HasKeystone=false
HasHorizon=false
```

Scores: all **1**.

Verdict: this is the biggest shared completeness failure.

---

## Asset Usage Summary

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | a89cd48f…169c | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-webapi | 1 | 9a779929…f5c6 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-artisan | 1 | 1f049787…31d1 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp, dotnet-tooling, using-dotnet, using-dotnet, dotnet-advisor, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | ca8efda4…6094 | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-project-setup, dotnet-modern-csharp, dotnet-entity-framework-core | — | ✅ |
| dotnet-skills | 1 | a8ff7968…a1ee | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |

---

## Raw Data References

- Per-run analysis: `reports/analysis-run-1.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
