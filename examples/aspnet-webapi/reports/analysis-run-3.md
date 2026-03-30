# Comparative Analysis: no-skills, dotnet-artisan, dotnet-skills, managedcode-dotnet-skills

## Introduction

This report analyzes the **VetClinicApi** scenario (veterinary clinic management system) generated across **4 Copilot configurations** in run-3. Each configuration produced a standalone ASP.NET Core Web API targeting .NET 10 with EF Core and SQLite. The `dotnet-webapi` configuration was not present in this run.

| Configuration | Directory | Description |
|---|---|---|
| **no-skills** | `output/no-skills/run-3/VetClinicApi/` | Baseline — default Copilot with no skills |
| **dotnet-artisan** | `output/dotnet-artisan/run-3/VetClinicApi/` | dotnet-artisan plugin chain (using-dotnet → dotnet-advisor → dotnet-csharp → dotnet-api) |
| **dotnet-skills** | `output/dotnet-skills/run-3/VetClinicApi/` | Official .NET Skills (optimizing-ef-core-queries + analyzing-dotnet-performance) |
| **managedcode-dotnet-skills** | `output/managedcode-dotnet-skills/run-3/VetClinicApi/` | Community managed-code skills (dotnet-aspnet-core + dotnet-entity-framework-core) |

All four produce a working VetClinicApi with 7 entity models, 7 service pairs, seed data, and an HTTP test file.

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-artisan | dotnet-skills | managedcode |
|---|---|---|---|---|
| Minimal API Architecture [CRITICAL] | 1 | 5 | 1 | 1 |
| NuGet & Package Discipline [CRITICAL] | 1 | 5 | 3 | 4 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 4 | 5 | 3 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 |
| Prefer Built-in over 3rd Party [HIGH] | 2 | 5 | 1 | 2 |
| Modern C# Adoption [HIGH] | 2 | 5 | 2 | 3 |
| Error Handling & Middleware [HIGH] | 4 | 4 | 2 | 4 |
| Async Patterns & Cancellation [HIGH] | 2 | 5 | 2 | 2 |
| EF Core Best Practices [HIGH] | 3 | 5 | 4 | 3 |
| Service Abstraction & DI [HIGH] | 3 | 5 | 3 | 3 |
| Security Configuration [HIGH] | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 4 | 5 | 4 | 4 |
| Sealed Types [MEDIUM] | 1 | 5 | 3 | 1 |
| DTO Design [MEDIUM] | 2 | 5 | 2 | 2 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 3 | 5 | 3 | 3 |
| File Organization [MEDIUM] | 3 | 5 | 3 | 3 |
| HTTP Test File Quality [MEDIUM] | 4 | 4 | 4 | 4 |
| Type Design & Resource Mgmt [MEDIUM] | 3 | 4 | 3 | 3 |
| Code Standards Compliance [LOW] | 3 | 5 | 3 | 3 |

---

## 1. Minimal API Architecture [CRITICAL]

### What Each Configuration Does

**dotnet-artisan** uses full Minimal APIs with route groups, TypedResults, and union return types:

```csharp
// dotnet-artisan: Endpoints/OwnerEndpoints.cs
public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/owners").WithTags("Owners");
        group.MapGet("/", GetAll).WithSummary("List all owners with search and pagination");
        group.MapGet("/{id:int}", GetById).WithSummary("Get owner by ID with pets");
        group.MapPost("/", Create).WithSummary("Create a new owner");
        // ...
        return group;
    }

    private static async Task<Results<Ok<OwnerResponse>, NotFound>> GetById(
        int id, IOwnerService service, CancellationToken ct = default)
    {
        var owner = await service.GetByIdAsync(id, ct);
        return owner is not null ? TypedResults.Ok(owner) : TypedResults.NotFound();
    }
}
```

**no-skills**, **dotnet-skills**, and **managedcode** all use traditional controllers:

```csharp
// no-skills, dotnet-skills, managedcode: Controllers/OwnersController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OwnersController : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OwnerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **1** | Controllers only — no MapGroup, no TypedResults, no union types |
| dotnet-artisan | **5** | Full Minimal APIs: MapGroup, TypedResults, Results\<T1,T2\> unions, endpoint extension methods |
| dotnet-skills | **1** | Controllers only — `app.MapControllers()` with `[ApiController]` |
| managedcode | **1** | Controllers only — `app.MapControllers()` with `[ApiController]` |

**Verdict**: **dotnet-artisan** is the only configuration that uses Minimal APIs — the modern .NET standard. Route groups keep `Program.cs` clean, `TypedResults` provides compile-time type safety, and `Results<T1,T2>` union types enable automatic OpenAPI schema generation without `[ProducesResponseType]` attributes.

---

## 2. NuGet & Package Discipline [CRITICAL]

### What Each Configuration Does

```xml
<!-- no-skills: WILDCARD VERSIONS -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />

<!-- dotnet-artisan: EXACT PREVIEW VERSIONS, NO SWASHBUCKLE -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0-preview.3.25172.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-preview.3.25171.5" />
<PackageReference Include="Scalar.AspNetCore" Version="2.0.36" />

<!-- dotnet-skills: EXACT VERSIONS + 3RD-PARTY -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />

<!-- managedcode: EXACT VERSIONS + SWASHBUCKLE -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **1** | Uses `Version="10.*"` wildcards for EF Core — non-reproducible builds, potential vulnerability exposure |
| dotnet-artisan | **5** | Exact versions on all 4 packages, no unnecessary dependencies |
| dotnet-skills | **3** | Exact versions but includes deprecated `FluentValidation.AspNetCore` and Swashbuckle (5 packages) |
| managedcode | **4** | Exact versions, 4 packages, includes Swashbuckle alongside built-in OpenAPI |

**Verdict**: **dotnet-artisan** has the cleanest dependency graph — 4 packages, all pinned, no Swashbuckle. **no-skills** is the worst with wildcard versions that can pull in breaking changes or vulnerabilities.

---

## 3. Input Validation & Guard Clauses [CRITICAL]

### What Each Configuration Does

**dotnet-skills** uses dual-layer validation with FluentValidation + Data Annotations:

```csharp
// dotnet-skills: Validators/Validators.cs
public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentDto>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.PetId).GreaterThan(0);
        RuleFor(x => x.AppointmentDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Appointment date must be in the future.");
        RuleFor(x => x.DurationMinutes).InclusiveBetween(15, 120);
    }
}

// Cross-field validation
public class CreateVaccinationValidator : AbstractValidator<CreateVaccinationDto>
{
    public CreateVaccinationValidator()
    {
        RuleFor(x => x.ExpirationDate)
            .GreaterThan(x => x.DateAdministered)
            .WithMessage("Expiration date must be after date administered.");
    }
}
```

**dotnet-artisan**, **no-skills**, and **managedcode** use Data Annotations only on DTOs plus guard clauses in services:

```csharp
// dotnet-artisan: DTOs/PetDtos.cs
public sealed record CreatePetRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;
    [Range(0.01, double.MaxValue, ErrorMessage = "Weight must be positive")]
    public decimal? Weight { get; init; }
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **3** | Data annotations on DTOs, guard clauses via custom exceptions in services |
| dotnet-artisan | **4** | Data annotations + comprehensive guard clauses with tuple error returns |
| dotnet-skills | **5** | FluentValidation with cross-field rules + Data annotations + service-level guards |
| managedcode | **3** | Data annotations on DTOs, guard clauses via custom exceptions |

**Verdict**: **dotnet-skills** has the most comprehensive validation with FluentValidation providing cross-field validation, range checks, and custom error messages. **dotnet-artisan** uses a cleaner tuple-based error return pattern in services.

---

## 4. EF Migration Usage [CRITICAL]

### What Each Configuration Does

All four configurations use the same anti-pattern:

```csharp
// ALL CONFIGS: Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetClinicDbContext>();
    await db.Database.EnsureCreatedAsync();  // ← Anti-pattern
    await DataSeeder.SeedAsync(db);
}
```

None use `Database.MigrateAsync()` or have any migration files.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **1** | Uses `EnsureCreatedAsync()` — bypasses migrations entirely |
| dotnet-artisan | **1** | Uses `EnsureCreatedAsync()` — same anti-pattern despite including EF Design package |
| dotnet-skills | **1** | Uses `EnsureCreatedAsync()` — same anti-pattern |
| managedcode | **1** | Uses `EnsureCreatedAsync()` — same anti-pattern |

**Verdict**: All configurations fail this dimension. `EnsureCreated` cannot evolve schemas safely and will cause data loss on model changes. The correct approach is `Database.MigrateAsync()` with migration files.

---

## 5. Prefer Built-in over 3rd Party [HIGH]

### What Each Configuration Does

```csharp
// dotnet-artisan: Program.cs — built-in OpenAPI + Scalar UI
builder.Services.AddOpenApi("v1", options => { /* document transformer */ });
app.MapOpenApi();
app.MapScalarApiReference();  // Scalar replaces Swagger UI

// dotnet-skills: Program.cs — Swashbuckle + FluentValidation
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationAutoValidation();
app.UseSwagger();
app.UseSwaggerUI();

// no-skills & managedcode: dual OpenAPI + Swashbuckle
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
app.UseSwagger();
app.UseSwaggerUI();
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **2** | Uses Swashbuckle alongside AddOpenApi — redundant; uses System.Text.Json (good) |
| dotnet-artisan | **5** | No Swashbuckle, no FluentValidation — fully built-in OpenAPI with Scalar UI |
| dotnet-skills | **1** | Both Swashbuckle AND deprecated FluentValidation.AspNetCore; traditional middleware |
| managedcode | **2** | Uses Swashbuckle alongside built-in OpenAPI — redundant but not deprecated |

**Verdict**: **dotnet-artisan** exclusively uses built-in .NET capabilities. **dotnet-skills** is worst — `FluentValidation.AspNetCore` is deprecated and Swashbuckle duplicates built-in OpenAPI.

---

## 6. Modern C# Adoption [HIGH]

### What Each Configuration Does

**dotnet-artisan** uses primary constructors, collection expressions, and sealed records:

```csharp
// dotnet-artisan: Services/OwnerService.cs — primary constructor
public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService

// dotnet-artisan: Models/Owner.cs — collection expressions
public ICollection<Pet> Pets { get; set; } = [];

// dotnet-artisan: DTOs/OwnerDtos.cs — sealed record with init properties
public sealed record CreateOwnerRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;
}
```

**Other configs** use traditional patterns:

```csharp
// no-skills, dotnet-skills: Services/OwnerService.cs — traditional constructor
public class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<OwnerService> _logger;
    public OwnerService(VetClinicDbContext context, ILogger<OwnerService> logger)
    {
        _context = context;
        _logger = logger;
    }
}

// no-skills, dotnet-skills: Models/Owner.cs — old collection init
public ICollection<Pet> Pets { get; set; } = new List<Pet>();
```

**managedcode** partially adopts modern patterns (collection expressions on some models):

```csharp
// managedcode: Models/Owner.cs — modern collection expression
public ICollection<Pet> Pets { get; set; } = [];
// BUT: traditional constructors in services
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **2** | `new List<T>()`, traditional constructors, class DTOs, expression-bodied controller methods |
| dotnet-artisan | **5** | Primary constructors, `= []`, sealed records, `init` properties, target-typed `new()` |
| dotnet-skills | **2** | `new List<T>()`, traditional constructors, class DTOs |
| managedcode | **3** | `= []` on models, but traditional constructors and mutable class DTOs |

**Verdict**: **dotnet-artisan** fully embraces C# 12+ features. **managedcode** partially adopts collection expressions. The others remain on older patterns.

---

## 7. Error Handling & Middleware [HIGH]

### What Each Configuration Does

**no-skills** and **managedcode** use modern `IExceptionHandler` with two ordered handlers:

```csharp
// no-skills & managedcode: Program.cs
builder.Services.AddExceptionHandler<BusinessExceptionHandler>();  // Specific first
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();    // Catch-all second
builder.Services.AddProblemDetails();
```

**dotnet-artisan** uses `IExceptionHandler` with a single generic handler (specific errors handled in endpoints):

```csharp
// dotnet-artisan: Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Errors returned via tuple pattern in endpoints, not thrown
private static async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> Delete(...)
{
    if (hasActivePets)
        return TypedResults.Conflict(new ProblemDetails { ... });
}
```

**dotnet-skills** uses legacy `RequestDelegate` middleware — NOT `IExceptionHandler`:

```csharp
// dotnet-skills: Middleware/GlobalExceptionHandlerMiddleware.cs
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (BusinessRuleException ex) { await WriteErrorResponse(...); }
        catch (NotFoundException ex) { await WriteErrorResponse(...); }
        catch (Exception ex) { await WriteErrorResponse(...); }
    }
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **4** | `IExceptionHandler`, two handlers in correct order, ProblemDetails, custom exceptions |
| dotnet-artisan | **4** | `IExceptionHandler`, single handler, ProblemDetails, tuple-based error flow in endpoints |
| dotnet-skills | **2** | Legacy `RequestDelegate` middleware — not `IExceptionHandler`; ProblemDetails present |
| managedcode | **4** | `IExceptionHandler`, two ordered handlers, ProblemDetails, custom exceptions |

**Verdict**: **no-skills**, **dotnet-artisan**, and **managedcode** all use the modern `IExceptionHandler` pattern. **dotnet-skills** uses the older middleware approach despite its performance-focused skills.

---

## 8. Async Patterns & Cancellation [HIGH]

### What Each Configuration Does

**dotnet-artisan** propagates `CancellationToken` through all layers:

```csharp
// dotnet-artisan: Endpoints/OwnerEndpoints.cs
private static async Task<Ok<PagedResult<OwnerResponse>>> GetAll(
    IOwnerService service,
    [FromQuery] string? search = null,
    CancellationToken ct = default)  // ← Token from HTTP pipeline
{
    var result = await service.GetAllAsync(search, page, pageSize, ct);
    return TypedResults.Ok(result);
}

// dotnet-artisan: Services/OwnerService.cs
public async Task<PagedResult<OwnerResponse>> GetAllAsync(
    string? search, int page, int pageSize, CancellationToken ct = default)
{
    var totalCount = await query.CountAsync(ct);  // ← Token forwarded to EF Core
    var items = await query.ToListAsync(ct);       // ← Token forwarded
}
```

**All other configs** omit `CancellationToken` entirely:

```csharp
// no-skills, dotnet-skills, managedcode: Services/OwnerService.cs
public async Task<PaginatedResponse<OwnerResponseDto>> GetAllAsync(
    string? search, int page, int pageSize)  // ← No CancellationToken
{
    var totalCount = await query.CountAsync();    // ← No token
    var items = await query.ToListAsync();         // ← No token
}
```

| Config | CancellationToken occurrences in services |
|---|---|
| no-skills | **0** |
| dotnet-artisan | **70+** (all interfaces and implementations) |
| dotnet-skills | **0** |
| managedcode | **0** |

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **2** | All async/await, no sync-over-async, but zero CancellationToken usage |
| dotnet-artisan | **5** | Full CancellationToken chain from endpoint through service to EF Core |
| dotnet-skills | **2** | All async/await, but zero CancellationToken propagation |
| managedcode | **2** | All async/await, but zero CancellationToken propagation |

**Verdict**: **dotnet-artisan** is the only configuration that properly propagates `CancellationToken`. This prevents wasted server resources when clients disconnect — critical for production APIs.

---

## 9. EF Core Best Practices [HIGH]

### What Each Configuration Does

**dotnet-artisan** and **dotnet-skills** consistently use `AsNoTracking()` for read queries:

```csharp
// dotnet-artisan & dotnet-skills: Services/OwnerService.cs
var query = db.Owners.AsNoTracking().AsQueryable();

// dotnet-skills additionally uses AsSplitQuery for multi-Include
var appointment = await _context.Appointments
    .AsNoTracking()
    .AsSplitQuery()
    .Include(a => a.Pet)
    .Include(a => a.Veterinarian)
    .Include(a => a.MedicalRecord)
        .ThenInclude(m => m.Prescriptions)
    .FirstOrDefaultAsync(a => a.Id == id);
```

**no-skills** and **managedcode** never use `AsNoTracking()`:

```csharp
// no-skills & managedcode: Services/OwnerService.cs
var query = _context.Owners.AsQueryable();  // ← Tracked by default
```

All configs use Fluent API with explicit relationship configuration, `HasConversion<string>()` for AppointmentStatus, unique indexes, and `OnDelete(DeleteBehavior.Restrict)` where appropriate.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **3** | Fluent API, relationships, unique indexes, Restrict — but no AsNoTracking |
| dotnet-artisan | **5** | AsNoTracking everywhere, explicit relationships, Restrict/Cascade, Ignore for computed, timestamp override |
| dotnet-skills | **4** | AsNoTracking + AsSplitQuery, Fluent API, proper relationships |
| managedcode | **3** | Fluent API, relationships, Restrict — but no AsNoTracking |

**Verdict**: **dotnet-artisan** has the most thorough EF Core configuration. **dotnet-skills** adds valuable `AsSplitQuery()` to prevent cartesian explosion. **no-skills** and **managedcode** leave performance on the table by not using `AsNoTracking()`.

---

## 10. Service Abstraction & DI [HIGH]

### What Each Configuration Does

All four configurations use interface-based services with `AddScoped<IService, Service>()`:

```csharp
// ALL CONFIGS: Program.cs
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IPetService, PetService>();
// ... 7 total services
```

**dotnet-artisan** uses primary constructors for cleaner DI:

```csharp
// dotnet-artisan: Services/OwnerService.cs
public sealed class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService
```

**Others** use traditional constructor injection:

```csharp
// no-skills, managedcode: Services/OwnerService.cs
public class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<OwnerService> _logger;
    public OwnerService(VetClinicDbContext context, ILogger<OwnerService> logger) { ... }
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **3** | Interface-based, scoped, traditional constructors |
| dotnet-artisan | **5** | Interface-based, scoped, primary constructors, sealed services |
| dotnet-skills | **3** | Interface-based, scoped, sealed services but traditional constructors |
| managedcode | **3** | Interface-based, scoped, traditional constructors, not sealed |

**Verdict**: **dotnet-artisan** is cleanest with primary constructors eliminating boilerplate. All configs follow proper DI patterns.

---

## 11. Security Configuration [HIGH]

### What Each Configuration Does

**None** of the configurations include HSTS or HTTPS redirection:

```csharp
// NONE of the configs have:
if (!app.Environment.IsDevelopment()) { app.UseHsts(); }
app.UseHttpsRedirection();
```

Zero matches across all 4 `Program.cs` files for `UseHsts` or `UseHttpsRedirection`.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **1** | No HSTS, no HTTPS redirection, no CORS |
| dotnet-artisan | **1** | No HSTS, no HTTPS redirection, no CORS |
| dotnet-skills | **1** | No HSTS, no HTTPS redirection, no CORS |
| managedcode | **1** | No HSTS, no HTTPS redirection, no CORS |

**Verdict**: All configurations fail security configuration. This is a universal gap regardless of skill configuration.

---

## 12. Business Logic Correctness [HIGH]

### What Each Configuration Does

All four configurations implement the core business rules:

- **Appointment scheduling conflict detection** (time overlap with excluded cancelled/no-show)
- **Status workflow validation** (state machine with Dictionary lookup)
- **Cancellation rules** (requires reason, cannot cancel past appointments)
- **Medical record creation** (only for Completed/InProgress appointments)
- **Prescription EndDate computation** (`StartDate.AddDays(DurationDays)`)
- **Vaccination tracking** (IsExpired, IsDueSoon computed properties)
- **Soft delete for pets** (IsActive flag, filtered in list queries)

**dotnet-artisan** includes all 31 specified endpoints. Other configs include 28-30+, with minor variations in endpoint organization.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **4** | All business rules implemented, all endpoints present, comprehensive error responses |
| dotnet-artisan | **5** | All business rules, all endpoints, clean error flow via tuples instead of exceptions |
| dotnet-skills | **4** | All business rules, all endpoints, FluentValidation adds extra validation layer |
| managedcode | **4** | All business rules, all endpoints, exception-based error flow |

**Verdict**: All configs implement business rules correctly. **dotnet-artisan** edges ahead with complete endpoint coverage and a non-exception error flow pattern.

---

## 13. Sealed Types [MEDIUM]

### What Each Configuration Does

```csharp
// dotnet-artisan: ALL types sealed
public sealed class Owner { ... }
public sealed class OwnerService(...) : IOwnerService { ... }
public sealed record OwnerResponse(...);
public sealed class GlobalExceptionHandler(...) : IExceptionHandler { ... }
public sealed class VetClinicDbContext(...) : DbContext(...) { ... }

// dotnet-skills: Services sealed, models/DTOs not
public sealed class OwnerService : IOwnerService { ... }
public class Owner { ... }  // NOT sealed
public class OwnerResponseDto { ... }  // NOT sealed

// no-skills & managedcode: NOTHING sealed
public class OwnerService : IOwnerService { ... }
public class Owner { ... }
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **1** | Zero sealed types anywhere |
| dotnet-artisan | **5** | All models, DTOs, services, DbContext, and middleware sealed |
| dotnet-skills | **3** | Services sealed (7/7), but models and DTOs not sealed |
| managedcode | **1** | Zero sealed types anywhere |

**Verdict**: **dotnet-artisan** consistently seals all types. **dotnet-skills** seals services only (influenced by the performance analysis skill). Sealing enables JIT devirtualization and signals design intent.

---

## 14. DTO Design [MEDIUM]

### What Each Configuration Does

**dotnet-artisan** uses sealed records with `init` properties and `*Request/*Response` naming:

```csharp
// dotnet-artisan: DTOs/OwnerDtos.cs
public sealed record OwnerResponse(
    int Id, string FirstName, string LastName, string Email, string Phone,
    string? Address, string? City, string? State, string? ZipCode,
    DateTime CreatedAt, DateTime UpdatedAt, List<PetSummaryResponse>? Pets = null);

public sealed record CreateOwnerRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;
}

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
```

**All other configs** use mutable classes with `*Dto` naming:

```csharp
// no-skills, dotnet-skills, managedcode: DTOs/OwnerDtos.cs
public class CreateOwnerDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;  // ← mutable
}

public class OwnerResponseDto
{
    public int Id { get; set; }  // ← mutable
    public List<PetSummaryDto> Pets { get; set; } = new();
}
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **2** | Mutable classes, `*Dto` naming, `{ get; set; }` |
| dotnet-artisan | **5** | Sealed records, `init` properties, `*Request/*Response` naming, `IReadOnlyList<T>` |
| dotnet-skills | **2** | Mutable classes, `*Dto` naming |
| managedcode | **2** | Mutable classes, `*Dto` naming |

**Verdict**: **dotnet-artisan** produces immutable, type-safe DTOs that prevent over-posting and accidental mutation. The `*Request/*Response` naming convention is clearer than `*Dto`.

---

## 15. Data Seeder Design [MEDIUM]

All configs use an identical runtime seeding pattern with idempotency check:

```csharp
// ALL CONFIGS: Data/DataSeeder.cs
public static class DataSeeder
{
    public static async Task SeedAsync(VetClinicDbContext db)
    {
        if (await db.Owners.AnyAsync()) return;  // Idempotent
        // 5 owners, 8 pets, 3 vets, 10 appointments, 4 medical records, 5 prescriptions, 6 vaccinations
    }
}
```

All include realistic data with various states (completed, cancelled, scheduled appointments; active, expired, due-soon vaccinations).

### Scores

All configs score **4** — comprehensive runtime seeding with idempotency, but not using `HasData()` in migrations (which would be ideal but requires migrations first).

---

## 16. Structured Logging [MEDIUM]

All configs use `ILogger<T>` with structured message templates:

```csharp
// ALL CONFIGS use structured placeholders
logger.LogInformation("Owner created: {OwnerId} {FirstName} {LastName}", owner.Id, ...);
logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, status);
```

All use `LogInformation` for CRUD operations and `LogError` for exception handling. No config uses `[LoggerMessage]` source generators.

### Scores

All configs score **4** — proper structured logging with named placeholders, but no high-performance source generators.

---

## 17. Nullable Reference Types [MEDIUM]

All configs enable NRTs in `.csproj`:

```xml
<Nullable>enable</Nullable>
```

All use `?` annotations correctly for optional properties and `= null!` for required navigation properties:

```csharp
public string? Address { get; set; }           // Optional
public Owner Owner { get; set; } = null!;       // Required navigation
public MedicalRecord? MedicalRecord { get; set; } // Optional navigation
```

### Scores

All configs score **4** — NRTs enabled with generally correct annotations, minor `null!` usage on required navigations.

---

## 18. API Documentation [MEDIUM]

**dotnet-artisan** uses `WithSummary()`, `WithTags()`, and `WithName()`:

```csharp
// dotnet-artisan: Endpoints/OwnerEndpoints.cs
group.MapGet("/", GetAll).WithSummary("List all owners with search and pagination");
group.MapGet("/{id:int}", GetById).WithSummary("Get owner by ID with pets");
// Tags set at group level: .WithTags("Owners")
```

**Other configs** use `[ProducesResponseType]` and XML comments:

```csharp
// no-skills: Controllers/OwnersController.cs
/// <summary>List all owners with optional search and pagination</summary>
[HttpGet]
[ProducesResponseType(typeof(PaginatedResponse<OwnerResponseDto>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetAll(...)
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **3** | `[ProducesResponseType]` attributes, XML comments |
| dotnet-artisan | **5** | `WithSummary()`, `WithTags()`, TypedResults auto-generate schemas — no manual attributes needed |
| dotnet-skills | **3** | `[ProducesResponseType]` attributes, SwaggerGen |
| managedcode | **3** | `[ProducesResponseType]` attributes, XML comments |

**Verdict**: **dotnet-artisan**'s `TypedResults` + union types auto-generate complete OpenAPI schemas without requiring manual `[ProducesResponseType]` attributes.

---

## 19. File Organization [MEDIUM]

**dotnet-artisan** has a dedicated `Endpoints/` directory with clean Program.cs:

```
src/VetClinicApi/
├── Program.cs              # Clean — just config + endpoint mapping calls
├── Endpoints/              # Endpoint extension methods (one per resource)
├── Models/                 # Entity classes
├── DTOs/                   # Request/response records
├── Services/               # Interface + implementation pairs
├── Data/                   # DbContext + DataSeeder
└── Middleware/              # GlobalExceptionHandler
```

**Other configs** use `Controllers/` with logic split between controllers and services:

```
src/VetClinicApi/
├── Program.cs              # Config + MapControllers()
├── Controllers/            # 7 controller classes
├── Models/
├── DTOs/
├── Services/
├── Data/
└── Middleware/
```

**dotnet-skills** additionally has a `Validators/` directory for FluentValidation rules.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **3** | Standard controller layout, clean separation |
| dotnet-artisan | **5** | Endpoints/ directory, Program.cs only has endpoint mapping calls |
| dotnet-skills | **3** | Controllers/ + Validators/ — additional directory |
| managedcode | **3** | Standard controller layout |

---

## 20. HTTP Test File Quality [MEDIUM]

All configs produce comprehensive `.http` files with ~30-50 test scenarios covering CRUD operations, status transitions, and filtered queries. All use `@baseUrl` variable and group requests by resource with comment headers.

### Scores

All configs score **4** — good coverage with realistic request bodies, FK consistency with seed data, and filter/pagination examples. None systematically test error cases (invalid input, 404s).

---

## 21. Type Design & Resource Management [MEDIUM]

All configs use `AppointmentStatus` as an enum with `HasConversion<string>()`:

```csharp
// ALL CONFIGS
public enum AppointmentStatus
{
    Scheduled, CheckedIn, InProgress, Completed, Cancelled, NoShow
}
```

**dotnet-artisan** uses `IReadOnlyList<T>` for pagination results:

```csharp
// dotnet-artisan
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
```

**Others** use `IEnumerable<T>` or `List<T>`:

```csharp
// Others
public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
```

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **3** | Enum for status, `IEnumerable<T>` return types |
| dotnet-artisan | **4** | Enum for status, `IReadOnlyList<T>`, computed properties |
| dotnet-skills | **3** | Enum for status, `IEnumerable<T>` return types, `PaginationParams` with clamping |
| managedcode | **3** | Enum for status, `IEnumerable<T>` return types |

---

## 22. Code Standards Compliance [LOW]

**dotnet-artisan** uses explicit access modifiers, `Async` suffix, and verb-phrase naming:

```csharp
// dotnet-artisan
public sealed class OwnerService(...) : IOwnerService
public async Task<PagedResult<OwnerResponse>> GetAllAsync(...)
public static RouteGroupBuilder MapOwnerEndpoints(this IEndpointRouteBuilder routes)
```

All configs use PascalCase for public members, camelCase for parameters, and file-scoped namespaces.

### Scores

| Config | Score | Justification |
|---|---|---|
| no-skills | **3** | Standard naming, but defaults on access modifiers |
| dotnet-artisan | **5** | Explicit modifiers, consistent `Async` suffix, sealed on everything |
| dotnet-skills | **3** | Standard naming, sealed services |
| managedcode | **3** | Standard naming, defaults on access modifiers |

---

## Weighted Summary

Weighted scoring: Critical × 3, High × 2, Medium × 1, Low × 0.5

| Dimension [Tier] | Weight | no-skills | dotnet-artisan | dotnet-skills | managedcode |
|---|---|---|---|---|---|
| Minimal API Architecture [C] | ×3 | 3 | 15 | 3 | 3 |
| NuGet & Package Discipline [C] | ×3 | 3 | 15 | 9 | 12 |
| Input Validation [C] | ×3 | 9 | 12 | 15 | 9 |
| EF Migration Usage [C] | ×3 | 3 | 3 | 3 | 3 |
| Prefer Built-in [H] | ×2 | 4 | 10 | 2 | 4 |
| Modern C# Adoption [H] | ×2 | 4 | 10 | 4 | 6 |
| Error Handling [H] | ×2 | 8 | 8 | 4 | 8 |
| Async & Cancellation [H] | ×2 | 4 | 10 | 4 | 4 |
| EF Core Best Practices [H] | ×2 | 6 | 10 | 8 | 6 |
| Service Abstraction [H] | ×2 | 6 | 10 | 6 | 6 |
| Security Configuration [H] | ×2 | 2 | 2 | 2 | 2 |
| Business Logic [H] | ×2 | 8 | 10 | 8 | 8 |
| Sealed Types [M] | ×1 | 1 | 5 | 3 | 1 |
| DTO Design [M] | ×1 | 2 | 5 | 2 | 2 |
| Data Seeder [M] | ×1 | 4 | 4 | 4 | 4 |
| Structured Logging [M] | ×1 | 4 | 4 | 4 | 4 |
| Nullable Reference Types [M] | ×1 | 4 | 4 | 4 | 4 |
| API Documentation [M] | ×1 | 3 | 5 | 3 | 3 |
| File Organization [M] | ×1 | 3 | 5 | 3 | 3 |
| HTTP Test File [M] | ×1 | 4 | 4 | 4 | 4 |
| Type Design [M] | ×1 | 3 | 4 | 3 | 3 |
| Code Standards [L] | ×0.5 | 1.5 | 2.5 | 1.5 | 1.5 |
| **TOTAL** | | **89.5** | **157.5** | **99.5** | **100.5** |

### Final Rankings

| Rank | Configuration | Weighted Score | Percentage of Max |
|---|---|---|---|
| 🥇 1st | **dotnet-artisan** | **157.5** | 86% |
| 🥉 3rd | **managedcode-dotnet-skills** | **100.5** | 55% |
| 4th | **dotnet-skills** | **99.5** | 54% |
| 5th | **no-skills** | **89.5** | 49% |

---

## What All Versions Get Right

- **Interface-based service layer** with `AddScoped<IService, Service>()` DI registration
- **Entity Framework Core** with SQLite and Fluent API configuration
- **AppointmentStatus enum** stored as string in database via `HasConversion<string>()`
- **Comprehensive business rules**: scheduling conflict detection, status workflow, cancellation rules, medical record constraints
- **Soft-delete pattern** for pets using `IsActive` flag
- **Computed properties** (`IsActive`, `IsExpired`, `IsDueSoon`) correctly ignored in EF mapping
- **One-to-one relationship** between Appointment and MedicalRecord with unique index
- **Data annotations** on all input DTOs (`[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`)
- **Structured logging** with `ILogger<T>` and named message templates (`{OwnerId}`, `{Status}`)
- **Nullable reference types** enabled with correct annotations
- **Realistic seed data** covering edge cases (expired vaccinations, cancelled appointments, active/inactive prescriptions)
- **`DateOnly`** for date-only fields (DateOfBirth, HireDate, StartDate)
- **Pagination** with total count, page metadata, and HasNextPage/HasPreviousPage

---

## Summary: Impact of Skills

### Most Impactful Differences

1. **Minimal API Architecture** (dotnet-artisan only): The single largest differentiator. Only the dotnet-artisan plugin chain generates Minimal APIs with `MapGroup`, `TypedResults`, and `Results<T1,T2>` union types. This is worth 12 weighted points over all other configs.

2. **CancellationToken Propagation** (dotnet-artisan only): Zero other configurations propagate CancellationToken from endpoints to EF Core. This is a critical production concern that only the dotnet-artisan's `dotnet-api` skill addresses.

3. **AsNoTracking Usage** (dotnet-artisan + dotnet-skills): Both performance-focused skill sets correctly apply `AsNoTracking()` to read queries. The `optimizing-ef-core-queries` skill in dotnet-skills additionally adds `AsSplitQuery()`.

4. **Package Discipline** (dotnet-artisan best, no-skills worst): Wildcard versions in no-skills are a critical failure. dotnet-artisan avoids all unnecessary packages.

5. **Sealed Types** (dotnet-artisan best): Only dotnet-artisan consistently seals all types. dotnet-skills seals services only (from `analyzing-dotnet-performance`).

### Configuration Assessment

**dotnet-artisan (157.5 pts — 🥇)**: The clear winner. Its multi-skill chain (dotnet-csharp + dotnet-api) produces code that follows modern .NET best practices across the board — Minimal APIs, primary constructors, sealed types, CancellationToken, AsNoTracking, built-in OpenAPI, and immutable record DTOs. The only weaknesses are shared with all configs (no HSTS, `EnsureCreated`).

**managedcode-dotnet-skills (100.5 pts — 3rd)**: Solid fundamentals with `IExceptionHandler` and ordered exception handlers, but misses modern C# patterns and Minimal APIs. Very close to dotnet-skills. Some partial adoption of collection expressions (`= []`) suggests the community skill nudges toward modern patterns.

**dotnet-skills (99.5 pts — 4th)**: Its FluentValidation integration provides the best input validation, and `AsNoTracking`/`AsSplitQuery` demonstrate EF Core expertise. However, it loses heavily by using Swashbuckle, deprecated `FluentValidation.AspNetCore`, and legacy `RequestDelegate` middleware instead of `IExceptionHandler`. The performance skills are valuable but introduce third-party dependencies that conflict with .NET team guidelines.

**no-skills (89.5 pts — Last)**: The baseline is adequate but shows clear gaps — wildcard package versions, no `AsNoTracking`, no `CancellationToken`, no sealed types, no modern C# features. This represents what Copilot produces without any skill guidance.
