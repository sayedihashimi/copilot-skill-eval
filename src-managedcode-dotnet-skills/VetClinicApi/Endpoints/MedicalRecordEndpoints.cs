using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class MedicalRecordEndpoints
{
    public static RouteGroupBuilder MapMedicalRecordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/medical-records").WithTags("Medical Records");

        group.MapGet("/{id:int}", GetById).WithName("GetMedicalRecordById").WithSummary("Get medical record by ID");
        group.MapPost("/", Create).WithName("CreateMedicalRecord").WithSummary("Create a new medical record");
        group.MapPut("/{id:int}", Update).WithName("UpdateMedicalRecord").WithSummary("Update an existing medical record");

        return group;
    }

    private static async Task<IResult> GetById(int id, IMedicalRecordService service, CancellationToken ct)
    {
        var record = await service.GetByIdAsync(id, ct);
        return record is not null ? TypedResults.Ok(record) : TypedResults.NotFound();
    }

    private static async Task<IResult> Create(CreateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct)
    {
        var record = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/medical-records/{record.Id}", record);
    }

    private static async Task<IResult> Update(int id, UpdateMedicalRecordRequest request, IMedicalRecordService service, CancellationToken ct)
    {
        var record = await service.UpdateAsync(id, request, ct);
        return record is not null ? TypedResults.Ok(record) : TypedResults.NotFound();
    }
}
