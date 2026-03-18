# Code Generation Notes

## Skills Used During Generation

### `dotnet-webapi` Skill

The **dotnet-webapi** skill was used for the generation of **all three** API projects. This skill provides specialized guidance for creating and modifying ASP.NET Core Web API endpoints with correct HTTP semantics, OpenAPI metadata, error handling, and data access wiring.

#### Projects Built

| Project | Location | Description |
|---------|----------|-------------|
| **FitnessStudioApi** | `./src/FitnessStudioApi/` | Fitness & Wellness Studio Booking API for "Zenith Fitness Studio" |
| **LibraryApi** | `./src/LibraryApi/` | Community Library Management API for "Sunrise Community Library" |
| **VetClinicApi** | `./src/VetClinicApi/` | Veterinary Clinic Management API for "Happy Paws Veterinary Clinic" |

#### What the Skill Guided

The `dotnet-webapi` skill provided conventions and best practices applied consistently across all three projects:

1. **API Endpoint Design** — Controller-based APIs with `[ApiController]` attribute, proper HTTP verb usage, and RESTful route conventions.

2. **HTTP Semantics** — `201 Created` with `Location` header for POST create operations (via `CreatedAtAction`), `204 No Content` for DELETE, `200 OK` / `404 Not Found` for GET, proper use of `400 Bad Request` and `409 Conflict` for business rule violations.

3. **OpenAPI / Swagger** — `.NET 10` native `AddOpenApi()` + `MapOpenApi()` combined with Swashbuckle for Swagger UI. All endpoints annotated with `[ProducesResponseType]`, `[EndpointSummary]`, and `[EndpointDescription]`.

4. **Error Handling** — Global exception handling using `IExceptionHandler` implementation registered with `AddProblemDetails()` and `AddExceptionHandler<T>()`. Returns RFC 7807 ProblemDetails responses.

5. **Request/Response DTOs** — Dedicated DTOs following the naming convention `Create{Entity}Request`, `Update{Entity}Request`, `{Entity}Response`. EF Core entities are never exposed directly in API responses.

6. **CancellationToken** — Accepted in every controller action and forwarded to all async service/EF Core calls.

7. **EF Core Data Access** — SQLite database with EF Core migrations (not `EnsureCreated()`). Seed data using `HasData()` in `OnModelCreating`. `AsNoTracking()` for read-only queries. Enums stored as strings with `.HasConversion<string>()`. Decimal column types explicitly specified.

8. **Service Layer Pattern** — Interface + implementation pattern for all business logic, registered via dependency injection. Controllers remain thin, delegating to services.

9. **Enum Serialization** — `JsonStringEnumConverter` configured so enums serialize as human-readable strings in JSON responses.

10. **Pagination** — Consistent `page` and `pageSize` query parameters across all list endpoints. Responses include `PagedResponse<T>` with `totalCount`, `totalPages`, `hasNextPage`, `hasPreviousPage` metadata.

11. **`.http` Test Files** — Each project includes a `.http` file with `@baseUrl` variable, `###` section separators, realistic request bodies, and requests covering all endpoints. IDs correspond to seed data for out-of-the-box testing.

#### Build Results

All three projects compile successfully with **0 warnings and 0 errors**. Each project has an initial EF Core migration (`InitialCreate`) generated and ready to apply.

| Project | Build Status | Warnings | Errors | Migrations |
|---------|-------------|----------|--------|------------|
| FitnessStudioApi | ✅ Succeeded | 0 | 0 | InitialCreate |
| LibraryApi | ✅ Succeeded | 0 | 0 | InitialCreate |
| VetClinicApi | ✅ Succeeded | 0 | 0 | InitialCreate |
