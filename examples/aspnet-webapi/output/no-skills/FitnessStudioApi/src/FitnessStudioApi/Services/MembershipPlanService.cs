using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class MembershipPlanService : IMembershipPlanService
{
    private readonly FitnessDbContext _db;

    public MembershipPlanService(FitnessDbContext db) => _db = db;

    public async Task<PaginatedResult<MembershipPlanDto>> GetAllAsync(int page, int pageSize, bool? isActive)
    {
        var query = _db.MembershipPlans.AsQueryable();
        if (isActive.HasValue) query = query.Where(p => p.IsActive == isActive.Value);
        else query = query.Where(p => p.IsActive);

        var total = await query.CountAsync();
        var items = await query.OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => ToDto(p)).ToListAsync();

        return new PaginatedResult<MembershipPlanDto>(items, total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<MembershipPlanDto?> GetByIdAsync(int id)
    {
        var plan = await _db.MembershipPlans.FindAsync(id);
        return plan == null ? null : ToDto(plan);
    }

    public async Task<MembershipPlanDto> CreateAsync(CreateMembershipPlanDto dto)
    {
        if (await _db.MembershipPlans.AnyAsync(p => p.Name == dto.Name))
            throw new BusinessRuleException($"A membership plan with name '{dto.Name}' already exists.", 409, "Conflict");

        var plan = new MembershipPlan
        {
            Name = dto.Name,
            Description = dto.Description,
            DurationMonths = dto.DurationMonths,
            Price = dto.Price,
            MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = dto.AllowsPremiumClasses
        };

        _db.MembershipPlans.Add(plan);
        await _db.SaveChangesAsync();
        return ToDto(plan);
    }

    public async Task<MembershipPlanDto> UpdateAsync(int id, UpdateMembershipPlanDto dto)
    {
        var plan = await _db.MembershipPlans.FindAsync(id)
            ?? throw new BusinessRuleException("Membership plan not found.", 404, "Not Found");

        if (await _db.MembershipPlans.AnyAsync(p => p.Name == dto.Name && p.Id != id))
            throw new BusinessRuleException($"A membership plan with name '{dto.Name}' already exists.", 409, "Conflict");

        plan.Name = dto.Name;
        plan.Description = dto.Description;
        plan.DurationMonths = dto.DurationMonths;
        plan.Price = dto.Price;
        plan.MaxClassBookingsPerWeek = dto.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = dto.AllowsPremiumClasses;
        plan.IsActive = dto.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(plan);
    }

    public async Task DeleteAsync(int id)
    {
        var plan = await _db.MembershipPlans.FindAsync(id)
            ?? throw new BusinessRuleException("Membership plan not found.", 404, "Not Found");

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static MembershipPlanDto ToDto(MembershipPlan p) => new(
        p.Id, p.Name, p.Description, p.DurationMonths, p.Price,
        p.MaxClassBookingsPerWeek, p.AllowsPremiumClasses, p.IsActive,
        p.CreatedAt, p.UpdatedAt);
}
