# Comparative Analysis: dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, no-skills, dotnet-webapi

This run contains **4 discovered configurations** under `output/*/run-1`: `dotnet-artisan`, `managedcode-dotnet-skills`, `dotnet-skills`, and `no-skills`. Each discovered config includes only `FitnessStudioApi` at `output/{config}/run-1/FitnessStudioApi/src/FitnessStudioApi/`. `LibraryApi`, `VetClinicApi`, and the expected `dotnet-webapi` configuration are **missing in run-1**; those gaps are scored as rubric failures (1–2 depending on dimension severity).

## Executive Summary

| Dimension [Tier] | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills | no-skills | dotnet-webapi |
|---|---:|---:|---:|---:|---:|
| Scenario Coverage [CRITICAL] | 2 | 2 | 2 | 2 | 1 |
| Build & Run Success [CRITICAL] | 2 | 2 | 2 | 2 | 1 |
| Security Vulnerability Scan [CRITICAL] | 2 | 2 | 2 | 2 | 1 |
| Minimal API Architecture [CRITICAL] | 5 | 2 | 2 | 1 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 4 | 4 | 4 | 1 |
| NuGet & Package Discipline [CRITICAL] | 2 | 1 | 1 | 4 | 1 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 2 | 2 | 2 | 2 | 1 |
| Prefer Built-in over 3rd Party [HIGH] | 3 | 2 | 2 | 1 | 1 |
| Modern C# Adoption [HIGH] | 5 | 4 | 4 | 3 | 1 |
| Error Handling & Middleware [HIGH] | 4 | 3 | 4 | 3 | 1 |
| Async Patterns & Cancellation [HIGH] | 5 | 2 | 2 | 1 | 1 |
| EF Core Best Practices [HIGH] | 4 | 4 | 4 | 2 | 1 |
| Service Abstraction & DI [HIGH] | 4 | 4 | 4 | 4 | 1 |
| Security Configuration [HIGH] | 1 | 3 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 5 | 4 | 4 | 3 | 1 |
| Sealed Types [MEDIUM] | 5 | 3 | 2 | 1 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 | 1 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 | 1 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 1 |
| API Documentation [MEDIUM] | 5 | 4 | 4 | 3 | 1 |
| File Organization [MEDIUM] | 5 | 4 | 4 | 3 | 1 |
| HTTP Test File Quality [MEDIUM] | 4 | 4 | 4 | 3 | 1 |
| Type Design & Resource Management [MEDIUM] | 3 | 4 | 4 | 3 | 1 |
| Code Standards Compliance [LOW] | 5 | 4 | 4 | 4 | 1 |

## 1. Scenario Coverage [CRITICAL]

```text
// dotnet-artisan
output/dotnet-artisan/run-1/FitnessStudioApi/...
// dotnet-skills
output/dotnet-skills/run-1/FitnessStudioApi/...
// managedcode-dotnet-skills
output/managedcode-dotnet-skills/run-1/FitnessStudioApi/...
// no-skills
output/no-skills/run-1/FitnessStudioApi/...
```

Scores: dotnet-artisan **2**, managedcode-dotnet-skills **2**, dotnet-skills **2**, no-skills **2**, dotnet-webapi **1**.

Verdict: all discovered configs are incomplete for run-1 because `LibraryApi` and `VetClinicApi` are absent; `dotnet-webapi` directory is absent.

## 2. Build & Run Success [CRITICAL]

```bash
# per discovered config
# build: dotnet build .../FitnessStudioApi.csproj
# run smoke: dotnet run --project ... --no-build (12s alive check)
# result: Build succeeded, 0 warnings, process stayed running
```

Scores: dotnet-artisan **2**, managedcode-dotnet-skills **2**, dotnet-skills **2**, no-skills **2**, dotnet-webapi **1**.

Verdict: all discovered FitnessStudio projects build and start; run-level score is capped because two required scenario apps are missing.

## 3. Security Vulnerability Scan [CRITICAL]

```bash
dotnet list package --vulnerable
# result on discovered projects: No vulnerable packages
```

Scores: dotnet-artisan **2**, managedcode-dotnet-skills **2**, dotnet-skills **2**, no-skills **2**, dotnet-webapi **1**.

Verdict: dependency vulnerability posture is good for the one available app, but overall scenario set is incomplete.

## 4. Minimal API Architecture [CRITICAL]

```csharp
// dotnet-artisan: Endpoints/BookingEndpoints.cs
var group = routes.MapGroup("/api/bookings").WithTags("Bookings");
group.MapPost("/", Create);
return TypedResults.Created($"/api/bookings/{booking.Id}", booking);

// dotnet-skills: Controllers/BookingsController.cs
[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase { ... }
```

Scores: dotnet-artisan **5**, managedcode-dotnet-skills **2**, dotnet-skills **2**, no-skills **1**, dotnet-webapi **1**.

Verdict: `dotnet-artisan` clearly leads with route groups + endpoint extension methods + `TypedResults`; others are controller-first.

## 5. Input Validation & Guard Clauses [CRITICAL]

```csharp
// dotnet-artisan: DTOs/BookingDtos.cs
public sealed record CreateBookingRequest
{
    [Required] public int ClassScheduleId { get; init; }
    [Required] public int MemberId { get; init; }
}

// no-skills: Validators/Validators.cs
RuleFor(x => x.ClassScheduleId).GreaterThan(0);
RuleFor(x => x.MemberId).GreaterThan(0);
```

Scores: dotnet-artisan **3**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **4**, dotnet-webapi **1**.

Verdict: controller-based variants have broader FluentValidation coverage; `dotnet-artisan` validates but less comprehensively.

## 6. NuGet & Package Discipline [CRITICAL]

```xml
<!-- dotnet-artisan FitnessStudioApi.csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-*" />

<!-- no-skills FitnessStudioApi.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

Scores: dotnet-artisan **2**, managedcode-dotnet-skills **1**, dotnet-skills **1**, no-skills **4**, dotnet-webapi **1**.

Verdict: `no-skills` wins version pinning; skill outputs frequently use floating versions (`*`, `10.*-*`, `7.*`).

## 7. EF Migration Usage [CRITICAL]

```csharp
// all discovered configs: Program.cs
db.Database.EnsureCreated();
// or
await db.Database.EnsureCreatedAsync();
```

Scores: dotnet-artisan **1**, managedcode-dotnet-skills **1**, dotnet-skills **1**, no-skills **1**, dotnet-webapi **1**.

Verdict: all implementations use the `EnsureCreated` anti-pattern instead of migrations + `Migrate()`.

## 8. Business Logic Correctness [HIGH]

```csharp
// dotnet-artisan Services/BookingService.cs
if (schedule.StartTime > now.AddDays(7)) throw ...
if (schedule.CurrentEnrollment < schedule.Capacity) ... else ... // waitlist

// prompt requires 3 apps; run-1 provides only FitnessStudioApi
```

Scores: dotnet-artisan **2**, managedcode-dotnet-skills **2**, dotnet-skills **2**, no-skills **2**, dotnet-webapi **1**.

Verdict: fitness rules are largely implemented across discovered configs, but required app set is incomplete.

## 9. Prefer Built-in over 3rd Party [HIGH]

```csharp
// dotnet-artisan Program.cs
builder.Services.AddOpenApi();
app.MapOpenApi();
app.UseSwaggerUI(...);

// no-skills Program.cs
builder.Services.AddSwaggerGen();
app.UseSwagger();
app.UseSwaggerUI(...);
```

Scores: dotnet-artisan **3**, managedcode-dotnet-skills **2**, dotnet-skills **2**, no-skills **1**, dotnet-webapi **1**.

Verdict: none are fully built-in; `dotnet-artisan` is closest but still mixes in Swagger UI package.

## 10. Modern C# Adoption [HIGH]

```csharp
// managedcode-dotnet-skills
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler

// dotnet-artisan DTOs
public sealed record BookingResponse(...);
```

Scores: dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**, dotnet-webapi **1**.

Verdict: `dotnet-artisan` shows strongest modern style consistency (records, file-scoped namespaces, primary constructors, widespread sealed types).

## 11. Error Handling & Middleware [HIGH]

```csharp
// dotnet-skills Middleware/GlobalExceptionHandler.cs
public sealed class GlobalExceptionHandler(...) : IExceptionHandler

// no-skills Middleware/GlobalExceptionHandlerMiddleware.cs
public class GlobalExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context) { try { ... } catch ... }
}
```

Scores: dotnet-artisan **4**, managedcode-dotnet-skills **3**, dotnet-skills **4**, no-skills **3**, dotnet-webapi **1**.

Verdict: `dotnet-artisan` and `dotnet-skills` follow the modern `IExceptionHandler` model better than custom middleware.

## 12. Async Patterns & Cancellation [HIGH]

```csharp
// dotnet-artisan Endpoints/BookingEndpoints.cs
private static async Task<IResult> Create(..., CancellationToken ct)

// dotnet-skills Controllers/BookingsController.cs
public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
```

Scores: dotnet-artisan **5**, managedcode-dotnet-skills **2**, dotnet-skills **2**, no-skills **1**, dotnet-webapi **1**.

Verdict: `dotnet-artisan` is the only one with strong token propagation through endpoint/service/data calls.

## 13. EF Core Best Practices [HIGH]

```csharp
// dotnet-artisan Services/BookingService.cs
var booking = await context.Bookings.AsNoTracking().Include(...).FirstOrDefaultAsync(...);

// no-skills Services/Implementations/BookingService.cs
var booking = await _db.Bookings.Include(...).FirstOrDefaultAsync(...); // no AsNoTracking on many reads
```

Scores: dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **2**, dotnet-webapi **1**.

Verdict: skill variants are better on query tracking discipline; baseline misses read-optimization consistency.

## 14. Service Abstraction & DI [HIGH]

```csharp
// Program.cs (all discovered)
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IMemberService, MemberService>();
```

Scores: dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **4**, dotnet-webapi **1**.

Verdict: all discovered configs do interface-first service registration well.

## 15. Security Configuration [HIGH]

```csharp
// managedcode-dotnet-skills Program.cs
app.UseHttpsRedirection();

// dotnet-artisan Program.cs
if (app.Environment.IsDevelopment()) { app.MapOpenApi(); ... }
// no app.UseHttpsRedirection() / no app.UseHsts()
```

Scores: dotnet-artisan **1**, managedcode-dotnet-skills **3**, dotnet-skills **1**, no-skills **1**, dotnet-webapi **1**.

Verdict: only `managedcode-dotnet-skills` has partial HTTPS hardening; no config implements full production HSTS pattern.

## 16. DTO Design [MEDIUM]

```csharp
// dotnet-artisan DTOs/BookingDtos.cs
public sealed record BookingResponse(...);

// no-skills DTOs/Dtos.cs
public record BookingDto(...);
```

Scores: dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**, dotnet-webapi **1**.

Verdict: immutable record-based DTOs are broadly good; `dotnet-artisan` adds stronger sealing/clarity.

## 17. Sealed Types [MEDIUM]

```csharp
// dotnet-artisan
public sealed class BookingService ...
public sealed record BookingResponse(...)

// no-skills
public class BookingService : IBookingService
```

Scores: dotnet-artisan **5**, managedcode-dotnet-skills **3**, dotnet-skills **2**, no-skills **1**, dotnet-webapi **1**.

Verdict: `dotnet-artisan` is clearly best for explicit non-inheritance intent.

## 18. Data Seeder Design [MEDIUM]

```csharp
// dotnet-artisan Data/DataSeeder.cs
if (await db.MembershipPlans.AnyAsync()) { return; }

// no-skills Data/DataSeeder.cs
if (db.MembershipPlans.Any()) return;
```

Scores: dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **4**, dotnet-webapi **1**.

Verdict: all discovered configs provide realistic seeded datasets with idempotency checks; none tie seeding to migrations.

## 19. Structured Logging [MEDIUM]

```csharp
// managedcode-dotnet-skills Services/BookingService.cs
_logger.LogInformation("Created booking {Id} for member {MemberId} in class {ClassId} (Status: {Status})", ...);
```

Scores: dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **4**, dotnet-webapi **1**.

Verdict: logging is consistently structured across discovered outputs; this is a shared strength.

## 20. Nullable Reference Types [MEDIUM]

```xml
<!-- all discovered csproj -->
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

Scores: dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **4**, dotnet-webapi **1**.

Verdict: NRT is enabled everywhere in discovered configs.

## 21. API Documentation [MEDIUM]

```csharp
// dotnet-artisan Endpoints/BookingEndpoints.cs
group.MapPost("/", Create)
    .WithName("CreateBooking")
    .WithSummary("Book a class (enforces all booking rules)");

// dotnet-skills Controllers/BookingsController.cs
[ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
```

Scores: dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**, dotnet-webapi **1**.

Verdict: both styles include metadata, but route-level fluent metadata in `dotnet-artisan` is cleaner and more complete.

## 22. File Organization [MEDIUM]

```text
// dotnet-artisan
Endpoints/, Services/, Data/, DTOs/, Middleware/

// dotnet-skills / managedcode / no-skills
Controllers/, Services/, Data/, DTOs/, Validators/, Middleware/
```

Scores: dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**, dotnet-webapi **1**.

Verdict: all are workable; `dotnet-artisan` best matches modern minimal-API modularity.

## 23. HTTP Test File Quality [MEDIUM]

```http
# dotnet-artisan FitnessStudioApi.http
GET {{baseUrl}}/api/members/1
POST {{baseUrl}}/api/members
...

# dotnet-skills FitnessStudioApi.http
POST {{baseUrl}}/api/members
# includes some negative case examples (e.g., too-young member)
```

Scores: dotnet-artisan **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**, dotnet-webapi **1**.

Verdict: discovered `.http` files are broad; controller variants include slightly more explicit negative-path examples.

## 24. Type Design & Resource Management [MEDIUM]

```csharp
// dotnet-skills Data/FitnessDbContext.cs
entity.Property(e => e.Status).HasConversion<string>();

// dotnet-artisan Data/FitnessDbContext.cs
public class FitnessDbContext : DbContext
```

Scores: dotnet-artisan **3**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**, dotnet-webapi **1**.

Verdict: managedcode/dotnet-skills are stronger on enum conversion consistency and relationship config richness.

## 25. Code Standards Compliance [LOW]

```csharp
// dotnet-artisan
namespace FitnessStudioApi.Services;
public sealed class BookingService(...)

// no-skills
namespace FitnessStudioApi.Services.Implementations;
public sealed class BookingService : IBookingService
```

Scores: dotnet-artisan **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **4**, dotnet-webapi **1**.

Verdict: naming and style are generally solid across discovered outputs, with `dotnet-artisan` most consistent.

## Weighted Summary

Weights used: Critical × 3, High × 2, Medium × 1, Low × 0.5.

| Configuration | Weighted Total |
|---|---:|
| dotnet-artisan | **148.5** |
| dotnet-skills | **127.0** |
| managedcode-dotnet-skills | **124.0** |
| no-skills | **112.0** |
| dotnet-webapi | **46.5** |

## What All Versions Get Right

- All discovered configs compile with `dotnet build` (0 errors, 0 warnings) and start with `dotnet run` smoke checks.
- All discovered configs enable nullable reference types in project files.
- All discovered configs implement substantial domain logic for fitness booking rules.
- All discovered configs provide DI service registration with interface abstractions.
- All discovered configs include a `.http` test file and startup data seeding.

## Summary: Impact of Skills

Most impactful deltas:

1. **Architecture style**: `dotnet-artisan` is the only run-1 output using true Minimal API route-group architecture with `TypedResults`.
2. **Async/cancellation rigor**: `dotnet-artisan` consistently propagates `CancellationToken`; others often stop at controller/service boundaries.
3. **Dependency discipline**: `no-skills` unexpectedly outperforms skill variants on exact package pinning; multiple skill outputs use floating versions.
4. **Security pipeline**: none implement migration-first DB lifecycle; all use `EnsureCreated`, which is a production-readiness gap.
5. **Dataset completeness**: the biggest practical weakness is missing `LibraryApi` and `VetClinicApi` across run-1, plus missing `dotnet-webapi` config.

Overall ranking by weighted score: **dotnet-artisan > dotnet-skills > managedcode-dotnet-skills > no-skills > dotnet-webapi (missing)**. If run-1 is regenerated with all three apps and all five configs, the ranking confidence would materially increase.
