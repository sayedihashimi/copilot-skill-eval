using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static RouteGroupBuilder MapMembershipEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/memberships")
            .WithTags("Memberships");

        group.MapPost("/", async (CreateMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/memberships/{membership.Id}", membership);
        }).WithSummary("Purchase/create a membership for a member");

        group.MapGet("/{id:int}", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.GetByIdAsync(id, ct);
            return membership is not null ? Results.Ok(membership) : Results.NotFound();
        }).WithSummary("Get membership details");

        group.MapPost("/{id:int}/cancel", async (int id, IMembershipService service, CancellationToken ct) =>
            TypedResults.Ok(await service.CancelAsync(id, ct)))
            .WithSummary("Cancel a membership");

        group.MapPost("/{id:int}/freeze", async (int id, FreezeMembershipRequest request, IMembershipService service, CancellationToken ct) =>
            TypedResults.Ok(await service.FreezeAsync(id, request, ct)))
            .WithSummary("Freeze a membership (7-30 days)");

        group.MapPost("/{id:int}/unfreeze", async (int id, IMembershipService service, CancellationToken ct) =>
            TypedResults.Ok(await service.UnfreezeAsync(id, ct)))
            .WithSummary("Unfreeze a membership (extends end date)");

        group.MapPost("/{id:int}/renew", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.RenewAsync(id, ct);
            return TypedResults.Created($"/api/memberships/{membership.Id}", membership);
        }).WithSummary("Renew an expired membership");

        return group;
    }
}
