using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class ReservationEndpoints
{
    public static void MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reservations")
            .WithTags("Reservations");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<ReservationResponse>>, BadRequest>> (
            string? status, int? page, int? pageSize,
            IReservationService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetAllAsync(status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetReservations")
        .WithSummary("List reservations")
        .WithDescription("List reservations with optional status filter and pagination.")
        .Produces<PaginatedResponse<ReservationResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<ReservationResponse>, NotFound>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("GetReservationById")
        .WithSummary("Get reservation by ID")
        .WithDescription("Returns reservation details.")
        .Produces<ReservationResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<ReservationResponse>, BadRequest>> (
            CreateReservationRequest request, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/reservations/{result.Id}", result);
        })
        .WithName("CreateReservation")
        .WithSummary("Create a reservation")
        .WithDescription("Creates a reservation for a book. Patron cannot reserve a book they already have on active loan.")
        .Produces<ReservationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/cancel", async Task<Results<Ok<ReservationResponse>, NotFound, BadRequest>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.CancelAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("CancelReservation")
        .WithSummary("Cancel a reservation")
        .WithDescription("Cancels a pending or ready reservation.")
        .Produces<ReservationResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/fulfill", async Task<Results<Ok<LoanResponse>, NotFound, BadRequest>> (
            int id, IReservationService service, CancellationToken ct) =>
        {
            var result = await service.FulfillAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("FulfillReservation")
        .WithSummary("Fulfill a reservation")
        .WithDescription("Fulfills a 'Ready' reservation by creating a loan for the patron.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
