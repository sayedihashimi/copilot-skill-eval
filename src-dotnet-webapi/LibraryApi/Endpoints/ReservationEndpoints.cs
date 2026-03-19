using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class ReservationEndpoints
{
    public static void MapReservationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reservations").WithTags("Reservations");

        group.MapGet("/", async (
            [FromQuery] string? status,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IReservationService service,
            CancellationToken ct) =>
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize == 0 ? 10 : pageSize, 1, 100);
            return TypedResults.Ok(await service.GetReservationsAsync(status, page, pageSize, ct));
        })
        .WithName("GetReservations")
        .WithSummary("List reservations")
        .WithDescription("List reservations with optional filter by status and pagination.")
        .Produces<PaginatedResponse<ReservationResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<ReservationResponse>, NotFound>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var reservation = await service.GetReservationByIdAsync(id, ct);
            return reservation is not null ? TypedResults.Ok(reservation) : TypedResults.NotFound();
        })
        .WithName("GetReservationById")
        .WithSummary("Get reservation details")
        .WithDescription("Get details for a specific reservation.")
        .Produces<ReservationResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<ReservationResponse>, BadRequest<ProblemDetails>>> (
            CreateReservationRequest request, IReservationService service, CancellationToken ct) =>
        {
            var (reservation, error) = await service.CreateReservationAsync(request, ct);
            if (error is not null)
                return TypedResults.BadRequest(new ProblemDetails
                {
                    Title = "Reservation denied",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            return TypedResults.Created($"/api/reservations/{reservation!.Id}", reservation);
        })
        .WithName("CreateReservation")
        .WithSummary("Create a reservation")
        .WithDescription("Create a reservation enforcing all reservation rules.")
        .Produces<ReservationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/cancel", async Task<Results<Ok<ReservationResponse>, NotFound, BadRequest<ProblemDetails>>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var (reservation, error, notFound) = await service.CancelReservationAsync(id, ct);
            if (notFound) return TypedResults.NotFound();
            if (error is not null)
                return TypedResults.BadRequest(new ProblemDetails
                {
                    Title = "Cancellation denied",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            return TypedResults.Ok(reservation!);
        })
        .WithName("CancelReservation")
        .WithSummary("Cancel a reservation")
        .WithDescription("Cancel a pending or ready reservation.")
        .Produces<ReservationResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/fulfill", async Task<Results<Ok<LoanResponse>, NotFound, BadRequest<ProblemDetails>>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var (loan, error, notFound) = await service.FulfillReservationAsync(id, ct);
            if (notFound) return TypedResults.NotFound();
            if (error is not null)
                return TypedResults.BadRequest(new ProblemDetails
                {
                    Title = "Fulfillment denied",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            return TypedResults.Ok(loan!);
        })
        .WithName("FulfillReservation")
        .WithSummary("Fulfill a reservation")
        .WithDescription("Fulfill a 'Ready' reservation — creates a loan for the patron.")
        .Produces<LoanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
