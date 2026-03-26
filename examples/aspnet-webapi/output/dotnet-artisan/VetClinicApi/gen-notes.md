# VetClinicApi — Generation Notes

## Skills Used

The following Copilot skills were invoked and influenced the generated code:

### 1. `using-dotnet` (Process Gateway)
- **Role**: Detected .NET intent from the prompt (ASP.NET Core, EF Core, C# keywords) and initiated the routing pipeline.
- **Influence**: Enforced the "simplicity first" (KISS) principle — the architecture was kept appropriately scaled: service layer with interfaces (as spec required), but no extra abstractions like repositories, MediatR, or CQRS. Used `DbContext` directly as the data access layer per the skill's guidance to "use what the framework gives you."

### 2. `dotnet-advisor` (Router)
- **Role**: Routed the request to the correct domain skills and established the loading order.
- **Influence**: Identified this as a "build me an app" request requiring: `dotnet-csharp` (baseline) → `dotnet-api` (primary domain). Confirmed .NET-first defaults and SDK availability check.

### 3. `dotnet-csharp` (Baseline C# Standards)
- **Role**: Loaded coding standards, async/await correctness, and code smell avoidance rules.
- **Influence on generated code**:
  - **Naming conventions**: PascalCase for types/methods/properties, `_camelCase` for private fields, `Async` suffix on all async methods, boolean prefixes (`IsActive`, `IsExpired`, `IsDueSoon`, `HasPreviousPage`).
  - **File-scoped namespaces**: Every `.cs` file uses `namespace X;` (not block-scoped).
  - **Sealed classes**: All service implementations and model classes are `sealed` for performance and intent clarity.
  - **Primary constructors**: Used C# 12 primary constructors for DI injection in services (e.g., `OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger)`).
  - **Async/await correctness**: All async methods propagate `CancellationToken ct = default`, no `.Result` or `.Wait()` calls, no fire-and-forget.
  - **Null handling**: Used `is not null` / `is null` pattern matching, null-conditional operators, no null-forgiving (`!`) operator abuse.
  - **Collection expressions**: Used `[]` syntax for empty collections (e.g., `ICollection<Pet> Pets { get; set; } = [];`).
  - **Code smell avoidance**: No empty catch blocks, no swallowed exceptions, proper DI lifetime matching (all services are scoped to match DbContext).

### 4. `dotnet-api` (ASP.NET Core API Patterns)
- **Role**: Provided patterns for Minimal APIs, EF Core, middleware, and OpenAPI configuration.
- **Influence on generated code**:

#### Minimal APIs (from `references/minimal-apis.md`)
  - **Route groups**: Each resource uses `MapGroup("/api/resource").WithTags("Resource")` for organized endpoint registration.
  - **Extension method pattern**: Each resource has its own `*Endpoints.cs` file with a `Map*Endpoints()` extension method called from `Program.cs`.
  - **TypedResults**: All endpoints return `TypedResults.*` (not `Results.*`) for automatic OpenAPI schema generation. Union return types like `Results<Ok<T>, NotFound>` document all possible response codes.
  - **Parameter binding**: Uses `[FromQuery]` annotations for filter/pagination parameters with sensible defaults.
  - **ConfigureHttpJsonOptions**: JSON serialization configured with `JsonStringEnumConverter` for readable enum values and `WhenWritingNull` to omit nulls.

#### EF Core (from `references/efcore-patterns.md`)
  - **DbContext as scoped**: Registered via `AddDbContext<T>()` — one per request.
  - **AsNoTracking**: All read-only queries use `.AsNoTracking()` to reduce memory/CPU overhead.
  - **CancellationToken propagation**: Passed to all `ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()` calls.
  - **Timestamp interceptor pattern**: `SaveChanges` override sets `CreatedAt`/`UpdatedAt` automatically.
  - **Data seeding**: Uses a `DataSeeder` class that checks for existing data before seeding (idempotent).
  - **Connection string from config**: Read from `appsettings.json` via `GetConnectionString()`.

#### Agent Gotchas Avoided (from `references/agent-gotchas.md`)
  - Used `Microsoft.NET.Sdk.Web` (not `Microsoft.NET.Sdk`) for the project.
  - Did not add `PackageReference` for shared-framework packages (e.g., `Microsoft.Extensions.Logging`).
  - No `.Result` or `.Wait()` calls — async all the way.
  - `Nullable` enabled in `.csproj`.
  - Correct NuGet package names (`Microsoft.EntityFrameworkCore.Sqlite`, not `EntityFrameworkCore`).

#### Error Handling
  - **Global exception handler**: Implements `IExceptionHandler` (ASP.NET Core 8+ pattern) returning RFC 7807 `ProblemDetails`.
  - **Business rule violations**: Return `400 Bad Request` or `409 Conflict` with descriptive `ProblemDetails` responses.
  - **Validation**: Uses Data Annotations on DTOs (`[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`).

## Architecture Decisions

| Decision | Rationale |
|----------|-----------|
| Minimal APIs over Controllers | Recommended by `dotnet-api` skill for new projects; lighter weight, better OpenAPI integration |
| Service interfaces + implementations | Explicitly required by spec; enables testability |
| Sealed classes | `dotnet-csharp` coding standard for performance and design intent |
| `record` types for DTOs | Immutable by default, concise, value-based equality |
| `EnsureCreatedAsync` (not migrations) | Simpler for demo/seed scenarios; `efcore-patterns.md` notes this is appropriate for non-production usage |
| Swashbuckle for Swagger UI | Spec requires Swagger UI at `/swagger`; built-in `MapOpenApi` only serves JSON |

## .NET Version

- **Target Framework**: `net10.0` (LTS, C# 14)
- **C# 14 features used**: Primary constructors, collection expressions, file-scoped namespaces
- **Packages**: EF Core 10.0.5 (SQLite), Swashbuckle 10.1.6, Microsoft.AspNetCore.OpenApi 10.0.4
