using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static void MapInstructorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/instructors")
            .WithTags("Instructors");

        group.MapGet("/", async (string? specialization, bool? isActive,
            IInstructorService service, CancellationToken ct) =>
        {
            var instructors = await service.GetAllAsync(specialization, isActive, ct);
            return TypedResults.Ok(instructors);
        })
        .WithName("GetInstructors")
        .WithSummary("List instructors")
        .WithDescription("List instructors with optional filtering by specialization and active status.")
        .Produces<IReadOnlyList<InstructorResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound>> (
            int id, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.GetByIdAsync(id, ct);
            return instructor is null ? TypedResults.NotFound() : TypedResults.Ok(instructor);
        })
        .WithName("GetInstructorById")
        .WithSummary("Get an instructor by ID")
        .WithDescription("Returns the full details of a specific instructor.")
        .Produces<InstructorResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/instructors/{instructor.Id}", instructor);
        })
        .WithName("CreateInstructor")
        .WithSummary("Create a new instructor")
        .WithDescription("Register a new instructor with their specializations and bio.")
        .Produces<InstructorResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound>> (
            int id, UpdateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var instructor = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(instructor);
        })
        .WithName("UpdateInstructor")
        .WithSummary("Update an instructor")
        .WithDescription("Update an existing instructor's details.")
        .Produces<InstructorResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}/schedule", async (int id, DateTime? fromDate, DateTime? toDate,
            IInstructorService service, CancellationToken ct) =>
        {
            var schedule = await service.GetScheduleAsync(id, fromDate, toDate, ct);
            return TypedResults.Ok(schedule);
        })
        .WithName("GetInstructorSchedule")
        .WithSummary("Get an instructor's class schedule")
        .WithDescription("Returns all scheduled classes for an instructor, optionally filtered by date range.")
        .Produces<IReadOnlyList<ClassScheduleResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
