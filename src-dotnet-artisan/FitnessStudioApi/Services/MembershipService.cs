using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public interface IMembershipService
{
    Task<MembershipResponse> GetByIdAsync(int id);
    Task<MembershipResponse> CreateAsync(CreateMembershipRequest request);
    Task<MembershipResponse> CancelAsync(int id);
    Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request);
    Task<MembershipResponse> UnfreezeAsync(int id);
    Task<MembershipResponse> RenewAsync(int id);
}

public class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    public async Task<MembershipResponse> GetByIdAsync(int id)
    {
        var membership = await GetMembershipWithRelations(id);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request)
    {
        var member = await db.Members.FindAsync(request.MemberId)
            ?? throw new NotFoundException($"Member with ID {request.MemberId} not found");

        var plan = await db.MembershipPlans.FindAsync(request.MembershipPlanId)
            ?? throw new NotFoundException($"Membership plan with ID {request.MembershipPlanId} not found");

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot create a membership with an inactive plan");

        var hasActiveOrFrozen = await db.Memberships.AnyAsync(m =>
            m.MemberId == request.MemberId &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen));

        if (hasActiveOrFrozen)
            throw new BusinessRuleException("Member already has an active or frozen membership");

        if (!Enum.TryParse<PaymentStatus>(request.PaymentStatus, true, out var paymentStatus))
            throw new BusinessRuleException($"Invalid payment status: {request.PaymentStatus}");

        var membership = new Membership
        {
            MemberId = request.MemberId,
            MembershipPlanId = request.MembershipPlanId,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = paymentStatus
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync();
        logger.LogInformation("Created membership {MembershipId} for member {MemberId}", membership.Id, request.MemberId);

        return MapToResponse(await GetMembershipWithRelations(membership.Id));
    }

    public async Task<MembershipResponse> CancelAsync(int id)
    {
        var membership = await GetMembershipWithRelations(id);

        if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
            throw new BusinessRuleException($"Cannot cancel a membership with status '{membership.Status}'");

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Cancelled membership: {MembershipId}", id);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request)
    {
        var membership = await GetMembershipWithRelations(id);

        if (membership.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen");

        if (request.FreezeDays < 7 || request.FreezeDays > 30)
            throw new BusinessRuleException("Freeze duration must be between 7 and 30 days");

        if (membership.FreezeStartDate is not null)
            throw new BusinessRuleException("This membership has already been frozen once during this term");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = today;
        membership.FreezeEndDate = today.AddDays(request.FreezeDays);
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Froze membership {MembershipId} for {Days} days", id, request.FreezeDays);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id)
    {
        var membership = await GetMembershipWithRelations(id);

        if (membership.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen");

        if (membership.FreezeStartDate is null || membership.FreezeEndDate is null)
            throw new BusinessRuleException("Freeze dates are not set");

        var freezeDuration = membership.FreezeEndDate.Value.DayNumber - membership.FreezeStartDate.Value.DayNumber;
        membership.EndDate = membership.EndDate.AddDays(freezeDuration);
        membership.Status = MembershipStatus.Active;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Unfroze membership {MembershipId}, extended end date by {Days} days", id, freezeDuration);
        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> RenewAsync(int id)
    {
        var membership = await GetMembershipWithRelations(id);

        if (membership.Status is not (MembershipStatus.Expired or MembershipStatus.Cancelled))
            throw new BusinessRuleException("Only expired or cancelled memberships can be renewed");

        var plan = membership.MembershipPlan;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var newMembership = new Membership
        {
            MemberId = membership.MemberId,
            MembershipPlanId = membership.MembershipPlanId,
            StartDate = today,
            EndDate = today.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Pending
        };

        db.Memberships.Add(newMembership);
        await db.SaveChangesAsync();
        logger.LogInformation("Renewed membership for member {MemberId}, new membership {NewId}", membership.MemberId, newMembership.Id);

        return MapToResponse(await GetMembershipWithRelations(newMembership.Id));
    }

    private async Task<Membership> GetMembershipWithRelations(int id)
    {
        return await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new NotFoundException($"Membership with ID {id} not found");
    }

    private static MembershipResponse MapToResponse(Membership ms) => new(
        ms.Id, ms.MemberId, $"{ms.Member.FirstName} {ms.Member.LastName}",
        ms.MembershipPlanId, ms.MembershipPlan.Name,
        ms.StartDate, ms.EndDate, ms.Status.ToString(), ms.PaymentStatus.ToString(),
        ms.FreezeStartDate, ms.FreezeEndDate, ms.CreatedAt, ms.UpdatedAt);
}
