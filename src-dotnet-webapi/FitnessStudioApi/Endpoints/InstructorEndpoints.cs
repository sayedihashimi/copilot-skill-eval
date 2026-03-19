using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class InstructorEndpoints
{
    public static void MapInstructorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/instructors").WithTags("Instructors");

        group.MapGet("/", async Task<Ok<PaginatedResponse<InstructorResponse>>> (
            IInstructorService service,
            string? specialization = null, bool? isActive = null,
            int page = 1, int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await service.GetAllAsync(specialization, isActive, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetInstructors")
        .WithSummary("List instructors")
        .WithDescription("Returns a paginated list of instructors with optional specialization and active status filters.")
        .Produces<PaginatedResponse<InstructorResponse>>(200);

        group.MapGet("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound>> (
            int id, IInstructorService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetInstructor")
        .WithSummary("Get instructor details")
        .WithDescription("Returns details of a specific instructor by ID.")
        .Produces<InstructorResponse>(200)
        .Produces(404);

        group.MapPost("/", async Task<Created<InstructorResponse>> (
            CreateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/instructors/{result.Id}", result);
        })
        .WithName("CreateInstructor")
        .WithSummary("Create a new instructor")
        .WithDescription("Creates a new instructor profile.")
        .Produces<InstructorResponse>(201);

        group.MapPut("/{id:int}", async Task<Results<Ok<InstructorResponse>, NotFound>> (
            int id, UpdateInstructorRequest request, IInstructorService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("UpdateInstructor")
        .WithSummary("Update an instructor")
        .WithDescription("Updates an existing instructor's profile information.")
        .Produces<InstructorResponse>(200)
        .Produces(404);

        group.MapGet("/{id:int}/schedule", async Task<Ok<IReadOnlyList<ClassScheduleResponse>>> (
            int id, IInstructorService service,
            DateTime? fromDate = null, DateTime? toDate = null,
            CancellationToken ct = default) =>
        {
            var result = await service.GetScheduleAsync(id, fromDate, toDate, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetInstructorSchedule")
        .WithSummary("Get instructor's schedule")
        .WithDescription("Returns class schedules for an instructor with optional date range filter.")
        .Produces<IReadOnlyList<ClassScheduleResponse>>(200);
    }
}
