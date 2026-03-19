using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Services;

public class MembershipPlanService : IMembershipPlanService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<MembershipPlanService> _logger;

    public MembershipPlanService(FitnessDbContext context, ILogger<MembershipPlanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MembershipPlanDto>> GetAllAsync()
    {
        return await _context.MembershipPlans
            .Where(p => p.IsActive)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<MembershipPlanDto?> GetByIdAsync(int id)
    {
        var plan = await _context.MembershipPlans.FindAsync(id);
        return plan == null ? null : MapToDto(plan);
    }

    public async Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto)
    {
        if (await _context.MembershipPlans.AnyAsync(p => p.Name == dto.Name))
            throw new InvalidOperationException($"A membership plan with name '{dto.Name}' already exists.");

        var plan = new MembershipPlan
        {
            Name = dto.Name,
            Description = dto.Description,
            DurationMonths = dto.DurationMonths,
            Price = dto.Price,
            MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = dto.AllowsPremiumClasses
        };

        _context.MembershipPlans.Add(plan);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created membership plan: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);
        return MapToDto(plan);
    }

    public async Task<MembershipPlanDto?> UpdateAsync(int id, UpdateMembershipPlanDto dto)
    {
        var plan = await _context.MembershipPlans.FindAsync(id);
        if (plan == null) return null;

        if (await _context.MembershipPlans.AnyAsync(p => p.Name == dto.Name && p.Id != id))
            throw new InvalidOperationException($"A membership plan with name '{dto.Name}' already exists.");

        plan.Name = dto.Name;
        plan.Description = dto.Description;
        plan.DurationMonths = dto.DurationMonths;
        plan.Price = dto.Price;
        plan.MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = dto.AllowsPremiumClasses;
        plan.IsActive = dto.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(plan);
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var plan = await _context.MembershipPlans.FindAsync(id);
        if (plan == null) return false;

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deactivated membership plan: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);
        return true;
    }

    private static MembershipPlanDto MapToDto(MembershipPlan p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        DurationMonths = p.DurationMonths,
        Price = p.Price,
        MaxClassBookingsPerWeek = p.MaxClassBookingsPerWeek,
        AllowsPremiumClasses = p.AllowsPremiumClasses,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
