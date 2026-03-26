using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static void MapMembershipEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/memberships")
            .WithTags("Memberships");

        group.MapPost("/", async (CreateMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/memberships/{membership.Id}", membership);
        })
        .WithName("CreateMembership")
        .WithSummary("Purchase a membership")
        .WithDescription("Create a new membership for a member. Member must not have an active or frozen membership.")
        .Produces<MembershipResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}", async Task<Results<Ok<MembershipResponse>, NotFound>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.GetByIdAsync(id, ct);
            return membership is null ? TypedResults.NotFound() : TypedResults.Ok(membership);
        })
        .WithName("GetMembershipById")
        .WithSummary("Get membership details")
        .WithDescription("Returns details of a specific membership including plan information.")
        .Produces<MembershipResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/cancel", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.CancelAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("CancelMembership")
        .WithSummary("Cancel a membership")
        .WithDescription("Cancels an active or frozen membership.")
        .Produces<MembershipResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/freeze", async (int id, FreezeMembershipRequest request,
            IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.FreezeAsync(id, request, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("FreezeMembership")
        .WithSummary("Freeze a membership")
        .WithDescription("Freeze an active membership for 7-30 days. Only one freeze per membership term.")
        .Produces<MembershipResponse>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/unfreeze", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.UnfreezeAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("UnfreezeMembership")
        .WithSummary("Unfreeze a membership")
        .WithDescription("Unfreeze a frozen membership. The end date is extended by the freeze duration.")
        .Produces<MembershipResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/renew", async (int id, IMembershipService service, CancellationToken ct) =>
        {
            var membership = await service.RenewAsync(id, ct);
            return TypedResults.Ok(membership);
        })
        .WithName("RenewMembership")
        .WithSummary("Renew an expired membership")
        .WithDescription("Creates a new active membership based on an expired membership's plan.")
        .Produces<MembershipResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}
