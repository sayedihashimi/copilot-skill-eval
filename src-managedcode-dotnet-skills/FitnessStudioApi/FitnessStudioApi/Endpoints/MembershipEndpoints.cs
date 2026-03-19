using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static RouteGroupBuilder MapMembershipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/memberships")
            .WithTags("Memberships");

        group.MapPost("/", async (CreateMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/memberships/{membership.Id}", membership);
        })
        .WithName("CreateMembership")
        .WithSummary("Create a new membership");

        group.MapGet("/{id:int}", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.GetByIdAsync(id, ct);
            return membership is not null ? Results.Ok(membership) : Results.NotFound();
        })
        .WithName("GetMembership")
        .WithSummary("Get membership details");

        group.MapPost("/{id:int}/cancel", async (int id, CancelMembershipRequest? request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CancelAsync(id, request, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("CancelMembership")
        .WithSummary("Cancel a membership");

        group.MapPost("/{id:int}/freeze", async (int id, FreezeMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.FreezeAsync(id, request, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("FreezeMembership")
        .WithSummary("Freeze a membership");

        group.MapPost("/{id:int}/unfreeze", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.UnfreezeAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("UnfreezeMembership")
        .WithSummary("Unfreeze a membership");

        group.MapPost("/{id:int}/renew", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.RenewAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("RenewMembership")
        .WithSummary("Renew a membership");

        return group;
    }
}
