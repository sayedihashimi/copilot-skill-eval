# VetClinicApi — Generation Notes

## Skills Used

The following .NET Copilot skills were invoked during the generation of this project:

### 1. `dotnet` (Router Skill)
- **Purpose**: Classified the project by app model (ASP.NET Core Web API + EF Core) and routed to the narrowest matching specialized skills.
- **Influence**: Identified that `dotnet-project-setup`, `dotnet-aspnet-core`, `dotnet-entity-framework-core`, `dotnet-modern-csharp`, and `dotnet-microsoft-extensions` were the relevant specialized skills to invoke.

### 2. `dotnet-project-setup`
- **Purpose**: Guided project scaffolding, directory structure, and template selection.
- **Influence**:
  - Used `dotnet new webapi --use-controllers` to create a controller-based Web API targeting .NET 10.
  - Project organized with clear folder separation: `Models/`, `DTOs/`, `Services/`, `Controllers/`, `Data/`, `Middleware/`.
  - NuGet packages added via `dotnet add package` (EF Core SQLite, Swashbuckle.AspNetCore).
  - Reference patterns from `references/patterns.md` guided the solution layout.

### 3. `dotnet-aspnet-core`
- **Purpose**: Ensured correct ASP.NET Core hosting, middleware pipeline, and configuration patterns.
- **Influence**:
  - Middleware order follows the documented correct sequence: `UseExceptionHandler` → `UseSwagger` → `MapControllers`.
  - Global exception handling uses `IExceptionHandler` (the modern ASP.NET Core pattern) returning RFC 7807 `ProblemDetails`.
  - `ILogger<T>` used throughout services for structured logging at appropriate levels.
  - Controller pattern follows ASP.NET Core conventions: `[ApiController]`, `[Route]`, proper `ProducesResponseType` attributes.
  - JSON serialization configured with `JsonStringEnumConverter` and null-ignoring options.

### 4. `dotnet-entity-framework-core`
- **Purpose**: Guided data access design, model configuration, and query patterns.
- **Influence**:
  - `DbContext` registered with `AddDbContext` using scoped lifetime (EF Core best practice).
  - Fluent API configuration in `OnModelCreating` with proper indexes, unique constraints, and relationship definitions.
  - `AsNoTracking()` used on all read-only queries for performance.
  - DTO projections via `.Select()` to avoid loading entire entity graphs.
  - `DeleteBehavior.Restrict` on foreign keys to prevent cascade-delete issues.
  - Computed properties (`IsActive`, `IsExpired`, `IsDueSoon`) marked with `.Ignore()` in EF configuration since they're app-level computed values.
  - `SaveChangesAsync` override for automatic timestamp management.

### 5. `dotnet-modern-csharp`
- **Purpose**: Ensured modern C# 14 idioms compatible with .NET 10.
- **Influence**:
  - Primary constructors used on services and middleware classes (C# 12+).
  - Collection expressions (`[]`) used for initializing empty collections.
  - File-scoped namespaces throughout.
  - Records used for DTOs (immutable data transfer objects).
  - Pattern matching with `is not` and list patterns.
  - `null!` for required navigation properties.
  - Target-typed `new()` expressions.

### 6. `dotnet-microsoft-extensions`
- **Purpose**: Guided DI registration, configuration binding, and logging patterns.
- **Influence**:
  - All services registered as scoped behind interfaces (`IOwnerService` → `OwnerService`).
  - Connection string loaded from `appsettings.json` via `GetConnectionString()`.
  - `ILogger<T>` injected via primary constructors for structured, category-aware logging.
  - Service registration kept at the composition root (`Program.cs`).

## Architecture Summary

```
src/VetClinicApi/
├── Controllers/          # API controllers (7 controllers for all endpoints)
├── Data/                 # DbContext + data seeder
├── DTOs/                 # Request/response data transfer objects + PagedResult<T>
├── Middleware/            # GlobalExceptionHandler + BusinessRuleException
├── Models/               # Entity classes (7 entities + 1 enum)
├── Services/             # Service interfaces + implementations
├── Program.cs            # Application entry point and DI configuration
├── appsettings.json      # Configuration with SQLite connection string
└── VetClinicApi.http     # HTTP test file for all endpoints
```

## Key Design Decisions

1. **Controller-based API** over Minimal APIs — chosen for the complexity of this domain (7 resource types, 30+ endpoints) where controller grouping provides clearer organization.
2. **Service layer behind interfaces** — business logic isolated from controllers for testability and separation of concerns.
3. **Custom `BusinessRuleException`** — provides typed exception handling that maps to appropriate HTTP status codes (400, 409) via the global exception handler.
4. **`PagedResult<T>`** — consistent pagination pattern across all list endpoints.
5. **Seed data in `DataSeeder`** — runs only when the database is empty, uses `EnsureCreatedAsync` for schema creation.
