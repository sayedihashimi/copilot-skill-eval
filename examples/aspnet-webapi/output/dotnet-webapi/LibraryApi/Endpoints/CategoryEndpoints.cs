using LibraryApi.DTOs;
using LibraryApi.Services;

namespace LibraryApi.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<CategoryResponse>>, BadRequest>> (
            int? page, int? pageSize,
            ICategoryService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 10, 1, 100);
            var result = await service.GetAllAsync(p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetCategories")
        .WithSummary("List all categories")
        .WithDescription("Returns all categories with pagination.")
        .Produces<PaginatedResponse<CategoryResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<CategoryDetailResponse>, NotFound>> (
            int id, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("GetCategoryById")
        .WithSummary("Get category by ID")
        .WithDescription("Returns category details with count of associated books.")
        .Produces<CategoryDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<CategoryResponse>, BadRequest, Conflict<ProblemDetails>>> (
            CreateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/categories/{result.Id}", result);
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category")
        .WithDescription("Creates a new category. Name must be unique.")
        .Produces<CategoryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<CategoryResponse>, NotFound, BadRequest>> (
            int id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
        })
        .WithName("UpdateCategory")
        .WithSummary("Update a category")
        .WithDescription("Updates an existing category.")
        .Produces<CategoryResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> (
            int id, ICategoryService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteCategory")
        .WithSummary("Delete a category")
        .WithDescription("Deletes a category. Fails if the category has any associated books.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
