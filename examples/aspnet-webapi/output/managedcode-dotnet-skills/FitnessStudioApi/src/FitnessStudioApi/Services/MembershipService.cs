using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<MembershipDto> CreateAsync(CreateMembershipDto dto, CancellationToken ct)
    {
        var member = await db.Members.FindAsync([dto.MemberId], ct)
            ?? throw new NotFoundException($"Member with Id {dto.MemberId} not found.");

        var plan = await db.MembershipPlans.FindAsync([dto.MembershipPlanId], ct)
            ?? throw new NotFoundException($"Membership plan with Id {dto.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot create membership for an inactive plan.");

        var hasActiveMembership = await db.Memberships
            .AnyAsync(m => m.MemberId == dto.MemberId &&
                (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActiveMembership)
            throw new BusinessRuleException("Member already has an active or frozen membership.");

        var paymentStatus = Enum.TryParse<PaymentStatus>(dto.PaymentStatus, true, out var ps)
            ? ps : PaymentStatus.Paid;

        var membership = new Membership
        {
            MemberId = dto.MemberId,
            MembershipPlanId = dto.MembershipPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = paymentStatus
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created membership {MembershipId} for member {MemberId} on plan {PlanName}",
            membership.Id, member.Id, plan.Name);

        return await GetByIdAsync(membership.Id, ct)
            ?? throw new InvalidOperationException("Failed to retrieve created membership.");
    }

    public async Task<MembershipDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var ms = await db.Memberships
            .AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return ms is null ? null : MapToDto(ms);
    }

    public async Task<MembershipDto> CancelAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException($"Membership with Id {id} not found.");

        if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
            throw new BusinessRuleException($"Cannot cancel a membership with status '{membership.Status}'.");

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Cancelled membership {MembershipId}", id);

        return MapToDto(membership);
    }

    public async Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException($"Membership with Id {id} not found.");

        if (membership.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (membership.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once during this term.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = today;
        membership.FreezeEndDate = today.AddDays(dto.FreezeDurationDays);
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Froze membership {MembershipId} for {Days} days", id, dto.FreezeDurationDays);

        return MapToDto(membership);
    }

    public async Task<MembershipDto> UnfreezeAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException($"Membership with Id {id} not found.");

        if (membership.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var freezeStart = membership.FreezeStartDate!.Value;
        var frozenDays = today.DayNumber - freezeStart.DayNumber;
        if (frozenDays < 0) frozenDays = 0;

        membership.Status = MembershipStatus.Active;
        membership.FreezeEndDate = today;
        membership.EndDate = membership.EndDate.AddDays(frozenDays);
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Unfroze membership {MembershipId}, extended EndDate by {Days} days", id, frozenDays);

        return MapToDto(membership);
    }

    public async Task<MembershipDto> RenewAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException($"Membership with Id {id} not found.");

        if (membership.Status is not (MembershipStatus.Expired or MembershipStatus.Cancelled))
            throw new BusinessRuleException("Only expired or cancelled memberships can be renewed.");

        var hasActiveMembership = await db.Memberships
            .AnyAsync(m => m.MemberId == membership.MemberId && m.Id != id &&
                (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActiveMembership)
            throw new BusinessRuleException("Member already has an active or frozen membership.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
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

        logger.LogInformation("Renewed membership for member {MemberId}, new membership {NewMembershipId}",
            membership.MemberId, newMembership.Id);

        return (await GetByIdAsync(newMembership.Id, ct))!;
    }

    private static MembershipDto MapToDto(Membership ms) => new(
        ms.Id, ms.MemberId,
        $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId, ms.MembershipPlan.Name,
        ms.StartDate, ms.EndDate,
        ms.Status.ToString(), ms.PaymentStatus.ToString(),
        ms.FreezeStartDate, ms.FreezeEndDate);
}
