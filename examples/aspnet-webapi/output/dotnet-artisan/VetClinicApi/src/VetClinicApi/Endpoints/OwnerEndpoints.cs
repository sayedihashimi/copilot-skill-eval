using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class OwnerEndpoints
{
    public static RouteGroupBuilder MapOwnerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/owners").WithTags("Owners");

        group.MapGet("/", GetAll).WithSummary("List all owners with search and pagination");
        group.MapGet("/{id:int}", GetById).WithSummary("Get owner by ID with their pets");
        group.MapPost("/", Create).WithSummary("Create a new owner");
        group.MapPut("/{id:int}", Update).WithSummary("Update an existing owner");
        group.MapDelete("/{id:int}", Delete).WithSummary("Delete an owner");
        group.MapGet("/{id:int}/pets", GetPets).WithSummary("Get all pets for an owner");
        group.MapGet("/{id:int}/appointments", GetAppointments).WithSummary("Get appointment history for an owner's pets");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<OwnerDto>>> GetAll(
        IOwnerService service,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(search, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<OwnerDetailDto>, NotFound>> GetById(
        int id, IOwnerService service, CancellationToken ct = default)
    {
        var owner = await service.GetByIdAsync(id, ct);
        return owner is not null
            ? TypedResults.Ok(owner)
            : TypedResults.NotFound();
    }

    private static async Task<Created<OwnerDto>> Create(
        CreateOwnerDto dto, IOwnerService service, CancellationToken ct = default)
    {
        var owner = await service.CreateAsync(dto, ct);
        return TypedResults.Created($"/api/owners/{owner.Id}", owner);
    }

    private static async Task<Results<Ok<OwnerDto>, NotFound>> Update(
        int id, UpdateOwnerDto dto, IOwnerService service, CancellationToken ct = default)
    {
        var owner = await service.UpdateAsync(id, dto, ct);
        return owner is not null
            ? TypedResults.Ok(owner)
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound, Conflict<ProblemDetails>>> Delete(
        int id, IOwnerService service, CancellationToken ct = default)
    {
        var (success, error) = await service.DeleteAsync(id, ct);
        if (success)
        {
            return TypedResults.NoContent();
        }

        if (error == "Owner not found")
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Conflict(new ProblemDetails
        {
            Title = "Cannot delete owner",
            Detail = error,
            Status = StatusCodes.Status409Conflict
        });
    }

    private static async Task<Ok<IReadOnlyList<PetDto>>> GetPets(
        int id, IOwnerService service, CancellationToken ct = default)
    {
        var pets = await service.GetPetsAsync(id, ct);
        return TypedResults.Ok(pets);
    }

    private static async Task<Ok<PaginatedResponse<AppointmentDto>>> GetAppointments(
        int id, IOwnerService service,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var appointments = await service.GetAppointmentsAsync(id, page, pageSize, ct);
        return TypedResults.Ok(appointments);
    }
}
