using FitnessStudioApi.DTOs;
using FitnessStudioApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitnessStudioApi.Endpoints;

public static class MemberEndpoints
{
    public static void MapMemberEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/members")
            .WithTags("Members");

        group.MapGet("/", async (string? search, bool? isActive, int? page, int? pageSize,
            IMemberService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetAllAsync(search, isActive, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMembers")
        .WithSummary("List members")
        .WithDescription("List members with optional search by name/email, filter by active status, and pagination.")
        .Produces<PaginatedResponse<MemberResponse>>();

        group.MapGet("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (
            int id, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.GetByIdAsync(id, ct);
            return member is null ? TypedResults.NotFound() : TypedResults.Ok(member);
        })
        .WithName("GetMemberById")
        .WithSummary("Get a member by ID")
        .WithDescription("Returns member details including active membership information.")
        .Produces<MemberResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.CreateAsync(request, ct);
            return TypedResults.Created($"/api/members/{member.Id}", member);
        })
        .WithName("CreateMember")
        .WithSummary("Register a new member")
        .WithDescription("Register a new member. Must be at least 16 years old.")
        .Produces<MemberResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async Task<Results<Ok<MemberResponse>, NotFound>> (
            int id, UpdateMemberRequest request, IMemberService service, CancellationToken ct) =>
        {
            var member = await service.UpdateAsync(id, request, ct);
            return TypedResults.Ok(member);
        })
        .WithName("UpdateMember")
        .WithSummary("Update a member's profile")
        .WithDescription("Update an existing member's profile information.")
        .Produces<MemberResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:int}", async (int id, IMemberService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteMember")
        .WithSummary("Deactivate a member")
        .WithDescription("Deactivates a member. Fails if the member has future confirmed bookings.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:int}/bookings", async (int id, string? status, DateTime? fromDate, DateTime? toDate,
            int? page, int? pageSize, IMemberService service, CancellationToken ct) =>
        {
            var p = Math.Clamp(page ?? 1, 1, int.MaxValue);
            var ps = Math.Clamp(pageSize ?? 20, 1, 100);
            var result = await service.GetBookingsAsync(id, status, fromDate, toDate, p, ps, ct);
            return TypedResults.Ok(result);
        })
        .WithName("GetMemberBookings")
        .WithSummary("Get a member's bookings")
        .WithDescription("Get bookings for a member with optional filtering by status and date range.")
        .Produces<PaginatedResponse<BookingResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/bookings/upcoming", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var bookings = await service.GetUpcomingBookingsAsync(id, ct);
            return TypedResults.Ok(bookings);
        })
        .WithName("GetMemberUpcomingBookings")
        .WithSummary("Get a member's upcoming bookings")
        .WithDescription("Returns all future confirmed bookings for a member, ordered by class start time.")
        .Produces<IReadOnlyList<BookingResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/memberships", async (int id, IMemberService service, CancellationToken ct) =>
        {
            var memberships = await service.GetMembershipsAsync(id, ct);
            return TypedResults.Ok(memberships);
        })
        .WithName("GetMemberMemberships")
        .WithSummary("Get membership history for a member")
        .WithDescription("Returns all memberships (current and historical) for a member.")
        .Produces<IReadOnlyList<MembershipResponse>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}
