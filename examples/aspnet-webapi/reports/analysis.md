# Aggregated Analysis: ASP.NET Core Web API Skill Evaluation

**Runs:** 3 | **Configurations:** 5 | **Scenarios:** 3 | **Dimensions:** 25
**Date:** 2026-03-29 04:56 UTC

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 5.0 | 5.0 | 4.5 ± 0.7 | 1.0 | — |
| Security Vulnerability Scan [CRITICAL] | 3.5 ± 0.7 | 4.5 ± 0.7 | 4.0 ± 1.4 | 1.0 | — |
| Minimal API Architecture [CRITICAL] | 1.0 | 5.0 | 4.7 ± 0.6 | 1.0 | — |
| Input Validation & Guard Clauses [CRITICAL] | 3.3 ± 0.6 | 4.0 | 4.0 | 1.0 | — |
| NuGet & Package Discipline [CRITICAL] | 3.0 | 4.0 ± 1.0 | 4.3 ± 0.6 | 1.0 | — |
| EF Migration Usage [CRITICAL] | 1.7 ± 0.6 | 3.7 ± 1.5 | 1.7 ± 0.6 | 1.0 | — |
| Business Logic Correctness [HIGH] | 4.3 ± 0.6 | 4.3 ± 0.6 | 4.3 ± 0.6 | 1.0 | — |
| Prefer Built-in over 3rd Party [HIGH] | 1.7 ± 0.6 | 5.0 | 4.3 ± 1.2 | 1.0 | — |
| Modern C# Adoption [HIGH] | 2.0 | 4.7 ± 0.6 | 5.0 | 1.0 | — |
| Error Handling & Middleware [HIGH] | 3.3 ± 0.6 | 5.0 | 4.0 ± 1.0 | 1.0 | — |
| Async Patterns & Cancellation [HIGH] | 2.3 ± 0.6 | 5.0 | 5.0 | 1.0 | — |
| EF Core Best Practices [HIGH] | 2.7 ± 0.6 | 4.7 ± 0.6 | 4.3 ± 0.6 | 1.0 | — |
| Service Abstraction & DI [HIGH] | 3.7 ± 0.6 | 5.0 | 4.7 ± 0.6 | 1.0 | — |
| Security Configuration [HIGH] | 1.7 ± 0.6 | 1.7 ± 0.6 | 1.7 ± 0.6 | 1.0 | — |
| DTO Design [MEDIUM] | 2.0 | 5.0 | 5.0 | 1.0 | — |
| Sealed Types [MEDIUM] | 1.0 | 5.0 | 5.0 | 1.0 | — |
| Data Seeder Design [MEDIUM] | 3.7 ± 0.6 | 4.3 ± 0.6 | 3.7 ± 0.6 | 1.0 | — |
| Structured Logging [MEDIUM] | 3.3 ± 0.6 | 4.7 ± 0.6 | 4.7 ± 0.6 | 1.0 | — |
| Nullable Reference Types [MEDIUM] | 4.0 | 4.7 ± 0.6 | 4.7 ± 0.6 | 1.0 | — |
| API Documentation [MEDIUM] | 3.3 ± 0.6 | 5.0 | 4.3 ± 0.6 | 1.0 | — |
| File Organization [MEDIUM] | 3.3 ± 0.6 | 5.0 | 4.7 ± 0.6 | 1.0 | — |
| HTTP Test File Quality [MEDIUM] | 3.7 ± 0.6 | 4.7 ± 0.6 | 3.3 ± 2.1 | 1.0 | — |
| Type Design & Resource Mgmt [MEDIUM] | 3.0 | 4.5 ± 0.7 | 4.5 ± 0.7 | 1.0 | — |
| Code Standards Compliance [LOW] | 3.0 | 4.7 ± 0.6 | 4.7 ± 0.6 | 1.0 | — |
| Type Design & Resource Management [MEDIUM] | 3.0 | 5.0 | 5.0 | 1.0 | — |

---

## Final Rankings

| Rank | Configuration | Mean Weighted Score | Std Dev | Min | Max |
|---|---|---|---|---|---|
| 🥇 | dotnet-webapi | 185.0 | 13.1 | 170.0 | 194.5 |
| 🥈 | dotnet-artisan | 170.0 | 24.9 | 142.0 | 189.5 |
| 🥉 | no-skills | 116.2 | 20.5 | 92.5 | 128.5 |
| 4th | managedcode-dotnet-skills | 41.5 | 3.5 | 37.5 | 43.5 |
| 5th | dotnet-skills | 0.0 | 0.0 | 0.0 | 0.0 |

---

## Weighted Score per Run

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 128.5 | 194.5 | 178.5 | 43.5 | 0.0 |
| 2 | 127.5 | 190.5 | 189.5 | 43.5 | 0.0 |
| 3 | 92.5 | 170.0 | 142.0 | 37.5 | 0.0 |
| **Mean** | **116.2** | **185.0** | **170.0** | **41.5** | **0.0** |

---

## Verification Summary (All Runs)

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |
|---|---|---|---|
| no-skills | 3/9 (33%) | 3/9 (33%) | 35.6 |
| dotnet-webapi | 3/9 (33%) | 3/9 (33%) | 49.6 |
| dotnet-artisan | 2/9 (22%) | 2/9 (22%) | 39.1 |
| managedcode-dotnet-skills | 0/9 (0%) | 0/9 (0%) | 0.0 |
| dotnet-skills | 1/9 (11%) | 1/9 (11%) | 12.4 |

---

## Consistency Analysis

| Configuration | Score σ | Most Consistent Dim (σ) | Most Variable Dim (σ) |
|---|---|---|---|
| no-skills | 20.5 | Build & Run Success (0.0) | Security Vulnerability Scan (0.7) |
| dotnet-webapi | 13.1 | Build & Run Success (0.0) | EF Migration Usage (1.5) |
| dotnet-artisan | 24.9 | Input Validation & Guard Clauses (0.0) | HTTP Test File Quality (2.1) |
| managedcode-dotnet-skills | 3.5 | Build & Run Success (0.0) |  (0.0) |
| dotnet-skills | 0.0 |  (inf) |  (0.0) |

---

## Per-Dimension Analysis

### 1. Build & Run Success [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 4 | 1 | — |
| 2 | 5 | 5 | 5 | 1 | — |
| 3 | — | — | — | — | — |
| **Mean** | **5.0** | **5.0** | **4.5** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 4)
2 of 3 apps build and run successfully. VetClinicApi fails to compile. FitnessStudioApi has 114 warnings, LibraryApi has 162 warnings (primarily performance-related CA1862 async suggestions).

### dotnet-webapi (Score: 5)
All 3 apps build and run successfully. FitnessStudioApi: 130 warnings, LibraryApi: 182 warnings, VetClinicApi: 134 warnings. Despite higher warning counts, zero build errors.

### no-skills (Score: 5)
All 3 apps build and run successfully. FitnessStudioApi: 114 warnings, LibraryApi: 138 warnings, VetClinicApi: 68 warnings (fewest total).

### dotnet-skills (Score: 2)
Only 1 of 3 apps has actual code (VetClinicApi). FitnessStudioApi and LibraryApi directories contain only `Directory.Build.props` — no source code was generated. VetClinicApi builds with 112 warnings (14 naming issues).

### managedcode-dotnet-skills (Score: 1)
0 of 3 apps have any source code. Only `Directory.Build.props` files exist. Complete generation failure.

**Verdict**: **dotnet-webapi** and **no-skills** tie at 5 — both achieve 100% build success. dotnet-artisan is close behind. dotnet-skills and managedcode suffered catastrophic generation failures.

---

### 2. Security Vulnerability Scan [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 3 | 1 | — |
| 2 | 4 | 5 | 5 | 1 | — |
| 3 | — | — | — | — | — |
| **Mean** | **3.5** | **4.5** | **4.0** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 3)
FitnessStudioApi: 2 vulnerabilities (1 High, 1 Low). LibraryApi: 1 Low vulnerability. VetClinicApi: did not build. The High vulnerability on FitnessStudioApi is a concern.

### dotnet-webapi (Score: 4)
All 3 apps: 1 Low vulnerability each. Consistent, minimal exposure across all scenarios.

### no-skills (Score: 3)
All 3 apps: 1 Low vulnerability each. Same profile as dotnet-webapi, but the use of additional 3rd-party packages (Swashbuckle, FluentValidation) increases attack surface.

### dotnet-skills (Score: 3)
VetClinicApi (the only app that built): 1 Low vulnerability. Uses wildcard package versions (`10.0.0-*`) which could pull in vulnerable pre-release packages.

### managedcode-dotnet-skills (Score: 1)
No code to scan.

**Verdict**: **dotnet-webapi** leads with minimal vulnerability counts and the smallest dependency footprint. dotnet-artisan's High vulnerability on FitnessStudio is a notable risk. Wildcard versions in dotnet-skills are a latent threat.

---

### 3. Minimal API Architecture [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 5 | 1 | — |
| 2 | 1 | 5 | 4 | 1 | — |
| 3 | 1 | 5 | 5 | 1 | — |
| **Mean** | **1.0** | **5.0** | **4.7** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
Consistently uses Minimal APIs with `MapGroup()`, `TypedResults`, and endpoint extension methods across all apps.
```csharp
// dotnet-artisan: FitnessStudioApi/Endpoints/MembershipPlanEndpoints.cs
public static void MapMembershipPlanEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/membership-plans").WithTags("Membership Plans");
    group.MapGet("/", async (IMembershipPlanService service, CancellationToken ct) =>
        TypedResults.Ok(await service.GetAllAsync(ct)));
}
```
Program.cs is clean — endpoint registration is delegated:
```csharp
// dotnet-artisan: Program.cs
app.MapMembershipPlanEndpoints();
app.MapMemberEndpoints();
```

### dotnet-webapi (Score: 5)
Identical pattern — Minimal APIs with `MapGroup()`, `TypedResults`, and extension methods. Also includes rich OpenAPI metadata:
```csharp
// dotnet-webapi: FitnessStudioApi/Endpoints/MembershipPlanEndpoints.cs
group.MapGet("/", async (IMembershipPlanService service, CancellationToken ct) =>
{
    var plans = await service.GetAllAsync(ct);
    return TypedResults.Ok(plans);
})
.WithName("GetMembershipPlans")
.WithSummary("List all active membership plans")
.Produces<IReadOnlyList<MembershipPlanResponse>>();
```

### no-skills (Score: 1)
Uses traditional Controllers with `[ApiController]` attribute across all apps:
```csharp
// no-skills: FitnessStudioApi/Controllers/MembersController.cs
[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(...) { ... }
}
```
Program.cs uses `app.MapControllers()` — the older pattern.

### dotnet-skills (Score: 1)
VetClinicApi also uses Controllers:
```csharp
// dotnet-skills: VetClinicApi/Controllers/OwnersController.cs
[ApiController]
[Route("api/[controller]")]
public class OwnersController : ControllerBase { ... }
```

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** both achieve the modern ideal — Minimal APIs with route groups, TypedResults, and clean Program.cs. Both no-skills and dotnet-skills fall back to the controller pattern. This is the most impactful architectural difference.

---

### 4. Input Validation & Guard Clauses [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 1 | — |
| 2 | 4 | 4 | 4 | 1 | — |
| 3 | 3 | 4 | 4 | 1 | — |
| **Mean** | **3.3** | **4.0** | **4.0** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 4)
Uses Data Annotations on sealed record DTOs. No `ArgumentNullException.ThrowIfNull()` guard clauses — uses null-coalescing throw instead:
```csharp
// dotnet-artisan: DTOs
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
    [Required, EmailAddress]
    public required string Email { get; init; }
}
// Services use ?? throw pattern
var member = await db.Members.FindAsync([id], ct)
    ?? throw new KeyNotFoundException($"Member with ID {id} not found.");
```

### dotnet-webapi (Score: 4)
Same pattern — Data Annotations on sealed records, `?? throw` for null checks:
```csharp
// dotnet-webapi: DTOs/Dtos.cs
public sealed record CreateMembershipPlanRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }
    [Range(1, 24)]
    public required int DurationMonths { get; init; }
    [Range(0.01, 999999.99)]
    public required decimal Price { get; init; }
}
```

### no-skills (Score: 3)
FitnessStudioApi uses FluentValidation with dedicated validator classes:
```csharp
// no-skills: Validators/Validators.cs
public class CreateMemberValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```
LibraryApi and VetClinicApi use Data Annotations on DTOs. No guard clauses in any app.

### dotnet-skills (Score: 3)
VetClinicApi uses FluentValidation. No guard clauses.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** both combine Data Annotations with compile-time `required` keyword for a clean validation approach. Neither configuration uses `ArgumentNullException.ThrowIfNull()` in constructors — this is a gap across all configs. no-skills and dotnet-skills use FluentValidation, adding an unnecessary 3rd-party dependency.

---

### 5. NuGet & Package Discipline [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 1 | — |
| 2 | 3 | 3 | 5 | 1 | — |
| 3 | 3 | 5 | 4 | 1 | — |
| **Mean** | **3.0** | **4.0** | **4.3** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 4)
Minimal package set — only OpenAPI, EF Core Design, and EF Core SQLite. FitnessStudioApi uses preview versions (`10.0.0-preview.3`), LibraryApi uses stable versions (`10.0.5`). No Swashbuckle, no FluentValidation. VetClinic adds SwaggerUI only.
```xml
<!-- dotnet-artisan: LibraryApi.csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```

### dotnet-webapi (Score: 4)
Same minimal set. FitnessStudioApi: 3 packages (pinned). LibraryApi uses wildcard versions `10.*-*` which is a concern:
```xml
<!-- dotnet-webapi: LibraryApi.csproj — WILDCARD -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

### no-skills (Score: 3)
5 packages including unnecessary Swashbuckle and FluentValidation:
```xml
<!-- no-skills: FitnessStudioApi.csproj -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

### dotnet-skills (Score: 2)
Uses wildcard versions (`10.0.0-*`) for EF Core packages, plus Swashbuckle and FluentValidation:
```xml
<!-- dotnet-skills: VetClinicApi.csproj — WILDCARDS -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.0-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** achieve the minimal dependency footprint (3 packages vs 5). Wildcard versions in dotnet-webapi's LibraryApi and dotnet-skills are a reproducibility risk. no-skills adds unnecessary 3rd-party packages.

---

### 6. EF Migration Usage [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 4 | 2 | 1 | — |
| 2 | 2 | 2 | 2 | 1 | — |
| 3 | 1 | 5 | 1 | 1 | — |
| **Mean** | **1.7** | **3.7** | **1.7** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 2)
All apps use `EnsureCreatedAsync()` — bypasses migrations entirely:
```csharp
// dotnet-artisan: Program.cs (all apps)
await db.Database.EnsureCreatedAsync();
await DataSeeder.SeedAsync(db);
```

### dotnet-webapi (Score: 4)
FitnessStudioApi uses proper migrations with `MigrateAsync()` and includes a Migrations/ directory:
```csharp
// dotnet-webapi: FitnessStudioApi/Program.cs
await db.Database.MigrateAsync();
await DataSeeder.SeedAsync(db);
```
However, LibraryApi and VetClinicApi revert to `EnsureCreatedAsync()`. Inconsistent across apps.

### no-skills (Score: 2)
All apps use `EnsureCreatedAsync()`:
```csharp
// no-skills: Program.cs
await db.Database.EnsureCreatedAsync();
await DataSeeder.SeedAsync(db);
```

### dotnet-skills (Score: 2)
VetClinicApi uses synchronous `db.Database.EnsureCreated()`:
```csharp
// dotnet-skills: Program.cs
db.Database.EnsureCreated();
DataSeeder.Seed(db);
```

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-webapi** is the only configuration to use proper migrations (at least in FitnessStudioApi), which is the production-safe approach. All other configs use the `EnsureCreated()` anti-pattern. This is a universal weakness — even dotnet-webapi is inconsistent.

---

### 7. Business Logic Correctness [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 1 | — |
| 2 | 5 | 5 | 5 | 1 | — |
| 3 | 4 | 4 | 4 | 1 | — |
| **Mean** | **4.3** | **4.3** | **4.3** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 4)
Implements most specified endpoints and business rules across FitnessStudioApi and LibraryApi. Includes waitlist promotion, booking window enforcement, membership tier access, and weekly booking limits. Missing some edge cases.

### dotnet-webapi (Score: 4)
Comprehensive endpoint coverage across all 3 apps. FitnessStudioApi includes 354-line .http file testing business rules. Implements capacity management, cancellation policy, membership freeze/unfreeze, and check-in logic.

### no-skills (Score: 4)
All 3 apps implement the core business rules. Uses custom `BusinessRuleException` for domain violations. Includes state transition validation and booking constraints.

### dotnet-skills (Score: 3)
Only VetClinicApi has code. Implements appointment scheduling with conflict detection, status transitions with a `ValidTransitions` dictionary, and medical record management. Missing 2 of 3 apps entirely.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan**, **dotnet-webapi**, and **no-skills** all demonstrate competent business logic implementation. dotnet-skills shows strong patterns in its single app (state machine for appointments) but loses marks for generating only 1/3 apps.

---

### 8. Prefer Built-in over 3rd Party [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 1 | — |
| 2 | 1 | 5 | 5 | 1 | — |
| 3 | 2 | 5 | 3 | 1 | — |
| **Mean** | **1.7** | **5.0** | **4.3** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
Uses built-in `AddOpenApi()`/`MapOpenApi()`. No Swashbuckle. No FluentValidation — uses Data Annotations. Uses `System.Text.Json`. Uses built-in DI:
```csharp
// dotnet-artisan: Program.cs
builder.Services.AddOpenApi();
// ...
app.MapOpenApi();
```

### dotnet-webapi (Score: 5)
Same pattern — built-in OpenAPI, no Swashbuckle, no 3rd-party validation or serialization libraries.

### no-skills (Score: 2)
FitnessStudioApi uses both `AddOpenApi()` AND `AddSwaggerGen()` simultaneously. Includes `Swashbuckle.AspNetCore` and `FluentValidation.AspNetCore`:
```csharp
// no-skills: FitnessStudioApi/Program.cs
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c => { ... });
// ...
app.UseSwagger();
app.UseSwaggerUI(...);
```
LibraryApi and VetClinicApi also use Swashbuckle.

### dotnet-skills (Score: 2)
VetClinicApi uses both `AddOpenApi()` AND `AddSwaggerGen()` plus FluentValidation.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** correctly use only built-in capabilities. no-skills and dotnet-skills add Swashbuckle and FluentValidation — unnecessary when built-in OpenAPI and Data Annotations exist. This is the clearest signal of skill impact.

---

### 9. Modern C# Adoption [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 1 | — |
| 2 | 2 | 5 | 5 | 1 | — |
| 3 | 2 | 4 | 5 | 1 | — |
| **Mean** | **2.0** | **4.7** | **5.0** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
Primary constructors, collection expressions, sealed records, file-scoped namespaces, `required` keyword, nullable types:
```csharp
// dotnet-artisan: Services/MemberService.cs
public sealed class MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    : IMemberService { ... }

// Models
public ICollection<Booking> Bookings { get; set; } = []; // Collection expression
```

### dotnet-webapi (Score: 5)
Identical modern C# usage:
```csharp
// dotnet-webapi: Services/MembershipPlanService.cs
public sealed class MembershipPlanService(FitnessDbContext db, ILogger<MembershipPlanService> logger)
    : IMembershipPlanService { ... }

// Models
public ICollection<Membership> Memberships { get; set; } = [];
```

### no-skills (Score: 2)
Traditional constructors, `new List<T>()`, no `required` keyword, no sealed types:
```csharp
// no-skills: Services/MemberService.cs
public class MemberService : IMemberService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MemberService> _logger;
    public MemberService(FitnessDbContext db, ILogger<MemberService> logger)
    {
        _db = db;
        _logger = logger;
    }
}
// Models
public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
```

### dotnet-skills (Score: 2)
Traditional constructors, `new List<T>()`, some sealed types but inconsistent:
```csharp
// dotnet-skills: VetClinicApi/Services/OwnerService.cs
public sealed class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<OwnerService> _logger;
    public OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) { _db = db; _logger = logger; }
}
```

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** fully embrace C# 12+ features. no-skills and dotnet-skills produce older-style code with more boilerplate. Primary constructors alone save ~5 lines per service class.

---

### 10. Error Handling & Middleware [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 1 | — |
| 2 | 3 | 5 | 3 | 1 | — |
| 3 | 3 | 5 | 5 | 1 | — |
| **Mean** | **3.3** | **5.0** | **4.0** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 4)
FitnessStudioApi uses modern `IExceptionHandler`; LibraryApi uses inline middleware pattern (less clean but functional):
```csharp
// dotnet-artisan: FitnessStudioApi — IExceptionHandler
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (0, (string?)null)
        };
        // ... returns ProblemDetails
    }
}
```

### dotnet-webapi (Score: 5)
Consistently uses `IExceptionHandler` with `ProblemDetails` across all 3 apps. Clean registration:
```csharp
// dotnet-webapi: Program.cs (all apps)
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
app.UseStatusCodePages();
```

### no-skills (Score: 4)
FitnessStudioApi uses `IExceptionHandler` with custom `BusinessRuleException`. LibraryApi and VetClinicApi use traditional `RequestDelegate` middleware. All return ProblemDetails.

### dotnet-skills (Score: 3)
VetClinicApi uses traditional middleware (not IExceptionHandler) with custom `BusinessRuleException` and `NotFoundException`:
```csharp
// dotnet-skills: VetClinicApi/Middleware/GlobalExceptionHandlerMiddleware.cs
public sealed class GlobalExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (BusinessRuleException ex) { ... }
        catch (NotFoundException ex) { ... }
    }
}
```

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-webapi** achieves the gold standard — consistent `IExceptionHandler` everywhere. dotnet-artisan is close but inconsistent across apps. no-skills mixes approaches.

---

### 11. Async Patterns & Cancellation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 1 | — |
| 2 | 2 | 5 | 5 | 1 | — |
| 3 | 2 | 5 | 5 | 1 | — |
| **Mean** | **2.3** | **5.0** | **5.0** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
All service methods accept `CancellationToken`, propagate it through EF Core calls, and use proper `async Task<T>` return types:
```csharp
// dotnet-artisan: Services
public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken ct = default)
{
    await db.SaveChangesAsync(ct);
}
```

### dotnet-webapi (Score: 5)
Same pattern — full CancellationToken propagation:
```csharp
// dotnet-webapi: Services
public async Task<IReadOnlyList<MembershipPlanResponse>> GetAllAsync(CancellationToken ct)
{
    return await db.MembershipPlans.AsNoTracking().ToListAsync(ct);
}
```

### no-skills (Score: 3)
**No CancellationToken** parameters anywhere in services. Async/await is used but tokens are not propagated:
```csharp
// no-skills: Services/MemberService.cs
public async Task<MemberDto> CreateAsync(CreateMemberDto dto)  // No CancellationToken!
{
    await _db.SaveChangesAsync();  // No token passed
}
```

### dotnet-skills (Score: 3)
VetClinicApi also lacks CancellationToken:
```csharp
// dotnet-skills: VetClinicApi/Services/OwnerService.cs
public async Task<OwnerResponseDto> CreateAsync(OwnerCreateDto dto)  // No CancellationToken
{
    await _db.SaveChangesAsync();
}
```

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** both properly propagate CancellationToken through all layers. no-skills and dotnet-skills miss this entirely, meaning cancelled HTTP requests continue wasting server resources.

---

### 12. EF Core Best Practices [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 1 | — |
| 2 | 3 | 5 | 5 | 1 | — |
| 3 | 2 | 5 | 4 | 1 | — |
| **Mean** | **2.7** | **4.7** | **4.3** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 4)
Uses Fluent API extensively, `AsNoTracking()` for reads, `HasConversion<string>()` for enums, `DeleteBehavior.Restrict` for foreign keys:
```csharp
// dotnet-artisan: Services
var query = db.Members.AsNoTracking().AsQueryable();
// DbContext
entity.Property(e => e.Status).HasConversion<string>();
entity.HasOne(e => e.Member).WithMany(m => m.Memberships).OnDelete(DeleteBehavior.Restrict);
```

### dotnet-webapi (Score: 4)
Same EF Core patterns. Additionally overrides `SaveChanges` for automatic timestamps in VetClinicApi:
```csharp
// dotnet-webapi: VetClinicApi/Data/VetClinicDbContext.cs
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    SetTimestamps();
    return base.SaveChangesAsync(cancellationToken);
}
```

### no-skills (Score: 3)
Uses Fluent API but **does NOT use AsNoTracking()** for read queries. All queries track entities unnecessarily:
```csharp
// no-skills: BookService.cs — NO AsNoTracking()
var query = _db.Books
    .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
    .AsQueryable();  // Missing AsNoTracking()!
```

### dotnet-skills (Score: 3)
VetClinicApi uses `AsNoTracking()` and Fluent API. Uses `AsSplitQuery()` for complex includes (good). However, only 1 app to evaluate.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** consistently apply `AsNoTracking()` — doubling read performance. no-skills misses this optimization entirely. dotnet-skills shows good patterns in its single app.

---

### 13. Service Abstraction & DI [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 1 | — |
| 2 | 3 | 5 | 5 | 1 | — |
| 3 | 4 | 5 | 4 | 1 | — |
| **Mean** | **3.7** | **5.0** | **4.7** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
Interface per service, scoped registration, single responsibility. Each service in its own file:
```csharp
// dotnet-artisan: Program.cs
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IMemberService, MemberService>();
```

### dotnet-webapi (Score: 5)
Same pattern — one interface per service, individual files, scoped lifetime.

### no-skills (Score: 4)
Interfaces are present but organized differently — some in a single `Interfaces.cs` file (LibraryApi) or `Interfaces/` subdirectory (FitnessStudioApi). Same registration pattern.

### dotnet-skills (Score: 4)
VetClinicApi has interfaces in a single `IServices.cs` file. Services are properly separated and registered.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: All configs with code use interface-based DI correctly. **dotnet-artisan** and **dotnet-webapi** have the cleanest file organization with one interface file per service.

---

### 14. Security Configuration [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 1 | — |
| 2 | 1 | 1 | 1 | 1 | — |
| 3 | 2 | 2 | 2 | 1 | — |
| **Mean** | **1.7** | **1.7** | **1.7** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 2)
No HSTS, no HTTPS redirection. OpenAPI is Development-only (good).

### dotnet-webapi (Score: 2)
Same — no HSTS, no HTTPS redirection. OpenAPI is Development-only.

### no-skills (Score: 2)
No HSTS, no HTTPS. LibraryApi exposes Swagger unconditionally (not Development-only), which is worse:
```csharp
// no-skills: LibraryApi/Program.cs
app.UseSwagger();  // Always enabled — NOT gated on IsDevelopment!
app.UseSwaggerUI(...);
```

### dotnet-skills (Score: 2)
No HSTS, no HTTPS. Swagger gated on Development.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: All configurations miss HSTS and HTTPS redirection — a universal gap. No config achieves better than a 2 here. no-skills is slightly worse for exposing Swagger in production.

---

### 15. DTO Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 1 | — |
| 2 | 2 | 5 | 5 | 1 | — |
| 3 | 2 | 5 | 5 | 1 | — |
| **Mean** | **2.0** | **5.0** | **5.0** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
Sealed records, `*Request`/`*Response` naming, init-only properties, immutable:
```csharp
// dotnet-artisan
public sealed record MemberResponse(int Id, string FirstName, string LastName, string Email, ...);
public sealed record CreateMemberRequest { [Required] public required string FirstName { get; init; } }
```

### dotnet-webapi (Score: 5)
Identical pattern — sealed records with Request/Response naming.

### no-skills (Score: 2)
Mutable classes, `*Dto` naming, inheritance between DTOs:
```csharp
// no-skills
public class MemberDto { public string FirstName { get; set; } = string.Empty; }
public class MemberDetailDto : MemberDto { public ActiveMembershipInfo? ActiveMembership { get; set; } }
```

### dotnet-skills (Score: 2)
Mutable classes, `*Dto` naming, `UpdateDto : CreateDto` inheritance:
```csharp
// dotnet-skills
public class OwnerCreateDto { public string FirstName { get; set; } = string.Empty; }
public class OwnerUpdateDto : OwnerCreateDto { }
```

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** use the modern pattern — immutable sealed records. no-skills and dotnet-skills use mutable classes with DTO inheritance, which risks over-posting vulnerabilities and mutation bugs.

---

### 16. Sealed Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 5 | 1 | — |
| 2 | 1 | 5 | 5 | 1 | — |
| 3 | 1 | 5 | 5 | 1 | — |
| **Mean** | **1.0** | **5.0** | **5.0** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
All entities, DTOs, services, DbContext, and middleware are sealed:
```csharp
// dotnet-artisan
public sealed class Member { ... }
public sealed record CreateMemberRequest { ... }
public sealed class MemberService(...) : IMemberService { ... }
internal sealed class ApiExceptionHandler(...) : IExceptionHandler { ... }
```

### dotnet-webapi (Score: 5)
100% sealed across models, services, DTOs, DbContext, and handlers.

### no-skills (Score: 1)
Zero sealed types across all 3 apps (0/32 in FitnessStudio, 0/28 in LibraryApi, 0/total in VetClinicApi).

### dotnet-skills (Score: 3)
VetClinicApi has sealed services and middleware but NOT sealed entities or DTOs. Mixed approach.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** achieve 100% sealed types — enabling JIT devirtualization optimizations. no-skills has zero sealed types. This is a dramatic difference in optimization potential.

---

### 17. Data Seeder Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 1 | — |
| 2 | 4 | 4 | 4 | 1 | — |
| 3 | 3 | 5 | 3 | 1 | — |
| **Mean** | **3.7** | **4.3** | **3.7** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 4)
Static `DataSeeder.SeedAsync()` with idempotency guard. Rich seed data (8 members, 4 instructors, 12 schedules, 15+ bookings):
```csharp
// dotnet-artisan
public static async Task SeedAsync(FitnessDbContext db)
{
    if (await db.MembershipPlans.AnyAsync()) return;
    // ... comprehensive seeding
}
```

### dotnet-webapi (Score: 4)
Same pattern with comprehensive data. Uses async seeding consistently.

### no-skills (Score: 4)
Same DataSeeder pattern with thorough seed data across all scenarios.

### dotnet-skills (Score: 3)
VetClinicApi uses synchronous `DataSeeder.Seed()` (not async):
```csharp
// dotnet-skills: Program.cs
db.Database.EnsureCreated();
DataSeeder.Seed(db);  // Synchronous!
```

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: All configs with working code provide realistic seed data. **dotnet-skills** loses a point for synchronous seeding.

---

### 18. Structured Logging [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 1 | — |
| 2 | 4 | 5 | 5 | 1 | — |
| 3 | 3 | 4 | 4 | 1 | — |
| **Mean** | **3.3** | **4.7** | **4.7** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
`ILogger<T>` injected everywhere with structured message templates:
```csharp
// dotnet-artisan
logger.LogInformation("Member '{FirstName} {LastName}' registered with ID {MemberId}",
    member.FirstName, member.LastName, member.Id);
```

### dotnet-webapi (Score: 5)
Same pattern — structured templates, appropriate log levels:
```csharp
// dotnet-webapi
logger.LogInformation("Created membership plan {PlanName} with ID {PlanId}", plan.Name, plan.Id);
logger.LogWarning(exception, "Handled API exception: {Title}", title);
```

### no-skills (Score: 3)
FitnessStudioApi has structured logging; LibraryApi **does NOT inject ILogger** in services at all:
```csharp
// no-skills: LibraryApi/Services/BookService.cs
public class BookService : IBookService
{
    private readonly LibraryDbContext _db;
    public BookService(LibraryDbContext db) => _db = db;  // No ILogger!
}
```

### dotnet-skills (Score: 3)
VetClinicApi has ILogger with structured templates but limited to key operations.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** consistently inject and use structured logging. no-skills is inconsistent — some apps lack logging entirely.

---

### 19. Nullable Reference Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 1 | — |
| 2 | 4 | 5 | 5 | 1 | — |
| 3 | 4 | 4 | 4 | 1 | — |
| **Mean** | **4.0** | **4.7** | **4.7** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
`<Nullable>enable</Nullable>` in all .csproj files, proper `?` annotations:
```csharp
// dotnet-artisan
public string? Description { get; set; }
public string? Bio { get; set; }
```

### dotnet-webapi (Score: 5)
Same NRT configuration and usage.

### no-skills (Score: 4)
NRT enabled in .csproj but some inconsistent usage.

### dotnet-skills (Score: 4)
NRT enabled; proper `?` annotations.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: All configs enable NRT. **dotnet-artisan** and **dotnet-webapi** use it most consistently.

---

### 20. API Documentation [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 4 | 1 | — |
| 2 | 4 | 5 | 5 | 1 | — |
| 3 | 3 | 5 | 4 | 1 | — |
| **Mean** | **3.3** | **5.0** | **4.3** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 4)
Uses `AddOpenApi()` with document transformers for metadata. OpenAPI mapping in Development. No `WithSummary()` on every endpoint:
```csharp
// dotnet-artisan: LibraryApi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Sunrise Community Library API";
        return Task.CompletedTask;
    });
});
```

### dotnet-webapi (Score: 5)
Built-in OpenAPI plus rich endpoint metadata — `.WithName()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()`:
```csharp
// dotnet-webapi: Endpoints
group.MapGet("/", handler)
    .WithName("GetMembershipPlans")
    .WithSummary("List all active membership plans")
    .WithDescription("Returns all active membership plans sorted by price.")
    .Produces<IReadOnlyList<MembershipPlanResponse>>();
```

### no-skills (Score: 3)
Swashbuckle with XML comments. LibraryApi enables `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.

### dotnet-skills (Score: 2)
Basic Swashbuckle setup with SwaggerDoc metadata. No per-endpoint documentation.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-webapi** provides the richest API documentation with per-endpoint metadata that enables high-quality client generation.

---

### 21. File Organization [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 1 | — |
| 2 | 3 | 5 | 5 | 1 | — |
| 3 | 3 | 5 | 4 | 1 | — |
| **Mean** | **3.3** | **5.0** | **4.7** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
Clean separation: `Data/`, `DTOs/`, `Endpoints/`, `Middleware/`, `Models/`, `Services/`. One DTO file per entity, one endpoint file per entity, one service interface + implementation file pair:
```
Endpoints/BookingEndpoints.cs
Services/IBookingService.cs
Services/BookingService.cs
DTOs/BookingDtos.cs
Models/Booking.cs
```

### dotnet-webapi (Score: 5)
Same clean structure with Endpoints/, DTOs/, Services/ separation.

### no-skills (Score: 4)
Good structure but uses Controllers/ instead of Endpoints/. Some files are consolidated (LibraryApi puts all DTOs in one `Dtos.cs` file, all interfaces in one `Interfaces.cs` file).

### dotnet-skills (Score: 3)
VetClinicApi puts all DTOs in one `Dtos.cs` file and all interfaces in one `IServices.cs` file. Some services are combined (MedicalRecordService.cs contains PrescriptionService and VaccinationService).

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** have the cleanest per-entity file organization. no-skills and dotnet-skills consolidate related types into monolithic files.

---

### 22. HTTP Test File Quality [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 1 | — |
| 2 | 4 | 5 | 5 | 1 | — |
| 3 | 3 | 4 | 1 | 1 | — |
| **Mean** | **3.7** | **4.7** | **3.3** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 4)
40-50+ test cases per app covering CRUD, pagination, search/filter, and some business rule tests.

### dotnet-webapi (Score: 5)
FitnessStudioApi has a 354-line .http file testing ALL endpoints plus business rule edge cases (premium class restrictions, expired memberships, waitlist behavior):
```http
### Test: Book a full class (should go to waitlist)
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{
  "classScheduleId": 9,
  "memberId": 2
}

### Test: Member without active membership trying to book
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{
  "classScheduleId": 7,
  "memberId": 7
}
```

### no-skills (Score: 4)
Comprehensive .http files covering CRUD and basic business operations.

### dotnet-skills (Score: 3)
VetClinicApi has a .http file with basic coverage.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-webapi** produces the most thorough .http files with dedicated business rule test cases.

---

### 23. Type Design & Resource Mgmt [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 1 | — |
| 2 | — | — | — | — | — |
| 3 | 3 | 4 | 4 | 1 | — |
| **Mean** | **3.0** | **4.5** | **4.5** | **1.0** | — |

### 24. Code Standards Compliance [LOW × 0]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 1 | — |
| 2 | 3 | 5 | 5 | 1 | — |
| 3 | 3 | 4 | 4 | 1 | — |
| **Mean** | **3.0** | **4.7** | **4.7** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
PascalCase for public members, camelCase for parameters, Async suffix on async methods, I-prefix on interfaces, file-scoped namespaces, explicit access modifiers (`internal sealed`, `public sealed`).

### dotnet-webapi (Score: 5)
Same standards compliance.

### no-skills (Score: 3)
Follows naming conventions but misses Async suffix on some methods, uses default access modifiers, block-scoped namespaces in some places.

### dotnet-skills (Score: 3)
Reasonable naming but has 14 naming warnings in VetClinicApi build.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** produce the most standards-compliant code with zero naming warnings.

---

### 25. Type Design & Resource Management [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | — | — | — | — | — |
| 2 | 3 | 5 | 5 | 1 | — |
| 3 | — | — | — | — | — |
| **Mean** | **3.0** | **5.0** | **5.0** | **1.0** | — |

#### Analysis

### dotnet-artisan (Score: 5)
Enums for all status fields, `IReadOnlyList<T>` return types, `HasConversion<string>()` for EF Core:
```csharp
// dotnet-artisan
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }
public async Task<IReadOnlyList<BookingResponse>> GetUpcomingBookingsAsync(...)
```

### dotnet-webapi (Score: 5)
Same pattern — enums, IReadOnlyList, proper conversion.

### no-skills (Score: 3)
Enums present but returns `List<T>` (mutable) instead of `IReadOnlyList<T>`:
```csharp
// no-skills
public async Task<List<MembershipPlanDto>> GetAllActivePlansAsync() { ... }
```

### dotnet-skills (Score: 3)
Enums present, `IReadOnlyList<T>` in some places, `List<T>` in others.

### managedcode-dotnet-skills (Score: 1)
No code generated.

**Verdict**: **dotnet-artisan** and **dotnet-webapi** use `IReadOnlyList<T>` consistently, preventing accidental mutation. no-skills exposes mutable lists.

---

---

## Raw Data References

- Per-run analysis: `reports/analysis-run-1.md`
- Per-run analysis: `reports/analysis-run-2.md`
- Per-run analysis: `reports/analysis-run-3.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
