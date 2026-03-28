# Comparative Analysis: dotnet-artisan, dotnet-webapi, managedcode-dotnet-skills, dotnet-skills, no-skills

## Introduction

This report compares **5 Copilot configurations** used to generate the same **LibraryApi** application — a community library management system with book loans, reservations, overdue fines, and availability tracking. Each configuration produced the app in the `output/{config}/run-2/LibraryApi/` directory.

| Configuration | Label | Key Characteristics |
|---|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain | 5-skill chain (using-dotnet → dotnet-advisor → dotnet-csharp → dotnet-api → dotnet-webapi) |
| **dotnet-webapi** | dotnet-webapi skill | Single focused Web API skill |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | Multi-plugin suite (dotnet, dotnet-data, dotnet-nuget, etc.) |
| **managedcode-dotnet-skills** | Community managed-code skills | Community-maintained .NET skills |
| **no-skills** | Baseline (default Copilot) | No custom skills or plugins |

All five configurations produced a single LibraryApi project targeting .NET 10 with EF Core and SQLite, implementing 7 resource areas (Authors, Categories, Books, Patrons, Loans, Reservations, Fines) with ~37 endpoints and comprehensive business rules.

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 3 | 4 | 3 | 5 | 4 |
| Minimal API Architecture [CRITICAL] | 5 | 5 | 1 | 1 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 3 | 4 | 3 | 3 |
| NuGet & Package Discipline [CRITICAL] | 2 | 2 | 3 | 5 | 3 |
| EF Migration Usage [CRITICAL] | 1 | 5 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 5 | 5 | 4 | 4 | 4 |
| Prefer Built-in over 3rd Party [HIGH] | 2 | 5 | 1 | 5 | 2 |
| Modern C# Adoption [HIGH] | 5 | 5 | 3 | 5 | 2 |
| Error Handling & Middleware [HIGH] | 5 | 5 | 5 | 5 | 2 |
| Async Patterns & Cancellation [HIGH] | 5 | 5 | 2 | 5 | 1 |
| EF Core Best Practices [HIGH] | 5 | 5 | 5 | 4 | 2 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 5 | 5 | 4 |
| Security Configuration [HIGH] | 1 | 3 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 5 | 5 | 2 | 5 | 2 |
| Sealed Types [MEDIUM] | 5 | 5 | 2 | 5 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 5 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 | 3 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 5 | 5 | 4 | 4 | 3 |
| File Organization [MEDIUM] | 5 | 5 | 3 | 4 | 3 |
| HTTP Test File Quality [MEDIUM] | 5 | 5 | 4 | 4 | 4 |
| Type Design & Resource Management [MEDIUM] | 5 | 5 | 4 | 4 | 3 |
| Code Standards Compliance [LOW] | 5 | 5 | 4 | 4 | 3 |

---

## 1. Build & Run Success [CRITICAL]

All five configurations produce projects that compile cleanly with **zero errors and zero warnings**.

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

This result is consistent across all configurations — every generated project is immediately buildable.

**Scores:**
- dotnet-artisan: **5** — Clean build, 0 errors, 0 warnings
- dotnet-webapi: **5** — Clean build, 0 errors, 0 warnings
- dotnet-skills: **5** — Clean build, 0 errors, 0 warnings
- managedcode: **5** — Clean build, 0 errors, 0 warnings
- no-skills: **5** — Clean build, 0 errors, 0 warnings

**Verdict:** Tie. All configurations meet this fundamental quality gate.

---

## 2. Security Vulnerability Scan [CRITICAL]

Security posture is assessed by examining NuGet packages for known-vulnerable or deprecated dependencies and unnecessary development packages.

| Configuration | Swashbuckle | FluentValidation | Wildcards | Extra Packages |
|---|---|---|---|---|
| dotnet-artisan | 9.0.1 (older) | ❌ | ✅ (10.0.0-*) | None |
| dotnet-webapi | ❌ | ❌ | ✅ (10.0.*-*) | None |
| dotnet-skills | 10.1.7 | 11.3.1 (deprecated) | ❌ | FluentValidation.AspNetCore (deprecated) |
| managedcode | ❌ | ❌ | ❌ | EF Core Tools (PrivateAssets) |
| no-skills | 10.1.7 | ❌ | ❌ | None |

Key concerns:
- **dotnet-artisan** uses Swashbuckle 9.0.1 — an older major version that may have known issues
- **dotnet-webapi** uses wildcard versions (`10.0.*-*`) on all packages, which can pull in pre-release builds
- **dotnet-skills** includes `FluentValidation.AspNetCore` which is [deprecated](https://github.com/FluentValidation/FluentValidation/issues/1965) — the maintainer recommends manual integration
- **managedcode** has the cleanest package set with all exact versions and no unnecessary dependencies

```xml
<!-- managedcode: Clean exact versions -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />

<!-- dotnet-artisan: Wildcards + older Swashbuckle -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0-*" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.1" />
```

**Scores:**
- dotnet-artisan: **3** — Older Swashbuckle + wildcard versions
- dotnet-webapi: **4** — No unnecessary packages, but wildcards risk pre-release pulls
- dotnet-skills: **3** — Deprecated FluentValidation.AspNetCore package
- managedcode: **5** — All exact versions, no unnecessary packages
- no-skills: **4** — Exact versions, Swashbuckle adds surface area but is pinned

**Verdict:** **managedcode** is best — exact versions with no unnecessary dependencies. Wildcards in dotnet-artisan and dotnet-webapi create reproducibility and security risks.

---

## 3. Minimal API Architecture [CRITICAL]

The modern .NET standard for new Web APIs is Minimal APIs with route groups, TypedResults, and endpoint extension methods.

**dotnet-artisan and dotnet-webapi** both use Minimal APIs with `MapGroup()`:

```csharp
// dotnet-artisan: Endpoints/LoanEndpoints.cs
public static class LoanEndpoints
{
    public static RouteGroupBuilder MapLoanEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/loans").WithTags("Loans");

        group.MapGet("/{id:int}", async Task<Results<Ok<LoanResponse>, NotFound>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var loan = await service.GetByIdAsync(id, ct);
            return loan is not null ? TypedResults.Ok(loan) : TypedResults.NotFound();
        })
        .WithName("GetLoanById")
        .WithSummary("Get loan details")
        .Produces<LoanResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);
```

**dotnet-skills, managedcode, and no-skills** all use controllers:

```csharp
// no-skills: Controllers/LoansController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _service;
    public LoansController(ILoanService service) => _service = service;

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LoanDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLoan(int id)
    {
        var loan = await _service.GetLoanByIdAsync(id);
        return loan == null ? NotFound() : Ok(loan);
    }
```

Key differences:
- **TypedResults** with `Results<T1, T2>` union return types provide compile-time type safety and automatic OpenAPI schema generation — only dotnet-artisan and dotnet-webapi use these
- **Route groups** with `.WithTags()` keep endpoint definitions clean and allow shared configuration
- Controllers return `IActionResult` which is untyped and requires manual `[ProducesResponseType]` annotations
- Endpoint extension methods (`MapLoanEndpoints(this IEndpointRouteBuilder)`) keep Program.cs concise

**Scores:**
- dotnet-artisan: **5** — Full Minimal APIs with MapGroup, TypedResults, Results<T1,T2>, endpoint extensions
- dotnet-webapi: **5** — Full Minimal APIs with MapGroup, TypedResults, Results<T1,T2>, endpoint extensions
- dotnet-skills: **1** — Controllers with [ApiController], no Minimal APIs
- managedcode: **1** — Controllers with [ApiController], no Minimal APIs
- no-skills: **1** — Controllers with [ApiController], no Minimal APIs

**Verdict:** **dotnet-artisan** and **dotnet-webapi** tie for best — the only two configurations that generate modern Minimal APIs. Controller-based approaches add unnecessary boilerplate and lose compile-time type safety.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

All configurations use Data Annotations on DTOs. The key differentiator is **dotnet-skills** which adds FluentValidation.

```csharp
// dotnet-artisan: DTOs/BookDtos.cs — Data Annotations
public sealed record CreateBookRequest
{
    [Required, StringLength(300)]
    public required string Title { get; init; }

    [Required]
    public required string ISBN { get; init; }

    [Range(1, int.MaxValue)]
    public required int TotalCopies { get; init; }
}
```

```csharp
// dotnet-skills: Validators/Validators.cs — FluentValidation
public class CreateAuthorDtoValidator : AbstractValidator<CreateAuthorDto>
{
    public CreateAuthorDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Biography).MaximumLength(2000);
    }
}
```

No configuration uses explicit guard clauses (`ArgumentNullException.ThrowIfNull()`, `ArgumentException.ThrowIfNullOrEmpty()`) on constructor parameters or service method entries. All rely on DI framework validation and service-level business rule checks.

**Scores:**
- dotnet-artisan: **3** — Data Annotations on DTOs, service-level validation, no guard clauses
- dotnet-webapi: **3** — Data Annotations on DTOs, service-level validation, no guard clauses
- dotnet-skills: **4** — FluentValidation (comprehensive rules) + Data Annotations, auto-validation pipeline
- managedcode: **3** — Data Annotations on DTOs, service-level validation, no guard clauses
- no-skills: **3** — Data Annotations on DTOs, service-level validation, no guard clauses

**Verdict:** **dotnet-skills** is best due to FluentValidation providing richer validation rules (though the deprecated `FluentValidation.AspNetCore` package is a concern). All configurations miss guard clauses on constructors and public methods.

---

## 5. NuGet & Package Discipline [CRITICAL]

This dimension evaluates version pinning and minimal package selection.

| Configuration | Packages | Wildcards | Unnecessary |
|---|---|---|---|
| dotnet-artisan | 3 | 2 (EF `10.0.0-*`) | Swashbuckle |
| dotnet-webapi | 3 | 3 (all `10.0.*-*`) | None |
| dotnet-skills | 4 | 0 | Swashbuckle, FluentValidation |
| managedcode | 4 | 0 | None (EF Tools is PrivateAssets) |
| no-skills | 4 | 0 | Swashbuckle |

```xml
<!-- dotnet-webapi: ALL packages use wildcards — worst for reproducibility -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.*-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.*-*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.*-*" />

<!-- managedcode: All exact, all purposeful -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.5">
```

**Scores:**
- dotnet-artisan: **2** — Wildcard versions on EF packages, Swashbuckle unnecessary with built-in OpenAPI
- dotnet-webapi: **2** — All 3 packages use `10.0.*-*` wildcards, risking non-reproducible builds
- dotnet-skills: **3** — Exact versions, but includes deprecated FluentValidation.AspNetCore and Swashbuckle
- managedcode: **5** — All exact versions, every package is purposeful, no unnecessary dependencies
- no-skills: **3** — Exact versions, but includes Swashbuckle alongside built-in AddOpenApi

**Verdict:** **managedcode** is the clear winner — pinned versions with no unnecessary packages. The wildcard versions in dotnet-artisan and dotnet-webapi are the most concerning issue.

---

## 6. EF Migration Usage [CRITICAL]

Only **dotnet-webapi** uses proper EF Core migrations. All others use the `EnsureCreated()` anti-pattern.

```csharp
// dotnet-webapi: Program.cs — Correct: uses Migrate()
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    dbContext.Database.Migrate();
}
```

```csharp
// dotnet-artisan: Program.cs — Anti-pattern: EnsureCreated
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(db);
}
```

dotnet-webapi also has a real `Migrations/` directory with generated migration files and uses `HasData()` in `OnModelCreating` to integrate seed data with migrations — the production-safe approach.

**Scores:**
- dotnet-artisan: **1** — Uses `EnsureCreatedAsync()`, no migrations
- dotnet-webapi: **5** — Uses `Migrate()` with actual migration files and `HasData()` seeding
- dotnet-skills: **1** — Uses `EnsureCreated()`, no migrations
- managedcode: **1** — Uses `EnsureCreatedAsync()`, no migrations
- no-skills: **1** — Uses `EnsureCreated()`, no migrations

**Verdict:** **dotnet-webapi** is the only configuration that gets this right. `EnsureCreated()` bypasses migrations entirely, making schema evolution impossible and risking data loss. This is a critical production readiness gap in all other configurations.

---

## 7. Business Logic Correctness [HIGH]

The specification requires ~37 endpoints across 7 resource areas with complex business rules (borrowing limits, fine thresholds, reservation queues, renewal constraints). All configurations implement the full endpoint set.

Key business rules verified across all:
- Borrowing limits by membership type (Standard: 5/14d, Premium: 10/21d, Student: 3/7d)
- Fine threshold blocking ($10.00 unpaid fines blocks checkout/renewal)
- Overdue fine calculation ($0.25/day)
- Reservation queue promotion on return
- Renewal limits (max 2, blocked if overdue or pending reservations)

```csharp
// dotnet-artisan: Services/LoanService.cs — Checkout validation
var unpaidFines = await db.Fines
    .Where(f => f.PatronId == request.PatronId && f.Status == FineStatus.Unpaid)
    .SumAsync(f => f.Amount, ct);

if (unpaidFines >= MaxUnpaidFinesThreshold)
    throw new InvalidOperationException(
        $"Patron has ${unpaidFines:F2} in unpaid fines (threshold: ${MaxUnpaidFinesThreshold:F2}).");
```

dotnet-artisan and dotnet-webapi provide slightly more comprehensive endpoint implementations with richer error messages and edge case handling.

**Scores:**
- dotnet-artisan: **5** — All endpoints + all business rules with detailed error messages
- dotnet-webapi: **5** — All endpoints + all business rules with detailed error messages
- dotnet-skills: **4** — All endpoints + business rules, some error messages less descriptive
- managedcode: **4** — All endpoints + business rules implemented
- no-skills: **4** — All endpoints + business rules implemented

**Verdict:** **dotnet-artisan** and **dotnet-webapi** tie — most complete business rule implementations with descriptive error responses.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

This dimension checks whether configurations use built-in .NET capabilities instead of third-party libraries.

| Configuration | OpenAPI | Validation | JSON | Logging |
|---|---|---|---|---|
| dotnet-artisan | ❌ Swashbuckle | Built-in | System.Text.Json | ILogger<T> |
| dotnet-webapi | ✅ AddOpenApi/MapOpenApi | Built-in | System.Text.Json | ILogger<T> |
| dotnet-skills | ❌ Swashbuckle + AddOpenApi | ❌ FluentValidation | System.Text.Json | ILogger<T> |
| managedcode | ✅ AddOpenApi/MapOpenApi | Built-in | System.Text.Json | ILogger<T> |
| no-skills | ❌ Swashbuckle + AddOpenApi | Built-in | System.Text.Json | ILogger<T> |

```csharp
// dotnet-webapi: Program.cs — Built-in OpenAPI only
builder.Services.AddOpenApi();
// ...
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

```csharp
// dotnet-artisan: Program.cs — Swashbuckle instead of built-in
builder.Services.AddSwaggerGen(options => { /* ... */ });
app.UseSwagger();
app.UseSwaggerUI(options => { /* ... */ });
```

```csharp
// dotnet-skills: Program.cs — BOTH Swashbuckle AND built-in (redundant)
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options => { /* ... */ });
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "..."));
```

**Scores:**
- dotnet-artisan: **2** — Uses Swashbuckle instead of built-in AddOpenApi
- dotnet-webapi: **5** — Exclusively uses built-in OpenAPI, no third-party
- dotnet-skills: **1** — Uses both Swashbuckle AND FluentValidation (two unnecessary 3rd party libs)
- managedcode: **5** — Exclusively uses built-in OpenAPI, no third-party
- no-skills: **2** — Uses Swashbuckle alongside built-in AddOpenApi (redundant)

**Verdict:** **dotnet-webapi** and **managedcode** tie — both exclusively use built-in .NET capabilities. dotnet-skills is worst with two unnecessary third-party dependencies.

---

## 9. Modern C# Adoption [HIGH]

Modern C# features reduce boilerplate and signal current language proficiency.

| Feature | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| Primary constructors | ✅ | ✅ | ❌ | ✅ | ❌ |
| Sealed records for DTOs | ✅ | ✅ | ❌ | ✅ | ❌ |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ | ✅ |
| ImplicitUsings | ✅ | ✅ | ✅ | ✅ | ✅ |
| Target-typed new | ✅ | ✅ | ✅ | ✅ | ✅ |

```csharp
// dotnet-artisan: Primary constructor on service
public sealed class LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    : ILoanService
{
    // No private field declarations or constructor body needed
    public async Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct) =>
        // Direct access to 'db' and 'logger' parameters
```

```csharp
// no-skills: Traditional constructor boilerplate
public class LoanService : ILoanService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<LoanService> _logger;

    public LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    {
        _db = db;
        _logger = logger;
    }
```

```csharp
// dotnet-skills: Traditional constructor (even though services are sealed)
public sealed class LoanService : ILoanService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<LoanService> _logger;
    // ... traditional field assignments
```

**Scores:**
- dotnet-artisan: **5** — Primary constructors, sealed records, all modern patterns
- dotnet-webapi: **5** — Primary constructors, sealed records, all modern patterns
- dotnet-skills: **3** — File-scoped namespaces and ImplicitUsings, but traditional constructors and no sealed DTOs
- managedcode: **5** — Primary constructors, sealed records, all modern patterns
- no-skills: **2** — File-scoped namespaces only; traditional constructors, mutable DTOs, no sealed types

**Verdict:** **dotnet-artisan**, **dotnet-webapi**, and **managedcode** tie — all use primary constructors and modern patterns. no-skills generates the most dated C# style.

---

## 10. Error Handling & Middleware [HIGH]

The modern .NET 8+ approach uses `IExceptionHandler` for global error handling with DI support and composability.

```csharp
// dotnet-artisan/dotnet-webapi: IExceptionHandler with pattern matching
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
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
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
```

**dotnet-skills** uses a more sophisticated approach with custom exception types and two composable handlers:

```csharp
// dotnet-skills: Custom exceptions + composable handlers
public sealed class BusinessRuleException : Exception { /* with StatusCode property */ }
public sealed class NotFoundException : Exception { /* entity-specific message */ }

// BusinessExceptionHandler handles domain exceptions, falls through otherwise
// GlobalExceptionHandler catches everything else
builder.Services.AddExceptionHandler<BusinessExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

**no-skills** uses the older convention-based middleware approach:

```csharp
// no-skills: Convention middleware — NOT IExceptionHandler
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

**Scores:**
- dotnet-artisan: **5** — IExceptionHandler, ProblemDetails, pattern matching, structured logging
- dotnet-webapi: **5** — IExceptionHandler, ProblemDetails, falls through for unhandled (proper pattern)
- dotnet-skills: **5** — IExceptionHandler with custom exceptions and composable handler chain
- managedcode: **5** — IExceptionHandler, ProblemDetails, pattern matching
- no-skills: **2** — Convention middleware (RequestDelegate), not DI-aware, not composable

**Verdict:** Four-way tie among skill-assisted configurations. **no-skills** is notably worse — the convention-based middleware pattern is pre-.NET 8 and lacks DI integration and composability.

---

## 11. Async Patterns & Cancellation [HIGH]

CancellationToken propagation prevents wasted server resources on cancelled requests.

| Configuration | CancellationToken in Endpoints | CancellationToken in Services | Total CT References |
|---|---|---|---|
| dotnet-artisan | ✅ Every endpoint | ✅ Every method | ~60 |
| dotnet-webapi | ✅ Every endpoint | ✅ Every method | ~70 |
| dotnet-skills | ❌ None | ❌ None | 2 (IExceptionHandler only) |
| managedcode | ✅ Every endpoint | ✅ Every method | ~65 |
| no-skills | ❌ None | ❌ None | 0 |

```csharp
// dotnet-artisan: CancellationToken flows from endpoint through service to EF Core
group.MapGet("/{id:int}", async Task<Results<Ok<LoanResponse>, NotFound>> (
    int id, ILoanService service, CancellationToken ct) =>
{
    var loan = await service.GetByIdAsync(id, ct);
    // ...
});

// Service method forwards the token to EF Core
public async Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct) =>
    await db.Loans.AsNoTracking()
        .Where(l => l.Id == id)
        .Select(l => new LoanResponse(...))
        .FirstOrDefaultAsync(ct);
```

```csharp
// no-skills: No CancellationToken at all
public async Task<IActionResult> GetLoan(int id)
{
    var loan = await _service.GetLoanByIdAsync(id);  // No ct parameter
    return loan == null ? NotFound() : Ok(loan);
}
```

**Scores:**
- dotnet-artisan: **5** — CancellationToken on every async endpoint and service method, forwarded to EF Core
- dotnet-webapi: **5** — CancellationToken on every async endpoint and service method
- dotnet-skills: **2** — No CancellationToken in controllers or services (only in IExceptionHandler interface)
- managedcode: **5** — CancellationToken on every endpoint and service method
- no-skills: **1** — Zero CancellationToken usage anywhere

**Verdict:** **dotnet-artisan**, **dotnet-webapi**, and **managedcode** tie — full CancellationToken propagation. no-skills completely ignores cancellation, which wastes server resources on abandoned requests.

---

## 12. EF Core Best Practices [HIGH]

This covers Fluent API configuration, AsNoTracking for reads, and proper relationship setup.

| Configuration | AsNoTracking Count | Fluent API | Enum→String | Cascade Config | AsSplitQuery |
|---|---|---|---|---|---|
| dotnet-artisan | ~20 | ✅ Comprehensive | ✅ | ✅ Restrict on Loan/Reservation/Fine | ❌ |
| dotnet-webapi | ~30 | ✅ Comprehensive + HasMaxLength | ✅ | ✅ Restrict | ❌ |
| dotnet-skills | ~20 | ✅ Comprehensive | ✅ | ✅ Restrict | ✅ |
| managedcode | ~20 | ✅ Comprehensive | ✅ | ✅ Restrict | ❌ |
| no-skills | 0 | ✅ Basic | ✅ | ❌ Convention only | ❌ |

```csharp
// dotnet-webapi: Fluent API with property constraints in OnModelCreating
modelBuilder.Entity<Book>(e =>
{
    e.Property(b => b.Title).HasMaxLength(300).IsRequired();
    e.Property(b => b.ISBN).HasMaxLength(20).IsRequired();
    e.HasIndex(b => b.ISBN).IsUnique();
    e.Property(b => b.Language).HasMaxLength(50).HasDefaultValue("English");
});
```

```csharp
// dotnet-skills: AsSplitQuery for multi-Include queries (unique advantage)
var books = await _db.Books
    .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
    .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
    .AsSplitQuery()
    .AsNoTracking()
    .ToListAsync();
```

```csharp
// no-skills: No AsNoTracking on any query — all reads tracked unnecessarily
var loan = await _db.Loans
    .Include(l => l.Book)
    .Include(l => l.Patron)
    .FirstOrDefaultAsync(l => l.Id == id);
```

**Scores:**
- dotnet-artisan: **5** — AsNoTracking everywhere, comprehensive Fluent API, explicit cascade behaviors
- dotnet-webapi: **5** — AsNoTracking everywhere, most comprehensive Fluent API (HasMaxLength in config), HasData seeding
- dotnet-skills: **5** — AsNoTracking everywhere, AsSplitQuery on multi-Include queries (unique optimization)
- managedcode: **4** — AsNoTracking everywhere, good Fluent API, but less comprehensive property config
- no-skills: **2** — Zero AsNoTracking usage (all reads tracked), basic Fluent API, no explicit cascade config

**Verdict:** Three-way tie among **dotnet-artisan**, **dotnet-webapi**, and **dotnet-skills**. The complete absence of AsNoTracking in no-skills doubles the memory overhead of every read query.

---

## 13. Service Abstraction & DI [HIGH]

All configurations follow the interface + implementation pattern with proper DI registration.

```csharp
// Common pattern across all configurations
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();
// ... 7 service registrations total
```

The key differences are in interface organization:
- **dotnet-artisan**, **dotnet-webapi**, **managedcode**: Separate interface files per service (e.g., `IAuthorService.cs`, `IBookService.cs`)
- **dotnet-skills**: Separate interface files per service
- **no-skills**: All interfaces in a single `Interfaces.cs` file

**Scores:**
- dotnet-artisan: **5** — Interface per file, proper scoped lifetime, single responsibility
- dotnet-webapi: **5** — Interface per file, proper scoped lifetime, single responsibility
- dotnet-skills: **5** — Interface per file, proper scoped lifetime, single responsibility
- managedcode: **5** — Interface per file, proper scoped lifetime, single responsibility
- no-skills: **4** — All interfaces in one file (less discoverable), but pattern is correct

**Verdict:** Effective tie — all follow the pattern correctly. no-skills slightly less organized with interfaces in a single file.

---

## 14. Security Configuration [HIGH]

Only **dotnet-webapi** includes any HTTPS enforcement:

```csharp
// dotnet-webapi: Program.cs — Only config with HTTPS redirection
app.UseHttpsRedirection();
```

No configuration includes `UseHsts()` or CORS configuration. This is a gap across all implementations, though dotnet-webapi at least addresses HTTPS redirection.

**Scores:**
- dotnet-artisan: **1** — No HTTPS redirection, no HSTS
- dotnet-webapi: **3** — UseHttpsRedirection present, but no HSTS
- dotnet-skills: **1** — No HTTPS redirection, no HSTS
- managedcode: **1** — No HTTPS redirection, no HSTS
- no-skills: **1** — No HTTPS redirection, no HSTS

**Verdict:** **dotnet-webapi** is the only configuration addressing security headers. All configurations miss HSTS — a universal gap.

---

## 15. DTO Design [MEDIUM]

DTOs should be immutable records, sealed, with clear naming conventions and separation from entities.

```csharp
// dotnet-artisan/dotnet-webapi: Sealed positional records (immutable)
public sealed record LoanResponse(
    int Id, int BookId, string BookTitle, int PatronId, string PatronName,
    DateTime LoanDate, DateTime DueDate, DateTime? ReturnDate,
    LoanStatus Status, int RenewalCount, DateTime CreatedAt);

public sealed record CreateLoanRequest
{
    [Required]
    public required int BookId { get; init; }
    [Required]
    public required int PatronId { get; init; }
}
```

```csharp
// no-skills: Mutable classes (not records, not sealed)
public class AuthorCreateDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}
```

```csharp
// dotnet-skills: Mutable classes, not sealed, in one file
public class AuthorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    // ...
}
public class AuthorDetailDto : AuthorDto  // Inheritance-based
{
    public List<BookSummaryDto> Books { get; set; } = [];
}
```

**Scores:**
- dotnet-artisan: **5** — Sealed records, positional responses, init requests, separate files per entity
- dotnet-webapi: **5** — Sealed records, positional responses, init requests, separate files
- dotnet-skills: **2** — Mutable classes with `set`, inheritance between DTOs, not sealed, all in one file
- managedcode: **5** — Sealed records, separate files, immutable
- no-skills: **2** — Mutable classes with `set`, not sealed, all in one file, *Dto suffix

**Verdict:** **dotnet-artisan**, **dotnet-webapi**, and **managedcode** tie — all use the modern sealed record pattern. dotnet-skills and no-skills use mutable classes which allow unintended mutation.

---

## 16. Sealed Types [MEDIUM]

Sealed types enable JIT devirtualization and express design intent.

| Configuration | Sealed Types | Total Public Types | Seal Rate |
|---|---|---|---|
| dotnet-artisan | ~43 | ~43 | ~100% |
| dotnet-webapi | ~42 | ~42 | ~100% |
| dotnet-skills | ~11 | ~48 | ~23% |
| managedcode | ~40 | ~40 | ~100% |
| no-skills | 0 | ~45 | 0% |

```csharp
// dotnet-artisan: Everything sealed
public sealed class Book { /* ... */ }
public sealed class AuthorService(/*...*/) : IAuthorService { /* ... */ }
public sealed record BookResponse(/*...*/);
internal sealed class GlobalExceptionHandler : IExceptionHandler { /* ... */ }

// no-skills: Nothing sealed
public class Book { /* ... */ }
public class AuthorService : IAuthorService { /* ... */ }
public class AuthorDto { /* ... */ }
public class GlobalExceptionHandlerMiddleware { /* ... */ }
```

**Scores:**
- dotnet-artisan: **5** — 100% seal rate across all type categories
- dotnet-webapi: **5** — 100% seal rate across all type categories
- dotnet-skills: **2** — Only services and middleware sealed; models, DTOs, controllers unsealed
- managedcode: **5** — ~100% seal rate across all categories
- no-skills: **1** — Zero sealed types anywhere

**Verdict:** **dotnet-artisan**, **dotnet-webapi**, and **managedcode** tie at 100% seal rate. no-skills has zero sealed types — missing all JIT optimization opportunities.

---

## 17. Data Seeder Design [MEDIUM]

All configurations include seed data with realistic variety. The key differentiator is the seeding mechanism.

```csharp
// dotnet-webapi: HasData() in OnModelCreating — integrated with migrations
modelBuilder.Entity<Author>().HasData(
    new Author { Id = 1, FirstName = "Harper", LastName = "Lee", /* ... */ },
    new Author { Id = 2, FirstName = "George", LastName = "Orwell", /* ... */ }
);
```

```csharp
// dotnet-artisan: Static seeder with idempotency check
public static class DataSeeder
{
    public static async Task SeedAsync(LibraryDbContext db)
    {
        if (await db.Authors.AnyAsync()) return; // Idempotency guard
        // ... seed all entities
    }
}
```

**Scores:**
- dotnet-artisan: **4** — Runtime seeder with idempotency, comprehensive data
- dotnet-webapi: **5** — HasData() in OnModelCreating, migration-integrated, reproducible
- dotnet-skills: **4** — Runtime seeder with idempotency check
- managedcode: **4** — Runtime seeder with idempotency check
- no-skills: **4** — Runtime seeder with idempotency check

**Verdict:** **dotnet-webapi** is best — `HasData()` integrates seed data with migrations for fully reproducible database state.

---

## 18. Structured Logging [MEDIUM]

All configurations inject `ILogger<T>` and log business operations. None use `[LoggerMessage]` source generators.

```csharp
// dotnet-artisan: Structured templates with named placeholders
logger.LogInformation("Book checked out. LoanId: {LoanId}, BookId: {BookId}, PatronId: {PatronId}",
    loan.Id, request.BookId, request.PatronId);

logger.LogWarning(exception, "Handled API exception: {Title}", title);
```

```csharp
// no-skills: String interpolation in some places, less structured
_logger.LogWarning("Business rule violation: {Message}", exception.Message);
```

**Scores:**
- dotnet-artisan: **4** — ILogger<T>, structured templates, appropriate log levels
- dotnet-webapi: **4** — ILogger<T>, structured templates, appropriate log levels
- dotnet-skills: **4** — ILogger<T>, structured templates
- managedcode: **4** — ILogger<T>, structured templates
- no-skills: **3** — ILogger<T> present, but less consistent structured logging

**Verdict:** Effective tie among skill-assisted configs. No configuration uses high-performance `[LoggerMessage]` source generators.

---

## 19. Nullable Reference Types [MEDIUM]

All configurations enable NRT in their `.csproj`:

```xml
<Nullable>enable</Nullable>
```

All properly annotate optional properties with `?` (e.g., `DateTime? ReturnDate`, `string? Biography`). The main difference is in how thoroughly navigation properties and API responses handle nullability.

**Scores:** All configurations: **4** — NRT enabled, proper annotations on optional properties

**Verdict:** Tie across all configurations.

---

## 20. API Documentation [MEDIUM]

Minimal API configurations have richer inline documentation via fluent methods.

```csharp
// dotnet-artisan: Rich inline metadata
group.MapPost("/", async (CreateLoanRequest request, ILoanService service, CancellationToken ct) =>
{
    var loan = await service.CheckoutAsync(request, ct);
    return TypedResults.Created($"/api/loans/{loan.Id}", loan);
})
.WithName("CheckoutBook")
.WithSummary("Check out a book — creates a loan enforcing all checkout rules")
.WithDescription("Enforces: available copies, unpaid fines threshold ($10), ...")
.Produces<LoanResponse>(StatusCodes.Status201Created)
.ProducesValidationProblem()
.ProducesProblem(StatusCodes.Status409Conflict);
```

```csharp
// no-skills: Basic XML comments and attributes
/// <summary>Check out a book — create a loan enforcing all checkout rules</summary>
[HttpPost]
[ProducesResponseType(typeof(LoanDto), 201)]
[ProducesResponseType(400)]
public async Task<IActionResult> CheckoutBook([FromBody] LoanCreateDto dto)
```

**Scores:**
- dotnet-artisan: **5** — WithName, WithSummary, WithDescription, Produces<T>, ProducesValidationProblem
- dotnet-webapi: **5** — WithName, WithSummary, WithDescription, Produces<T>
- dotnet-skills: **4** — XML comments, ProducesResponseType, XML doc generation enabled
- managedcode: **4** — XML comments, ProducesResponseType
- no-skills: **3** — XML comments present but minimal, ProducesResponseType with magic numbers

**Verdict:** **dotnet-artisan** and **dotnet-webapi** tie — fluent endpoint metadata produces richer OpenAPI documentation than controller attributes.

---

## 21. File Organization [MEDIUM]

| Configuration | Structure | Endpoints Style | DTO Files | Interface Files |
|---|---|---|---|---|
| dotnet-artisan | Endpoints/ Services/ Models/ DTOs/ Data/ Middleware/ | 7 endpoint files | 8 separate files | 7 separate files |
| dotnet-webapi | Endpoints/ Services/ Models/ DTOs/ Data/ Middleware/ Migrations/ | 7 endpoint files | 8 separate files | 7 separate files |
| dotnet-skills | Controllers/ Services/ Models/ DTOs/ Data/ Middleware/ Validators/ | 7 controllers | 1 monolithic Dtos.cs | 7 separate files |
| managedcode | Controllers/ Services/ Models/ DTOs/ Data/ Middleware/ | 7 controllers | 8 separate files | 7 separate files |
| no-skills | Controllers/ Services/ Models/ DTOs/ Data/ Middleware/ | 7 controllers | 1 Dtos.cs + 1 PagedResult.cs | 1 Interfaces.cs |

**Scores:**
- dotnet-artisan: **5** — Clean separation with dedicated Endpoints/ directory and per-entity DTO files
- dotnet-webapi: **5** — Clean separation with Endpoints/ and Migrations/ directories
- dotnet-skills: **3** — Monolithic Dtos.cs (22 types in one file), but has dedicated Validators/
- managedcode: **4** — Clean per-entity DTO files, but controllers instead of endpoints
- no-skills: **3** — Monolithic Dtos.cs, single Interfaces.cs, less discoverable

**Verdict:** **dotnet-artisan** and **dotnet-webapi** tie — best file organization with per-entity grouping.

---

## 22. HTTP Test File Quality [MEDIUM]

All configurations include comprehensive `.http` files. dotnet-artisan has the most extensive at 346 lines with 50+ requests.

All .http files include:
- `@baseUrl` variable
- Requests grouped by resource with comment headers
- POST/PUT requests with realistic JSON bodies
- Business rule test cases (inactive patron checkout, fine threshold, etc.)

**Scores:**
- dotnet-artisan: **5** — 346 lines, 50+ requests, comprehensive business rule edge cases
- dotnet-webapi: **5** — Comprehensive coverage with seed data IDs
- dotnet-skills: **4** — Good coverage, less edge case testing
- managedcode: **4** — Good coverage
- no-skills: **4** — 306 lines, good coverage

**Verdict:** **dotnet-artisan** and **dotnet-webapi** tie with the most thorough .http test files.

---

## 23. Type Design & Resource Management [MEDIUM]

All configurations use enums for status fields with `HasConversion<string>()` in EF Core.

```csharp
// Common across all: Proper enum definitions
public enum LoanStatus { Active, Returned, Overdue }
public enum ReservationStatus { Pending, Ready, Fulfilled, Cancelled, Expired }
public enum FineStatus { Unpaid, Paid, Waived }
public enum MembershipType { Standard, Premium, Student }
```

Key differences:
- **dotnet-artisan/dotnet-webapi**: Use `IReadOnlyList<T>` in pagination responses
- **dotnet-skills/no-skills**: Use `List<T>` or `IEnumerable<T>` in responses (allows mutation)

**Scores:**
- dotnet-artisan: **5** — Proper enums, IReadOnlyList<T>, separate enum files
- dotnet-webapi: **5** — Proper enums, IReadOnlyList<T>, separate enum files
- dotnet-skills: **4** — Proper enums, but List<T> in responses
- managedcode: **4** — Proper enums, IReadOnlyList<T> in PaginatedResponse
- no-skills: **3** — Proper enums, but List<T> in responses, all enums in one file

**Verdict:** **dotnet-artisan** and **dotnet-webapi** tie — most precise type design with immutable collection types.

---

## 24. Code Standards Compliance [LOW]

| Standard | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| PascalCase public members | ✅ | ✅ | ✅ | ✅ | ✅ |
| Async suffix | ✅ | ✅ | ✅ | ✅ | ✅ |
| I prefix on interfaces | ✅ | ✅ | ✅ | ✅ | ✅ |
| Explicit access modifiers | ✅ | ✅ | ✅ | ✅ | ✅ |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ | ✅ |
| `internal` on non-public types | ✅ | ✅ | ❌ | ✅ | ❌ |

All configurations include `Directory.Build.props` with .NET analyzers enabled:

```xml
<PropertyGroup>
  <AnalysisLevel>latest</AnalysisLevel>
  <AnalysisMode>Recommended</AnalysisMode>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Meziantou.Analyzer" Version="*" />
</ItemGroup>
```

**Scores:**
- dotnet-artisan: **5** — Full compliance with `internal sealed` on non-public types
- dotnet-webapi: **5** — Full compliance with `internal sealed` on non-public types
- dotnet-skills: **4** — Good compliance but exception handler is `public sealed` instead of `internal`
- managedcode: **4** — Good compliance, `internal` on handler
- no-skills: **3** — Missing `sealed`, missing `internal` on non-public types

**Verdict:** **dotnet-artisan** and **dotnet-webapi** tie with the most precise access modifier usage.

---

## Weighted Summary

Weights: Critical ×3, High ×2, Medium ×1, Low ×0.5

| Dimension [Tier] | Weight | dotnet-artisan | dotnet-webapi | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|---|
| Build & Run [CRITICAL] | ×3 | 15 | 15 | 15 | 15 | 15 |
| Security Scan [CRITICAL] | ×3 | 9 | 12 | 9 | 15 | 12 |
| Minimal API [CRITICAL] | ×3 | 15 | 15 | 3 | 3 | 3 |
| Input Validation [CRITICAL] | ×3 | 9 | 9 | 12 | 9 | 9 |
| NuGet Discipline [CRITICAL] | ×3 | 6 | 6 | 9 | 15 | 9 |
| EF Migration [CRITICAL] | ×3 | 3 | 15 | 3 | 3 | 3 |
| Business Logic [HIGH] | ×2 | 10 | 10 | 8 | 8 | 8 |
| Built-in vs 3rd Party [HIGH] | ×2 | 4 | 10 | 2 | 10 | 4 |
| Modern C# [HIGH] | ×2 | 10 | 10 | 6 | 10 | 4 |
| Error Handling [HIGH] | ×2 | 10 | 10 | 10 | 10 | 4 |
| Async & Cancellation [HIGH] | ×2 | 10 | 10 | 4 | 10 | 2 |
| EF Core Best Practices [HIGH] | ×2 | 10 | 10 | 10 | 8 | 4 |
| Service Abstraction [HIGH] | ×2 | 10 | 10 | 10 | 10 | 8 |
| Security Config [HIGH] | ×2 | 2 | 6 | 2 | 2 | 2 |
| DTO Design [MEDIUM] | ×1 | 5 | 5 | 2 | 5 | 2 |
| Sealed Types [MEDIUM] | ×1 | 5 | 5 | 2 | 5 | 1 |
| Data Seeder [MEDIUM] | ×1 | 4 | 5 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | ×1 | 4 | 4 | 4 | 4 | 3 |
| Nullable Reference Types [MEDIUM] | ×1 | 4 | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | ×1 | 5 | 5 | 4 | 4 | 3 |
| File Organization [MEDIUM] | ×1 | 5 | 5 | 3 | 4 | 3 |
| HTTP Test File [MEDIUM] | ×1 | 5 | 5 | 4 | 4 | 4 |
| Type Design [MEDIUM] | ×1 | 5 | 5 | 4 | 4 | 3 |
| Code Standards [LOW] | ×0.5 | 2.5 | 2.5 | 2 | 2 | 1.5 |
| **TOTAL** | | **167.5** | **191.5** | **134** | **168** | **114.5** |

### Final Rankings

| Rank | Configuration | Weighted Score | Key Strengths |
|---|---|---|---|
| 🥇 1st | **dotnet-webapi** | **191.5** | Only config with migrations, built-in OpenAPI, HTTPS, Minimal APIs |
| 🥈 2nd | **managedcode** | **168.0** | Best package discipline, modern C#, CancellationToken, but uses controllers |
| 🥉 3rd | **dotnet-artisan** | **167.5** | Excellent Minimal APIs & TypedResults, but Swashbuckle & wildcards |
| 4th | **dotnet-skills** | **134.0** | Best validation (FluentValidation), AsSplitQuery, but controllers & no CT |
| 5th | **no-skills** | **114.5** | Functional but dated: no sealed, no CT, no AsNoTracking, old middleware |

---

## What All Versions Get Right

- ✅ **Zero build errors and warnings** — all projects compile cleanly on first build
- ✅ **Interface-based service layer** — all use `IService`/`Service` pattern with `AddScoped<>` DI registration
- ✅ **7 resource areas with 37+ endpoints** — complete implementation of the specification
- ✅ **Comprehensive seed data** — 6+ authors, 12+ books, 7+ patrons with varied states
- ✅ **RFC 7807 ProblemDetails** — all return structured error responses
- ✅ **Entity separation from DTOs** — none expose EF Core entities directly in API responses
- ✅ **Enum types for status fields** — all use proper enums with `HasConversion<string>()`
- ✅ **JSON enum serialization** — all configure `JsonStringEnumConverter` for readable API responses
- ✅ **Fluent API relationship configuration** — all define composite keys and foreign key relationships
- ✅ **ILogger<T> injection** — all services and middleware use structured logging
- ✅ **.NET 10 targeting** — all use `net10.0` TFM with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- ✅ **Directory.Build.props** — all include Meziantou.Analyzer and .NET analyzers with recommended analysis mode
- ✅ **File-scoped namespaces** — all use `namespace X;` syntax
- ✅ **Comprehensive .http test files** — all include business rule test cases

---

## Summary: Impact of Skills

### Most Impactful Differences

1. **EF Migrations vs EnsureCreated** (Critical gap, only dotnet-webapi gets it right) — The single biggest production-readiness differentiator. Four out of five configurations use the `EnsureCreated()` anti-pattern, which makes schema evolution impossible.

2. **Minimal APIs vs Controllers** (Critical architectural choice) — dotnet-artisan and dotnet-webapi generate Minimal APIs with TypedResults and route groups; all others fall back to controllers. This is the modern .NET standard and provides compile-time type safety.

3. **CancellationToken propagation** (High-impact performance pattern) — Three configs (dotnet-artisan, dotnet-webapi, managedcode) propagate CancellationToken through every async call chain. dotnet-skills and no-skills completely omit it, wasting server resources on cancelled requests.

4. **AsNoTracking on reads** (High-impact performance pattern) — Four configs use AsNoTracking consistently. no-skills omits it entirely, doubling memory overhead on all read queries.

5. **Built-in vs 3rd Party libraries** (High-impact maintainability) — dotnet-webapi and managedcode exclusively use built-in .NET capabilities. Others add Swashbuckle and/or FluentValidation unnecessarily.

### Configuration Assessment

**dotnet-webapi (191.5 pts — 🥇 Best Overall):** The single dotnet-webapi skill produces the highest-quality output. It is the only configuration that uses EF Core migrations, built-in OpenAPI, HTTPS redirection, and Minimal APIs with TypedResults. Its only weakness is wildcard package versions.

**managedcode (168.0 pts — 🥈 Runner-up):** Excellent package discipline and modern C# adoption. Uses CancellationToken everywhere and sealed types consistently. Falls short on Minimal APIs (uses controllers) and migrations (uses EnsureCreated).

**dotnet-artisan (167.5 pts — 🥉 Close Third):** The 5-skill chain produces excellent Minimal APIs with TypedResults and comprehensive code quality. Narrowly misses second place due to Swashbuckle usage, wildcard versions, and EnsureCreated instead of migrations.

**dotnet-skills (134.0 pts — 4th):** Unique strengths in FluentValidation and AsSplitQuery optimization, but falls behind on Minimal APIs, CancellationToken propagation, sealed types, and DTO design. The multi-plugin approach doesn't translate to consistently modern output.

**no-skills (114.5 pts — 5th Baseline):** Produces functional but dated code. Missing sealed types, CancellationToken, AsNoTracking, modern IExceptionHandler, and Minimal APIs. Clearly demonstrates the value of skills — every skill-assisted configuration significantly outperforms the baseline on modern .NET best practices.

### The Skills Gap

The gap between the best skill-assisted configuration (dotnet-webapi: 191.5) and the baseline (no-skills: 114.5) is **77 points (67% improvement)**. Even the weakest skill configuration (dotnet-skills: 134.0) outperforms the baseline by **19.5 points (17%)**. Skills consistently improve:
- Modern C# patterns (primary constructors, sealed types)
- Performance practices (AsNoTracking, CancellationToken)
- Error handling (IExceptionHandler vs convention middleware)
- Code organization (per-entity files, clean separation)

The data clearly shows that **specialized skills produce measurably better .NET code**, with the focused **dotnet-webapi** skill achieving the best results by combining Minimal API architecture, EF Core migrations, and built-in platform capabilities.
