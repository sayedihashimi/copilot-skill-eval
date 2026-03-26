using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs.Membership;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipService : IMembershipService
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
            throw new BusinessRuleException("Cannot purchase an inactive membership plan.");

        // Check if member already has an active or frozen membership
        var hasActiveMembership = await _context.Memberships
            .AnyAsync(m => m.MemberId == dto.MemberId
                && (m.Status == MembershipStatus.Active || m.Status == MembershipStatus.Frozen));

        if (hasActiveMembership)
            throw new BusinessRuleException("Member already has an active or frozen membership. Cancel or let it expire first.");

        var membership = new Membership
        {
            MemberId = dto.MemberId,
            MembershipPlanId = dto.MembershipPlanId,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddMonths(plan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = Enum.TryParse<PaymentStatus>(dto.PaymentStatus, true, out var ps) ? ps : PaymentStatus.Paid
        };

        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created membership for member {MemberId} with plan {PlanName} (Membership ID: {MembershipId})",
            dto.MemberId, plan.Name, membership.Id);

        return await GetByIdAsync(membership.Id);
    }

    public async Task<MembershipDto> GetByIdAsync(int id)
    {
        var membership = await _context.Memberships.AsNoTracking()
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        return MapToDto(membership);
    }

    public async Task<MembershipDto> CancelAsync(int id)
    {
        var membership = await _context.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Active && membership.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException($"Cannot cancel a membership with status '{membership.Status}'.");

        membership.Status = MembershipStatus.Cancelled;
        membership.PaymentStatus = PaymentStatus.Refunded;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cancelled membership {MembershipId} for member {MemberId}", id, membership.MemberId);
        return MapToDto(membership);
    }

    public async Task<MembershipDto> FreezeAsync(int id, FreezeMembershipDto dto)
    {
        var membership = await _context.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Active)
            throw new BusinessRuleException("Only active memberships can be frozen.");

        if (membership.FreezeStartDate.HasValue)
            throw new BusinessRuleException("This membership has already been frozen once during this term.");

        var freezeStart = DateOnly.FromDateTime(DateTime.UtcNow);
        var freezeEnd = freezeStart.AddDays(dto.FreezeDurationDays);

        membership.Status = MembershipStatus.Frozen;
        membership.FreezeStartDate = freezeStart;
        membership.FreezeEndDate = freezeEnd;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Frozen membership {MembershipId} from {FreezeStart} to {FreezeEnd}",
            id, freezeStart, freezeEnd);
        return MapToDto(membership);
    }

    public async Task<MembershipDto> UnfreezeAsync(int id)
    {
        var membership = await _context.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Frozen)
            throw new BusinessRuleException("Only frozen memberships can be unfrozen.");

        if (!membership.FreezeStartDate.HasValue || !membership.FreezeEndDate.HasValue)
            throw new BusinessRuleException("Freeze dates are not set.");

        var freezeDuration = membership.FreezeEndDate.Value.DayNumber - membership.FreezeStartDate.Value.DayNumber;
        membership.EndDate = membership.EndDate.AddDays(freezeDuration);
        membership.Status = MembershipStatus.Active;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Unfrozen membership {MembershipId}, end date extended by {Days} days to {NewEndDate}",
            id, freezeDuration, membership.EndDate);
        return MapToDto(membership);
    }

    public async Task<MembershipDto> RenewAsync(int id)
    {
        var membership = await _context.Memberships
            .Include(m => m.Member)
            .Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Membership with ID {id} not found.");

        if (membership.Status != MembershipStatus.Expired)
            throw new BusinessRuleException("Only expired memberships can be renewed.");

        // Create a new active membership
        var newMembership = new Membership
        {
            MemberId = membership.MemberId,
            MembershipPlanId = membership.MembershipPlanId,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(membership.MembershipPlan.DurationMonths),
            Status = MembershipStatus.Active,
            PaymentStatus = PaymentStatus.Paid
        };

        _context.Memberships.Add(newMembership);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Renewed membership for member {MemberId}, new membership ID: {NewMembershipId}",
            membership.MemberId, newMembership.Id);

        return await GetByIdAsync(newMembership.Id);
    }

    private static MembershipDto MapToDto(Membership membership) => new()
    {
        Id = membership.Id,
        MemberId = membership.MemberId,
        MemberName = $"{membership.Member.FirstName} {membership.Member.LastName}",
        MembershipPlanId = membership.MembershipPlanId,
        PlanName = membership.MembershipPlan.Name,
        StartDate = membership.StartDate,
        EndDate = membership.EndDate,
        Status = membership.Status.ToString(),
        PaymentStatus = membership.PaymentStatus.ToString(),
        FreezeStartDate = membership.FreezeStartDate,
        FreezeEndDate = membership.FreezeEndDate,
        CreatedAt = membership.CreatedAt,
        UpdatedAt = membership.UpdatedAt
    };
}
