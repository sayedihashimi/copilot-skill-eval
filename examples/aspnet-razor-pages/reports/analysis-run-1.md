# Comparative Analysis: dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, no-skills, dotnet-webapi

This analysis evaluates **5 configurations** from `output/{config}/run-1/`. In this run, every configuration contains only `SparkEvents` under `run-1`; `KeystoneProperties` and `HorizonHR` are missing for all configs. Configuration identity was taken from `gen-notes.md` when present, otherwise from directory names.

## Executive Summary

| Dimension [Tier] | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills | no-skills | dotnet-webapi |
|---|---:|---:|---:|---:|---:|
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Input Validation & Guard Clauses [CRITICAL] | 4 | 4 | 3 | 3 | 4 |
| NuGet & Package Discipline [CRITICAL] | 5 | 5 | 2 | 5 | 2 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 4 | 4 | 3 | 3 | 4 |
| Prefer Built-in over 3rd Party [HIGH] | 5 | 5 | 5 | 5 | 5 |
| Modern C# Adoption [HIGH] | 4 | 4 | 3 | 3 | 5 |
| Page Model Design [HIGH] | 4 | 4 | 2 | 2 | 4 |
| Form Handling & Validation [HIGH] | 4 | 4 | 3 | 3 | 4 |
| Error Handling Strategy [HIGH] | 3 | 4 | 3 | 3 | 4 |
| Async Patterns & Cancellation [HIGH] | 3 | 3 | 2 | 2 | 5 |
| EF Core Best Practices [HIGH] | 4 | 5 | 3 | 3 | 4 |
| Security Configuration [HIGH] | 2 | 3 | 2 | 2 | 2 |
| Service Abstraction & DI [MEDIUM] | 4 | 4 | 3 | 3 | 5 |
| UI Quality & Accessibility [MEDIUM] | 3 | 4 | 3 | 3 | 3 |
| Reusable UI Components [MEDIUM] | 2 | 4 | 2 | 2 | 2 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 3 | 3 | 4 |
| Structured Logging [MEDIUM] | 4 | 4 | 3 | 3 | 4 |
| Nullable Reference Types [MEDIUM] | 5 | 5 | 5 | 5 | 5 |
| File Organization [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Type Design & Resource Management [MEDIUM] | 4 | 4 | 3 | 3 | 4 |
| Code Standards Compliance [LOW] | 4 | 4 | 3 | 3 | 4 |
| Scenario Coverage (3 apps expected) [CRITICAL] | 1 | 1 | 1 | 1 | 1 |

## 1. Build & Run Success [CRITICAL]
All five `SparkEvents` projects compile and start (`BuildSucceeded=true`, `WarningCount=0`, `RunAlive8s=true`).

```csharp
// dotnet-artisan: Program.cs
await app.RunAsync();

// dotnet-skills/no-skills/dotnet-webapi: Program.cs
app.Run();
```

Scores: artisan **5**, managedcode **5**, dotnet-skills **5**, no-skills **5**, webapi **5**.

Verdict: Tie; all are runnable at baseline.

## 2. Security Vulnerability Scan [CRITICAL]
`dotnet list package --vulnerable` returned no vulnerable packages across all five.

```text
# measured for each config under run-1/SparkEvents/src/SparkEvents
HasVulnerabilities=false
```

Scores: all **5**.

Verdict: Tie.

## 3. Input Validation & Guard Clauses [CRITICAL]
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

## 4. NuGet & Package Discipline [CRITICAL]
Pinned versions in artisan/managedcode/no-skills; floating versions in dotnet-skills and webapi.

```xml
<!-- dotnet-artisan: SparkEvents.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />

<!-- dotnet-webapi: SparkEvents.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

Scores: **5, 5, 2, 5, 2**.

Verdict: exact pinning is the clear winner.

## 5. EF Migration Usage [CRITICAL]
All projects use `EnsureCreated` and do not use migrations startup.

```csharp
// all Program.cs variants
await context.Database.EnsureCreatedAsync(); // or EnsureCreated()
```

Scores: all **1**.

Verdict: shared production-readiness gap.

## 6. Business Logic Correctness [HIGH]
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

## 7. Prefer Built-in over 3rd Party [HIGH]
All use built-in ASP.NET Core + EF Core only (no Swashbuckle/Newtonsoft/Autofac/Serilog).

```xml
<!-- representative across configs -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" ... />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" ... />
```

Scores: all **5**.

Verdict: excellent shared dependency discipline.

## 8. Modern C# Adoption [HIGH]
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

## 9. Page Model Design [HIGH]
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

## 10. Form Handling & Validation [HIGH]
All include validation helpers and scripts partial; PRG pattern is generally used.

```html
<!-- representative across configs -->
<div asp-validation-summary="ModelOnly"></div>
<span asp-validation-for="Input.AttendeeId"></span>
<partial name="_ValidationScriptsPartial" />
```

Scores: **4, 4, 3, 3, 4**.

Verdict: strong baseline; webapi/artisan/managedcode are most consistent.

## 11. Error Handling Strategy [HIGH]
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

## 12. Async Patterns & Cancellation [HIGH]
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

## 13. EF Core Best Practices [HIGH]
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

## 14. Security Configuration [HIGH]
No config enables `UseHttpsRedirection`; only managedcode includes `UseHsts`.

```csharp
// managedcode Program.cs
app.UseHsts();
```

Scores: **2, 3, 2, 2, 2**.

Verdict: important shared gap despite otherwise solid app quality.

## 15. Service Abstraction & DI [MEDIUM]
webapi has broadest service surface; others are narrower.

```csharp
// dotnet-webapi Program.cs
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IAttendeeService, AttendeeService>();
```

Scores: **4, 4, 3, 3, 5**.

Verdict: webapi strongest architecture layering.

## 16. UI Quality & Accessibility [MEDIUM]
All use Bootstrap + tag helpers; managedcode stands out with reusable status badge component.

```csharp
// managedcode: Pages/Shared/Components/StatusBadge/StatusBadgeViewComponent.cs
public IViewComponentResult Invoke(string statusText, string? displayText = null)
```

Scores: **3, 4, 3, 3, 3**.

Verdict: managedcode best UX reuse signal.

## 17. Reusable UI Components [MEDIUM]
Only managedcode includes explicit ViewComponent composition.

```csharp
// managedcode
public class StatusBadgeViewComponent : ViewComponent
```

Scores: **2, 4, 2, 2, 2**.

Verdict: managedcode clearly leads DRY Razor composition.

## 18. Data Seeder Design [MEDIUM]
artisan/managedcode/webapi use async seeding; dotnet-skills uses synchronous seeding.

```csharp
// dotnet-artisan Program.cs
await DataSeeder.SeedAsync(context);

// dotnet-skills DataSeeder.cs
public static void Seed(SparkEventsDbContext db)
```

Scores: **4, 4, 3, 3, 4**.

Verdict: async seeding patterns are preferable.

## 19. Structured Logging [MEDIUM]
Structured `ILogger<T>` templates are present in service layers.

```csharp
// dotnet-artisan: Services/RegistrationService.cs
logger.LogInformation("Registration created: {ConfirmationNumber} for event {EventId}, status: {Status}",
    confirmationNumber, eventId, registration.Status);
```

Scores: **4, 4, 3, 3, 4**.

Verdict: good baseline across variants.

## 20. Nullable Reference Types [MEDIUM]
All enable NRT.

```xml
<!-- all SparkEvents.csproj -->
<Nullable>enable</Nullable>
```

Scores: all **5**.

Verdict: tie.

## 21. File Organization [MEDIUM]
All follow clean Razor layout by concern.

```text
src/SparkEvents/{Data,Models,Pages,Services,Program.cs}
```

Scores: all **4**.

Verdict: tie.

## 22. Type Design & Resource Management [MEDIUM]
webapi/artisan/managedcode have stronger modern type style; no manual DbContext disposal anti-pattern detected.

```csharp
// dotnet-webapi
public sealed class EventService(...)
```

Scores: **4, 4, 3, 3, 4**.

Verdict: modern sealed/service patterns improve consistency.

## 23. Code Standards Compliance [LOW]
Naming and async suffix usage are generally aligned; webapi/artisan/managedcode slightly tighter.

```csharp
// representative
public async Task<IActionResult> OnPostAsync(...)
```

Scores: **4, 4, 3, 3, 4**.

Verdict: no severe standards issues.

## 24. Scenario Coverage (3 apps expected) [CRITICAL]
Every config’s `run-1` includes only `SparkEvents`; no `KeystoneProperties` or `HorizonHR`.

```text
# output/{config}/run-1
Run1Apps: SparkEvents
HasKeystone=false
HasHorizon=false
```

Scores: all **1**.

Verdict: this is the biggest shared completeness failure.

## Weighted Summary
Weights applied: Critical×3, High×2, Medium×1, Low×0.5

| Configuration | Weighted Total |
|---|---:|
| managedcode-dotnet-skills | **170.0** |
| dotnet-artisan | **161.0** |
| dotnet-webapi | **161.0** |
| no-skills | **139.5** |
| dotnet-skills | **130.5** |

## What All Versions Get Right
- Build and run baseline is stable for generated `SparkEvents`.
- Vulnerability scan is clean for current package graph.
- Built-in .NET stack is preferred over third-party framework sprawl.
- Razor validation plumbing (`asp-validation-*` and `_ValidationScriptsPartial`) exists.
- Nullable reference types and project organization are consistently good.

## Summary: Impact of Skills
Most impactful differences:
- **Package version pinning**: managedcode/artisan/no-skills outperform dotnet-skills/webapi on reproducibility.
- **Cancellation correctness**: webapi is clearly strongest.
- **EF operational quality**: managedcode leads with timestamp update hooks.
- **Razor component reuse**: only managedcode demonstrates ViewComponent composition.
- **Security middleware**: managedcode is slightly better via HSTS, but all miss HTTPS redirection.

Overall assessment (weighted): **managedcode-dotnet-skills** ranks first; **dotnet-artisan** and **dotnet-webapi** tie next with different strengths; **no-skills** and especially **dotnet-skills** trail. The universal blocker is missing 2 of 3 expected scenario apps in `run-1`.
