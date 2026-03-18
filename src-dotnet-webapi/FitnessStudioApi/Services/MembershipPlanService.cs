using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class MembershipPlanService(FitnessDbContext db) : IMembershipPlanService
{
    public async Task<List<MembershipPlanResponse>> GetAllActiveAsync(CancellationToken ct)
    {
        return await db.MembershipPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);
    }

    public async Task<MembershipPlanResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var plan = await db.MembershipPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return plan is null ? null : MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct)
    {
        var existing = await db.MembershipPlans
            .AsNoTracking()
            .AnyAsync(p => p.Name == request.Name, ct);

        if (existing)
            throw new InvalidOperationException($"A membership plan with name '{request.Name}' already exists.");

        var plan = new MembershipPlan
        {
            Name = request.Name,
            Description = request.Description,
            DurationMonths = request.DurationMonths,
            Price = request.Price,
            MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = request.AllowsPremiumClasses,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.MembershipPlans.Add(plan);
        await db.SaveChangesAsync(ct);

        return MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        var duplicate = await db.MembershipPlans
            .AsNoTracking()
            .AnyAsync(p => p.Name == request.Name && p.Id != id, ct);

        if (duplicate)
            throw new InvalidOperationException($"A membership plan with name '{request.Name}' already exists.");

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.DurationMonths = request.DurationMonths;
        plan.Price = request.Price;
        plan.MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = request.AllowsPremiumClasses;
        plan.IsActive = request.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return MapToResponse(plan);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var plan = await db.MembershipPlans.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    private static MembershipPlanResponse MapToResponse(MembershipPlan p) => new()
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
