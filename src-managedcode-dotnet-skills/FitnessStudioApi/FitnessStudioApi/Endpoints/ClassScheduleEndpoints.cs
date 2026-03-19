using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static RouteGroupBuilder MapClassScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes")
            .WithTags("Class Schedules");

        group.MapGet("/", async (int page, int pageSize, IClassScheduleService service, CancellationToken ct) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var result = await service.GetAllAsync(page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetClassSchedules")
        .WithSummary("List scheduled classes with pagination");

        group.MapGet("/{id:int}", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.GetByIdAsync(id, ct);
            return schedule is not null ? Results.Ok(schedule) : Results.NotFound();
        })
        .WithName("GetClassSchedule")
        .WithSummary("Get class schedule details");

        group.MapPost("/", async (CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/classes/{schedule.Id}", schedule);
        })
        .WithName("CreateClassSchedule")
        .WithSummary("Schedule a new class");

        group.MapPut("/{id:int}", async (int id, UpdateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.UpdateAsync(id, request, ct);
            return schedule is not null ? Results.Ok(schedule) : Results.NotFound();
        })
        .WithName("UpdateClassSchedule")
        .WithSummary("Update a class schedule");

        group.MapPatch("/{id:int}/cancel", async (int id, CancelClassRequest? request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("CancelClassSchedule")
        .WithSummary("Cancel a class (cascade cancels all bookings)");

        group.MapGet("/{id:int}/roster", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var roster = await service.GetRosterAsync(id, ct);
            return TypedResults.Ok(roster);
        })
        .WithName("GetClassRoster")
        .WithSummary("Get class roster");

        group.MapGet("/{id:int}/waitlist", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var waitlist = await service.GetWaitlistAsync(id, ct);
            return TypedResults.Ok(waitlist);
        })
        .WithName("GetClassWaitlist")
        .WithSummary("Get class waitlist");

        group.MapGet("/available", async (IClassScheduleService service, CancellationToken ct) =>
        {
            var available = await service.GetAvailableAsync(ct);
            return TypedResults.Ok(available);
        })
        .WithName("GetAvailableClasses")
        .WithSummary("Get available classes with open spots");

        return group;
    }
}
