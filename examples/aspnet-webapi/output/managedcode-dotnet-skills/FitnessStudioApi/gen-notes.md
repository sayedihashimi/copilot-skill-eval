# Generation Notes — FitnessStudioApi

## Skills Used

The following Copilot skills were loaded and consulted during code generation:

### 1. `dotnet` (Router Skill)
**Role:** Classified the project as an ASP.NET Core Web API with EF Core data access, then routed to the narrowest matching skills below.

### 2. `dotnet-aspnet-core`
**Influence on generated code:**
- **Middleware ordering** follows the documented ASP.NET Core convention (exception handler → routing → endpoints).
- **Global exception handling** uses the `IExceptionHandler` interface (not a raw middleware) to produce RFC 7807 `ProblemDetails` responses — as recommended over catching exceptions silently.
- **`ILogger<T>`** is used for structured logging throughout services with proper log categories.
- **Controller patterns** follow the template guidance: `[ApiController]`, `[Route]`, `CancellationToken` parameters, `ProducesResponseType` attributes, and `CreatedAtAction` for POST responses.
- **No `async void`** — all async methods return `Task` or `Task<T>`.

### 3. `dotnet-entity-framework-core`
**Influence on generated code:**
- **`DbContext` lifetime is scoped** via `AddDbContext<T>()` — aligned with unit-of-work pattern per request.
- **`AsNoTracking()`** is used for all read-only queries to avoid unnecessary change-tracking overhead.
- **Fluent API entity configuration** is done directly in `OnModelCreating` with explicit indexes on commonly queried columns (`Email`, `StartTime/EndTime`, `MemberId+Status`).
- **Enum-to-string conversion** is configured via `HasConversion<string>()` for readable database storage.
- **Projection to DTOs** (`Select(...)`) is used where possible to load only needed data, avoiding loading full entity graphs.
- **Foreign key relationships** are explicitly configured with appropriate `DeleteBehavior` (Cascade vs Restrict).
- **No N+1 queries** — `Include()` is used when navigation properties are needed, and queries are batched.

### 4. `dotnet-project-setup`
**Influence on generated code:**
- **Project created with `dotnet new webapi --use-controllers`** following the template guidance.
- **Clean separation of concerns**: Models, DTOs, Services (interface + implementation), Controllers, Data, and Middleware folders.
- **Connection string** is configured in `appsettings.json` (not hardcoded) per the patterns reference.

### 5. `dotnet-modern-csharp`
**Influence on generated code:**
- **Primary constructors** (C# 12+) used on all services, controllers, middleware, and DbContext — reduces boilerplate.
- **Record types** used for all DTOs, providing value semantics and immutability.
- **Collection expressions** (`[]` syntax) used for initializing empty collections on entity navigation properties.
- **Pattern matching** (`is not null`, `switch` expressions with property patterns) used throughout for null checks and status branching.
- **File-scoped namespaces** used consistently.
- **`FindAsync([id], ct)`** uses collection expressions for params.
- **Target-typed `new()`** expressions used for object initialization.

### 6. `dotnet-microsoft-extensions`
**Influence on generated code:**
- **DI registration** at the composition root in `Program.cs` with scoped lifetimes matching DbContext per-request pattern.
- **`ILogger<T>`** injected via primary constructor for structured, category-aware logging.
- **Configuration** loaded from `appsettings.json` with connection string binding.
- **No mini-frameworks** built over Microsoft.Extensions — the skill explicitly warned against this.

## Architecture Summary

```
src/FitnessStudioApi/
├── Controllers/         7 API controllers (one per resource)
├── Data/               DbContext + DataSeeder
├── DTOs/               Record-based request/response DTOs
├── Middleware/          Global exception handler (IExceptionHandler)
├── Models/             8 entity classes + enums
├── Services/           7 service interfaces + implementations
├── Program.cs          Composition root with middleware pipeline
└── FitnessStudioApi.http   Sample requests for all endpoints
```

## Key Design Decisions

| Decision | Rationale | Skill Source |
|----------|-----------|-------------|
| Controller-based API (not Minimal APIs) | Spec requested structured Web API with services layer | `dotnet-aspnet-core` |
| Scoped DbContext | Aligns with EF Core unit-of-work pattern | `dotnet-entity-framework-core` |
| `IExceptionHandler` over middleware | Modern .NET pattern, cleaner than raw `try/catch` middleware | `dotnet-aspnet-core` |
| Primary constructors everywhere | Reduces boilerplate, supported by .NET 10 (C# 14) | `dotnet-modern-csharp` |
| Records for DTOs | Immutable, value semantics, concise syntax | `dotnet-modern-csharp` |
| Interface + implementation pattern for services | Testability, separation of concerns | `dotnet-microsoft-extensions` |
| Enum stored as strings in database | Readability and debugging ease | `dotnet-entity-framework-core` |
| AsNoTracking for reads | Performance — no change-tracking overhead | `dotnet-entity-framework-core` |
