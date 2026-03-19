using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class FineEndpoints
{
    public static void MapFineEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/fines").WithTags("Fines");

        group.MapGet("/", async (
            [FromQuery] string? status,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IFineService service,
            CancellationToken ct) =>
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize == 0 ? 10 : pageSize, 1, 100);
            return TypedResults.Ok(await service.GetFinesAsync(status, page, pageSize, ct));
        })
        .WithName("GetFines")
        .WithSummary("List fines")
        .WithDescription("List fines with optional filter by status and pagination.")
        .Produces<PaginatedResponse<FineResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<FineResponse>, NotFound>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var fine = await service.GetFineByIdAsync(id, ct);
            return fine is not null ? TypedResults.Ok(fine) : TypedResults.NotFound();
        })
        .WithName("GetFineById")
        .WithSummary("Get fine details")
        .WithDescription("Get details for a specific fine.")
        .Produces<FineResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/pay", async Task<Results<Ok<FineResponse>, NotFound, BadRequest<ProblemDetails>>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var (fine, error, notFound) = await service.PayFineAsync(id, ct);
            if (notFound) return TypedResults.NotFound();
            if (error is not null)
                return TypedResults.BadRequest(new ProblemDetails
                {
                    Title = "Payment failed",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            return TypedResults.Ok(fine!);
        })
        .WithName("PayFine")
        .WithSummary("Pay a fine")
        .WithDescription("Pay a fine — sets PaidDate and updates status to Paid.")
        .Produces<FineResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/waive", async Task<Results<Ok<FineResponse>, NotFound, BadRequest<ProblemDetails>>> (
            int id, IFineService service, CancellationToken ct) =>
        {
            var (fine, error, notFound) = await service.WaiveFineAsync(id, ct);
            if (notFound) return TypedResults.NotFound();
            if (error is not null)
                return TypedResults.BadRequest(new ProblemDetails
                {
                    Title = "Waive failed",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            return TypedResults.Ok(fine!);
        })
        .WithName("WaiveFine")
        .WithSummary("Waive a fine")
        .WithDescription("Waive a fine — updates status to Waived.")
        .Produces<FineResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
