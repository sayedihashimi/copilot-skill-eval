using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static RouteGroupBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", GetAll).WithSummary("List appointments with filters and pagination");
        group.MapGet("/today", GetToday).WithSummary("Get all of today's appointments");
        group.MapGet("/{id:int}", GetById).WithSummary("Get appointment details with pet, vet, and medical record");
        group.MapPost("/", Create).WithSummary("Schedule a new appointment with conflict detection");
        group.MapPut("/{id:int}", Update).WithSummary("Update appointment details");
        group.MapPatch("/{id:int}/status", UpdateStatus).WithSummary("Update appointment status with workflow rules");

        return group;
    }

    private static async Task<Ok<PaginatedResponse<AppointmentDto>>> GetAll(
        IAppointmentService service,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? status = null,
        [FromQuery] int? vetId = null,
        [FromQuery] int? petId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(fromDate, toDate, status, vetId, petId, page, pageSize, ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<IReadOnlyList<AppointmentDto>>> GetToday(
        IAppointmentService service, CancellationToken ct = default)
    {
        var appointments = await service.GetTodayAsync(ct);
        return TypedResults.Ok(appointments);
    }

    private static async Task<Results<Ok<AppointmentDetailDto>, NotFound>> GetById(
        int id, IAppointmentService service, CancellationToken ct = default)
    {
        var appointment = await service.GetByIdAsync(id, ct);
        return appointment is not null
            ? TypedResults.Ok(appointment)
            : TypedResults.NotFound();
    }

    private static async Task<Results<Created<AppointmentDto>, BadRequest<ProblemDetails>, Conflict<ProblemDetails>>> Create(
        CreateAppointmentDto dto, IAppointmentService service, CancellationToken ct = default)
    {
        var (appointment, error) = await service.CreateAsync(dto, ct);
        if (appointment is not null)
        {
            return TypedResults.Created($"/api/appointments/{appointment.Id}", appointment);
        }

        if (error!.Contains("conflict", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Scheduling conflict",
                Detail = error,
                Status = StatusCodes.Status409Conflict
            });
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Invalid appointment",
            Detail = error,
            Status = StatusCodes.Status400BadRequest
        });
    }

    private static async Task<Results<Ok<AppointmentDto>, NotFound, BadRequest<ProblemDetails>, Conflict<ProblemDetails>>> Update(
        int id, UpdateAppointmentDto dto, IAppointmentService service, CancellationToken ct = default)
    {
        var (appointment, error) = await service.UpdateAsync(id, dto, ct);
        if (appointment is not null)
        {
            return TypedResults.Ok(appointment);
        }

        if (error == "Appointment not found")
        {
            return TypedResults.NotFound();
        }

        if (error!.Contains("conflict", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Scheduling conflict",
                Detail = error,
                Status = StatusCodes.Status409Conflict
            });
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Invalid update",
            Detail = error,
            Status = StatusCodes.Status400BadRequest
        });
    }

    private static async Task<Results<NoContent, NotFound, BadRequest<ProblemDetails>>> UpdateStatus(
        int id, UpdateAppointmentStatusDto dto, IAppointmentService service, CancellationToken ct = default)
    {
        var (success, error) = await service.UpdateStatusAsync(id, dto, ct);
        if (success)
        {
            return TypedResults.NoContent();
        }

        if (error == "Appointment not found")
        {
            return TypedResults.NotFound();
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Invalid status update",
            Detail = error,
            Status = StatusCodes.Status400BadRequest
        });
    }
}
