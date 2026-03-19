using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VaccinationEndpoints
{
    public static void MapVaccinationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vaccinations").WithTags("Vaccinations");

        group.MapPost("/", async Task<Results<Created<VaccinationResponse>, BadRequest<ProblemDetails>>> (
            CreateVaccinationRequest request, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/vaccinations/{vaccination.Id}", vaccination);
        })
        .WithName("CreateVaccination")
        .WithSummary("Record a new vaccination")
        .WithDescription("Records a new vaccination for a pet. Expiration date must be after administration date.")
        .Produces<VaccinationResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", async Task<Results<Ok<VaccinationResponse>, NotFound>> (
            int id, IVaccinationService service, CancellationToken ct) =>
        {
            var vaccination = await service.GetByIdAsync(id, ct);
            return vaccination is null ? TypedResults.NotFound() : TypedResults.Ok(vaccination);
        })
        .WithName("GetVaccinationById")
        .WithSummary("Get vaccination by ID")
        .WithDescription("Returns vaccination details including expiry and due-soon status.")
        .Produces<VaccinationResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
