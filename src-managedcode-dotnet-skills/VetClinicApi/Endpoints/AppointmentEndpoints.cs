using VetClinicApi.DTOs;
using VetClinicApi.Services;

namespace VetClinicApi.Endpoints;

public static class AppointmentEndpoints
{
    public static RouteGroupBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments").WithTags("Appointments");

        group.MapGet("/", GetAll).WithName("GetAppointments").WithSummary("Get all appointments with pagination");
        group.MapGet("/{id:int}", GetById).WithName("GetAppointmentById").WithSummary("Get appointment by ID");
        group.MapPost("/", Create).WithName("CreateAppointment").WithSummary("Create a new appointment");
        group.MapPut("/{id:int}", Update).WithName("UpdateAppointment").WithSummary("Update an existing appointment");
        group.MapPatch("/{id:int}/status", UpdateStatus).WithName("UpdateAppointmentStatus").WithSummary("Update appointment status");
        group.MapGet("/today", GetToday).WithName("GetTodayAppointments").WithSummary("Get today's appointments");

        return group;
    }

    private static async Task<IResult> GetAll(IAppointmentService service, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 50) pageSize = 10;
        return TypedResults.Ok(await service.GetAllAsync(page, pageSize, ct));
    }

    private static async Task<IResult> GetById(int id, IAppointmentService service, CancellationToken ct)
    {
        var appt = await service.GetByIdAsync(id, ct);
        return appt is not null ? TypedResults.Ok(appt) : TypedResults.NotFound();
    }

    private static async Task<IResult> Create(CreateAppointmentRequest request, IAppointmentService service, CancellationToken ct)
    {
        var appt = await service.CreateAsync(request, ct);
        return TypedResults.Created($"/api/appointments/{appt.Id}", appt);
    }

    private static async Task<IResult> Update(int id, UpdateAppointmentRequest request, IAppointmentService service, CancellationToken ct)
    {
        var appt = await service.UpdateAsync(id, request, ct);
        return appt is not null ? TypedResults.Ok(appt) : TypedResults.NotFound();
    }

    private static async Task<IResult> UpdateStatus(int id, UpdateAppointmentStatusRequest request, IAppointmentService service, CancellationToken ct)
    {
        var appt = await service.UpdateStatusAsync(id, request, ct);
        return appt is not null ? TypedResults.Ok(appt) : TypedResults.NotFound();
    }

    private static async Task<IResult> GetToday(IAppointmentService service, CancellationToken ct)
    {
        return TypedResults.Ok(await service.GetTodayAsync(ct));
    }
}
