using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static void MapOwnerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/owners").WithTags("Owners");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<OwnerResponse>>, BadRequest>> (
            string? search, int? page, int? pageSize,
            IOwnerService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(search, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAllOwners")
        .WithSummary("List all owners")
        .WithDescription("Returns a paginated list of owners. Supports search by name or email.")
        .Produces<PaginatedResponse<OwnerResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<OwnerResponse>, NotFound>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.GetByIdAsync(id, ct);
            return owner is null ? TypedResults.NotFound() : TypedResults.Ok(owner);
        })
        .WithName("GetOwnerById")
        .WithSummary("Get owner by ID")
        .WithDescription("Returns owner details including their pets.")
        .Produces<OwnerResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<OwnerResponse>, BadRequest>> (
            CreateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/owners/{owner.Id}", owner);
        })
        .WithName("CreateOwner")
        .WithSummary("Create a new owner")
        .WithDescription("Creates a new pet owner record.")
        .Produces<OwnerResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<OwnerResponse>, NotFound, BadRequest>> (
            int id, UpdateOwnerRequest request, IOwnerService service, CancellationToken ct) =>
        {
            var owner = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(owner);
        })
        .WithName("UpdateOwner")
        .WithSummary("Update an existing owner")
        .WithDescription("Updates all fields of an existing owner.")
        .Produces<OwnerResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound, Conflict>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteOwner")
        .WithSummary("Delete an owner")
        .WithDescription("Deletes an owner. Fails if the owner has active pets.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/pets", async Task<Results<Ok<IReadOnlyList<PetResponse>>, NotFound>> (
            int id, IOwnerService service, CancellationToken ct) =>
        {
            var pets = await service.GetPetsByOwnerIdAsync(id, ct);
            return TypedResults.Ok(pets);
        })
        .WithName("GetOwnerPets")
        .WithSummary("Get all pets for an owner")
        .WithDescription("Returns all pets belonging to the specified owner.")
        .Produces<IReadOnlyList<PetResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/appointments", async Task<Results<Ok<PaginatedResponse<AppointmentResponse>>, NotFound>> (
            int id, int? page, int? pageSize,
            IOwnerService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetOwnerAppointmentsAsync(id, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetOwnerAppointments")
        .WithSummary("Get appointment history for an owner's pets")
        .WithDescription("Returns all appointments for all pets belonging to the specified owner.")
        .Produces<PaginatedResponse<AppointmentResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
