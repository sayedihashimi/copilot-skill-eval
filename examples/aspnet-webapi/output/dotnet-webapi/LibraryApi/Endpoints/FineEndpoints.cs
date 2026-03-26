using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class FineEndpoints
{
    public static void MapFineEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/fines")
            .WithTags("Fines");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<FineResponse>>, BadRequest>> (
            string? status, int? page, int? pageSize,
            IFineService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetAllAsync(status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetFines")
        .WithSummary("List fines")
        .WithDescription("List fines with optional status filter and pagination.")
        .Produces<PaginatedResponse<FineResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<FineResponse>, NotFound>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("GetFineById")
        .WithSummary("Get fine by ID")
        .WithDescription("Returns fine details.")
        .Produces<FineResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/pay", async Task<Results<Ok<FineResponse>, NotFound, BadRequest>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.PayAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("PayFine")
        .WithSummary("Pay a fine")
        .WithDescription("Marks a fine as paid.")
        .Produces<FineResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/waive", async Task<Results<Ok<FineResponse>, NotFound, BadRequest>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var result = await service.WaiveAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("WaiveFine")
        .WithSummary("Waive a fine")
        .WithDescription("Waives a fine (sets status to Waived).")
        .Produces<FineResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
