using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", async Task<Results<Ok<PaginatedResponse<AppointmentResponse>>, BadRequest>> (
            DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId,
            int? page, int? pageSize,
            IAppointmentService service, CancellationToken ct) =>
        {
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(fromDate, toDate, status, vetId, petId, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAppointments")
        .WithSummary("List appointments")
        .WithDescription("Returns a paginated list of appointments with optional filters for date range, status, vet, and pet.")
        .Produces<PaginatedResponse<AppointmentResponse>>();

        group.MapGet("/today", async Task<Ok<IReadOnlyList<AppointmentResponse>>> (
            IAppointmentService service, CancellationToken ct) =>
        {
            var result = await service.GetTodayAsync(ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetTodayAppointments")
        .WithSummary("Get today's appointments")
        .WithDescription("Returns all appointments scheduled for today.")
        .Produces<IReadOnlyList<AppointmentResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<AppointmentDetailResponse>, NotFound>> (
            int id, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.GetByIdAsync(id, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("GetAppointmentById")
        .WithSummary("Get appointment by ID")
        .WithDescription("Returns appointment details including pet, vet, and medical record if available.")
        .Produces<AppointmentDetailResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async Task<Results<Created<AppointmentResponse>, BadRequest<ProblemDetails>>> (
            CreateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/appointments/{appointment.Id}", appointment);
        })
        .WithName("CreateAppointment")
        .WithSummary("Schedule a new appointment")
        .WithDescription("Creates a new appointment. Validates scheduling conflicts and requires a future date.")
        .Produces<AppointmentResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<AppointmentResponse>, NotFound>> (
            int id, UpdateAppointmentRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateAsync(id, request, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("UpdateAppointment")
        .WithSummary("Update an appointment")
        .WithDescription("Updates appointment details. Date/time changes re-check for scheduling conflicts.")
        .Produces<AppointmentResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPatch("/{id:int}/status", async Task<Results<Ok<AppointmentResponse>, NotFound>> (
            int id, UpdateAppointmentStatusRequest request, IAppointmentService service, CancellationToken ct) =>
        {
            var appointment = await service.UpdateStatusAsync(id, request, ct);
            return appointment is null ? TypedResults.NotFound() : TypedResults.Ok(appointment);
        })
        .WithName("UpdateAppointmentStatus")
        .WithSummary("Update appointment status")
        .WithDescription("Updates appointment status following the allowed workflow transitions.")
        .Produces<AppointmentResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
