using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class VeterinarianEndpoints
{
    public static void MapVeterinarianEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/veterinarians").WithTags("Veterinarians");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<VeterinarianResponse>>, BadRequest>> (
            string? specialization, bool? isAvailable, int? page, int? pageSize,
            IVeterinarianService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(specialization, isAvailable, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetVeterinarians")
        .WithSummary("List all veterinarians")
        .WithDescription("Returns a paginated list of veterinarians. Supports filtering by specialization and availability.")
        .Produces<PaginatedResponse<VeterinarianResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<VeterinarianResponse>, NotFound>> (
            int id, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.GetByIdAsync(id, ct);
            return vet is null ? TypedResults.NotFound() : TypedResults.Ok(vet);
        })
        .WithName("GetVeterinarianById")
        .WithSummary("Get veterinarian by ID")
        .WithDescription("Returns veterinarian details.")
        .Produces<VeterinarianResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<VeterinarianResponse>, Conflict<ProblemDetails>>> (
            CreateVeterinarianRequest request, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/veterinarians/{vet.Id}", vet);
        })
        .WithName("CreateVeterinarian")
        .WithSummary("Create a new veterinarian")
        .WithDescription("Creates a new veterinarian. Email and license number must be unique.")
        .Produces<VeterinarianResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", async Task<Results<Ok<VeterinarianResponse>, NotFound>> (
            int id, UpdateVeterinarianRequest request, IVeterinarianService service, CancellationToken ct) =>
        {
            var vet = await service.UpdateAsync(id, request, ct);
            return vet is null ? TypedResults.NotFound() : TypedResults.Ok(vet);
        })
        .WithName("UpdateVeterinarian")
        .WithSummary("Update a veterinarian")
        .WithDescription("Updates veterinarian details.")
        .Produces<VeterinarianResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/schedule", async Task<Results<Ok<IReadOnlyList<AppointmentResponse>>, NotFound>> (
            int id, DateOnly date, IVeterinarianService service, CancellationToken ct) =>
        {
            var schedule = await service.GetScheduleAsync(id, date, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("GetVeterinarianSchedule")
        .WithSummary("Get vet schedule for a date")
        .WithDescription("Returns all appointments for a veterinarian on a specific date.")
        .Produces<IReadOnlyList<AppointmentResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/appointments", async Task<Results<Ok<PaginatedResponse<AppointmentResponse>>, NotFound>> (
            int id, string? status, int? page, int? pageSize,
            IVeterinarianService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAppointmentsAsync(id, status, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetVeterinarianAppointments")
        .WithSummary("Get all appointments for a vet")
        .WithDescription("Returns a paginated list of appointments for a veterinarian with optional status filter.")
        .Produces<PaginatedResponse<AppointmentResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
