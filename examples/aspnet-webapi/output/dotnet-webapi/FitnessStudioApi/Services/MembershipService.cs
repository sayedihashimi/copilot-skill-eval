using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger)
    : IMembershipService
{
    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .AsNoTracking()
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct);

        return membership is null ? null : MapToResponse(membership);
    }

    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {request.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new ArgumentException("Cannot purchase an inactive membership plan.");

        var hasActive = await db.Memberships
            .AnyAsync(ms => ms.MemberId == request.MemberId &&
                           (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

        var membership = new Membership
        {
            MemberId = request.MemberId,
            MembershipPlanId = request.MembershipPlanId,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        await db.Entry(membership).Reference(ms => ms.Member).LoadAsync(ct);
        await db.Entry(membership).Reference(ms => ms.MembershipPlan).LoadAsync(ct);

        logger.LogInformation("Created membership {MembershipId} for member {MemberId} on plan {PlanName}",
            membership.Id, membership.MemberId, plan.Name);

        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
            throw new InvalidOperationException($"Cannot cancel a membership that is {membership.Status}.");

        membership.Status = MembershipStatus.Cancelled;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled membership {MembershipId}", membership.Id);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Active)
            throw new InvalidOperationException("Only active memberships can be frozen.");

        if (membership.FreezeStartDate.HasValue)
            throw new InvalidOperationException("This membership has already been frozen once. Only one freeze per term is allowed.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = today;
        membership.FreezeEndDate = today.AddDays(request.FreezeDurationDays);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Froze membership {MembershipId} for {Days} days", membership.Id, request.FreezeDurationDays);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Frozen)
            throw new InvalidOperationException("Only frozen memberships can be unfrozen.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var freezeDuration = membership.FreezeStartDate.HasValue
            ? today.DayNumber - membership.FreezeStartDate.Value.DayNumber
            : 0;

        membership.Status = MembershipStatus.Active;
        membership.EndDate = membership.EndDate.AddDays(freezeDuration);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Unfroze membership {MembershipId}, extended end date by {Days} days",
            membership.Id, freezeDuration);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(ms => ms.Member)
            .Include(ms => ms.MembershipPlan)
            .FirstOrDefaultAsync(ms => ms.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Expired)
            throw new InvalidOperationException("Only expired memberships can be renewed.");

        var hasActive = await db.Memberships
            .AnyAsync(ms => ms.MemberId == membership.MemberId &&
                           ms.Id != id &&
                           (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var newMembership = new Membership
        {
            MemberId = membership.MemberId,
            MembershipPlanId = membership.MembershipPlanId,
            StartDate = today,
            EndDate = today.AddMonths(membership.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };

        db.Memberships.Add(newMembership);
        await db.SaveChangesAsync(ct);

        await db.Entry(newMembership).Reference(ms => ms.Member).LoadAsync(ct);
        await db.Entry(newMembership).Reference(ms => ms.MembershipPlan).LoadAsync(ct);

        logger.LogInformation("Renewed membership for member {MemberId}, new membership {MembershipId}",
            membership.MemberId, newMembership.Id);

        return MapToResponse(newMembership);
    }

    private static MembershipResponse MapToResponse(Membership ms) =>
        new(ms.Id, ms.MemberId,
            $"{ms.Member.FirstName} {ms.Member.LastName}",
            ms.MembershipPlanId, ms.MembershipPlan.Name,
            ms.StartDate, ms.EndDate,
            ms.Status, ms.PaymentStatus,
            ms.FreezeStartDate, ms.FreezeEndDate,
            ms.CreatedAt, ms.UpdatedAt);
}
