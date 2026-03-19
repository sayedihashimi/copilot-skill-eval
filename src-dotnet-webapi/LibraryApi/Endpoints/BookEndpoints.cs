using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class BookEndpoints
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] bool? available,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IBookService service,
            CancellationToken ct) =>
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize == 0 ? 10 : pageSize, 1, 100);
            return TypedResults.Ok(await service.GetBooksAsync(search, available, sortBy, sortOrder, page, pageSize, ct));
        })
        .WithName("GetBooks")
        .WithSummary("List books")
        .WithDescription("List books with search, filter by availability, pagination, and sorting.")
        .Produces<PaginatedResponse<BookResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<BookDetailResponse>, NotFound>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var book = await service.GetBookByIdAsync(id, ct);
            return book is not null ? TypedResults.Ok(book) : TypedResults.NotFound();
        })
        .WithName("GetBookById")
        .WithSummary("Get book details")
        .WithDescription("Get book details including authors, categories, and availability info.")
        .Produces<BookDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<BookResponse>> (
            CreateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var book = await service.CreateBookAsync(request, ct);
            return TypedResults.Created($"/api/books/{book.Id}", book);
        })
        .WithName("CreateBook")
        .WithSummary("Create a new book")
        .WithDescription("Create a new book with author IDs and category IDs.")
        .Produces<BookResponse>(StatusCodes.Status201Created);

        group.MapPut("/{id:int}", async Task<Results<Ok<BookResponse>, NotFound>> (
            int id, UpdateBookRequest request, IBookService service, CancellationToken ct) =>
        {
            var book = await service.UpdateBookAsync(id, request, ct);
            return book is not null ? TypedResults.Ok(book) : TypedResults.NotFound();
        })
        .WithName("UpdateBook")
        .WithSummary("Update a book")
        .WithDescription("Update an existing book record.")
        .Produces<BookResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var (found, hasActiveLoans) = await service.DeleteBookAsync(id, ct);
            if (!found) return TypedResults.NotFound();
            if (hasActiveLoans) return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Cannot delete book",
                Detail = "Book has active loans and cannot be deleted.",
                Status = StatusCodes.Status409Conflict
            });
            return TypedResults.NoContent();
        })
        .WithName("DeleteBook")
        .WithSummary("Delete a book")
        .WithDescription("Delete a book. Fails if the book has active loans.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/loans", async Task<Results<Ok<IReadOnlyList<LoanResponse>>, NotFound>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var loans = await service.GetBookLoansAsync(id, ct);
            return loans is not null ? TypedResults.Ok(loans) : TypedResults.NotFound();
        })
        .WithName("GetBookLoans")
        .WithSummary("Get book loan history")
        .WithDescription("Get the loan history for a specific book.")
        .Produces<IReadOnlyList<LoanResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/reservations", async Task<Results<Ok<IReadOnlyList<ReservationResponse>>, NotFound>> (
            int id, IBookService service, CancellationToken ct) =>
        {
            var reservations = await service.GetBookReservationsAsync(id, ct);
            return reservations is not null ? TypedResults.Ok(reservations) : TypedResults.NotFound();
        })
        .WithName("GetBookReservations")
        .WithSummary("Get book reservations")
        .WithDescription("Get the active reservations queue for a specific book.")
        .Produces<IReadOnlyList<ReservationResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
