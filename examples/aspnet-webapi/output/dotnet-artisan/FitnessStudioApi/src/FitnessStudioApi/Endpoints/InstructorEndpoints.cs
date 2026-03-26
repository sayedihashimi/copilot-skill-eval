using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static RouteGroupBuilder MapInstructorEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/instructors")
            .WithTags("Instructors");

        group.MapGet("/", async (
            IInstructorService service,
            CancellationToken ct,
            [FromQuery] string? specialization = null,
            [FromQuery] bool? isActive = null) =>
            TypedResults.Ok(await service.GetAllAsync(specialization, isActive, ct)))
            .WithSummary("List instructors with filtering");

        group.MapGet("/{id:int}", async (int id, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.GetByIdAsync(id, ct);
            return instructor is not null ? Results.Ok(instructor) : Results.NotFound();
        }).WithSummary("Get instructor details");

        group.MapPost("/", async (CreateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/instructors/{instructor.Id}", instructor);
        }).WithSummary("Create a new instructor");

        group.MapPut("/{id:int}", async (int id, UpdateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.UpdateAsync(id, request, ct);
            return instructor is not null ? Results.Ok(instructor) : Results.NotFound();
        }).WithSummary("Update instructor details");

        group.MapGet("/{id:int}/schedule", async (
            int id,
            IInstructorService service,
            CancellationToken ct,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null) =>
            TypedResults.Ok(await service.GetScheduleAsync(id, fromDate, toDate, ct)))
            .WithSummary("Get instructor's class schedule");

        return group;
    }
}
