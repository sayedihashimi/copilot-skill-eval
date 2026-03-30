# Comparative Analysis: no-skills, dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, dotnet-webapi

This analysis compares **5 expected Copilot configurations** under `output/{config}/run-3`. In the actual filesystem, only `no-skills`, `dotnet-artisan`, `managedcode-dotnet-skills`, and `dotnet-skills` exist, and each contains only `VetClinicApi` (the expected `FitnessStudioApi` and `LibraryApi` are missing). `dotnet-webapi` is not present at all, so it is scored as missing baseline (1) across dimensions.

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills | dotnet-webapi |
|---|---:|---:|---:|---:|---:|
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 | 1 |
| Security Vulnerability Scan [CRITICAL] | 5 | 5 | 5 | 5 | 1 |
| Minimal API Architecture [CRITICAL] | 1 | 5 | 1 | 1 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 4 | 4 | 4 | 1 |
| NuGet & Package Discipline [CRITICAL] | 1 | 4 | 2 | 4 | 1 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 2 | 2 | 2 | 2 | 1 |
| Prefer Built-in over 3rd Party [HIGH] | 2 | 3 | 2 | 2 | 1 |
| Modern C# Adoption [HIGH] | 3 | 5 | 4 | 3 | 1 |
| Error Handling & Middleware [HIGH] | 3 | 4 | 4 | 4 | 1 |
| Async Patterns & Cancellation [HIGH] | 2 | 3 | 3 | 3 | 1 |
| EF Core Best Practices [HIGH] | 2 | 5 | 4 | 4 | 1 |
| Service Abstraction & DI [HIGH] | 4 | 5 | 4 | 4 | 1 |
| Security Configuration [HIGH] | 1 | 2 | 3 | 1 | 1 |
| DTO Design [MEDIUM] | 3 | 5 | 3 | 3 | 1 |
| Sealed Types [MEDIUM] | 2 | 5 | 2 | 5 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 | 1 |
| Structured Logging [MEDIUM] | 3 | 4 | 4 | 4 | 1 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 1 |
| API Documentation [MEDIUM] | 3 | 5 | 3 | 3 | 1 |
| File Organization [MEDIUM] | 4 | 5 | 4 | 4 | 1 |
| HTTP Test File Quality [MEDIUM] | 3 | 4 | 3 | 3 | 1 |
| Type Design & Resource Management [MEDIUM] | 3 | 4 | 3 | 3 | 1 |
| Code Standards Compliance [LOW] | 4 | 5 | 4 | 4 | 1 |

## 1. Build & Run Success [CRITICAL]

All 4 available configs built successfully (`Build succeeded`) and ran long enough for a smoke window (~12s).

```bash
# all available run-3 projects
BuildSucceeded=true, Warnings=0, Errors=0, RunStarted=true, RunStayedAlive12s=true
```

Scores: no-skills **5**, dotnet-artisan **5**, managedcode-dotnet-skills **5**, dotnet-skills **5**, dotnet-webapi **1** (missing).

**Verdict:** Tie among available configs; missing `dotnet-webapi` fails baseline availability.

## 2. Security Vulnerability Scan [CRITICAL]

`dotnet list package --vulnerable --include-transitive` reported no vulnerable packages for all present projects.

Scores: no-skills **5**, dotnet-artisan **5**, managedcode-dotnet-skills **5**, dotnet-skills **5**, dotnet-webapi **1**.

**Verdict:** Tie on known vulnerability surface for available outputs.

## 3. Minimal API Architecture [CRITICAL]

Only `dotnet-artisan` uses modern Minimal API route groups + TypedResults.

```csharp
// dotnet-artisan: Endpoints/OwnerEndpoints.cs
var group = routes.MapGroup("/api/owners").WithTags("Owners");
group.MapGet("/{id:int}", async Task<Results<Ok<OwnerDetailResponse>, NotFound>> (...) => ...
    ? TypedResults.Ok(owner)
    : TypedResults.NotFound());
```

```csharp
// no-skills / managedcode / dotnet-skills: Controllers
[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
```

Scores: no-skills **1**, dotnet-artisan **5**, managedcode-dotnet-skills **1**, dotnet-skills **1**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` clearly best; it matches current .NET Minimal API guidance.

## 4. Input Validation & Guard Clauses [CRITICAL]

All available versions validate request models; style differs.

```csharp
// dotnet-artisan DTOs (DataAnnotations)
[Required, MaxLength(500)]
public required string Reason { get; init; }
[Range(15, 120)]
public int DurationMinutes { get; init; } = 30;
```

```csharp
// managedcode / dotnet-skills Validators.cs
public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    RuleFor(x => x.AppointmentDate).GreaterThan(DateTime.UtcNow);
}
```

Guard-clause primitives (`ArgumentNullException.ThrowIfNull`) are generally absent.

Scores: no-skills **3**, dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** FluentValidation variants and artisan are stronger than baseline, but all miss consistent guard-API usage.

## 5. NuGet & Package Discipline [CRITICAL]

`no-skills` and `managedcode` use floating versions; others mostly pin exact versions.

```xml
<!-- no-skills -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.*-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

```xml
<!-- dotnet-artisan -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```

Scores: no-skills **1**, dotnet-artisan **4**, managedcode-dotnet-skills **2**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` and `dotnet-skills` are materially better on reproducibility.

## 6. EF Migration Usage [CRITICAL]

All present configurations use `EnsureCreated(Async)` instead of migrations.

```csharp
// all available configs, Program.cs pattern
await context.Database.EnsureCreatedAsync();
```

Scores: no-skills **1**, dotnet-artisan **1**, managedcode-dotnet-skills **1**, dotnet-skills **1**, dotnet-webapi **1**.

**Verdict:** Universal critical weakness; production-safe migration workflow is missing.

## 7. Business Logic Correctness [HIGH]

Within `VetClinicApi`, business logic is substantial (status transitions, scheduling conflicts, cancellation constraints). But expected run-3 app set is incomplete (missing `FitnessStudioApi` and `LibraryApi` in every available config).

```csharp
// dotnet-artisan AppointmentService
if (!_validTransitions.TryGetValue(appointment.Status, out var validNext) || !validNext.Contains(newStatus))
    throw new InvalidOperationException(...);
```

Scores: no-skills **2**, dotnet-artisan **2**, managedcode-dotnet-skills **2**, dotnet-skills **2**, dotnet-webapi **1**.

**Verdict:** Functional depth exists in VetClinic, but scenario completeness penalty dominates.

## 8. Prefer Built-in over 3rd Party [HIGH]

All available versions include Swashbuckle; two also include FluentValidation packages.

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
```

Scores: no-skills **2**, dotnet-artisan **3**, managedcode-dotnet-skills **2**, dotnet-skills **2**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` slightly better package minimalism, but none fully prefer built-ins.

## 9. Modern C# Adoption [HIGH]

`dotnet-artisan` uses primary constructors, sealed records, collection expressions broadly.

```csharp
// dotnet-artisan
public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService
private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> _validTransitions = new() { ... };
```

Scores: no-skills **3**, dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **3**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` is strongest and most idiomatic for current C#.

## 10. Error Handling & Middleware [HIGH]

`dotnet-artisan`, `managedcode`, and `dotnet-skills` use `IExceptionHandler` + ProblemDetails; `no-skills` uses custom middleware.

```csharp
// managedcode Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
```

Scores: no-skills **3**, dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** The `IExceptionHandler` implementations are preferable to manual middleware.

## 11. Async Patterns & Cancellation [HIGH]

Async is broadly correct (`Task`, `await`, no `async void`), but cancellation token propagation is sparse.

Scores: no-skills **2**, dotnet-artisan **3**, managedcode-dotnet-skills **3**, dotnet-skills **3**, dotnet-webapi **1**.

**Verdict:** Good async baseline, incomplete cancellation flow through layers.

## 12. EF Core Best Practices [HIGH]

`dotnet-artisan` is strongest: frequent `AsNoTracking()`, includes, and clean service query patterns.

```csharp
// dotnet-artisan
var query = db.Appointments
    .AsNoTracking()
    .Include(a => a.Pet)
    .Include(a => a.Veterinarian);
```

Scores: no-skills **2**, dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` best aligns with performant EF read patterns.

## 13. Service Abstraction & DI [HIGH]

All available outputs use interface-based service registration and scoped DI.

```csharp
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
```

Scores: no-skills **4**, dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** Strong across available configs; artisan is most consistent and cleanly organized.

## 14. Security Configuration [HIGH]

`managedcode` is best of available set (uses HTTPS redirection). None use non-dev HSTS pattern.

```csharp
// managedcode Program.cs
app.UseHttpsRedirection();
```

Scores: no-skills **1**, dotnet-artisan **2**, managedcode-dotnet-skills **3**, dotnet-skills **1**, dotnet-webapi **1**.

**Verdict:** Security middleware is incomplete in all generated variants.

## 15. DTO Design [MEDIUM]

`dotnet-artisan` uses immutable sealed records extensively; others mix mutable classes/records.

```csharp
// dotnet-artisan
public sealed record CreateAppointmentRequest { public required int PetId { get; init; } }
```

```csharp
// managedcode
public sealed class AppointmentCreateDto { public int PetId { get; set; } }
```

Scores: no-skills **3**, dotnet-artisan **5**, managedcode-dotnet-skills **3**, dotnet-skills **3**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` has best API contract immutability and intent.

## 16. Sealed Types [MEDIUM]

`dotnet-skills` and `dotnet-artisan` heavily use sealing; others are mixed.

Scores: no-skills **2**, dotnet-artisan **5**, managedcode-dotnet-skills **2**, dotnet-skills **5**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` and `dotnet-skills` best convey non-inheritance intent/perf posture.

## 17. Data Seeder Design [MEDIUM]

All available configs include seed data and startup seeding paths.

```csharp
await DataSeeder.SeedAsync(context);
```

Scores: no-skills **4**, dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** Similar quality across available outputs.

## 18. Structured Logging [MEDIUM]

All available outputs use `ILogger<T>` and named placeholders.

```csharp
logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId}", appointment.Id, appointment.PetId);
```

Scores: no-skills **3**, dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** Good baseline; artisan/managed/dotnet-skills are more consistently structured.

## 19. Nullable Reference Types [MEDIUM]

All available `.csproj` files enable nullable context.

```xml
<Nullable>enable</Nullable>
```

Scores: no-skills **4**, dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** Strong consistency among available projects.

## 20. API Documentation [MEDIUM]

`dotnet-artisan` is strongest due endpoint-level summaries/tags with Minimal APIs.

```csharp
// dotnet-artisan
group.MapGet("/", ...).WithSummary("List all owners with optional search and pagination");
```

Scores: no-skills **3**, dotnet-artisan **5**, managedcode-dotnet-skills **3**, dotnet-skills **3**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` provides richest OpenAPI metadata shape.

## 21. File Organization [MEDIUM]

All available projects are organized by concern; `dotnet-artisan` is notably clean with endpoint extensions.

Scores: no-skills **4**, dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` best separation in Program.cs + endpoint modules.

## 22. HTTP Test File Quality [MEDIUM]

All available `.http` files are substantial and cover many happy paths; explicit negative-path tests are sparse.

Scores: no-skills **3**, dotnet-artisan **4**, managedcode-dotnet-skills **3**, dotnet-skills **3**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` has strongest practical API walkthrough, but still mostly success-path heavy.

## 23. Type Design & Resource Management [MEDIUM]

Enums and domain types are generally good; precision choices vary (`List<T>` vs `IReadOnlyList<T>`), and cancellation/resource flow can be tighter.

Scores: no-skills **3**, dotnet-artisan **4**, managedcode-dotnet-skills **3**, dotnet-skills **3**, dotnet-webapi **1**.

**Verdict:** `dotnet-artisan` leads on type precision and modern signatures.

## 24. Code Standards Compliance [LOW]

Naming and access patterns are generally compliant across available outputs; `dotnet-artisan` is most consistent.

Scores: no-skills **4**, dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, dotnet-webapi **1**.

**Verdict:** All available variants are readable and convention-friendly; artisan is most polished.

## Weighted Summary

Weights used:
- Critical ×3
- High ×2
- Medium ×1
- Low ×0.5

| Configuration | Weighted Total |
|---|---:|
| dotnet-artisan | **172.5** |
| dotnet-skills | **141.0** |
| managedcode-dotnet-skills | **138.0** |
| no-skills | **117.0** |
| dotnet-webapi | **43.5** |

## What All Versions Get Right

- All available run-3 outputs compile cleanly with zero warnings/errors.
- All available run-3 outputs start successfully (`dotnet run` smoke success).
- All available outputs implement substantial vet-clinic domain rules (appointments, records, vaccinations).
- All available outputs enable nullable reference types.
- All available outputs use EF Core + SQLite with organized project structure.

## Summary: Impact of Skills

Most impactful differences:
1. **API architecture style** (Minimal API route groups + TypedResults vs controllers).
2. **Package/version discipline** (exact pinning vs floating ranges).
3. **EF query quality and modern C# adoption** (especially in `dotnet-artisan`).
4. **Error handling modernization** (`IExceptionHandler` adoption vs legacy middleware).

Overall assessment:
- **dotnet-artisan** produced the strongest modern architecture and code quality profile.
- **dotnet-skills** and **managedcode-dotnet-skills** are solid but controller-centric and weaker on built-in-first/package rigor.
- **no-skills** is functional but lags on architecture modernization and dependency discipline.
- **dotnet-webapi** cannot be evaluated from run-3 output due missing artifacts.

---

### Evidence sources
- `output/*/run-3/VetClinicApi/src/VetClinicApi/Program.cs`
- `output/*/run-3/VetClinicApi/src/VetClinicApi/*.csproj`
- `output/*/run-3/VetClinicApi/src/VetClinicApi/{Controllers|Endpoints|Services|Data|Middleware|Validators}`
- `output/*/run-3/VetClinicApi/src/VetClinicApi/VetClinicApi.http`
- Command checks: `dotnet build`, `dotnet run --no-build`, `dotnet list package --vulnerable --include-transitive`