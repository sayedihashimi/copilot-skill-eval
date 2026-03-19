using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return membership is null ? null : MapToResponse(membership);
    }

    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([request.MemberId], ct)
            ?? throw new BusinessRuleException("Member not found.");

        var plan = await db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new BusinessRuleException("Membership plan not found.");

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot create membership with an inactive plan.");

        // Check for existing active/frozen membership
        var hasActive = await db.Memberships.AnyAsync(m =>
            m.MemberId == request.MemberId &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership.");

        var membership = new Membership
        {
            MemberId = request.MemberId,
            MembershipPlanId = request.MembershipPlanId,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddMonths(plan.DurationMonths),
            PaymentStatus = request.PaymentStatus
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        // Reload with includes
        await db.Entry(membership).Reference(m => m.Member).LoadAsync(ct);
        await db.Entry(membership).Reference(m => m.MembershipPlan).LoadAsync(ct);

        logger.LogInformation("Created membership {MembershipId} for member {MemberId}", membership.Id, membership.MemberId);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancelMembershipRequest? request, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new BusinessRuleException("Membership not found.");

        if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
            throw new BusinessRuleException($"Cannot cancel a membership with status '{membership.Status}'.");

        membership.Status = MembershipStatus.Cancelled;
        membership.PaymentStatus = PaymentStatus.Refunded;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Cancelled membership {MembershipId}", membership.Id);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new BusinessRuleException("Membership not found.");

        if (membership.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (membership.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once this term.");

        var freezeDays = request.FreezeEndDate.DayNumber - request.FreezeStartDate.DayNumber;
        if (freezeDays < 7 || freezeDays > 30)
            throw new BusinessRuleException("Freeze duration must be between 7 and 30 days.");

        if (request.FreezeStartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleException("Freeze start date cannot be in the past.");

        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = request.FreezeStartDate;
        membership.FreezeEndDate = request.FreezeEndDate;
        membership.EndDate = membership.EndDate.AddDays(freezeDays);
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Froze membership {MembershipId} from {Start} to {End}",
            membership.Id, request.FreezeStartDate, request.FreezeEndDate);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new BusinessRuleException("Membership not found.");

        if (membership.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        membership.Status = MembershipStatus.Active;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Unfroze membership {MembershipId}", membership.Id);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new BusinessRuleException("Membership not found.");

        if (membership.Status is not (MembershipStatus.Active or MembershipStatus.Expired))
            throw new BusinessRuleException("Only active or expired memberships can be renewed.");

        // Check for an existing active/frozen membership that isn't this one
        var hasOtherActive = await db.Memberships.AnyAsync(m =>
            m.MemberId == membership.MemberId &&
            m.Id != id &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasOtherActive)
            throw new BusinessRuleException("Member already has another active or frozen membership.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newStart = membership.EndDate > today ? membership.EndDate : today;

        membership.StartDate = newStart;
        membership.EndDate = newStart.AddMonths(membership.MembershipPlan.DurationMonths);
        membership.Status = MembershipStatus.Active;
        membership.PaymentStatus = PaymentStatus.Pending;
        membership.FreezeStartDate = null;
        membership.FreezeEndDate = null;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Renewed membership {MembershipId}", membership.Id);
        return MapToResponse(membership);
    }

    private static MembershipResponse MapToResponse(Membership m) => new(
        m.Id, m.MemberId, $"{m.Member.FirstName} {m.Member.LastName}",
        m.MembershipPlanId, m.MembershipPlan.Name,
        m.StartDate, m.EndDate, m.Status, m.PaymentStatus,
        m.FreezeStartDate, m.FreezeEndDate, m.CreatedAt, m.UpdatedAt);
}
