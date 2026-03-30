# Comparative Analysis: dotnet-artisan, dotnet-webapi, managedcode-dotnet-skills, dotnet-skills, no-skills

This run contains **4 discovered configuration directories** under `output/` (`dotnet-artisan`, `dotnet-skills`, `managedcode-dotnet-skills`, `no-skills`). The expected `dotnet-webapi` directory was not present. Inside each discovered config's `run-2`, only `LibraryApi` exists; `FitnessStudioApi` and `VetClinicApi` are missing. Configuration identity was taken from `gen-notes.md` content where available, otherwise inferred from directory name.

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-webapi | managedcode-dotnet-skills | dotnet-skills | no-skills |
|---|---:|---:|---:|---:|---:|
| Scenario Coverage Across Apps [CRITICAL] | 2 | 1 | 2 | 2 | 2 |
| Build & Run Success [CRITICAL] | 5 | 1 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 5 | 1 | 5 | 5 | 5 |
| Minimal API Architecture [CRITICAL] | 5 | 1 | 2 | 2 | 2 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 1 | 3 | 4 | 3 |
| NuGet & Package Discipline [CRITICAL] | 4 | 1 | 2 | 2 | 3 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 4 | 1 | 4 | 4 | 3 |
| Prefer Built-in over 3rd Party [HIGH] | 3 | 1 | 2 | 2 | 2 |
| Modern C# Adoption [HIGH] | 5 | 1 | 4 | 4 | 2 |
| Error Handling & Middleware [HIGH] | 4 | 1 | 4 | 3 | 3 |
| Async Patterns & Cancellation [HIGH] | 3 | 1 | 3 | 3 | 2 |
| EF Core Best Practices [HIGH] | 4 | 1 | 4 | 4 | 2 |
| Service Abstraction & DI [HIGH] | 5 | 1 | 5 | 5 | 4 |
| Security Configuration [HIGH] | 1 | 1 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 5 | 1 | 2 | 4 | 2 |
| Sealed Types [MEDIUM] | 5 | 1 | 1 | 3 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 1 | 4 | 4 | 2 |
| Structured Logging [MEDIUM] | 5 | 1 | 4 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 4 | 1 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 5 | 1 | 4 | 4 | 4 |
| File Organization [MEDIUM] | 5 | 1 | 4 | 4 | 4 |
| HTTP Test File Quality [MEDIUM] | 5 | 1 | 4 | 4 | 4 |
| Type Design & Resource Management [MEDIUM] | 5 | 1 | 4 | 4 | 3 |
| Code Standards Compliance [LOW] | 5 | 1 | 3 | 4 | 3 |

## 1. Scenario Coverage Across Apps [CRITICAL]

```text
# dotnet-artisan
output/dotnet-artisan/run-2/LibraryApi

# dotnet-skills
output/dotnet-skills/run-2/LibraryApi

# managedcode-dotnet-skills
output/managedcode-dotnet-skills/run-2/LibraryApi

# no-skills
output/no-skills/run-2/LibraryApi

# dotnet-webapi
(no directory found under output/)
```

Scores: dotnet-artisan 2, dotnet-webapi 1, managedcode-dotnet-skills 2, dotnet-skills 2, no-skills 2.

Verdict: All available configs failed scenario completeness (only 1/3 apps generated), while `dotnet-webapi` has no run artifacts.

## 2. Build & Run Success [CRITICAL]

```bash
# all discovered configs (LibraryApi)
dotnet build --nologo   # Build succeeded, 0 Warning(s), 0 Error(s)
dotnet run --no-build   # process remained alive for 12s timeout
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 5, dotnet-skills 5, no-skills 5.

Verdict: Runtime viability is strong for the four available outputs.

## 3. Security Vulnerability Scan [CRITICAL]

```bash
# all discovered configs (LibraryApi)
dotnet list package --vulnerable --include-transitive
# ...has no vulnerable packages given the current sources.
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 5, dotnet-skills 5, no-skills 5.

Verdict: Dependency CVE posture is currently clean.

## 4. Minimal API Architecture [CRITICAL]

```csharp
// dotnet-artisan: Endpoints + route groups + TypedResults
var group = routes.MapGroup("/api/loans").WithTags("Loans");
group.MapGet("/{id:int}", async Task<Results<Ok<LoanResponse>, NotFound>> (...) => ...);
return TypedResults.Ok(result);

// dotnet-skills / managedcode-dotnet-skills / no-skills: Controllers
[ApiController]
[Route("api/[controller]")]
public sealed class LoansController : ControllerBase
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 2, dotnet-skills 2, no-skills 2.

Verdict: `dotnet-artisan` is the only output aligned to modern Minimal API route-group patterns.

## 5. Input Validation & Guard Clauses [CRITICAL]

```csharp
// dotnet-skills: FluentValidation
RuleFor(x => x.ISBN).NotEmpty().Matches(@"^(?:\d{9}[\dX]|\d{13})$");
RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(1);

// dotnet-artisan: Data annotations only
[Required, MaxLength(300)] public string Title { get; init; } = string.Empty;

// no-skills: Data annotations + mutable DTO classes
[Required, MaxLength(300)] public string Title { get; set; } = string.Empty;
```

Scores: dotnet-artisan 3, dotnet-webapi 1, managedcode-dotnet-skills 3, dotnet-skills 4, no-skills 3.

Verdict: `dotnet-skills` is strongest due explicit validator layer; all variants lack consistent `ArgumentNullException.ThrowIfNull` guard-clause style.

## 6. NuGet & Package Discipline [CRITICAL]

```xml
<!-- dotnet-artisan -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />

<!-- dotnet-skills -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.*" />

<!-- managedcode-dotnet-skills -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

Scores: dotnet-artisan 4, dotnet-webapi 1, managedcode-dotnet-skills 2, dotnet-skills 2, no-skills 3.

Verdict: Exact pinning in `dotnet-artisan` and `no-skills` is safer; wildcard versions are a reproducibility and supply-chain risk.

## 7. EF Migration Usage [CRITICAL]

```csharp
// dotnet-artisan / dotnet-skills / managedcode-dotnet-skills
await db.Database.EnsureCreatedAsync();

// no-skills
context.Database.EnsureCreated();
```

Scores: dotnet-artisan 1, dotnet-webapi 1, managedcode-dotnet-skills 1, dotnet-skills 1, no-skills 1.

Verdict: All outputs use `EnsureCreated` anti-pattern instead of migrations (`Migrate/MigrateAsync`).

## 8. Business Logic Correctness [HIGH]

```csharp
// dotnet-artisan LoanService
if (unpaidFines >= 10.00m) throw new InvalidOperationException(...);
if (loan.RenewalCount >= 2) throw new InvalidOperationException(...);
var nextReservation = await db.Reservations.Where(...).OrderBy(...).FirstOrDefaultAsync();

// managedcode-dotnet-skills LoanService
if (hasPendingReservations) throw new BusinessRuleException(...);
```

Scores: dotnet-artisan 4, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 3.

Verdict: Library business rules are broadly complete in available configs, but run-level scenario incompleteness limits overall functional delivery.

## 9. Prefer Built-in over 3rd Party [HIGH]

```csharp
// dotnet-artisan Program.cs
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// dotnet-skills Program.cs
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddSwaggerGen();

// no-skills Program.cs
builder.Services.AddSwaggerGen(...);
```

Scores: dotnet-artisan 3, dotnet-webapi 1, managedcode-dotnet-skills 2, dotnet-skills 2, no-skills 2.

Verdict: None are purely first-party; `dotnet-artisan` is closest but still relies on third-party API docs UI package.

## 10. Modern C# Adoption [HIGH]

```csharp
// dotnet-artisan
public sealed class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
public List<int> AuthorIds { get; init; } = [];

// no-skills
public class LoanService : ILoanService
{
    private readonly LibraryDbContext _db;
}
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 2.

Verdict: `dotnet-artisan` demonstrates the most consistent C# 12+ idioms.

## 11. Error Handling & Middleware [HIGH]

```csharp
// managedcode-dotnet-skills
public class GlobalExceptionHandler(...) : IExceptionHandler
{
    var (statusCode, title) = exception switch { ... };
}

// dotnet-skills / no-skills
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

Scores: dotnet-artisan 4, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 3, no-skills 3.

Verdict: `managedcode-dotnet-skills` uses the most modern pipeline abstraction (`IExceptionHandler`).

## 12. Async Patterns & Cancellation [HIGH]

```csharp
// all available configs use async service methods
public async Task<LoanDto> RenewLoanAsync(int loanId)

// cancellation propagation is mostly absent
// (very few/no CancellationToken parameters through endpoint->service->EF chains)
```

Scores: dotnet-artisan 3, dotnet-webapi 1, managedcode-dotnet-skills 3, dotnet-skills 3, no-skills 2.

Verdict: Async usage is acceptable, but cancellation propagation is weak across the board.

## 13. EF Core Best Practices [HIGH]

```csharp
// dotnet-skills
_db.Books.AsNoTracking().Include(...).AsSplitQuery();

// no-skills (BookService read path)
_db.Books.Include(...).AsQueryable(); // no AsNoTracking in key read query
```

Scores: dotnet-artisan 4, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 2.

Verdict: `dotnet-artisan`, `dotnet-skills`, and `managedcode-dotnet-skills` consistently apply read-query optimizations.

## 14. Service Abstraction & DI [HIGH]

```csharp
// all discovered configs
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ILoanService, LoanService>();
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 5, dotnet-skills 5, no-skills 4.

Verdict: Interface-first DI is consistently strong in available outputs.

## 15. Security Configuration [HIGH]

```csharp
// observed Program.cs across available configs
// no app.UseHttpsRedirection();
// no app.UseHsts();
```

Scores: dotnet-artisan 1, dotnet-webapi 1, managedcode-dotnet-skills 1, dotnet-skills 1, no-skills 1.

Verdict: Production transport security middleware is missing everywhere.

## 16. DTO Design [MEDIUM]

```csharp
// dotnet-artisan
public sealed record CreateBookRequest { [Required] public string Title { get; init; } = string.Empty; }

// managedcode-dotnet-skills
public class CreateBookDto { [Required] public string Title { get; set; } = string.Empty; }
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 2, dotnet-skills 4, no-skills 2.

Verdict: Immutable record-based DTOs in `dotnet-artisan` are the cleanest contract style.

## 17. Sealed Types [MEDIUM]

```csharp
// dotnet-artisan
public sealed class BookService ...
public sealed record BookResponse(...);

// no-skills
public class BookService : IBookService
public class LoansController : ControllerBase
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 1, dotnet-skills 3, no-skills 1.

Verdict: `dotnet-artisan` best communicates non-inheritance intent and unlocks JIT devirtualization opportunities.

## 18. Data Seeder Design [MEDIUM]

```csharp
// dotnet-artisan
if (await db.Authors.AnyAsync()) { return; }

// no-skills
public static void Seed(LibraryDbContext context)
{
    context.Database.EnsureCreated();
    if (context.Authors.Any()) return;
}
```

Scores: dotnet-artisan 4, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 2.

Verdict: Async + idempotent seeding is good in three configs; synchronous EnsureCreated seeding is weaker.

## 19. Structured Logging [MEDIUM]

```csharp
// dotnet-artisan
logger.LogInformation("Loan {LoanId} renewed (renewal #{Count}), new due date: {DueDate}", ...);

// no-skills
_logger.LogInformation("Created book {BookId}: {Title}", book.Id, book.Title);
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 4.

Verdict: Structured templates are generally good; `dotnet-artisan` is most pervasive/consistent.

## 20. Nullable Reference Types [MEDIUM]

```xml
<!-- all discovered configs -->
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

Scores: dotnet-artisan 4, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 4.

Verdict: NRT is enabled consistently in all available projects.

## 21. API Documentation [MEDIUM]

```csharp
// dotnet-artisan endpoints
.WithName("GetBookById")
.WithSummary("Get book details including authors, categories, and availability");

// dotnet-skills/no-skills controllers
[ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 4.

Verdict: `dotnet-artisan` yields the best OpenAPI metadata density for Minimal APIs.

## 22. File Organization [MEDIUM]

```text
# dotnet-artisan
Endpoints/, Services/, DTOs/, Data/, Models/

# controller-based variants
Controllers/, Services/, DTOs/, Data/, Models/, Middleware/
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 4.

Verdict: All available outputs are organized; endpoint-extension layout keeps `Program.cs` cleanest.

## 23. HTTP Test File Quality [MEDIUM]

```http
# dotnet-artisan LibraryApi.http
### BUSINESS RULE TEST: Check out for patron with too many unpaid fines
POST {{baseUrl}}/api/loans

# dotnet-skills LibraryApi.http
### FAIL: Renew an overdue loan
POST {{baseUrl}}/api/loans/8/renew
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 4.

Verdict: Test files are comprehensive in available projects, including negative-path scenarios.

## 24. Type Design & Resource Management [MEDIUM]

```csharp
// dotnet-artisan
public async Task<IReadOnlyList<LoanResponse>> GetOverdueLoansAsync()
modelBuilder.Entity<Loan>().Property(l => l.Status).HasConversion<string>();

// no-skills
public async Task<PagedResult<LoanResponseDto>> GetOverdueLoansAsync(int page, int pageSize)
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 4, dotnet-skills 4, no-skills 3.

Verdict: Strong enum/return-type precision in artisan and skill-based outputs; baseline is adequate but less disciplined.

## 25. Code Standards Compliance [LOW]

```csharp
// dotnet-artisan
namespace LibraryApi.Endpoints;
public static class LoanEndpoints

// dotnet-skills
public sealed class LoansController : ControllerBase

// managedcode-dotnet-skills
public class LoansController(ILoanService loanService) : ControllerBase
```

Scores: dotnet-artisan 5, dotnet-webapi 1, managedcode-dotnet-skills 3, dotnet-skills 4, no-skills 3.

Verdict: `dotnet-artisan` has the most consistent modern style (file-scoped namespaces, explicit modifiers, naming consistency).

## Weighted Summary

Weights used: Critical ×3, High ×2, Medium ×1, Low ×0.5.

| Configuration | Weighted Total |
|---|---:|
| dotnet-artisan | **178.5** |
| dotnet-skills | **152.0** |
| managedcode-dotnet-skills | **146.5** |
| no-skills | **130.5** |
| dotnet-webapi | **46.5** |

## What All Versions Get Right

- All discovered `LibraryApi` projects build successfully with zero compiler warnings/errors.
- All discovered `LibraryApi` projects run successfully (stable process during timeout window).
- `dotnet list package --vulnerable --include-transitive` reported no known vulnerable packages.
- All discovered variants use EF Core + SQLite with explicit relationship configuration.
- All discovered variants provide broad endpoint coverage for the library scenario and include `.http` request collections.

## Summary: Impact of Skills

Most impactful differences (highest practical effect):

1. **Architecture shape**: `dotnet-artisan` is the only config consistently delivering Minimal APIs + route groups + TypedResults.
2. **Dependency hygiene**: wildcard NuGet versioning in `dotnet-skills` and `managedcode-dotnet-skills` materially lowers reproducibility quality.
3. **Validation strategy**: `dotnet-skills` benefits from dedicated FluentValidation rules; others rely mostly on annotations/service checks.
4. **Modern C# and type sealing**: `dotnet-artisan` is substantially more modern and explicit.
5. **Run completeness**: all discovered configs missed `FitnessStudioApi` and `VetClinicApi`; `dotnet-webapi` output is entirely missing.

Overall assessment by weighted score:

- **1st: dotnet-artisan** — best architecture and modern coding quality.
- **2nd: dotnet-skills** — strong functional output, weaker package discipline.
- **3rd: managedcode-dotnet-skills** — good behavior and middleware, but older architecture style and weak sealing discipline.
- **4th: no-skills** — functional baseline, but weakest technical quality among available outputs.
- **5th: dotnet-webapi** — no generated run-2 artifacts available for assessment.
