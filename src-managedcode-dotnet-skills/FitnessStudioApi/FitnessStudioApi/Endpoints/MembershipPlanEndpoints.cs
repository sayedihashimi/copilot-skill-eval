using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Endpoints;

public static class MembershipPlanEndpoints
{
    public static RouteGroupBuilder MapMembershipPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/membership-plans")
            .WithTags("Membership Plans");

        group.MapGet("/", async (IMembershipPlanService service, CancellationToken ct) =>
        {
            var plans = await service.GetAllActiveAsync(ct);
            return TypedResults.Ok(plans);
        })
        .WithName("GetMembershipPlans")
        .WithSummary("List all active membership plans");

        group.MapGet("/{id:int}", async (int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.GetByIdAsync(id, ct);
            return plan is not null ? Results.Ok(plan) : Results.NotFound();
        })
        .WithName("GetMembershipPlan")
        .WithSummary("Get membership plan details");

        group.MapPost("/", async (CreateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/membership-plans/{plan.Id}", plan);
        })
        .WithName("CreateMembershipPlan")
        .WithSummary("Create a new membership plan");

        group.MapPut("/{id:int}", async (int id, UpdateMembershipPlanRequest request, IMembershipPlanService service, CancellationToken ct) =>
        {
            var plan = await service.UpdateAsync(id, request, ct);
            return plan is not null ? Results.Ok(plan) : Results.NotFound();
        })
        .WithName("UpdateMembershipPlan")
        .WithSummary("Update a membership plan");

        group.MapDelete("/{id:int}", async (int id, IMembershipPlanService service, CancellationToken ct) =>
        {
            var result = await service.DeactivateAsync(id, ct);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeactivateMembershipPlan")
        .WithSummary("Deactivate a membership plan");

        return group;
    }
}
