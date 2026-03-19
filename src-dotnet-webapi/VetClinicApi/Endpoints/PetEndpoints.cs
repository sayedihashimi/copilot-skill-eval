using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static void MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets").WithTags("Pets");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<PetResponse>>, BadRequest>> (
            string? name, string? species, bool? includeInactive, int? page, int? pageSize,
            IPetService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(name, species, includeInactive ?? false, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetPets")
        .WithSummary("List all active pets")
        .WithDescription("Returns a paginated list of pets. By default only active pets are shown. Use includeInactive=true to include inactive pets.")
        .Produces<PaginatedResponse<PetResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<PetResponse>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.GetByIdAsync(id, ct);
            return pet is null ? TypedResults.NotFound() : TypedResults.Ok(pet);
        })
        .WithName("GetPetById")
        .WithSummary("Get pet by ID")
        .WithDescription("Returns pet details including owner information.")
        .Produces<PetResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<PetResponse>, BadRequest>> (
            CreatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/pets/{pet.Id}", pet);
        })
        .WithName("CreatePet")
        .WithSummary("Create a new pet")
        .WithDescription("Creates a new pet. The owner must exist. Microchip number must be unique if provided.")
        .Produces<PetResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<PetResponse>, NotFound>> (
            int id, UpdatePetRequest request, IPetService service, CancellationToken ct) =>
        {
            var pet = await service.UpdateAsync(id, request, ct);
            return pet is null ? TypedResults.NotFound() : TypedResults.Ok(pet);
        })
        .WithName("UpdatePet")
        .WithSummary("Update a pet")
        .WithDescription("Updates pet details. Changing OwnerId transfers ownership.")
        .Produces<PetResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            await service.SoftDeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeletePet")
        .WithSummary("Soft-delete a pet")
        .WithDescription("Marks a pet as inactive (soft delete).")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/medical-records", async Task<Results<Ok<IReadOnlyList<MedicalRecordSummaryResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var records = await service.GetMedicalRecordsAsync(id, ct);
            return TypedResults.Ok(records);
        })
        .WithName("GetPetMedicalRecords")
        .WithSummary("Get medical records for a pet")
        .WithDescription("Returns all medical records for the specified pet.")
        .Produces<IReadOnlyList<MedicalRecordSummaryResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/vaccinations", async Task<Results<Ok<IReadOnlyList<VaccinationResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var vaccinations = await service.GetVaccinationsAsync(id, ct);
            return TypedResults.Ok(vaccinations);
        })
        .WithName("GetPetVaccinations")
        .WithSummary("Get vaccinations for a pet")
        .WithDescription("Returns all vaccination records for the specified pet.")
        .Produces<IReadOnlyList<VaccinationResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/vaccinations/upcoming", async Task<Results<Ok<IReadOnlyList<VaccinationResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var vaccinations = await service.GetUpcomingVaccinationsAsync(id, ct);
            return TypedResults.Ok(vaccinations);
        })
        .WithName("GetPetUpcomingVaccinations")
        .WithSummary("Get upcoming and overdue vaccinations")
        .WithDescription("Returns vaccinations that are due soon (within 30 days) or already expired for the specified pet.")
        .Produces<IReadOnlyList<VaccinationResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/prescriptions/active", async Task<Results<Ok<IReadOnlyList<PrescriptionResponse>>, NotFound>> (
            int id, IPetService service, CancellationToken ct) =>
        {
            var prescriptions = await service.GetActivePrescriptionsAsync(id, ct);
            return TypedResults.Ok(prescriptions);
        })
        .WithName("GetPetActivePrescriptions")
        .WithSummary("Get active prescriptions for a pet")
        .WithDescription("Returns all currently active prescriptions for the specified pet.")
        .Produces<IReadOnlyList<PrescriptionResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
