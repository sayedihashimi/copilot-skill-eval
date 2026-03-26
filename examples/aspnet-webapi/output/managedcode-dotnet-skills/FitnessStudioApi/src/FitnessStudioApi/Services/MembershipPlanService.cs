using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MembershipPlanService(FitnessDbContext db, ILogger<MembershipPlanService> logger) : IMembershipPlanService
{
    public async Task<IReadOnlyList<MembershipPlanDto>> GetAllActiveAsync(CancellationToken ct)
    {
        return await db.MembershipPlans
            .Where(p => p.IsActive)
            .AsNoTracking()
            .Select(p => MapToDto(p))
            .ToListAsync(ct);
    }

    public async Task<MembershipPlanDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var plan = await db.MembershipPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return plan is null ? null : MapToDto(plan);
    }

    public async Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto, CancellationToken ct)
    {
        if (await db.MembershipPlans.AnyAsync(p => p.Name == dto.Name, ct))
            throw new ConflictException($"A membership plan with name '{dto.Name}' already exists.");

        var plan = new MembershipPlan
        {
            Name = dto.Name,
            Description = dto.Description,
            DurationMonths = dto.DurationMonths,
            Price = dto.Price,
            MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = dto.AllowsPremiumClasses
        };

        db.MembershipPlans.Add(plan);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created membership plan {PlanName} with Id {PlanId}", plan.Name, plan.Id);
        return MapToDto(plan);
    }

    public async Task<MembershipPlanDto> UpdateAsync(int id, UpdateMembershipPlanDto dto, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct)
            ?? throw new NotFoundException($"Membership plan with Id {id} not found.");

        if (await db.MembershipPlans.AnyAsync(p => p.Name == dto.Name && p.Id != id, ct))
            throw new ConflictException($"A membership plan with name '{dto.Name}' already exists.");

        plan.Name = dto.Name;
        plan.Description = dto.Description;
        plan.DurationMonths = dto.DurationMonths;
        plan.Price = dto.Price;
        plan.MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = dto.AllowsPremiumClasses;
        plan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated membership plan {PlanId}", id);
        return MapToDto(plan);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct)
            ?? throw new NotFoundException($"Membership plan with Id {id} not found.");

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated membership plan {PlanId}", id);
    }

    private static MembershipPlanDto MapToDto(MembershipPlan p) => new(
        p.Id, p.Name, p.Description, p.DurationMonths,
        p.Price, p.MaxClassBookingsPerWeek, p.AllowsPremiumClasses, p.IsActive);
}
