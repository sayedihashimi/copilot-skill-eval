using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VeterinarianEndpoints
{
    public static RouteGroupBuilder MapVeterinarianEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/veterinarians").WithTags("Veterinarians");

        group.MapGet("/", GetAll).WithSummary("List all veterinarians with filters");
        group.MapGet("/{id:int}", GetById).WithSummary("Get veterinarian details");
        group.MapPost("/", Create).WithSummary("Create a new veterinarian");
        group.MapPut("/{id:int}", Update).WithSummary("Update veterinarian info");
        group.MapGet("/{id:int}/schedule", GetSchedule).WithSummary("Get vet's appointments for a specific date");
        group.MapGet("/{id:int}/appointments", GetAppointments).WithSummary("Get all appointments for a vet");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<VeterinarianDto>>> GetAll(
        IVeterinarianService service,
        [FromQuery] string? specialization = null,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(specialization, isAvailable, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<VeterinarianDto>, NotFound>> GetById(
        int id, IVeterinarianService service, CancellationToken ct = default)
    {
        var vet = await service.GetByIdAsync(id, ct);
        return vet is not null
            ? TypedResults.Ok(vet)
            : TypedResults.NotFound();
    }

    private static async Task<Created<VeterinarianDto>> Create(
        CreateVeterinarianDto dto, IVeterinarianService service, CancellationToken ct = default)
    {
        var vet = await service.CreateAsync(dto, ct);
        return TypedResults.Created($"/api/veterinarians/{vet.Id}", vet);
    }

    private static async Task<Results<Ok<VeterinarianDto>, NotFound>> Update(
        int id, UpdateVeterinarianDto dto, IVeterinarianService service, CancellationToken ct = default)
    {
        var vet = await service.UpdateAsync(id, dto, ct);
        return vet is not null
            ? TypedResults.Ok(vet)
            : TypedResults.NotFound();
    }

    private static async Task<Ok<IReadOnlyList<AppointmentDto>>> GetSchedule(
        int id, IVeterinarianService service,
        [FromQuery] DateOnly? date = null,
        CancellationToken ct = default)
    {
        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var schedule = await service.GetScheduleAsync(id, scheduleDate, ct);
        return TypedResults.Ok(schedule);
    }

    private static async Task<Ok<PaginatedResponse<AppointmentDto>>> GetAppointments(
        int id, IVeterinarianService service,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var appointments = await service.GetAppointmentsAsync(id, status, page, pageSize, ct);
        return TypedResults.Ok(appointments);
    }
}
