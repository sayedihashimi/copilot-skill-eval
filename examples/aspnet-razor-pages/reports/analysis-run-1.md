# Comparative Analysis: dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, no-skills, dotnet-webapi

I analyzed **5 configuration directories** under `output/` and evaluated generated apps for run 1. Actual contents differed from expectation: `dotnet-artisan`, `managedcode-dotnet-skills`, `dotnet-skills`, and `no-skills` each generated only `SparkEvents` under `run-1/`; `dotnet-webapi` generated `SparkEvents`, `KeystoneProperties`, and `HorizonHR` but directly under `output/dotnet-webapi/` (no `run-1/`). Config identity was confirmed via `gen-notes.md` when present, otherwise inferred from directory names.

## Executive Summary

| Dimension [Tier] | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills | no-skills | dotnet-webapi |
|---|---:|---:|---:|---:|---:|
| Build & Run Success [CRITICAL] | 2 | 2 | 2 | 2 | 4 |
| Security Vulnerability Scan [CRITICAL] | 2 | 4 | 4 | 4 | 4 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 3 | 3 | 4 | 4 |
| NuGet & Package Discipline [CRITICAL] | 3 | 5 | 5 | 4 | 1 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 2 | 2 | 2 | 2 | 4 |
| Prefer Built-in over 3rd Party [HIGH] | 4 | 5 | 5 | 4 | 5 |
| Modern C# Adoption [HIGH] | 5 | 4 | 3 | 2 | 5 |
| Page Model Design [HIGH] | 4 | 4 | 4 | 3 | 4 |
| Form Handling & Validation [HIGH] | 4 | 3 | 3 | 4 | 4 |
| Error Handling Strategy [HIGH] | 3 | 3 | 3 | 3 | 3 |
| Async Patterns & Cancellation [HIGH] | 2 | 2 | 2 | 2 | 5 |
| EF Core Best Practices [HIGH] | 4 | 3 | 3 | 2 | 5 |
| Security Configuration [HIGH] | 4 | 1 | 1 | 1 | 2 |
| Service Abstraction & DI [MEDIUM] | 3 | 4 | 4 | 4 | 5 |
| UI Quality & Accessibility [MEDIUM] | 4 | 2 | 3 | 2 | 4 |
| Reusable UI Components [MEDIUM] | 4 | 3 | 4 | 4 | 4 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 3 | 4 | 3 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 5 | 5 | 5 | 5 | 5 |
| File Organization [MEDIUM] | 4 | 4 | 4 | 4 | 5 |
| Type Design & Resource Management [MEDIUM] | 3 | 3 | 3 | 3 | 5 |
| Code Standards Compliance [LOW] | 4 | 3 | 4 | 3 | 4 |

## 1. Build & Run Success [CRITICAL]

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

## 2. Security Vulnerability Scan [CRITICAL]

```text
# dotnet-artisan/SparkEvents (dotnet list package --vulnerable)
> Microsoft.Build.Tasks.Core 17.7.2 High GHSA-h4j7-5rxr-p4wc
```

`dotnet-artisan` showed a transitive vulnerability; all other generated apps reported no vulnerable packages.

**Scores:** 2, 4, 4, 4, 4.

**Verdict:** Managed, official skills, baseline, and webapi tie ahead of artisan.

## 3. Input Validation & Guard Clauses [CRITICAL]

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

## 4. NuGet & Package Discipline [CRITICAL]

```xml
<!-- managedcode/dotnet-skills -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />

<!-- dotnet-webapi/SparkEvents -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="*" />
```

Managed and official skills pin exact versions cleanly. `dotnet-webapi` uses floating/wildcard versions (`*`, `10.*-*`), the biggest packaging risk.

**Scores:** 3, 5, 5, 4, 1.

**Verdict:** `managedcode-dotnet-skills` and `dotnet-skills` are best.

## 5. EF Migration Usage [CRITICAL]

```csharp
// all configurations (Program.cs pattern)
await db.Database.EnsureCreatedAsync();
// or
db.Database.EnsureCreated();
```

No generated app used `Database.Migrate()` or migration workflow.

**Scores:** 1, 1, 1, 1, 1.

**Verdict:** All fail this production-critical criterion.

## 6. Business Logic Correctness [HIGH]

```text
# prompt requires SparkEvents + KeystoneProperties + HorizonHR per configuration
# actual: only dotnet-webapi has all 3 apps; others only SparkEvents
```

SparkEvents implementations are generally broad, but 4/5 configs miss two whole scenarios.

**Scores:** 2, 2, 2, 2, 4.

**Verdict:** `dotnet-webapi` clearly best for scenario completeness.

## 7. Prefer Built-in over 3rd Party [HIGH]

```xml
<!-- all .csproj samples -->
<Project Sdk="Microsoft.NET.Sdk.Web">
<!-- no Swashbuckle / Newtonsoft / Autofac / AutoMapper / Serilog -->
```

All outputs stay close to built-in platform dependencies; no unnecessary third-party architectural framework choices.

**Scores:** 4, 5, 5, 4, 5.

**Verdict:** Managed, official skills, and webapi are strongest.

## 8. Modern C# Adoption [HIGH]

```csharp
// dotnet-artisan EventService.cs
public sealed class EventService(SparkEventsDbContext db, ILogger<EventService> logger)

// dotnet-webapi/HorizonHR Program-wide patterns
public sealed class DepartmentService(...)
```

`dotnet-artisan` and `dotnet-webapi` use primary constructors, collection expressions, sealed classes heavily. `no-skills` is mostly traditional constructors.

**Scores:** 5, 4, 3, 2, 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` lead modern language usage.

## 9. Page Model Design [HIGH]

```csharp
// no-skills Pages/Events/Create.cshtml.cs
[BindProperty] public InputModel Input { get; set; } = new();

// managedcode Pages/Events/Create.cshtml.cs
[BindProperty] public InputModel Input { get; set; } = new();
```

All configs generally keep page models service-oriented with `InputModel`-based binding; no widespread entity over-posting anti-pattern was observed.

**Scores:** 4, 4, 4, 3, 4.

**Verdict:** Mostly good across the board; `no-skills` slightly less consistent.

## 10. Form Handling & Validation [HIGH]

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

## 11. Error Handling Strategy [HIGH]

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

## 12. Async Patterns & Cancellation [HIGH]

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

## 13. EF Core Best Practices [HIGH]

```csharp
// dotnet-webapi EventService
.Include(e => e.EventCategory).AsNoTracking()

// no-skills EventService
_context.Events.Include(e => e.Category).AsQueryable();
```

`dotnet-webapi` consistently uses `AsNoTracking` and explicit configuration patterns; `no-skills` underuses no-tracking for reads.

**Scores:** 4, 3, 3, 2, 5.

**Verdict:** `dotnet-webapi` wins.

## 14. Security Configuration [HIGH]

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

## 15. Service Abstraction & DI [MEDIUM]

```csharp
// Program.cs
builder.Services.AddScoped<IEventService, EventService>();
```

All configs use interface-based DI; `dotnet-webapi` has broader, consistent interface-service coverage across three apps.

**Scores:** 3, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` strongest, then managed/official/baseline.

## 16. UI Quality & Accessibility [MEDIUM]

```html
<!-- dotnet-artisan _Layout.cshtml -->
<nav ... aria-label="Main navigation"> ... aria-current="page" ... </nav>
<div class="alert ..." role="alert">...</div>
```

Artisan and webapi variants include richer ARIA and semantic usage. Managed and baseline are more basic.

**Scores:** 4, 2, 3, 2, 4.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` lead accessibility detail.

## 17. Reusable UI Components [MEDIUM]

```html
<!-- shared partial patterns -->
<partial name="_Pagination" />
<partial name="_StatusBadge" />
```

All variants use reusable partials; baseline also includes a ViewComponent (`StatusBadge`).

**Scores:** 4, 3, 4, 4, 4.

**Verdict:** Good overall; managedcode slightly behind in component richness.

## 18. Data Seeder Design [MEDIUM]

```csharp
// Program.cs seeding pattern (all)
await DataSeeder.SeedAsync(db);
```

All generated apps include startup seeding with practical domain samples.

**Scores:** 4, 4, 4, 4, 4.

**Verdict:** Broad tie.

## 19. Structured Logging [MEDIUM]

```csharp
// managedcode EventService
logger.LogInformation("Created event {EventTitle} (ID: {EventId})", evt.Title, evt.Id);
```

All configs use `ILogger<T>` and structured templates to varying depth.

**Scores:** 3, 4, 3, 4, 4.

**Verdict:** Managed, no-skills, and webapi are strongest.

## 20. Nullable Reference Types [MEDIUM]

```xml
<Nullable>enable</Nullable>
```

Enabled in all examined `.csproj` files.

**Scores:** 5, 5, 5, 5, 5.

**Verdict:** Full tie at best score.

## 21. File Organization [MEDIUM]

```text
Pages/{Feature}/..., Services/, Models/, Data/
```

All follow clean Razor feature folders; `dotnet-webapi` shows strongest consistency across all three scenarios.

**Scores:** 4, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` best overall structure consistency.

## 22. Type Design & Resource Management [MEDIUM]

```csharp
// dotnet-webapi
public sealed class EventService(...)
modelBuilder.Entity<Event>().Property(e => e.Status).HasConversion<string>();
```

`dotnet-webapi` stands out with pervasive sealed types, enum string conversions, and strong resource-safe async usage.

**Scores:** 3, 3, 3, 3, 5.

**Verdict:** `dotnet-webapi` best.

## 23. Code Standards Compliance [LOW]

```csharp
namespace SparkEvents.Pages.Events;
public async Task<IActionResult> OnPostAsync() { ... }
```

Naming and file-scoped namespaces are generally good; consistency is a bit lower in managedcode/no-skills.

**Scores:** 4, 3, 4, 3, 4.

**Verdict:** `dotnet-artisan`, `dotnet-skills`, and `dotnet-webapi` are strongest.

## Weighted Summary

Weighting rules: **Critical ×3**, **High ×2**, **Medium ×1**, **Low ×0.5**.

| Configuration | Weighted Total |
|---|---:|
| dotnet-webapi | **154.0** |
| managedcode-dotnet-skills | **129.5** |
| dotnet-artisan | **129.0** |
| dotnet-skills | **129.0** |
| no-skills | **122.5** |

## What All Versions Get Right

- Use ASP.NET Core Razor Pages architecture (`AddRazorPages()` + `MapRazorPages()`).
- Enable nullable reference types in project config.
- Include EF Core + SQLite integration and startup seeding.
- Apply PRG flow (`RedirectToPage`) in many successful form handlers.
- Use DI service registration rather than direct manual wiring.

## Summary: Impact of Skills

Most impactful differences (highest practical impact):

1. **Scenario completeness**: only `dotnet-webapi` generated all three requested apps.
2. **CancellationToken propagation**: `dotnet-webapi` consistently threads cancellation through service/data layers.
3. **NuGet discipline**: managed and official dotnet skills pinned versions cleanly; `dotnet-webapi` used risky floating ranges.
4. **Security middleware**: uneven across outputs; artisan strongest among SparkEvents-only configs.
5. **EF migration anti-pattern**: all variants use `EnsureCreated`, which is unsuitable for production schema evolution.

Overall assessment: **`dotnet-webapi` produced the strongest functional breadth and modern engineering patterns**, but package versioning and migration strategy need correction. Among SparkEvents-only outputs, **managedcode-dotnet-skills**, **dotnet-artisan**, and **dotnet-skills** are tightly clustered, with baseline `no-skills` trailing slightly.
