using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static void MapClassScheduleEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/classes")
            .WithTags("Class Schedules");

        group.MapGet("/", async (DateTime? fromDate, DateTime? toDate, int? classTypeId,
            int? instructorId, bool? hasAvailability, int? page, int? pageSize,
            IClassScheduleService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, hasAvailability, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetClassSchedules")
        .WithSummary("List scheduled classes")
        .WithDescription("List scheduled classes with filtering by date range, class type, instructor, and availability.")
        .Produces<PaginatedResponse<ClassScheduleResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound>> (
            int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.GetByIdAsync(id, ct);
            return schedule is null ? TypedResults.NotFound() : TypedResults.Ok(schedule);
        })
        .WithName("GetClassScheduleById")
        .WithSummary("Get class details")
        .WithDescription("Returns the full details of a scheduled class including roster count and availability.")
        .Produces<ClassScheduleResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/classes/{schedule.Id}", schedule);
        })
        .WithName("CreateClassSchedule")
        .WithSummary("Schedule a new class")
        .WithDescription("Schedule a new class. Enforces instructor schedule conflict checks.")
        .Produces<ClassScheduleResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound>> (
            int id, UpdateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("UpdateClassSchedule")
        .WithSummary("Update class details")
        .WithDescription("Update a scheduled class. Checks for instructor conflicts if instructor or time changes.")
        .Produces<ClassScheduleResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPatch("/{id:int}/cancel", async (int id, CancelClassRequest request,
            IClassScheduleService service, CancellationToken ct) =>
        {
            var schedule = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("CancelClassSchedule")
        .WithSummary("Cancel a class")
        .WithDescription("Cancels a scheduled class. All bookings are automatically cancelled with reason 'Class cancelled by studio'.")
        .Produces<ClassScheduleResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/roster", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var roster = await service.GetRosterAsync(id, ct);
            return TypedResults.Ok(roster);
        })
        .WithName("GetClassRoster")
        .WithSummary("Get class roster")
        .WithDescription("Returns the list of confirmed and attended members for a class.")
        .Produces<IReadOnlyList<ClassRosterEntry>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/waitlist", async (int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var waitlist = await service.GetWaitlistAsync(id, ct);
            return TypedResults.Ok(waitlist);
        })
        .WithName("GetClassWaitlist")
        .WithSummary("Get class waitlist")
        .WithDescription("Returns the waitlisted members for a class, ordered by position.")
        .Produces<IReadOnlyList<ClassWaitlistEntry>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/available", async (IClassScheduleService service, CancellationToken ct) =>
        {
            var available = await service.GetAvailableAsync(ct);
            return TypedResults.Ok(available);
        })
        .WithName("GetAvailableClasses")
        .WithSummary("Get available classes")
        .WithDescription("Returns classes with available spots in the next 7 days.")
        .Produces<IReadOnlyList<ClassScheduleResponse>>();
    }
}
