# LibraryApi — Generation Notes

## Skills Used

### 1. `optimizing-ef-core-queries`

**Purpose:** Optimize Entity Framework Core queries by fixing N+1 problems, choosing correct tracking modes, and avoiding common performance traps.

**How it influenced the code:**

- **Removed redundant `.Include()` calls before `.Select()` projections.** The skill documents that `Include` is ignored when a `Select` projection follows — the projection tells EF Core exactly which columns and joins to generate. This was applied in `BookService.GetBooksAsync()`, `BookService.GetBookByIdAsync()`, `BookService.GetBookLoansAsync()`, and `BookService.GetBookReservationsAsync()`. The includes were creating unnecessary query plan overhead.

- **`AsNoTracking()` used on all read-only queries.** Per the skill's guidance, change tracking overhead is significant and should be disabled when entities won't be modified. All `Get*` and list queries across every service use `AsNoTracking()`, while mutation operations (checkout, return, renew) correctly use tracked queries since they need to persist changes.

- **Inline projections instead of mapping tracked entities.** Where possible, LINQ `Select` projections construct DTOs directly in the query expression. This ensures EF Core generates optimized SQL that fetches only the columns needed for the response, rather than materializing full entity graphs.

- **`AnyAsync()` used instead of `CountAsync()` for existence checks.** Following the skill's guidance to avoid scanning all rows when only existence matters, all "does this entity exist?" checks use `AnyAsync()` (e.g., checking for active loans before delete, checking for pending reservations before renewal).

### 2. `analyzing-dotnet-performance`

**Purpose:** Scan .NET code for ~50 performance anti-patterns across async, memory, strings, collections, LINQ, and structural patterns.

**How it influenced the code:**

- **Sealed all service implementation classes.** The skill flags unsealed classes as a structural anti-pattern because the JIT can devirtualize method calls on sealed types. All service classes (`AuthorService`, `BookService`, `CategoryService`, `PatronService`, `LoanService`, `ReservationService`, `FineService`) were sealed using the `sealed` keyword, enabling potential devirtualization optimizations.

- **Structured logging with compile-time template caching.** All `ILogger` calls use structured logging with message templates (e.g., `logger.LogInformation("Book {BookId} checked out to patron {PatronId}", ...)`) rather than string interpolation. This avoids string allocation on every log call when the log level is disabled, following the skill's string allocation avoidance guidance.

- **Scoped DbContext lifetime.** Per the skill's guidance (and EF Core best practices), `LibraryDbContext` is registered as scoped (`AddDbContext` defaults to scoped), ensuring it's not kept alive longer than a single request.

## Architecture Decisions

- **Interface + implementation pattern:** All services use `I*Service` interfaces to enable testability and DI.
- **Global exception handling middleware:** Returns RFC 7807 ProblemDetails for all error types.
- **Consistent pagination:** `PagedResult<T>` wrapper used across all list endpoints.
- **Seed data:** `DataSeeder` runs only when the database is empty, with internally consistent data (AvailableCopies matches active loans, fine amounts match overdue days).

## Project Structure

```
src/LibraryApi/
├── Controllers/        # API controllers (one per resource)
├── Data/               # DbContext and data seeder
├── DTOs/               # Request/response DTOs and pagination
├── Middleware/          # Global exception handler
├── Models/             # Entity classes and enums
├── Services/           # Business logic (interface + sealed implementation)
├── Program.cs          # App configuration and startup
├── appsettings.json    # Configuration with SQLite connection string
└── LibraryApi.http     # Sample HTTP requests for all endpoints
```
