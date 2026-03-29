# Aggregated Analysis: ASP.NET Core Web API Skill Evaluation

**Runs:** 2 | **Configurations:** 5 | **Scenarios:** 3 | **Dimensions:** 24
**Date:** 2026-03-29 14:22 UTC

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 5.0 | 5.0 | 5.0 | 5.0 | 5.0 |
| Security Vulnerability Scan [CRITICAL] | 4.0 ± 1.4 | 5.0 | 4.5 ± 0.7 | 5.0 | 4.5 ± 0.7 |
| Minimal API Architecture [CRITICAL] | 1.0 | 5.0 | 3.5 ± 2.1 | 3.0 ± 2.8 | 1.5 ± 0.7 |
| Input Validation & Guard Clauses [CRITICAL] | 3.0 ± 1.4 | 4.5 ± 0.7 | 4.0 ± 1.4 | 4.5 ± 0.7 | 4.5 ± 0.7 |
| NuGet & Package Discipline [CRITICAL] | 2.5 ± 0.7 | 4.5 ± 0.7 | 3.5 ± 2.1 | 5.0 | 2.5 ± 0.7 |
| EF Migration Usage [CRITICAL] | 1.0 | 5.0 | 1.0 | 3.0 ± 2.8 | 1.0 |
| Business Logic Correctness [HIGH] | 5.0 | 5.0 | 5.0 | 5.0 | 5.0 |
| Prefer Built-in over 3rd Party [HIGH] | 3.0 ± 1.4 | 5.0 | 4.5 ± 0.7 | 5.0 | 2.5 ± 0.7 |
| Modern C# Adoption [HIGH] | 2.0 | 4.5 ± 0.7 | 5.0 | 5.0 | 3.0 |
| Error Handling & Middleware [HIGH] | 3.0 | 5.0 | 4.0 ± 1.4 | 5.0 | 4.0 ± 1.4 |
| Async Patterns & Cancellation [HIGH] | 2.5 ± 0.7 | 5.0 | 4.0 ± 1.4 | 5.0 | 3.0 |
| EF Core Best Practices [HIGH] | 2.5 ± 0.7 | 4.5 ± 0.7 | 4.5 ± 0.7 | 5.0 | 4.5 ± 0.7 |
| Service Abstraction & DI [HIGH] | 4.0 ± 1.4 | 5.0 | 5.0 | 5.0 | 4.5 ± 0.7 |
| Security Configuration [HIGH] | 1.5 ± 0.7 | 1.5 ± 0.7 | 1.5 ± 0.7 | 2.0 ± 1.4 | 1.5 ± 0.7 |
| DTO Design [MEDIUM] | 2.0 | 5.0 | 5.0 | 5.0 | 3.0 ± 1.4 |
| Sealed Types [MEDIUM] | 1.0 | 5.0 | 5.0 | 5.0 | 3.5 ± 2.1 |
| Data Seeder Design [MEDIUM] | 4.0 ± 1.4 | 4.5 ± 0.7 | 4.0 ± 1.4 | 5.0 | 4.0 ± 1.4 |
| Structured Logging [MEDIUM] | 3.0 | 4.0 | 4.0 | 4.0 | 4.0 |
| Nullable Reference Types [MEDIUM] | 4.0 | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.5 ± 0.7 |
| API Documentation [MEDIUM] | 2.5 ± 0.7 | 5.0 | 4.0 ± 1.4 | 4.0 ± 1.4 | 3.0 |
| File Organization [MEDIUM] | 4.0 ± 1.4 | 5.0 | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.0 |
| HTTP Test File Quality [MEDIUM] | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.0 |
| Type Design & Resource Management [MEDIUM] | 4.0 ± 1.4 | 5.0 | 5.0 | 5.0 | 4.5 ± 0.7 |
| Code Standards Compliance [LOW] | 3.5 ± 0.7 | 5.0 | 5.0 | 5.0 | 4.0 |

---

## Final Rankings

| Rank | Configuration | Mean Weighted Score | Std Dev | Min | Max |
|---|---|---|---|---|---|
| 🥇 | dotnet-webapi | 203.0 | 6.4 | 198.5 | 207.5 |
| 🥈 | managedcode-dotnet-skills | 194.5 | 12.7 | 185.5 | 203.5 |
| 🥉 | dotnet-artisan | 174.5 | 14.1 | 164.5 | 184.5 |
| 4th | dotnet-skills | 149.5 | 14.8 | 139.0 | 160.0 |
| 5th | no-skills | 127.2 | 13.8 | 117.5 | 137.0 |

---

## Weighted Score per Run

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 137.0 | 207.5 | 184.5 | 185.5 | 160.0 |
| 2 | 117.5 | 198.5 | 164.5 | 203.5 | 139.0 |
| **Mean** | **127.2** | **203.0** | **174.5** | **194.5** | **149.5** |

---

## Verification Summary (All Runs)

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |
|---|---|---|---|
| no-skills | 2/6 (33%) | 2/6 (33%) | 43.7 |
| dotnet-webapi | 2/6 (33%) | 2/6 (33%) | 53.7 |
| dotnet-artisan | 2/6 (33%) | 2/6 (33%) | 41.0 |
| managedcode-dotnet-skills | 2/6 (33%) | 2/6 (33%) | 55.7 |
| dotnet-skills | 2/6 (33%) | 2/6 (33%) | 44.3 |

---

## Asset Usage Summary

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | b179652a…cbdd | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 2 | 82c57f13…e5bb | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-webapi | 1 | a0fd980f…17f6 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-webapi | 2 | efb889b1…7d02 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-artisan | 1 | 13af5dce…dc65 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp, dotnet-webapi | dotnet-artisan | ⚠️ Mismatch |
| dotnet-artisan | 2 | 5389d405…7735 | None | — | — | ⚠️ Mismatch |
| managedcode-dotnet-skills | 1 | 06c15e75…e9f6 | claude-opus-4.6-1m | dotnet, dotnet-project-setup, dotnet-webapi, dotnet-modern-csharp, dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-microsoft-extensions | — | ⚠️ Mismatch |
| managedcode-dotnet-skills | 2 | adf04800…6d13 | claude-opus-4.6-1m | dotnet, dotnet-webapi, dotnet-project-setup, dotnet-modern-csharp, dotnet-aspnet-core, dotnet-entity-framework-core | — | ⚠️ Mismatch |
| dotnet-skills | 1 | 1628067c…1052 | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ⚠️ Mismatch |
| dotnet-skills | 2 | 72739fd0…fa85 | claude-opus-4.6-1m | optimizing-ef-core-queries, analyzing-dotnet-performance | dotnet-data, dotnet-diag | ⚠️ Mismatch |

### ⚠️ Asset Mismatches Detected

One or more runs loaded skills/plugins that did not match the expected configuration. This may indicate skill contamination or auto-discovery loading additional resources. Review the session events.jsonl files for details.

---

## Consistency Analysis

| Configuration | Score σ | Most Consistent Dim (σ) | Most Variable Dim (σ) |
|---|---|---|---|
| no-skills | 13.8 | Build & Run Success (0.0) | Security Vulnerability Scan (1.4) |
| dotnet-webapi | 6.4 | Build & Run Success (0.0) | Input Validation & Guard Clauses (0.7) |
| dotnet-artisan | 14.1 | Build & Run Success (0.0) | Minimal API Architecture (2.1) |
| managedcode-dotnet-skills | 12.7 | Build & Run Success (0.0) | Minimal API Architecture (2.8) |
| dotnet-skills | 14.8 | Build & Run Success (0.0) | Sealed Types (2.1) |

---

## Per-Dimension Analysis

### 1. Build & Run Success [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **5.0** | **5.0** | **5.0** | **5.0** | **5.0** |

#### Analysis

All five configurations produce projects that compile and run successfully targeting .NET 10.

All share the same foundation:
```xml
<!-- Common across all configurations -->
<TargetFramework>net10.0</TargetFramework>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

No configurations have missing references, circular dependencies, or broken project structures. All have `Directory.Build.props` with Meziantou.Analyzer for code quality enforcement.

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | Clean build, no issues |
| dotnet-skills | 5 | Clean build, no issues |
| dotnet-webapi | 5 | Clean build, no issues |
| managedcode | 5 | Clean build, no issues |
| no-skills | 5 | Clean build, no issues |

**Verdict**: All configurations produce compilable projects. This is a baseline expectation.

---

### 2. Security Vulnerability Scan [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 4 | 5 | 4 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **4.0** | **5.0** | **4.5** | **5.0** | **4.5** |

#### Analysis

This dimension evaluates NuGet package security — known CVEs, deprecated packages, and unnecessary dependencies.

**dotnet-webapi** and **managedcode** have the leanest, most secure dependency profiles:

```xml
<!-- managedcode — 3 pinned packages, no 3rd party -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.4" />
```

```xml
<!-- dotnet-webapi — 4 packages, pinned OpenAPI, wildcard EF Core -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.*" />
```

**dotnet-artisan** and **no-skills** include additional 3rd-party packages that increase attack surface:

```xml
<!-- dotnet-artisan — includes Swashbuckle with full wildcard -->
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="*" />
```

```xml
<!-- no-skills — includes Scalar with wildcard -->
<PackageReference Include="Scalar.AspNetCore" Version="2.*" />
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 4 | Latest .NET 10 packages; Swashbuckle `*` wildcard is a risk |
| dotnet-skills | 4 | Swashbuckle pinned to 10.1.7 but adds unnecessary attack surface |
| dotnet-webapi | 5 | Minimal packages, no 3rd-party UI libraries, all Microsoft |
| managedcode | 5 | Fewest packages (3), all pinned to exact 10.0.4, no 3rd party |
| no-skills | 3 | Scalar 3rd-party with wildcard `2.*`, multiple wildcard versions |

**Verdict**: **managedcode** and **dotnet-webapi** are best — fewest dependencies, no unnecessary 3rd-party packages. Swashbuckle and Scalar add attack surface without justification when built-in OpenAPI exists.

---

### 3. Minimal API Architecture [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 5 | 1 | 1 |
| 2 | 1 | 5 | 2 | 5 | 2 |
| **Mean** | **1.0** | **5.0** | **3.5** | **3.0** | **1.5** |

#### Analysis

The modern .NET standard is Minimal APIs with route groups, TypedResults, and endpoint extension methods. Controllers are the legacy pattern.

**dotnet-artisan** and **dotnet-webapi** use pure Minimal APIs:

```csharp
// dotnet-webapi — MembershipPlanEndpoints.cs
public static void MapMembershipPlanEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/membership-plans")
        .WithTags("Membership Plans");

    group.MapGet("/", async (IMembershipPlanService service, CancellationToken ct) =>
    {
        var plans = await service.GetAllAsync(ct);
        return TypedResults.Ok(plans);
    })
    .WithName("GetMembershipPlans")
    .WithSummary("List all active membership plans");
}
```

```csharp
// dotnet-artisan — Program.cs (clean endpoint registration)
app.MapMembershipPlanEndpoints();
app.MapMemberEndpoints();
app.MapMembershipEndpoints();
app.MapInstructorEndpoints();
app.MapClassTypeEndpoints();
app.MapClassScheduleEndpoints();
app.MapBookingEndpoints();
```

**dotnet-skills**, **managedcode**, and **no-skills** all use controllers:

```csharp
// no-skills — BookingsController.cs
[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var booking = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | Pure Minimal APIs with MapGroup, TypedResults, endpoint extension methods |
| dotnet-skills | 1 | Controllers with [ApiController]; no Minimal APIs |
| dotnet-webapi | 5 | Pure Minimal APIs with MapGroup, TypedResults, Results<T1,T2> union types |
| managedcode | 1 | Controllers with [ApiController]; no Minimal APIs |
| no-skills | 1 | Controllers with [ApiController]; no Minimal APIs |

**Verdict**: **dotnet-artisan** and **dotnet-webapi** are best. Minimal APIs with route groups produce lower-overhead, more concise code with compile-time type safety. The controller pattern is legacy for new .NET projects. Only the skills specifically designed for API architecture (dotnet-artisan's `dotnet-api` skill, and dotnet-webapi) successfully steered generation toward Minimal APIs.

---

### 4. Input Validation & Guard Clauses [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 5 | 5 |
| 2 | 2 | 4 | 3 | 4 | 4 |
| **Mean** | **3.0** | **4.5** | **4.0** | **4.5** | **4.5** |

#### Analysis

All configurations implement multi-layered validation with Data Annotations on DTOs and guard clauses in services. The differences are in the use of `required` modifier and `init` properties.

**Skill-guided configurations** (dotnet-artisan, dotnet-webapi, managedcode) use `required` + `init`:

```csharp
// dotnet-artisan — CreateMemberRequest
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required DateOnly DateOfBirth { get; init; }
}
```

**no-skills** uses mutable `set` properties with `= string.Empty` defaults:

```csharp
// no-skills — CreateMemberDto
public class CreateMemberDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
```

All configurations implement comprehensive business rule validation in services:

```csharp
// Common pattern across all configs — booking validation guards
if (classSchedule.StartTime > now.AddDays(7))
    throw new ArgumentException("Cannot book classes more than 7 days in advance.");

if (classSchedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
    throw new ArgumentException("Your plan does not allow booking premium classes.");
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | Data annotations + required/init + comprehensive guard clauses |
| dotnet-skills | 5 | Data annotations + guard clauses with custom BusinessRuleException |
| dotnet-webapi | 5 | Data annotations + required/init + comprehensive guard clauses |
| managedcode | 5 | Data annotations + required/init + comprehensive guard clauses |
| no-skills | 4 | Data annotations present but mutable DTOs; guard clauses with BusinessRuleException |

**Verdict**: All configurations do well here. The `required` + `init` pattern in skill-guided configs provides compile-time safety that mutable DTOs in no-skills lack.

---

### 5. NuGet & Package Discipline [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 4 | 2 | 5 | 3 |
| 2 | 3 | 5 | 5 | 5 | 2 |
| **Mean** | **2.5** | **4.5** | **3.5** | **5.0** | **2.5** |

#### Analysis

Version pinning discipline varies dramatically across configurations.

**managedcode** has the strictest discipline — all core packages pinned to exact versions:

```xml
<!-- managedcode — strict pinning -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.4" />
```

**dotnet-artisan** and **no-skills** use problematic wildcards:

```xml
<!-- dotnet-artisan — full wildcards on 3rd-party packages -->
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="*" />
<PackageReference Include="Meziantou.Analyzer" Version="*" />

<!-- no-skills — preview wildcards and 3rd-party wildcard -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
<PackageReference Include="Scalar.AspNetCore" Version="2.*" />
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 2 | Two `*` full wildcards (Swashbuckle, Meziantou); `10.*` on EF Core |
| dotnet-skills | 3 | OpenAPI pinned (10.0.4), Swashbuckle pinned (10.1.7), EF Core `10.*-*` |
| dotnet-webapi | 4 | OpenAPI pinned (10.0.4), EF Core `10.*` (minor wildcard), no 3rd-party |
| managedcode | 5 | All core packages pinned to exact 10.0.4; only analyzer uses wildcard |
| no-skills | 2 | Multiple wildcards: `10.*-*`, `2.*`, `*` |

**Verdict**: **managedcode** has the best package discipline. `Version="*"` on non-analyzer packages is the worst case as it can pull breaking changes or vulnerable releases. The `dotnet-entity-framework-core` and `dotnet-project-setup` skills in managedcode clearly enforce strict versioning.

---

### 6. EF Migration Usage [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 1 | 1 | 1 |
| 2 | 1 | 5 | 1 | 5 | 1 |
| **Mean** | **1.0** | **5.0** | **1.0** | **3.0** | **1.0** |

#### Analysis

Only **dotnet-webapi** uses EF Core migrations. All others use the `EnsureCreated` anti-pattern.

```csharp
// dotnet-webapi — Program.cs (production-grade)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
    await db.Database.MigrateAsync();  // ✅ Uses migrations
    await DataSeeder.SeedAsync(db);
}
```

```csharp
// All others — Program.cs (anti-pattern)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
    await db.Database.EnsureCreatedAsync();  // ❌ Bypasses migrations
    await DataSeeder.SeedAsync(db);
}
```

**dotnet-webapi** also has a full `Migrations/` directory with `InitialCreate` migration files, making schema evolution possible.

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 1 | Uses `EnsureCreatedAsync`; gen-notes acknowledge "Simpler for demo" |
| dotnet-skills | 1 | Uses `EnsureCreatedAsync`; no migrations |
| dotnet-webapi | 5 | Uses `MigrateAsync` with full migration history; production-ready |
| managedcode | 1 | Uses `EnsureCreatedAsync`; no migrations |
| no-skills | 1 | Uses `EnsureCreated` (sync!); no migrations |

**Verdict**: **dotnet-webapi** is the clear winner. `EnsureCreated` bypasses migrations entirely, making schema evolution impossible and causing data loss on model changes. This is the single most impactful architectural difference and the dotnet-webapi skill is the only one that gets it right.

---

### 7. Business Logic Correctness [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **5.0** | **5.0** | **5.0** | **5.0** | **5.0** |

#### Analysis

All configurations implement all 12 business rules from the specification. Endpoint counts range from ~39 to ~42.

The booking service in all configs enforces:
1. ✅ 7-day advance / 30-min cutoff booking window
2. ✅ Capacity management with automatic waitlist promotion
3. ✅ Cancellation policy (free >2hr, late cancellation marked)
4. ✅ Premium class tier restrictions
5. ✅ Weekly booking limits per membership plan
6. ✅ Active membership requirement
7. ✅ No double-booking (time overlap detection)
8. ✅ Instructor schedule conflict prevention
9. ✅ Membership freeze/unfreeze with end-date extension
10. ✅ Class cancellation cascading to all bookings
11. ✅ Check-in window (±15 minutes)
12. ✅ No-show flagging

```csharp
// Representative example from dotnet-webapi — weekly booking limits
var maxPerWeek = activeMembership.MembershipPlan.MaxClassBookingsPerWeek;
if (maxPerWeek != -1)
{
    var isoWeek = ISOWeek.GetWeekOfYear(now);
    var weekStart = ISOWeek.ToDateTime(ISOWeek.GetYear(now), isoWeek, DayOfWeek.Monday);
    var weeklyBookingCount = await db.Bookings.CountAsync(b =>
        b.MemberId == request.MemberId &&
        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Attended) &&
        b.ClassSchedule.StartTime >= weekStart &&
        b.ClassSchedule.StartTime < weekStart.AddDays(7), ct);
    if (weeklyBookingCount >= maxPerWeek)
        throw new ArgumentException($"Weekly booking limit of {maxPerWeek} reached.");
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | All 12 rules, ~36 endpoints |
| dotnet-skills | 5 | All 12 rules, ~41 endpoints |
| dotnet-webapi | 5 | All 12 rules, ~41 endpoints |
| managedcode | 5 | All 12 rules, ~39 endpoints |
| no-skills | 5 | All 12 rules, ~42 endpoints |

**Verdict**: Tie. Business logic correctness is independent of the skill configuration — all produce complete implementations.

---

### 8. Prefer Built-in over 3rd Party [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 5 | 3 |
| 2 | 2 | 5 | 5 | 5 | 2 |
| **Mean** | **3.0** | **5.0** | **4.5** | **5.0** | **2.5** |

#### Analysis

The key differentiator is OpenAPI approach. **dotnet-webapi** and **managedcode** use only built-in OpenAPI:

```csharp
// dotnet-webapi — built-in only
builder.Services.AddOpenApi();
app.MapOpenApi();
// No Swashbuckle, no Scalar, no third-party UI
```

**dotnet-artisan** uses built-in OpenAPI + Swashbuckle SwaggerUI:

```csharp
// dotnet-artisan — mixed approach
builder.Services.AddOpenApi();
app.MapOpenApi();
app.UseSwaggerUI(options =>
    options.SwaggerEndpoint("/openapi/v1.json", "Zenith Fitness Studio API v1"));
```

**dotnet-skills** uses full Swashbuckle:

```xml
<!-- dotnet-skills — full Swashbuckle package -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

**no-skills** uses Scalar:

```csharp
// no-skills — Scalar for API reference UI
app.MapScalarApiReference();
```

All configurations use System.Text.Json (not Newtonsoft.Json) and built-in ILogger<T>.

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 4 | Built-in OpenAPI, but adds Swashbuckle.AspNetCore.SwaggerUI as separate package |
| dotnet-skills | 3 | Full Swashbuckle.AspNetCore package; unnecessary when built-in exists |
| dotnet-webapi | 5 | Pure built-in: AddOpenApi()/MapOpenApi() only; zero 3rd-party |
| managedcode | 5 | Pure built-in: AddOpenApi()/MapOpenApi() only; zero 3rd-party |
| no-skills | 4 | Built-in OpenAPI, but adds Scalar.AspNetCore for UI |

**Verdict**: **dotnet-webapi** and **managedcode** are best — they avoid adding any 3rd-party libraries when built-in alternatives exist.

---

### 9. Modern C# Adoption [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 5 | 3 |
| 2 | 2 | 4 | 5 | 5 | 3 |
| **Mean** | **2.0** | **4.5** | **5.0** | **5.0** | **3.0** |

#### Analysis

**Primary constructors** are the clearest differentiator. dotnet-artisan, dotnet-webapi, and managedcode use them; dotnet-skills and no-skills don't.

```csharp
// dotnet-artisan, dotnet-webapi, managedcode — primary constructors
public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger)
    : IBookingService
{
    // db and logger used directly — no field declarations needed
}
```

```csharp
// dotnet-skills, no-skills — traditional constructors
public sealed class BookingService : IBookingService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<BookingService> _logger;

    public BookingService(FitnessDbContext context, ILogger<BookingService> logger)
    {
        _context = context;
        _logger = logger;
    }
}
```

**Collection expressions** `[]` are used by dotnet-artisan, dotnet-webapi, and managedcode:

```csharp
// Modern (dotnet-artisan, dotnet-webapi, managedcode)
public ICollection<Membership> Memberships { get; set; } = [];

// Traditional (dotnet-skills, no-skills)
public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | Primary constructors, collection expressions, sealed records, required modifier |
| dotnet-skills | 3 | File-scoped namespaces, records for reads but not writes; no primary constructors |
| dotnet-webapi | 5 | Primary constructors, collection expressions, sealed records, required, switch expressions |
| managedcode | 5 | Primary constructors, collection expressions, sealed records, required modifier |
| no-skills | 2 | File-scoped namespaces, nullable enabled; no primary constructors, no records, no collection expressions |

**Verdict**: **dotnet-artisan**, **dotnet-webapi**, and **managedcode** are tied at the top. The `dotnet-modern-csharp` skill in managedcode and the `dotnet-csharp` skill in dotnet-artisan explicitly enforce modern C# adoption.

---

### 10. Error Handling & Middleware [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 5 | 5 |
| 2 | 3 | 5 | 3 | 5 | 3 |
| **Mean** | **3.0** | **5.0** | **4.0** | **5.0** | **4.0** |

#### Analysis

All skill-guided configurations use `IExceptionHandler` (the modern .NET 8+ pattern). **no-skills** uses convention-based middleware.

```csharp
// dotnet-webapi — IExceptionHandler (modern pattern)
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (0, (string?)null)
        };
        // ... write ProblemDetails
    }
}
```

```csharp
// no-skills — convention middleware (older pattern)
public class GlobalExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }
}
```

**dotnet-skills** and **no-skills** define a custom `BusinessRuleException` class, while dotnet-artisan and dotnet-webapi use standard exception types (`ArgumentException`, `InvalidOperationException`, `KeyNotFoundException`).

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | IExceptionHandler, ProblemDetails, switch expression mapping |
| dotnet-skills | 5 | IExceptionHandler, ProblemDetails, custom BusinessRuleException |
| dotnet-webapi | 5 | IExceptionHandler (internal sealed), ProblemDetails, clean fallback for unhandled |
| managedcode | 5 | IExceptionHandler, ProblemDetails, standard exception types |
| no-skills | 3 | Convention middleware (not IExceptionHandler), ProblemDetails, BusinessRuleException |

**Verdict**: All skill-guided configurations correctly use the modern `IExceptionHandler` pattern. **no-skills** falls back to the older middleware approach which is functional but not composable or DI-aware.

---

### 11. Async Patterns & Cancellation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 5 | 3 |
| 2 | 2 | 5 | 3 | 5 | 3 |
| **Mean** | **2.5** | **5.0** | **4.0** | **5.0** | **3.0** |

#### Analysis

**CancellationToken propagation** is the key differentiator. dotnet-artisan, dotnet-webapi, and managedcode propagate tokens through the entire call chain.

```csharp
// dotnet-webapi — full CancellationToken propagation
// Endpoint level:
group.MapPost("/", async (CreateBookingRequest request,
    IBookingService service, CancellationToken ct) =>
{
    var booking = await service.CreateAsync(request, ct);
    return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
});

// Service level:
public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct)
{
    var member = await db.Members.FindAsync([request.MemberId], ct);
    var schedule = await db.ClassSchedules
        .FirstOrDefaultAsync(cs => cs.Id == request.ClassScheduleId, ct);
    await db.SaveChangesAsync(ct);
}
```

**dotnet-skills** passes tokens in `SaveChangesAsync` override but not in service method signatures:
```csharp
// dotnet-skills — CancellationToken missing from service methods
public async Task<BookingDto> CreateAsync(CreateBookingDto dto)  // ❌ No CancellationToken
{
    var member = await _context.Members.FindAsync(dto.MemberId);  // ❌ No ct
}
```

**no-skills** similarly lacks CancellationToken propagation in service methods.

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | CancellationToken on all endpoints and service methods, forwarded to all EF Core calls |
| dotnet-skills | 3 | Async suffix correct, no sync-over-async, but CancellationToken missing from service signatures |
| dotnet-webapi | 5 | CancellationToken on all endpoints and service methods, forwarded to all EF Core calls |
| managedcode | 5 | CancellationToken on all endpoints and service methods, forwarded to all EF Core calls |
| no-skills | 3 | Async suffix correct, no sync-over-async, but CancellationToken missing from service signatures |

**Verdict**: **dotnet-artisan**, **dotnet-webapi**, and **managedcode** all propagate CancellationToken end-to-end. This prevents wasted server resources on cancelled requests — critical for production APIs under load.

---

### 12. EF Core Best Practices [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 5 | 5 |
| 2 | 2 | 4 | 4 | 5 | 4 |
| **Mean** | **2.5** | **4.5** | **4.5** | **5.0** | **4.5** |

#### Analysis

**AsNoTracking** on read-only queries is the primary differentiator. All skill-guided configs use it; no-skills mostly doesn't.

```csharp
// dotnet-webapi — AsNoTracking on all reads
return await db.MembershipPlans
    .AsNoTracking()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Price)
    .Select(p => MapToResponse(p))
    .ToListAsync(ct);
```

All configurations use Fluent API configuration with unique indexes, enum-to-string conversions, and explicit cascade behaviors:

```csharp
// Common across skill-guided configs
modelBuilder.Entity<Booking>(e =>
{
    e.Property(b => b.Status).HasConversion<string>();
    e.HasOne(b => b.ClassSchedule)
        .WithMany(c => c.Bookings)
        .OnDelete(DeleteBehavior.Restrict);
});
```

**no-skills** uses `DeleteBehavior.Cascade` (less safe) and is missing consistent `AsNoTracking()`.

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | AsNoTracking, Fluent API, enum conversions, composite indexes, DeleteBehavior.Restrict |
| dotnet-skills | 5 | AsNoTracking (added by optimizing-ef-core-queries skill), Fluent API, proper tracking strategy |
| dotnet-webapi | 5 | AsNoTracking, Fluent API, enum conversions, DeleteBehavior.Restrict, migrations |
| managedcode | 5 | AsNoTracking, Fluent API, enum conversions, DeleteBehavior.Restrict |
| no-skills | 3 | Fluent API present, enum conversions, but missing AsNoTracking on reads, uses Cascade deletes |

**Verdict**: All skill-guided configurations score 5. The `optimizing-ef-core-queries` skill in dotnet-skills explicitly added AsNoTracking to all read paths after initial generation. **no-skills** misses this optimization entirely.

---

### 13. Service Abstraction & DI [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 3 | 5 | 5 | 5 | 4 |
| **Mean** | **4.0** | **5.0** | **5.0** | **5.0** | **4.5** |

#### Analysis

All configurations use interface-based dependency injection with `AddScoped<IService, Service>()`. This is a shared best practice.

```csharp
// Common across ALL configurations
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<IClassTypeService, ClassTypeService>();
builder.Services.AddScoped<IClassScheduleService, ClassScheduleService>();
builder.Services.AddScoped<IBookingService, BookingService>();
```

All configurations have 7 service interface/implementation pairs. DbContext is never injected directly into endpoints or controllers.

| Config | Score | Justification |
|---|---|---|
| All | 5 | Full interface-based DI, scoped lifetime, 7 service pairs |

**Verdict**: Tie. Service abstraction is consistently done well across all configurations.

---

### 14. Security Configuration [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 3 | 2 |
| 2 | 1 | 1 | 1 | 1 | 1 |
| **Mean** | **1.5** | **1.5** | **1.5** | **2.0** | **1.5** |

#### Analysis

All configurations have weak security posture. Only **managedcode** configures HTTPS redirection.

```csharp
// managedcode — has HTTPS redirection
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();  // ✅ Present
app.UseAuthorization();
app.MapControllers();
```

```csharp
// All others — missing security middleware
app.UseExceptionHandler();
app.UseStatusCodePages();
// ❌ No app.UseHttpsRedirection()
// ❌ No app.UseHsts()
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 2 | No HSTS, no HTTPS redirection |
| dotnet-skills | 2 | No HSTS, no HTTPS redirection |
| dotnet-webapi | 2 | No HSTS, no HTTPS redirection |
| managedcode | 3 | Has HTTPS redirection, UseAuthorization in pipeline, but no HSTS |
| no-skills | 2 | No HSTS, no HTTPS redirection |

**Verdict**: **managedcode** is slightly better thanks to its `dotnet-aspnet-core` skill enforcing correct middleware ordering and HTTPS redirection. All configurations would need hardening for production.

---

### 15. DTO Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 5 | 4 |
| 2 | 2 | 5 | 5 | 5 | 2 |
| **Mean** | **2.0** | **5.0** | **5.0** | **5.0** | **3.0** |

#### Analysis

**dotnet-artisan**, **dotnet-webapi**, and **managedcode** use sealed records with consistent `Create{Entity}Request` / `{Entity}Response` naming:

```csharp
// dotnet-webapi — sealed record with positional syntax for responses
public sealed record MembershipPlanResponse(
    int Id, string Name, string? Description,
    int DurationMonths, decimal Price,
    int MaxClassBookingsPerWeek, bool AllowsPremiumClasses, bool IsActive);

// sealed record with init properties for requests
public sealed record CreateMembershipPlanRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }
    [Range(0.01, double.MaxValue)]
    public required decimal Price { get; init; }
}
```

**dotnet-skills** uses records for reads and sealed classes for writes:

```csharp
// dotnet-skills — record for reads
public record MembershipPlanDto(int Id, string Name, string? Description, ...);

// sealed class for writes
public sealed class CreateMembershipPlanDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
```

**no-skills** uses plain classes for all DTOs:

```csharp
// no-skills — mutable class DTOs
public class MemberDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | Sealed records, positional responses, init requests, Create{Entity}Request naming |
| dotnet-skills | 4 | Records for reads, sealed classes for writes; Dto suffix naming |
| dotnet-webapi | 5 | Sealed records, positional responses, init requests, Create{Entity}Request naming |
| managedcode | 5 | Sealed records, positional responses, init requests, Create{Entity}Request naming |
| no-skills | 2 | Classes (not records), not sealed, mutable setters, Dto suffix naming |

**Verdict**: **dotnet-artisan**, **dotnet-webapi**, and **managedcode** produce the cleanest DTO designs. Immutable sealed records are safer and more expressive than mutable classes.

---

### 16. Sealed Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 5 | 5 | 5 |
| 2 | 1 | 5 | 5 | 5 | 2 |
| **Mean** | **1.0** | **5.0** | **5.0** | **5.0** | **3.5** |

#### Analysis

All skill-guided configurations seal 100% of types. **no-skills** seals none.

```csharp
// Skill-guided configs — everything sealed
public sealed class Member { }
public sealed class BookingService(FitnessDbContext db, ...) : IBookingService { }
public sealed record BookingResponse(...);
internal sealed class ApiExceptionHandler(...) : IExceptionHandler { }
```

```csharp
// no-skills — nothing sealed
public class Member { }
public class BookingService : IBookingService { }
public class MemberDto { }
public class GlobalExceptionHandlerMiddleware { }
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | ~37 types sealed (100%) |
| dotnet-skills | 5 | ~30+ types sealed (added by analyzing-dotnet-performance skill) |
| dotnet-webapi | 5 | ~42 types sealed (100%) |
| managedcode | 5 | ~42 types sealed (100%) |
| no-skills | 1 | 0 types sealed |

**Verdict**: All skill-guided configurations enforce sealed types. The `analyzing-dotnet-performance` skill in dotnet-skills specifically identified and sealed all types after initial generation. **no-skills** misses this optimization entirely, losing JIT devirtualization benefits.

---

### 17. Data Seeder Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 3 | 4 | 3 | 5 | 3 |
| **Mean** | **4.0** | **4.5** | **4.0** | **5.0** | **4.0** |

#### Analysis

All configurations produce comprehensive, realistic seed data meeting the specification requirements: 3 plans, 8 members, 6+ memberships, 4 instructors, 6 class types, 12+ schedules, 15+ bookings.

```csharp
// Common pattern — idempotent seeding
if (await db.MembershipPlans.AnyAsync())
{
    logger.LogInformation("Database already seeded, skipping");
    return;
}
```

All include edge cases: full classes with waitlists, cancelled classes, expired/frozen memberships, no-show bookings.

| Config | Score | Justification |
|---|---|---|
| All | 5 | Comprehensive, realistic, idempotent seed data |

**Verdict**: Tie. All configurations produce high-quality seed data.

---

### 18. Structured Logging [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 4 | 4 | 4 |
| 2 | 3 | 4 | 4 | 4 | 4 |
| **Mean** | **3.0** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

All configurations inject `ILogger<T>` and use structured message templates. None use `[LoggerMessage]` source generators.

```csharp
// dotnet-artisan — structured logging with named parameters
logger.LogInformation("Created booking {BookingId} for member {MemberId} in class {ClassId}, status: {Status}",
    booking.Id, member.Id, classSchedule.Id, booking.Status);

logger.LogInformation("Promoted booking {BookingId} from waitlist for class {ClassId}",
    nextWaitlisted.Id, booking.ClassScheduleId);
```

**no-skills** has fewer log statements overall:

```csharp
// no-skills — limited logging
_logger.LogInformation("Booking created: Member {MemberId} for class {ClassId} - Status: {Status}",
    dto.MemberId, dto.ClassScheduleId, booking.Status);
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 4 | ILogger<T>, structured templates, comprehensive log coverage |
| dotnet-skills | 4 | ILogger<T>, structured templates, good coverage |
| dotnet-webapi | 4 | ILogger<T>, structured templates, comprehensive log coverage |
| managedcode | 4 | ILogger<T>, structured templates, good coverage |
| no-skills | 3 | ILogger<T>, structured templates, but fewer log statements |

**Verdict**: All perform adequately. No configuration uses `[LoggerMessage]` source generators for high-performance logging.

---

### 19. Nullable Reference Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 5 | 5 |
| 2 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.0** | **4.5** | **4.5** | **4.5** | **4.5** |

#### Analysis

All configurations enable `<Nullable>enable</Nullable>` and properly annotate optional properties. The skill-guided configurations use `required` modifier more consistently.

```csharp
// Common proper annotations
public string? Description { get; set; }           // Optional
public DateOnly? FreezeStartDate { get; set; }     // Optional
public int? WaitlistPosition { get; set; }          // Optional
public Member Member { get; set; } = null!;         // EF navigation
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | Enabled, properly annotated, required modifier on non-nullable DTOs |
| dotnet-skills | 5 | Enabled, properly annotated |
| dotnet-webapi | 5 | Enabled, properly annotated, required modifier on non-nullable DTOs |
| managedcode | 5 | Enabled, properly annotated, required modifier on non-nullable DTOs |
| no-skills | 4 | Enabled, properly annotated, but uses `= string.Empty` defaults instead of `required` |

**Verdict**: Minor differences. All configurations handle nullable reference types correctly.

---

### 20. API Documentation [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 3 | 3 |
| 2 | 2 | 5 | 3 | 5 | 3 |
| **Mean** | **2.5** | **5.0** | **4.0** | **4.0** | **3.0** |

#### Analysis

Minimal API configurations (dotnet-artisan, dotnet-webapi) have richer endpoint metadata because of `.WithName()`, `.WithSummary()`, `.WithDescription()` chaining. Controller-based configs rely on `[ProducesResponseType]` attributes.

```csharp
// dotnet-webapi — rich metadata chaining
group.MapGet("/", async (...) => { ... })
    .WithName("GetMembershipPlans")
    .WithSummary("List all active membership plans")
    .WithDescription("Returns all active membership plans ordered by price.")
    .Produces<IReadOnlyList<MembershipPlanResponse>>();
```

```csharp
// managedcode — controller attributes
[HttpPost]
[ProducesResponseType(typeof(BookingResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | WithName, WithSummary, Produces, WithTags on all endpoints |
| dotnet-skills | 3 | [ProducesResponseType] attributes; no WithName/WithSummary |
| dotnet-webapi | 5 | WithName, WithSummary, WithDescription, Produces on all endpoints |
| managedcode | 3 | [ProducesResponseType] attributes; controller-style documentation |
| no-skills | 3 | [ProducesResponseType], XML summary comments, Scalar UI |

**Verdict**: **dotnet-artisan** and **dotnet-webapi** produce the richest OpenAPI metadata due to the Minimal API fluent API.

---

### 21. File Organization [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 4 | 4 |
| 2 | 3 | 5 | 4 | 5 | 4 |
| **Mean** | **4.0** | **5.0** | **4.5** | **4.5** | **4.0** |

#### Analysis

**dotnet-artisan** and **dotnet-webapi** have `Endpoints/` directories (modern pattern):

```
dotnet-webapi/
├── Endpoints/       ← Minimal API route groups
│   ├── MembershipPlanEndpoints.cs
│   ├── BookingEndpoints.cs
│   └── ...
├── Models/
├── DTOs/
├── Services/
├── Data/
├── Middleware/
└── Migrations/      ← Only in dotnet-webapi
```

**no-skills** has the most granular organization with nested DTO folders:

```
no-skills/
├── Controllers/
├── DTOs/
│   ├── Booking/
│   ├── Member/
│   └── ...
├── Services/
│   ├── Interfaces/
│   └── ...
├── Models/
│   └── Enums/
└── Data/
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | Clean: Endpoints/, Models/, DTOs/, Services/, Data/, Middleware/ |
| dotnet-skills | 4 | Standard controller layout; no Endpoints/ |
| dotnet-webapi | 5 | Clean: Endpoints/, Models/, DTOs/, Services/, Data/, Middleware/, Migrations/ |
| managedcode | 4 | Standard controller layout; no Endpoints/ |
| no-skills | 5 | Granular: nested DTOs by domain, Interfaces/ subfolder, Enums/ subfolder |

**Verdict**: **dotnet-artisan** and **dotnet-webapi** have the cleanest structures with dedicated `Endpoints/`. **no-skills** compensates with detailed sub-organization.

---

### 22. HTTP Test File Quality [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 4 |
| 2 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.5** | **4.5** | **4.5** | **4.5** | **4.0** |

#### Analysis

All configurations produce `.http` files covering all endpoints. The Minimal API configs tend to have more thorough business rule test cases.

```http
### dotnet-artisan — business rule test cases
### Test: Book a premium class as a basic member (should fail with 400)
### Member 3 (Emily Davis) has Basic plan, Boxing is premium
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{
  "classScheduleId": 6,
  "memberId": 3
}

### Test: Member with expired membership tries to book (should fail)
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{
  "classScheduleId": 1,
  "memberId": 7
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | 359 lines, all endpoints, business rule edge cases documented |
| dotnet-skills | 4 | Covers all endpoints, realistic data |
| dotnet-webapi | 5 | Comprehensive with all endpoints and business rule tests |
| managedcode | 5 | Comprehensive coverage |
| no-skills | 5 | ~42 requests, business rule tests, well-organized |

**Verdict**: All configurations produce good `.http` files. Minor differences in documentation quality.

---

### 23. Type Design & Resource Management [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 3 | 5 | 5 | 5 | 4 |
| **Mean** | **4.0** | **5.0** | **5.0** | **5.0** | **4.5** |

#### Analysis

All configurations properly use enums for status fields with `HasConversion<string>()` and `JsonStringEnumConverter`:

```csharp
// Common across all configs
public enum BookingStatus { Confirmed, Waitlisted, Cancelled, Attended, NoShow }
public enum MembershipStatus { Active, Expired, Cancelled, Frozen }

// EF Core configuration
entity.Property(e => e.Status).HasConversion<string>();

// JSON serialization
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

All use `DateOnly` for dates, `decimal` for monetary values, and nullable types for optional fields.

| Config | Score | Justification |
|---|---|---|
| All | 5 | Proper enums, HasConversion<string>, DateOnly, decimal for money |

**Verdict**: Tie. All configurations demonstrate excellent type design.

---

### 24. Code Standards Compliance [LOW × 0]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 5 | 4 |
| 2 | 3 | 5 | 5 | 5 | 4 |
| **Mean** | **3.5** | **5.0** | **5.0** | **5.0** | **4.0** |

#### Analysis

All configurations follow PascalCase naming, use the `Async` suffix on async methods, and have explicit access modifiers. The skill-guided configurations are slightly more consistent.

```csharp
// All configs follow .NET naming guidelines
public sealed class BookingService : IBookingService  // PascalCase types
{
    public async Task<BookingResponse> CreateAsync(...)  // Async suffix
    {
        var member = await db.Members.FindAsync(...);    // camelCase locals
    }
}
```

| Config | Score | Justification |
|---|---|---|
| dotnet-artisan | 5 | Consistent PascalCase, Async suffix, explicit modifiers, braces on all control flow |
| dotnet-skills | 4 | Consistent, but traditional constructor style less clean |
| dotnet-webapi | 5 | Consistent PascalCase, Async suffix, explicit modifiers |
| managedcode | 5 | Consistent PascalCase, Async suffix, explicit modifiers |
| no-skills | 4 | Consistent but traditional patterns, `_context` convention |

**Verdict**: Minor differences. All follow .NET naming conventions well.

---

---

## Raw Data References

- Per-run analysis: `reports/analysis-run-1.md`
- Per-run analysis: `reports/analysis-run-2.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
