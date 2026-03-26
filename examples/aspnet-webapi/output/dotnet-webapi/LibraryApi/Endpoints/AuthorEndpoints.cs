using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class AuthorEndpoints
{
    public static void MapAuthorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/authors")
            .WithTags("Authors");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<AuthorResponse>>, BadRequest>> (
            string? search, int? page, int? pageSize,
            IAuthorService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetAllAsync(search, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAuthors")
        .WithSummary("List authors")
        .WithDescription("List authors with optional search by name and pagination.")
        .Produces<PaginatedResponse<AuthorResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<AuthorDetailResponse>, NotFound>> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("GetAuthorById")
        .WithSummary("Get author by ID")
        .WithDescription("Returns author details including their books.")
        .Produces<AuthorDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<AuthorResponse>, BadRequest>> (
            CreateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/authors/{result.Id}", result);
        })
        .WithName("CreateAuthor")
        .WithSummary("Create a new author")
        .WithDescription("Creates a new author record.")
        .Produces<AuthorResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<AuthorResponse>, NotFound, BadRequest>> (
            int id, UpdateAuthorRequest request, IAuthorService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("UpdateAuthor")
        .WithSummary("Update an author")
        .WithDescription("Updates an existing author's information.")
        .Produces<AuthorResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> (
            int id, IAuthorService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteAuthor")
        .WithSummary("Delete an author")
        .WithDescription("Deletes an author. Fails if the author has any associated books.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
