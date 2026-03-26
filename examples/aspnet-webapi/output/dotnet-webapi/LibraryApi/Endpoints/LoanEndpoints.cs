using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class LoanEndpoints
{
    public static void MapLoanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/loans")
            .WithTags("Loans");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<LoanResponse>>, BadRequest>> (
            string? status, bool? overdue, DateTime? fromDate, DateTime? toDate,
            int? page, int? pageSize,
            ILoanService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetAllAsync(status, overdue, fromDate, toDate, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetLoans")
        .WithSummary("List loans")
        .WithDescription("List loans with filter by status, overdue flag, date range, and pagination.")
        .Produces<PaginatedResponse<LoanResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<LoanResponse>, NotFound>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("GetLoanById")
        .WithSummary("Get loan by ID")
        .WithDescription("Returns loan details.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<LoanResponse>, BadRequest>> (
            CreateLoanRequest request, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.CheckoutAsync(request, ct);
            return TypedResults.Created($"/api/loans/{result.Id}", result);
        })
        .WithName("CreateLoan")
        .WithSummary("Check out a book")
        .WithDescription("Creates a loan (checks out a book) enforcing all checkout rules: availability, fine threshold, borrowing limits, and active membership.")
        .Produces<LoanResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/return", async Task<Results<Ok<LoanResponse>, NotFound, BadRequest>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.ReturnAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("ReturnLoan")
        .WithSummary("Return a book")
        .WithDescription("Processes a book return: updates loan status, increments available copies, generates overdue fines if applicable, and processes reservation queue.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/renew", async Task<Results<Ok<LoanResponse>, NotFound, BadRequest>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var result = await service.RenewAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("RenewLoan")
        .WithSummary("Renew a loan")
        .WithDescription("Renews a loan extending the due date. Limited to 2 renewals. Cannot renew if overdue or if there are pending reservations.")
        .Produces<LoanResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/overdue", async Task<Results<Ok<PaginatedResponse<LoanResponse>>, BadRequest>> (
            int? page, int? pageSize,
            ILoanService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetOverdueAsync(p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetOverdueLoans")
        .WithSummary("Get overdue loans")
        .WithDescription("Returns all currently overdue loans. Also flags active loans past their due date as Overdue.")
        .Produces<PaginatedResponse<LoanResponse>>();
    }
}
