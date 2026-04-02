# Aggregated Analysis: ASP.NET Core Web API Skill Evaluation

**Runs:** 3 | **Configurations:** 5 | **Scenarios:** 3 | **Dimensions:** 27
**Date:** 2026-04-02 00:57 UTC

---

## Overview

Evaluate how custom Copilot skills impact the quality of generated ASP.NET Core Web API code across three realistic application scenarios.

---

## What Was Tested

### Scenarios

Each run generates one of the following application scenarios (randomly selected per run):

| Scenario | Description |
|---|---|
| FitnessStudioApi | Booking/membership system with class scheduling, waitlists, and instructor management |
| LibraryApi | Book loans, reservations, overdue fines, and availability tracking |
| VetClinicApi | Pet healthcare with appointments, vaccinations, and medical records |

### Configurations

Each configuration gives Copilot different custom skills or plugins. The **no-skills** baseline uses default Copilot with no custom instructions.

| Configuration | Description | Skills | Plugins |
|---|---|---|---|
| no-skills | Baseline (default Copilot) | — | — |
| dotnet-webapi | dotnet-webapi skill | skills/dotnet-webapi | — |
| dotnet-artisan | dotnet-artisan plugin chain | — | plugins/dotnet-artisan |
| managedcode-dotnet-skills | Community managed-code skills | skills/managedcode-dotnet-skills | — |
| dotnet-skills | Official .NET Skills (dotnet/skills) | — | plugins/dotnet-skills/dotnet, plugins/dotnet-skills/dotnet-ai, plugins/dotnet-skills/dotnet-data, plugins/dotnet-skills/dotnet-diag, plugins/dotnet-skills/dotnet-experimental, plugins/dotnet-skills/dotnet-maui, plugins/dotnet-skills/dotnet-msbuild, plugins/dotnet-skills/dotnet-nuget, plugins/dotnet-skills/dotnet-template-engine, plugins/dotnet-skills/dotnet-test, plugins/dotnet-skills/dotnet-upgrade |

### How It Works

1. **Generate** — For each configuration, Copilot CLI (`copilot --yolo`) is given a scenario prompt and generates a complete project from scratch. One scenario is randomly selected per run.
2. **Verify** — Each generated project is built (`dotnet build`), run, format-checked, and scanned for NuGet vulnerabilities.
3. **Analyze** — An AI judge reviews the source code of all configurations side-by-side and scores each across 27 quality dimensions.

Generation model: **claude-opus-4.6-1m**
Analysis model: **gpt-5.3-codex**

---

## Scoring Methodology

Each dimension is scored on a **1–5 scale**:

| Score | Meaning |
|:---:|---|
| 5 | Excellent — follows all best practices |
| 4 | Good — minor gaps only |
| 3 | Acceptable — some issues present |
| 2 | Below average — significant gaps |
| 1 | Poor — missing or fundamentally wrong |

Dimensions are grouped into **tiers** that determine their weight in the final weighted score:

| Tier | Weight | Dimensions |
|---|:---:|:---:|
| CRITICAL | ×3 | 6 |
| HIGH | ×2 | 8 |
| MEDIUM | ×1 | 9 |
| LOW | ×0.5 | 1 |

**Maximum possible weighted score: 217.5** (all dimensions scoring 5).
Scores shown as **mean ± standard deviation** across runs.

---

## Executive Summary

| Dimension [Tier] | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| Scenario Coverage [MEDIUM] | 2.0 | 2.0 | 2.0 | 2.0 | 2.0 |
| Build & Run Success [CRITICAL] | 5.0 | 4.7 ± 0.6 | 5.0 | 5.0 | 5.0 |
| Security Vulnerability Scan [CRITICAL] | 4.7 ± 0.6 | 3.7 ± 2.3 | 5.0 | 5.0 | 4.7 ± 0.6 |
| Minimal API Architecture [CRITICAL] | 1.0 | 5.0 | 4.3 ± 0.6 | 1.7 ± 0.6 | 1.0 |
| Input Validation & Guard Clauses [CRITICAL] | 3.0 | 4.0 | 3.3 ± 0.6 | 3.7 ± 0.6 | 3.7 ± 0.6 |
| NuGet & Package Discipline [CRITICAL] | 2.3 ± 1.5 | 3.7 ± 2.3 | 3.0 ± 1.7 | 2.7 ± 1.5 | 4.0 |
| EF Migration Usage [CRITICAL] | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 |
| Business Logic Correctness [HIGH] | 4.0 | 5.0 | 4.3 ± 0.6 | 3.3 ± 1.2 | 4.0 |
| Prefer Built-in over 3rd Party [HIGH] | 1.7 ± 0.6 | 5.0 | 2.7 ± 0.6 | 2.0 | 2.7 ± 1.2 |
| Modern C# Adoption [HIGH] | 2.3 ± 0.6 | 5.0 | 4.7 ± 0.6 | 3.7 ± 0.6 | 3.3 ± 0.6 |
| Error Handling & Middleware [HIGH] | 3.0 | 4.3 ± 0.6 | 3.3 ± 1.5 | 3.3 ± 0.6 | 3.7 ± 0.6 |
| Async Patterns & Cancellation [HIGH] | 2.0 | 5.0 | 4.0 ± 1.7 | 2.7 ± 1.2 | 2.3 ± 0.6 |
| EF Core Best Practices [HIGH] | 2.7 ± 0.6 | 4.3 ± 1.2 | 4.7 ± 0.6 | 4.0 | 4.0 |
| Service Abstraction & DI [HIGH] | 4.3 ± 0.6 | 4.7 ± 0.6 | 4.3 ± 1.2 | 4.7 ± 0.6 | 4.3 ± 0.6 |
| Security Configuration [HIGH] | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 |
| DTO Design [MEDIUM] | 2.7 ± 0.6 | 5.0 | 4.7 ± 0.6 | 4.0 | 3.3 ± 0.6 |
| Sealed Types [MEDIUM] | 1.5 ± 0.7 | 4.5 ± 0.7 | 5.0 | 2.0 ± 1.4 | 3.0 ± 1.4 |
| Data Seeder Design [MEDIUM] | 3.7 ± 0.6 | 4.0 | 4.0 | 4.0 | 4.0 |
| Structured Logging [MEDIUM] | 4.0 | 4.0 | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.0 |
| Nullable Reference Types [MEDIUM] | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.5 ± 0.7 | 4.5 ± 0.7 |
| API Documentation [MEDIUM] | 3.3 ± 0.6 | 5.0 | 3.7 ± 0.6 | 3.0 ± 1.0 | 3.7 ± 0.6 |
| File Organization [MEDIUM] | 4.0 | 5.0 | 5.0 | 4.0 | 4.0 |
| HTTP Test File Quality [MEDIUM] | 4.0 | 5.0 | 4.0 | 4.0 | 4.0 |
| Type Design & Resource Management [MEDIUM] | 3.5 ± 0.7 | 4.5 ± 0.7 | 4.0 | 4.0 | 4.0 |
| Code Standards Compliance [LOW] | 3.0 | 5.0 | 4.3 ± 0.6 | 4.0 | 4.0 |
| Token Efficiency [MEDIUM] | 4.3 ± 0.6 | 4.3 ± 0.6 | 2.7 ± 1.2 | 2.7 ± 1.2 | 2.0 ± 1.0 |
| Scenario Coverage (all 3 apps) [MEDIUM] | 1.0 | 1.0 | 1.0 | 1.0 | 1.0 |

---

## Final Rankings

| Rank | Configuration | Mean Score | % of Max (217.5) | Std Dev | Min | Max |
|---|---|---|---|---|---|---|
| 🥇 | dotnet-webapi | 176.5 | 81% | 17.3 | 157.5 | 191.5 |
| 🥈 | dotnet-artisan | 160.5 | 74% | 27.3 | 130.0 | 182.5 |
| 🥉 | dotnet-skills | 141.7 | 65% | 10.1 | 130.0 | 148.0 |
| 4th | managedcode-dotnet-skills | 139.7 | 64% | 9.6 | 131.0 | 150.0 |
| 5th | no-skills | 125.2 | 58% | 13.3 | 110.5 | 136.5 |

---

## Weighted Score per Run

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 128.5 | 191.5 | 182.5 | 150.0 | 148.0 |
| 2 | 110.5 | 157.5 | 130.0 | 131.0 | 130.0 |
| 3 | 136.5 | 180.5 | 169.0 | 138.0 | 147.0 |
| **Mean** | **125.2** | **176.5** | **160.5** | **139.7** | **141.7** |

---

## Verification Summary (All Runs)

| Configuration | Build Pass Rate | Run Pass Rate | Avg Warnings |
|---|---|---|---|
| no-skills | 3/3 (100%) | 3/3 (100%) | 116.0 |
| dotnet-webapi | 3/3 (100%) | 3/3 (100%) | 132.0 |
| dotnet-artisan | 3/3 (100%) | 3/3 (100%) | 92.7 |
| managedcode-dotnet-skills | 3/3 (100%) | 3/3 (100%) | 134.0 |
| dotnet-skills | 3/3 (100%) | 3/3 (100%) | 108.7 |

---

## Token Usage Summary

Average token consumption per configuration across all runs.

| Configuration | Avg Input Tokens | Avg Output Tokens | Avg Cache Read | Avg API Calls | Avg Wall Time | Δ Input vs Baseline |
|---|---|---|---|---|---|---|
| no-skills | 2,302,594 | 50,516 | 2,152,268 | 35 | 11m 0s | — (baseline) |
| dotnet-webapi | 2,375,147 | 52,735 | 2,199,089 | 33 | 11m 51s | +3.2% |
| dotnet-artisan | 3,717,309 | 54,367 | 3,518,324 | 36 | 17m 1s | +61.4% |
| managedcode-dotnet-skills | 3,921,513 | 52,566 | 3,721,304 | 47 | 12m 38s | +70.3% |
| dotnet-skills | 5,738,030 | 82,012 | 5,387,456 | 87 | 14m 55s | +149.2% |

---

## Token Usage Per Run

| Configuration | Run | Scenario | Input Tokens | Output Tokens | Cache Read | API Calls | Wall Time |
|---|---|---|---|---|---|---|---|
| no-skills | 1 | FitnessStudioApi | 2,348,300 | 53,225 | 2,201,793 | 34 | 11m 22s |
| no-skills | 2 | LibraryApi | 2,111,275 | 49,999 | 1,951,727 | 32 | 10m 49s |
| no-skills | 3 | VetClinicApi | 2,448,208 | 48,323 | 2,303,283 | 38 | 10m 50s |
| dotnet-webapi | 1 | FitnessStudioApi | 2,390,904 | 55,952 | 2,223,400 | 31 | 12m 57s |
| dotnet-webapi | 2 | LibraryApi | 2,605,283 | 52,263 | 2,449,946 | 35 | 11m 54s |
| dotnet-webapi | 3 | VetClinicApi | 2,129,253 | 49,991 | 1,923,922 | 33 | 10m 43s |
| dotnet-artisan | 1 | FitnessStudioApi | 4,286,599 | 51,838 | 4,061,890 | 40 | 19m 36s |
| dotnet-artisan | 2 | LibraryApi | 2,692,257 | 53,173 | 2,542,911 | 34 | 17m 55s |
| dotnet-artisan | 3 | VetClinicApi | 4,173,072 | 58,091 | 3,950,170 | 35 | 13m 32s |
| managedcode-dotnet-skills | 1 | FitnessStudioApi | 4,548,270 | 55,939 | 4,373,101 | 45 | 13m 1s |
| managedcode-dotnet-skills | 2 | LibraryApi | 4,375,470 | 54,359 | 4,147,525 | 55 | 13m 19s |
| managedcode-dotnet-skills | 3 | VetClinicApi | 2,840,798 | 47,400 | 2,643,285 | 41 | 11m 34s |
| dotnet-skills | 1 | FitnessStudioApi | 8,897,537 | 133,544 | 8,160,452 | 163 | 17m 17s |
| dotnet-skills | 2 | LibraryApi | 3,233,174 | 52,071 | 3,074,168 | 42 | 12m 11s |
| dotnet-skills | 3 | VetClinicApi | 5,083,380 | 60,420 | 4,927,747 | 55 | 15m 16s |

---

## Consistency Analysis

| Configuration | Score σ | Most Consistent Dim (σ) | Most Variable Dim (σ) |
|---|---|---|---|
| no-skills | 13.3 | Scenario Coverage (0.0) | NuGet & Package Discipline (1.5) |
| dotnet-webapi | 17.3 | Scenario Coverage (0.0) | Security Vulnerability Scan (2.3) |
| dotnet-artisan | 27.3 | Scenario Coverage (0.0) | NuGet & Package Discipline (1.7) |
| managedcode-dotnet-skills | 9.6 | Scenario Coverage (0.0) | NuGet & Package Discipline (1.5) |
| dotnet-skills | 10.1 | Scenario Coverage (0.0) | Sealed Types (1.4) |

---

## Per-Dimension Analysis

### 1. Scenario Coverage [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 2 | 2 | 2 | 2 |
| 2 | — | — | — | — | — |
| 3 | — | — | — | — | — |
| **Mean** | **2.0** | **2.0** | **2.0** | **2.0** | **2.0** |

#### Analysis

```text
output/<config>/run-3 contains:
- VetClinicApi ✅
- FitnessStudioApi ❌
- LibraryApi ❌
```

Scores: all 1.

**Verdict:** Cross-scenario completeness is missing for every configuration, limiting comparison scope.

### 2. Build & Run Success [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 5 | 4 | 5 | 5 | 5 |
| 3 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **5.0** | **4.7** | **5.0** | **5.0** | **5.0** |

#### Analysis

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

### 3. Security Vulnerability Scan [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 5 | 4 |
| 2 | 5 | 1 | 5 | 5 | 5 |
| 3 | 5 | 5 | 5 | 5 | 5 |
| **Mean** | **4.7** | **3.7** | **5.0** | **5.0** | **4.7** |

#### Analysis

```text
# all configs
The given project `VetClinicApi` has no vulnerable packages given the current sources.
```

Scores: all 5. No vulnerable packages detected by `dotnet list package --vulnerable`.

**Verdict:** Tie. Dependency vulnerability posture is clean for this snapshot.

### 4. Minimal API Architecture [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 5 | 2 | 1 |
| 2 | 1 | 5 | 4 | 1 | 1 |
| 3 | 1 | 5 | 4 | 2 | 1 |
| **Mean** | **1.0** | **5.0** | **4.3** | **1.7** | **1.0** |

#### Analysis

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

### 5. Input Validation & Guard Clauses [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 3 | 4 | 4 |
| 2 | 3 | 4 | 3 | 4 | 4 |
| 3 | 3 | 4 | 4 | 3 | 3 |
| **Mean** | **3.0** | **4.0** | **3.3** | **3.7** | **3.7** |

#### Analysis

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

### 6. NuGet & Package Discipline [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 5 | 4 | 4 | 4 |
| 2 | 2 | 5 | 1 | 3 | 4 |
| 3 | 4 | 1 | 4 | 1 | 4 |
| **Mean** | **2.3** | **3.7** | **3.0** | **2.7** | **4.0** |

#### Analysis

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

### 7. EF Migration Usage [CRITICAL × 3]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 1 | 1 | 1 | 1 |
| 2 | 1 | 1 | 1 | 1 | 1 |
| 3 | 1 | 1 | 1 | 1 | 1 |
| **Mean** | **1.0** | **1.0** | **1.0** | **1.0** | **1.0** |

#### Analysis

```csharp
// all configs (Program.cs variants)
await context.Database.EnsureCreatedAsync();
// or
context.Database.EnsureCreated();
```

Scores: all 1. No configuration uses `Database.Migrate()` with migrations.

**Verdict:** Universal weakness. This is the largest shared production-readiness gap.

### 8. Business Logic Correctness [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 2 | 4 |
| 2 | 4 | 5 | 4 | 4 | 4 |
| 3 | 4 | 5 | 4 | 4 | 4 |
| **Mean** | **4.0** | **5.0** | **4.3** | **3.3** | **4.0** |

#### Analysis

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

### 9. Prefer Built-in over 3rd Party [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 3 | 2 | 2 |
| 2 | 1 | 5 | 3 | 2 | 2 |
| 3 | 2 | 5 | 2 | 2 | 4 |
| **Mean** | **1.7** | **5.0** | **2.7** | **2.0** | **2.7** |

#### Analysis

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

### 10. Modern C# Adoption [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 4 | 3 |
| 2 | 2 | 5 | 4 | 4 | 3 |
| 3 | 3 | 5 | 5 | 3 | 4 |
| **Mean** | **2.3** | **5.0** | **4.7** | **3.7** | **3.3** |

#### Analysis

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

### 11. Error Handling & Middleware [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 4 | 5 | 3 | 4 |
| 2 | 3 | 5 | 2 | 3 | 4 |
| 3 | 3 | 4 | 3 | 4 | 3 |
| **Mean** | **3.0** | **4.3** | **3.3** | **3.3** | **3.7** |

#### Analysis

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

### 12. Async Patterns & Cancellation [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 2 | 2 |
| 2 | 2 | 5 | 2 | 4 | 3 |
| 3 | 2 | 5 | 5 | 2 | 2 |
| **Mean** | **2.0** | **5.0** | **4.0** | **2.7** | **2.3** |

#### Analysis

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

### 13. EF Core Best Practices [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 3 | 4 | 4 | 4 |
| 2 | 3 | 5 | 5 | 4 | 4 |
| 3 | 3 | 5 | 5 | 4 | 4 |
| **Mean** | **2.7** | **4.3** | **4.7** | **4.0** | **4.0** |

#### Analysis

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

### 14. Service Abstraction & DI [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | 4 | 5 | 5 | 5 | 4 |
| 3 | 4 | 4 | 3 | 4 | 4 |
| **Mean** | **4.3** | **4.7** | **4.3** | **4.7** | **4.3** |

#### Analysis

```csharp
// no-skills Program.cs
builder.Services.AddScoped<IOwnerService, OwnerService>();

// dotnet-artisan
public interface IAppointmentService { ... }
public sealed class AppointmentService(...) : IAppointmentService
```

Scores: 4, 3, 4, 4, 4.

**Verdict:** Most are good; `dotnet-artisan` loses points for less consistent interface separation by file/structure.

### 15. Security Configuration [HIGH × 2]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 1 | 1 | 1 | 1 | 1 |
| 2 | 1 | 1 | 1 | 1 | 1 |
| 3 | 1 | 1 | 1 | 1 | 1 |
| **Mean** | **1.0** | **1.0** | **1.0** | **1.0** | **1.0** |

#### Analysis

```csharp
// Program.cs (all configs): no UseHsts(), no UseHttpsRedirection()
```

Scores: all 1.

**Verdict:** Universal production-hardening gap.

### 16. DTO Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 4 | 3 |
| 2 | 2 | 5 | 4 | 4 | 3 |
| 3 | 3 | 5 | 5 | 4 | 4 |
| **Mean** | **2.7** | **5.0** | **4.7** | **4.0** | **3.3** |

#### Analysis

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

### 17. Sealed Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 2 | 5 | 5 | 3 | 4 |
| 2 | — | — | — | — | — |
| 3 | 1 | 4 | 5 | 1 | 2 |
| **Mean** | **1.5** | **4.5** | **5.0** | **2.0** | **3.0** |

#### Analysis

```csharp
// dotnet-artisan
public sealed class AppointmentService ...
public sealed class GlobalExceptionHandler ...

// no-skills
public class AppointmentService : IAppointmentService
```

Scores: 1, 5, 1, 2, 4.

**Verdict:** `dotnet-artisan` is clearly strongest.

### 18. Data Seeder Design [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 | 4 |
| 2 | 4 | 4 | 4 | 4 | 4 |
| 3 | 3 | 4 | 4 | 4 | 4 |
| **Mean** | **3.7** | **4.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

```csharp
// all configs: Data/DataSeeder.cs called in Program.cs startup scope
await DataSeeder.SeedAsync(db); // or DataSeeder.Seed(context)
```

Scores: 3, 4, 4, 4, 4.

**Verdict:** All provide realistic seeders; `no-skills` is a bit less polished overall around surrounding patterns.

### 19. Structured Logging [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 5 | 5 | 4 |
| 2 | — | — | — | — | — |
| 3 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.0** | **4.0** | **4.5** | **4.5** | **4.0** |

#### Analysis

```csharp
// dotnet-webapi
logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, request.Status);

// no-skills
_logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId} with Vet {VetId}", ...);
```

Scores: all 4.

**Verdict:** Tie at good quality; all use structured placeholders.

### 20. Nullable Reference Types [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 5 | 5 | 5 | 5 | 5 |
| 2 | — | — | — | — | — |
| 3 | 4 | 4 | 4 | 4 | 4 |
| **Mean** | **4.5** | **4.5** | **4.5** | **4.5** | **4.5** |

#### Analysis

```xml
<!-- all configs: .csproj -->
<Nullable>enable</Nullable>
```

Scores: all 4.

**Verdict:** Solid baseline nullability posture across all five.

### 21. API Documentation [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 2 | 4 |
| 2 | 3 | 5 | 3 | 4 | 4 |
| 3 | 3 | 5 | 4 | 3 | 3 |
| **Mean** | **3.3** | **5.0** | **3.7** | **3.0** | **3.7** |

#### Analysis

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

### 22. File Organization [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 5 | 4 | 4 |
| 2 | — | — | — | — | — |
| 3 | 4 | 5 | 5 | 4 | 4 |
| **Mean** | **4.0** | **5.0** | **5.0** | **4.0** | **4.0** |

#### Analysis

```text
# dotnet-webapi / dotnet-artisan
Endpoints/, Services/, DTOs/, Middleware/, Data/

# no-skills / managed / dotnet-skills
Controllers/, Services/, DTOs/, Middleware/, Data/
```

Scores: 4, 5, 4, 4, 5.

**Verdict:** Minimal-API endpoint extension structure is most maintainable for these generated projects.

### 23. HTTP Test File Quality [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 5 | 4 | 4 | 4 |
| 2 | 4 | 5 | 4 | 4 | 4 |
| 3 | 4 | 5 | 4 | 4 | 4 |
| **Mean** | **4.0** | **5.0** | **4.0** | **4.0** | **4.0** |

#### Analysis

```http
# dotnet-webapi VetClinicApi.http
GET {{baseUrl}}/api/appointments?page=1&pageSize=10
PATCH {{baseUrl}}/api/appointments/6/status
```

All `.http` files are broad and realistic. `dotnet-webapi` is slightly better aligned with mapped routes and richer endpoint metadata.

Scores: 4, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` leads narrowly.

### 24. Type Design & Resource Management [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 4 | 4 | 4 |
| 2 | — | — | — | — | — |
| 3 | 3 | 5 | 4 | 4 | 4 |
| **Mean** | **3.5** | **4.5** | **4.0** | **4.0** | **4.0** |

#### Analysis

```csharp
// dotnet-webapi
public async Task<IReadOnlyList<AppointmentResponse>> GetTodayAsync(CancellationToken ct)

// no-skills
public async Task<List<AppointmentSummaryDto>> GetTodayAsync()
```

Scores: 3, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` has the most precise return types and enum usage consistency.

### 25. Code Standards Compliance [LOW × 0]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 3 | 5 | 5 | 4 | 4 |
| 2 | 3 | 5 | 4 | 4 | 4 |
| 3 | 3 | 5 | 4 | 4 | 4 |
| **Mean** | **3.0** | **5.0** | **4.3** | **4.0** | **4.0** |

#### Analysis

```csharp
// managedcode-dotnet-skills
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase

// dotnet-webapi
public sealed class AppointmentService(...) : IAppointmentService
```

Scores: 3, 4, 4, 4, 5.

**Verdict:** `dotnet-webapi` is the most consistently modern and convention-aligned.

### 26. Token Efficiency [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | 4 | 4 | 2 | 2 | 1 |
| 2 | 5 | 4 | 4 | 2 | 3 |
| 3 | 4 | 5 | 2 | 4 | 2 |
| **Mean** | **4.3** | **4.3** | **2.7** | **2.7** | **2.0** |

### 27. Scenario Coverage (all 3 apps) [MEDIUM × 1]

#### Scores Across Runs

| Run | no-skills | dotnet-webapi | dotnet-artisan | managedcode-dotnet-skills | dotnet-skills |
|---|---|---|---|---|---|
| 1 | — | — | — | — | — |
| 2 | — | — | — | — | — |
| 3 | 1 | 1 | 1 | 1 | 1 |
| **Mean** | **1.0** | **1.0** | **1.0** | **1.0** | **1.0** |

#### Analysis

```text
output/<config>/run-3 contains:
- VetClinicApi ✅
- FitnessStudioApi ❌
- LibraryApi ❌
```

Scores: all 1.

**Verdict:** Cross-scenario completeness is missing for every configuration, limiting comparison scope.

---

## Asset Usage Summary

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| no-skills | 1 | 3d221ee7…b060 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 2 | 8d159e82…5d92 | claude-opus-4.6-1m | — | — | ✅ |
| no-skills | 3 | a6d566ce…f64f | claude-opus-4.6-1m | — | — | ✅ |
| dotnet-webapi | 1 | 88ec51d6…8163 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-webapi | 2 | 4ea81de9…c8db | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-webapi | 3 | c5890f1b…66b1 | claude-opus-4.6-1m | dotnet-webapi | — | ✅ |
| dotnet-artisan | 1 | 8a16012c…24ab | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api | dotnet-artisan | ✅ |
| dotnet-artisan | 2 | d3493166…4f8a | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-csharp, dotnet-api, dotnet-tooling | dotnet-artisan | ✅ |
| dotnet-artisan | 3 | 7898f8bb…ab89 | claude-opus-4.6-1m | using-dotnet, dotnet-advisor, dotnet-api, dotnet-csharp | dotnet-artisan | ✅ |
| managedcode-dotnet-skills | 1 | 406ec9a8…34ed | claude-opus-4.6-1m | dotnet, dotnet-modern-csharp, dotnet-project-setup, dotnet-entity-framework-core, dotnet-microsoft-extensions, dotnet-aspnet-core, dotnet-minimal-apis | — | ✅ |
| managedcode-dotnet-skills | 2 | fe7444c2…1ec8 | None | — | — | ✅ |
| managedcode-dotnet-skills | 3 | 75f70326…8f8d | claude-opus-4.6-1m | dotnet, dotnet-aspnet-core, dotnet-modern-csharp, dotnet-entity-framework-core, dotnet-project-setup | — | ✅ |
| dotnet-skills | 1 | c6aebf53…20c6 | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |
| dotnet-skills | 2 | f896f3c2…291c | claude-opus-4.6-1m | optimizing-ef-core-queries, analyzing-dotnet-performance | dotnet-data, dotnet-diag | ✅ |
| dotnet-skills | 3 | ba79ee8f…57ff | claude-opus-4.6-1m | analyzing-dotnet-performance, optimizing-ef-core-queries | dotnet-diag, dotnet-data | ✅ |

---

## Raw Data References

- Per-run analysis: `reports/analysis-run-1.md`
- Per-run analysis: `reports/analysis-run-2.md`
- Per-run analysis: `reports/analysis-run-3.md`
- Verification data: `reports/verification-data.json`
- Score data: `reports/scores-data.json`
- Build notes: `reports/build-notes.md`
- Generation usage: `reports/generation-usage.json`
