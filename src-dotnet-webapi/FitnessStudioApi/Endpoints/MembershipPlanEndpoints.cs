using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipPlanEndpoints
{
    public static void MapMembershipPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/membership-plans").WithTags("Membership Plans");

        group.MapGet("/", async Task<Ok<PaginatedResponse<MembershipPlanResponse>>> (
            IMembershipPlanService service,
            int page = 1, int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await service.GetAllAsync(page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMembershipPlans")
        .WithSummary("List all active membership plans")
        .WithDescription("Returns a paginated list of all active membership plans.")
        .Produces<PaginatedResponse<MembershipPlanResponse>>(200);

        group.MapGet("/{id:int}", async Task<Results<Ok<MembershipPlanResponse>, NotFound>> (
            int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetMembershipPlan")
        .WithSummary("Get membership plan details")
        .WithDescription("Returns details of a specific membership plan by ID.")
        .Produces<MembershipPlanResponse>(200)
        .Produces(404);

        group.MapPost("/", async Task<Created<MembershipPlanResponse>> (
            CreateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/membership-plans/{result.Id}", result);
        })
        .WithName("CreateMembershipPlan")
        .WithSummary("Create a new membership plan")
        .WithDescription("Creates a new membership plan with the specified details.")
        .Produces<MembershipPlanResponse>(201);

        group.MapPut("/{id:int}", async Task<Results<Ok<MembershipPlanResponse>, NotFound>> (
            int id, UpdateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("UpdateMembershipPlan")
        .WithSummary("Update a membership plan")
        .WithDescription("Updates an existing membership plan by ID.")
        .Produces<MembershipPlanResponse>(200)
        .Produces(404);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            var result = await service.DeactivateAsync(id, ct);
            return result ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeactivateMembershipPlan")
        .WithSummary("Deactivate a membership plan")
        .WithDescription("Deactivates a membership plan (soft delete).")
        .Produces(204)
        .Produces(404);
    }
}
