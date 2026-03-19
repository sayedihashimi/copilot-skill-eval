using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public interface IMembershipPlanService
{
    Task<IReadOnlyList<MembershipPlanResponse>> GetAllActiveAsync();
    Task<MembershipPlanResponse> GetByIdAsync(int id);
    Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request);
    Task<MembershipPlanResponse> UpdateAsync(int id, UpdateMembershipPlanRequest request);
    Task DeleteAsync(int id);
}

public class MembershipPlanService(FitnessDbContext db, ILogger<MembershipPlanService> logger) : IMembershipPlanService
{
    public async Task<IReadOnlyList<MembershipPlanResponse>> GetAllActiveAsync()
    {
        return await db.MembershipPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .Select(p => MapToResponse(p))
            .ToListAsync();
    }

    public async Task<MembershipPlanResponse> GetByIdAsync(int id)
    {
        var plan = await db.MembershipPlans.FindAsync(id)
            ?? throw new NotFoundException($"Membership plan with ID {id} not found");
        return MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request)
    {
        if (await db.MembershipPlans.AnyAsync(p => p.Name == request.Name))
            throw new BusinessRuleException($"A membership plan with name '{request.Name}' already exists");

        var plan = new MembershipPlan
        {
            Name = request.Name,
            Description = request.Description,
            DurationMonths = request.DurationMonths,
            Price = request.Price,
            MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = request.AllowsPremiumClasses
        };

        db.MembershipPlans.Add(plan);
        await db.SaveChangesAsync();
        logger.LogInformation("Created membership plan: {PlanName}", plan.Name);
        return MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> UpdateAsync(int id, UpdateMembershipPlanRequest request)
    {
        var plan = await db.MembershipPlans.FindAsync(id)
            ?? throw new NotFoundException($"Membership plan with ID {id} not found");

        if (await db.MembershipPlans.AnyAsync(p => p.Name == request.Name && p.Id != id))
            throw new BusinessRuleException($"A membership plan with name '{request.Name}' already exists");

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.DurationMonths = request.DurationMonths;
        plan.Price = request.Price;
        plan.MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = request.AllowsPremiumClasses;
        plan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated membership plan: {PlanId}", id);
        return MapToResponse(plan);
    }

    public async Task DeleteAsync(int id)
    {
        var plan = await db.MembershipPlans.FindAsync(id)
            ?? throw new NotFoundException($"Membership plan with ID {id} not found");

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Deactivated membership plan: {PlanId}", id);
    }

    private static MembershipPlanResponse MapToResponse(MembershipPlan p) => new(
        p.Id, p.Name, p.Description, p.DurationMonths, p.Price,
        p.MaxClassBookingsPerWeek, p.AllowsPremiumClasses, p.IsActive,
        p.CreatedAt, p.UpdatedAt);
}
