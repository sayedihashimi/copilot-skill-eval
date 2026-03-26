using VetClinicApi.DTOs;
using VetClinicApi.Models;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<AppointmentResponse>>, BadRequest>> (
            DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status,
            int? vetId, int? petId, int? page, int? pageSize,
            IAppointmentService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(dateFrom, dateTo, status, vetId, petId, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAllAppointments")
        .WithSummary("List appointments")
        .WithDescription("Returns a paginated list of appointments. Supports filtering by date range, status, veterinarian, and pet.")
        .Produces<PaginatedResponse<AppointmentResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<AppointmentResponse>, NotFound>> (
            int id, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.GetByIdAsync(id, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("GetAppointmentById")
        .WithSummary("Get appointment by ID")
        .WithDescription("Returns appointment details including pet, veterinarian, and medical record information.")
        .Produces<AppointmentResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<AppointmentResponse>, BadRequest>> (
            CreateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/appointments/{appointment.Id}", appointment);
        })
        .WithName("CreateAppointment")
        .WithSummary("Schedule a new appointment")
        .WithDescription("Creates a new appointment. Enforces scheduling conflict detection and validates the appointment date is in the future.")
        .Produces<AppointmentResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<AppointmentResponse>, NotFound, BadRequest>> (
            int id, UpdateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(appointment);
        })
        .WithName("UpdateAppointment")
        .WithSummary("Update appointment details")
        .WithDescription("Updates appointment details. Date/time changes re-check for scheduling conflicts.")
        .Produces<AppointmentResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPatch("/{id:int}/status", async Task<Results<Ok<AppointmentResponse>, NotFound, BadRequest>> (
            int id, UpdateAppointmentStatusRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateStatusAsync(id, request, ct);
            return TypedResults.Ok(appointment);
        })
        .WithName("UpdateAppointmentStatus")
        .WithSummary("Update appointment status")
        .WithDescription("Updates the appointment status following the allowed workflow transitions.")
        .Produces<AppointmentResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/today", async (
            IAppointmentService service, CancellationToken ct) =>
        {
            var appointments = await service.GetTodayAsync(ct);
            return TypedResults.Ok(appointments);
        })
        .WithName("GetTodayAppointments")
        .WithSummary("Get today's appointments")
        .WithDescription("Returns all appointments scheduled for today.")
        .Produces<IReadOnlyList<AppointmentResponse>>();
    }
}
