using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs.MembershipPlan;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipPlanService : IMembershipPlanService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<MembershipPlanService> _logger;

    public MembershipPlanService(FitnessDbContext context, ILogger<MembershipPlanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MembershipPlanDto>> GetAllAsync(bool? isActive = null)
    {
        var query = _context.MembershipPlans.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        return await query.Select(p => MapToDto(p)).ToListAsync();
    }

    public async Task<MembershipPlanDto> GetByIdAsync(int id)
    {
        var plan = await _context.MembershipPlans.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        return MapToDto(plan);
    }

    public async Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto)
    {
        if (await _context.MembershipPlans.AnyAsync(p => p.Name == dto.Name))
            throw new BusinessRuleException($"A membership plan with name '{dto.Name}' already exists.", 409, "Duplicate Resource");

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

    public async Task<MembershipPlanDto> UpdateAsync(int id, UpdateMembershipPlanDto dto)
    {
        var plan = await _context.MembershipPlans.FindAsync(id)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        if (await _context.MembershipPlans.AnyAsync(p => p.Name == dto.Name && p.Id != id))
            throw new BusinessRuleException($"A membership plan with name '{dto.Name}' already exists.", 409, "Duplicate Resource");

        plan.Name = dto.Name;
        plan.Description = dto.Description;
        plan.DurationMonths = dto.DurationMonths;
        plan.Price = dto.Price;
        plan.MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = dto.AllowsPremiumClasses;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated membership plan: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);
        return MapToDto(plan);
    }

    public async Task DeleteAsync(int id)
    {
        var plan = await _context.MembershipPlans.FindAsync(id)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        plan.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deactivated membership plan: {PlanName} (ID: {PlanId})", plan.Name, plan.Id);
    }

    private static MembershipPlanDto MapToDto(MembershipPlan plan) => new()
    {
        Id = plan.Id,
        Name = plan.Name,
        Description = plan.Description,
        DurationMonths = plan.DurationMonths,
        Price = plan.Price,
        MaxClassBookingsPerWeek = plan.MaxClassBookingsPerWeek,
        AllowsPremiumClasses = plan.AllowsPremiumClasses,
        IsActive = plan.IsActive,
        CreatedAt = plan.CreatedAt,
        UpdatedAt = plan.UpdatedAt
    };
}
