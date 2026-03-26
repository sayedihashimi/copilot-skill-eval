# Generation Notes — LibraryApi

## Skills Used

### `dotnet-webapi` Skill

The **dotnet-webapi** skill was the primary skill used to guide the generation of this ASP.NET Core Web API application. It influenced the following aspects of the generated code:

## How the Skill Influenced the Code

### 1. API Style — Minimal APIs (Step 1)
The skill directs that new projects should **default to minimal APIs** unless the user explicitly requests controllers. Since this was a new project with no existing endpoint patterns, all endpoints were implemented using minimal API route groups (`MapGroup`) with extension methods per resource (e.g., `AuthorEndpoints.cs`, `BookEndpoints.cs`).

### 2. Sealed Types Throughout (General Rule)
Per the skill's directive to "seal all types by default," every class and record in the project is marked `sealed`:
- **Entity classes**: `public sealed class Book { ... }`
- **Service implementations**: `public sealed class LoanService(...) : ILoanService`
- **DTO records**: `public sealed record BookResponse(...)`
- **Middleware**: `internal sealed class ApiExceptionHandler(...) : IExceptionHandler`

### 3. DTO Design (Step 2)
Following the skill's naming conventions and immutability requirements:
- **Request DTOs**: `sealed record` with `init` properties and Data Annotations (e.g., `CreateBookRequest`, `UpdatePatronRequest`)
- **Response DTOs**: Positional `sealed record` types (e.g., `BookResponse(int Id, string Title, ...)`)
- **Naming**: `Create{Entity}Request`, `Update{Entity}Request`, `{Entity}Response`
- **EF Core entities are never exposed directly** in API responses

### 4. TypedResults with Explicit Return Types (Step 3)
All minimal API handlers use `TypedResults` (not the `Results` factory) with explicit `Results<T1, T2>` return type annotations to enable proper OpenAPI metadata inference:
```csharp
async Task<Results<Ok<BookResponse>, NotFound>> (int id, ...) => ...
```

### 5. HTTP Semantics (Step 3)
Correct status codes per the skill's table:
- **POST create** → `201 Created` with `Location` header
- **DELETE** → `204 No Content`
- **GET single** → `200 OK` / `404 Not Found`
- Errors → `400 Bad Request` / `409 Conflict`

### 6. CancellationToken (Step 3)
Every endpoint accepts `CancellationToken` and forwards it through to all async calls (service methods, EF Core queries).

### 7. Pagination (Step 3)
A reusable `PaginatedResponse<T>` record with `IReadOnlyList<T>` (not `List<T>`) is used across all list endpoints, following the skill's pagination pattern with page/pageSize/totalCount/totalPages/hasNextPage/hasPreviousPage.

### 8. OpenAPI Configuration (Step 4)
- Used built-in `AddOpenApi()` + `MapOpenApi()` for .NET 10 — **no Swashbuckle packages** per the skill's explicit prohibition
- `JsonStringEnumConverter` registered for enum serialization as strings
- All endpoints have `.WithName()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()` metadata

### 9. Error Handling (Step 5)
- `IExceptionHandler` implementation (`ApiExceptionHandler`) placed in the `Middleware/` folder
- Returns RFC 7807 `ProblemDetails` responses
- Exception type mapping: `KeyNotFoundException` → 404, `ArgumentException` → 400, `InvalidOperationException` → 409
- Registered via `AddExceptionHandler<T>()` + `AddProblemDetails()` + `UseExceptionHandler()`

### 10. Data Access Layer (Step 6)
- **Service layer with interfaces**: Every service has a corresponding interface (`IBookService` / `BookService`), registered in DI with `AddScoped<IService, Service>()`
- **DbContext not injected into handlers**: Services own all data access logic
- **`AsNoTracking()`** on all read-only queries
- **EF Core migrations** used (not `EnsureCreated()`)
- **Fluent API configuration**: Unique indexes, explicit delete behaviors, enum-to-string conversions, decimal column types
- **Seed data** implemented via a dedicated `DataSeeder` service (runs once when DB is empty)

### 11. .http Test File (Step 7)
A comprehensive `LibraryApi.http` file covers every endpoint with realistic request bodies, query parameters, and business rule test cases, matching the port from `launchSettings.json`.

### 12. Build Verification (Step 8)
The project builds with **zero errors and zero warnings**, the OpenAPI document loads at `/openapi/v1.json`, and all endpoints return correct status codes.

## Project Structure

```
LibraryApi/
├── Data/
│   ├── DataSeeder.cs           # Seed data for all entities
│   └── LibraryDbContext.cs     # EF Core DbContext with Fluent API
├── DTOs/
│   ├── AuthorDtos.cs           # Author request/response records
│   ├── BookDtos.cs             # Book request/response records
│   ├── CategoryDtos.cs         # Category request/response records
│   ├── FineDtos.cs             # Fine response record
│   ├── LoanDtos.cs             # Loan request/response records
│   ├── PaginatedResponse.cs    # Generic pagination wrapper
│   ├── PatronDtos.cs           # Patron request/response records
│   └── ReservationDtos.cs      # Reservation request/response records
├── Endpoints/
│   ├── AuthorEndpoints.cs      # /api/authors routes
│   ├── BookEndpoints.cs        # /api/books routes
│   ├── CategoryEndpoints.cs    # /api/categories routes
│   ├── FineEndpoints.cs        # /api/fines routes
│   ├── LoanEndpoints.cs        # /api/loans routes
│   ├── PatronEndpoints.cs      # /api/patrons routes
│   └── ReservationEndpoints.cs # /api/reservations routes
├── Middleware/
│   └── ApiExceptionHandler.cs  # Global IExceptionHandler
├── Models/
│   ├── Author.cs, Book.cs, BookAuthor.cs, BookCategory.cs
│   ├── Category.cs, Fine.cs, Loan.cs, Patron.cs, Reservation.cs
│   └── Enums: FineStatus, LoanStatus, MembershipType, ReservationStatus
├── Services/
│   ├── Interfaces: IAuthorService, IBookService, ICategoryService, etc.
│   └── Implementations: AuthorService, BookService, LoanService, etc.
├── Migrations/                 # EF Core migration (auto-generated)
├── GlobalUsings.cs
├── LibraryApi.csproj
├── LibraryApi.http             # HTTP test file for all endpoints
├── Program.cs                  # Application entry point
├── appsettings.json            # Connection string & logging config
└── Properties/launchSettings.json
```
