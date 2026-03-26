# VetClinicApi — Generation Notes

## Skills Used

### `dotnet-webapi` Skill

The **dotnet-webapi** skill was the primary skill used throughout code generation. It provided authoritative guidance on ASP.NET Core Web API patterns and directly influenced every layer of the application.

## How the Skill Influenced the Generated Code

### API Style — Minimal APIs (Step 1)
The skill instructs to **default to minimal APIs** for new projects. All endpoints use `app.MapGroup()` with chained metadata, rather than controllers.

### Sealed Types (General Rule)
Every class and record is marked `sealed` per the skill's blanket rule — entities, services, DTOs, and middleware. This follows CA1852 and enables JIT devirtualization.

### DTOs as Sealed Records (Step 2)
- **Response DTOs** use positional `sealed record` syntax (e.g., `OwnerResponse(int Id, ...)`).
- **Request DTOs** use `sealed record` with `required init` properties so Data Annotations validate naturally.
- Naming follows `Create{Entity}Request`, `Update{Entity}Request`, `{Entity}Response`.

### TypedResults with Explicit Return Types (Step 3)
All endpoint handlers use `TypedResults` (not the `Results` factory) with explicit `Task<Results<T1, T2>>` return types, as the skill requires. This provides compile-time OpenAPI metadata.

### HTTP Status Codes (Step 3)
- `GET` → 200 OK / 404 Not Found
- `POST` → 201 Created with `Location` header
- `PUT` → 200 OK / 404 Not Found
- `PATCH` → 200 OK / 400 Bad Request
- `DELETE` → 204 No Content / 404 Not Found / 409 Conflict

### CancellationToken (Step 3)
Every endpoint accepts `CancellationToken` and forwards it through all async service and EF Core calls.

### Pagination (Step 3)
A generic `PaginatedResponse<T>` sealed record with `IReadOnlyList<T>` items is used across all list endpoints, with sensible defaults (page 1, pageSize 20, max 100).

### OpenAPI — Built-in Support, No Swashbuckle (Step 4)
The skill explicitly warns **not** to add any `Swashbuckle.*` package for .NET 9+ projects. The project uses `builder.Services.AddOpenApi()` + `app.MapOpenApi()` only. Enum serialization is configured with `JsonStringEnumConverter`.

### Global Error Handling with IExceptionHandler (Step 5)
The skill's `IExceptionHandler` pattern is implemented in `Middleware/ApiExceptionHandler.cs`, mapping domain exceptions to RFC 7807 ProblemDetails responses. The handler is placed in the `Middleware/` folder as the skill requires.

### Service Layer with Interfaces (Step 6)
Every service has a corresponding interface (`IOwnerService` / `OwnerService`, etc.) registered via `AddScoped<IService, Service>()`. DbContext is never injected directly into endpoints.

### EF Core Configuration (Step 6)
- **Migrations** are used (not `EnsureCreated()`).
- **Seed data** uses `HasData()` in `OnModelCreating` so it is captured in migrations.
- Unique indexes, cascade/restrict delete behaviors, enum-as-string conversion, and decimal column types are all configured via Fluent API.
- Read-only queries use `AsNoTracking()`.

### .http Test File (Step 7)
A `VetClinicApi.http` file covers every endpoint with realistic bodies matching seed data IDs, grouped by resource with comment headers.

### Build Verification (Step 8)
The project builds with **0 errors and 0 warnings**.

## Project Structure

```
src/VetClinicApi/
├── Data/                  # DbContext with Fluent API config and seed data
├── DTOs/                  # Sealed record request/response types
├── Endpoints/             # Minimal API endpoint groups (one per resource)
├── Middleware/             # ApiExceptionHandler (IExceptionHandler)
├── Migrations/            # EF Core migration (InitialCreate)
├── Models/                # Entity classes (all sealed)
├── Services/              # Interfaces and implementations
├── GlobalUsings.cs        # Global using for HttpResults
├── Program.cs             # App configuration and startup
├── appsettings.json       # Connection string for SQLite
└── VetClinicApi.http      # HTTP test file for all endpoints
```
