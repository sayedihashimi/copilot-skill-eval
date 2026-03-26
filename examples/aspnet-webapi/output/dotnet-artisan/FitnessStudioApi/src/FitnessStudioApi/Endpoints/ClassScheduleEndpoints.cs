using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static RouteGroupBuilder MapClassScheduleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/classes")
            .WithTags("Class Schedules");

        group.MapGet("/", async (
            IClassScheduleService service,
            CancellationToken ct,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? classTypeId = null,
            [FromQuery] int? instructorId = null,
            [FromQuery] bool? hasAvailability = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
            TypedResults.Ok(await service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, hasAvailability, page, pageSize, ct)))
            .WithSummary("List scheduled classes with filtering");

        group.MapGet("/{id:int}", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.GetByIdAsync(id, ct);
            return schedule is not null ? Results.Ok(schedule) : Results.NotFound();
        }).WithSummary("Get class details with enrollment info");

        group.MapPost("/", async (CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/classes/{schedule.Id}", schedule);
        }).WithSummary("Schedule a new class");

        group.MapPut("/{id:int}", async (int id, UpdateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.UpdateAsync(id, request, ct);
            return schedule is not null ? Results.Ok(schedule) : Results.NotFound();
        }).WithSummary("Update class details");

        group.MapPatch("/{id:int}/cancel", async (int id, CancelClassRequest request, IClassScheduleService service, CancellationToken ct) =>
            TypedResults.Ok(await service.CancelClassAsync(id, request, ct)))
            .WithSummary("Cancel a class (cascades to all bookings)");

        group.MapGet("/{id:int}/roster", async (int id, IClassScheduleService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetRosterAsync(id, ct)))
            .WithSummary("Get confirmed members for a class");

        group.MapGet("/{id:int}/waitlist", async (int id, IClassScheduleService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetWaitlistAsync(id, ct)))
            .WithSummary("Get waitlist for a class");

        group.MapGet("/available", async (IClassScheduleService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetAvailableAsync(ct)))
            .WithSummary("Get classes with available spots in the next 7 days");

        return group;
    }
}
