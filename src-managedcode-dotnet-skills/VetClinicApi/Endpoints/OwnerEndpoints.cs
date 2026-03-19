using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/owners").WithTags("Owners");

        group.MapGet("/", GetAll).WithName("GetOwners").WithSummary("Get all owners with pagination");
        group.MapGet("/{id:int}", GetById).WithName("GetOwnerById").WithSummary("Get owner by ID");
        group.MapPost("/", Create).WithName("CreateOwner").WithSummary("Create a new owner");
        group.MapPut("/{id:int}", Update).WithName("UpdateOwner").WithSummary("Update an existing owner");
        group.MapDelete("/{id:int}", Delete).WithName("DeleteOwner").WithSummary("Delete an owner");
        group.MapGet("/{id:int}/pets", GetPets).WithName("GetOwnerPets").WithSummary("Get pets for an owner");
        group.MapGet("/{id:int}/appointments", GetAppointments).WithName("GetOwnerAppointments").WithSummary("Get appointments for an owner");

        return group;
    }

    private static async Task<IResult> GetAll(IOwnerService service, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 50) pageSize = 10;
        return TypedResults.Ok(await service.GetAllAsync(page, pageSize, ct));
    }

    private static async Task<IResult> GetById(int id, IOwnerService service, CancellationToken ct)
    {
        var owner = await service.GetByIdAsync(id, ct);
        return owner is not null ? TypedResults.Ok(owner) : TypedResults.NotFound();
    }

    private static async Task<IResult> Create(CreateOwnerRequest request, IOwnerService service, CancellationToken ct)
    {
        var owner = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/owners/{owner.Id}", owner);
    }

    private static async Task<IResult> Update(int id, UpdateOwnerRequest request, IOwnerService service, CancellationToken ct)
    {
        var owner = await service.UpdateAsync(id, request, ct);
        return owner is not null ? TypedResults.Ok(owner) : TypedResults.NotFound();
    }

    private static async Task<IResult> Delete(int id, IOwnerService service, CancellationToken ct)
    {
        var deleted = await service.DeleteAsync(id, ct);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static async Task<IResult> GetPets(int id, IOwnerService service, CancellationToken ct)
    {
        if (await service.GetByIdAsync(id, ct) is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(await service.GetPetsAsync(id, ct));
    }

    private static async Task<IResult> GetAppointments(int id, IOwnerService service, CancellationToken ct)
    {
        if (await service.GetByIdAsync(id, ct) is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(await service.GetAppointmentsAsync(id, ct));
    }
}
