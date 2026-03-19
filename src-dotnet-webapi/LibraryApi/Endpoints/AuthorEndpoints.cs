using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class AuthorEndpoints
{
    public static void MapAuthorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IAuthorService service,
            CancellationToken ct) =>
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize == 0 ? 10 : pageSize, 1, 100);
            return TypedResults.Ok(await service.GetAuthorsAsync(search, page, pageSize, ct));
        })
        .WithName("GetAuthors")
        .WithSummary("List authors")
        .WithDescription("List authors with optional search by name and pagination.")
        .Produces<PaginatedResponse<AuthorResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<AuthorDetailResponse>, NotFound>> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            var author = await service.GetAuthorByIdAsync(id, ct);
            return author is not null ? TypedResults.Ok(author) : TypedResults.NotFound();
        })
        .WithName("GetAuthorById")
        .WithSummary("Get author details")
        .WithDescription("Get author details including their books.")
        .Produces<AuthorDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<AuthorResponse>> (
            CreateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var author = await service.CreateAuthorAsync(request, ct);
            return TypedResults.Created($"/api/authors/{author.Id}", author);
        })
        .WithName("CreateAuthor")
        .WithSummary("Create a new author")
        .WithDescription("Create a new author record.")
        .Produces<AuthorResponse>(StatusCodes.Status201Created);

        group.MapPut("/{id:int}", async Task<Results<Ok<AuthorResponse>, NotFound>> (
            int id, UpdateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var author = await service.UpdateAuthorAsync(id, request, ct);
            return author is not null ? TypedResults.Ok(author) : TypedResults.NotFound();
        })
        .WithName("UpdateAuthor")
        .WithSummary("Update an author")
        .WithDescription("Update an existing author record.")
        .Produces<AuthorResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            var (found, hasBooks) = await service.DeleteAuthorAsync(id, ct);
            if (!found) return TypedResults.NotFound();
            if (hasBooks) return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Cannot delete author",
                Detail = "Author has associated books and cannot be deleted.",
                Status = StatusCodes.Status409Conflict
            });
            return TypedResults.NoContent();
        })
        .WithName("DeleteAuthor")
        .WithSummary("Delete an author")
        .WithDescription("Delete an author. Fails if the author has any books.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
