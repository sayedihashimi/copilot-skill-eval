using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class MedicalRecordEndpoints
{
    public static void MapMedicalRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/medical-records").WithTags("Medical Records");

        group.MapGet("/{id:int}", async Task<Results<Ok<MedicalRecordResponse>, NotFound>> (
            int id, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.GetByIdAsync(id, ct);
            return record is null ? TypedResults.NotFound() : TypedResults.Ok(record);
        })
        .WithName("GetMedicalRecordById")
        .WithSummary("Get medical record by ID")
        .WithDescription("Returns the medical record with prescriptions.")
        .Produces<MedicalRecordResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<MedicalRecordResponse>, BadRequest>> (
            CreateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/medical-records/{record.Id}", record);
        })
        .WithName("CreateMedicalRecord")
        .WithSummary("Create a medical record")
        .WithDescription("Creates a medical record for an appointment. The appointment must have status 'Completed' or 'InProgress'.")
        .Produces<MedicalRecordResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<MedicalRecordResponse>, NotFound, BadRequest>> (
            int id, UpdateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct) =>
        {
            var record = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(record);
        })
        .WithName("UpdateMedicalRecord")
        .WithSummary("Update a medical record")
        .WithDescription("Updates an existing medical record.")
        .Produces<MedicalRecordResponse>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
