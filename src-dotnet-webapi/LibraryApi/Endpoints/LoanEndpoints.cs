using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class LoanEndpoints
{
    public static void MapLoanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/loans").WithTags("Loans");

        group.MapGet("/", async (
            [FromQuery] string? status,
            [FromQuery] bool? overdue,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ILoanService service,
            CancellationToken ct) =>
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize == 0 ? 10 : pageSize, 1, 100);
            return TypedResults.Ok(await service.GetLoansAsync(status, overdue, fromDate, toDate, page, pageSize, ct));
        })
        .WithName("GetLoans")
        .WithSummary("List loans")
        .WithDescription("List loans with filter by status, overdue flag, date range, and pagination.")
        .Produces<PaginatedResponse<LoanResponse>>(StatusCodes.Status200OK);

        group.MapGet("/overdue", async (ILoanService service, CancellationToken ct) =>
        {
            return TypedResults.Ok(await service.GetOverdueLoansAsync(ct));
        })
        .WithName("GetOverdueLoans")
        .WithSummary("Get overdue loans")
        .WithDescription("Get all currently overdue loans. Also flags any newly overdue loans.")
        .Produces<IReadOnlyList<LoanResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<LoanResponse>, NotFound>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var loan = await service.GetLoanByIdAsync(id, ct);
            return loan is not null ? TypedResults.Ok(loan) : TypedResults.NotFound();
        })
        .WithName("GetLoanById")
        .WithSummary("Get loan details")
        .WithDescription("Get details for a specific loan.")
        .Produces<LoanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<LoanResponse>, BadRequest<ProblemDetails>>> (
            CreateLoanRequest request, ILoanService service, CancellationToken ct) =>
        {
            var (loan, error) = await service.CheckoutBookAsync(request, ct);
            if (error is not null)
                return TypedResults.BadRequest(new ProblemDetails
                {
                    Title = "Checkout denied",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            return TypedResults.Created($"/api/loans/{loan!.Id}", loan);
        })
        .WithName("CheckoutBook")
        .WithSummary("Check out a book")
        .WithDescription("Create a loan — check out a book enforcing all checkout rules.")
        .Produces<LoanResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/return", async Task<Results<Ok<LoanResponse>, NotFound, BadRequest<ProblemDetails>>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var (loan, error, notFound) = await service.ReturnBookAsync(id, ct);
            if (notFound) return TypedResults.NotFound();
            if (error is not null)
                return TypedResults.BadRequest(new ProblemDetails
                {
                    Title = "Return failed",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            return TypedResults.Ok(loan!);
        })
        .WithName("ReturnBook")
        .WithSummary("Return a book")
        .WithDescription("Return a book — enforce all return processing rules including fine generation.")
        .Produces<LoanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:int}/renew", async Task<Results<Ok<LoanResponse>, NotFound, BadRequest<ProblemDetails>>> (
            int id, ILoanService service, CancellationToken ct) =>
        {
            var (loan, error, notFound) = await service.RenewLoanAsync(id, ct);
            if (notFound) return TypedResults.NotFound();
            if (error is not null)
                return TypedResults.BadRequest(new ProblemDetails
                {
                    Title = "Renewal denied",
                    Detail = error,
                    Status = StatusCodes.Status400BadRequest
                });
            return TypedResults.Ok(loan!);
        })
        .WithName("RenewLoan")
        .WithSummary("Renew a loan")
        .WithDescription("Renew a loan — enforce all renewal rules.")
        .Produces<LoanResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
