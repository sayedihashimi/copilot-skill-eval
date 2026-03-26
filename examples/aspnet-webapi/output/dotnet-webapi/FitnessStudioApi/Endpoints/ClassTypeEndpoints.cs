using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static void MapClassTypeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/class-types")
            .WithTags("Class Types");

        group.MapGet("/", async (DifficultyLevel? difficulty, bool? isPremium,
            IClassTypeService service, CancellationToken ct) =>
        {
            var types = await service.GetAllAsync(difficulty, isPremium, ct);
            return TypedResults.Ok(types);
        })
        .WithName("GetClassTypes")
        .WithSummary("List class types")
        .WithDescription("List active class types with optional filtering by difficulty level and premium status.")
        .Produces<IReadOnlyList<ClassTypeResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<ClassTypeResponse>, NotFound>> (
            int id, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.GetByIdAsync(id, ct);
            return classType is null ? TypedResults.NotFound() : TypedResults.Ok(classType);
        })
        .WithName("GetClassTypeById")
        .WithSummary("Get a class type by ID")
        .WithDescription("Returns the full details of a specific class type.")
        .Produces<ClassTypeResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/class-types/{classType.Id}", classType);
        })
        .WithName("CreateClassType")
        .WithSummary("Create a new class type")
        .WithDescription("Create a new class type with difficulty level and premium status.")
        .Produces<ClassTypeResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<ClassTypeResponse>, NotFound>> (
            int id, UpdateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(classType);
        })
        .WithName("UpdateClassType")
        .WithSummary("Update a class type")
        .WithDescription("Update an existing class type's details.")
        .Produces<ClassTypeResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
