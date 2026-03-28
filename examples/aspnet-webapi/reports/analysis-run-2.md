# Comparative Analysis: dotnet-artisan, dotnet-webapi, managedcode-dotnet-skills, dotnet-skills, no-skills

## Introduction

This report compares five Copilot skill configurations used to generate ASP.NET Core Web API applications. Each configuration was given the same three scenario prompts — **FitnessStudioApi** (booking/membership system), **LibraryApi** (book loans/reservations), and **VetClinicApi** (pet healthcare) — targeting .NET 10 with EF Core and SQLite.

| Configuration | Description | Apps Generated (run-2) |
|---|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain (9 skills + 14 specialist agents) | 2 of 3 (missing VetClinicApi) |
| **dotnet-webapi** | dotnet-webapi skill | 3 of 3 |
| **managedcode-dotnet-skills** | Community managed-code skills | 3 of 3 |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | 3 of 3 |
| **no-skills** | Baseline (default Copilot, no skills) | 3 of 3 |

All generated projects target `net10.0` with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`.

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| Build & Run Success [CRITICAL] | 3 | 4 | 4 | 4 | 4 |
| Security Vulnerability Scan [CRITICAL] | 3 | 3 | 2 | 2 | 2 |
| Minimal API Architecture [CRITICAL] | 5 | 5 | 1 | 2 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 4 | 4 | 4 | 4 | 3 |
| NuGet & Package Discipline [CRITICAL] | 3 | 2 | 2 | 2 | 2 |
| EF Migration Usage [CRITICAL] | 1 | 4 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 5 | 5 | 5 | 5 | 5 |
| Prefer Built-in over 3rd Party [HIGH] | 4 | 4 | 2 | 3 | 2 |
| Modern C# Adoption [HIGH] | 5 | 4 | 4 | 3 | 2 |
| Error Handling & Middleware [HIGH] | 5 | 5 | 5 | 3 | 3 |
| Async Patterns & Cancellation [HIGH] | 5 | 5 | 3 | 3 | 2 |
| EF Core Best Practices [HIGH] | 5 | 5 | 4 | 3 | 3 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 4 | 4 | 4 |
| Security Configuration [HIGH] | 1 | 1 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 5 | 5 | 4 | 3 | 2 |
| Sealed Types [MEDIUM] | 5 | 5 | 1 | 3 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 3 | 3 | 3 | 3 | 3 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 5 | 5 | 3 | 3 | 3 |
| File Organization [MEDIUM] | 5 | 5 | 4 | 3 | 4 |
| HTTP Test File Quality [MEDIUM] | 3 | 5 | 5 | 3 | 4 |
| Type Design & Resource Management [MEDIUM] | 5 | 5 | 4 | 3 | 4 |
| Code Standards Compliance [LOW] | 5 | 5 | 5 | 5 | 5 |

---

## 1. Build & Run Success [CRITICAL]

All configurations target **`net10.0`** with nullable reference types and implicit usings enabled.

```xml
<!-- Shared across all configurations -->
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

All include a `Directory.Build.props` with `Meziantou.Analyzer`, `AnalysisLevel=latest`, `AnalysisMode=Recommended`, and `EnforceCodeStyleInBuild=true`.

**Key difference:** `dotnet-artisan` failed to generate VetClinicApi in run-2, producing only 2 of 3 apps. All other configurations successfully generated all 3 apps.

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **3** | Missing VetClinicApi (only 2/3 apps generated) |
| dotnet-webapi | **4** | All 3 apps generated with proper project structure |
| managedcode | **4** | All 3 apps generated with proper project structure |
| dotnet-skills | **4** | All 3 apps generated with proper project structure |
| no-skills | **4** | All 3 apps generated with proper project structure |

**Verdict:** All configs except dotnet-artisan achieve full app generation. dotnet-artisan's missing VetClinicApi is a significant gap — a project that doesn't exist can't be used.

---

## 2. Security Vulnerability Scan [CRITICAL]

Wildcard NuGet versions and legacy Swashbuckle packages are the primary concerns across all configurations.

| Package Concern | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| `Version="*"` (worst case) | Meziantou only | Library EF Core ⚠️ | Meziantou only | Meziantou only | Meziantou only |
| `Version="10.*-*"` prerelease | Library EF | VetClinic | Library EF | Fitness EF | Fitness/Library EF |
| Swashbuckle present | Library (UI only) | Fitness (UI only) | All 3 apps ⚠️ | Library only | Fitness + VetClinic |
| Deprecated FluentValidation.AspNetCore | ❌ | ❌ | ❌ | Fitness ⚠️ | ❌ |

```xml
<!-- dotnet-webapi LibraryApi — worst-case wildcard versions -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="*" />
```

```xml
<!-- dotnet-artisan FitnessStudioApi — properly pinned -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.4" />
```

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **3** | Fitness has pinned versions; Library has wildcards for EF Core; Meziantou wildcard is dev-only |
| dotnet-webapi | **3** | Fitness pinned, but Library uses `Version="*"` and VetClinic uses prerelease wildcards |
| managedcode | **2** | Swashbuckle in all 3 apps plus wildcard versions |
| dotnet-skills | **2** | Wildcard versions, Swashbuckle in Library, deprecated FluentValidation.AspNetCore |
| no-skills | **2** | Swashbuckle in 2/3 apps, wildcard EF Core versions |

**Verdict:** No configuration fully avoids wildcard versions. dotnet-artisan's FitnessStudioApi is the only app with fully pinned versions. The `Version="*"` pattern in dotnet-webapi's LibraryApi is the worst case — it can pull any version including ones with known CVEs.

---

## 3. Minimal API Architecture [CRITICAL]

This is the **single largest differentiator** between configurations. Modern .NET guidance strongly recommends Minimal APIs with route groups for new projects.

```csharp
// dotnet-artisan — Minimal API with MapGroup + endpoint extension methods + TypedResults
// FitnessStudioApi/Endpoints/BookingEndpoints.cs
public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings").WithTags("Bookings");
        group.MapPost("/", async Task<Results<Created<BookingResponse>, Conflict<ProblemDetails>>> (
            CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        });
    }
}
```

```csharp
// dotnet-webapi — Same pattern: Minimal API + MapGroup + TypedResults + union returns
// LibraryApi/Endpoints/BookEndpoints.cs
group.MapGet("/{id:int}", async Task<Results<Ok<BookResponse>, NotFound>> (
    int id, IBookService service, CancellationToken ct) =>
{
    var book = await service.GetByIdAsync(id, ct);
    return book is null ? TypedResults.NotFound() : TypedResults.Ok(book);
})
```

```csharp
// managedcode / dotnet-skills / no-skills — Controllers
[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController(IBookingService service) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await service.GetByIdAsync(id);
        return booking is null ? NotFound() : Ok(booking);
    }
}
```

| Feature | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| Minimal APIs (MapGet/MapPost) | ✅ All | ✅ All | ❌ | ❌ (1 of 3) | ❌ |
| MapGroup() | ✅ | ✅ | ❌ | ❌ (1 of 3) | ❌ |
| Endpoint extension methods | ✅ | ✅ | ❌ | ❌ | ❌ |
| TypedResults | ✅ | ✅ | ❌ | ❌ | ❌ |
| Results\<T1, T2\> unions | ✅ | ✅ | ❌ | ❌ | ❌ |

`dotnet-skills` VetClinicApi uses Minimal APIs but inlines all endpoints in Program.cs (~220 lines) without extension methods — a half-measure.

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | Full Minimal API with MapGroup, extension methods, TypedResults, and union return types |
| dotnet-webapi | **5** | Same excellent pattern consistently across all 3 apps |
| managedcode | **1** | Controllers only — no Minimal API patterns |
| dotnet-skills | **2** | Controllers for 2/3 apps; VetClinic uses Minimal API but inline in Program.cs |
| no-skills | **1** | Controllers only — no Minimal API patterns |

**Verdict:** dotnet-artisan and dotnet-webapi are the clear winners. Their `Results<Created<T>, Conflict<ProblemDetails>>` union return types automatically generate correct OpenAPI schemas — a significant advantage over `Task<IActionResult>` which provides no compile-time type safety.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

All configurations use data annotations on DTOs. Some add FluentValidation. No configuration uses modern guard clauses (`ArgumentNullException.ThrowIfNull`, `ArgumentException.ThrowIfNullOrEmpty`).

```csharp
// dotnet-artisan — Data annotations on sealed record DTOs
public sealed record CreateMembershipPlanRequest
{
    [Required, MaxLength(100)] public required string Name { get; init; }
    [Range(1, 24)] public required int DurationMonths { get; init; }
    [Range(0.01, 999999.99)] public required decimal Price { get; init; }
}
```

```csharp
// dotnet-skills FitnessStudioApi — FluentValidation
public sealed class CreateMemberValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DateOfBirth).Must(d =>
            d <= DateOnly.FromDateTime(DateTime.Today.AddYears(-16)))
            .WithMessage("Member must be at least 16 years old.");
    }
}
```

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **4** | Comprehensive annotations on all DTOs; no guard clauses |
| dotnet-webapi | **4** | Highest annotation counts (39 [Required], 15 [Range] in Fitness alone); no guard clauses |
| managedcode | **4** | Annotations + FluentValidation in Library with 10 validator classes; no guard clauses |
| dotnet-skills | **4** | FluentValidation in Fitness with 12 validators; annotations elsewhere; no guard clauses |
| no-skills | **3** | Basic annotations present; no FluentValidation; no guard clauses |

**Verdict:** All are adequate. No configuration implements guard clauses on service constructors or method bodies, which is a universal gap. dotnet-webapi has the highest density of validation attributes.

---

## 5. NuGet & Package Discipline [CRITICAL]

The ideal is: exact version pinning, minimal packages, no 3rd-party where built-in exists.

| Config | Pinned Versions | Wildcards | Unnecessary Packages |
|---|---|---|---|
| dotnet-artisan | Fitness: ✅ all pinned | Library: `10.*-*` for EF; `*` Meziantou | Library: SwaggerUI |
| dotnet-webapi | Fitness: ✅ pinned | Library: `*` (worst case); VetClinic: `10.0.0-*` | Fitness: SwaggerUI |
| managedcode | Fitness/VetClinic: ✅ pinned | Library: `10.0.*-*`; `*` Meziantou | Swashbuckle in all 3 |
| dotnet-skills | Library: ✅ pinned | Fitness: `10.0.0-*`; VetClinic: `10.*-*`; `*` Meziantou | Library: Swashbuckle; Fitness: deprecated FluentValidation.AspNetCore |
| no-skills | VetClinic: ✅ pinned | Fitness: `10.0.0-*`; Library: `10.*-*`; `*` Meziantou | Fitness/VetClinic: Swashbuckle |

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **3** | Fitness fully pinned; Library has wildcards and SwaggerUI |
| dotnet-webapi | **2** | Library uses `Version="*"` — the worst possible pattern |
| managedcode | **2** | Swashbuckle everywhere adds unnecessary attack surface |
| dotnet-skills | **2** | Deprecated FluentValidation.AspNetCore + wildcards |
| no-skills | **2** | Swashbuckle in 2/3 apps + wildcard versions |

**Verdict:** No configuration is consistently excellent. dotnet-artisan's FitnessStudioApi with all `Version="10.0.4"` is the gold standard, but its LibraryApi backslides. All configs share the `Meziantou.Analyzer Version="*"` issue, though analyzer packages are dev-only.

---

## 6. EF Migration Usage [CRITICAL]

`EnsureCreated()` bypasses the migration pipeline, making schema evolution impossible. Only `dotnet-webapi` uses proper migrations in some apps.

```csharp
// dotnet-artisan / managedcode / dotnet-skills / no-skills — anti-pattern
await db.Database.EnsureCreatedAsync();
await DataSeeder.SeedAsync(db);
```

```csharp
// dotnet-webapi LibraryApi & VetClinicApi — correct approach
await dbContext.Database.MigrateAsync();
```

| Config | Fitness | Library | VetClinic | Score |
|---|:---:|:---:|:---:|:---:|
| dotnet-artisan | EnsureCreated | EnsureCreated | N/A | **1** |
| dotnet-webapi | EnsureCreated | Migrate ✅ | Migrate ✅ | **4** |
| managedcode | EnsureCreated | EnsureCreated | EnsureCreated | **1** |
| dotnet-skills | EnsureCreated | EnsureCreated | EnsureCreated | **1** |
| no-skills | EnsureCreated | EnsureCreated | EnsureCreated | **1** |

**Verdict:** dotnet-webapi is the only configuration that uses EF Core migrations (2 of 3 apps). This is a massive differentiator — all other configurations produce code that would cause data loss on schema changes.

---

## 7. Business Logic Correctness [HIGH]

All configurations implement rich, accurate business logic matching the scenario specifications. This is an area of near-universal strength.

```csharp
// dotnet-artisan — Waitlist promotion on cancellation
var nextWaitlisted = await db.Bookings
    .Where(b => b.ClassScheduleId == booking.ClassScheduleId &&
        b.Status == BookingStatus.Waitlisted)
    .OrderBy(b => b.WaitlistPosition)
    .FirstOrDefaultAsync(ct);
if (nextWaitlisted is not null) {
    nextWaitlisted.Status = BookingStatus.Confirmed;
    nextWaitlisted.WaitlistPosition = null;
}
```

```csharp
// no-skills — Appointment status state machine (VetClinicApi)
private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> ValidTransitions = new() {
    { AppointmentStatus.Scheduled, [AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow] },
    { AppointmentStatus.CheckedIn, [AppointmentStatus.InProgress, AppointmentStatus.Cancelled] },
    { AppointmentStatus.InProgress, [AppointmentStatus.Completed] },
};
```

| App | Endpoints (typical) | Key Rules Implemented |
|---|---|---|
| FitnessStudioApi | 30–41 | Booking window, capacity/waitlist, premium access, weekly limits, check-in window, membership freeze |
| LibraryApi | 32–40 | Loan periods by tier, fine thresholds, renewal limits, reservation queue promotion |
| VetClinicApi | 30–35 | Appointment conflict detection, status state machine, vaccination tracking |

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | Comprehensive rules in both apps; waitlist promotion, freeze/unfreeze logic |
| dotnet-webapi | **5** | Full coverage across all 3 apps with thorough edge-case handling |
| managedcode | **5** | Exceptionally deep rules including late cancellation, ISO week calculation |
| dotnet-skills | **5** | Rich rules; Fitness has 12 booking rules; Library has full loan lifecycle |
| no-skills | **5** | Strong logic including explicit state machine in VetClinic |

**Verdict:** Business logic is the great equalizer — all configurations produce thorough, correct implementations. This suggests the scenario prompts are detailed enough to drive correct behavior regardless of skills.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

.NET 9+ includes built-in OpenAPI support via `AddOpenApi()`/`MapOpenApi()`, making Swashbuckle unnecessary.

```csharp
// dotnet-artisan FitnessStudioApi — pure built-in
builder.Services.AddOpenApi();
app.MapOpenApi();
```

```csharp
// no-skills FitnessStudioApi — legacy Swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
app.UseSwagger();
app.UseSwaggerUI();
```

```csharp
// managedcode — redundant dual setup
builder.Services.AddOpenApi();   // Built-in
builder.Services.AddSwaggerGen(); // Also Swashbuckle?!
app.MapOpenApi();
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "..."));
```

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **4** | Built-in OpenAPI in Fitness; Library adds SwaggerUI (UI only, not doc gen) |
| dotnet-webapi | **4** | Built-in OpenAPI everywhere; Fitness adds SwaggerUI for convenience |
| managedcode | **2** | Swashbuckle in all 3 apps alongside built-in OpenAPI — redundant |
| dotnet-skills | **3** | Fitness/VetClinic use built-in; Library uses Swashbuckle |
| no-skills | **2** | Swashbuckle in 2/3 apps (Fitness, VetClinic); only Library uses built-in |

**Verdict:** dotnet-artisan and dotnet-webapi correctly use built-in OpenAPI as the primary approach. The SwaggerUI-only dependency is acceptable for dev convenience. managedcode's dual Swashbuckle + built-in setup is wasteful and confusing.

---

## 9. Modern C# Adoption [HIGH]

Modern C# features (primary constructors, collection expressions, records) reduce boilerplate and signal current language proficiency.

```csharp
// dotnet-artisan — Primary constructor + sealed class
public sealed class BookingService(AppDbContext db, ILogger<BookingService> logger) : IBookingService

// dotnet-artisan — Collection expressions in models
public ICollection<Booking> Bookings { get; set; } = [];

// dotnet-artisan — Pattern matching in FindAsync
var member = await db.Members.FindAsync([request.MemberId], ct);
```

```csharp
// no-skills — Traditional constructor + private fields
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    public BookingsController(IBookingService service) { _service = service; }
}

// no-skills — Old-style collection initialization
public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
```

| Feature | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| Primary constructors | ✅ All | ✅ All | ✅ All | ✅ Library only | ❌ |
| Collection expressions `[]` | ✅ | ✅ (2/3) | ✅ | ✅ | ❌ |
| Records for DTOs | ✅ | ✅ | ✅ | ❌ (classes) | ❌ (classes) |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ | ✅ |
| Pattern matching | ✅ | ✅ | ✅ | ✅ | ✅ (limited) |

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | Primary constructors, collection expressions, records, pattern matching everywhere |
| dotnet-webapi | **4** | Same as artisan except Fitness lacks collection expressions |
| managedcode | **4** | Primary constructors and collection expressions throughout |
| dotnet-skills | **3** | Inconsistent — Library uses primary constructors, Fitness/VetClinic don't |
| no-skills | **2** | File-scoped namespaces and switch expressions only; no primary constructors or collection expressions |

**Verdict:** dotnet-artisan leads with the most consistent adoption of modern C# features. no-skills generates code that feels like .NET 6-era patterns.

---

## 10. Error Handling & Middleware [HIGH]

`IExceptionHandler` (introduced in .NET 8) is the modern, composable, DI-aware approach. Traditional middleware still works but is less ergonomic.

```csharp
// dotnet-artisan / dotnet-webapi — Modern IExceptionHandler
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (0, (string?)null)
        };
        // ... writes ProblemDetails
    }
}
```

```csharp
// managedcode — IExceptionHandler with custom exception classes
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    var (statusCode, title) = exception switch {
        BusinessRuleException => (StatusCodes.Status400BadRequest, "Business Rule Violation"),
        ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };
}
```

```csharp
// no-skills — Traditional middleware
public class GlobalExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (KeyNotFoundException ex) { /* manual ProblemDetails response */ }
    }
}
```

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | IExceptionHandler in both apps; ProblemDetails; clean switch expressions |
| dotnet-webapi | **5** | IExceptionHandler in all 3 apps; consistent ProblemDetails |
| managedcode | **5** | IExceptionHandler in all 3 apps; custom exception classes for domain errors |
| dotnet-skills | **3** | Mixed: Fitness uses IExceptionHandler, Library/VetClinic use traditional middleware |
| no-skills | **3** | Traditional middleware only; ProblemDetails used but no IExceptionHandler |

**Verdict:** dotnet-artisan, dotnet-webapi, and managedcode all use the modern `IExceptionHandler` pattern. managedcode additionally defines custom domain exception classes (`BusinessRuleException`, `ConflictException`), which improves semantic clarity.

---

## 11. Async Patterns & Cancellation [HIGH]

Proper `CancellationToken` propagation prevents wasted server resources when clients disconnect.

```csharp
// dotnet-artisan / dotnet-webapi — Full CancellationToken chain
// Endpoint → Service → EF Core
group.MapPost("/", async (CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
{
    var booking = await service.CreateAsync(request, ct);
    return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
});

// Service layer passes ct to every EF call
var member = await db.Members.FindAsync([request.MemberId], ct);
await db.SaveChangesAsync(ct);
```

```csharp
// no-skills — No CancellationToken at all
public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
{
    var member = await _context.Members.FindAsync(dto.MemberId);
    await _context.SaveChangesAsync();  // No ct parameter
}
```

| Config | Async Suffix | CancellationToken | Sync-over-async | Score |
|---|:---:|:---:|:---:|:---:|
| dotnet-artisan | ✅ Consistent | ✅ Full chain | ✅ None | **5** |
| dotnet-webapi | ✅ Consistent | ✅ Full chain (84 occurrences in Fitness) | ✅ None | **5** |
| managedcode | ✅ Consistent | ⚠️ Fitness/VetClinic only; Library lacks it | ✅ None | **3** |
| dotnet-skills | ✅ Consistent | ⚠️ Partial; missing in most service methods | ✅ None | **3** |
| no-skills | ✅ Consistent | ❌ Not propagated in any app | ✅ None | **2** |

**Verdict:** dotnet-artisan and dotnet-webapi consistently propagate CancellationToken from endpoint handlers through services to EF Core calls. no-skills never passes CancellationToken at all.

---

## 12. EF Core Best Practices [HIGH]

Key practices: Fluent API configuration, `AsNoTracking()` for read queries, explicit cascade behaviors, and enum string conversions.

```csharp
// dotnet-artisan — AsNoTracking + HasConversion
var query = db.ClassSchedules.AsNoTracking()
    .Include(cs => cs.ClassType)
    .Include(cs => cs.Instructor)
    .Where(cs => cs.Status != ClassScheduleStatus.Cancelled);

// OnModelCreating
e.Property(m => m.Status).HasConversion<string>();
e.HasOne(m => m.Member).WithMany(m => m.Memberships)
    .HasForeignKey(m => m.MemberId).OnDelete(DeleteBehavior.Restrict);
```

| Feature | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| Fluent API in OnModelCreating | ✅ Extensive | ✅ Extensive | ✅ Extensive | ✅ Extensive | ✅ Extensive |
| AsNoTracking for reads | ✅ 38+ usages | ✅ 46+ usages | ✅ 15 usages | ⚠️ 37 usages (missing in VetClinic) | ❌ None |
| HasConversion\<string\>() | ✅ All enums | ✅ All enums | ⚠️ Partial (Library lacks it) | ⚠️ Partial | ⚠️ Partial |
| OnDelete(Restrict) | ✅ | ✅ | ✅ | ✅ | ✅ |
| SaveChanges timestamp override | ✅ | ✅ | ✅ (VetClinic) | ✅ (Fitness/VetClinic) | ❌ |

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | AsNoTracking everywhere, all enums as strings, explicit cascades |
| dotnet-webapi | **5** | Most AsNoTracking usages (46+), all enums as strings |
| managedcode | **4** | Good Fluent API; moderate AsNoTracking; Library enums stored as int |
| dotnet-skills | **3** | AsNoTracking in Fitness/Library but missing in VetClinic; partial enum conversion |
| no-skills | **3** | Good Fluent API and cascades but zero AsNoTracking usage |

**Verdict:** dotnet-artisan and dotnet-webapi consistently apply `AsNoTracking()` — doubling read query performance. no-skills never uses it, meaning every read query tracks entities unnecessarily.

---

## 13. Service Abstraction & DI [HIGH]

All configurations use interface-based services with `AddScoped<IService, Service>()` and single responsibility per domain entity.

```csharp
// Universal pattern across all configurations
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
```

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | 7 interfaces + implementations per app; primary constructor injection; clean separation |
| dotnet-webapi | **5** | Same pattern; 7 services per app; DI-aware throughout |
| managedcode | **4** | Interface-based; Library puts all interfaces in one file (less discoverable) |
| dotnet-skills | **4** | Interface-based; good SRP; some interfaces in single files |
| no-skills | **4** | Interface-based but uses traditional constructors with private fields |

**Verdict:** All configurations implement proper DI patterns. The differences are minor — mainly around constructor style (primary vs traditional) and interface file organization.

---

## 14. Security Configuration [HIGH]

No configuration implements HSTS, HTTPS redirection, or CORS.

| Feature | All 5 Configurations |
|---|:---:|
| `UseHsts()` | ❌ |
| `UseHttpsRedirection()` | ❌ |
| CORS | ❌ |
| Authentication | ❌ (per spec) |

| Config | Score | Justification |
|---|:---:|---|
| All configs | **1** | No security middleware configured; all listen HTTP-only |

**Verdict:** This is a universal gap. While the spec says "no authentication required," HSTS and HTTPS redirection are baseline security for any web API and should be present by default with an environment check.

---

## 15. DTO Design [MEDIUM]

The ideal: sealed records with `init` properties, Request/Response naming, separation from entities.

```csharp
// dotnet-artisan — Sealed record with positional syntax (response)
public sealed record BookingResponse(int Id, int ClassScheduleId, string ClassName, ...);

// dotnet-artisan — Sealed record with property syntax (request, for annotations)
public sealed record CreateMemberRequest {
    [Required, MaxLength(100)] public required string FirstName { get; init; }
    [Required, EmailAddress] public required string Email { get; init; }
}
```

```csharp
// no-skills — Mutable class DTOs
public class CreateBookingDto {
    [Required] public int ClassScheduleId { get; set; }
    [Required] public int MemberId { get; set; }
}
```

| Feature | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| Records | ✅ All | ✅ All | ✅ All | ❌ Classes | ❌ Classes |
| Sealed | ✅ All | ✅ All | ❌ | ⚠️ Fitness only | ❌ |
| Immutable (init) | ✅ | ✅ | ✅ | ⚠️ Mixed | ❌ (mutable set) |
| Naming | Request/Response | Request/Response | Mixed (Dto + Request/Response) | Dto | Dto |

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | Sealed records, immutable, consistent Request/Response naming |
| dotnet-webapi | **5** | Sealed records, immutable, consistent Request/Response naming |
| managedcode | **4** | Records (immutable) but not sealed |
| dotnet-skills | **3** | Classes not records; Fitness sealed, others not; mixed immutability |
| no-skills | **2** | Mutable classes, not sealed, inconsistent naming |

**Verdict:** dotnet-artisan and dotnet-webapi produce ideal DTO design. Using `sealed record` with `init` properties creates immutable, value-semantic API contracts that are safer and more expressive.

---

## 16. Sealed Types [MEDIUM]

Sealed types enable JIT devirtualization and signal design intent. Classes not designed for inheritance should be sealed.

| Config | Sealed Classes/Records | Total Types | Ratio | Score |
|---|:---:|:---:|:---:|:---:|
| dotnet-artisan | ~76 | ~76 | **100%** | **5** |
| dotnet-webapi | ~128 | ~128 | **100%** | **5** |
| managedcode | 0 | ~90 | **0%** | **1** |
| dotnet-skills | ~43 | ~128 | **~34%** (Fitness 97%, Library 14%, VetClinic 0%) | **3** |
| no-skills | 0 | ~90 | **0%** | **1** |

```csharp
// dotnet-artisan / dotnet-webapi — Everything sealed
public sealed class BookingService(AppDbContext db, ...) : IBookingService
public sealed class Member { ... }
internal sealed class ApiExceptionHandler : IExceptionHandler

// managedcode / no-skills — Nothing sealed
public class BookingService : IBookingService
public class Member { ... }
public class GlobalExceptionHandler : IExceptionHandler
```

**Verdict:** dotnet-artisan and dotnet-webapi achieve 100% sealed types — models, services, DTOs, middleware, DbContext, everything. managedcode and no-skills seal nothing.

---

## 17. Data Seeder Design [MEDIUM]

All configurations use runtime seeders with idempotency guards. One notable exception: dotnet-webapi's VetClinicApi uses `HasData()` in `OnModelCreating`, which integrates with migrations.

```csharp
// Common pattern (all configs)
public static async Task SeedAsync(AppDbContext db)
{
    if (await db.MembershipPlans.AnyAsync()) return; // idempotent
    // ... realistic seed data
}
```

All include realistic, varied seed data (multiple membership tiers, edge cases like full classes, expired memberships, overdue items).

| Config | Score | Justification |
|---|:---:|---|
| All configs | **4** | Runtime seeders, idempotent, realistic data; none use HasData() extensively |

**Verdict:** Seed data quality is consistently good across all configurations. All produce realistic named entities with varied statuses and edge cases.

---

## 18. Structured Logging [MEDIUM]

All configurations inject `ILogger<T>` and use structured message templates. None use `[LoggerMessage]` source generators.

```csharp
// dotnet-artisan / dotnet-webapi — Structured with semantic placeholders
logger.LogInformation("Booking created for member {MemberId} in class {ClassId}, status: {Status}",
    request.MemberId, request.ClassScheduleId, booking.Status);
```

```csharp
// no-skills — Same structured pattern
_logger.LogInformation("Book {BookId} checked out to patron {PatronId}. Loan {LoanId} created. Due: {DueDate}",
    book.Id, dto.PatronId, loan.Id, loan.DueDate);
```

| Config | Score | Justification |
|---|:---:|---|
| All configs | **3** | ILogger<T> with structured templates; no [LoggerMessage] source generators |

**Verdict:** All produce good structured logging. The universal absence of `[LoggerMessage]` source generators (which eliminate allocations for disabled log levels) is a shared gap.

---

## 19. Nullable Reference Types [MEDIUM]

All configurations enable NRTs via `<Nullable>enable</Nullable>` and use `?` annotations appropriately.

| Config | Score | Justification |
|---|:---:|---|
| All configs | **4** | Enabled; proper annotations; `null!` for navigation properties |

---

## 20. API Documentation [MEDIUM]

Minimal API configurations can use `.WithName()`, `.WithSummary()`, `.WithDescription()`, and `.Produces<T>()`. Controller configurations use `[ProducesResponseType]` and XML comments.

```csharp
// dotnet-webapi — Rich metadata on every endpoint
group.MapPost("/", handler)
    .WithName("CreateBooking")
    .WithSummary("Book a class")
    .WithDescription("Creates a booking enforcing all business rules...")
    .Produces<BookingResponse>(StatusCodes.Status201Created)
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status409Conflict);
```

```csharp
// managedcode / no-skills — Controller attributes
[HttpPost]
[ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
```

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | WithName + WithSummary + WithTags + Produces on all endpoints |
| dotnet-webapi | **5** | Most thorough: WithName + WithSummary + WithDescription + Produces on every endpoint |
| managedcode | **3** | ProducesResponseType + XML comments; adequate but less rich |
| dotnet-skills | **3** | ProducesResponseType; VetClinic adds WithSummary/WithTags (Minimal API) |
| no-skills | **3** | ProducesResponseType + some XML comments |

**Verdict:** dotnet-webapi produces the most comprehensive API documentation with `WithDescription` on every endpoint. The Minimal API metadata approach is inherently more expressive than controller attributes.

---

## 21. File Organization [MEDIUM]

The ideal: per-concern folders with a dedicated `Endpoints/` directory for Minimal API route groups.

```
// dotnet-artisan / dotnet-webapi structure
├── Data/          (DbContext, seeder)
├── DTOs/          (Request/Response records)
├── Endpoints/     (One file per entity group) ← dedicated
├── Middleware/     (ApiExceptionHandler)
├── Models/        (Entity classes, enums)
├── Services/      (Interface + implementation)
```

```
// managedcode / dotnet-skills / no-skills structure
├── Controllers/   (One controller per entity)
├── Data/          (DbContext, seeder)
├── DTOs/          (DTO classes)
├── Middleware/     (Exception handler)
├── Models/        (Entity classes, enums)
├── Services/      (Interface + implementation)
```

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | Dedicated Endpoints/ directory; clean Program.cs; DTOs split per entity in Library |
| dotnet-webapi | **5** | Dedicated Endpoints/ directory in all 3 apps; Library/VetClinic have Migrations/ |
| managedcode | **4** | Clean per-concern with Controllers/; consistent across apps |
| dotnet-skills | **3** | Mostly good; VetClinic inlines all endpoints in Program.cs (~220 lines) |
| no-skills | **4** | Consistent per-concern layout with Controllers/ |

**Verdict:** dotnet-artisan and dotnet-webapi's `Endpoints/` directory pattern with static extension methods is cleaner than monolithic controllers and keeps Program.cs focused on configuration.

---

## 22. HTTP Test File Quality [MEDIUM]

High-quality `.http` files serve as executable API documentation and catch routing issues during development.

```http
# dotnet-webapi — Business rule edge-case testing
### Book a class — test premium restriction (Basic member booking Boxing)
### This should fail for member 1 (Basic plan) trying to book class 3 (Boxing = premium)
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{ "classScheduleId": 3, "memberId": 1 }
```

```http
# dotnet-skills VetClinicApi — Scaffold only (6 lines!)
@VetClinicApi_HostAddress = http://localhost:5101
GET {{VetClinicApi_HostAddress}}/weatherforecast/
```

| Config | Fitness | Library | VetClinic | Score |
|---|:---:|:---:|:---:|:---:|
| dotnet-artisan | ✅ Excellent (business rule tests) | ❌ Missing | N/A | **3** |
| dotnet-webapi | ✅ Excellent | ✅ Excellent | ✅ Good | **5** |
| managedcode | ✅ Excellent (344 lines) | ✅ Excellent (326 lines) | ✅ Excellent (349 lines) | **5** |
| dotnet-skills | ✅ Excellent (361 lines) | ✅ Excellent (326 lines) | ❌ Scaffold only (6 lines) | **3** |
| no-skills | ✅ Good | ✅ Good | ✅ Good | **4** |

**Verdict:** dotnet-webapi and managedcode produce the most consistently high-quality `.http` files across all apps, including business rule edge-case tests. dotnet-skills' VetClinic `.http` file is essentially useless (only a weatherforecast GET).

---

## 23. Type Design & Resource Management [MEDIUM]

Proper enum design, string conversion for EF Core storage, and JSON serialization configuration.

```csharp
// dotnet-artisan — Complete enum pipeline
// Model
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }

// EF Core storage as string
e.Property(m => m.Status).HasConversion<string>();

// JSON serialization as string
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

| Feature | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| Enums for status fields | ✅ All | ✅ All | ✅ All | ✅ All | ✅ All |
| HasConversion\<string\>() | ✅ All enums | ✅ All enums | ⚠️ Partial (Library as int) | ⚠️ Partial | ⚠️ Partial (Library as int) |
| JsonStringEnumConverter | ✅ | ✅ | ✅ | ✅ | ✅ |

| Config | Score | Justification |
|---|:---:|---|
| dotnet-artisan | **5** | Complete enum pipeline: model → EF string storage → JSON string serialization |
| dotnet-webapi | **5** | Same complete pipeline across all 3 apps |
| managedcode | **4** | Good enums; Library stores as integers (not ideal for debugging) |
| dotnet-skills | **3** | Enums present but inconsistent string conversion |
| no-skills | **4** | Good enums; Library lacks string conversion |

**Verdict:** dotnet-artisan and dotnet-webapi implement the complete enum pipeline. Storing enums as strings in the database makes debugging and ad-hoc queries dramatically easier.

---

## 24. Code Standards Compliance [LOW]

All configurations follow .NET naming conventions: PascalCase for public members, camelCase for parameters, Async suffix on async methods, file-scoped namespaces, and explicit access modifiers.

| Config | Score | Justification |
|---|:---:|---|
| All configs | **5** | Consistent PascalCase, Async suffix, explicit access modifiers, file-scoped namespaces |

**Verdict:** Code standards compliance is universally good. All configurations produce code that follows .NET conventions.

---

## Weighted Summary

Weights: Critical ×3, High ×2, Medium ×1, Low ×0.5.

### Critical Dimensions (×3)

| Dimension | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| Build & Run Success | 3 | 4 | 4 | 4 | 4 |
| Security Vulnerability Scan | 3 | 3 | 2 | 2 | 2 |
| Minimal API Architecture | 5 | 5 | 1 | 2 | 1 |
| Input Validation & Guard Clauses | 4 | 4 | 4 | 4 | 3 |
| NuGet & Package Discipline | 3 | 2 | 2 | 2 | 2 |
| EF Migration Usage | 1 | 4 | 1 | 1 | 1 |
| **Subtotal (raw)** | **19** | **22** | **14** | **15** | **13** |
| **Subtotal (×3)** | **57** | **66** | **42** | **45** | **39** |

### High Dimensions (×2)

| Dimension | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| Business Logic Correctness | 5 | 5 | 5 | 5 | 5 |
| Prefer Built-in over 3rd Party | 4 | 4 | 2 | 3 | 2 |
| Modern C# Adoption | 5 | 4 | 4 | 3 | 2 |
| Error Handling & Middleware | 5 | 5 | 5 | 3 | 3 |
| Async Patterns & Cancellation | 5 | 5 | 3 | 3 | 2 |
| EF Core Best Practices | 5 | 5 | 4 | 3 | 3 |
| Service Abstraction & DI | 5 | 5 | 4 | 4 | 4 |
| Security Configuration | 1 | 1 | 1 | 1 | 1 |
| **Subtotal (raw)** | **35** | **34** | **28** | **25** | **22** |
| **Subtotal (×2)** | **70** | **68** | **56** | **50** | **44** |

### Medium Dimensions (×1)

| Dimension | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| DTO Design | 5 | 5 | 4 | 3 | 2 |
| Sealed Types | 5 | 5 | 1 | 3 | 1 |
| Data Seeder Design | 4 | 4 | 4 | 4 | 4 |
| Structured Logging | 3 | 3 | 3 | 3 | 3 |
| Nullable Reference Types | 4 | 4 | 4 | 4 | 4 |
| API Documentation | 5 | 5 | 3 | 3 | 3 |
| File Organization | 5 | 5 | 4 | 3 | 4 |
| HTTP Test File Quality | 3 | 5 | 5 | 3 | 4 |
| Type Design & Resource Management | 5 | 5 | 4 | 3 | 4 |
| **Subtotal (×1)** | **39** | **41** | **32** | **29** | **29** |

### Low Dimensions (×0.5)

| Dimension | dotnet-artisan | dotnet-webapi | managedcode | dotnet-skills | no-skills |
|---|:---:|:---:|:---:|:---:|:---:|
| Code Standards Compliance | 5 | 5 | 5 | 5 | 5 |
| **Subtotal (×0.5)** | **2.5** | **2.5** | **2.5** | **2.5** | **2.5** |

### Final Weighted Scores

| Configuration | Critical (×3) | High (×2) | Medium (×1) | Low (×0.5) | **Total** |
|---|:---:|:---:|:---:|:---:|:---:|
| **dotnet-webapi** | 66 | 68 | 41 | 2.5 | **177.5** |
| **dotnet-artisan** | 57 | 70 | 39 | 2.5 | **168.5** |
| **managedcode-dotnet-skills** | 42 | 56 | 32 | 2.5 | **132.5** |
| **dotnet-skills** | 45 | 50 | 29 | 2.5 | **126.5** |
| **no-skills** | 39 | 44 | 29 | 2.5 | **114.5** |

Maximum possible score: **232.5** (all 5s across every dimension).

---

## What All Versions Get Right

Despite significant differences in architecture and code quality, all five configurations share these strengths:

- **Rich business logic** — All implement complex domain rules (waitlist promotion, fine calculation, state machines) accurately and thoroughly
- **.NET 10 targeting** — All use `net10.0` with nullable reference types and implicit usings enabled
- **Interface-based DI** — All register services via `AddScoped<IService, Service>()` with single-responsibility interfaces
- **Structured logging** — All inject `ILogger<T>` and use structured message templates with named placeholders (`{Id}`, `{Status}`)
- **Enums for status fields** — No configuration uses magic strings; all define proper enum types
- **ProblemDetails error responses** — All return RFC 7807 ProblemDetails on errors (400, 404, 409, 500)
- **Data seeding** — All include realistic, idempotent seed data with varied entity states
- **.NET naming conventions** — All follow PascalCase, Async suffix, file-scoped namespaces, and explicit access modifiers
- **No Newtonsoft.Json** — All use the built-in `System.Text.Json` serializer
- **No AutoMapper** — All use manual mapping, avoiding the performance overhead and magic of reflection-based mapping

---

## Summary: Impact of Skills

### Ranking by Weighted Score

| Rank | Configuration | Score | % of Max |
|:---:|---|:---:|:---:|
| 🥇 | **dotnet-webapi** | **177.5** | 76.3% |
| 🥈 | **dotnet-artisan** | **168.5** | 72.5% |
| 🥉 | **managedcode-dotnet-skills** | **132.5** | 57.0% |
| 4 | **dotnet-skills** | **126.5** | 54.4% |
| 5 | **no-skills** | **114.5** | 49.2% |

### Most Impactful Differences

1. **Minimal API Architecture** (CRITICAL, ×3 weight) — The single largest scoring differentiator. dotnet-artisan and dotnet-webapi produce modern Minimal APIs with route groups, TypedResults, and union return types. The other three generate legacy controllers. This 4-point gap (5 vs 1) ×3 weight = 12 points of separation.

2. **EF Migration Usage** (CRITICAL, ×3 weight) — dotnet-webapi is the only configuration that uses `Database.Migrate()` instead of `EnsureCreated()`. This 3-point gap (4 vs 1) ×3 weight = 9 points of separation.

3. **Async Patterns & CancellationToken** (HIGH, ×2 weight) — dotnet-artisan and dotnet-webapi propagate CancellationToken from endpoints through services to EF Core. no-skills never passes CancellationToken at all. This 3-point gap ×2 weight = 6 points.

4. **Sealed Types** (MEDIUM, ×1 weight) — dotnet-artisan and dotnet-webapi seal 100% of their types. managedcode and no-skills seal 0%. A 4-point gap that affects JIT performance and design clarity.

5. **Modern C# Features** (HIGH, ×2 weight) — Primary constructors, collection expressions, and record DTOs separate skill-guided from baseline configurations. A 3-point gap ×2 = 6 points.

### Overall Assessment

**dotnet-webapi (177.5)** delivers the best overall quality by combining Minimal API architecture with the only use of EF Core migrations, full CancellationToken propagation, 100% sealed types, and the most comprehensive API documentation metadata. Its main weakness is inconsistent package version pinning across apps.

**dotnet-artisan (168.5)** produces the highest *per-file* code quality — primary constructors, sealed records, full async chains, rich OpenAPI metadata — but is penalized for generating only 2 of 3 apps and universally using `EnsureCreated()`. If it had generated VetClinicApi and used migrations, it would likely lead.

**managedcode-dotnet-skills (132.5)** generates well-structured controller-based apps with strong business logic and excellent `.http` files, but misses modern patterns: no Minimal APIs, no sealed types, redundant Swashbuckle, and inconsistent CancellationToken propagation.

**dotnet-skills (126.5)** is inconsistent across apps — FitnessStudioApi uses FluentValidation and sealed types while VetClinicApi uses Minimal APIs but inlines everything in Program.cs. This inconsistency suggests the skill collection lacks a unified architectural vision.

**no-skills (114.5)** establishes the baseline: correct business logic with adequate structure, but no modern patterns. It consistently uses controllers, traditional constructors, mutable class DTOs, no CancellationToken, no AsNoTracking, no sealed types, and Swashbuckle instead of built-in OpenAPI. It is functional but dated.

**Key insight:** Skills have the most impact on *architectural decisions* (Minimal API vs Controllers, IExceptionHandler vs middleware, EF Migrations vs EnsureCreated) and *modern pattern adoption* (primary constructors, sealed types, TypedResults). They have minimal impact on *functional correctness* — business logic quality is consistently strong across all configurations when prompts are sufficiently detailed.
