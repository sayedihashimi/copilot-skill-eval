using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipPlanService : IMembershipPlanService
{
    private readonly FitnessDbContext _db;

    public MembershipPlanService(FitnessDbContext db) => _db = db;

    public async Task<PaginatedResponse<MembershipPlanResponse>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = _db.MembershipPlans.AsNoTracking().Where(p => p.IsActive);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Price)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToResponse(p))
            .ToListAsync(ct);

        return new PaginatedResponse<MembershipPlanResponse>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var plan = await _db.MembershipPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return plan is null ? null : ToResponse(plan);
    }

    public async Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct)
    {
        if (await _db.MembershipPlans.AnyAsync(p => p.Name == request.Name, ct))
            throw new BusinessRuleException($"A membership plan with name '{request.Name}' already exists.", 409);

        var plan = new MembershipPlan
        {
            Name = request.Name,
            Description = request.Description,
            DurationMonths = request.DurationMonths,
            Price = request.Price,
            MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = request.AllowsPremiumClasses,
            IsActive = request.IsActive
        };

        _db.MembershipPlans.Add(plan);
        await _db.SaveChangesAsync(ct);
        return ToResponse(plan);
    }

    public async Task<MembershipPlanResponse?> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await _db.MembershipPlans.FindAsync([id], ct);
        if (plan is null) return null;

        if (await _db.MembershipPlans.AnyAsync(p => p.Name == request.Name && p.Id != id, ct))
            throw new BusinessRuleException($"A membership plan with name '{request.Name}' already exists.", 409);

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.DurationMonths = request.DurationMonths;
        plan.Price = request.Price;
        plan.MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = request.AllowsPremiumClasses;
        plan.IsActive = request.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToResponse(plan);
    }

    public async Task<bool> DeactivateAsync(int id, CancellationToken ct)
    {
        var plan = await _db.MembershipPlans.FindAsync([id], ct);
        if (plan is null) return false;

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static MembershipPlanResponse ToResponse(MembershipPlan p) => new(
        p.Id, p.Name, p.Description, p.DurationMonths, p.Price,
        p.MaxClassBookingsPerWeek, p.AllowsPremiumClasses, p.IsActive,
        p.CreatedAt, p.UpdatedAt
    );
}
