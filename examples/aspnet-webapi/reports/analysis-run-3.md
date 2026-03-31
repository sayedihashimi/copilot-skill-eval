# Comparative Analysis: no-skills, dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, dotnet-webapi

This run contains **5 configuration directories** under `output/*/run-3`, but only the `VetClinicApi` scenario is present in each (`FitnessStudioApi` and `LibraryApi` are missing in all five). Configuration identity was confirmed via each `run-3/VetClinicApi/gen-notes.md` and directory names: `no-skills`, `dotnet-artisan`, `managedcode-dotnet-skills`, `dotnet-skills`, `dotnet-webapi`.

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills | dotnet-webapi |
|---|---:|---:|---:|---:|---:|
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Minimal API Architecture [CRITICAL] | 1 | 4 | 2 | 1 | 5 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 4 | 3 | 3 | 4 |
| NuGet & Package Discipline [CRITICAL] | 4 | 4 | 1 | 4 | 1 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 4 | 4 | 4 | 4 | 5 |
| Prefer Built-in over 3rd Party [HIGH] | 2 | 2 | 2 | 4 | 5 |
| Modern C# Adoption [HIGH] | 3 | 5 | 3 | 4 | 5 |
| Error Handling & Middleware [HIGH] | 3 | 3 | 4 | 3 | 4 |
| Async Patterns & Cancellation [HIGH] | 2 | 5 | 2 | 2 | 5 |
| EF Core Best Practices [HIGH] | 3 | 5 | 4 | 4 | 5 |
| Service Abstraction & DI [HIGH] | 4 | 3 | 4 | 4 | 4 |
| Security Configuration [HIGH] | 1 | 1 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 3 | 5 | 4 | 4 | 5 |
| Sealed Types [MEDIUM] | 1 | 5 | 1 | 2 | 4 |
| Data Seeder Design [MEDIUM] | 3 | 4 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 3 | 4 | 3 | 3 | 5 |
| File Organization [MEDIUM] | 4 | 5 | 4 | 4 | 5 |
| HTTP Test File Quality [MEDIUM] | 4 | 4 | 4 | 4 | 5 |
| Type Design & Resource Management [MEDIUM] | 3 | 4 | 4 | 4 | 5 |
| Code Standards Compliance [LOW] | 3 | 4 | 4 | 4 | 5 |
| Scenario Coverage (all 3 apps) [CRITICAL] | 1 | 1 | 1 | 1 | 1 |

## 1. Build & Run Success [CRITICAL]

```text
# All configs (build)
Build succeeded.
0 Warning(s)
0 Error(s)

# All configs (run)
RUN_SUMMARY|StillRunningAfter10s=True
```

Scores: no-skills 5, dotnet-artisan 5, managedcode-dotnet-skills 5, dotnet-skills 5, dotnet-webapi 5. All build and stay running for the timeout.

**Verdict:** Tie. Baseline operability is strong across all five.

## 2. Security Vulnerability Scan [CRITICAL]

```text
# all configs
The given project `VetClinicApi` has no vulnerable packages given the current sources.
```

Scores: all 5. No vulnerable packages detected by `dotnet list package --vulnerable`.

**Verdict:** Tie. Dependency vulnerability posture is clean for this snapshot.

## 3. Minimal API Architecture [CRITICAL]

```csharp
// no-skills (Program.cs)
builder.Services.AddControllers();
app.MapControllers();

// dotnet-artisan (Endpoints/AppointmentEndpoints.cs)
var group = routes.MapGroup("/api/appointments").WithTags("Appointments");
return Results.Ok(result);

// managedcode-dotnet-skills (Controllers/AppointmentsController.cs)
[ApiController]
public class AppointmentsController(...) : ControllerBase

// dotnet-skills (Controllers/AppointmentsController.cs)
[ApiController]
public class AppointmentsController : ControllerBase

// dotnet-webapi (Endpoints/AppointmentEndpoints.cs)
group.MapGet("/{id:int}", async Task<Results<Ok<AppointmentDetailResponse>, NotFound>> (...) =>
    appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment));
```

Scores: 1, 4, 2, 1, 5 respectively.

**Verdict:** `dotnet-webapi` is best: route groups + endpoint extension style + `TypedResults` + union return types.

## 4. Input Validation & Guard Clauses [CRITICAL]

```csharp
// no-skills DTOs
[Required, MaxLength(500)]
public required string Reason { get; init; }

// dotnet-artisan endpoints
var validation = await validator.ValidateAsync(request, ct);
if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());

// managedcode-dotnet-skills service
if (request.AppointmentDate <= DateTime.UtcNow)
    throw new InvalidOperationException("Appointment date must be in the future.");

// dotnet-skills validators (FluentValidation)
RuleFor(x => x.AppointmentDate).GreaterThan(DateTime.UtcNow);

// dotnet-webapi DTOs
[Range(15, 120)]
public int DurationMinutes { get; init; } = 30;
```

Scores: 3, 4, 3, 3, 4. Validation is present everywhere, but explicit guard-clause APIs (`ThrowIfNull`, etc.) are largely absent.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` lead due to stronger endpoint/service validation flow.

## 5. NuGet & Package Discipline [CRITICAL]

```xml
<!-- managedcode-dotnet-skills: VetClinicApi.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="*" />

<!-- dotnet-webapi: VetClinicApi.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />

<!-- no-skills: pinned -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```

Scores: 4, 4, 1, 4, 1. Wildcards/floating versions strongly reduce reproducibility for managedcode and dotnet-webapi.

**Verdict:** `no-skills`, `dotnet-artisan`, and `dotnet-skills` are materially better here.

## 6. EF Migration Usage [CRITICAL]

```csharp
// all configs (Program.cs variants)
await context.Database.EnsureCreatedAsync();
// or
context.Database.EnsureCreated();
```

Scores: all 1. No configuration uses `Database.Migrate()` with migrations.

**Verdict:** Universal weakness. This is the largest shared production-readiness gap.

## 7. Business Logic Correctness [HIGH]

```csharp
// dotnet-webapi AppointmentService
if (request.Status == AppointmentStatus.Cancelled)
{
    if (string.IsNullOrWhiteSpace(request.CancellationReason)) throw new ArgumentException(...);
    if (appointment.AppointmentDate < DateTime.UtcNow) throw new ArgumentException(...);
}
```

All five implement the key clinic rules (conflict detection, status transitions, cancellation constraints, medical-record constraints, soft delete).

Scores: 4, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` is most consistent in endpoint semantics + business-rule surfacing.

## 8. Prefer Built-in over 3rd Party [HIGH]

```csharp
// no-skills Program.cs
builder.Services.AddSwaggerGen();
app.UseSwagger();

// dotnet-skills Program.cs
builder.Services.AddOpenApi();
app.UseSwaggerUI(...); // no Swashbuckle package, uses Scalar.AspNetCore package

// dotnet-webapi Program.cs
builder.Services.AddOpenApi();
app.MapOpenApi();
```

Scores: 2, 2, 2, 4, 5.

**Verdict:** `dotnet-webapi` best aligns with built-in OpenAPI-first guidance.

## 9. Modern C# Adoption [HIGH]

```csharp
// dotnet-artisan
public sealed class AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger) : IAppointmentService

// dotnet-webapi
public sealed record CreateAppointmentRequest { public required int PetId { get; init; } }

// no-skills
public class AppointmentService : IAppointmentService
```

Scores: 3, 5, 3, 4, 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` consistently apply modern C# patterns.

## 10. Error Handling & Middleware [HIGH]

```csharp
// managedcode-dotnet-skills
public class GlobalExceptionHandler(...) : IExceptionHandler

// dotnet-artisan
public sealed class GlobalExceptionHandler(...) : IExceptionHandler
{
    // always returns 500
}

// no-skills
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

Scores: 3, 3, 4, 3, 4.

**Verdict:** `managedcode-dotnet-skills` and `dotnet-webapi` are best due to typed `IExceptionHandler` mapping.

## 11. Async Patterns & Cancellation [HIGH]

```csharp
// dotnet-webapi endpoint signature
group.MapGet("/", async (..., CancellationToken ct = default) => ...)

// dotnet-artisan service method
public async Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct = default)

// no-skills service method
public async Task<AppointmentResponseDto> CreateAsync(AppointmentCreateDto dto) // no ct
```

Scores: 2, 5, 2, 2, 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` clearly win on cancellation propagation depth.

## 12. EF Core Best Practices [HIGH]

```csharp
// dotnet-artisan
var query = db.Appointments.AsNoTracking().Include(a => a.Pet).Include(a => a.Veterinarian);

// managedcode-dotnet-skills
modelBuilder.ApplyConfigurationsFromAssembly(typeof(VetClinicDbContext).Assembly);

// no-skills
var query = _context.Appointments.Include(a => a.Pet).Include(a => a.Veterinarian); // no AsNoTracking
```

Scores: 3, 5, 4, 4, 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` are strongest for read-query tracking discipline and relationship config.

## 13. Service Abstraction & DI [HIGH]

```csharp
// no-skills Program.cs
builder.Services.AddScoped<IOwnerService, OwnerService>();

// dotnet-artisan
public interface IAppointmentService { ... }
public sealed class AppointmentService(...) : IAppointmentService
```

Scores: 4, 3, 4, 4, 4.

**Verdict:** Most are good; `dotnet-artisan` loses points for less consistent interface separation by file/structure.

## 14. Security Configuration [HIGH]

```csharp
// Program.cs (all configs): no UseHsts(), no UseHttpsRedirection()
```

Scores: all 1.

**Verdict:** Universal production-hardening gap.

## 15. DTO Design [MEDIUM]

```csharp
// dotnet-webapi
public sealed record AppointmentResponse(...);

// no-skills
public record AppointmentResponse(...);

// dotnet-skills
public class AppointmentResponseDto { public int Id { get; set; } }
```

Scores: 3, 5, 4, 4, 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` best express immutable API contracts.

## 16. Sealed Types [MEDIUM]

```csharp
// dotnet-artisan
public sealed class AppointmentService ...
public sealed class GlobalExceptionHandler ...

// no-skills
public class AppointmentService : IAppointmentService
```

Scores: 1, 5, 1, 2, 4.

**Verdict:** `dotnet-artisan` is clearly strongest.

## 17. Data Seeder Design [MEDIUM]

```csharp
// all configs: Data/DataSeeder.cs called in Program.cs startup scope
await DataSeeder.SeedAsync(db); // or DataSeeder.Seed(context)
```

Scores: 3, 4, 4, 4, 4.

**Verdict:** All provide realistic seeders; `no-skills` is a bit less polished overall around surrounding patterns.

## 18. Structured Logging [MEDIUM]

```csharp
// dotnet-webapi
logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, request.Status);

// no-skills
_logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId} with Vet {VetId}", ...);
```

Scores: all 4.

**Verdict:** Tie at good quality; all use structured placeholders.

## 19. Nullable Reference Types [MEDIUM]

```xml
<!-- all configs: .csproj -->
<Nullable>enable</Nullable>
```

Scores: all 4.

**Verdict:** Solid baseline nullability posture across all five.

## 20. API Documentation [MEDIUM]

```csharp
// dotnet-webapi endpoint metadata
.WithName("GetAppointmentById")
.WithSummary("Get appointment details")
.WithDescription("Returns full appointment details...")
.Produces<AppointmentDetailResponse>();

// dotnet-skills controller metadata
[ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
```

Scores: 3, 4, 3, 3, 5.

**Verdict:** `dotnet-webapi` has the richest, most modern endpoint-level metadata.

## 21. File Organization [MEDIUM]

```text
# dotnet-webapi / dotnet-artisan
Endpoints/, Services/, DTOs/, Middleware/, Data/

# no-skills / managed / dotnet-skills
Controllers/, Services/, DTOs/, Middleware/, Data/
```

Scores: 4, 5, 4, 4, 5.

**Verdict:** Minimal-API endpoint extension structure is most maintainable for these generated projects.

## 22. HTTP Test File Quality [MEDIUM]

```http
# dotnet-webapi VetClinicApi.http
GET {{baseUrl}}/api/appointments?page=1&pageSize=10
PATCH {{baseUrl}}/api/appointments/6/status
```

All `.http` files are broad and realistic. `dotnet-webapi` is slightly better aligned with mapped routes and richer endpoint metadata.

Scores: 4, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` leads narrowly.

## 23. Type Design & Resource Management [MEDIUM]

```csharp
// dotnet-webapi
public async Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct)

// no-skills
public async Task<List<AppointmentSummaryDto>> GetTodayAsync()
```

Scores: 3, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` has the most precise return types and enum usage consistency.

## 24. Code Standards Compliance [LOW]

```csharp
// managedcode-dotnet-skills
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase

// dotnet-webapi
public sealed class AppointmentService(...) : IAppointmentService
```

Scores: 3, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` is the most consistently modern and convention-aligned.

## 25. Scenario Coverage (all 3 apps) [CRITICAL]

```text
output/<config>/run-3 contains:
- VetClinicApi ✅
- FitnessStudioApi ❌
- LibraryApi ❌
```

Scores: all 1.

**Verdict:** Cross-scenario completeness is missing for every configuration, limiting comparison scope.

## Weighted Summary

Weights used: Critical ×3, High ×2, Medium ×1, Low ×0.5.

| Configuration | Critical subtotal | High subtotal | Medium subtotal | Low subtotal | Total weighted |
|---|---:|---:|---:|---:|---:|
| no-skills | 60.0 | 44.0 | 29.0 | 1.5 | **134.5** |
| dotnet-artisan | 72.0 | 56.0 | 39.0 | 2.0 | **169.0** |
| managedcode-dotnet-skills | 54.0 | 48.0 | 32.0 | 2.0 | **136.0** |
| dotnet-skills | 60.0 | 52.0 | 33.0 | 2.0 | **147.0** |
| dotnet-webapi | 66.0 | 68.0 | 41.0 | 2.5 | **177.5** |

## What All Versions Get Right

- All five `VetClinicApi` projects compile cleanly (`Build succeeded`, 0 warnings, 0 errors).
- All five remain running successfully under `dotnet run --no-build` timeout checks.
- All five report no current vulnerable NuGet packages.
- All five enable `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`.
- All five implement the core veterinary domain entities and major endpoint surfaces.

## Summary: Impact of Skills

Most impactful differences (highest practical effect):

1. **API architecture quality**: `dotnet-webapi` (and secondarily `dotnet-artisan`) meaningfully improves endpoint structure and modern Minimal API patterns.
2. **Async + cancellation propagation**: strong in `dotnet-webapi` and `dotnet-artisan`, weak in controller-heavy variants.
3. **Package/version discipline**: wildcard package versions in `managedcode-dotnet-skills` and `dotnet-webapi` materially hurt reproducibility.
4. **Universal migration gap**: every variant still uses `EnsureCreated` instead of migrations.

Overall ranking by weighted score:

1. **dotnet-webapi** — 177.5
2. **dotnet-artisan** — 169.0
3. **dotnet-skills** — 147.0
4. **managedcode-dotnet-skills** — 136.0
5. **no-skills** — 134.5
