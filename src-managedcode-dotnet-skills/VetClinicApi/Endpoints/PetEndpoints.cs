using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PetEndpoints
{
    public static RouteGroupBuilder MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets").WithTags("Pets");

        group.MapGet("/", GetAll).WithName("GetPets").WithSummary("Get all pets with pagination");
        group.MapGet("/{id:int}", GetById).WithName("GetPetById").WithSummary("Get pet by ID");
        group.MapPost("/", Create).WithName("CreatePet").WithSummary("Create a new pet");
        group.MapPut("/{id:int}", Update).WithName("UpdatePet").WithSummary("Update an existing pet");
        group.MapDelete("/{id:int}", Delete).WithName("DeletePet").WithSummary("Soft delete a pet");
        group.MapGet("/{id:int}/medical-records", GetMedicalRecords).WithName("GetPetMedicalRecords").WithSummary("Get medical records for a pet");
        group.MapGet("/{id:int}/vaccinations", GetVaccinations).WithName("GetPetVaccinations").WithSummary("Get vaccinations for a pet");
        group.MapGet("/{id:int}/vaccinations/upcoming", GetUpcomingVaccinations).WithName("GetPetUpcomingVaccinations").WithSummary("Get upcoming vaccinations for a pet");
        group.MapGet("/{id:int}/prescriptions/active", GetActivePrescriptions).WithName("GetPetActivePrescriptions").WithSummary("Get active prescriptions for a pet");

        return group;
    }

    private static async Task<IResult> GetAll(IPetService service, int page = 1, int pageSize = 10, bool includeInactive = false, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 50) pageSize = 10;
        return TypedResults.Ok(await service.GetAllAsync(page, pageSize, includeInactive, ct));
    }

    private static async Task<IResult> GetById(int id, IPetService service, CancellationToken ct)
    {
        var pet = await service.GetByIdAsync(id, ct);
        return pet is not null ? TypedResults.Ok(pet) : TypedResults.NotFound();
    }

    private static async Task<IResult> Create(CreatePetRequest request, IPetService service, CancellationToken ct)
    {
        var pet = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/pets/{pet.Id}", pet);
    }

    private static async Task<IResult> Update(int id, UpdatePetRequest request, IPetService service, CancellationToken ct)
    {
        var pet = await service.UpdateAsync(id, request, ct);
        return pet is not null ? TypedResults.Ok(pet) : TypedResults.NotFound();
    }

    private static async Task<IResult> Delete(int id, IPetService service, CancellationToken ct)
    {
        var deleted = await service.DeleteAsync(id, ct);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    private static async Task<IResult> GetMedicalRecords(int id, IPetService service, CancellationToken ct)
    {
        if (await service.GetByIdAsync(id, ct) is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(await service.GetMedicalRecordsAsync(id, ct));
    }

    private static async Task<IResult> GetVaccinations(int id, IPetService service, CancellationToken ct)
    {
        if (await service.GetByIdAsync(id, ct) is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(await service.GetVaccinationsAsync(id, ct));
    }

    private static async Task<IResult> GetUpcomingVaccinations(int id, IPetService service, CancellationToken ct)
    {
        if (await service.GetByIdAsync(id, ct) is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(await service.GetUpcomingVaccinationsAsync(id, ct));
    }

    private static async Task<IResult> GetActivePrescriptions(int id, IPetService service, CancellationToken ct)
    {
        if (await service.GetByIdAsync(id, ct) is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(await service.GetActivePrescriptionsAsync(id, ct));
    }
}
