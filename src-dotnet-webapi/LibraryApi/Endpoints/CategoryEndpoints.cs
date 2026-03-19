using LibraryApi.DTOs;
using LibraryApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories");

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ICategoryService service,
            CancellationToken ct) =>
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize == 0 ? 10 : pageSize, 1, 100);
            return TypedResults.Ok(await service.GetCategoriesAsync(page, pageSize, ct));
        })
        .WithName("GetCategories")
        .WithSummary("List all categories")
        .WithDescription("List all categories with pagination.")
        .Produces<PaginatedResponse<CategoryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", async Task<Results<Ok<CategoryDetailResponse>, NotFound>> (
            int id, ICategoryService service, CancellationToken ct) =>
        {
            var category = await service.GetCategoryByIdAsync(id, ct);
            return category is not null ? TypedResults.Ok(category) : TypedResults.NotFound();
        })
        .WithName("GetCategoryById")
        .WithSummary("Get category details")
        .WithDescription("Get category details with count of books in the category.")
        .Produces<CategoryDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Created<CategoryResponse>> (
            CreateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var category = await service.CreateCategoryAsync(request, ct);
            return TypedResults.Created($"/api/categories/{category.Id}", category);
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category")
        .WithDescription("Create a new category record.")
        .Produces<CategoryResponse>(StatusCodes.Status201Created);

        group.MapPut("/{id:int}", async Task<Results<Ok<CategoryResponse>, NotFound>> (
            int id, UpdateCategoryRequest request, ICategoryService service, CancellationToken ct) =>
        {
            var category = await service.UpdateCategoryAsync(id, request, ct);
            return category is not null ? TypedResults.Ok(category) : TypedResults.NotFound();
        })
        .WithName("UpdateCategory")
        .WithSummary("Update a category")
        .WithDescription("Update an existing category record.")
        .Produces<CategoryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> (
            int id, ICategoryService service, CancellationToken ct) =>
        {
            var (found, hasBooks) = await service.DeleteCategoryAsync(id, ct);
            if (!found) return TypedResults.NotFound();
            if (hasBooks) return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Cannot delete category",
                Detail = "Category has associated books and cannot be deleted.",
                Status = StatusCodes.Status409Conflict
            });
            return TypedResults.NoContent();
        })
        .WithName("DeleteCategory")
        .WithSummary("Delete a category")
        .WithDescription("Delete a category. Fails if the category has any books.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
