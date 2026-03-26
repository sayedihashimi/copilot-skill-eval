using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct = default)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {request.MembershipPlanId} not found.");

        if (!plan.IsActive)
        {
            throw new InvalidOperationException("Cannot create membership with an inactive plan.");
        }

        var hasActive = await db.Memberships.AnyAsync(
            ms => ms.MemberId == request.MemberId &&
                  (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
        {
            throw new InvalidOperationException("Member already has an active or frozen membership.");
        }

        var membership = new Membership
        {
            MemberId = request.MemberId,
            MembershipPlanId = request.MembershipPlanId,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = request.PaymentStatus,
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Membership {MembershipId} created for member {MemberId} on plan {PlanName}",
            membership.Id, member.Id, plan.Name);

        return await GetByIdAsync(membership.Id, ct)
            ?? throw new InvalidOperationException("Failed to retrieve created membership.");
    }

    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var ms = await db.Memberships
            .AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (ms is null)
        {
            return null;
        }

        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancellationToken ct = default)
    {
        var ms = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
        {
            throw new InvalidOperationException($"Cannot cancel a membership that is already {ms.Status}.");
        }

        ms.Status = MembershipStatus.Cancelled;
        ms.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Membership {MembershipId} cancelled", id);

        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct = default)
    {
        var ms = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Active)
        {
            throw new InvalidOperationException("Only active memberships can be frozen.");
        }

        if (ms.FreezeStartDate.HasValue)
        {
            throw new InvalidOperationException("This membership has already been frozen once. Only one freeze per term is allowed.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        ms.Status = MembershipStatus.Frozen;
        ms.FreezeStartDate = today;
        ms.FreezeEndDate = today.AddDays(request.FreezeDays);
        ms.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Membership {MembershipId} frozen for {Days} days", id, request.FreezeDays);

        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct = default)
    {
        var ms = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Frozen)
        {
            throw new InvalidOperationException("Only frozen memberships can be unfrozen.");
        }

        var freezeDuration = ms.FreezeEndDate!.Value.DayNumber - ms.FreezeStartDate!.Value.DayNumber;
        ms.EndDate = ms.EndDate.AddDays(freezeDuration);
        ms.Status = MembershipStatus.Active;
        ms.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Membership {MembershipId} unfrozen, end date extended by {Days} days", id, freezeDuration);

        return MapToResponse(ms);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct = default)
    {
        var ms = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Expired)
        {
            throw new InvalidOperationException("Only expired memberships can be renewed.");
        }

        var hasActive = await db.Memberships.AnyAsync(
            m => m.MemberId == ms.MemberId &&
                 m.Id != id &&
                 (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
        {
            throw new InvalidOperationException("Member already has an active or frozen membership.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newMembership = new Membership
        {
            MemberId = ms.MemberId,
            MembershipPlanId = ms.MembershipPlanId,
            StartDate = today,
            EndDate = today.AddMonths(ms.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid,
        };

        db.Memberships.Add(newMembership);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Membership renewed for member {MemberId}, new membership {NewMembershipId}", ms.MemberId, newMembership.Id);

        return await GetByIdAsync(newMembership.Id, ct)
            ?? throw new InvalidOperationException("Failed to retrieve renewed membership.");
    }

    private static MembershipResponse MapToResponse(Membership ms) => new(
        ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId, ms.MembershipPlan.Name,
        ms.StartDate, ms.EndDate, ms.Status, ms.PaymentStatus,
        ms.FreezeStartDate, ms.FreezeEndDate, ms.CreatedAt, ms.UpdatedAt);
}
