using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static RouteGroupBuilder MapVaccinationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vaccinations").WithTags("Vaccinations");

        group.MapPost("/", Create).WithName("CreateVaccination").WithSummary("Create a new vaccination record");
        group.MapGet("/{id:int}", GetById).WithName("GetVaccinationById").WithSummary("Get vaccination by ID");

        return group;
    }

    private static async Task<IResult> GetById(int id, IVaccinationService service, CancellationToken ct)
    {
        var vaccination = await service.GetByIdAsync(id, ct);
        return vaccination is not null ? TypedResults.Ok(vaccination) : TypedResults.NotFound();
    }

    private static async Task<IResult> Create(CreateVaccinationRequest request, IVaccinationService service, CancellationToken ct)
    {
        var vaccination = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/vaccinations/{vaccination.Id}", vaccination);
    }
}
