# Generation Notes — FitnessStudioApi

## Skills Used

### `dotnet-webapi` (Primary Skill)

The **dotnet-webapi** skill was the primary skill used to guide the entire API design and implementation. It influenced the generated code in the following ways:

#### API Style Selection (Step 1)
- **Minimal APIs** were chosen as the default since this is a new project (no existing controllers).
- All endpoints are organized as static extension methods in the `Endpoints/` folder, grouped by resource.

#### DTO Design (Step 2)
- All DTOs are **`sealed record`** types following the skill's naming conventions:
  - `Create{Entity}Request` for input (create operations) with `init` properties and data annotations.
  - `Update{Entity}Request` for input (update operations).
  - `{Entity}Response` as positional records for concise, immutable output.
- EF Core entities are **never** exposed directly in API responses — all mapping goes through dedicated DTOs.

#### HTTP Semantics (Step 3)
- **`TypedResults`** with explicit `Results<T1, T2>` return types for endpoints returning multiple result types (e.g., `Ok` or `NotFound`).
- Correct HTTP status codes: `201 Created` with `Location` header for POST create, `204 No Content` for DELETE, `200 OK` for GET/PUT/PATCH.
- **`CancellationToken`** is accepted in every endpoint signature and forwarded through all async calls.
- **`PaginatedResponse<T>`** sealed record used for all list endpoints with consistent pagination (default 20, max 100).
- **`IReadOnlyList<T>`** used in all response collection properties to signal immutability.

#### OpenAPI Configuration (Step 4)
- Built-in `AddOpenApi()` + `MapOpenApi()` used (**.NET 10**) — **no Swashbuckle** packages added.
- Every endpoint has `.WithName()`, `.WithSummary()`, `.WithDescription()`, and `.Produces<T>()` metadata.
- Enums are serialized as strings via `JsonStringEnumConverter` for readable API responses and OpenAPI schemas.

#### Error Handling (Step 5)
- Global exception handler implementing `IExceptionHandler` placed in **`Middleware/`** folder.
- Returns RFC 7807 `ProblemDetails` for all error responses.
- Exception-to-status-code mapping: `KeyNotFoundException → 404`, `ArgumentException → 400`, `InvalidOperationException → 409`.
- `AddProblemDetails()` + `UseExceptionHandler()` + `UseStatusCodePages()` middleware pipeline.

#### Data Access (Step 6)
- **Service layer** with interfaces (`IService` + sealed `Service` implementation) between endpoints and `DbContext`.
- Services registered as `AddScoped<IService, Service>()`.
- `AsNoTracking()` used on all read-only queries.
- EF Core Fluent API configuration: unique indexes, enum-to-string conversions, decimal column types, explicit cascade behaviors.
- **EF Core migrations** used (`dotnet ef migrations add InitialCreate`) — not `EnsureCreated()`.
- **DataSeeder service** used for seeding (spec allows either HasData or a seeder service; seeder was chosen because seed data requires dynamic dates for class schedules).

#### Sealed Types
- **All classes and records are sealed** per the skill's guidance (CA1852), including entity classes, services, DTOs, and middleware.

#### .http File (Step 7)
- Comprehensive `FitnessStudioApi.http` file created with requests for every endpoint, realistic request bodies, query parameter examples, and business rule test scenarios.
- Port matches `launchSettings.json` (5176).

## Architecture Overview

```
FitnessStudioApi/
├── Models/          — 7 entity classes + enums (all sealed)
├── DTOs/            — Request/response records + PaginatedResponse<T>
├── Data/            — FitnessDbContext + DataSeeder
├── Services/        — 7 service interfaces + 7 sealed implementations
├── Middleware/       — ApiExceptionHandler (IExceptionHandler)
├── Endpoints/       — 7 minimal API endpoint groups
├── Migrations/      — EF Core migration (InitialCreate)
└── Program.cs       — Composition root
```

## Business Rules Implemented

All 12 business rules from the specification are enforced in the service layer:
1. Booking window (7 days advance, 30 min before)
2. Capacity management with automatic waitlist promotion
3. Cancellation policy (2-hour late cancellation marking)
4. Membership tier access for premium classes
5. Weekly booking limits per plan
6. Active membership required
7. No double booking (time overlap check)
8. Instructor schedule conflict prevention
9. Membership freeze (7–30 days, once per term, extends end date)
10. Class cancellation cascade (all bookings auto-cancelled)
11. Check-in window (15 min before to 15 min after)
12. No-show flagging (after 15 min past class start)
