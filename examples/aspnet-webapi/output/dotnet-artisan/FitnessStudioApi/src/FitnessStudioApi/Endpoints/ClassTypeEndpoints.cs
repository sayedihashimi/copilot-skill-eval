using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static RouteGroupBuilder MapClassTypeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/class-types")
            .WithTags("Class Types");

        group.MapGet("/", async (
            IClassTypeService service,
            CancellationToken ct,
            [FromQuery] string? difficulty = null,
            [FromQuery] bool? isPremium = null) =>
            TypedResults.Ok(await service.GetAllAsync(difficulty, isPremium, ct)))
            .WithSummary("List class types with filtering");

        group.MapGet("/{id:int}", async (int id, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.GetByIdAsync(id, ct);
            return classType is not null ? Results.Ok(classType) : Results.NotFound();
        }).WithSummary("Get class type details");

        group.MapPost("/", async (CreateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/class-types/{classType.Id}", classType);
        }).WithSummary("Create a new class type");

        group.MapPut("/{id:int}", async (int id, UpdateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.UpdateAsync(id, request, ct);
            return classType is not null ? Results.Ok(classType) : Results.NotFound();
        }).WithSummary("Update a class type");

        return group;
    }
}
