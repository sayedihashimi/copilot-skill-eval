# Comparative Analysis: dotnet-artisan, dotnet-webapi, managedcode-dotnet-skills, dotnet-skills, no-skills

This run contains **5 configurations** under `output/*/run-2`. In the current repository snapshot, each configuration's `run-2` contains **LibraryApi** only (`FitnessStudioApi` and `VetClinicApi` are not present in `run-2`). Configuration attribution comes from `gen-notes.md` plus folder names: `dotnet-artisan`, `dotnet-webapi`, `managedcode-dotnet-skills`, `dotnet-skills`, `no-skills`.

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-webapi | managedcode-dotnet-skills | dotnet-skills | no-skills |
|---|---:|---:|---:|---:|---:|
| Build & Run Success [CRITICAL] | 5 | 4 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 5 | 1 | 5 | 5 | 5 |
| Minimal API Architecture [CRITICAL] | 4 | 5 | 1 | 1 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 4 | 4 | 4 | 3 |
| NuGet & Package Discipline [CRITICAL] | 1 | 5 | 3 | 4 | 2 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 4 | 5 | 4 | 4 | 4 |
| Prefer Built-in over 3rd Party [HIGH] | 3 | 5 | 2 | 2 | 1 |
| Modern C# Adoption [HIGH] | 4 | 5 | 4 | 3 | 2 |
| Error Handling & Middleware [HIGH] | 2 | 5 | 3 | 4 | 3 |
| Async Patterns & Cancellation [HIGH] | 2 | 5 | 4 | 3 | 2 |
| EF Core Best Practices [HIGH] | 5 | 5 | 4 | 4 | 3 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 5 | 4 | 4 |
| Security Configuration [HIGH] | 1 | 1 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 4 | 5 | 4 | 3 | 2 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 3 | 5 | 4 | 4 | 3 |
| HTTP Test File Quality [MEDIUM] | 4 | 5 | 4 | 4 | 4 |
| Code Standards Compliance [LOW] | 4 | 5 | 4 | 4 | 3 |

## 1. Build & Run Success [CRITICAL]

All five variants built and ran. `dotnet-webapi` had 2 warnings; others had 0.

```text
# dotnet-webapi (build/runtime checks)
Build succeeded with 2 Warning(s)
Now listening on: http://127.0.0.1:6101
```

```text
# dotnet-artisan (build/runtime checks)
Build succeeded with 0 Warning(s)
Now listening on: http://127.0.0.1:6100
```

**Score**: dotnet-artisan **5**, dotnet-webapi **4**, managedcode-dotnet-skills **5**, dotnet-skills **5**, no-skills **5**.

**Verdict**: All are runnable. `dotnet-webapi` is penalized only for warning noise.

## 2. Security Vulnerability Scan [CRITICAL]

`dotnet-webapi` has a transitive high-severity advisory; others were clean.

```text
# dotnet-webapi vulnerability scan
Transitive Package: Microsoft.Build.Tasks.Core 17.7.2
Severity: High
https://github.com/advisories/GHSA-h4j7-5rxr-p4wc
```

```text
# dotnet-skills vulnerability scan
The given project `LibraryApi` has no vulnerable packages
```

**Score**: dotnet-artisan **5**, dotnet-webapi **1**, managedcode-dotnet-skills **5**, dotnet-skills **5**, no-skills **5**.

**Verdict**: Non-negotiable gate; `dotnet-webapi` is materially weaker until dependency chain is remediated.

## 3. Minimal API Architecture [CRITICAL]

`dotnet-webapi` and `dotnet-artisan` are minimal-API-first. Others use controllers.

```csharp
// dotnet-webapi: Endpoints/AuthorEndpoints.cs
var group = app.MapGroup("/api/authors").WithTags("Authors");
group.MapGet("/{id}", async Task<Results<Ok<AuthorDetailResponse>, NotFound>> (...) => ...);
```

```csharp
// dotnet-skills: Controllers/AuthorsController.cs
[ApiController]
[Route("api/[controller]")]
public sealed class AuthorsController : ControllerBase
```

**Score**: dotnet-artisan **4**, dotnet-webapi **5**, managedcode-dotnet-skills **1**, dotnet-skills **1**, no-skills **1**.

**Verdict**: `dotnet-webapi` best matches modern minimal-API guidance (route groups + typed unions).

## 4. Input Validation & Guard Clauses [CRITICAL]

All variants use DataAnnotations on request DTOs; guard-clause patterns at service boundaries are inconsistent.

```csharp
// dotnet-webapi: DTOs/LoanDtos.cs
public sealed record CreateLoanRequest
{
    [Required] public required int BookId { get; init; }
    [Required] public required int PatronId { get; init; }
}
```

```csharp
// no-skills: DTOs/LoanDtos.cs
public class CreateLoanRequest
{
    [Required] public int BookId { get; set; }
    [Required] public int PatronId { get; set; }
}
```

**Score**: dotnet-artisan **3**, dotnet-webapi **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**.

**Verdict**: Controller variants benefit from `[ApiController]` auto-400 behavior. `dotnet-webapi` has strong DTO contracts but still limited explicit guard-clause usage.

## 5. NuGet & Package Discipline [CRITICAL]

`dotnet-webapi` pins exact versions and keeps package set minimal. `dotnet-artisan` and `no-skills` use floating versions.

```xml
<!-- dotnet-artisan: LibraryApi.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.*-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

```xml
<!-- dotnet-webapi: LibraryApi.csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0-preview.3.25172.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-preview.3.25171.5" />
```

**Score**: dotnet-artisan **1**, dotnet-webapi **5**, managedcode-dotnet-skills **3**, dotnet-skills **4**, no-skills **2**.

**Verdict**: Exact pinning + least dependency surface is the strongest operational posture.

## 6. EF Migration Usage [CRITICAL]

All variants use `EnsureCreated`/`EnsureCreatedAsync` instead of migrations.

```csharp
// dotnet-webapi: Program.cs
await db.Database.EnsureCreatedAsync();
```

```csharp
// managedcode-dotnet-skills: Program.cs
db.Database.EnsureCreated();
```

**Score**: dotnet-artisan **1**, dotnet-webapi **1**, managedcode-dotnet-skills **1**, dotnet-skills **1**, no-skills **1**.

**Verdict**: Universal weakness; production-safe schema evolution requires `Database.Migrate()` and checked-in migration history.

## 7. Business Logic Correctness [HIGH]

All five variants register the required endpoint surface (authors/categories/books/patrons/loans/reservations/fines). `dotnet-webapi` is the most explicit in endpoint contracts.

```csharp
// dotnet-webapi: Endpoints/LoanEndpoints.cs
group.MapPost("/{id}/renew", async Task<Ok<LoanResponse>> (...) => ...);
group.MapGet("/overdue", async Task<Ok<PaginatedResponse<LoanResponse>>> (...) => ...);
```

```csharp
// no-skills: Controllers/LoansController.cs
[HttpPost("{id}/renew")]
[HttpGet("overdue")]
```

**Score**: dotnet-artisan **4**, dotnet-webapi **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **4**.

**Verdict**: Functional completeness is generally strong across variants; `dotnet-webapi` has the clearest contract-level precision.

## 8. Prefer Built-in over 3rd Party [HIGH]

`dotnet-webapi` is closest to built-in OpenAPI-first. Others rely on Swashbuckle packages/patterns.

```csharp
// dotnet-webapi: Program.cs
builder.Services.AddOpenApi();
app.MapOpenApi();
app.UseSwaggerUI(...);
```

```csharp
// no-skills: Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
app.UseSwagger();
```

**Score**: dotnet-artisan **3**, dotnet-webapi **5**, managedcode-dotnet-skills **2**, dotnet-skills **2**, no-skills **1**.

**Verdict**: Built-in OpenAPI stack is the better forward-compatible default.

## 9. Modern C# Adoption [HIGH]

`dotnet-webapi` and `dotnet-artisan` use sealed records and concise patterns; `no-skills` is more traditional mutable-class-heavy.

```csharp
// dotnet-artisan: DTOs/LoanDtos.cs
public sealed record LoanResponse(...);
```

```csharp
// no-skills: DTOs/LoanDtos.cs
public class LoanDto
{
    public int Id { get; set; }
}
```

**Score**: dotnet-artisan **4**, dotnet-webapi **5**, managedcode-dotnet-skills **4**, dotnet-skills **3**, no-skills **2**.

**Verdict**: Stronger modern-language adoption correlates with lower boilerplate and clearer contracts.

## 10. Error Handling & Middleware [HIGH]

`dotnet-webapi` has modern `IExceptionHandler` with status mapping. `dotnet-artisan` catches all as 500.

```csharp
// dotnet-webapi: Middleware/ApiExceptionHandler.cs
KeyNotFoundException => (404, "Not Found"),
ArgumentException => (400, "Bad Request"),
InvalidOperationException => (409, "Conflict")
```

```csharp
// dotnet-artisan: Services/GlobalExceptionHandler.cs
var problemDetails = new ProblemDetails
{
    Status = StatusCodes.Status500InternalServerError
};
```

**Score**: dotnet-artisan **2**, dotnet-webapi **5**, managedcode-dotnet-skills **3**, dotnet-skills **4**, no-skills **3**.

**Verdict**: Explicit exception-to-status mapping with RFC7807 payloads is the strongest client-facing behavior.

## 11. Async Patterns & Cancellation [HIGH]

`dotnet-webapi` consistently threads `CancellationToken` through endpoint and service calls.

```csharp
// dotnet-webapi: Endpoints/LoanEndpoints.cs
group.MapPost("/", async Task<Created<LoanResponse>> (
    CreateLoanRequest request, ILoanService service, CancellationToken ct) =>
{
    var result = await service.CheckoutAsync(request, ct);
    return TypedResults.Created($"/api/loans/{result.Id}", result);
});
```

```csharp
// dotnet-artisan: Endpoints/LoanEndpoints.cs
group.MapPost("/", async Task<Results<Created<LoanResponse>, ProblemHttpResult>> (
    CreateLoanRequest request, ILoanService service) => ...);
```

**Score**: dotnet-artisan **2**, dotnet-webapi **5**, managedcode-dotnet-skills **4**, dotnet-skills **3**, no-skills **2**.

**Verdict**: Cancellation propagation is a meaningful reliability/perf differentiator under load.

## 12. EF Core Best Practices [HIGH]

All variants have fluent relationship mapping. `dotnet-artisan`/`dotnet-webapi` are strongest on query discipline (`AsNoTracking`).

```csharp
// dotnet-artisan: Services/BookService.cs
var query = db.Books
    .AsNoTracking()
    .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
    .Include(b => b.BookCategories).ThenInclude(bc => bc.Category);
```

```csharp
// no-skills: Services/BookService.cs
var query = _db.Books
    .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
    .Include(b => b.BookCategories).ThenInclude(bc => bc.Category);
```

**Score**: dotnet-artisan **5**, dotnet-webapi **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**.

**Verdict**: Read-query no-tracking + explicit relationship configuration remains the best baseline.

## 13. Service Abstraction & DI [HIGH]

All variants use interface-based service wiring with scoped lifetime.

```csharp
// dotnet-webapi: Program.cs
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ILoanService, LoanService>();
```

```csharp
// no-skills: Program.cs
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
```

**Score**: dotnet-artisan **5**, dotnet-webapi **5**, managedcode-dotnet-skills **5**, dotnet-skills **4**, no-skills **4**.

**Verdict**: All are testable and DI-friendly; slight simplicity advantage to cleaner service separation.

## 14. Security Configuration [HIGH]

No variant configures HSTS/HTTPS redirection in `Program.cs`.

```csharp
// representative (dotnet-webapi Program.cs)
// No app.UseHsts();
// No app.UseHttpsRedirection();
```

**Score**: dotnet-artisan **1**, dotnet-webapi **1**, managedcode-dotnet-skills **1**, dotnet-skills **1**, no-skills **1**.

**Verdict**: Shared production-readiness gap.

## 15. DTO Design [MEDIUM]

`dotnet-webapi` is strongest: sealed record-based request/response DTOs with explicit naming conventions.

```csharp
// dotnet-webapi: DTOs/LoanDtos.cs
public sealed record LoanResponse(...);
public sealed record CreateLoanRequest { ... }
```

```csharp
// no-skills: DTOs/LoanDtos.cs
public class LoanDto { ... }
public class CreateLoanRequest { ... }
```

**Score**: dotnet-artisan **4**, dotnet-webapi **5**, managedcode-dotnet-skills **4**, dotnet-skills **3**, no-skills **2**.

**Verdict**: Immutable record DTOs create cleaner contracts and reduce accidental mutation.

## 16. Data Seeder Design [MEDIUM]

All variants seed only when empty and provide realistic, varied data.

```csharp
// managedcode-dotnet-skills: Data/DataSeeder.cs
if (await db.Authors.AnyAsync())
    return; // Already seeded
```

```csharp
// dotnet-skills: Data/DataSeeder.cs
if (context.Authors.Any()) return;
```

**Score**: dotnet-artisan **4**, dotnet-webapi **4**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **4**.

**Verdict**: Strong parity; all include enough states to exercise business rules.

## 17. API Documentation [MEDIUM]

`dotnet-webapi` has the richest endpoint metadata (`WithSummary`, `WithDescription`, `Produces`).

```csharp
// dotnet-webapi: Endpoints/LoanEndpoints.cs
.WithSummary("Renew a loan")
.WithDescription("Renew an active loan...")
.Produces<LoanResponse>(StatusCodes.Status200OK)
```

```csharp
// dotnet-artisan: Endpoints/LoanEndpoints.cs
.WithSummary("Renew a loan — enforce all renewal rules");
```

**Score**: dotnet-artisan **3**, dotnet-webapi **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**.

**Verdict**: High-fidelity OpenAPI metadata materially improves client generation and dev UX.

## 18. HTTP Test File Quality [MEDIUM]

All variants include broad `.http` coverage; `dotnet-webapi` is the most complete (65 HTTP verbs captured).

```text
# dotnet-webapi: LibraryApi.http
~65 request lines (GET/POST/PUT/DELETE)
```

```text
# dotnet-skills: LibraryApi.http
~60 request lines (GET/POST/PUT/DELETE)
```

**Score**: dotnet-artisan **4**, dotnet-webapi **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **4**.

**Verdict**: All are useful as executable docs; `dotnet-webapi` is best for breadth and scenario realism.

## 19. Code Standards Compliance [LOW]

Generally good naming and structure across all variants, with strongest consistency in `dotnet-webapi`.

```csharp
// dotnet-webapi
internal sealed class ApiExceptionHandler : IExceptionHandler
```

```csharp
// managedcode-dotnet-skills
public class GlobalExceptionHandlerMiddleware
```

**Score**: dotnet-artisan **4**, dotnet-webapi **5**, managedcode-dotnet-skills **4**, dotnet-skills **4**, no-skills **3**.

**Verdict**: No major convention failures; differences are mostly around modernity and explicitness.

## Weighted Summary

Weights: Critical ×3, High ×2, Medium ×1, Low ×0.5.

| Configuration | Weighted Total |
|---|---:|
| dotnet-webapi | **153.5** |
| managedcode-dotnet-skills | **129.0** |
| dotnet-skills | **127.0** |
| dotnet-artisan | **126.0** |
| no-skills | **105.5** |

## What All Versions Get Right

- All five `LibraryApi` variants build and start successfully.
- All use EF Core with SQLite and clear domain/entity separation.
- All implement service interfaces and DI registration for domain services.
- All include substantial seed data and idempotent seed guards.
- All provide OpenAPI exposure and a companion `.http` test file.
- All expose the full expected endpoint surface for the library domain.

## Summary: Impact of Skills

Most impactful differences:

- **Architecture mode**: `dotnet-webapi` and `dotnet-artisan` generated modern minimal APIs; the other three stayed controller-centric.
- **Dependency hygiene**: exact pinning (`dotnet-webapi`) vs floating versions (`dotnet-artisan`, `no-skills`) strongly affected reproducibility and risk.
- **Error/cancellation quality**: `dotnet-webapi` had the strongest end-to-end modern pipeline (`IExceptionHandler` + cancellation propagation).
- **Security scan outcome**: despite architectural strength, `dotnet-webapi` had a transitive high-severity vulnerability in this run.

Overall assessment:

- **1st: dotnet-webapi** — best modern API architecture and contract quality, with one major dependency-security caveat.
- **2nd: managedcode-dotnet-skills** — strong controller-based implementation quality and broad correctness.
- **3rd: dotnet-skills** — similar strengths, slightly less modern consistency.
- **4th: dotnet-artisan** — excellent EF/query quality and minimal APIs, but package/version discipline and error mapping reduce score.
- **5th: no-skills** — functional baseline, but older patterns and weaker package/security posture.
