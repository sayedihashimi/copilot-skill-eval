using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static RouteGroupBuilder MapVaccinationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/vaccinations").WithTags("Vaccinations");

        group.MapPost("/", Create).WithSummary("Record a new vaccination");
        group.MapGet("/{id:int}", GetById).WithSummary("Get vaccination details");

        return group;
    }

    private static async Task<Results<Ok<VaccinationDto>, NotFound>> GetById(
        int id, IVaccinationService service, CancellationToken ct = default)
    {
        var vaccination = await service.GetByIdAsync(id, ct);
        return vaccination is not null
            ? TypedResults.Ok(vaccination)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<VaccinationDto>, BadRequest<ProblemDetails>>> Create(
        CreateVaccinationDto dto, IVaccinationService service, CancellationToken ct = default)
    {
        var (vaccination, error) = await service.CreateAsync(dto, ct);
        if (vaccination is not null)
        {
            return TypedResults.Created($"/api/vaccinations/{vaccination.Id}", vaccination);
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Invalid vaccination",
            Detail = error,
            Status = StatusCodes.Status400BadRequest
        });
    }
}
