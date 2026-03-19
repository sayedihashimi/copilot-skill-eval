using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Services;

public class MembershipService : IMembershipService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<MembershipService> _logger;

    public MembershipService(FitnessDbContext context, ILogger<MembershipService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MembershipDto> CreateAsync(CreateMembershipDto dto)
    {
        var member = await _context.Members.FindAsync(dto.MemberId)
            ?? throw new KeyNotFoundException($"Member with ID {dto.MemberId} not found.");

        var plan = await _context.MembershipPlans.FindAsync(dto.MembershipPlanId)
            ?? throw new KeyNotFoundException($"Membership plan with ID {dto.MembershipPlanId} not found.");

        if (!plan.IsActive)
            throw new InvalidOperationException("Cannot create membership with an inactive plan.");

        var hasActive = await _context.Memberships
            .AnyAsync(ms => ms.MemberId == dto.MemberId && (ms.Status == MembershipStatus.Active || ms.Status == MembershipStatus.Frozen));
        if (hasActive)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

        var startDate = dto.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var membership = new Membership
        {
            MemberId = dto.MemberId,
            MembershipPlanId = dto.MembershipPlanId,
            StartDate = startDate,
            EndDate = startDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = dto.PaymentStatus
        };

        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created membership (ID: {MembershipId}) for member {MemberId} with plan {PlanName}", membership.Id, dto.MemberId, plan.Name);

        return await GetDtoByIdAsync(membership.Id);
    }

    public async Task<MembershipDto?> GetByIdAsync(int id)
    {
        var ms = await _context.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id);

        return ms == null ? null : MapToDto(ms);
    }

    public async Task<MembershipDto> CancelAsync(int id)
    {
        var ms = await _context.Memberships.Include(m => m.MembershipPlan).Include(m => m.Member)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status == MembershipStatus.Cancelled)
            throw new InvalidOperationException("Membership is already cancelled.");

        if (ms.Status == MembershipStatus.Expired)
            throw new InvalidOperationException("Cannot cancel an expired membership.");

        ms.Status = MembershipStatus.Cancelled;
        ms.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cancelled membership (ID: {MembershipId})", id);
        return MapToDto(ms);
    }

    public async Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto)
    {
        var ms = await _context.Memberships.Include(m => m.MembershipPlan).Include(m => m.Member)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Active)
            throw new InvalidOperationException("Only active memberships can be frozen.");

        if (ms.FreezeStartDate.HasValue)
            throw new InvalidOperationException("This membership has already been frozen once. Only one freeze per term is allowed.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        ms.Status = MembershipStatus.Frozen;
        ms.FreezeStartDate = today;
        ms.FreezeEndDate = today.AddDays(dto.FreezeDurationDays);
        ms.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Frozen membership (ID: {MembershipId}) for {Days} days", id, dto.FreezeDurationDays);
        return MapToDto(ms);
    }

    public async Task<MembershipDto> UnfreezeAsync(int id)
    {
        var ms = await _context.Memberships.Include(m => m.MembershipPlan).Include(m => m.Member)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Frozen)
            throw new InvalidOperationException("Only frozen memberships can be unfrozen.");

        if (ms.FreezeStartDate.HasValue && ms.FreezeEndDate.HasValue)
        {
            var freezeDuration = ms.FreezeEndDate.Value.DayNumber - ms.FreezeStartDate.Value.DayNumber;
            ms.EndDate = ms.EndDate.AddDays(freezeDuration);
        }

        ms.Status = MembershipStatus.Active;
        ms.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Unfrozen membership (ID: {MembershipId}), EndDate extended", id);
        return MapToDto(ms);
    }

    public async Task<MembershipDto> RenewAsync(int id)
    {
        var ms = await _context.Memberships.Include(m => m.MembershipPlan).Include(m => m.Member)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (ms.Status != MembershipStatus.Expired && ms.Status != MembershipStatus.Cancelled)
            throw new InvalidOperationException("Only expired or cancelled memberships can be renewed.");

        var hasActive = await _context.Memberships
            .AnyAsync(m => m.MemberId == ms.MemberId && m.Id != id && (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen));
        if (hasActive)
            throw new InvalidOperationException("Member already has an active or frozen membership.");

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

        _context.Memberships.Add(newMembership);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Renewed membership for member {MemberId}, new membership ID: {NewId}", ms.MemberId, newMembership.Id);

        return await GetDtoByIdAsync(newMembership.Id);
    }

    private async Task<MembershipDto> GetDtoByIdAsync(int id)
    {
        var ms = await _context.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstAsync(m => m.Id == id);
        return MapToDto(ms);
    }

    private static MembershipDto MapToDto(Membership ms) => new()
    {
        Id = ms.Id,
        MemberId = ms.MemberId,
        MemberName = ms.Member != null ? $"{ms.Member.FirstName} {ms.Member.LastName}" : "",
        MembershipPlanId = ms.MembershipPlanId,
        PlanName = ms.MembershipPlan?.Name ?? "",
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
