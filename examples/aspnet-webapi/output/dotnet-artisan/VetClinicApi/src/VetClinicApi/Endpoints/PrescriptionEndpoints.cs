using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class PrescriptionEndpoints
{
    public static RouteGroupBuilder MapPrescriptionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/prescriptions").WithTags("Prescriptions");

        group.MapGet("/{id:int}", GetById).WithSummary("Get prescription details");
        group.MapPost("/", Create).WithSummary("Create prescription for a medical record");

        return group;
    }

    private static async Task<Results<Ok<PrescriptionDto>, NotFound>> GetById(
        int id, IPrescriptionService service, CancellationToken ct = default)
    {
        var prescription = await service.GetByIdAsync(id, ct);
        return prescription is not null
            ? TypedResults.Ok(prescription)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<PrescriptionDto>, BadRequest<ProblemDetails>>> Create(
        CreatePrescriptionDto dto, IPrescriptionService service, CancellationToken ct = default)
    {
        var (prescription, error) = await service.CreateAsync(dto, ct);
        if (prescription is not null)
        {
            return TypedResults.Created($"/api/prescriptions/{prescription.Id}", prescription);
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Invalid prescription",
            Detail = error,
            Status = StatusCodes.Status400BadRequest
        });
    }
}
