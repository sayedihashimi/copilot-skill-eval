using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MembershipService(FitnessDbContext db) : IMembershipService
{
    public async Task<MembershipResponse> CreateAsync(CreateMembershipRequest request, CancellationToken ct)
    {
        var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == request.MemberId, ct)
            ?? throw new KeyNotFoundException($"Member with ID {request.MemberId} not found.");

        var plan = await db.MembershipPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.MembershipPlanId, ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {request.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new ArgumentException("Cannot create membership with an inactive plan.");

        var hasActive = await db.Memberships.AsNoTracking()
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
            PaymentStatus = request.PaymentStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Memberships.Add(membership);
        await db.SaveChangesAsync(ct);

        return (await GetByIdAsync(membership.Id, ct))!;
    }

    public async Task<MembershipResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var ms = await db.Memberships.AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        return ms is null ? null : MapToResponse(ms);
    }

    public async Task<MembershipResponse> CancelAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status == MembershipStatus.Cancelled)
            throw new InvalidOperationException("Membership is already cancelled.");

        if (membership.Status == MembershipStatus.Expired)
            throw new InvalidOperationException("Cannot cancel an expired membership.");

        membership.Status = MembershipStatus.Cancelled;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> FreezeAsync(int id, FreezeMembershipRequest request, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Active)
            throw new InvalidOperationException("Only active memberships can be frozen.");

        if (request.FreezeDurationDays < 7 || request.FreezeDurationDays > 30)
            throw new ArgumentException("Freeze duration must be between 7 and 30 days.");

        if (membership.FreezeStartDate.HasValue)
            throw new InvalidOperationException("This membership has already been frozen once during this term.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = today;
        membership.FreezeEndDate = today.AddDays(request.FreezeDurationDays);
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> UnfreezeAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Frozen)
            throw new InvalidOperationException("Only frozen memberships can be unfrozen.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var freezeDuration = today.DayNumber - (membership.FreezeStartDate?.DayNumber ?? today.DayNumber);

        membership.Status = MembershipStatus.Active;
        membership.EndDate = membership.EndDate.AddDays(freezeDuration);
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return MapToResponse(membership);
    }

    public async Task<MembershipResponse> RenewAsync(int id, CancellationToken ct)
    {
        var membership = await db.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Expired)
            throw new InvalidOperationException("Only expired memberships can be renewed.");

        var hasActive = await db.Memberships.AsNoTracking()
            .AnyAsync(ms => ms.MemberId == membership.MemberId && ms.Id != id &&
                           (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen), ct);

        if (hasActive)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        membership.StartDate = today;
        membership.EndDate = today.AddMonths(membership.MembershipPlan.DurationMonths);
        membership.Status = MembershipStatus.Active;
        membership.PaymentStatus = PaymentStatus.Paid;
        membership.FreezeStartDate = null;
        membership.FreezeEndDate = null;
        membership.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return MapToResponse(membership);
    }

    private static MembershipResponse MapToResponse(Membership ms) => new()
    {
        Id = ms.Id,
        MemberId = ms.MemberId,
        MemberName = $"{ms.Member.FirstName} {ms.Member.LastName}",
        MembershipPlanId = ms.MembershipPlanId,
        PlanName = ms.MembershipPlan.Name,
        StartDate = ms.StartDate,
        EndDate = ms.EndDate,
        Status = ms.Status,
        PaymentStatus = ms.PaymentStatus,
        FreezeStartDate = ms.FreezeStartDate,
        FreezeEndDate = ms.FreezeEndDate,
        CreatedAt = ms.CreatedAt,
        UpdatedAt = ms.UpdatedAt
    };
}
