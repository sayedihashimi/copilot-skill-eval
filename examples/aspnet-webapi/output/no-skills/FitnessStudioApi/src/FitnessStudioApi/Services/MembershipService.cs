using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class MembershipService : IMembershipService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MembershipService> _logger;

    public MembershipService(FitnessDbContext db, ILogger<MembershipService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MembershipDto> CreateAsync(CreateMembershipDto dto)
    {
        var member = await _db.Members.FindAsync(dto.MemberId)
            ?? throw new BusinessRuleException("Member not found.", 404, "Not Found");

        var plan = await _db.MembershipPlans.FindAsync(dto.MembershipPlanId)
            ?? throw new BusinessRuleException("Membership plan not found.", 404, "Not Found");

        if (!plan.IsActive)
            throw new BusinessRuleException("This membership plan is no longer active.");

        // Check for existing active/frozen membership
        var hasActive = await _db.Memberships.AnyAsync(ms =>
            ms.MemberId == dto.MemberId &&
            (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen));

        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership. Cancel or let it expire first.");

        var membership = new Membership
        {
            MemberId = dto.MemberId,
            MembershipPlanId = dto.MembershipPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };

        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Membership created for member {MemberId} with plan {PlanName}", dto.MemberId, plan.Name);

        return await GetByIdInternalAsync(membership.Id);
    }

    public async Task<MembershipDto?> GetByIdAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id);

        return ms == null ? null : MembershipServiceHelper.ToDto(ms);
    }

    public async Task<MembershipDto> CancelAsync(int id)
    {
        var ms = await _db.Memberships.FindAsync(id)
            ?? throw new BusinessRuleException("Membership not found.", 404, "Not Found");

        if (ms.Status == MembershipStatus.Cancelled)
            throw new BusinessRuleException("Membership is already cancelled.");

        if (ms.Status == MembershipStatus.Expired)
            throw new BusinessRuleException("Cannot cancel an expired membership.");

        ms.Status = MembershipStatus.Cancelled;
        ms.PaymentStatus = PaymentStatus.Refunded;
        ms.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Membership {Id} cancelled", id);

        return await GetByIdInternalAsync(id);
    }

    public async Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto)
    {
        var ms = await _db.Memberships.FindAsync(id)
            ?? throw new BusinessRuleException("Membership not found.", 404, "Not Found");

        if (ms.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (ms.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once during this term.");

        ms.Status = MembershipStatus.Frozen;
        ms.FreezeStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        ms.FreezeEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(dto.FreezeDurationDays));
        ms.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Membership {Id} frozen for {Days} days", id, dto.FreezeDurationDays);

        return await GetByIdInternalAsync(id);
    }

    public async Task<MembershipDto> UnfreezeAsync(int id)
    {
        var ms = await _db.Memberships.FindAsync(id)
            ?? throw new BusinessRuleException("Membership not found.", 404, "Not Found");

        if (ms.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        if (!ms.FreezeStartDate.HasValue || !ms.FreezeEndDate.HasValue)
            throw new BusinessRuleException("Freeze dates are not set.");

        var freezeDuration = ms.FreezeEndDate.Value.DayNumber - ms.FreezeStartDate.Value.DayNumber;
        ms.EndDate = ms.EndDate.AddDays(freezeDuration);
        ms.Status = MembershipStatus.Active;
        ms.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Membership {Id} unfrozen, end date extended by {Days} days", id, freezeDuration);

        return await GetByIdInternalAsync(id);
    }

    public async Task<MembershipDto> RenewAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new BusinessRuleException("Membership not found.", 404, "Not Found");

        if (ms.Status != MembershipStatus.Expired && ms.Status != MembershipStatus.Cancelled)
            throw new BusinessRuleException("Only expired or cancelled memberships can be renewed.");

        // Check for existing active/frozen membership
        var hasActive = await _db.Memberships.AnyAsync(m =>
            m.MemberId == ms.MemberId && m.Id != id &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen));

        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newMembership = new Membership
        {
            MemberId = ms.MemberId,
            MembershipPlanId = ms.MembershipPlanId,
            StartDate = today,
            EndDate = today.AddMonths(ms.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };

        _db.Memberships.Add(newMembership);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Membership renewed for member {MemberId}", ms.MemberId);

        return await GetByIdInternalAsync(newMembership.Id);
    }

    private async Task<MembershipDto> GetByIdInternalAsync(int id)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstAsync(m => m.Id == id);

        return MembershipServiceHelper.ToDto(ms);
    }
}
