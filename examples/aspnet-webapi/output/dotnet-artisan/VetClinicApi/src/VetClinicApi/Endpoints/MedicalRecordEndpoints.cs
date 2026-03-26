using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class MedicalRecordEndpoints
{
    public static RouteGroupBuilder MapMedicalRecordEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/medical-records").WithTags("Medical Records");

        group.MapGet("/{id:int}", GetById).WithSummary("Get medical record with prescriptions");
        group.MapPost("/", Create).WithSummary("Create medical record for a completed/in-progress appointment");
        group.MapPut("/{id:int}", Update).WithSummary("Update medical record");

        return group;
    }

    private static async Task<Results<Ok<MedicalRecordDto>, NotFound>> GetById(
        int id, IMedicalRecordService service, CancellationToken ct = default)
    {
        var record = await service.GetByIdAsync(id, ct);
        return record is not null
            ? TypedResults.Ok(record)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<MedicalRecordDto>, BadRequest<ProblemDetails>, Conflict<ProblemDetails>>> Create(
        CreateMedicalRecordDto dto, IMedicalRecordService service, CancellationToken ct = default)
    {
        var (record, error) = await service.CreateAsync(dto, ct);
        if (record is not null)
        {
            return TypedResults.Created($"/api/medical-records/{record.Id}", record);
        }

        if (error!.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Duplicate medical record",
                Detail = error,
                Status = StatusCodes.Status409Conflict
            });
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Invalid medical record",
            Detail = error,
            Status = StatusCodes.Status400BadRequest
        });
    }

    private static async Task<Results<Ok<MedicalRecordDto>, NotFound>> Update(
        int id, UpdateMedicalRecordDto dto, IMedicalRecordService service, CancellationToken ct = default)
    {
        var (record, error) = await service.UpdateAsync(id, dto, ct);
        return record is not null
            ? TypedResults.Ok(record)
            : TypedResults.NotFound();
    }
}
