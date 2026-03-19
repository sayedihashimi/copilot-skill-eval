using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VeterinarianEndpoints
{
    public static RouteGroupBuilder MapVeterinarianEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/veterinarians").WithTags("Veterinarians");

        group.MapGet("/", GetAll).WithName("GetVeterinarians").WithSummary("Get all veterinarians with pagination");
        group.MapGet("/{id:int}", GetById).WithName("GetVeterinarianById").WithSummary("Get veterinarian by ID");
        group.MapPost("/", Create).WithName("CreateVeterinarian").WithSummary("Create a new veterinarian");
        group.MapPut("/{id:int}", Update).WithName("UpdateVeterinarian").WithSummary("Update an existing veterinarian");
        group.MapGet("/{id:int}/schedule", GetSchedule).WithName("GetVeterinarianSchedule").WithSummary("Get schedule for a specific date");
        group.MapGet("/{id:int}/appointments", GetAppointments).WithName("GetVeterinarianAppointments").WithSummary("Get all appointments for a veterinarian");

        return group;
    }

    private static async Task<IResult> GetAll(IVeterinarianService service, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 50) pageSize = 10;
        return TypedResults.Ok(await service.GetAllAsync(page, pageSize, ct));
    }

    private static async Task<IResult> GetById(int id, IVeterinarianService service, CancellationToken ct)
    {
        var vet = await service.GetByIdAsync(id, ct);
        return vet is not null ? TypedResults.Ok(vet) : TypedResults.NotFound();
    }

    private static async Task<IResult> Create(CreateVeterinarianRequest request, IVeterinarianService service, CancellationToken ct)
    {
        var vet = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/veterinarians/{vet.Id}", vet);
    }

    private static async Task<IResult> Update(int id, UpdateVeterinarianRequest request, IVeterinarianService service, CancellationToken ct)
    {
        var vet = await service.UpdateAsync(id, request, ct);
        return vet is not null ? TypedResults.Ok(vet) : TypedResults.NotFound();
    }

    private static async Task<IResult> GetSchedule(int id, IVeterinarianService service, DateOnly? date = null, CancellationToken ct = default)
    {
        if (await service.GetByIdAsync(id, ct) is null)
            return TypedResults.NotFound();

        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return TypedResults.Ok(await service.GetScheduleAsync(id, scheduleDate, ct));
    }

    private static async Task<IResult> GetAppointments(int id, IVeterinarianService service, CancellationToken ct)
    {
        if (await service.GetByIdAsync(id, ct) is null)
            return TypedResults.NotFound();
        return TypedResults.Ok(await service.GetAppointmentsAsync(id, ct));
    }
}
