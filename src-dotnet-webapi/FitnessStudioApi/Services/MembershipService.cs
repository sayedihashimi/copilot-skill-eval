using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService : IMembershipService
{
    private readonly FitnessDbContext _db;
    private readonly ILogger<MembershipService> _logger;

    public MembershipService(FitnessDbContext db, ILogger<MembershipService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await _db.Members.FindAsync([request.MemberId], ct)
            ?? throw new KeyNotFoundException($"Member with id {request.MemberId} not found.");

        var plan = await _db.MembershipPlans.FindAsync([request.MembershipPlanId], ct)
            ?? throw new KeyNotFoundException($"Membership plan with id {request.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new BusinessRuleException("Cannot purchase an inactive membership plan.");

        // Check for existing active/frozen membership
        var hasActive = await _db.Memberships.AnyAsync(ms =>
            ms.MemberId == request.MemberId &&
            (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership.", 409);

        var membership = new Membership
        {
            MemberId = request.MemberId,
            MembershipPlanId = request.MembershipPlanId,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = Enum.TryParse<PaymentStatus>(request.PaymentStatus, true, out var ps) ? ps : PaymentStatus.Paid
        };

        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Membership {MembershipId} created for member {MemberId} on plan {PlanName}",
            membership.Id, member.Id, plan.Name);

        return ToResponse(membership, member, plan);
    }

    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var ms = await _db.Memberships.AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return ms is null ? null : ToResponse(ms, ms.Member, ms.MembershipPlan);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancellationToken ct)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with id {id} not found.");

        if (ms.Status == MembershipStatus.Cancelled)
            throw new BusinessRuleException("Membership is already cancelled.");

        ms.Status = MembershipStatus.Cancelled;
        ms.PaymentStatus = PaymentStatus.Refunded;
        ms.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Membership {MembershipId} cancelled for member {MemberId}", ms.Id, ms.MemberId);
        return ToResponse(ms, ms.Member, ms.MembershipPlan);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with id {id} not found.");

        if (ms.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (ms.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once during this term.");

        ms.Status = MembershipStatus.Frozen;
        ms.FreezeStartDate = DateOnly.FromDateTime(DateTime.UtcNow);
        ms.FreezeEndDate = ms.FreezeStartDate.Value.AddDays(request.FreezeDurationDays);
        ms.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Membership {MembershipId} frozen for {Days} days", ms.Id, request.FreezeDurationDays);
        return ToResponse(ms, ms.Member, ms.MembershipPlan);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with id {id} not found.");

        if (ms.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        var freezeDuration = ms.FreezeEndDate!.Value.DayNumber - ms.FreezeStartDate!.Value.DayNumber;
        ms.EndDate = ms.EndDate.AddDays(freezeDuration);
        ms.Status = MembershipStatus.Active;
        ms.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Membership {MembershipId} unfrozen, end date extended by {Days} days", ms.Id, freezeDuration);
        return ToResponse(ms, ms.Member, ms.MembershipPlan);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct)
    {
        var ms = await _db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with id {id} not found.");

        if (ms.Status != MembershipStatus.Expired)
            throw new BusinessRuleException("Only expired memberships can be renewed.");

        // Check no other active membership
        var hasActive = await _db.Memberships.AnyAsync(m =>
            m.MemberId == ms.MemberId && m.Id != ms.Id &&
            (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
            throw new BusinessRuleException("Member already has an active or frozen membership.", 409);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var newMembership = new Membership
        {
            MemberId = ms.MemberId,
            MembershipPlanId = ms.MembershipPlanId,
            StartDate = startDate,
            EndDate = startDate.AddMonths(ms.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };

        _db.Memberships.Add(newMembership);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Membership renewed for member {MemberId}, new membership {MembershipId}", ms.MemberId, newMembership.Id);
        return ToResponse(newMembership, ms.Member, ms.MembershipPlan);
    }

    private static MembershipResponse ToResponse(Membership ms, Member m, MembershipPlan p) => new(
        ms.Id, ms.MemberId, $"{m.FirstName} {m.LastName}",
        ms.MembershipPlanId, p.Name,
        ms.StartDate, ms.EndDate,
        ms.Status.ToString(), ms.PaymentStatus.ToString(),
        ms.FreezeStartDate, ms.FreezeEndDate,
        ms.CreatedAt, ms.UpdatedAt);
}
