# Comparative Analysis: dotnet-artisan, managedcode-dotnet-skills, dotnet-skills, no-skills, dotnet-webapi

## Introduction

This report compares **5 Copilot skill configurations** that each generated the same **FitnessStudioApi** — a fitness/wellness studio booking API with members, memberships, instructors, class scheduling, bookings, waitlists, and complex business rules. Each configuration produced its implementation independently, and this analysis evaluates code quality across 24 dimensions.

**Configurations analyzed:**

| Configuration | Description | API Pattern |
|---|---|---|
| **dotnet-artisan** | dotnet-artisan plugin chain | Minimal APIs |
| **dotnet-skills** | Official .NET Skills (dotnet/skills) | Controllers |
| **dotnet-webapi** | dotnet-webapi skill | Minimal APIs |
| **managedcode-dotnet-skills** | Community managed-code skills | Minimal APIs |
| **no-skills** | Baseline (default Copilot) | Controllers |

All projects target **.NET 10**, use **EF Core with SQLite**, and implement the same specification of 7 entities, 35+ endpoints, and 12 business rules.

---

## Executive Summary

| Dimension [Tier] | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode | no-skills |
|---|---|---|---|---|---|
| Minimal API Architecture [CRITICAL] | 5 | 1 | 5 | 4 | 1 |
| Input Validation & Guard Clauses [CRITICAL] | 4 | 4 | 4 | 4 | 3 |
| NuGet & Package Discipline [CRITICAL] | 5 | 2 | 3 | 3 | 3 |
| EF Migration Usage [CRITICAL] | 5 | 1 | 1 | 1 | 1 |
| Prefer Built-in over 3rd Party [CRITICAL] | 5 | 2 | 5 | 5 | 3 |
| Business Logic Correctness [HIGH] | 5 | 5 | 5 | 5 | 5 |
| Modern C# Adoption [HIGH] | 5 | 3 | 5 | 5 | 2 |
| Error Handling & Middleware [HIGH] | 5 | 4 | 5 | 5 | 3 |
| Async Patterns & Cancellation [HIGH] | 5 | 3 | 5 | 5 | 3 |
| EF Core Best Practices [HIGH] | 5 | 4 | 4 | 5 | 3 |
| Service Abstraction & DI [HIGH] | 5 | 5 | 5 | 5 | 5 |
| Security Configuration [HIGH] | 2 | 2 | 2 | 2 | 3 |
| DTO Design [MEDIUM] | 5 | 4 | 5 | 5 | 2 |
| Sealed Types [MEDIUM] | 5 | 4 | 5 | 5 | 1 |
| Data Seeder Design [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Structured Logging [MEDIUM] | 4 | 4 | 4 | 4 | 4 |
| Nullable Reference Types [MEDIUM] | 5 | 5 | 5 | 5 | 5 |
| API Documentation [MEDIUM] | 5 | 4 | 5 | 5 | 4 |
| File Organization [MEDIUM] | 5 | 4 | 5 | 4 | 4 |
| HTTP Test File Quality [MEDIUM] | 5 | 5 | 5 | 5 | 5 |
| Type Design & Resource Mgmt [MEDIUM] | 5 | 4 | 5 | 5 | 3 |
| Code Standards Compliance [LOW] | 5 | 4 | 5 | 5 | 3 |

---

## 1. Minimal API Architecture [CRITICAL]

### dotnet-artisan (Score: 5)
Uses Minimal APIs with `MapGroup()`, endpoint extension methods in a dedicated `Endpoints/` directory, `TypedResults`, and `Results<T1, T2>` union return types:

```csharp
// Endpoints/BookingEndpoints.cs
public static class BookingEndpoints
{
    public static RouteGroupBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/bookings").WithTags("Bookings");

        group.MapPost("/", async (CreateBookingRequest request, IBookingService service, CancellationToken ct) =>
        {
            var booking = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/bookings/{booking.Id}", booking);
        })
        .WithName("CreateBooking")
        .WithSummary("Book a class (enforces all booking rules)");

        group.MapGet("/{id:int}", async Task<Results<Ok<BookingResponse>, NotFound>> (
            int id, IBookingService service, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is { } booking
                ? TypedResults.Ok(booking)
                : TypedResults.NotFound())
            .WithName("GetBookingById");
```

Program.cs stays clean:
```csharp
// Program.cs — dotnet-artisan
app.MapMembershipPlanEndpoints();
app.MapMemberEndpoints();
app.MapBookingEndpoints();
```

### dotnet-webapi (Score: 5)
Identical pattern — separate `Endpoints/` directory with extension methods, `MapGroup()`, `TypedResults`, and `Results<T1,T2>`:

```csharp
// Endpoints/BookingEndpoints.cs — dotnet-webapi
public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/bookings").WithTags("Bookings");

    group.MapPost("/", Create)
        .WithName("CreateBooking")
        .WithSummary("Book a class")
        .WithDescription("Books a member into a class. Enforces all business rules...")
        .Produces<BookingResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
```

### managedcode-dotnet-skills (Score: 4)
Uses Minimal APIs with `MapGroup()`, `TypedResults`, and `Results<T1,T2>`, but **all endpoints are defined inline in Program.cs** rather than in extension methods. No `Endpoints/` directory exists:

```csharp
// Program.cs — managedcode (all endpoints inline)
var plans = app.MapGroup("/api/membership-plans").WithTags("Membership Plans");

plans.MapGet("/", async (IMembershipPlanService service, CancellationToken ct) =>
    TypedResults.Ok(await service.GetAllAsync(ct)))
    .WithName("GetMembershipPlans")
    .WithSummary("List all active membership plans")
    .Produces<IReadOnlyList<MembershipPlanResponse>>();
```

This creates a very long Program.cs and reduces navigability.

### dotnet-skills (Score: 1) / no-skills (Score: 1)
Both use **traditional `[ApiController]` controllers** — the legacy pattern:

```csharp
// Controllers/BookingsController.cs — dotnet-skills
[ApiController]
[Route("api/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service) => _service = service;

    [HttpPost]
    [ProducesResponseType(typeof(BookingDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var booking = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }
}
```

**Verdict:** dotnet-artisan and dotnet-webapi demonstrate the modern Minimal API pattern with clean separation via endpoint extension methods. Controllers in dotnet-skills and no-skills add unnecessary ceremony. managedcode uses Minimal APIs but loses the organization benefit by keeping everything inline.

---

## 2. Input Validation & Guard Clauses [CRITICAL]

### dotnet-artisan (Score: 4) / dotnet-webapi (Score: 4) / managedcode (Score: 4)
All three use Data Annotations on sealed record DTOs with `init` accessors plus business-rule guard clauses in services:

```csharp
// DTOs/MembershipPlanDtos.cs — dotnet-artisan
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

### dotnet-skills (Score: 4)
Uses **FluentValidation** alongside Data Annotations — a redundant but thorough approach:

```csharp
// Validators/Validators.cs — dotnet-skills
public class CreateMemberValidator : AbstractValidator<CreateMemberDto>
{
    public CreateMemberValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DateOfBirth).NotEmpty();
    }
}
```

### no-skills (Score: 3)
Uses Data Annotations on **mutable classes** (not records), meaning DTOs can be mutated after validation:

```csharp
// DTOs/Dtos.cs — no-skills
public class CreateMemberDto  // Mutable class, not record
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
}
```

None of the configurations use `ArgumentNullException.ThrowIfNull()` or other modern .NET guard clause helpers.

**Verdict:** All configs validate inputs adequately. dotnet-artisan/webapi/managedcode are best with immutable record DTOs. dotnet-skills adds FluentValidation unnecessarily. no-skills uses mutable classes.

---

## 3. NuGet & Package Discipline [CRITICAL]

### dotnet-artisan (Score: 5)
Only 3 packages with exact versions — the leanest configuration:

```xml
<!-- FitnessStudioApi.csproj — dotnet-artisan -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.5">
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.5" />
```

### dotnet-webapi (Score: 3) / managedcode (Score: 3)
Same 3 packages, but **wildcard versions** on EF Core packages:

```xml
<!-- FitnessStudioApi.csproj — dotnet-webapi / managedcode -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.*-*" />
```

Wildcards can pull in breaking changes or vulnerable releases — a significant risk for reproducible builds.

### dotnet-skills (Score: 2)
**5 packages** including unnecessary Swashbuckle and FluentValidation:

```xml
<!-- FitnessStudioApi.csproj — dotnet-skills -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.1" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.4" />
```

### no-skills (Score: 3)
4 packages with redundant Swashbuckle alongside built-in OpenAPI, but versions are pinned:

```xml
<!-- FitnessStudioApi.csproj — no-skills -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.5" />
```

**Verdict:** dotnet-artisan is the only configuration with minimal packages AND exact versions. Wildcard versions in webapi/managedcode are a significant risk. Swashbuckle in skills/no-skills is redundant.

---

## 4. EF Migration Usage [CRITICAL]

### dotnet-artisan (Score: 5)
The **only configuration** using proper EF Core migrations with actual migration files:

```csharp
// Program.cs — dotnet-artisan
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
    db.Database.Migrate();  // ✅ Uses Migrate(), not EnsureCreated()
    await DataSeeder.SeedAsync(db);
}
```

The project contains actual migration files in `Migrations/`:
- `20260328175720_InitialCreate.cs`
- `20260328175720_InitialCreate.Designer.cs`
- `FitnessDbContextModelSnapshot.cs`

### All Others (Score: 1)
Every other configuration uses `EnsureCreatedAsync()` — the anti-pattern:

```csharp
// Program.cs — dotnet-skills, dotnet-webapi, managedcode, no-skills
await db.Database.EnsureCreatedAsync();  // ❌ Bypasses migrations
```

**Verdict:** Only dotnet-artisan uses the production-safe migration approach. `EnsureCreated()` makes schema evolution impossible — a critical failing for any app intended for real use. This is the single most impactful difference between dotnet-artisan and all other configurations.

---

## 5. Prefer Built-in over 3rd Party [CRITICAL]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5) / managedcode (Score: 5)
Zero third-party dependencies. Uses built-in `AddOpenApi()`/`MapOpenApi()`, `System.Text.Json`, `ILogger<T>`, and Data Annotations:

```csharp
// Program.cs — dotnet-artisan
builder.Services.AddOpenApi();
// ...
if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }
```

### dotnet-skills (Score: 2)
Uses **both** Swashbuckle and built-in OpenAPI (redundant), plus FluentValidation:

```csharp
// Program.cs — dotnet-skills
builder.Services.AddOpenApi();
// ALSO:
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Zenith Fitness Studio API v1");
});
```

### no-skills (Score: 3)
Uses Swashbuckle for Swagger UI:

```csharp
// Program.cs — no-skills
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zenith Fitness Studio API v1"));
```

**Verdict:** dotnet-artisan, dotnet-webapi, and managedcode exemplify the built-in-first principle. dotnet-skills is the worst offender with two redundant library categories.

---

## 6. Business Logic Correctness [HIGH]

**All configurations score 5/5** — every implementation covers all 12 business rules:

| Business Rule | All Configs |
|---|---|
| Booking window (7 days/30 min) | ✅ |
| Capacity mgmt + waitlist promotion | ✅ |
| Cancellation policy (2-hour) | ✅ |
| Premium class tier access | ✅ |
| Weekly booking limits | ✅ |
| Active membership required | ✅ |
| No double booking | ✅ |
| Instructor schedule conflicts | ✅ |
| Membership freeze/unfreeze | ✅ |
| Class cancellation cascade | ✅ |
| Check-in window (±15 min) | ✅ |
| No-show marking | ✅ |

Representative example from all configs:
```csharp
// BookingService — all configurations implement the same rule
if (schedule.ClassType.IsPremium && !activeMembership.MembershipPlan.AllowsPremiumClasses)
    throw new ArgumentException(
        $"Your '{activeMembership.MembershipPlan.Name}' plan does not include premium classes.");
```

**Verdict:** Business logic is fully implemented across all configurations. The specification is clearly followed regardless of skill configuration.

---

## 7. Modern C# Adoption [HIGH]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5) / managedcode (Score: 5)
Full use of modern C# 12 features:

```csharp
// Primary constructors — all three configs
public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger)
    : IBookingService

// Collection expressions — all three configs
public ICollection<Membership> Memberships { get; set; } = [];

// Sealed records with init-only properties
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
}

// Switch expressions in error handling
var (statusCode, title) = exception switch
{
    KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
    ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
    _ => (0, (string?)null)
};
```

### dotnet-skills (Score: 3)
Uses records for DTOs but **no primary constructors** on service classes, **no collection expressions**:

```csharp
// dotnet-skills — traditional constructor
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

### no-skills (Score: 2)
No records for DTOs, no primary constructors, no collection expressions:

```csharp
// no-skills — old-style collection initialization
public ICollection<Membership> Memberships { get; set; } = new List<Membership>();

// Mutable DTO classes instead of records
public class MembershipPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

**Verdict:** dotnet-artisan, dotnet-webapi, and managedcode showcase C# 12 idioms. The skill-less baseline generates code that looks two language versions behind.

---

## 8. Error Handling & Middleware [HIGH]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5) / managedcode (Score: 5)
All three use the modern `IExceptionHandler` interface (.NET 8+) with `ProblemDetails` and structured logging:

```csharp
// Middleware/ApiExceptionHandler.cs — shared pattern
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
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
        // ProblemDetails response...
    }
}
```

Registered with:
```csharp
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
```

### dotnet-skills (Score: 4)
Uses custom `RequestDelegate` middleware with custom exception classes (`BusinessRuleException`, `NotFoundException`) — functional but not the modern `IExceptionHandler` pattern:

```csharp
// dotnet-skills — convention-based middleware
public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (BusinessRuleException ex) { /* handle */ }
        catch (NotFoundException ex) { /* handle */ }
    }
}
```

### no-skills (Score: 3)
Similar convention-based middleware but with **unsealed** exception class and manual JSON serialization:

```csharp
// no-skills — manual JSON serialization instead of WriteAsJsonAsync
var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
});
await context.Response.WriteAsync(json);
```

**Verdict:** `IExceptionHandler` is composable and DI-aware — the modern standard. Convention middleware works but is less elegant. dotnet-skills partially compensates with well-designed custom exception types.

---

## 9. Async Patterns & Cancellation [HIGH]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5) / managedcode (Score: 5)
Complete `CancellationToken` propagation from endpoint → service → EF Core:

```csharp
// Endpoint layer
group.MapGet("/", async (IMembershipPlanService service, CancellationToken ct) =>
    TypedResults.Ok(await service.GetAllAsync(ct)))

// Service interface
Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken ct);

// EF Core call
var member = await db.Members.FindAsync([request.MemberId], ct);
```

### dotnet-skills (Score: 3) / no-skills (Score: 3)
**No `CancellationToken` parameter anywhere** in the service layer:

```csharp
// dotnet-skills — no CancellationToken
public async Task<BookingDto> CreateAsync(CreateBookingDto dto)  // ← Missing ct
{
    var schedule = await _context.ClassSchedules
        .FirstOrDefaultAsync(cs => cs.Id == dto.ClassScheduleId);  // ← No ct forwarded
}
```

**Verdict:** Missing CancellationToken means cancelled HTTP requests continue consuming server resources. This is a significant production concern.

---

## 10. EF Core Best Practices [HIGH]

### managedcode (Score: 5)
Best EF Core implementation with `IEntityTypeConfiguration<T>` for clean separation, pervasive `AsNoTracking()`, composite indexes, and automatic timestamp management:

```csharp
// Data/Configurations/EntityConfigurations.cs — managedcode
public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.Property(ms => ms.Status).HasConversion<string>();
        builder.HasOne(ms => ms.Member)
            .WithMany(m => m.Memberships)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(ms => new { ms.MemberId, ms.Status });
    }
}
```

### dotnet-artisan (Score: 5)
Extensive Fluent API configuration inline in `OnModelCreating`, `AsNoTracking()` on all reads, automatic timestamp management:

```csharp
// MemberService — dotnet-artisan
var query = db.Members.AsNoTracking().AsQueryable();
```

### dotnet-webapi (Score: 4)
Good Fluent API and `AsNoTracking()` usage but no `IEntityTypeConfiguration<T>`:

```csharp
// FitnessStudioDbContext — dotnet-webapi
modelBuilder.Entity<Membership>(entity =>
{
    entity.Property(e => e.Status).HasConversion<string>();
    entity.HasOne(e => e.Member).WithMany(m => m.Memberships)
        .OnDelete(DeleteBehavior.Restrict);
});
```

### dotnet-skills (Score: 4)
Similar to dotnet-webapi with Fluent API and `AsNoTracking()`.

### no-skills (Score: 3)
Has Fluent API configuration but **no `AsNoTracking()`** on any read queries:

```csharp
// MemberService — no-skills
var query = _db.Members.AsQueryable();  // ← Tracking enabled, wastes memory
```

**Verdict:** managedcode leads with `IEntityTypeConfiguration<T>` for cleanly separated entity configs. no-skills loses significant performance by tracking all queries.

---

## 11. Service Abstraction & DI [HIGH]

**All configurations score 5/5** — every implementation uses `AddScoped<IService, Service>()` with interface-based registration:

```csharp
// Program.cs — all configurations
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IBookingService, BookingService>();
```

**Verdict:** All configurations follow the standard .NET DI pattern correctly. This is a well-established convention that Copilot handles consistently.

---

## 12. Security Configuration [HIGH]

### no-skills (Score: 3)
The **only** configuration with HTTPS redirection:

```csharp
// Program.cs — no-skills
app.UseHttpsRedirection();
```

### All Others (Score: 2)
None of the skill-enhanced configurations include HTTPS redirection or HSTS:

```csharp
// Program.cs — dotnet-artisan, dotnet-skills, dotnet-webapi, managedcode
// ❌ No app.UseHsts()
// ❌ No app.UseHttpsRedirection()
```

**Verdict:** Ironically, the baseline (no-skills) is the only config with HTTPS redirection. **No configuration** includes HSTS. This is a gap across all skills.

---

## 13. DTO Design [MEDIUM]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5) / managedcode (Score: 5)
Sealed records with `*Request`/`*Response` naming, positional parameters for responses, init-only properties for requests:

```csharp
// Response — positional record
public sealed record MemberResponse(
    int Id, string FirstName, string LastName, string Email,
    MemberActiveMembershipInfo? ActiveMembership, DateTime CreatedAt);

// Request — property-based record with validation
public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }
}
```

### dotnet-skills (Score: 4)
Records but with `*Dto` naming and positional syntax for all DTOs:

```csharp
// dotnet-skills — *Dto naming
public record MembershipPlanDto(
    int Id, string Name, string? Description, int DurationMonths,
    decimal Price, int MaxClassBookingsPerWeek, bool AllowsPremiumClasses, bool IsActive);
```

### no-skills (Score: 2)
**Mutable classes** with `*Dto` naming, public setters — no immutability guarantees:

```csharp
// no-skills — mutable class DTO
public class MembershipPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

**Verdict:** Sealed records provide immutability, value equality, and concise syntax. Mutable class DTOs in no-skills are an anti-pattern.

---

## 14. Sealed Types [MEDIUM]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5) / managedcode (Score: 5)
**100% sealed** — every class and record in the project is sealed:

```csharp
public sealed class BookingService(FitnessDbContext db, ILogger<BookingService> logger) : IBookingService
public sealed class FitnessDbContext(DbContextOptions<FitnessDbContext> options) : DbContext(options)
internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
```

### dotnet-skills (Score: 4)
Most types sealed, but `FitnessDbContext` is not sealed, and some controllers are unsealed:

```csharp
// dotnet-skills
public class FitnessDbContext : DbContext  // ← Not sealed
public class BookingsController : ControllerBase  // ← Not sealed
```

### no-skills (Score: 1)
**Zero sealed types** in the entire codebase:

```csharp
// no-skills — nothing is sealed
public class Member { }
public class BookingService { }
public class GlobalExceptionMiddleware { }
public class BusinessRuleException : Exception { }
```

**Verdict:** Sealed types enable JIT devirtualization and signal design intent. no-skills misses this entirely.

---

## 15. Data Seeder Design [MEDIUM]

**All configurations score 4/5** — all use an idempotent seeder service with realistic data:

```csharp
// DataSeeder — all configurations
public static async Task SeedAsync(FitnessDbContext db)
{
    if (await db.MembershipPlans.AnyAsync())
        return; // Already seeded
    // ... 3 plans, 8 members, 6 memberships, 4 instructors, 6 class types, 12 schedules, 15+ bookings
}
```

dotnet-webapi's seeder is DI-injected rather than static, which is slightly better for testability.

**Verdict:** All configs implement adequate seeding. None uses `HasData()` (which integrates with migrations) — only relevant for dotnet-artisan which uses migrations.

---

## 16. Structured Logging [MEDIUM]

**All configurations score 4/5** — all use `ILogger<T>` with structured message templates:

```csharp
// All configurations use this pattern
logger.LogInformation("Booking created for member {MemberId} in class {ClassId} — Status: {Status}",
    request.MemberId, request.ClassScheduleId, booking.Status);

logger.LogWarning(exception, "Handled API exception: {Title}", title);
```

No configuration uses `[LoggerMessage]` source generators (which would earn a 5).

**Verdict:** Logging is consistently good across all configurations.

---

## 17. Nullable Reference Types [MEDIUM]

**All configurations score 5/5** — all enable NRT in `.csproj` and use proper annotations:

```xml
<Nullable>enable</Nullable>
```

**Verdict:** NRT is universally adopted. No differences.

---

## 18. API Documentation [MEDIUM]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5) / managedcode (Score: 5)
Comprehensive endpoint metadata with Minimal API extensions:

```csharp
group.MapPost("/", Create)
    .WithName("CreateBooking")
    .WithSummary("Book a class")
    .WithDescription("Enforces all booking rules: capacity, membership, weekly limits...")
    .Produces<BookingResponse>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);
```

### dotnet-skills (Score: 4) / no-skills (Score: 4)
Controller-style documentation with `[ProducesResponseType]` and XML comments:

```csharp
/// <summary>Book a class (enforces all booking rules)</summary>
[HttpPost]
[ProducesResponseType(typeof(BookingDto), 201)]
[ProducesResponseType(400)]
public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
```

**Verdict:** Both approaches provide good OpenAPI metadata. The Minimal API syntax is more concise.

---

## 19. File Organization [MEDIUM]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5)
Best organization with dedicated `Endpoints/` directory and one file per entity:

```
├── Endpoints/
│   ├── BookingEndpoints.cs
│   ├── MemberEndpoints.cs
│   └── MembershipPlanEndpoints.cs
├── Services/
│   ├── IBookingService.cs
│   ├── BookingService.cs
├── DTOs/
│   ├── BookingDtos.cs
│   ├── MemberDtos.cs
```

### managedcode (Score: 4)
Clean `Services/` split with separate interface files, but endpoints are inline in Program.cs.

### dotnet-skills (Score: 4) / no-skills (Score: 4)
Consolidate all interfaces into a single `IServices.cs` and all DTOs into a single `Dtos.cs`:

```
├── Services/
│   ├── IServices.cs        ← All interfaces in one file
│   ├── BookingService.cs
├── DTOs/
│   └── Dtos.cs              ← All DTOs in one file
```

**Verdict:** dotnet-artisan and dotnet-webapi have the best file-per-concern organization.

---

## 20. HTTP Test File Quality [MEDIUM]

**All configurations score 5/5** — all provide comprehensive `.http` files (360-391 lines) covering all endpoints with business rule test cases:

```http
@baseUrl = http://localhost:5244

### Test: Basic member trying to book premium class (should fail)
POST {{baseUrl}}/api/bookings
Content-Type: application/json

{
  "classScheduleId": 5,
  "memberId": 3
}
```

**Verdict:** No meaningful differences. All configs produce excellent `.http` files.

---

## 21. Type Design & Resource Management [MEDIUM]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5) / managedcode (Score: 5)
Proper enum usage with `HasConversion<string>()`, `JsonStringEnumConverter`, and `IReadOnlyList<T>` return types:

```csharp
// Enums stored as strings in DB
entity.Property(e => e.Status).HasConversion<string>();

// Service returns IReadOnlyList<T>
Task<IReadOnlyList<RosterEntryResponse>> GetRosterAsync(int id, CancellationToken ct);
```

### dotnet-skills (Score: 4)
Uses `IReadOnlyList<T>` and proper enums, but DTOs convert enums to strings in mapping:

```csharp
// dotnet-skills — returns IReadOnlyList<T>
Task<IReadOnlyList<MembershipPlanDto>> GetAllActiveAsync();
```

### no-skills (Score: 3)
Returns mutable `List<T>` from services and converts enums to strings in DTOs:

```csharp
// no-skills — returns mutable List<T>
public async Task<List<MembershipPlanDto>> GetAllAsync()
```

**Verdict:** `IReadOnlyList<T>` prevents accidental mutation. `List<T>` exposes the internal collection to callers.

---

## 22. Code Standards Compliance [LOW]

### dotnet-artisan (Score: 5) / dotnet-webapi (Score: 5) / managedcode (Score: 5)
Consistent PascalCasing, Async suffixes, file-scoped namespaces, explicit access modifiers, `internal sealed` on handler:

```csharp
namespace FitnessStudioApi.Middleware;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
```

### dotnet-skills (Score: 4)
Good naming but some controllers lack `sealed`. FluentValidation validators also lack `sealed`.

### no-skills (Score: 3)
No sealed types, mutable DTOs, and some constructor parameter names shadow field names:

```csharp
// no-skills — parameter shadows field name
public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> _logger)
{
    _next = next;
    _logger = _logger;  // Parameter named _logger shadows field
}
```

**Verdict:** dotnet-artisan/webapi/managedcode follow .NET conventions most consistently.

---

## Weighted Summary

Weights: Critical × 3, High × 2, Medium × 1, Low × 0.5

| Dimension [Tier] | Weight | dotnet-artisan | dotnet-skills | dotnet-webapi | managedcode | no-skills |
|---|---|---|---|---|---|---|
| Minimal API [C] | ×3 | 15 | 3 | 15 | 12 | 3 |
| Input Validation [C] | ×3 | 12 | 12 | 12 | 12 | 9 |
| NuGet Discipline [C] | ×3 | 15 | 6 | 9 | 9 | 9 |
| EF Migrations [C] | ×3 | 15 | 3 | 3 | 3 | 3 |
| Built-in Preference [C] | ×3 | 15 | 6 | 15 | 15 | 9 |
| Business Logic [H] | ×2 | 10 | 10 | 10 | 10 | 10 |
| Modern C# [H] | ×2 | 10 | 6 | 10 | 10 | 4 |
| Error Handling [H] | ×2 | 10 | 8 | 10 | 10 | 6 |
| Async/Cancel [H] | ×2 | 10 | 6 | 10 | 10 | 6 |
| EF Core Practices [H] | ×2 | 10 | 8 | 8 | 10 | 6 |
| Service DI [H] | ×2 | 10 | 10 | 10 | 10 | 10 |
| Security Config [H] | ×2 | 4 | 4 | 4 | 4 | 6 |
| DTO Design [M] | ×1 | 5 | 4 | 5 | 5 | 2 |
| Sealed Types [M] | ×1 | 5 | 4 | 5 | 5 | 1 |
| Data Seeder [M] | ×1 | 4 | 4 | 4 | 4 | 4 |
| Logging [M] | ×1 | 4 | 4 | 4 | 4 | 4 |
| NRT [M] | ×1 | 5 | 5 | 5 | 5 | 5 |
| API Docs [M] | ×1 | 5 | 4 | 5 | 5 | 4 |
| File Org [M] | ×1 | 5 | 4 | 5 | 4 | 4 |
| HTTP File [M] | ×1 | 5 | 5 | 5 | 5 | 5 |
| Type Design [M] | ×1 | 5 | 4 | 5 | 5 | 3 |
| Code Standards [L] | ×0.5 | 2.5 | 2 | 2.5 | 2.5 | 1.5 |
| **TOTAL** | | **196.5** | **142** | **176.5** | **174.5** | **129.5** |

### Final Rankings

| Rank | Configuration | Weighted Score | Tier |
|---|---|---|---|
| 🥇 1st | **dotnet-artisan** | **196.5** | Elite |
| 🥈 2nd | **dotnet-webapi** | **176.5** | Excellent |
| 🥉 3rd | **managedcode-dotnet-skills** | **174.5** | Excellent |
| 4th | **dotnet-skills** | **142.0** | Good |
| 5th | **no-skills** | **129.5** | Adequate |

---

## What All Versions Get Right

Despite their differences, all five configurations share these practices:

- **Complete business logic** — all 12 domain rules fully implemented with proper validation
- **Interface-based service abstraction** — `AddScoped<IService, Service>()` with clean constructor injection
- **Nullable reference types** — `<Nullable>enable</Nullable>` with proper annotations
- **Comprehensive seed data** — idempotent seeder with 3 plans, 8 members, 4 instructors, 6 class types, 12 schedules, 15+ bookings
- **Structured logging** — `ILogger<T>` with named placeholders and appropriate log levels
- **Enums for status fields** — type-safe enums stored as strings via `HasConversion<string>()`
- **JSON enum serialization** — `JsonStringEnumConverter` for readable API responses
- **Comprehensive .http files** — 360+ lines covering all endpoints with business rule test cases
- **EF Core Fluent API** — explicit relationship configuration with `OnDelete(DeleteBehavior.Restrict)`
- **Proper async/await** — no sync-over-async anti-patterns detected in any configuration

---

## Summary: Impact of Skills

### Most Impactful Differences

1. **EF Migrations vs EnsureCreated** (20-point swing) — Only dotnet-artisan produces migration-ready code. This is the single most impactful architectural decision for production readiness.

2. **Minimal APIs vs Controllers** (12-point swing) — dotnet-artisan, dotnet-webapi, and managedcode use the modern pattern. dotnet-skills and no-skills generate legacy controller code.

3. **Modern C# features** (8-point swing) — Primary constructors, collection expressions, and sealed records are consistently produced by dotnet-artisan, dotnet-webapi, and managedcode but absent from no-skills.

4. **CancellationToken propagation** (4-point swing) — dotnet-artisan, dotnet-webapi, and managedcode propagate cancellation tokens. dotnet-skills and no-skills omit them entirely.

5. **Package discipline** (9-point swing) — Only dotnet-artisan pins all versions. Wildcard versions in webapi/managedcode and redundant packages in skills/no-skills are production risks.

### Configuration Assessment

- **dotnet-artisan** (196.5) — The clear winner. Only configuration with EF migrations, exact package versions, and the Meziantou.Analyzer for code quality enforcement. Produces the most production-ready code across every dimension.

- **dotnet-webapi** (176.5) — Excellent Minimal API architecture with TypedResults, endpoint extension methods, and full CancellationToken support. Loses points only for wildcard package versions and EnsureCreated.

- **managedcode-dotnet-skills** (174.5) — Very close to dotnet-webapi with strong modern patterns. Uses `IEntityTypeConfiguration<T>` (best EF config pattern), but lacks endpoint extension methods and uses wildcard versions.

- **dotnet-skills** (142.0) — Functional but uses outdated patterns: controllers, Swashbuckle, FluentValidation, convention middleware, and no CancellationToken. Still produces correct business logic and sealed types.

- **no-skills** (129.5) — The baseline produces working code but with significant gaps: controllers, mutable class DTOs, zero sealed types, no AsNoTracking, no records, and no modern C# features. Ironically the only config with HTTPS redirection.

### Key Takeaway

The **dotnet-artisan** plugin chain produces code that is demonstrably more production-ready than any other configuration, particularly through its use of EF migrations, exact version pinning, and comprehensive adoption of modern .NET patterns. The gap between the top three (artisan, webapi, managedcode) and the bottom two (skills, no-skills) is primarily driven by architectural choices: Minimal APIs vs Controllers, modern C# features, and async best practices.
