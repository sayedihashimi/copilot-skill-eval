using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MembershipEndpoints
{
    public static void MapMembershipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/memberships").WithTags("Memberships");

        group.MapPost("/", async Task<Created<MembershipResponse>> (
            CreateMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/memberships/{result.Id}", result);
        })
        .WithName("CreateMembership")
        .WithSummary("Purchase a membership")
        .WithDescription("Creates a new membership for a member. Member can only have one active/frozen membership at a time.")
        .Produces<MembershipResponse>(201);

        group.MapGet("/{id:int}", async Task<Results<Ok<MembershipResponse>, NotFound>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetMembership")
        .WithSummary("Get membership details")
        .WithDescription("Returns details of a specific membership by ID.")
        .Produces<MembershipResponse>(200)
        .Produces(404);

        group.MapPost("/{id:int}/cancel", async Task<Ok<MembershipResponse>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var result = await service.CancelAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("CancelMembership")
        .WithSummary("Cancel a membership")
        .WithDescription("Cancels a membership and issues a refund.")
        .Produces<MembershipResponse>(200);

        group.MapPost("/{id:int}/freeze", async Task<Ok<MembershipResponse>> (
            int id, FreezeMembershipRequest request, IMembershipService service, CancellationToken ct) =>
        {
            var result = await service.FreezeAsync(id, request, ct);
            return TypedResults.Ok(result);
        })
        .WithName("FreezeMembership")
        .WithSummary("Freeze a membership")
        .WithDescription("Freezes an active membership for 7-30 days. Can only freeze once per term.")
        .Produces<MembershipResponse>(200);

        group.MapPost("/{id:int}/unfreeze", async Task<Ok<MembershipResponse>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var result = await service.UnfreezeAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("UnfreezeMembership")
        .WithSummary("Unfreeze a membership")
        .WithDescription("Unfreezes a frozen membership and extends the end date by the freeze duration.")
        .Produces<MembershipResponse>(200);

        group.MapPost("/{id:int}/renew", async Task<Ok<MembershipResponse>> (
            int id, IMembershipService service, CancellationToken ct) =>
        {
            var result = await service.RenewAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("RenewMembership")
        .WithSummary("Renew an expired membership")
        .WithDescription("Renews an expired membership by creating a new active membership with the same plan.")
        .Produces<MembershipResponse>(200);
    }
}
