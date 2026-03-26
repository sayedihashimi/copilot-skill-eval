using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class PatronEndpoints
{
    public static void MapPatronEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patrons")
            .WithTags("Patrons");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<PatronResponse>>, BadRequest>> (
            string? search, MembershipType? membershipType, int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetAllAsync(search, membershipType, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatrons")
        .WithSummary("List patrons")
        .WithDescription("List patrons with search, membership type filter, and pagination.")
        .Produces<PaginatedResponse<PatronResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<PatronDetailResponse>, NotFound>> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("GetPatronById")
        .WithSummary("Get patron by ID")
        .WithDescription("Returns patron details with active loans count and unpaid fines balance.")
        .Produces<PatronDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<PatronResponse>, BadRequest, Conflict<ProblemDetails>>> (
            CreatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/patrons/{result.Id}", result);
        })
        .WithName("CreatePatron")
        .WithSummary("Create a new patron")
        .WithDescription("Creates a new patron. Email must be unique.")
        .Produces<PatronResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<PatronResponse>, NotFound, BadRequest>> (
            int id, UpdatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("UpdatePatron")
        .WithSummary("Update a patron")
        .WithDescription("Updates an existing patron's information.")
        .Produces<PatronResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeletePatron")
        .WithSummary("Deactivate a patron")
        .WithDescription("Deactivates a patron (sets IsActive = false). Fails if patron has active loans.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/loans", async Task<Results<Ok<PaginatedResponse<LoanResponse>>, NotFound>> (
            int id, string? status, int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetPatronLoansAsync(id, status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatronLoans")
        .WithSummary("Get patron's loans")
        .WithDescription("Returns patron's loans, optionally filtered by status (active, returned, overdue).")
        .Produces<PaginatedResponse<LoanResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async Task<Results<Ok<PaginatedResponse<ReservationResponse>>, NotFound>> (
            int id, int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetPatronReservationsAsync(id, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatronReservations")
        .WithSummary("Get patron's reservations")
        .WithDescription("Returns patron's reservations.")
        .Produces<PaginatedResponse<ReservationResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/fines", async Task<Results<Ok<PaginatedResponse<FineResponse>>, NotFound>> (
            int id, string? status, int? page, int? pageSize,
            IPatronService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetPatronFinesAsync(id, status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPatronFines")
        .WithSummary("Get patron's fines")
        .WithDescription("Returns patron's fines, optionally filtered by status (unpaid, paid, waived).")
        .Produces<PaginatedResponse<FineResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
