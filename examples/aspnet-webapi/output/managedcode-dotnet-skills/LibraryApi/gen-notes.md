# Generation Notes — Sunrise Community Library API

## Skills Used

### 1. `dotnet` (Router Skill)
**Role:** Initial classification and routing.
The router skill identified this as an ASP.NET Core Web API with EF Core, then delegated to the narrower specialized skills below.

### 2. `dotnet-aspnet-core`
**Influence on generated code:**
- **Middleware ordering:** `UseExceptionHandler()` is placed first in the pipeline, followed by OpenAPI/Swagger, then controllers — matching the correct ASP.NET Core middleware order.
- **Global exception handling:** Implemented `IExceptionHandler` (the modern .NET 8+ pattern) instead of custom middleware, producing RFC 7807 `ProblemDetails` responses.
- **Scoped service lifetimes:** All services registered as `Scoped` to align with the per-request DbContext lifetime.
- **ILogger<T> usage:** Structured logging with category-specific loggers injected via primary constructors throughout all services.

### 3. `dotnet-entity-framework-core`
**Influence on generated code:**
- **Fluent API configuration:** All entity relationships, indexes, and constraints configured in `OnModelCreating` using Fluent API rather than relying solely on data annotations.
- **AsNoTracking() for reads:** All read-only queries use `.AsNoTracking()` to reduce memory overhead, following EF Core performance best practices.
- **Projection to DTOs:** Queries project directly to DTO types via `.Select()` rather than loading full entities and mapping in-memory.
- **Index definitions:** Indexes on `Status`, `DueDate`, and other frequently filtered columns for query performance.
- **Scoped DbContext lifetime:** Aligned with the unit-of-work pattern per the skill's guidance.

### 4. `dotnet-project-setup`
**Influence on generated code:**
- **Project structure:** Clean separation into `Models/`, `DTOs/`, `Services/`, `Controllers/`, `Data/`, `Middleware/`, and `Validators/` directories following recommended conventions.
- **Target framework:** `net10.0` with `Nullable` and `ImplicitUsings` enabled.
- **Self-contained project:** Standalone Web API project with no external project dependencies, runnable with `dotnet run`.

### 5. `dotnet-microsoft-extensions`
**Influence on generated code:**
- **Dependency injection:** Interface + implementation pattern (`IAuthorService` → `AuthorService`) with proper scoped registration.
- **Configuration:** Connection string configured via `appsettings.json` and accessed through the standard `IConfiguration` pattern.
- **Logging:** `ILogger<T>` used throughout with category-based logging; key business operations logged at Information level, errors at Error level.
- **Service registration:** Composition at the edge (Program.cs) with concrete implementations hidden behind interfaces.

### 6. `dotnet-minimal-apis`
**Influence on generated code:**
- While the skill was consulted, the decision was made to use **Controllers** instead of Minimal APIs because the spec required "clear separation of concerns" with many endpoints and complex business logic that maps naturally to controller classes.
- The skill's DTOs pattern (using records for request/response types) was adopted.

### 7. `dotnet-modern-csharp`
**Influence on generated code:**
- **Primary constructors:** All services and controllers use C# 12 primary constructors for dependency injection (e.g., `public class AuthorService(LibraryDbContext db, ILogger<AuthorService> logger)`).
- **Collection expressions:** Empty collections initialized with `[]` syntax (C# 12).
- **Pattern matching:** Switch expressions used for membership type lookups and exception categorization.
- **Records:** DTOs defined as `record` types for immutability and value semantics.
- **Target-typed new:** `new()` used throughout for concise object creation.
- **Null-coalescing patterns:** `?? throw` pattern for required entity lookups.

## Architecture Summary

| Layer | Pattern | Rationale |
|-------|---------|-----------|
| Controllers | `[ApiController]` with FluentValidation | Clear separation, Swagger integration, validation |
| Services | Interface + Implementation | Testability, DI, single responsibility |
| Data | EF Core with SQLite | Spec requirement; AsNoTracking + projection for perf |
| DTOs | C# records | Immutable, concise, value equality |
| Error Handling | `IExceptionHandler` → ProblemDetails | Modern .NET pattern, RFC 7807 compliance |
| Validation | FluentValidation | Rich validation rules with clean separation |
