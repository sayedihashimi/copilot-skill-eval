# Comparative Analysis: dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, no-skills, dotnet-webapi

This analysis compares 5 Copilot configurations under `output/{config}/run-1/`.  
Observed `run-1` content includes only `FitnessStudioApi` for all configurations; `LibraryApi` and `VetClinicApi` are not present in `run-1` and are scored as a critical coverage gap. Configuration identity was confirmed from folder names and `gen-notes.md` where present.

## Executive Summary

| Dimension [Tier] | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills | no-skills | dotnet-webapi |
|---|---:|---:|---:|---:|---:|
| Scenario Coverage [CRITICAL] | 2 | 2 | 2 | 2 | 2 |
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 5 | 5 | 4 | 4 | 5 |
| Minimal API Architecture [CRITICAL] | 5 | 2 | 1 | 1 | 5 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 4 | 4 | 3 | 4 |
| NuGet & Package Discipline [CRITICAL] | 4 | 4 | 4 | 1 | 5 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 5 | 2 | 4 | 4 | 5 |
| Prefer Built-in over 3rd Party [HIGH] | 3 | 2 | 2 | 2 | 5 |
| Modern C# Adoption [HIGH] | 5 | 4 | 3 | 2 | 5 |
| Error Handling & Middleware [HIGH] | 5 | 3 | 4 | 3 | 4 |
| Async Patterns & Cancellation [HIGH] | 5 | 2 | 2 | 2 | 5 |
| EF Core Best Practices [HIGH] | 4 | 4 | 4 | 2 | 3 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 5 | 5 | 5 |
| Security Configuration [HIGH] | 1 | 1 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 5 | 4 | 3 | 3 | 5 |
| Sealed Types [MEDIUM] | 5 | 3 | 4 | 2 | 5 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 5 | 5 | 4 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 5 | 5 | 5 | 5 | 5 |
| API Documentation [MEDIUM] | 4 | 2 | 4 | 4 | 5 |
| File Organization [MEDIUM] | 5 | 4 | 4 | 4 | 5 |
| HTTP Test File Quality [MEDIUM] | 4 | 4 | 4 | 4 | 5 |
| Type Design & Resource Management [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Code Standards Compliance [LOW] | 5 | 4 | 4 | 3 | 5 |

## 1. Scenario Coverage [CRITICAL]

```text
# all configs (run-1)
output/{config}/run-1/
  events.jsonl
  FitnessStudioApi/
```

Scores: dotnet-artisan 2, managedcode-dotnet-skills 2, dotnet-skills 2, no-skills 2, dotnet-webapi 2.  
All miss `LibraryApi` and `VetClinicApi` in `run-1`.

**Verdict:** Tie (all incomplete for run-1 multi-scenario coverage).

## 2. Build & Run Success [CRITICAL]

```text
# all configs
Build succeeded.
0 Warning(s)
0 Error(s)
Now listening on: http://localhost:{port}
Application started.
```

Scores: all 5.  
Each builds cleanly and stays running during timed `dotnet run --no-build`.

**Verdict:** Tie (all production-viable from basic compile/start perspective).

## 3. Security Vulnerability Scan [CRITICAL]

```text
# all configs
dotnet list package --vulnerable
... has no vulnerable packages ...
# dotnet-skills only
dotnet list package --deprecated
FluentValidation.AspNetCore 11.3.1 (Legacy)
```

Scores: artisan 5, managedcode 5, dotnet-skills 4, no-skills 4, webapi 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` are strongest (no vulnerable/deprecated hits in observed output).

## 4. Minimal API Architecture [CRITICAL]

```csharp
// dotnet-webapi: Program.cs
app.MapMembershipPlanEndpoints();
app.MapMemberEndpoints();
// dotnet-skills: Program.cs
builder.Services.AddControllers();
app.MapControllers();
```

Scores: artisan 5, managedcode 2, dotnet-skills 1, no-skills 1, webapi 5.  
`managedcode` has endpoint files but actually maps controllers in `Program.cs`.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` best align to modern Minimal API guidance.

## 5. Input Validation & Guard Clauses [CRITICAL]

```csharp
// managedcode: Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// dotnet-artisan: service ctor lacks ThrowIfNull guards
public class BookingService(FitnessDbContext db, ILogger<BookingService> logger)
```

Scores: artisan 3, managedcode 4, dotnet-skills 4, no-skills 3, webapi 4.

**Verdict:** `managedcode`/`dotnet-skills` are stronger on validation pipelines.

## 6. NuGet & Package Discipline [CRITICAL]

```xml
<!-- no-skills: floating version -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*-*" />
<!-- dotnet-webapi: strict -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5" />
```

Scores: artisan 4, managedcode 4, dotnet-skills 4, no-skills 1, webapi 5.

**Verdict:** `dotnet-webapi` is clearly best; `no-skills` is worst due to non-reproducible package ranges.

## 7. EF Migration Usage [CRITICAL]

```csharp
// all configs
db.Database.EnsureCreated(); // or EnsureCreatedAsync()
```

Scores: all 1.  
No configuration uses `Database.Migrate()` + migrations.

**Verdict:** Universal critical gap.

## 8. Business Logic Correctness [HIGH]

```csharp
// dotnet-artisan: BookingService.cs
if (schedule.StartTime > now.AddDays(7)) throw ...
if (schedule.CurrentEnrollment < schedule.Capacity) ... else waitlist...
```

Scores: artisan 5, managedcode 2, dotnet-skills 4, no-skills 4, webapi 5.  
`managedcode` defines rich endpoint files but runtime maps controllers, reducing effective coverage of intended Minimal API behavior.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` are strongest on implemented rules + active routing model consistency.

## 9. Prefer Built-in over 3rd Party [HIGH]

```csharp
// dotnet-webapi: Program.cs
builder.Services.AddOpenApi();
app.MapOpenApi();
// no-skills/dotnet-skills/managedcode
builder.Services.AddSwaggerGen();
```

Scores: artisan 3, managedcode 2, dotnet-skills 2, no-skills 2, webapi 5.

**Verdict:** `dotnet-webapi` best matches first-party platform preference.

## 10. Modern C# Adoption [HIGH]

```csharp
// dotnet-artisan
public sealed class FitnessDbContext(DbContextOptions<FitnessDbContext> options) : DbContext(options)
// no-skills (older style)
public class BookingService : IBookingService { private readonly ... }
```

Scores: artisan 5, managedcode 4, dotnet-skills 3, no-skills 2, webapi 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` show strongest modern-C# idioms.

## 11. Error Handling & Middleware [HIGH]

```csharp
// dotnet-artisan
public sealed class GlobalExceptionHandler(...) : IExceptionHandler
// dotnet-skills
public sealed class GlobalExceptionHandlerMiddleware
```

Scores: artisan 5, managedcode 3, dotnet-skills 4, no-skills 3, webapi 4.

**Verdict:** `dotnet-artisan` is best with modern `IExceptionHandler` and clear status mapping.

## 12. Async Patterns & Cancellation [HIGH]

```csharp
// dotnet-webapi endpoints
async (..., CancellationToken ct) => await service.CreateAsync(request, ct)
// dotnet-skills controllers
public async Task<IActionResult> Create([FromBody] BookingCreateDto dto)
```

Scores: artisan 5, managedcode 2, dotnet-skills 2, no-skills 2, webapi 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` propagate cancellation consistently.

## 13. EF Core Best Practices [HIGH]

```csharp
// dotnet-artisan
var booking = await db.Bookings.AsNoTracking().Include(...).FirstOrDefaultAsync(...)
// no-skills: no AsNoTracking in reads
var b = await _db.Bookings.Include(...).FirstOrDefaultAsync(...)
```

Scores: artisan 4, managedcode 4, dotnet-skills 4, no-skills 2, webapi 3.

**Verdict:** `dotnet-artisan`, `managedcode`, and `dotnet-skills` lead on read-query tracking discipline.

## 14. Service Abstraction & DI [HIGH]

```csharp
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IBookingService, BookingService>();
```

Scores: all 5.

**Verdict:** Tie (all follow interface-driven DI registration).

## 15. Security Configuration [HIGH]

```csharp
// Program.cs (all)
// no app.UseHsts();
// no app.UseHttpsRedirection();
```

Scores: all 1.

**Verdict:** Universal production-hardening gap.

## 16. DTO Design [MEDIUM]

```csharp
// dotnet-webapi: DTOs/Dtos.cs
public sealed record MemberResponse(int Id, string FirstName, ...);
// dotnet-skills
public class BookingDto { ... }
```

Scores: artisan 5, managedcode 4, dotnet-skills 3, no-skills 3, webapi 5.

**Verdict:** `dotnet-webapi`/`dotnet-artisan` strongest with immutable record-centric DTOs.

## 17. Sealed Types [MEDIUM]

```csharp
// dotnet-artisan
public sealed class BookingService ...
// no-skills
public class BookingService ...
```

Scores: artisan 5, managedcode 3, dotnet-skills 4, no-skills 2, webapi 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` best express inheritance intent and optimization.

## 18. Data Seeder Design [MEDIUM]

```csharp
// all: Program.cs
await DataSeeder.SeedAsync(db); // guarded by emptiness checks in seeder
```

Scores: all 4.  
Seeders are generally practical and startup-safe for demo data.

**Verdict:** Tie.

## 19. Structured Logging [MEDIUM]

```csharp
logger.LogInformation("Booking created: Member {MemberId} -> Class {ClassId}, Status: {Status}", ...);
```

Scores: artisan 5, managedcode 5, dotnet-skills 4, no-skills 4, webapi 4.

**Verdict:** `dotnet-artisan` and `managedcode` slightly ahead on consistent structured logging usage.

## 20. Nullable Reference Types [MEDIUM]

```xml
<Nullable>enable</Nullable>
```

Scores: all 5.

**Verdict:** Tie (all enforce NRT at project level).

## 21. API Documentation [MEDIUM]

```csharp
// dotnet-webapi endpoints
.WithSummary("...").WithDescription("...").Produces<T>(...)
// managedcode runtime path uses controllers; endpoint metadata underused
app.MapControllers();
```

Scores: artisan 4, managedcode 2, dotnet-skills 4, no-skills 4, webapi 5.

**Verdict:** `dotnet-webapi` provides the richest and most consistent OpenAPI metadata.

## 22. File Organization [MEDIUM]

```text
FitnessStudioApi/
  Data/ DTOs/ Endpoints/ Middleware/ Models/ Services/
```

Scores: artisan 5, managedcode 4, dotnet-skills 4, no-skills 4, webapi 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` are cleanest and most coherent.

## 23. HTTP Test File Quality [MEDIUM]

```http
@baseUrl = http://localhost:5236
GET {{baseUrl}}/api/members/1/bookings/upcoming
POST {{baseUrl}}/api/bookings
```

Scores: artisan 4, managedcode 4, dotnet-skills 4, no-skills 4, webapi 5.  
All provide broad endpoint coverage; `dotnet-webapi` is best organized and scenario-rich.

**Verdict:** `dotnet-webapi` wins by clarity and completeness of request narratives.

## 24. Type Design & Resource Management [MEDIUM]

```csharp
// common positive pattern
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }
```

Scores: artisan 4, managedcode 4, dotnet-skills 4, no-skills 4, webapi 4.

**Verdict:** Mostly even; generally solid enum-driven domain modeling.

## 25. Code Standards Compliance [LOW]

```csharp
// dotnet-artisan
namespace FitnessStudioApi.Services;
public sealed class BookingService ...
```

Scores: artisan 5, managedcode 4, dotnet-skills 4, no-skills 3, webapi 5.

**Verdict:** `dotnet-artisan` and `dotnet-webapi` are the most consistent on modern style and naming.

## Weighted Summary

Weights used:
- Critical × 3
- High × 2
- Medium × 1
- Low × 0.5

| Configuration | Weighted Total |
|---|---:|
| dotnet-webapi | **191.5** |
| dotnet-artisan | **184.5** |
| managedcode-dotnet-skills | **152.0** |
| dotnet-skills | **151.0** |
| no-skills | **128.5** |

## What All Versions Get Right

- All `run-1` projects compile and start successfully with zero build warnings/errors in observed runs.
- All enable nullable reference types (`<Nullable>enable</Nullable>`).
- All use EF Core + SQLite and include functional data seeding for demo usage.
- All define a service layer with DI registrations for domain services.
- All provide an `.http` file with useful endpoint exercises.

## Summary: Impact of Skills

Most impactful differences:

- **Architecture fidelity**: `dotnet-webapi` and `dotnet-artisan` consistently produce Minimal APIs; controller-oriented variants lag modern .NET direction.
- **Dependency hygiene**: `dotnet-webapi` has the cleanest package set and strict pinning; `no-skills` uses floating EF versions.
- **Execution consistency**: `managedcode-dotnet-skills` includes strong endpoint metadata but maps controllers in runtime pipeline, reducing architectural coherence.
- **Modern platform alignment**: first-party OpenAPI + typed minimal endpoints materially improves maintainability and generated API docs.

Overall assessment:

1. **dotnet-webapi**: strongest overall (best weighted score, architecture, package discipline, OpenAPI quality).  
2. **dotnet-artisan**: close second (excellent modern patterns; slight penalty for SwaggerUI package and `EnsureCreated`).  
3. **managedcode-dotnet-skills**: good internals but inconsistent runtime architecture in `Program.cs`.  
4. **dotnet-skills**: solid business logic but older controller-first and extra third-party dependency choices.  
5. **no-skills**: functional baseline, but weakest dependency discipline and modern architecture adoption.

