using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class PatronEndpoints
{
    public static void MapPatronEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/patrons").WithTags("Patrons");

        group.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] MembershipType? membershipType,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IPatronService service,
            CancellationToken ct) =>
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize == 0 ? 10 : pageSize, 1, 100);
            return TypedResults.Ok(await service.GetPatronsAsync(search, membershipType, page, pageSize, ct));
        })
        .WithName("GetPatrons")
        .WithSummary("List patrons")
        .WithDescription("List patrons with search by name/email, filter by membership type, and pagination.")
        .Produces<PaginatedResponse<PatronResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<PatronDetailResponse>, NotFound>> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            var patron = await service.GetPatronByIdAsync(id, ct);
            return patron is not null ? TypedResults.Ok(patron) : TypedResults.NotFound();
        })
        .WithName("GetPatronById")
        .WithSummary("Get patron details")
        .WithDescription("Get patron details with summary including active loans count and total unpaid fines.")
        .Produces<PatronDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<PatronResponse>> (
            CreatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var patron = await service.CreatePatronAsync(request, ct);
            return TypedResults.Created($"/api/patrons/{patron.Id}", patron);
        })
        .WithName("CreatePatron")
        .WithSummary("Create a new patron")
        .WithDescription("Create a new library patron.")
        .Produces<PatronResponse>(StatusCodes.Status201Created);

        group.MapPut("/{id:int}", async Task<Results<Ok<PatronResponse>, NotFound>> (
            int id, UpdatePatronRequest request, IPatronService service, CancellationToken ct) =>
        {
            var patron = await service.UpdatePatronAsync(id, request, ct);
            return patron is not null ? TypedResults.Ok(patron) : TypedResults.NotFound();
        })
        .WithName("UpdatePatron")
        .WithSummary("Update a patron")
        .WithDescription("Update an existing patron record.")
        .Produces<PatronResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            var (found, hasActiveLoans) = await service.DeactivatePatronAsync(id, ct);
            if (!found) return TypedResults.NotFound();
            if (hasActiveLoans) return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Cannot deactivate patron",
                Detail = "Patron has active loans and cannot be deactivated.",
                Status = StatusCodes.Status409Conflict
            });
            return TypedResults.NoContent();
        })
        .WithName("DeactivatePatron")
        .WithSummary("Deactivate a patron")
        .WithDescription("Deactivate a patron (set IsActive = false). Fails if the patron has active loans.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/loans", async Task<Results<Ok<IReadOnlyList<LoanResponse>>, NotFound>> (
            int id, [FromQuery] string? status, IPatronService service, CancellationToken ct) =>
        {
            var loans = await service.GetPatronLoansAsync(id, status, ct);
            return loans is not null ? TypedResults.Ok(loans) : TypedResults.NotFound();
        })
        .WithName("GetPatronLoans")
        .WithSummary("Get patron's loans")
        .WithDescription("Get patron's loans with optional filter by status (active, returned, overdue).")
        .Produces<IReadOnlyList<LoanResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async Task<Results<Ok<IReadOnlyList<ReservationResponse>>, NotFound>> (
            int id, IPatronService service, CancellationToken ct) =>
        {
            var reservations = await service.GetPatronReservationsAsync(id, ct);
            return reservations is not null ? TypedResults.Ok(reservations) : TypedResults.NotFound();
        })
        .WithName("GetPatronReservations")
        .WithSummary("Get patron's reservations")
        .WithDescription("Get patron's reservations.")
        .Produces<IReadOnlyList<ReservationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/fines", async Task<Results<Ok<IReadOnlyList<FineResponse>>, NotFound>> (
            int id, [FromQuery] string? status, IPatronService service, CancellationToken ct) =>
        {
            var fines = await service.GetPatronFinesAsync(id, status, ct);
            return fines is not null ? TypedResults.Ok(fines) : TypedResults.NotFound();
        })
        .WithName("GetPatronFines")
        .WithSummary("Get patron's fines")
        .WithDescription("Get patron's fines with optional filter by status (unpaid, paid, waived).")
        .Produces<IReadOnlyList<FineResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
