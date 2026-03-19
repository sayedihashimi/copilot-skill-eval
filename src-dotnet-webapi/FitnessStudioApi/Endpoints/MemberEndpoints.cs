using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/members").WithTags("Members");

        group.MapGet("/", async Task<Ok<PaginatedResponse<MemberResponse>>> (
            IMemberService service,
            string? search = null, bool? isActive = null,
            int page = 1, int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await service.GetAllAsync(search, isActive, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMembers")
        .WithSummary("List members")
        .WithDescription("Returns a paginated list of members with optional search and active status filter.")
        .Produces<PaginatedResponse<MemberResponse>>(200);

        group.MapGet("/{id:int}", async Task<Results<Ok<MemberDetailResponse>, NotFound>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("GetMember")
        .WithSummary("Get member details")
        .WithDescription("Returns detailed member information including active membership and booking stats.")
        .Produces<MemberDetailResponse>(200)
        .Produces(404);

        group.MapPost("/", async Task<Created<MemberResponse>> (
            CreateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/members/{result.Id}", result);
        })
        .WithName("CreateMember")
        .WithSummary("Register a new member")
        .WithDescription("Creates a new member. Member must be at least 16 years old.")
        .Produces<MemberResponse>(201);

        group.MapPut("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (
            int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return result is not null ? TypedResults.Ok(result) : TypedResults.NotFound();
        })
        .WithName("UpdateMember")
        .WithSummary("Update member profile")
        .WithDescription("Updates an existing member's profile information.")
        .Produces<MemberResponse>(200)
        .Produces(404);

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.DeactivateAsync(id, ct);
            return result ? TypedResults.NoContent() : TypedResults.NotFound();
        })
        .WithName("DeactivateMember")
        .WithSummary("Deactivate a member")
        .WithDescription("Deactivates a member. Fails if they have active bookings.")
        .Produces(204)
        .Produces(404);

        group.MapGet("/{id:int}/bookings", async Task<Ok<PaginatedResponse<BookingResponse>>> (
            int id, IMemberService service,
            string? status = null, DateTime? fromDate = null, DateTime? toDate = null,
            int page = 1, int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageSize = Math.Clamp(pageSize, 1, 100);
            var result = await service.GetMemberBookingsAsync(id, status, fromDate, toDate, page, pageSize, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMemberBookings")
        .WithSummary("Get member's bookings")
        .WithDescription("Returns a paginated list of bookings for a member with optional status and date range filters.")
        .Produces<PaginatedResponse<BookingResponse>>(200);

        group.MapGet("/{id:int}/bookings/upcoming", async Task<Ok<IReadOnlyList<BookingResponse>>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.GetUpcomingBookingsAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMemberUpcomingBookings")
        .WithSummary("Get member's upcoming bookings")
        .WithDescription("Returns upcoming confirmed bookings for a member.")
        .Produces<IReadOnlyList<BookingResponse>>(200);

        group.MapGet("/{id:int}/memberships", async Task<Ok<IReadOnlyList<MembershipResponse>>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var result = await service.GetMemberMembershipsAsync(id, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMemberMemberships")
        .WithSummary("Get membership history")
        .WithDescription("Returns all memberships (current and historical) for a member.")
        .Produces<IReadOnlyList<MembershipResponse>>(200);
    }
}
