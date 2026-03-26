using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMemberEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/members")
            .WithTags("Members");

        group.MapGet("/", async (
            IMemberService service,
            CancellationToken ct,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
            TypedResults.Ok(await service.GetAllAsync(search, isActive, page, pageSize, ct)))
            .WithSummary("List members with search and filtering");

        group.MapGet("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.GetByIdAsync(id, ct);
            return member is not null ? Results.Ok(member) : Results.NotFound();
        }).WithSummary("Get member details with active membership info");

        group.MapPost("/", async (CreateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/members/{member.Id}", member);
        }).WithSummary("Register a new member");

        group.MapPut("/{id:int}", async (int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.UpdateAsync(id, request, ct);
            return member is not null ? Results.Ok(member) : Results.NotFound();
        }).WithSummary("Update member profile");

        group.MapDelete("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.DeactivateAsync(id, ct);
            return result ? Results.NoContent() : Results.NotFound();
        }).WithSummary("Deactivate member (fails if future bookings exist)");

        group.MapGet("/{id:int}/bookings", async (
            int id,
            IMemberService service,
            CancellationToken ct,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
            TypedResults.Ok(await service.GetMemberBookingsAsync(id, status, fromDate, toDate, page, pageSize, ct)))
            .WithSummary("Get member's bookings with filtering");

        group.MapGet("/{id:int}/bookings/upcoming", async (int id, IMemberService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetUpcomingBookingsAsync(id, ct)))
            .WithSummary("Get member's upcoming confirmed bookings");

        group.MapGet("/{id:int}/memberships", async (int id, IMemberService service, CancellationToken ct) =>
            TypedResults.Ok(await service.GetMemberMembershipsAsync(id, ct)))
            .WithSummary("Get membership history for a member");

        return group;
    }
}
