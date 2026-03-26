# Generation Notes — FitnessStudioApi

## Skills Used

### 1. `using-dotnet` (Process Gateway)
**How it influenced the code:**
- Detected .NET intent and enforced the routing sequence: `using-dotnet` → `dotnet-advisor` → `dotnet-csharp` + `dotnet-api`.
- Applied the **KISS principle** from the skill: avoided over-engineering (no MediatR, no CQRS, no repository pattern wrapping EF Core). Used `DbContext` directly as the unit of work and `DbSet<T>` as the repository.
- Matched architecture to scope: service + interface pattern is appropriate for the business logic complexity without adding unnecessary layers.

### 2. `dotnet-advisor` (Router)
**How it influenced the code:**
- Routed to `dotnet-api` as the primary domain skill (ASP.NET Core Web API + EF Core).
- Loaded `dotnet-csharp` as the baseline skill for all code paths.
- Detected project version (.NET 10 / C# 14) and ensured version-appropriate patterns were used.
- Applied ".NET-first defaults" — used ASP.NET Core Minimal APIs as the default API approach.

### 3. `dotnet-csharp` (Baseline C# Patterns)
**How it influenced the code:**
- **Coding standards** (`references/coding-standards.md`): PascalCase naming, file-scoped namespaces, `sealed` classes for non-inheritable types, explicit access modifiers, `_camelCase` private fields via primary constructors.
- **Async patterns** (`references/async-patterns.md`): Async all the way, `CancellationToken` propagated through all async methods, no `.Result` or `.Wait()` blocking calls, proper `Task<T>` return types.
- **Modern patterns** (`references/dotnet-releases.md`): Targeted net10.0 with C# 14 features. Used primary constructors for DI injection, collection expressions (`[]`), `is not null` pattern matching.
- **Code smells** (`references/code-smells.md`): Avoided async void, swallowed exceptions, unnecessary null-forgiving operators. All exceptions are logged before being handled.

### 4. `dotnet-api` (ASP.NET Core & EF Core)
**How it influenced the code:**
- **Minimal APIs** (`references/minimal-apis.md`): Used route groups with `MapGroup()` and extension method organization pattern. Each resource has its own `*Endpoints.cs` file with a `Map*Endpoints()` extension method.
- **EF Core patterns** (`references/efcore-patterns.md`): Scoped `DbContext` lifetime (one per request), `AsNoTracking()` for read-only queries, proper navigation property configuration with `OnDelete` behaviors.
- **Architecture patterns** (`references/architecture-patterns.md`): ProblemDetails (RFC 9457) for error responses via `IExceptionHandler`, consistent error handling with `InvalidOperationException` for business rule violations and `KeyNotFoundException` for missing entities.
- **OpenAPI** (`references/openapi.md`): Used `Microsoft.AspNetCore.OpenApi` (built-in, recommended for .NET 10) with document transformers instead of Swashbuckle for document generation. Added `Swashbuckle.AspNetCore.SwaggerUI` only for the Swagger UI at the root path. OpenAPI 3.1 format.
- **Agent gotchas** (`references/agent-gotchas.md`): Used `Microsoft.NET.Sdk.Web` (not `Microsoft.NET.Sdk`), didn't reference shared-framework packages explicitly, used correct EF Core package names, avoided `Database.Migrate()` at startup (used `EnsureCreatedAsync()` for development seeding).

## Architecture Decisions

| Decision | Rationale |
|----------|-----------|
| Minimal APIs over Controllers | Simpler, less boilerplate, first-class .NET 10 support per skill guidance |
| Service + Interface pattern | Spec requires "interface + implementation" separation of concerns |
| `Results.*` for conditional returns | Inline ternary with `TypedResults` causes delegate resolution issues in .NET 10; block bodies with `Results.*` resolves cleanly |
| `IExceptionHandler` | Built-in .NET 8+ pattern for global exception handling, returns RFC 9457 ProblemDetails |
| SQLite with `EnsureCreatedAsync` | Development convenience; spec requires SQLite with seed data on startup |
| `JsonStringEnumConverter` | Enums serialize as strings for readable API responses |
| Data seeder with guard clause | Seeds only when database is empty, preventing duplicates on restart |

## Project Structure

```
src/FitnessStudioApi/
├── Data/           # DbContext and seed data
├── DTOs/           # Request/response records
├── Endpoints/      # Minimal API route group extensions
├── Middleware/      # Global exception handler
├── Models/         # Entity classes and enums
├── Services/       # Business logic (interface + implementation)
├── Program.cs      # App configuration and startup
└── FitnessStudioApi.http  # HTTP client test file
```

## Business Rules Implemented

All 12 business rules from the specification are implemented in the service layer:
1. Booking window (7 days advance, 30 min minimum)
2. Capacity management with automatic waitlist promotion
3. Cancellation policy (2-hour threshold, late cancellation flagging)
4. Membership tier access (premium class restrictions)
5. Weekly booking limits (ISO week calculation)
6. Active membership requirement
7. No double booking (time overlap check)
8. Instructor schedule conflict prevention
9. Membership freeze (7-30 days, once per term, end date extension)
10. Class cancellation cascade
11. Check-in window (±15 minutes of class start)
12. No-show flagging (after 15 minutes past start)
