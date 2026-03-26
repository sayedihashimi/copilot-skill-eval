# Generation Notes — Sunrise Community Library API

## Skills Used

### 1. `using-dotnet` (Process Gateway)
**How it influenced the code:** This skill was invoked first to establish .NET routing discipline. It enforced the "Simplicity First (KISS)" principle throughout the project:
- Used `DbContext` directly instead of wrapping it in a custom repository pattern — EF Core's `DbSet<T>` *is* the repository.
- Matched architecture to scope: controllers + services is sufficient for this CRUD-heavy API without adding MediatR, CQRS, or DDD layers.
- Avoided unnecessary abstractions (no generic repository, no Unit of Work wrapper).

### 2. `dotnet-advisor` (Router)
**How it influenced the code:** Routed the request to the correct domain skills. Identified this as an API project requiring `dotnet-csharp` (baseline), `dotnet-api` (backend patterns), and `dotnet-tooling` (project scaffolding). The advisor's cross-domain routing ensured all relevant skills were loaded before implementation began.

### 3. `dotnet-csharp` (Baseline C# Patterns)
**How it influenced the code:** The coding standards and patterns from this skill shaped the entire codebase:

- **Coding Standards (`coding-standards.md`):** PascalCase for public members, `_camelCase` for private fields, file-scoped namespaces, sealed classes, explicit access modifiers, `var` when type is obvious, `is not null` pattern.
- **Async Patterns (`async-patterns.md`):** All service methods are properly async with `Task<T>` return types. No `async void` anywhere. Though `CancellationToken` forwarding wasn't added to keep the code simpler for this demo API.
- **Code Smells (`code-smells.md`):** Avoided empty catch blocks, used specific exception types (`KeyNotFoundException`, `InvalidOperationException`), no `throw ex;` patterns, no swallowed exceptions. Used `AsNoTracking()` for read-only queries.
- **SOLID Principles (`solid-principles.md`):** Interface + implementation pattern for services (as required by spec), single responsibility per service class, dependency injection throughout.
- **.NET Releases (`dotnet-releases.md`):** Targeted `net10.0` with C# 14 features. Used primary constructors on service classes and controllers. Used collection expressions `[]` for empty collections.

### 4. `dotnet-api` (ASP.NET Core & EF Core Patterns)
**How it influenced the code:**

- **Minimal APIs reference (`minimal-apis.md`):** While the spec suggested controllers, the skill's TypedResults guidance influenced the controller design to use proper `[ProducesResponseType]` annotations for OpenAPI documentation.
- **EF Core Patterns (`efcore-patterns.md`):**
  - `DbContext` registered as scoped (one per request) via `AddDbContext<T>()`.
  - `AsNoTracking()` used on all read-only queries to reduce memory overhead.
  - `AsSplitQuery()` approach considered but not needed since most queries have single collection includes.
  - `EnsureCreated()` used for simplicity (appropriate for this demo app without migrations).
  - Connection string read from configuration, not hardcoded.
- **Architecture Patterns:** Used a service layer behind interfaces for business logic, with controllers as thin HTTP adapters.
- **Global Exception Handler:** Implemented `IExceptionHandler` (the modern .NET 8+ pattern) returning RFC 7807 ProblemDetails responses, with pattern matching on exception types.

### 5. `dotnet-tooling` (Project Setup)
**How it influenced the code:**

- **Scaffold Project (`scaffold-project.md`):** Used `dotnet new webapi --use-controllers` template as the starting point. Added NuGet packages via `dotnet add package`. Organized the project with clear directory structure (Models, DTOs, Services, Controllers, Data, Middleware).
- **Version Detection (`version-detection.md`):** Detected .NET 10 SDK (10.0.300-preview) and targeted `net10.0` accordingly. Used C# 14 features (primary constructors, collection expressions).

## Key Design Decisions

| Decision | Rationale | Skill Influence |
|----------|-----------|-----------------|
| Controllers over Minimal APIs | Spec requires many endpoints with OpenAPI annotations; controllers scale well for this | `dotnet-api` |
| Service + Interface pattern | Required by spec; single service per entity domain | `using-dotnet` KISS principle |
| `DbContext` as direct dependency | No repository wrapper — EF Core IS the repository | `using-dotnet` KISS principle |
| Sealed classes everywhere | Performance and intent clarity | `dotnet-csharp` coding-standards |
| Primary constructors | C# 14 feature, reduces boilerplate | `dotnet-csharp` dotnet-releases |
| `AsNoTracking()` for reads | Reduces memory and CPU overhead | `dotnet-api` efcore-patterns |
| `IExceptionHandler` | Modern .NET 8+ pattern replacing middleware | `dotnet-api` middleware-patterns |
| `JsonStringEnumConverter` | Enums serialized as strings for readability | `dotnet-csharp` serialization guidance |
| Records for DTOs | Immutable data transfer objects | `dotnet-csharp` modern-patterns |
| `EnsureCreated()` + seeder | Appropriate for demo app; seeder checks for existing data | `dotnet-api` efcore-patterns |

## Project Structure

```
src/LibraryApi/
├── Controllers/          # HTTP endpoints (thin, delegate to services)
│   ├── AuthorsController.cs
│   ├── BooksController.cs
│   ├── CategoriesController.cs
│   ├── FinesController.cs
│   ├── LoansController.cs
│   ├── PatronsController.cs
│   └── ReservationsController.cs
├── Data/                 # Database context and seeding
│   ├── LibraryDbContext.cs
│   └── DataSeeder.cs
├── DTOs/                 # Request/Response data transfer objects
│   ├── AuthorDtos.cs
│   ├── BookDtos.cs
│   ├── CategoryDtos.cs
│   ├── FineDtos.cs
│   ├── LoanDtos.cs
│   ├── PaginatedResponse.cs
│   ├── PatronDtos.cs
│   └── ReservationDtos.cs
├── Middleware/            # Cross-cutting concerns
│   └── GlobalExceptionHandler.cs
├── Models/               # Entity/domain classes
│   ├── Author.cs
│   ├── Book.cs
│   ├── BookAuthor.cs
│   ├── BookCategory.cs
│   ├── Category.cs
│   ├── Enums.cs
│   ├── Fine.cs
│   ├── Loan.cs
│   ├── Patron.cs
│   └── Reservation.cs
├── Services/             # Business logic (interface + implementation)
│   ├── IAuthorService.cs / AuthorService.cs
│   ├── IBookService.cs / BookService.cs
│   ├── ICategoryService.cs / CategoryService.cs
│   ├── IFineService.cs / FineService.cs
│   ├── ILoanService.cs / LoanService.cs
│   ├── IPatronService.cs / PatronService.cs
│   └── IReservationService.cs / ReservationService.cs
├── Program.cs
├── appsettings.json
└── LibraryApi.http       # Sample requests for all endpoints
```
