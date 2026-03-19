using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static void MapClassTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/class-types").WithTags("Class Types");

        group.MapGet("/", async Task<Ok<PaginatedResponse<ClassTypeResponse>>> (
            IClassTypeService service,
            string? difficulty = null, bool? isPremium = null,
            int page = 1, int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await service.GetAllAsync(difficulty, isPremium, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetClassTypes")
        .WithSummary("List class types")
        .WithDescription("Returns a paginated list of class types with optional difficulty and premium filters.")
        .Produces<PaginatedResponse<ClassTypeResponse>>(200);

        group.MapGet("/{id:int}", async Task<Results<Ok<ClassTypeResponse>, NotFound>> (
            int id, IClassTypeService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetClassType")
        .WithSummary("Get class type details")
        .WithDescription("Returns details of a specific class type by ID.")
        .Produces<ClassTypeResponse>(200)
        .Produces(404);

        group.MapPost("/", async Task<Created<ClassTypeResponse>> (
            CreateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/class-types/{result.Id}", result);
        })
        .WithName("CreateClassType")
        .WithSummary("Create a new class type")
        .WithDescription("Creates a new class type for scheduling.")
        .Produces<ClassTypeResponse>(201);

        group.MapPut("/{id:int}", async Task<Results<Ok<ClassTypeResponse>, NotFound>> (
            int id, UpdateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("UpdateClassType")
        .WithSummary("Update a class type")
        .WithDescription("Updates an existing class type.")
        .Produces<ClassTypeResponse>(200)
        .Produces(404);
    }
}
