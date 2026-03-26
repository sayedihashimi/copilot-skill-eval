using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books")
            .WithTags("Books");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<BookResponse>>, BadRequest>> (
            string? search, bool? available, string? sortBy, string? sortDirection,
            int? page, int? pageSize,
            IBookService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetAllAsync(search, available, sortBy, sortDirection, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetBooks")
        .WithSummary("List books")
        .WithDescription("List books with search, availability filter, sorting, and pagination.")
        .Produces<PaginatedResponse<BookResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<BookResponse>, NotFound>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("GetBookById")
        .WithSummary("Get book by ID")
        .WithDescription("Returns book details including authors, categories, and availability.")
        .Produces<BookResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<BookResponse>, BadRequest>> (
            CreateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/books/{result.Id}", result);
        })
        .WithName("CreateBook")
        .WithSummary("Create a new book")
        .WithDescription("Creates a new book with author and category associations.")
        .Produces<BookResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<BookResponse>, NotFound, BadRequest>> (
            int id, UpdateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("UpdateBook")
        .WithSummary("Update a book")
        .WithDescription("Updates an existing book's details.")
        .Produces<BookResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteBook")
        .WithSummary("Delete a book")
        .WithDescription("Deletes a book. Fails if the book has any active loans.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/loans", async Task<Results<Ok<PaginatedResponse<LoanResponse>>, NotFound>> (
            int id, int? page, int? pageSize,
            IBookService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetBookLoansAsync(id, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetBookLoans")
        .WithSummary("Get book loan history")
        .WithDescription("Returns loan history for a specific book.")
        .Produces<PaginatedResponse<LoanResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async Task<Results<Ok<PaginatedResponse<ReservationResponse>>, NotFound>> (
            int id, int? page, int? pageSize,
            IBookService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetBookReservationsAsync(id, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetBookReservations")
        .WithSummary("Get book reservations")
        .WithDescription("Returns active reservation queue for a specific book.")
        .Produces<PaginatedResponse<ReservationResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
