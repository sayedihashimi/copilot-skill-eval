using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static RouteGroupBuilder MapPetEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/pets").WithTags("Pets");

        group.MapGet("/", GetAll).WithSummary("List all active pets with search, species filter, and pagination");
        group.MapGet("/{id:int}", GetById).WithSummary("Get pet by ID with owner info");
        group.MapPost("/", Create).WithSummary("Create a new pet");
        group.MapPut("/{id:int}", Update).WithSummary("Update pet details including owner transfer");
        group.MapDelete("/{id:int}", SoftDelete).WithSummary("Soft-delete a pet (set IsActive = false)");
        group.MapGet("/{id:int}/medical-records", GetMedicalRecords).WithSummary("Get all medical records for a pet");
        group.MapGet("/{id:int}/vaccinations", GetVaccinations).WithSummary("Get all vaccinations for a pet");
        group.MapGet("/{id:int}/vaccinations/upcoming", GetUpcomingVaccinations).WithSummary("Get vaccinations that are due soon or overdue");
        group.MapGet("/{id:int}/prescriptions/active", GetActivePrescriptions).WithSummary("Get active prescriptions for a pet");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<PetDto>>> GetAll(
        IPetService service,
        [FromQuery] string? search = null,
        [FromQuery] string? species = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(search, species, includeInactive, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<PetDetailDto>, NotFound>> GetById(
        int id, IPetService service, CancellationToken ct = default)
    {
        var pet = await service.GetByIdAsync(id, ct);
        return pet is not null
            ? TypedResults.Ok(pet)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<PetDto>, BadRequest<ProblemDetails>>> Create(
        CreatePetDto dto, IPetService service, CancellationToken ct = default)
    {
        var pet = await service.CreateAsync(dto, ct);
        if (pet is null)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Invalid owner",
                Detail = "The specified owner does not exist",
                Status = StatusCodes.Status400BadRequest
            });
        }
        return TypedResults.Created($"/api/pets/{pet.Id}", pet);
    }

    private static async Task<Results<Ok<PetDto>, NotFound>> Update(
        int id, UpdatePetDto dto, IPetService service, CancellationToken ct = default)
    {
        var pet = await service.UpdateAsync(id, dto, ct);
        return pet is not null
            ? TypedResults.Ok(pet)
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> SoftDelete(
        int id, IPetService service, CancellationToken ct = default)
    {
        var result = await service.SoftDeleteAsync(id, ct);
        return result
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<Ok<IReadOnlyList<MedicalRecordDto>>> GetMedicalRecords(
        int id, IPetService service, CancellationToken ct = default)
    {
        var records = await service.GetMedicalRecordsAsync(id, ct);
        return TypedResults.Ok(records);
    }

    private static async Task<Ok<IReadOnlyList<VaccinationDto>>> GetVaccinations(
        int id, IPetService service, CancellationToken ct = default)
    {
        var vaccinations = await service.GetVaccinationsAsync(id, ct);
        return TypedResults.Ok(vaccinations);
    }

    private static async Task<Ok<IReadOnlyList<VaccinationDto>>> GetUpcomingVaccinations(
        int id, IPetService service, CancellationToken ct = default)
    {
        var vaccinations = await service.GetUpcomingVaccinationsAsync(id, ct);
        return TypedResults.Ok(vaccinations);
    }

    private static async Task<Ok<IReadOnlyList<PrescriptionDto>>> GetActivePrescriptions(
        int id, IPetService service, CancellationToken ct = default)
    {
        var prescriptions = await service.GetActivePrescriptionsAsync(id, ct);
        return TypedResults.Ok(prescriptions);
    }
}
