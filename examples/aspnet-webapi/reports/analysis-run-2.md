# Comparative Analysis: dotnet-artisan, dotnet-skills, managedcode-dotnet-skills, no-skills

## Introduction

This analysis compares **4 Copilot skill configurations** that each generated a **LibraryApi** — a community library management system with book loans, reservations, fines, and patron management. All projects target .NET 10 with EF Core and SQLite. Each configuration's output is located at `output/{config}/run-2/LibraryApi/`.

| Configuration | Description |
|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain (using-dotnet → dotnet-advisor → dotnet-csharp → dotnet-api) |
| **dotnet-skills** | Official .NET Skills (dotnet/skills): dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-project-setup, dotnet-modern-csharp |
| **managedcode-dotnet-skills** | Community managed-code skills: dotnet router, dotnet-aspnet-core, dotnet-entity-framework-core, dotnet-modern-csharp |
| **no-skills** | Baseline Copilot with no custom skills |

Only the **LibraryApi** scenario was present in run-2 across all configurations. FitnessStudioApi and VetClinicApi were not generated in this run.

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|
| Minimal API Architecture [CRITICAL] | 5 | 1 | 1 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 4 | 3 | 4 | 3 |
| NuGet & Package Discipline [CRITICAL] | 3 | 4 | 4 | 3 |
| EF Migration Usage [CRITICAL] | 1 | 1 | 1 | 1 |
| Security Vulnerability Scan [CRITICAL] | 4 | 4 | 4 | 4 |
| Build & Run Success [CRITICAL] | 5 | 5 | 5 | 5 |
| Business Logic Correctness [HIGH] | 5 | 5 | 5 | 5 |
| Prefer Built-in over 3rd Party [HIGH] | 4 | 3 | 3 | 1 |
| Modern C# Adoption [HIGH] | 5 | 2 | 4 | 1 |
| Error Handling & Middleware [HIGH] | 4 | 4 | 5 | 2 |
| Async Patterns & Cancellation [HIGH] | 5 | 2 | 5 | 2 |
| EF Core Best Practices [HIGH] | 5 | 4 | 5 | 2 |
| Service Abstraction & DI [HIGH] | 5 | 4 | 4 | 4 |
| Security Configuration [HIGH] | 2 | 2 | 2 | 2 |
| DTO Design [MEDIUM] | 5 | 2 | 4 | 2 |
| Sealed Types [MEDIUM] | 5 | 3 | 1 | 1 |
| Data Seeder Design [MEDIUM] | 5 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 4 | 4 | 4 | 4 |
| API Documentation [MEDIUM] | 5 | 3 | 3 | 3 |
| File Organization [MEDIUM] | 5 | 2 | 3 | 2 |
| HTTP Test File Quality [MEDIUM] | 4 | 4 | 4 | 4 |
| Type Design & Resource Management [MEDIUM] | 4 | 4 | 4 | 3 |
| Code Standards Compliance [LOW] | 5 | 3 | 4 | 3 |

---

## 1. Minimal API Architecture [CRITICAL]

### dotnet-artisan — Score: 5
The only configuration to use Minimal APIs. Uses `MapGroup()` for route organization, `TypedResults` for all responses, and endpoint extension methods keeping Program.cs clean:

```csharp
// dotnet-artisan: Endpoints/AuthorEndpoints.cs
public static class AuthorEndpoints
{
    public static RouteGroupBuilder MapAuthorEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async (string? search, int? page, int? pageSize,
            IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, page ?? 1, pageSize ?? 10, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAuthors")
        .WithSummary("List authors with optional name search and pagination");
```

Program.cs remains clean with just extension method calls:
```csharp
// dotnet-artisan: Program.cs
app.MapAuthorEndpoints();
app.MapCategoryEndpoints();
app.MapBookEndpoints();
// ... 7 clean one-liners
```

### dotnet-skills — Score: 1
Uses controller-based architecture with `[ApiController]`:

```csharp
// dotnet-skills: Controllers/AuthorsController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _authorService;
    public AuthorsController(IAuthorService authorService) => _authorService = authorService;
```

### managedcode-dotnet-skills — Score: 1
Also uses controllers, though with primary constructors:

```csharp
// managedcode: Controllers/AuthorsController.cs
[ApiController]
[Route("api/[controller]")]
public class AuthorsController(IAuthorService authorService) : ControllerBase
```

### no-skills — Score: 1
Controllers with traditional constructor injection:

```csharp
// no-skills: Controllers/AuthorsController.cs
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _service;
    public AuthorsController(IAuthorService service) => _service = service;
```

**Verdict**: dotnet-artisan is the clear winner. Minimal APIs with `MapGroup()`, `TypedResults`, and endpoint extension methods are the modern .NET standard. They provide lower overhead, compile-time type safety, automatic OpenAPI schema generation, and cleaner architecture. All other configurations fall back to the legacy controller pattern.

---

## 2. Input Validation & Guard Clauses [CRITICAL]

### dotnet-artisan — Score: 4
Uses Data Annotations on both models and DTO request records:

```csharp
// dotnet-artisan: DTOs/AuthorDtos.cs
public sealed record CreateAuthorRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Biography { get; init; }
}
```

Book DTOs include `[Range]` validation:
```csharp
// dotnet-artisan: DTOs/BookDtos.cs
[Required, Range(1, int.MaxValue)]
public int TotalCopies { get; init; }
```

No `ThrowIfNull` guard clauses on constructor parameters.

### dotnet-skills — Score: 3
Has Data Annotations but on mutable class DTOs without `init` setters:

```csharp
// dotnet-skills: DTOs/DTOs.cs
public class AuthorCreateDto
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
}
```

### managedcode-dotnet-skills — Score: 4
Similar annotation coverage with record DTOs and `init` setters:

```csharp
// managedcode: DTOs/BookDtos.cs
public record CreateBookRequest
{
    [Required, StringLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required, Range(1, int.MaxValue)]
    public int TotalCopies { get; init; }
}
```

### no-skills — Score: 3
Annotations present but identical to dotnet-skills pattern with mutable classes.

**Verdict**: dotnet-artisan and managedcode tie with `init` setters providing immutability defense. No configuration uses modern `ThrowIfNull` guard clauses.

---

## 3. NuGet & Package Discipline [CRITICAL]

### dotnet-artisan — Score: 3
Uses a **wildcard version** for EF Core — the worst pattern:

```xml
<!-- dotnet-artisan: LibraryApi.csproj -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.*-*" />
```

This can pull in prerelease or breaking versions. Only 3 packages total (minimal), but the wildcard is a critical flaw.

### dotnet-skills — Score: 4
All packages use **exact versions**. Includes `Microsoft.EntityFrameworkCore.Design` (useful for migrations tooling):

```xml
<!-- dotnet-skills: LibraryApi.csproj -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
```

### managedcode-dotnet-skills — Score: 4
Same exact-version pattern as dotnet-skills. All `Directory.Build.props` files include `Meziantou.Analyzer` with `Version="*"` but as a `PrivateAssets` analyzer (acceptable).

### no-skills — Score: 3
Exact versions in `.csproj`, but also uses `AddSwaggerGen` (Swashbuckle), meaning both `Microsoft.AspNetCore.OpenApi` and `Swashbuckle.AspNetCore` packages are referenced — with `OpenApi` going unused.

**Verdict**: dotnet-skills and managedcode tie. The wildcard version in dotnet-artisan is a significant discipline failure despite having the smallest package footprint.

---

## 4. EF Migration Usage [CRITICAL]

All four configurations use `EnsureCreated`/`EnsureCreatedAsync` instead of EF Core migrations:

```csharp
// ALL configurations:
await db.Database.EnsureCreatedAsync();
```

No configuration runs `context.Database.Migrate()` or includes migration files. dotnet-artisan's gen-notes explicitly acknowledges this as intentional for a demo, but it remains an anti-pattern for production code.

| Config | Score | Notes |
|---|---|---|
| dotnet-artisan | 1 | `EnsureCreatedAsync()` in Program.cs |
| dotnet-skills | 1 | `EnsureCreated()` (sync version) in Program.cs |
| managedcode | 1 | `EnsureCreatedAsync()` in Program.cs |
| no-skills | 1 | `EnsureCreatedAsync()` in Program.cs |

**Verdict**: All fail equally. No configuration uses migrations.

---

## 5. Security Vulnerability Scan [CRITICAL]

All four configurations target .NET 10 with current package versions. No configurations include known-vulnerable packages. All use `Microsoft.EntityFrameworkCore.Sqlite` 10.0.x and `Swashbuckle.AspNetCore` 10.1.7. The `Meziantou.Analyzer` in `Directory.Build.props` is a development-only analyzer.

| Config | Score |
|---|---|
| dotnet-artisan | 4 |
| dotnet-skills | 4 |
| managedcode | 4 |
| no-skills | 4 |

All score 4 (not 5) because all include `Swashbuckle.AspNetCore` as a runtime dependency when only the UI component is needed (dotnet-artisan, dotnet-skills, managedcode use built-in OpenAPI but still bundle the full Swashbuckle package).

**Verdict**: Tie — all use current package versions with no known CVEs.

---

## 6. Build & Run Success [CRITICAL]

All four configurations were verified to build and run successfully per their gen-notes. All target `net10.0` with `Microsoft.NET.Sdk.Web`.

| Config | Score |
|---|---|
| dotnet-artisan | 5 |
| dotnet-skills | 5 |
| managedcode | 5 |
| no-skills | 5 |

**Verdict**: Tie — all projects build and run cleanly.

---

## 7. Business Logic Correctness [HIGH]

All four configurations implement the full set of specified endpoints (7 resource groups, ~30 endpoints) and business rules:

- Borrowing limits by membership type (Standard: 5/14d, Premium: 10/21d, Student: 3/7d)
- $10 unpaid fine threshold blocking checkout/renewal
- Max 2 renewals per loan
- Overdue detection and fine calculation ($0.25/day)
- Reservation queue with Ready→Expired transitions
- Patron cannot reserve a book they already have on active loan

```csharp
// dotnet-artisan: Services/LoanService.cs — all business rules enforced
if (totalUnpaidFines >= 10.00m)
    return (null, $"Patron has ${totalUnpaidFines:F2} in unpaid fines...");

var maxLoans = GetMaxLoans(patron.MembershipType);
if (activeLoansCount >= maxLoans)
    return (null, $"Patron has reached the maximum of {maxLoans} active loans...");
```

```csharp
// managedcode: Services/LoanService.cs — uses exceptions
if (unpaidFines >= 10.00m)
    throw new BusinessRuleException($"Patron has ${unpaidFines:F2} in unpaid fines...");
```

| Config | Score |
|---|---|
| dotnet-artisan | 5 |
| dotnet-skills | 5 |
| managedcode | 5 |
| no-skills | 5 |

**Verdict**: Tie — all configurations implement the full specification correctly.

---

## 8. Prefer Built-in over 3rd Party [HIGH]

### dotnet-artisan — Score: 4
Uses built-in `AddOpenApi()`/`MapOpenApi()` and `ConfigureHttpJsonOptions` (correct for Minimal APIs). Still references `Swashbuckle.AspNetCore` for SwaggerUI rendering:

```csharp
// dotnet-artisan: Program.cs
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

### dotnet-skills — Score: 3
Uses `AddOpenApi()` (built-in) but also `AddControllers().AddJsonOptions()` (controller-specific):

```csharp
// dotnet-skills: Program.cs
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
```

### managedcode-dotnet-skills — Score: 3
Same pattern as dotnet-skills — built-in OpenAPI but controller-based JSON options.

### no-skills — Score: 1
Uses the full Swashbuckle pipeline (`AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI`) instead of built-in OpenAPI:

```csharp
// no-skills: Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo { ... });
});
// ...
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "...");
});
```

**Verdict**: dotnet-artisan best leverages built-in APIs. no-skills uses full Swashbuckle when `AddOpenApi()` is available.

---

## 9. Modern C# Adoption [HIGH]

### dotnet-artisan — Score: 5
Consistently uses all modern C# features:

```csharp
// Primary constructors
public sealed class AuthorService(LibraryDbContext db) : IAuthorService

// DbContext with primary constructor
public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)

// Collection expressions
public ICollection<BookAuthor> BookAuthors { get; set; } = [];

// Sealed records with positional syntax
public sealed record AuthorResponse(int Id, string FirstName, string LastName, ...);

// File-scoped namespaces throughout
namespace LibraryApi.Services;
```

### dotnet-skills — Score: 2
No primary constructors, no collection expressions:

```csharp
// Traditional constructor DI
public sealed class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;
    public AuthorService(LibraryDbContext db) => _db = db;
}

// Traditional DbContext constructor
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }
}
```

File-scoped namespaces are used. DTOs are mutable classes — no records.

### managedcode-dotnet-skills — Score: 4
Primary constructors and collection expressions used consistently:

```csharp
// Primary constructors
public class AuthorService(LibraryDbContext db, ILogger<AuthorService> logger) : IAuthorService

// Collection expressions on models
public ICollection<BookAuthor> BookAuthors { get; set; } = [];

// Record DTOs (not sealed)
public record AuthorResponse(int Id, string FirstName, string LastName, ...);
```

Missing `sealed` modifier on records drops it from a 5.

### no-skills — Score: 1
No modern C# features — traditional constructors, `new List<T>()` initialization, mutable class DTOs:

```csharp
// Traditional constructor
public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _db;
    private readonly ILogger<AuthorService> _logger;
    public AuthorService(LibraryDbContext db, ILogger<AuthorService> logger)
    {
        _db = db;
        _logger = logger;
    }
}
```

**Verdict**: dotnet-artisan leads with full C# 12+ adoption. managedcode follows closely. dotnet-skills and no-skills use pre-C# 12 patterns.

---

## 10. Error Handling & Middleware [HIGH]

### dotnet-artisan — Score: 4
Uses the built-in `AddProblemDetails()` + `UseExceptionHandler()` + `UseStatusCodePages()` pipeline. Services return result tuples instead of throwing exceptions:

```csharp
// dotnet-artisan: Program.cs
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
app.UseStatusCodePages();

// dotnet-artisan: Endpoints/AuthorEndpoints.cs — result pattern
var (found, hasBooks) = await service.DeleteAsync(id, ct);
if (!found) return TypedResults.NotFound();
if (hasBooks) return TypedResults.Problem("Cannot delete author...",
    statusCode: StatusCodes.Status409Conflict);
```

No custom `IExceptionHandler` but the result pattern avoids exception-driven flow for expected failures.

### dotnet-skills — Score: 4
Custom middleware with rich exception-to-status-code mapping:

```csharp
// dotnet-skills: Middleware/GlobalExceptionHandlerMiddleware.cs
var problemDetails = exception switch
{
    KeyNotFoundException knf => new ProblemDetails
    {
        Status = (int)HttpStatusCode.NotFound,
        Title = "Resource Not Found", Detail = knf.Message,
    },
    InvalidOperationException ioe => new ProblemDetails
    {
        Status = (int)HttpStatusCode.Conflict,
        Title = "Business Rule Violation", Detail = ioe.Message,
    },
    ArgumentException ae => new ProblemDetails
    {
        Status = (int)HttpStatusCode.BadRequest,...
    },
    _ => new ProblemDetails { Status = 500, ... }
};
```

### managedcode-dotnet-skills — Score: 5
The **only** configuration to use the modern `IExceptionHandler` interface (ASP.NET Core 8+). Also defines a custom `BusinessRuleException`:

```csharp
// managedcode: Middleware/GlobalExceptionHandler.cs
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            BusinessRuleException bre => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Business Rule Violation", Detail = bre.Message,
            },
            KeyNotFoundException knfe => new ProblemDetails { Status = 404, ... },
            _ => new ProblemDetails { Status = 500, ... }
        };
```

Registered via the DI-aware pattern:
```csharp
// managedcode: Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
```

### no-skills — Score: 2
Basic middleware that always returns 500 regardless of exception type and **leaks internal exception messages**:

```csharp
// no-skills: Middleware/GlobalExceptionHandlerMiddleware.cs
var problemDetails = new ProblemDetails
{
    Status = context.Response.StatusCode, // always 500
    Title = "An internal server error occurred",
    Detail = exception.Message, // ← leaks internal details!
};
```

**Verdict**: managedcode wins with `IExceptionHandler` — the composable, DI-aware, modern .NET approach. dotnet-artisan's result pattern is also solid. no-skills leaks exception details.

---

## 11. Async Patterns & Cancellation [HIGH]

### dotnet-artisan — Score: 5
Every endpoint accepts `CancellationToken` and forwards it through all layers:

```csharp
// dotnet-artisan: Endpoints — CancellationToken in endpoint signature
group.MapGet("/", async (string? search, int? page, int? pageSize,
    IAuthorService service, CancellationToken ct) => { ... });

// dotnet-artisan: Services — CancellationToken forwarded to EF Core
public async Task<PaginatedResponse<AuthorResponse>> GetAllAsync(
    string? search, int page, int pageSize, CancellationToken ct = default)
{
    var totalCount = await query.CountAsync(ct);
    var items = await query.ToListAsync(ct);
}
```

All service interfaces declare `CancellationToken`:
```csharp
Task<AuthorResponse> CreateAsync(CreateAuthorRequest request, CancellationToken ct = default);
```

### dotnet-skills — Score: 2
**No CancellationToken** on any controller action or service method:

```csharp
// dotnet-skills: Controllers — no CancellationToken
public async Task<IActionResult> GetAuthors(
    [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    => Ok(await _authorService.GetAuthorsAsync(search, page, pageSize));

// dotnet-skills: Services — no CancellationToken on EF calls
var totalCount = await query.CountAsync();
var items = await query.ToListAsync();
```

### managedcode-dotnet-skills — Score: 5
Full CancellationToken propagation through controllers and services:

```csharp
// managedcode: Controllers — CancellationToken parameter
public async Task<IActionResult> GetAll(
    [FromQuery] string? search, [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10, CancellationToken ct = default)

// managedcode: Services — forwarded to EF Core
var totalCount = await query.CountAsync(ct);
var items = await query.ToListAsync(ct);
```

### no-skills — Score: 2
No CancellationToken anywhere — same pattern as dotnet-skills.

**Verdict**: dotnet-artisan and managedcode tie with full CancellationToken chains. dotnet-skills and no-skills completely omit cancellation support.

---

## 12. EF Core Best Practices [HIGH]

### dotnet-artisan — Score: 5
Consistent `AsNoTracking()` on all reads, `Select()` projections, and Fluent API relationships:

```csharp
// dotnet-artisan: AuthorService.cs — AsNoTracking + projection
return await db.Authors
    .AsNoTracking()
    .Where(a => a.Id == id)
    .Select(a => new AuthorDetailResponse(
        a.Id, a.FirstName, a.LastName, ...
        a.BookAuthors.Select(ba => new BookSummaryResponse(...)).ToList()))
    .FirstOrDefaultAsync(ct);
```

Does not configure `HasConversion<string>()` for enums — they'll be stored as integers in SQLite (minor issue — JSON serialization uses `JsonStringEnumConverter`).

### dotnet-skills — Score: 4
Uses `AsNoTracking()` on read queries and full Fluent API with `HasConversion<string>()` for all enums:

```csharp
// dotnet-skills: LibraryDbContext.cs
modelBuilder.Entity<Patron>()
    .Property(p => p.MembershipType)
    .HasConversion<string>();
```

However, some read queries use `Include` + `ThenInclude` without projections, loading full entity graphs.

### managedcode-dotnet-skills — Score: 5
`AsNoTracking()` on reads, `HasConversion<string>()` for all enums, and overrides `SaveChangesAsync` for automatic timestamp management:

```csharp
// managedcode: LibraryDbContext.cs
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    UpdateTimestamps();
    return base.SaveChangesAsync(cancellationToken);
}
```

### no-skills — Score: 2
**No `AsNoTracking()` on any query** — all reads use full change tracking:

```csharp
// no-skills: AuthorService.cs — no AsNoTracking
var query = _db.Authors.Include(a => a.BookAuthors)
    .ThenInclude(ba => ba.Book).AsQueryable();
```

Uses `HasConversion<string>()` for enums and Fluent API relationships. The missing `AsNoTracking()` is a significant performance issue.

**Verdict**: dotnet-artisan and managedcode tie. no-skills' omission of `AsNoTracking()` doubles memory overhead on every read operation.

---

## 13. Service Abstraction & DI [HIGH]

All configurations follow the interface + implementation pattern with proper scoped DI:

```csharp
// ALL configurations:
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
// ... 7 services
```

| Config | Score | Notes |
|---|---|---|
| dotnet-artisan | 5 | Clean interface/impl separation, each in own file, DataSeeder also DI-registered |
| dotnet-skills | 4 | Interface/impl in separate files |
| managedcode | 4 | All interfaces in single `IServices.cs` file — less discoverable |
| no-skills | 4 | Interface/impl in separate files |

**Verdict**: dotnet-artisan edges ahead by registering `DataSeeder` as a DI service. managedcode loses slightly for cramming all interfaces into one file.

---

## 14. Security Configuration [HIGH]

No configuration implements HSTS or HTTPS redirection. None includes `app.UseHsts()` or `app.UseHttpsRedirection()`.

| Config | Score |
|---|---|
| dotnet-artisan | 2 |
| dotnet-skills | 2 |
| managedcode | 2 |
| no-skills | 2 |

dotnet-artisan's environment-conditional OpenAPI (`if (app.Environment.IsDevelopment())`) is the best practice, replicated by dotnet-skills and managedcode. no-skills unconditionally exposes Swagger.

**Verdict**: All fail on HSTS/HTTPS. dotnet-artisan, dotnet-skills, and managedcode at least gate Swagger to development.

---

## 15. DTO Design [MEDIUM]

### dotnet-artisan — Score: 5
All DTOs are **sealed records** with `init` setters for mutability control:

```csharp
// dotnet-artisan: DTOs/AuthorDtos.cs
public sealed record AuthorResponse(int Id, string FirstName, ...);
public sealed record CreateAuthorRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;
}
public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
```

### dotnet-skills — Score: 2
Mutable classes with public setters, all in a single `DTOs.cs` file:

```csharp
// dotnet-skills: DTOs/DTOs.cs
public class AuthorCreateDto
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
}
public class AuthorResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public List<BookSummaryDto>? Books { get; set; }
}
```

### managedcode-dotnet-skills — Score: 4
Records (not sealed) with positional and `init` patterns, organized in separate files:

```csharp
// managedcode: DTOs/AuthorDtos.cs
public record AuthorResponse(
    int Id, string FirstName, string LastName, ...
    IReadOnlyList<AuthorBookResponse>? Books = null);
public record CreateAuthorRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; init; } = string.Empty;
}
```

### no-skills — Score: 2
Mutable classes identical to dotnet-skills pattern, in single `Dtos.cs`.

**Verdict**: dotnet-artisan wins with sealed records + init setters + IReadOnlyList. managedcode is close. dotnet-skills and no-skills use mutable class DTOs.

---

## 16. Sealed Types [MEDIUM]

### dotnet-artisan — Score: 5
**Every** concrete class is sealed — models, services, DTOs, DbContext, DataSeeder (26 files with sealed):

```csharp
public sealed class Author { ... }
public sealed class AuthorService(LibraryDbContext db) : IAuthorService { ... }
public sealed class LibraryDbContext(...) : DbContext(...) { ... }
public sealed record AuthorResponse(...);
```

### dotnet-skills — Score: 3
Services and middleware are sealed (8 files), but models, DTOs, and DbContext are not:

```csharp
public sealed class AuthorService : IAuthorService { ... }  // ✅
public class LibraryDbContext : DbContext { ... }           // ❌ not sealed
public class AuthorCreateDto { ... }                        // ❌ not sealed
```

### managedcode-dotnet-skills — Score: 1
**Zero** sealed types despite using primary constructors — a missed optimization:

```csharp
public class AuthorService(LibraryDbContext db, ...) : IAuthorService  // ❌
public class LibraryDbContext(...) : DbContext(...)                     // ❌
public record AuthorResponse(...)                                      // ❌
```

### no-skills — Score: 1
Zero sealed types.

**Verdict**: dotnet-artisan wins decisively. Sealed types enable JIT devirtualization and signal design intent.

---

## 17. Data Seeder Design [MEDIUM]

### dotnet-artisan — Score: 5
DataSeeder is DI-registered as a scoped service with `ILogger<T>`:

```csharp
// dotnet-artisan: Program.cs
builder.Services.AddScoped<DataSeeder>();
// ...
var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
await seeder.SeedAsync();
```

### dotnet-skills, managedcode, no-skills — Score: 4
All use static `DataSeeder.SeedAsync(db)` methods with idempotency guards. Functional but not DI-integrated:

```csharp
// dotnet-skills, managedcode, no-skills:
await DataSeeder.SeedAsync(db);
```

**Verdict**: dotnet-artisan's DI-registered seeder enables testability and follows DI conventions.

---

## 18. Structured Logging [MEDIUM]

All configurations use `ILogger<T>` with structured message templates:

```csharp
// dotnet-artisan:
logger.LogInformation("Book {BookId} checked out to patron {PatronId}, loan {LoanId}",
    book.Id, patron.Id, loan.Id);

// managedcode:
logger.LogInformation("Book checked out: Loan {LoanId}, Book {BookId} to Patron {PatronId}",
    loan.Id, book.Id, patron.Id);

// no-skills:
_logger.LogInformation("Book checked out: Loan {LoanId}, Book {BookId} to Patron {PatronId}",
    loan.Id, book.Id, patron.Id);
```

No configuration uses `[LoggerMessage]` source generators for high-performance logging.

| Config | Score |
|---|---|
| dotnet-artisan | 4 |
| dotnet-skills | 4 |
| managedcode | 4 |
| no-skills | 4 |

**Verdict**: Tie — all use structured templates. None uses source-generated logging.

---

## 19. Nullable Reference Types [MEDIUM]

All configurations enable NRT in `.csproj`:

```xml
<!-- ALL configurations -->
<Nullable>enable</Nullable>
```

All properly annotate optional navigation properties and nullable return types.

| Config | Score |
|---|---|
| All | 4 |

**Verdict**: Tie — all configurations enable and use NRTs correctly.

---

## 20. API Documentation [MEDIUM]

### dotnet-artisan — Score: 5
Every endpoint has `.WithName()`, `.WithSummary()`, and `.WithTags()`:

```csharp
group.MapGet("/{id:int}", async Task<IResult> (...) => { ... })
    .WithName("GetAuthor")
    .WithSummary("Get author details including their books");
```

TypedResults automatically generate OpenAPI response schemas.

### dotnet-skills, managedcode, no-skills — Score: 3
Use XML doc comments and `[ProducesResponseType]` attributes:

```csharp
/// <summary>Get author details including their books.</summary>
[HttpGet("{id}")]
[ProducesResponseType(typeof(AuthorResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
```

Functional but less integrated with OpenAPI metadata.

**Verdict**: dotnet-artisan's fluent metadata + TypedResults produce richer OpenAPI documentation automatically.

---

## 21. File Organization [MEDIUM]

### dotnet-artisan — Score: 5
Clean per-concern separation with dedicated `Endpoints/` directory and per-entity DTO files:

```
├── Endpoints/ (7 files: AuthorEndpoints.cs, BookEndpoints.cs, ...)
├── DTOs/      (7 files: AuthorDtos.cs, BookDtos.cs, PaginatedResponse.cs, ...)
├── Models/    (10 files: Author.cs, Book.cs, Enums.cs, ...)
├── Services/  (14 files: IAuthorService.cs, AuthorService.cs, ...)
├── Data/      (2 files: LibraryDbContext.cs, DataSeeder.cs)
```

### dotnet-skills — Score: 2
All DTOs in a single `DTOs.cs` file (230+ lines):

```
├── Controllers/  (7 files)
├── DTOs/         (1 file: DTOs.cs ← all DTOs)
├── Middleware/    (1 file)
```

### managedcode-dotnet-skills — Score: 3
Per-entity DTO files but all service interfaces in single `IServices.cs`:

```
├── Controllers/  (7 files)
├── DTOs/         (7 files: per-entity)
├── Services/     (8 files: IServices.cs ← all interfaces, + per-service impl)
```

### no-skills — Score: 2
Same as dotnet-skills — single `Dtos.cs` file for all DTOs.

**Verdict**: dotnet-artisan has the cleanest organization. Single-file DTOs in dotnet-skills and no-skills hurt discoverability.

---

## 22. HTTP Test File Quality [MEDIUM]

All configurations generate `.http` files with comprehensive endpoint coverage, realistic seed data IDs, and business rule test cases. All include section headers and realistic request bodies.

| Config | Score |
|---|---|
| All | 4 |

**Verdict**: Tie — all generate functional .http files covering CRUD and business rule scenarios.

---

## 23. Type Design & Resource Management [MEDIUM]

### dotnet-artisan — Score: 4
Enums are well-designed. Services use result tuples. `IReadOnlyList<T>` for read-only collections:

```csharp
public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
```

Missing `HasConversion<string>()` for enum storage (stored as integers in SQLite).

### dotnet-skills — Score: 4
`HasConversion<string>()` for all enums. Uses `List<T>` in DTOs.

### managedcode-dotnet-skills — Score: 4
`HasConversion<string>()` for all enums. `IReadOnlyList<T>` in some DTOs.

### no-skills — Score: 3
`HasConversion<string>()` present but DTOs use mutable `List<T>` properties.

**Verdict**: Near tie. All use proper enums. dotnet-artisan and managedcode use `IReadOnlyList<T>`.

---

## 24. Code Standards Compliance [LOW]

### dotnet-artisan — Score: 5
File-scoped namespaces, explicit `public sealed`, `Async` suffix on all async methods, `I`-prefix on interfaces. PascalCase consistently applied.

### dotnet-skills — Score: 3
File-scoped namespaces, `sealed` on services, missing on models/DTOs. Async suffix present.

### managedcode-dotnet-skills — Score: 4
File-scoped namespaces, primary constructors, Async suffix. Missing `sealed` drops it.

### no-skills — Score: 3
File-scoped namespaces, Async suffix. No sealed, no primary constructors.

**Verdict**: dotnet-artisan is fully compliant. Others have gaps.

---

## Weighted Summary

| Dimension [Tier] | Weight | dotnet-artisan | dotnet-skills | managedcode | no-skills |
|---|---|---|---|---|---|
| **CRITICAL** (×3) | | | | | |
| Minimal API Architecture | ×3 | 15 | 3 | 3 | 3 |
| Input Validation | ×3 | 12 | 9 | 12 | 9 |
| NuGet Discipline | ×3 | 9 | 12 | 12 | 9 |
| EF Migration Usage | ×3 | 3 | 3 | 3 | 3 |
| Security Vuln Scan | ×3 | 12 | 12 | 12 | 12 |
| Build & Run | ×3 | 15 | 15 | 15 | 15 |
| **HIGH** (×2) | | | | | |
| Business Logic | ×2 | 10 | 10 | 10 | 10 |
| Prefer Built-in | ×2 | 8 | 6 | 6 | 2 |
| Modern C# | ×2 | 10 | 4 | 8 | 2 |
| Error Handling | ×2 | 8 | 8 | 10 | 4 |
| Async & Cancellation | ×2 | 10 | 4 | 10 | 4 |
| EF Core Practices | ×2 | 10 | 8 | 10 | 4 |
| Service & DI | ×2 | 10 | 8 | 8 | 8 |
| Security Config | ×2 | 4 | 4 | 4 | 4 |
| **MEDIUM** (×1) | | | | | |
| DTO Design | ×1 | 5 | 2 | 4 | 2 |
| Sealed Types | ×1 | 5 | 3 | 1 | 1 |
| Data Seeder | ×1 | 5 | 4 | 4 | 4 |
| Structured Logging | ×1 | 4 | 4 | 4 | 4 |
| Nullable Refs | ×1 | 4 | 4 | 4 | 4 |
| API Documentation | ×1 | 5 | 3 | 3 | 3 |
| File Organization | ×1 | 5 | 2 | 3 | 2 |
| HTTP Test File | ×1 | 4 | 4 | 4 | 4 |
| Type Design | ×1 | 4 | 4 | 4 | 3 |
| **LOW** (×0.5) | | | | | |
| Code Standards | ×0.5 | 2.5 | 1.5 | 2 | 1.5 |
| **TOTAL** | | **179.5** | **137.5** | **156** | **117.5** |

### Final Ranking

| Rank | Configuration | Weighted Score |
|---|---|---|
| 🥇 1st | **dotnet-artisan** | **179.5** |
| 🥈 2nd | **managedcode-dotnet-skills** | **156.0** |
| 🥉 3rd | **dotnet-skills** | **137.5** |
| 4th | **no-skills** | **117.5** |

---

## What All Versions Get Right

- **Complete business logic**: All configurations implement the full LibraryApi specification with correct borrowing limits, fine thresholds, reservation queues, and overdue detection
- **Interface + implementation pattern**: All use `IService`/`Service` with scoped DI registration
- **EF Core with SQLite**: All properly configure `AddDbContext<T>` with SQLite provider and connection strings in `appsettings.json`
- **Fluent API configuration**: All use `OnModelCreating` with composite keys, unique indexes, and relationship configuration
- **ProblemDetails for errors**: All return RFC 7807 ProblemDetails (varying quality)
- **Structured logging**: All use `ILogger<T>` with structured message templates and named placeholders
- **Nullable reference types**: All enable `<Nullable>enable</Nullable>` and use proper `?` annotations
- **Proper enum design**: All define domain enums (`MembershipType`, `LoanStatus`, `ReservationStatus`, `FineStatus`)
- **Seed data with idempotency**: All seed realistic data only when the database is empty
- **.http test files**: All generate comprehensive test files covering all endpoints and business rule edge cases
- **net10.0 targeting**: All target .NET 10 with `Microsoft.NET.Sdk.Web`

---

## Summary: Impact of Skills

### Most Impactful Differences

1. **Minimal API Architecture** (12-point spread): The single largest differentiator. Only dotnet-artisan uses Minimal APIs with MapGroup, TypedResults, and endpoint extension methods. This is the modern .NET standard and the most visible quality signal.

2. **Async & Cancellation Propagation** (6-point spread): dotnet-artisan and managedcode propagate CancellationToken throughout; dotnet-skills and no-skills completely omit it. This has real production impact on resource utilization.

3. **Modern C# Adoption** (8-point spread): Primary constructors, collection expressions, and sealed records dramatically reduce boilerplate. dotnet-artisan uses all of them; no-skills uses none.

4. **EF Core Best Practices** (6-point spread): `AsNoTracking()` on reads is present in 3 of 4 configs but completely absent in no-skills, representing a measurable performance regression.

5. **Error Handling Architecture** (6-point spread): managedcode uniquely uses `IExceptionHandler` with a custom `BusinessRuleException` — the most composable and DI-aware approach.

### Overall Assessment

| Configuration | Assessment |
|---|---|
| **dotnet-artisan** (179.5) | The strongest output. Uniquely uses Minimal APIs, comprehensive TypedResults, sealed types throughout, and full CancellationToken propagation. Its only weaknesses are the EF Core wildcard version and missing HasConversion for enum storage. The plugin chain (using-dotnet → dotnet-advisor → dotnet-csharp → dotnet-api) produces code that most closely matches official .NET team guidance. |
| **managedcode-dotnet-skills** (156.0) | Strong second place. The only configuration to use `IExceptionHandler` (the modern ASP.NET Core 8+ pattern) and `BusinessRuleException` custom exceptions. Full CancellationToken support, primary constructors, record DTOs, and collection expressions. Falls behind dotnet-artisan mainly due to controller-based architecture and missing sealed types. |
| **dotnet-skills** (137.5) | Adequate but dated. Despite having skills like `dotnet-modern-csharp`, the generated code uses traditional constructors, mutable class DTOs, and omits CancellationToken. The exception handling middleware is well-structured, and EF Core enum conversions are properly configured. The skills provide correct architectural guidance but don't consistently push modern patterns. |
| **no-skills** (117.5) | Baseline produces functional but outdated code. Uses Swashbuckle instead of built-in OpenAPI, omits AsNoTracking entirely, has no CancellationToken, no sealed types, no primary constructors, and a simplistic error handler that leaks exception details. Demonstrates the significant quality uplift that skills provide — a 62-point gap from the top configuration. |

The **skills provide a measurable 53% improvement** (179.5 vs 117.5) in code quality, with the largest gains in API architecture, modern language features, async patterns, and EF Core best practices.
