using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static RouteGroupBuilder MapInstructorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/instructors")
            .WithTags("Instructors");

        group.MapGet("/", async (IInstructorService service, CancellationToken ct) =>
        {
            var instructors = await service.GetAllAsync(ct);
            return TypedResults.Ok(instructors);
        })
        .WithName("GetInstructors")
        .WithSummary("List all active instructors");

        group.MapGet("/{id:int}", async (int id, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.GetByIdAsync(id, ct);
            return instructor is not null ? Results.Ok(instructor) : Results.NotFound();
        })
        .WithName("GetInstructor")
        .WithSummary("Get instructor details");

        group.MapPost("/", async (CreateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/instructors/{instructor.Id}", instructor);
        })
        .WithName("CreateInstructor")
        .WithSummary("Create a new instructor");

        group.MapPut("/{id:int}", async (int id, UpdateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.UpdateAsync(id, request, ct);
            return instructor is not null ? Results.Ok(instructor) : Results.NotFound();
        })
        .WithName("UpdateInstructor")
        .WithSummary("Update an instructor");

        group.MapGet("/{id:int}/schedule", async (int id, IInstructorService service, CancellationToken ct) =>
        {
            var schedule = await service.GetScheduleAsync(id, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("GetInstructorSchedule")
        .WithSummary("Get instructor's upcoming schedule");

        return group;
    }
}
