using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Endpoints;

public static class ClassTypeEndpoints
{
    public static RouteGroupBuilder MapClassTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/class-types")
            .WithTags("Class Types");

        group.MapGet("/", async (IClassTypeService service, CancellationToken ct) =>
        {
            var classTypes = await service.GetAllAsync(ct);
            return TypedResults.Ok(classTypes);
        })
        .WithName("GetClassTypes")
        .WithSummary("List all active class types");

        group.MapGet("/{id:int}", async (int id, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.GetByIdAsync(id, ct);
            return classType is not null ? Results.Ok(classType) : Results.NotFound();
        })
        .WithName("GetClassType")
        .WithSummary("Get class type details");

        group.MapPost("/", async (CreateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/class-types/{classType.Id}", classType);
        })
        .WithName("CreateClassType")
        .WithSummary("Create a new class type");

        group.MapPut("/{id:int}", async (int id, UpdateClassTypeRequest request, IClassTypeService service, CancellationToken ct) =>
        {
            var classType = await service.UpdateAsync(id, request, ct);
            return classType is not null ? Results.Ok(classType) : Results.NotFound();
        })
        .WithName("UpdateClassType")
        .WithSummary("Update a class type");

        return group;
    }
}
