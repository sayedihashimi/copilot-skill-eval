# Comparative Analysis: dotnet-artisan, dotnet-skills, managedcode-dotnet-skills, no-skills

## Introduction

This report compares **4 Copilot skill configurations**, each generating the same **FitnessStudioApi** — a booking and membership management system for "Zenith Fitness Studio" targeting .NET 10 with EF Core + SQLite. The configurations are:

| Config | Description | App Location |
|---|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain (using-dotnet → dotnet-advisor → dotnet-csharp → dotnet-api) | `output/dotnet-artisan/run-1/FitnessStudioApi/` |
| **dotnet-skills** | Official .NET Skills (analyzing-dotnet-performance + optimizing-ef-core-queries) | `output/dotnet-skills/run-1/FitnessStudioApi/` |
| **managedcode-dotnet-skills** | Community managed-code skills (dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-modern-csharp, etc.) | `output/managedcode-dotnet-skills/run-1/FitnessStudioApi/` |
| **no-skills** | Baseline (default Copilot, no skills) | `output/no-skills/run-1/FitnessStudioApi/` |

Each configuration generated a single app (`FitnessStudioApi`) with 7 entities, 7 services, 38–41 endpoints, and all 12 specified business rules.

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 5 | 5 | 5 | 5 |
| Minimal API Architecture [CRITICAL] | 5 | 3 | 1 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 4 | 4 | 3 | 3 |
| NuGet & Package Discipline [CRITICAL] | 4 | 4 | 3 | 2 |
| EF Migration Usage [CRITICAL] | 2 | 2 | 2 | 2 |
| Business Logic Correctness [HIGH] | 5 | 5 | 5 | 5 |
| Prefer Built-in over 3rd Party [HIGH] | 4 | 2 | 2 | 2 |
| Modern C# Adoption [HIGH] | 5 | 3 | 4 | 2 |
| Error Handling & Middleware [HIGH] | 5 | 4 | 5 | 3 |
| Async Patterns & Cancellation [HIGH] | 5 | 2 | 2 | 2 |
| EF Core Best Practices [HIGH] | 4 | 4 | 5 | 3 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 5 | 5 |
| Security Configuration [HIGH] | 2 | 2 | 2 | 2 |
| DTO Design [MEDIUM] | 5 | 4 | 3 | 4 |
| Sealed Types [MEDIUM] | 5 | 5 | 1 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 5 | 5 | 5 | 5 |
| API Documentation [MEDIUM] | 5 | 4 | 4 | 4 |
| File Organization [MEDIUM] | 5 | 4 | 5 | 3 |
| HTTP Test File Quality [MEDIUM] | 5 | 4 | 5 | 4 |
| Type Design & Resource Management [MEDIUM] | 5 | 4 | 4 | 4 |
| Code Standards Compliance [LOW] | 5 | 5 | 4 | 4 |

---

## 1. Build & Run Success [CRITICAL]

All four configurations compile successfully with 0 errors on .NET 10.

| Config | Build Result | Warnings (clean build) |
|---|---|---|
| dotnet-artisan | ✅ Build succeeded | 256 (analyzer warnings from Meziantou) |
| dotnet-skills | ✅ Build succeeded | 211 |
| managedcode | ✅ Build succeeded | 239 |
| no-skills | ✅ Build succeeded | 211 |

All warnings are analyzer-level (MA0004 ConfigureAwait, CA1848 LoggerMessage delegates) from the shared `Directory.Build.props` which includes `Meziantou.Analyzer`. These are informational — all four configs produce the same warning categories.

**Scores**: dotnet-artisan: **5**, dotnet-skills: **5**, managedcode: **5**, no-skills: **5**

**Verdict**: All configurations pass this critical gate equally.

---

## 2. Security Vulnerability Scan [CRITICAL]

```
dotnet list package --vulnerable
→ "has no vulnerable packages" (all 4 configs)
```

No known CVEs in any configuration's package set. All use current .NET 10 preview packages.

**Scores**: dotnet-artisan: **5**, dotnet-skills: **5**, managedcode: **5**, no-skills: **5**

**Verdict**: Tie — all clean.

---

## 3. Minimal API Architecture [CRITICAL]

This is the **highest-signal differentiator** across configurations.

### dotnet-artisan — Minimal APIs with Route Groups, TypedResults, Endpoint Extensions

```csharp
// Program.cs — clean delegation to 7 endpoint extension methods
app.MapMembershipPlanEndpoints();
app.MapMemberEndpoints();
app.MapBookingEndpoints();
// ... 4 more
```

```csharp
// Endpoints/BookingEndpoints.cs
public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings").WithTags("Bookings");

        group.MapPost("/", async (CreateBookingDto dto, IBookingService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(dto, ct);
            return TypedResults.Created($"/api/bookings/{result.Id}", result);
        }).WithSummary("Book a class (enforces all booking rules)");

        return group;
    }
}
```

✅ MapGroup with route prefixes, ✅ TypedResults.Created/Ok/NoContent, ✅ WithTags + WithSummary, ✅ CancellationToken injection, ✅ Endpoint extension methods keep Program.cs clean (7 lines of mapping)

### dotnet-skills — Minimal APIs with Route Groups, but Results (not TypedResults), Inline

```csharp
// Program.cs — ALL 41 endpoints defined inline (283 lines)
var planGroup = app.MapGroup("/api/membership-plans").WithTags("Membership Plans");

planGroup.MapGet("/", async (IMembershipPlanService svc) =>
    Results.Ok(await svc.GetAllAsync()))
    .WithSummary("List all active membership plans");

planGroup.MapPost("/", async (CreateMembershipPlanDto dto, IMembershipPlanService svc) =>
{
    var plan = await svc.CreateAsync(dto);
    return Results.Created($"/api/membership-plans/{plan.Id}", plan);
});
```

✅ MapGroup, ✅ WithSummary, ❌ Uses `Results.*` not `TypedResults.*` (no compile-time type safety, no automatic OpenAPI schema), ❌ All endpoints inline in Program.cs (283 lines), ❌ No CancellationToken, ❌ No endpoint extension methods

### managedcode-dotnet-skills — Controllers (no Minimal APIs)

```csharp
// Controllers/MembersController.cs
[ApiController]
[Route("api/members")]
[Produces("application/json")]
public class MembersController(IMemberService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search, ...)
    {
        var result = await service.GetAllAsync(search, isActive, page, pageSize);
        return Ok(result);
    }
}
```

❌ No Minimal APIs — uses `[ApiController]` controllers, ❌ No MapGroup/TypedResults, ✅ ProducesResponseType for OpenAPI

### no-skills — Controllers (no Minimal APIs)

```csharp
// Controllers/MembershipPlansController.cs
[ApiController]
[Route("api/membership-plans")]
[Produces("application/json")]
public class MembershipPlansController : ControllerBase
{
    private readonly IMembershipPlanService _service;
    public MembershipPlansController(IMembershipPlanService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MembershipPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() { ... }
}
```

❌ Controllers pattern, ❌ Traditional constructors (no primary constructors)

**Scores**: dotnet-artisan: **5**, dotnet-skills: **3**, managedcode: **1**, no-skills: **1**

**Verdict**: **dotnet-artisan** is the only configuration that fully implements the modern Minimal API pattern with route groups, TypedResults, and endpoint extension methods. dotnet-skills uses Minimal APIs but misses TypedResults and keeps everything inline. managedcode and no-skills use the legacy Controllers pattern.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

### dotnet-artisan

```csharp
// DTOs/MemberDtos.cs — sealed records with validation attributes
public sealed record CreateMemberDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    DateOnly DateOfBirth,
    [Required, MaxLength(200)] string EmergencyContactName,
    [Required] string EmergencyContactPhone);
```

✅ Validation attributes on DTO constructor parameters, ✅ Business rule validation in services (throw expressions), ❌ No ArgumentNullException.ThrowIfNull guard clauses on constructors

### dotnet-skills

```csharp
// DTOs use sealed classes for input with validation attributes
public sealed class CreateMembershipPlanDto
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue)] public decimal Price { get; set; }
}
```

✅ Validation attributes on input DTOs, ✅ Business rule exceptions, ❌ No guard clauses

### managedcode-dotnet-skills

```csharp
// DTOs/MembershipPlan/MembershipPlanDtos.cs — classes, no validation
public class CreateMembershipPlanDto { /* no validation attributes */ }
```

❌ No validation attributes on input DTOs (validation only on model entities), ✅ Business rule validation in services, ❌ FluentValidation package referenced but never used

### no-skills

```csharp
// Dtos.cs — records without validation attributes
public record MembershipPlanCreateDto(
    string Name, string? Description, int DurationMonths, decimal Price, ...);
```

❌ No validation attributes on DTO records, ✅ Business rule validation in services

**Scores**: dotnet-artisan: **4**, dotnet-skills: **4**, managedcode: **3**, no-skills: **3**

**Verdict**: dotnet-artisan and dotnet-skills both apply validation attributes on input DTOs. managedcode has validation only on entities (not DTOs), and no-skills uses records without annotations. None use guard clauses.

---

## 5. NuGet & Package Discipline [CRITICAL]

| Config | Packages | Version Strategy | Issues |
|---|---|---|---|
| dotnet-artisan | 4 | Exact (`10.0.4`, `10.0.5`, `10.1.7`) | SwaggerUI-only (not full Swashbuckle) ✅ |
| dotnet-skills | 4 | Exact (`10.0.5`, `10.1.7`) | Full Swashbuckle |
| managedcode | 5 | Exact (`10.0.4`, `10.0.5`, `11.3.1`) | FluentValidation.AspNetCore included but **never used** ❌ |
| no-skills | 4 | **Wildcards** (`10.0.*-*`) ❌ | Non-reproducible builds |

```xml
<!-- no-skills: WORST — wildcard versions -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.*-*" />

<!-- dotnet-artisan: BEST — exact versions, minimal packages -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="10.1.7" />
```

**Scores**: dotnet-artisan: **4**, dotnet-skills: **4**, managedcode: **3**, no-skills: **2**

**Verdict**: **no-skills** uses wildcard versions — the worst practice for reproducibility. **managedcode** includes an unused FluentValidation package (unnecessary dependency). dotnet-artisan is most minimal (SwaggerUI only instead of full Swashbuckle). All lose a point because `Meziantou.Analyzer` uses `Version="*"` in the shared `Directory.Build.props`.

---

## 6. EF Migration Usage [CRITICAL]

All four configurations use `EnsureCreatedAsync()`:

```csharp
// All configs — identical pattern
await db.Database.EnsureCreatedAsync();
await DataSeeder.SeedAsync(db);
```

None use `context.Database.MigrateAsync()` or reference a migrations folder. While acceptable for a demo/dev SQLite database, this prevents schema evolution in production.

**Scores**: dotnet-artisan: **2**, dotnet-skills: **2**, managedcode: **2**, no-skills: **2**

**Verdict**: Tie — all use the anti-pattern. For a demo app with SQLite this is understandable, but none demonstrate the migration-based approach.

---

## 7. Business Logic Correctness [HIGH]

All four configurations implement all 12 specified business rules and all specified endpoints:

| Business Rule | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Booking window (7d/30min) | ✅ | ✅ | ✅ | ✅ |
| Capacity + waitlist promotion | ✅ | ✅ | ✅ | ✅ |
| Cancellation policy (2hr) | ✅ | ✅ | ✅ | ✅ |
| Premium class tier access | ✅ | ✅ | ✅ | ✅ |
| Weekly booking limits | ✅ | ✅ | ✅ | ✅ |
| Active membership required | ✅ | ✅ | ✅ | ✅ |
| No double booking (overlap) | ✅ | ✅ | ✅ | ✅ |
| Instructor schedule conflicts | ✅ | ✅ | ✅ | ✅ |
| Membership freeze/unfreeze | ✅ | ✅ | ✅ | ✅ |
| Class cancellation cascade | ✅ | ✅ | ✅ | ✅ |
| Check-in window (±15min) | ✅ | ✅ | ✅ | ✅ |
| No-show marking | ✅ | ✅ | ✅ | ✅ |
| **Endpoints** | 40+ | 41 | 38 | 41 |

**Scores**: dotnet-artisan: **5**, dotnet-skills: **5**, managedcode: **5**, no-skills: **5**

**Verdict**: All configurations implement the full specification. The prompt is detailed enough that all skill configurations produce functionally complete implementations.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

| Config | OpenAPI | Swagger UI | JSON | DI | Logging |
|---|---|---|---|---|---|
| dotnet-artisan | `AddOpenApi()` ✅ | SwaggerUI standalone | System.Text.Json ✅ | Built-in ✅ | ILogger ✅ |
| dotnet-skills | `AddOpenApi()` referenced | `AddSwaggerGen()` ❌ | System.Text.Json ✅ | Built-in ✅ | ILogger ✅ |
| managedcode | `AddOpenApi()` + `AddSwaggerGen()` | Both | System.Text.Json ✅ | Built-in ✅ | ILogger ✅ |
| no-skills | `AddOpenApi()` referenced | `AddSwaggerGen()` ❌ | System.Text.Json ✅ | Built-in ✅ | ILogger ✅ |

```csharp
// dotnet-artisan — built-in OpenAPI + standalone SwaggerUI
builder.Services.AddOpenApi();
app.MapOpenApi();
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Zenith Fitness Studio API"));

// no-skills — full Swashbuckle
builder.Services.AddSwaggerGen(options => { ... });
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "..."));
```

dotnet-artisan uses `AddOpenApi()` + `MapOpenApi()` (built-in .NET 10) with only `Swashbuckle.AspNetCore.SwaggerUI` for the UI. All others use the full `Swashbuckle.AspNetCore` package which is no longer recommended for .NET 10.

**Scores**: dotnet-artisan: **4**, dotnet-skills: **2**, managedcode: **2**, no-skills: **2**

**Verdict**: **dotnet-artisan** correctly separates OpenAPI generation (built-in) from UI (SwaggerUI standalone). All others use full Swashbuckle which duplicates framework capabilities.

---

## 9. Modern C# Adoption [HIGH]

| Feature | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Primary constructors | ✅ All services + DbContext | ❌ Traditional | ✅ Services + controllers | ❌ Traditional |
| Collection expressions `[]` | ✅ (`= []`) | ❌ (`= new List<T>()`) | ✅ (`= []`) | ❌ (`= new List<T>()`) |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ |
| Record DTOs | ✅ Sealed records | ✅ Records (output) + sealed classes (input) | ❌ Classes only | ✅ Records |
| Sealed modifier | ✅ All types | ✅ All types | ❌ None | ❌ None |
| Pattern matching | ✅ `is not null`, switch expr | ✅ `is not null`, switch expr | ✅ `is not null`, switch expr | ✅ `is null` / `is not null` |
| Nullable reference types | ✅ | ✅ | ✅ | ✅ |

```csharp
// dotnet-artisan — primary constructor + sealed + collection expression
public sealed class MembershipPlanService(FitnessDbContext db) : IMembershipPlanService { }
public ICollection<Booking> Bookings { get; set; } = [];

// no-skills — traditional constructor + new List<T>()
private readonly FitnessDbContext _db;
public MembershipPlanService(FitnessDbContext db) { _db = db; }
public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
```

**Scores**: dotnet-artisan: **5**, dotnet-skills: **3**, managedcode: **4**, no-skills: **2**

**Verdict**: **dotnet-artisan** uses all modern C# features consistently. **managedcode** is close with primary constructors and collection expressions but lacks sealed types and records. **dotnet-skills** seals everything but uses traditional constructors. **no-skills** has the most dated patterns.

---

## 10. Error Handling & Middleware [HIGH]

### dotnet-artisan & managedcode — IExceptionHandler with multiple exception types

```csharp
// dotnet-artisan: Middleware/GlobalExceptionHandler.cs
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title) = exception switch
        {
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Business Rule Violation"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
        // ProblemDetails response...
    }
}
```

managedcode adds a third custom exception (`ConflictException`) beyond `BusinessRuleException` and `NotFoundException`, providing finer-grained HTTP status mapping.

### dotnet-skills — IExceptionHandler but only maps BusinessRuleException

```csharp
// Only maps BusinessRuleException → 400, everything else → 500
var problemDetails = exception switch
{
    BusinessRuleException bre => new ProblemDetails { Status = 400, ... },
    _ => new ProblemDetails { Status = 500, ... }
};
```

❌ Missing 404 mapping — NotFoundException not defined as separate type.

### no-skills — Convention-based middleware (not IExceptionHandler)

```csharp
// Middleware/GlobalExceptionHandlerMiddleware.cs — legacy pattern
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }
}

// Registered via: app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

❌ Uses `RequestDelegate` middleware instead of `IExceptionHandler`, ✅ But maps Business (with custom status code), NotFoundException, and 500 correctly.

**Scores**: dotnet-artisan: **5**, dotnet-skills: **4**, managedcode: **5**, no-skills: **3**

**Verdict**: **dotnet-artisan** and **managedcode** both use the modern `IExceptionHandler` with proper exception-to-status mapping. **no-skills** uses the legacy middleware pattern.

---

## 11. Async Patterns & Cancellation [HIGH]

| Config | CancellationToken in Services | CancellationToken in Endpoints | Async Suffix | Sync-over-async |
|---|---|---|---|---|
| dotnet-artisan | ✅ All methods (`ct = default`) | ✅ All handlers | ✅ `*Async` | ✅ None |
| dotnet-skills | ❌ None | ❌ None | ✅ `*Async` | ✅ None |
| managedcode | ❌ None | ❌ None | ✅ `*Async` | ✅ None |
| no-skills | ❌ None | ❌ None | ✅ `*Async` | ✅ None |

```csharp
// dotnet-artisan — CancellationToken propagated through all layers
public async Task<MembershipPlanDto> GetByIdAsync(int id, CancellationToken ct = default)
{
    var plan = await db.MembershipPlans
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id, ct)  // ← ct forwarded to EF Core
        ?? throw new KeyNotFoundException(...);
}

// Endpoint handler also receives CancellationToken
group.MapGet("/{id:int}", async (int id, IBookingService service, CancellationToken ct) =>
    TypedResults.Ok(await service.GetByIdAsync(id, ct)));
```

```csharp
// no-skills — no CancellationToken anywhere
public async Task<MembershipPlanDto?> GetByIdAsync(int id)
{
    var plan = await _db.MembershipPlans.FirstOrDefaultAsync(p => p.Id == id);
    // No way to cancel long-running queries
}
```

**Scores**: dotnet-artisan: **5**, dotnet-skills: **2**, managedcode: **2**, no-skills: **2**

**Verdict**: **dotnet-artisan** is the only configuration that propagates `CancellationToken` through the full call chain (endpoint → service → EF Core). All others ignore cancellation entirely.

---

## 12. EF Core Best Practices [HIGH]

| Config | AsNoTracking | Fluent API | IEntityTypeConfiguration | Enum Conversion | Indexes |
|---|---|---|---|---|---|
| dotnet-artisan | ✅ 20+ instances | ✅ Inline in OnModelCreating | ❌ | ✅ `HasConversion<string>()` | Unique (Name, Email) |
| dotnet-skills | ✅ 20+ instances | ✅ Inline in OnModelCreating | ❌ | ❌ No enum conversion | Unique (Name, Email) |
| managedcode | ✅ 20+ instances | ✅ Separate config classes | ✅ `ApplyConfigurationsFromAssembly()` | ✅ `HasConversion<string>()` | Unique + composite + query indexes |
| no-skills | ❌ 0 instances | ✅ Inline | ❌ | ✅ `HasConversion<string>()` | Unique (Name, Email) |

```csharp
// managedcode — IEntityTypeConfiguration (best pattern)
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasIndex(b => new { b.ClassScheduleId, b.MemberId });  // composite index
        builder.HasIndex(b => b.Status);  // query index
        builder.Property(b => b.Status).HasConversion<string>();
    }
}
// Applied via: modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessDbContext).Assembly);

// no-skills — NO AsNoTracking on any read query
public async Task<IReadOnlyList<MembershipPlanDto>> GetAllActivePlansAsync()
{
    return await _db.MembershipPlans
        .Where(p => p.IsActive)  // ← Missing .AsNoTracking()
        .Select(p => ToDto(p))
        .ToListAsync();
}
```

**Scores**: dotnet-artisan: **4**, dotnet-skills: **4**, managedcode: **5**, no-skills: **3**

**Verdict**: **managedcode** has the best EF Core configuration with `IEntityTypeConfiguration`, composite indexes, and query indexes. **no-skills** is the weakest — zero `AsNoTracking()` calls across the entire codebase.

---

## 13. Service Abstraction & DI [HIGH]

All four configurations use the same pattern:

```csharp
// All configs — Interface + Implementation, Scoped lifetime
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IMemberService, MemberService>();
// ... 5 more services
```

All 7 services follow the `IService` / `Service` pattern with appropriate scoped lifetime.

**Scores**: dotnet-artisan: **5**, dotnet-skills: **5**, managedcode: **5**, no-skills: **5**

**Verdict**: Tie — all configurations implement proper interface-based DI registration.

---

## 14. Security Configuration [HIGH]

No configuration implements HSTS or HTTPS redirection:

```csharp
// None of the 4 configs include:
// app.UseHsts();
// app.UseHttpsRedirection();
```

All apps are configured for HTTP-only development serving (ports 5058–5243).

**Scores**: dotnet-artisan: **2**, dotnet-skills: **2**, managedcode: **2**, no-skills: **2**

**Verdict**: Tie — none implement production security headers. All rely on the default development-only configuration.

---

## 15. DTO Design [MEDIUM]

| Config | Type | Sealed | Immutability | Naming | Separation |
|---|---|---|---|---|---|
| dotnet-artisan | Sealed records | ✅ | ✅ Positional records | `*Dto` | ✅ Separate from entities |
| dotnet-skills | Records (output) + sealed classes (input) | ✅ | ✅ Records immutable, classes mutable | `*Dto` | ✅ |
| managedcode | Classes | ❌ | ❌ Mutable | `*Dto` | ✅ Per-entity folders |
| no-skills | Records | ❌ | ✅ Positional records | `*CreateDto` / `*Dto` | ✅ Single file |

```csharp
// dotnet-artisan — best: sealed records with validation
public sealed record CreateMemberDto(
    [Required, MaxLength(100)] string FirstName, ...);

// managedcode — classes, no validation on DTOs
public class CreateMemberDto { public string FirstName { get; set; } = string.Empty; }

// no-skills — records but no validation
public record MemberCreateDto(string FirstName, string LastName, ...);
```

**Scores**: dotnet-artisan: **5**, dotnet-skills: **4**, managedcode: **3**, no-skills: **4**

**Verdict**: **dotnet-artisan** uses sealed records with validation — the most idiomatic pattern. **no-skills** uses records (good) but without validation. **managedcode** uses mutable classes.

---

## 16. Sealed Types [MEDIUM]

| Config | Models | Services | DTOs | Middleware | Total Sealed |
|---|---|---|---|---|---|
| dotnet-artisan | ✅ All 7 | ✅ All 7 | ✅ All | ✅ | ~30+ |
| dotnet-skills | ✅ All 8 | ✅ All 7 | ✅ All | ✅ | ~32 |
| managedcode | ❌ 0 | ❌ 0 | ❌ 0 | ❌ 0 | 0 |
| no-skills | ❌ 0 | ❌ 0 | ❌ 0 | ❌ 0 | 0 |

```csharp
// dotnet-artisan / dotnet-skills
public sealed class MembershipPlanService(FitnessDbContext db) : IMembershipPlanService { }
public sealed class Booking { ... }

// managedcode / no-skills
public class MembershipPlanService : IMembershipPlanService { }
public class Booking { ... }
```

**Scores**: dotnet-artisan: **5**, dotnet-skills: **5**, managedcode: **1**, no-skills: **1**

**Verdict**: The skill-guided configurations (artisan, dotnet-skills) systematically seal all leaf classes. managedcode and no-skills leave everything unsealed, missing JIT devirtualization benefits.

---

## 17. Data Seeder Design [MEDIUM]

All configurations use the same pattern: a static `DataSeeder.SeedAsync()` method called from `Program.cs` with an idempotency check:

```csharp
// All configs
public static async Task SeedAsync(FitnessDbContext context)
{
    if (await context.MembershipPlans.AnyAsync()) return;
    // 3 plans, 8 members, 6-9 memberships, 4 instructors, 6 class types,
    // 12 schedules, 15-22 bookings
}
```

All include realistic data with varied states (active, expired, frozen memberships; confirmed, waitlisted, cancelled bookings).

**Scores**: dotnet-artisan: **4**, dotnet-skills: **4**, managedcode: **4**, no-skills: **4**

**Verdict**: Tie — all use a reasonable runtime seeder. None use `HasData()` in migrations (consistent with using `EnsureCreated`).

---

## 18. Structured Logging [MEDIUM]

All configurations inject `ILogger<T>` and use structured message templates:

```csharp
// Common pattern across all configs
_logger.LogInformation("Booking {BookingId} created for member {MemberId}. Status: {Status}",
    booking.Id, dto.MemberId, booking.Status);
```

None use `[LoggerMessage]` source generators (which CA1848 analyzer warnings suggest).

**Scores**: dotnet-artisan: **4**, dotnet-skills: **4**, managedcode: **4**, no-skills: **4**

**Verdict**: Tie — all use structured logging with named placeholders. None use high-performance source generators.

---

## 19. Nullable Reference Types [MEDIUM]

All four configurations enable NRT in `.csproj`:

```xml
<Nullable>enable</Nullable>
```

All properly annotate nullable navigation properties and optional fields with `?`.

**Scores**: dotnet-artisan: **5**, dotnet-skills: **5**, managedcode: **5**, no-skills: **5**

---

## 20. API Documentation [MEDIUM]

| Config | Endpoint Metadata | Tags | Summaries | Response Types |
|---|---|---|---|---|
| dotnet-artisan | `.WithTags()`, `.WithSummary()` | ✅ Per group | ✅ All endpoints | Via TypedResults |
| dotnet-skills | `.WithSummary()` | ✅ Per group | ✅ All endpoints | Via Results (less precise) |
| managedcode | `[ProducesResponseType]` | ✅ Via controller route | ✅ XML comments | ✅ Explicit attributes |
| no-skills | `[ProducesResponseType]` | ✅ Via controller route | ✅ XML comments | ✅ Explicit attributes |

**Scores**: dotnet-artisan: **5**, dotnet-skills: **4**, managedcode: **4**, no-skills: **4**

**Verdict**: dotnet-artisan's TypedResults automatically generate accurate OpenAPI schemas without manual `ProducesResponseType` attributes.

---

## 21. File Organization [MEDIUM]

| Config | Structure | Program.cs Size | Endpoint Organization |
|---|---|---|---|
| dotnet-artisan | Models/, DTOs/, Services/, **Endpoints/**, Data/, Middleware/ | ~25 lines (clean) | 7 extension method files |
| dotnet-skills | Models/ (with Enums/), DTOs/, Services/ (with Interfaces/), Data/, Middleware/ | ~283 lines (all endpoints inline) | None (all inline) |
| managedcode | Models/, DTOs/ (per-entity subfolders), Services/ (Interfaces/ + Implementations/), Controllers/, Data/ (with Configurations/), Middleware/ | ~75 lines | 7 controllers |
| no-skills | Models/, DTOs/ (single Dtos.cs), Services/, Controllers/, Data/, Middleware/ | ~65 lines | 7 controllers |

**Scores**: dotnet-artisan: **5**, dotnet-skills: **4**, managedcode: **5**, no-skills: **3**

**Verdict**: dotnet-artisan has the cleanest organization with a minimal Program.cs. managedcode has excellent per-entity DTO folders and separate EF configs. no-skills puts all DTOs in one file.

---

## 22. HTTP Test File Quality [MEDIUM]

| Config | Scenarios | Business Rule Tests | Error Cases | Completeness |
|---|---|---|---|---|
| dotnet-artisan | 40+ | ✅ Premium access, freeze duration, waitlist | ✅ | All endpoints |
| dotnet-skills | ~40 | ✅ Premium access, instructor conflicts | ✅ | All endpoints |
| managedcode | 40+ | ✅ Premium access, expired membership, freeze, waitlist | ✅ | All endpoints |
| no-skills | ~20+ | ✅ Premium access, frozen membership | ✅ | Most endpoints |

**Scores**: dotnet-artisan: **5**, dotnet-skills: **4**, managedcode: **5**, no-skills: **4**

---

## 23. Type Design & Resource Management [MEDIUM]

All configurations properly use enums instead of magic strings:

```csharp
// All configs define 5 enums
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }
public enum MembershipStatus { Active, Expired, Cancelled, Frozen }
```

dotnet-artisan uses `IReadOnlyList<T>` for pagination results. dotnet-skills returns `IEnumerable<T>`. managedcode and no-skills use `List<T>`.

**Scores**: dotnet-artisan: **5**, dotnet-skills: **4**, managedcode: **4**, no-skills: **4**

---

## 24. Code Standards Compliance [LOW]

| Config | File-scoped NS | PascalCase | Async suffix | Explicit modifiers |
|---|---|---|---|---|
| dotnet-artisan | ✅ | ✅ | ✅ | ✅ `public sealed` |
| dotnet-skills | ✅ | ✅ | ✅ | ✅ `public sealed` |
| managedcode | ✅ | ✅ | ✅ | ✅ `public` (no sealed) |
| no-skills | ✅ | ✅ | ✅ | ✅ `public` (no sealed) |

**Scores**: dotnet-artisan: **5**, dotnet-skills: **5**, managedcode: **4**, no-skills: **4**

---

## Weighted Summary

Weighted scoring: Critical ×3, High ×2, Medium ×1, Low ×0.5

| Dimension | Tier | Weight | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|---|
| Build & Run Success | CRITICAL | ×3 | 15 | 15 | 15 | 15 |
| Security Vulnerability Scan | CRITICAL | ×3 | 15 | 15 | 15 | 15 |
| Minimal API Architecture | CRITICAL | ×3 | **15** | 9 | 3 | 3 |
| Input Validation | CRITICAL | ×3 | 12 | 12 | 9 | 9 |
| NuGet & Package Discipline | CRITICAL | ×3 | 12 | 12 | 9 | 6 |
| EF Migration Usage | CRITICAL | ×3 | 6 | 6 | 6 | 6 |
| Business Logic Correctness | HIGH | ×2 | 10 | 10 | 10 | 10 |
| Prefer Built-in over 3rd Party | HIGH | ×2 | **8** | 4 | 4 | 4 |
| Modern C# Adoption | HIGH | ×2 | **10** | 6 | 8 | 4 |
| Error Handling & Middleware | HIGH | ×2 | 10 | 8 | 10 | 6 |
| Async Patterns & Cancellation | HIGH | ×2 | **10** | 4 | 4 | 4 |
| EF Core Best Practices | HIGH | ×2 | 8 | 8 | **10** | 6 |
| Service Abstraction & DI | HIGH | ×2 | 10 | 10 | 10 | 10 |
| Security Configuration | HIGH | ×2 | 4 | 4 | 4 | 4 |
| DTO Design | MEDIUM | ×1 | 5 | 4 | 3 | 4 |
| Sealed Types | MEDIUM | ×1 | 5 | 5 | 1 | 1 |
| Data Seeder Design | MEDIUM | ×1 | 4 | 4 | 4 | 4 |
| Structured Logging | MEDIUM | ×1 | 4 | 4 | 4 | 4 |
| Nullable Reference Types | MEDIUM | ×1 | 5 | 5 | 5 | 5 |
| API Documentation | MEDIUM | ×1 | 5 | 4 | 4 | 4 |
| File Organization | MEDIUM | ×1 | 5 | 4 | 5 | 3 |
| HTTP Test File Quality | MEDIUM | ×1 | 5 | 4 | 5 | 4 |
| Type Design | MEDIUM | ×1 | 5 | 4 | 4 | 4 |
| Code Standards | LOW | ×0.5 | 2.5 | 2.5 | 2 | 2 |
| **TOTAL** | | | **195.5** | **167.5** | **159** | **141** |

### Final Ranking

| Rank | Configuration | Score | Δ from #1 |
|---|---|---|---|
| 🥇 1st | **dotnet-artisan** | **195.5** | — |
| 🥈 2nd | **dotnet-skills** | **167.5** | −28 |
| 🥉 3rd | **managedcode-dotnet-skills** | **159** | −36.5 |
| 4th | **no-skills** | **141** | −54.5 |

---

## What All Versions Get Right

- ✅ **Build success**: All compile with 0 errors on .NET 10
- ✅ **No vulnerabilities**: All packages are CVE-free
- ✅ **Complete business logic**: All 12 business rules implemented correctly
- ✅ **Interface-based DI**: All use `AddScoped<IService, Service>()` pattern
- ✅ **RFC 7807 ProblemDetails**: All return standard error responses
- ✅ **Custom exceptions**: All define `BusinessRuleException` for domain rule violations
- ✅ **Nullable reference types**: All enable NRT project-wide
- ✅ **Structured logging**: All use `ILogger<T>` with named placeholders
- ✅ **SQLite + EF Core**: All properly configure DbContext with connection strings
- ✅ **Comprehensive seed data**: All seed realistic test data with idempotency
- ✅ **Enum-based state machines**: All use proper enums (BookingStatus, MembershipStatus, etc.)
- ✅ **File-scoped namespaces**: All use the modern `namespace X;` syntax
- ✅ **Async/await throughout**: No sync-over-async patterns in any configuration
- ✅ **Comprehensive HTTP test files**: All include `.http` files covering major endpoints

---

## Summary: Impact of Skills

### Most Impactful Differences (ranked by weighted score delta)

1. **Minimal API Architecture** (−12 pts spread): dotnet-artisan's skill chain explicitly guides toward Minimal APIs with route groups, TypedResults, and endpoint extension methods. This is the single largest differentiator — it requires specific skill guidance to avoid the Controller pattern that LLMs default to.

2. **CancellationToken Propagation** (−6 pts spread): Only dotnet-artisan propagates CancellationToken from endpoints through services to EF Core calls. This requires explicit skill guidance in `async-patterns.md`.

3. **Sealed Types** (−4 pts spread): dotnet-artisan's `coding-standards.md` and dotnet-skills' `analyzing-dotnet-performance` both enforce sealing all leaf classes. Without skill guidance, no configuration seals anything.

4. **Modern C# Features** (−6 pts spread): Primary constructors and collection expressions require skill prompting — baseline Copilot defaults to traditional patterns.

5. **Built-in OpenAPI** (−6 pts spread): Only dotnet-artisan correctly uses `AddOpenApi()` + `MapOpenApi()` instead of Swashbuckle for document generation.

### Per-Configuration Assessment

**🥇 dotnet-artisan (195.5 pts)** — The comprehensive skill chain produces the most modern, well-architected code. Its `dotnet-api/minimal-apis.md` drives the correct Minimal API architecture with TypedResults and endpoint extension methods. `dotnet-csharp/async-patterns.md` ensures CancellationToken propagation. `dotnet-csharp/coding-standards.md` enforces sealed types and modern C# syntax. The only weaknesses are shared across all configs: EnsureCreated instead of Migrate, and no HSTS/HTTPS.

**🥈 dotnet-skills (167.5 pts)** — The performance-focused skills (`analyzing-dotnet-performance` + `optimizing-ef-core-queries`) add significant value through sealed types, `AsNoTracking()`, `EF.Functions.Like()`, and `StringComparison.OrdinalIgnoreCase`. However, without architectural guidance, the generated code uses Minimal APIs but misses TypedResults, puts all endpoints inline in Program.cs, uses traditional constructors, and omits CancellationToken.

**🥉 managedcode-dotnet-skills (159 pts)** — The community skills improve EF Core configuration (best `IEntityTypeConfiguration` usage with composite indexes) and enable modern C# features (primary constructors, collection expressions). However, they guide toward Controllers instead of Minimal APIs, don't seal types, and include an unused FluentValidation dependency. The 3 custom exception types provide the most granular error handling.

**4th no-skills (141 pts)** — The baseline produces functionally correct code but with dated patterns: traditional constructors, Controllers, no AsNoTracking, no sealed types, wildcard package versions, and legacy middleware-based error handling. This represents what default Copilot generates without specialized .NET guidance.

### Key Takeaway

**Skills have a measurable, compounding impact.** The 54.5-point gap between dotnet-artisan and no-skills (a 39% improvement) comes from architectural patterns (Minimal APIs), performance practices (sealed types, AsNoTracking, CancellationToken), and modern C# adoption (primary constructors, collection expressions, TypedResults). These are precisely the areas where LLMs need domain-specific guidance to override their training-data defaults.
