using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members");

        group.MapGet("/", async (
            string? search, bool? isActive, int page, int pageSize,
            IMemberService service, CancellationToken ct) =>
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMembers")
        .WithSummary("List members with search, filter, and pagination");

        group.MapGet("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.GetByIdAsync(id, ct);
            return member is not null ? Results.Ok(member) : Results.NotFound();
        })
        .WithName("GetMember")
        .WithSummary("Get member details");

        group.MapPost("/", async (CreateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/members/{member.Id}", member);
        })
        .WithName("CreateMember")
        .WithSummary("Register a new member");

        group.MapPut("/{id:int}", async (int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.UpdateAsync(id, request, ct);
            return member is not null ? Results.Ok(member) : Results.NotFound();
        })
        .WithName("UpdateMember")
        .WithSummary("Update member information");

        group.MapDelete("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.DeactivateAsync(id, ct);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeactivateMember")
        .WithSummary("Deactivate a member");

        group.MapGet("/{id:int}/bookings", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var bookings = await service.GetBookingsAsync(id, ct);
            return TypedResults.Ok(bookings);
        })
        .WithName("GetMemberBookings")
        .WithSummary("Get member's bookings");

        group.MapGet("/{id:int}/bookings/upcoming", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var bookings = await service.GetUpcomingBookingsAsync(id, ct);
            return TypedResults.Ok(bookings);
        })
        .WithName("GetMemberUpcomingBookings")
        .WithSummary("Get member's upcoming bookings");

        group.MapGet("/{id:int}/memberships", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var memberships = await service.GetMembershipsAsync(id, ct);
            return TypedResults.Ok(memberships);
        })
        .WithName("GetMemberMemberships")
        .WithSummary("Get member's membership history");

        return group;
    }
}
