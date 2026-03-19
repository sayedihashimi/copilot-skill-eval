using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class ClassScheduleEndpoints
{
    public static void MapClassScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes").WithTags("Class Schedules");

        group.MapGet("/", async Task<Ok<PaginatedResponse<ClassScheduleResponse>>> (
            IClassScheduleService service,
            DateTime? fromDate = null, DateTime? toDate = null,
            int? classTypeId = null, int? instructorId = null,
            bool? hasAvailability = null,
            int page = 1, int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await service.GetAllAsync(fromDate, toDate, classTypeId, instructorId, hasAvailability, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetClassSchedules")
        .WithSummary("List scheduled classes")
        .WithDescription("Returns a paginated list of class schedules with optional filters for date, type, instructor, and availability.")
        .Produces<PaginatedResponse<ClassScheduleResponse>>(200);

        group.MapGet("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound>> (
            int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetClassSchedule")
        .WithSummary("Get class details")
        .WithDescription("Returns details of a specific class including enrollment and waitlist counts.")
        .Produces<ClassScheduleResponse>(200)
        .Produces(404);

        group.MapPost("/", async Task<Created<ClassScheduleResponse>> (
            CreateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/classes/{result.Id}", result);
        })
        .WithName("CreateClassSchedule")
        .WithSummary("Schedule a new class")
        .WithDescription("Creates a new class schedule. Validates instructor availability.")
        .Produces<ClassScheduleResponse>(201);

        group.MapPut("/{id:int}", async Task<Results<Ok<ClassScheduleResponse>, NotFound>> (
            int id, UpdateClassScheduleRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("UpdateClassSchedule")
        .WithSummary("Update class details")
        .WithDescription("Updates an existing class schedule. Validates instructor availability.")
        .Produces<ClassScheduleResponse>(200)
        .Produces(404);

        group.MapPatch("/{id:int}/cancel", async Task<Ok<ClassScheduleResponse>> (
            int id, CancelClassRequest request, IClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(result);
        })
        .WithName("CancelClass")
        .WithSummary("Cancel a class")
        .WithDescription("Cancels a scheduled class. All active bookings are automatically cancelled with reason 'Class cancelled by studio'.")
        .Produces<ClassScheduleResponse>(200);

        group.MapGet("/{id:int}/roster", async Task<Ok<IReadOnlyList<RosterEntryResponse>>> (
            int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.GetRosterAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetClassRoster")
        .WithSummary("Get class roster")
        .WithDescription("Returns the list of confirmed and attended members for a class.")
        .Produces<IReadOnlyList<RosterEntryResponse>>(200);

        group.MapGet("/{id:int}/waitlist", async Task<Ok<IReadOnlyList<WaitlistEntryResponse>>> (
            int id, IClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.GetWaitlistAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetClassWaitlist")
        .WithSummary("Get class waitlist")
        .WithDescription("Returns the waitlist for a class ordered by position.")
        .Produces<IReadOnlyList<WaitlistEntryResponse>>(200);

        group.MapGet("/available", async Task<Ok<IReadOnlyList<ClassScheduleResponse>>> (
            IClassScheduleService service, CancellationToken ct) =>
        {
            var result = await service.GetAvailableAsync(ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetAvailableClasses")
        .WithSummary("Get available classes")
        .WithDescription("Returns classes with available spots in the next 7 days.")
        .Produces<IReadOnlyList<ClassScheduleResponse>>(200);
    }
}
