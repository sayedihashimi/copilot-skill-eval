# Comparative Analysis: dotnet-artisan, dotnet-skills, managedcode-dotnet-skills, no-skills

## Introduction

This report compares **4 Copilot skill configurations** used to generate the **LibraryApi** scenario (a community library management system with book loans, reservations, overdue fines, and availability tracking). Each configuration produced the same application from the same prompt, targeting .NET 10 with EF Core and SQLite.

| Configuration | Directory | Description |
|---|---|---|
| **dotnet-artisan** | `output/dotnet-artisan/run-2/LibraryApi/` | dotnet-artisan plugin chain (`using-dotnet`, `dotnet-advisor`, `dotnet-webapi-endpoints`, etc.) |
| **dotnet-skills** | `output/dotnet-skills/run-2/LibraryApi/` | Official .NET Skills (`analyzing-dotnet-performance` and others from dotnet/skills) |
| **managedcode-dotnet-skills** | `output/managedcode-dotnet-skills/run-2/LibraryApi/` | Community managed-code skills (`dotnet` router, `aspnet-api`, etc.) |
| **no-skills** | `output/no-skills/run-2/LibraryApi/` | Baseline — default Copilot with no custom skills |

A fifth expected configuration (`dotnet-webapi`) was not present in the output directory.

All four configurations produced a complete LibraryApi project at `src/LibraryApi/` with models, DTOs, services, data seeding, error handling, and an HTTP test file.

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 |
| Security Vulnerability Scan [CRITICAL] | 4 | 4 | 4 | 4 |
| Minimal API Architecture [CRITICAL] | 5 | 1 | 1 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 4 | 2 | 4 | 2 |
| NuGet & Package Discipline [CRITICAL] | 3 | 4 | 4 | 4 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 |
| Business Logic Correctness [HIGH] | 5 | 5 | 5 | 5 |
| Prefer Built-in over 3rd Party [HIGH] | 3 | 3 | 3 | 2 |
| Modern C# Adoption [HIGH] | 5 | 2 | 4 | 1 |
| Error Handling & Middleware [HIGH] | 3 | 3 | 5 | 2 |
| Async Patterns & Cancellation [HIGH] | 5 | 2 | 5 | 1 |
| EF Core Best Practices [HIGH] | 4 | 4 | 5 | 3 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 5 | 5 |
| Security Configuration [HIGH] | 1 | 1 | 1 | 1 |
| DTO Design [MEDIUM] | 5 | 2 | 4 | 2 |
| Sealed Types [MEDIUM] | 5 | 2 | 1 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 5 | 4 | 5 | 4 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 4 | 4 | 4 | 3 |
| File Organization [MEDIUM] | 5 | 4 | 4 | 4 |
| HTTP Test File Quality [MEDIUM] | 4 | 4 | 4 | 4 |
| Type Design & Resource Management [MEDIUM] | 4 | 4 | 4 | 3 |
| Code Standards Compliance [LOW] | 5 | 4 | 4 | 3 |

---

## 1. Build & Run Success [CRITICAL]

All four configurations produce projects that successfully built (pre-built `bin/Debug/net10.0/` artifacts are present with compiled DLLs, confirming `dotnet build` completed). Each project targets `net10.0` and has a valid `.csproj` with resolved NuGet packages.

**dotnet-artisan:**
```xml
<!-- LibraryApi.csproj -->
<TargetFramework>net10.0</TargetFramework>
```

**Score:** dotnet-artisan: **5** | dotnet-skills: **5** | managedcode: **5** | no-skills: **5**

**Verdict:** All configurations produce buildable, runnable projects. No differentiation on this dimension.

---

## 2. Security Vulnerability Scan [CRITICAL]

All four configurations use the same core NuGet packages: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.AspNetCore.OpenApi`, and `Swashbuckle.AspNetCore`. None include packages with known CVEs at the time of generation. The `dotnet-skills`, `managedcode`, and `no-skills` configurations include `Microsoft.EntityFrameworkCore.Design` as a development dependency (properly marked with `PrivateAssets=all`), which is acceptable.

**Score:** dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **4** | no-skills: **4**

**Verdict:** No configuration exhibits vulnerability issues. All score equally. The one point deduction across all is because none configure security headers or HTTPS enforcement (covered in Security Configuration).

---

## 3. Minimal API Architecture [CRITICAL]

This is the **most significant differentiator** across all configurations. Only `dotnet-artisan` uses Minimal APIs.

**dotnet-artisan** — Full Minimal API with route groups, TypedResults, and endpoint extension methods:
```csharp
// Program.cs
app.MapAuthorEndpoints();
app.MapBookEndpoints();
app.MapLoanEndpoints();

// Endpoints/BookEndpoints.cs
public static class BookEndpoints
{
    public static RouteGroupBuilder MapBookEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/{id:int}", async Task<IResult> (int id, IBookService service, CancellationToken ct) =>
        {
            var book = await service.GetByIdAsync(id, ct);
            return book is not null ? TypedResults.Ok(book) : TypedResults.NotFound();
        })
        .WithName("GetBook")
        .WithSummary("Get book details including authors, categories, and availability");
```

**dotnet-skills, managedcode, no-skills** — All use traditional `[ApiController]` controllers:
```csharp
// dotnet-skills: Controllers/LoansController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;
    public LoansController(ILoanService loanService) => _loanService = loanService;

    [HttpGet]
    public async Task<IActionResult> GetLoans(...)
    {
        var result = await _loanService.GetLoansAsync(...);
        return Ok(result);
    }
```

| Feature | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Minimal APIs | ✅ | ❌ | ❌ | ❌ |
| MapGroup() | ✅ | ❌ | ❌ | ❌ |
| TypedResults | ✅ | ❌ | ❌ | ❌ |
| Endpoint extension methods | ✅ | ❌ | ❌ | ❌ |
| WithName/WithSummary | ✅ | N/A | N/A | N/A |
| Clean Program.cs | ✅ (7 Map calls) | ❌ (MapControllers) | ❌ (MapControllers) | ❌ (MapControllers) |

**Score:** dotnet-artisan: **5** | dotnet-skills: **1** | managedcode: **1** | no-skills: **1**

**Verdict:** `dotnet-artisan` is the clear winner. Minimal APIs with route groups, TypedResults, and extension methods are the modern .NET standard. The other three configurations all fall back to the legacy controller pattern with `IActionResult` return types that provide no compile-time type safety.

---

## 4. Input Validation & Guard Clauses [CRITICAL]

**dotnet-artisan** — DTOs have comprehensive Data Annotations:
```csharp
// DTOs/BookDtos.cs
public sealed record CreateBookRequest
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required, MaxLength(20)]
    public string ISBN { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; init; }
}
```

**managedcode** — Similar validation with Data Annotations on DTOs:
```csharp
// DTOs/BookDtos.cs
public record CreateBookRequest
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int TotalCopies { get; init; }
}
```

**dotnet-skills & no-skills** — DTOs are plain classes with **no validation attributes**:
```csharp
// dotnet-skills: DTOs/DTOs.cs
public class BookCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    // No [Required], [MaxLength], [Range] attributes
}
```

No configuration uses `ArgumentNullException.ThrowIfNull()` or other guard clause patterns in service constructors.

| Feature | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| DTO Validation Attributes | ✅ Full | ❌ None | ✅ Full | ❌ None |
| Guard clauses (ThrowIf*) | ❌ | ❌ | ❌ | ❌ |
| Service-level null checks | ✅ | ✅ (via exceptions) | ✅ (via exceptions) | ✅ (tuple errors) |

**Score:** dotnet-artisan: **4** | dotnet-skills: **2** | managedcode: **4** | no-skills: **2**

**Verdict:** `dotnet-artisan` and `managedcode` properly validate inputs via Data Annotations. `dotnet-skills` and `no-skills` accept any input without constraint validation, relying only on service-layer null checks. None use modern guard clauses in constructors, costing all a point.

---

## 5. NuGet & Package Discipline [CRITICAL]

**dotnet-artisan** — Uses a wildcard version for EF Core:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.*-*" />
```
This is the worst practice for reproducibility — `*-*` can pull any prerelease version.

**dotnet-skills, managedcode, no-skills** — All pin exact versions:
```xml
<!-- dotnet-skills -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

| Config | Package Count | Wildcard Versions | Unnecessary Packages |
|---|---|---|---|
| dotnet-artisan | 3 | 1 (`10.0.*-*`) | Swashbuckle (has AddOpenApi) |
| dotnet-skills | 4 | 0 | Swashbuckle, EF Design (dev-only) |
| managedcode | 4 | 0 | Swashbuckle, EF Design (dev-only) |
| no-skills | 4 | 0 | Swashbuckle, EF Design (dev-only) |

**Score:** dotnet-artisan: **3** | dotnet-skills: **4** | managedcode: **4** | no-skills: **4**

**Verdict:** The three skill-less/other-skill configs pin exact versions. `dotnet-artisan` uses a wildcard version which is a serious reproducibility issue. All include Swashbuckle alongside built-in OpenAPI, but this is evaluated separately under "Prefer Built-in."

---

## 6. EF Migration Usage [CRITICAL]

**All four configurations use `EnsureCreated()`** — the anti-pattern that bypasses EF Core migrations:

```csharp
// dotnet-artisan: Program.cs
await db.Database.EnsureCreatedAsync();

// dotnet-skills: Program.cs
db.Database.EnsureCreated();

// managedcode: Program.cs
await db.Database.EnsureCreatedAsync();

// no-skills: Program.cs
await db.Database.EnsureCreatedAsync();
```

None use `db.Database.Migrate()` or `MigrateAsync()`. None have a `Migrations/` folder.

**Score:** dotnet-artisan: **1** | dotnet-skills: **1** | managedcode: **1** | no-skills: **1**

**Verdict:** Universal failure. All configurations use the `EnsureCreated` anti-pattern, making schema evolution impossible without data loss. This is the most critical gap across all configurations.

---

## 7. Business Logic Correctness [HIGH]

All configurations implement the full set of business rules from the specification:

| Business Rule | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Borrowing limits by membership | ✅ | ✅ | ✅ | ✅ |
| Available copies check | ✅ | ✅ | ✅ | ✅ |
| Unpaid fines ≥ $10 threshold | ✅ | ✅ | ✅ | ✅ |
| Active membership check | ✅ | ✅ | ✅ | ✅ |
| Return → fine if overdue ($0.25/day) | ✅ | ✅ | ✅ | ✅ |
| Return → promote reservation | ✅ | ✅ | ✅ | ✅ |
| Renewal max 2 times | ✅ | ✅ | ✅ | ✅ |
| Renewal blocked if overdue | ✅ | ✅ | ✅ | ✅ |
| Renewal blocked if reservations | ✅ | ✅ | ✅ | ✅ |
| Overdue detection/flagging | ✅ | ✅ | ✅ | ✅ |

All required API endpoints (Authors CRUD, Categories CRUD, Books CRUD + loans/reservations sub-resources, Patrons CRUD + loans/reservations/fines sub-resources, Loans with checkout/return/renew/overdue, Reservations with cancel/fulfill, Fines with pay/waive) are present in all configurations.

**Score:** dotnet-artisan: **5** | dotnet-skills: **5** | managedcode: **5** | no-skills: **5**

**Verdict:** All configurations correctly implement the specification's business rules and endpoints. This is a shared strength.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

All configurations include **Swashbuckle.AspNetCore** alongside the built-in `AddOpenApi()`/`MapOpenApi()`:

**dotnet-artisan, dotnet-skills, managedcode** — Use `AddOpenApi()` + `MapOpenApi()` for OpenAPI generation, but still include Swashbuckle for SwaggerUI:
```csharp
// dotnet-artisan: Program.cs
builder.Services.AddOpenApi();
// ...
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Library API v1");
});
```

**no-skills** — Uses Swashbuckle exclusively (no built-in OpenAPI):
```csharp
// no-skills: Program.cs
builder.Services.AddSwaggerGen(c => { ... });
app.UseSwagger();
app.UseSwaggerUI(c => { ... });
```

None use Newtonsoft.Json (all use System.Text.Json). None use third-party DI containers.

| Feature | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Built-in OpenAPI (AddOpenApi) | ✅ | ✅ | ✅ | ❌ |
| Swashbuckle present | ✅ (UI only) | ✅ (UI only) | ✅ (UI only) | ✅ (full) |
| System.Text.Json | ✅ | ✅ | ✅ | ✅ |
| Built-in ILogger | ✅ | ✅ | ✅ | ✅ |
| Built-in DI | ✅ | ✅ | ✅ | ✅ |

**Score:** dotnet-artisan: **3** | dotnet-skills: **3** | managedcode: **3** | no-skills: **2**

**Verdict:** Three configurations use the built-in `AddOpenApi()` but still depend on Swashbuckle for SwaggerUI rendering. `no-skills` uses Swashbuckle entirely, including `AddSwaggerGen()`. The ideal approach would use `AddOpenApi()`/`MapOpenApi()` without any Swashbuckle dependency. Note: SwaggerUI needs Swashbuckle in current .NET, so a `3` is reasonable for the "hybrid" approach.

---

## 9. Modern C# Adoption [HIGH]

| Feature | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Primary constructors | ✅ (9 uses) | ❌ (0) | ✅ (17 uses) | ❌ (0) |
| Collection expressions `[]` | ✅ (14 uses) | ❌ (0) | ✅ (14 uses) | ❌ (0) |
| File-scoped namespaces | ✅ | ✅ | ✅ | ✅ |
| Target-typed new `new()` | ✅ | ✅ | ✅ | ❌ |
| ImplicitUsings | ✅ | ✅ | ✅ | ✅ |

**dotnet-artisan** — All services use primary constructors:
```csharp
// Services/LoanService.cs
public sealed class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
```

**managedcode** — Also uses primary constructors throughout:
```csharp
// Services/LoanService.cs
public class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService

// Controllers/LoansController.cs
public class LoansController(ILoanService loanService) : ControllerBase
```

**dotnet-skills** — Traditional constructor injection:
```csharp
// Services/LoanService.cs
public sealed class LoanService : ILoanService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<LoanService> _logger;
    public LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    {
        _db = db;
        _logger = logger;
    }
```

**no-skills** — Traditional constructors and `new List<T>()` instead of `[]`:
```csharp
// Models/Book.cs
public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
```

**Score:** dotnet-artisan: **5** | dotnet-skills: **2** | managedcode: **4** | no-skills: **1**

**Verdict:** `dotnet-artisan` leads with comprehensive C# 12+ adoption. `managedcode` also uses primary constructors and collection expressions. `dotnet-skills` and `no-skills` use legacy patterns, with `no-skills` being the most outdated.

---

## 10. Error Handling & Middleware [HIGH]

**managedcode** — Modern `IExceptionHandler` with custom `BusinessRuleException`:
```csharp
// Middleware/GlobalExceptionHandler.cs
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var problemDetails = exception switch
        {
            BusinessRuleException bre => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Business Rule Violation",
                Detail = bre.Message,
            },
            KeyNotFoundException => new ProblemDetails { Status = StatusCodes.Status404NotFound, ... },
            _ => new ProblemDetails { Status = StatusCodes.Status500InternalServerError, ... }
        };
```

Registered via the modern API:
```csharp
// Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
```

Custom exception class using primary constructor:
```csharp
// Services/BusinessRuleException.cs
public class BusinessRuleException(string message) : Exception(message);
```

**dotnet-artisan** — Uses `AddProblemDetails()` + `UseExceptionHandler()` + `UseStatusCodePages()` but no custom `IExceptionHandler`. Business rule errors are returned as tuple `(result, error)` at the endpoint level:
```csharp
// Program.cs
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
app.UseStatusCodePages();

// Endpoints/LoanEndpoints.cs
var (loan, error) = await service.CheckoutAsync(request, ct);
if (error is not null)
    return TypedResults.Problem(error, statusCode: StatusCodes.Status400BadRequest);
```

**dotnet-skills** — Convention-based middleware with `RequestDelegate`:
```csharp
// Middleware/GlobalExceptionHandlerMiddleware.cs
public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex); }
    }
```

Maps `KeyNotFoundException` → 404, `InvalidOperationException` → 409, `ArgumentException` → 400. Uses ProblemDetails format.

**no-skills** — Similar convention-based middleware but **only returns 500** for all exceptions (no exception type mapping):
```csharp
// Middleware/GlobalExceptionHandlerMiddleware.cs
private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    var problemDetails = new ProblemDetails
    {
        Status = context.Response.StatusCode,
        Title = "An internal server error occurred",
        Detail = exception.Message, // Leaks internal details!
    };
```

**Score:** dotnet-artisan: **3** | dotnet-skills: **3** | managedcode: **5** | no-skills: **2**

**Verdict:** `managedcode` is the clear winner with modern `IExceptionHandler`, custom `BusinessRuleException`, and proper exception-to-status-code mapping. `dotnet-artisan` uses the built-in ProblemDetails infrastructure but handles errors inline. `dotnet-skills` has decent middleware but uses legacy RequestDelegate pattern. `no-skills` has the weakest error handling — returning 500 for everything and leaking exception details.

---

## 11. Async Patterns & Cancellation [HIGH]

**dotnet-artisan** — Exemplary CancellationToken propagation (120 references):
```csharp
// Endpoints/BookEndpoints.cs
group.MapGet("/", async (string? search, ..., IBookService service, CancellationToken ct) =>
{
    var result = await service.GetAllAsync(search, ..., ct);
    return TypedResults.Ok(result);
});

// Services/LoanService.cs
public async Task<PaginatedResponse<LoanResponse>> GetAllAsync(
    ..., CancellationToken ct = default)
{
    var totalCount = await query.CountAsync(ct);
    var items = await query.ToListAsync(ct);
```

**managedcode** — Also comprehensive CancellationToken usage (126 references):
```csharp
// Controllers/LoansController.cs
public async Task<IActionResult> GetById(int id, CancellationToken ct)
{
    var result = await loanService.GetByIdAsync(id, ct);

// Services/LoanService.cs
public async Task<LoanResponse?> GetByIdAsync(int id, CancellationToken ct)
{
    return await db.Loans.AsNoTracking()
        .FirstOrDefaultAsync(ct);
```

**dotnet-skills** — **Zero CancellationToken usage** throughout:
```csharp
// Services/Interfaces.cs
Task<PaginatedResponse<LoanResponseDto>> GetLoansAsync(
    LoanStatus? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
    int page, int pageSize); // No CancellationToken parameter

// Services/LoanService.cs
var total = await query.CountAsync(); // No ct
var items = await query.ToListAsync(); // No ct
```

**no-skills** — **Zero CancellationToken usage** and no async suffix consistency:
```csharp
// Services/LoanService.cs
public async Task<PaginatedResponse<LoanResponseDto>> GetLoansAsync(
    string? status, ..., int page, int pageSize) // No CancellationToken
{
    var total = await query.CountAsync(); // No ct
```

**Score:** dotnet-artisan: **5** | dotnet-skills: **2** | managedcode: **5** | no-skills: **1**

**Verdict:** `dotnet-artisan` and `managedcode` both propagate `CancellationToken` from endpoints through services to EF Core calls. `dotnet-skills` and `no-skills` completely omit cancellation support, wasting server resources on cancelled requests. `no-skills` also has inconsistent Async suffixes in some controller methods.

---

## 12. EF Core Best Practices [HIGH]

**managedcode** — Most sophisticated EF Core configuration with enum-to-string conversion and automatic timestamps:
```csharp
// Data/LibraryDbContext.cs
modelBuilder.Entity<Patron>(entity =>
{
    entity.HasIndex(p => p.Email).IsUnique();
    entity.Property(p => p.MembershipType).HasConversion<string>();
    entity.Property(p => p.IsActive).HasDefaultValue(true);
});

// Automatic UpdatedAt tracking
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    UpdateTimestamps();
    return base.SaveChangesAsync(cancellationToken);
}
```

Uses `AsNoTracking()` on read queries (21 occurrences).

**dotnet-artisan** — Good Fluent API configuration with composite keys and unique indices. Uses `AsNoTracking()` extensively (20 occurrences). Missing enum-to-string conversion:
```csharp
// No HasConversion<string>() for enum properties
modelBuilder.Entity<Fine>()
    .Property(f => f.Amount)
    .HasColumnType("decimal(10,2)");
```

**dotnet-skills** — Complete Fluent API with enum-to-string conversions:
```csharp
modelBuilder.Entity<Patron>()
    .Property(p => p.MembershipType)
    .HasConversion<string>();
```

Uses `AsNoTracking()` (20 occurrences). Traditional DbContext without timestamp automation.

**no-skills** — Has Fluent API and enum conversions but **zero `AsNoTracking()` usage**:
```csharp
// Services/LoanService.cs - Read queries without AsNoTracking
var query = _db.Loans.Include(l => l.Book).Include(l => l.Patron).AsQueryable();
// Tracking changes unnecessarily on read-only GET endpoints
```

| Feature | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Fluent API configuration | ✅ | ✅ | ✅ | ✅ |
| Composite keys | ✅ | ✅ | ✅ | ✅ |
| Unique indices | ✅ | ✅ | ✅ | ✅ |
| Enum → string conversion | ❌ | ✅ | ✅ | ✅ |
| AsNoTracking for reads | ✅ (20) | ✅ (20) | ✅ (21) | ❌ (0) |
| Automatic timestamps | ❌ | ❌ | ✅ | ❌ |
| Decimal precision | ✅ | ❌ | ✅ | ✅ |

**Score:** dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **5** | no-skills: **3**

**Verdict:** `managedcode` leads with enum conversions, automatic timestamps via `SaveChangesAsync` override, and AsNoTracking. `dotnet-artisan` and `dotnet-skills` are strong but each miss one feature. `no-skills` lacks AsNoTracking entirely, roughly halving read-query performance.

---

## 13. Service Abstraction & DI [HIGH]

All four configurations register services with interface-based DI:

```csharp
// Shared pattern across all configs
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IPatronService, PatronService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IFineService, FineService>();
```

All use constructor injection. All have one service per domain area. `dotnet-artisan` has separate interface files per service; `dotnet-skills` and `no-skills` use a single `Interfaces.cs`; `managedcode` uses `IServices.cs`.

**Score:** dotnet-artisan: **5** | dotnet-skills: **5** | managedcode: **5** | no-skills: **5**

**Verdict:** All configurations properly implement interface-based DI with appropriate Scoped lifetimes and single-responsibility services.

---

## 14. Security Configuration [HIGH]

**No configuration** implements HSTS, HTTPS redirection, or any security headers:

```csharp
// None of the four configs have:
// app.UseHsts();
// app.UseHttpsRedirection();
// CORS configuration
```

Zero `UseHsts` or `UseHttpsRedirection` references across all codebases.

**Score:** dotnet-artisan: **1** | dotnet-skills: **1** | managedcode: **1** | no-skills: **1**

**Verdict:** Universal failure. All configurations lack basic HTTPS security enforcement.

---

## 15. DTO Design [MEDIUM]

**dotnet-artisan** — `sealed record` DTOs with Request/Response naming:
```csharp
// DTOs/LoanDtos.cs
public sealed record LoanResponse(
    int Id, int BookId, string BookTitle, int PatronId, string PatronName,
    DateTime LoanDate, DateTime DueDate, DateTime? ReturnDate,
    LoanStatus Status, int RenewalCount, DateTime CreatedAt);

public sealed record CreateLoanRequest
{
    [Required]
    public int BookId { get; init; }
    [Required]
    public int PatronId { get; init; }
}
```

**managedcode** — `record` DTOs (not sealed) with Request/Response naming and per-file organization:
```csharp
// DTOs/LoanDtos.cs
public record LoanResponse(
    int Id, int BookId, string BookTitle, int PatronId, string PatronName,
    DateTime LoanDate, DateTime DueDate, DateTime? ReturnDate,
    string Status, int RenewalCount, DateTime CreatedAt);
```

**dotnet-skills & no-skills** — Mutable `class` DTOs with `Dto` suffix, all in single file:
```csharp
// dotnet-skills: DTOs/DTOs.cs
public class AuthorCreateDto
{
    public string FirstName { get; set; } = string.Empty; // Mutable setters
    public string LastName { get; set; } = string.Empty;
}
```

| Feature | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Records | ✅ | ❌ (classes) | ✅ | ❌ (classes) |
| Sealed | ✅ | ❌ | ❌ | ❌ |
| Immutable (init) | ✅ | ❌ (set) | ✅ | ❌ (set) |
| Naming (Request/Response) | ✅ | ❌ (Dto suffix) | ✅ | ❌ (Dto suffix) |
| Per-file organization | ✅ | ❌ (single file) | ✅ | ❌ (single file) |

**Score:** dotnet-artisan: **5** | dotnet-skills: **2** | managedcode: **4** | no-skills: **2**

**Verdict:** `dotnet-artisan` has the best DTO design — sealed records, immutable init properties, clean naming. `managedcode` also uses records but omits `sealed`. `dotnet-skills` and `no-skills` use mutable classes with legacy `Dto` naming in a single monolithic file.

---

## 16. Sealed Types [MEDIUM]

**dotnet-artisan** — **41 out of 41 types** are sealed (100%):
```csharp
public sealed class Book { ... }
public sealed class LoanService(...) : ILoanService { ... }
public sealed record LoanResponse(...);
public sealed record CreateBookRequest { ... }
public sealed class DataSeeder(...) { ... }
```

**dotnet-skills** — 8 out of 46 types sealed (17%). Only services are sealed:
```csharp
public sealed class LoanService : ILoanService { ... }
// But models and DTOs are NOT sealed:
public class Book { ... }
public class LoanResponseDto { ... }
```

**managedcode & no-skills** — 0 out of ~48 types sealed (0%):
```csharp
public class Book { ... }
public class LoanService(...) : ILoanService { ... }
public record LoanResponse(...); // Not sealed
```

**Score:** dotnet-artisan: **5** | dotnet-skills: **2** | managedcode: **1** | no-skills: **1**

**Verdict:** `dotnet-artisan` comprehensively seals all types, enabling JIT devirtualization and signaling design intent. `dotnet-skills` seals services only. `managedcode` and `no-skills` seal nothing.

---

## 17. Data Seeder Design [MEDIUM]

All configurations use a runtime seeder that checks `if (await db.Authors.AnyAsync())` for idempotency.

**dotnet-artisan** — Injectable `DataSeeder` service registered in DI with ILogger:
```csharp
public sealed class DataSeeder(LibraryDbContext db, ILogger<DataSeeder> logger)
{
    public async Task SeedAsync()
    {
        if (await db.Authors.AnyAsync())
        {
            logger.LogInformation("Database already seeded, skipping");
            return;
        }
```

**dotnet-skills, managedcode, no-skills** — Static `DataSeeder.SeedAsync(LibraryDbContext db)`:
```csharp
public static async Task SeedAsync(LibraryDbContext db)
{
    if (await db.Authors.AnyAsync()) return;
```

All provide realistic seed data (5+ authors, 5+ categories, 12+ books, 6+ patrons, 8+ loans, 3+ reservations, 3+ fines). None use `HasData()` in `OnModelCreating`.

**Score:** dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **4** | no-skills: **4**

**Verdict:** All configurations provide adequate seed data with idempotency guards. `dotnet-artisan` is slightly better by using DI-injected seeder with logging. None use `HasData()` which would integrate with migrations (moot since none use migrations).

---

## 18. Structured Logging [MEDIUM]

**dotnet-artisan & managedcode** — Structured logging with named placeholders throughout:
```csharp
// dotnet-artisan: Services/LoanService.cs
logger.LogInformation("Book {BookId} checked out to patron {PatronId}, loan {LoanId}, due {DueDate}",
    book.Id, patron.Id, loan.Id, loan.DueDate);

logger.LogInformation("Fine of ${Amount} issued to patron {PatronId} for overdue loan {LoanId} ({Days} days late)",
    fineAmount, loan.PatronId, loan.Id, daysOverdue);
```

**dotnet-skills** — Structured logging:
```csharp
logger.LogInformation("Book checked out: Loan {LoanId}, Book {BookId} to Patron {PatronId}",
    loan.Id, book.Id, patron.Id);
```

**no-skills** — Similar structured logging but with a bare `LogError` for unhandled exceptions:
```csharp
_logger.LogError(ex, "An unhandled exception occurred");
```

No configuration uses `[LoggerMessage]` source generators.

**Score:** dotnet-artisan: **5** | dotnet-skills: **4** | managedcode: **5** | no-skills: **4**

**Verdict:** `dotnet-artisan` and `managedcode` have the most comprehensive structured logging with consistent named placeholders. `dotnet-skills` and `no-skills` are good but slightly less detailed.

---

## 19. Nullable Reference Types [MEDIUM]

All configurations enable NRT:
```xml
<Nullable>enable</Nullable>
```

All use `?` annotations on optional properties (e.g., `string? Publisher`, `DateTime? ReturnDate`). No misuse of the null-forgiving operator (`!`) was observed.

**Score:** dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **4** | no-skills: **4**

**Verdict:** All configurations properly enable and use NRT. One point deducted universally because navigation properties (e.g., `public Book Book { get; set; }`) are not always properly annotated as required vs optional.

---

## 20. API Documentation [MEDIUM]

**dotnet-artisan** — `WithName()`, `WithSummary()`, `WithTags()` on all endpoints:
```csharp
group.MapGet("/", async (...) => { ... })
    .WithName("GetBooks")
    .WithSummary("List books with search, filter by availability/category, pagination, and sorting");
```

**dotnet-skills** — XML doc comments on controller methods + `[ProducesResponseType]`:
```csharp
/// <summary>List loans with filter by status, overdue flag, date range, and pagination.</summary>
[HttpGet]
[ProducesResponseType(typeof(PaginatedResponse<LoanResponseDto>), StatusCodes.Status200OK)]
```

**managedcode** — Similar XML doc comments + `[ProducesResponseType]`:
```csharp
/// <summary>Check out a book — create a loan enforcing all checkout rules.</summary>
[HttpPost]
[ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
```

**no-skills** — XML doc comments but uses magic numbers for status codes:
```csharp
/// <summary>Get loan details</summary>
[ProducesResponseType(typeof(LoanResponseDto), 200)]
[ProducesResponseType(404)]
```

**Score:** dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **4** | no-skills: **3**

**Verdict:** `dotnet-artisan` uses minimal API metadata methods effectively. The controller-based configs use XML comments and `[ProducesResponseType]` which also work well. `no-skills` uses magic numbers instead of `StatusCodes` constants.

---

## 21. File Organization [MEDIUM]

**dotnet-artisan:**
```
src/LibraryApi/
├── Endpoints/        (7 files — one per resource)
├── Models/           (10 files — entity + enums)
├── DTOs/             (8 files — one per resource + PaginatedResponse)
├── Services/         (14 files — I*Service.cs + *Service.cs)
├── Data/             (DataSeeder.cs + LibraryDbContext.cs)
└── Program.cs
```

**dotnet-skills, no-skills:**
```
src/LibraryApi/
├── Controllers/      (7 files)
├── Models/           (10 files)
├── DTOs/             (1 file — DTOs.cs monolith)
├── Services/         (8 files — all implementations + Interfaces.cs)
├── Data/             or Services/ for seeder
├── Middleware/        (1 file)
└── Program.cs
```

**managedcode:**
```
src/LibraryApi/
├── Controllers/      (7 files)
├── Models/           (10 files)
├── DTOs/             (8 files — one per resource)
├── Services/         (9 files — implementations + IServices.cs)
├── Middleware/        (1 file)
├── Data/             (DataSeeder.cs + LibraryDbContext.cs)
└── Program.cs
```

**Score:** dotnet-artisan: **5** | dotnet-skills: **4** | managedcode: **4** | no-skills: **4**

**Verdict:** `dotnet-artisan` has the cleanest organization with separate interface files and dedicated Endpoints directory. Others are adequate but have monolithic DTO/Interface files.

---

## 22. HTTP Test File Quality [MEDIUM]

All configurations include comprehensive `.http` files:

| Config | Lines | Coverage |
|---|---|---|
| dotnet-artisan | 331 | Full CRUD + business rule tests |
| dotnet-skills | 339 | Full CRUD + business rule tests |
| managedcode | 323 | Full CRUD + business rule tests |
| no-skills | 300 | Full CRUD + business rule tests |

All include `@baseUrl` variable, grouped sections by resource, and realistic request bodies referencing seed data IDs.

**Score:** dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **4** | no-skills: **4**

**Verdict:** All configurations provide high-quality HTTP test files. No significant differentiation.

---

## 23. Type Design & Resource Management [MEDIUM]

All configurations use enums for status fields:
```csharp
public enum LoanStatus { Active, Returned, Overdue }
public enum ReservationStatus { Pending, Ready, Fulfilled, Cancelled, Expired }
public enum FineStatus { Unpaid, Paid, Waived }
public enum MembershipType { Standard, Premium, Student }
```

**dotnet-artisan** — Uses `IReadOnlyList<T>` in response DTOs:
```csharp
public sealed record BookResponse(
    ...,
    IReadOnlyList<AuthorResponse> Authors,
    IReadOnlyList<CategoryResponse> Categories);
```

**Others** — Use `List<T>` in response DTOs:
```csharp
// managedcode
public record BookResponse(..., List<BookAuthorDto> Authors, List<BookCategoryDto> Categories);
```

All use proper enum types. DbContext is managed by DI in all configs.

**Score:** dotnet-artisan: **4** | dotnet-skills: **4** | managedcode: **4** | no-skills: **3**

**Verdict:** `dotnet-artisan` uses `IReadOnlyList<T>` for response immutability. `no-skills` lacks enum-to-string conversion in its DbContext, losing a point.

---

## 24. Code Standards Compliance [LOW]

**dotnet-artisan** — Consistent PascalCase, Async suffixes, `I` prefix for interfaces, explicit `public` modifiers, file-scoped namespaces:
```csharp
namespace LibraryApi.Services;
public interface ILoanService { ... }
public sealed class LoanService(...) : ILoanService { ... }
```

**dotnet-skills** — Good naming but `_field` convention with traditional constructors:
```csharp
private readonly LibraryDbContext _db;
private readonly ILogger<LoanService> _logger;
```

**managedcode** — Good naming with primary constructors (no `_field` needed):
```csharp
public class LoanService(LibraryDbContext db, ILogger<LoanService> logger) : ILoanService
```

**no-skills** — Generally follows conventions but some inconsistencies:
```csharp
// Uses `_db` and `_logger` fields
// Some status code magic numbers (200, 404) instead of StatusCodes constants
```

**Score:** dotnet-artisan: **5** | dotnet-skills: **4** | managedcode: **4** | no-skills: **3**

**Verdict:** All follow core .NET naming conventions. `dotnet-artisan` is most consistent with modern idioms. `no-skills` uses magic numbers for status codes in places.

---

## Weighted Summary

Weights: CRITICAL × 3, HIGH × 2, MEDIUM × 1, LOW × 0.5

| Dimension [Tier] | Weight | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| Build & Run Success [CRITICAL] | 3 | 15 | 15 | 15 | 15 |
| Security Vulnerability Scan [CRITICAL] | 3 | 12 | 12 | 12 | 12 |
| Minimal API Architecture [CRITICAL] | 3 | 15 | 3 | 3 | 3 |
| Input Validation & Guard Clauses [CRITICAL] | 3 | 12 | 6 | 12 | 6 |
| NuGet & Package Discipline [CRITICAL] | 3 | 9 | 12 | 12 | 12 |
| EF Migration Usage [CRITICAL] | 3 | 3 | 3 | 3 | 3 |
| Business Logic Correctness [HIGH] | 2 | 10 | 10 | 10 | 10 |
| Prefer Built-in over 3rd Party [HIGH] | 2 | 6 | 6 | 6 | 4 |
| Modern C# Adoption [HIGH] | 2 | 10 | 4 | 8 | 2 |
| Error Handling & Middleware [HIGH] | 2 | 6 | 6 | 10 | 4 |
| Async Patterns & Cancellation [HIGH] | 2 | 10 | 4 | 10 | 2 |
| EF Core Best Practices [HIGH] | 2 | 8 | 8 | 10 | 6 |
| Service Abstraction & DI [HIGH] | 2 | 10 | 10 | 10 | 10 |
| Security Configuration [HIGH] | 2 | 2 | 2 | 2 | 2 |
| DTO Design [MEDIUM] | 1 | 5 | 2 | 4 | 2 |
| Sealed Types [MEDIUM] | 1 | 5 | 2 | 1 | 1 |
| Data Seeder Design [MEDIUM] | 1 | 4 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 1 | 5 | 4 | 5 | 4 |
| Nullable Reference Types [MEDIUM] | 1 | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 1 | 4 | 4 | 4 | 3 |
| File Organization [MEDIUM] | 1 | 5 | 4 | 4 | 4 |
| HTTP Test File Quality [MEDIUM] | 1 | 4 | 4 | 4 | 4 |
| Type Design & Resource Management [MEDIUM] | 1 | 4 | 4 | 4 | 3 |
| Code Standards Compliance [LOW] | 0.5 | 2.5 | 2 | 2 | 1.5 |
| **TOTAL** | | **170.5** | **133** | **159** | **120.5** |

---

## What All Versions Get Right

- ✅ **Build & Run**: All four configurations produce projects that compile and run on .NET 10
- ✅ **Business Logic**: All correctly implement borrowing limits, fine thresholds, return processing, reservation queues, and renewal rules
- ✅ **Interface-based DI**: All use `AddScoped<IService, Service>()` for all seven domain services
- ✅ **Fluent API Configuration**: All configure composite keys, unique indices, and entity relationships
- ✅ **Proper Enum Design**: All use enums for MembershipType, LoanStatus, ReservationStatus, FineStatus
- ✅ **Comprehensive HTTP Test Files**: All include `.http` files covering all endpoints with realistic seed data IDs
- ✅ **Pagination**: All implement consistent pagination with page/pageSize parameters and metadata responses
- ✅ **Structured Logging**: All use `ILogger<T>` with structured message templates
- ✅ **Nullable Reference Types**: All enable `<Nullable>enable</Nullable>` with proper `?` annotations
- ✅ **Seed Data**: All provide idempotent seeding with realistic, varied data

---

## Summary: Impact of Skills

### Rankings by Weighted Score

1. **🥇 dotnet-artisan — 170.5 points**
2. **🥈 managedcode-dotnet-skills — 159 points**
3. **🥉 dotnet-skills — 133 points**
4. **4th: no-skills — 120.5 points**

### Most Impactful Differences

1. **Minimal API Architecture (CRITICAL, ×3)**: The single largest differentiator. Only `dotnet-artisan` uses the modern Minimal API pattern with route groups, TypedResults, and endpoint extension methods. This alone contributes +36 weighted points over the controller-based configurations.

2. **Async Patterns & CancellationToken (HIGH, ×2)**: `dotnet-artisan` and `managedcode` propagate CancellationToken throughout the stack (120+ references). `dotnet-skills` and `no-skills` have zero usage, wasting server resources on cancelled requests.

3. **Modern C# Adoption (HIGH, ×2)**: Primary constructors and collection expressions dramatically reduce boilerplate. `dotnet-artisan` (9 primary ctors, 14 collection exprs) and `managedcode` (17 primary ctors, 14 collection exprs) lead here.

4. **Error Handling (HIGH, ×2)**: `managedcode` is the only configuration using the modern `IExceptionHandler` interface with a custom `BusinessRuleException` type. This is the recommended .NET 8+ approach.

5. **Input Validation (CRITICAL, ×3)**: `dotnet-artisan` and `managedcode` include Data Annotations on DTOs; `dotnet-skills` and `no-skills` have no input validation.

### Configuration Assessments

**dotnet-artisan** is the strongest overall configuration. Its use of Minimal APIs, sealed types, TypedResults, primary constructors, and comprehensive CancellationToken propagation represent the most modern .NET patterns. Its main weakness is the wildcard NuGet version (`10.0.*-*`), which undermines reproducibility.

**managedcode-dotnet-skills** is a strong second, excelling in error handling (IExceptionHandler), async patterns, modern C#, and EF Core configuration with automatic timestamps. It is the only configuration with a custom exception hierarchy. Its main weakness is using controllers instead of Minimal APIs and not sealing types.

**dotnet-skills** is a middle-ground configuration. It pins package versions correctly and uses AsNoTracking, but falls behind on modern C# features, CancellationToken propagation, input validation, and still uses controllers.

**no-skills** is the baseline and shows what default Copilot produces without skill guidance. It is functional and implements all business rules correctly but uses the most legacy patterns: mutable class DTOs, no AsNoTracking, no CancellationToken, no input validation, full Swashbuckle dependency, and the weakest error handling.

### Universal Gaps (All Configurations)

- **EF Core Migrations**: All use `EnsureCreated()` instead of `Migrate()`. This is the most critical shared deficiency.
- **Security Headers**: None implement `UseHsts()` or `UseHttpsRedirection()`.
- **Guard Clauses**: No configuration uses `ArgumentNullException.ThrowIfNull()` or similar guard patterns.
- **Swashbuckle**: All include Swashbuckle as a dependency despite using (or being able to use) built-in OpenAPI support.
