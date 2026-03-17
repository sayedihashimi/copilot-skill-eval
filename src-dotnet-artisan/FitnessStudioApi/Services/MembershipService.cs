using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService(FitnessDbContext db, ILogger<MembershipService> logger) : IMembershipService
{
    private readonly FitnessDbContext _db = db;
    private readonly ILogger<MembershipService> _logger = logger;

    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await _db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        var plan = await _db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {request.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot purchase an inactive membership plan.");

        var hasActiveOrFrozen = await _db.Memberships.AnyAsync(
            m => m.MemberId == request.MemberId &&
                 (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActiveOrFrozen)
            throw new ConflictException("Member already has an active or frozen membership.");

        var membership = new Membership
        {
            MemberId = request.MemberId,
            MembershipPlanId = request.MembershipPlanId,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = request.PaymentStatus
        };

        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created membership {Id} for member {MemberId} on plan {PlanName}",
            membership.Id, member.Id, plan.Name);

        return MapToResponse(membership, member, plan);
    }

    public async Task<MembershipResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var membership = await _db.Memberships
            .AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancellationToken ct)
    {
        var membership = await GetMembershipWithRelations(id, ct);

        if (membership.Status is MembershipStatus.Cancelled or MembershipStatus.Expired)
            throw new BusinessRuleException($"Cannot cancel a membership that is already {membership.Status}.");

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Cancelled membership {Id}", id);
        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var membership = await GetMembershipWithRelations(id, ct);

        if (membership.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (request.DurationDays < 7 || request.DurationDays > 30)
            throw new BusinessRuleException("Freeze duration must be between 7 and 30 days.");

        var hasExistingFreeze = membership.FreezeStartDate.HasValue;
        if (hasExistingFreeze)
            throw new BusinessRuleException("This membership has already been frozen once during the current term.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = today;
        membership.FreezeEndDate = today.AddDays(request.DurationDays);
        membership.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Froze membership {Id} for {Days} days", id, request.DurationDays);
        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct)
    {
        var membership = await GetMembershipWithRelations(id, ct);

        if (membership.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var freezeDuration = membership.FreezeStartDate.HasValue
            ? today.DayNumber - membership.FreezeStartDate.Value.DayNumber
            : 0;

        membership.Status = MembershipStatus.Active;
        membership.EndDate = membership.EndDate.AddDays(freezeDuration);
        membership.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Unfroze membership {Id}, extended end date by {Days} days", id, freezeDuration);
        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct)
    {
        var membership = await GetMembershipWithRelations(id, ct);

        if (membership.Status != MembershipStatus.Expired)
            throw new BusinessRuleException("Only expired memberships can be renewed.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.StartDate = today;
        membership.EndDate = today.AddMonths(membership.MembershipPlan.DurationMonths);
        membership.Status = MembershipStatus.Active;
        membership.PaymentStatus = PaymentStatus.Paid;
        membership.FreezeStartDate = null;
        membership.FreezeEndDate = null;
        membership.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Renewed membership {Id}", id);
        return MapToResponse(membership, membership.Member, membership.MembershipPlan);
    }

    private async Task<Membership> GetMembershipWithRelations(int id, CancellationToken ct)
    {
        return await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");
    }

    private static MembershipResponse MapToResponse(Membership ms, Member m, MembershipPlan p) => new(
        ms.Id, ms.MemberId, $"{m.FirstName} {m.LastName}",
        ms.MembershipPlanId, p.Name,
        ms.StartDate, ms.EndDate, ms.Status, ms.PaymentStatus,
        ms.FreezeStartDate, ms.FreezeEndDate,
        ms.CreatedAt, ms.UpdatedAt);
}
