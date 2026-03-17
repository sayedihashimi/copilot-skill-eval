using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class MembershipPlanService(FitnessDbContext db, ILogger<MembershipPlanService> logger) : IMembershipPlanService
{
    private readonly FitnessDbContext _db = db;
    private readonly ILogger<MembershipPlanService> _logger = logger;

    public async Task<IReadOnlyList<MembershipPlanResponse>> GetAllActivePlansAsync(CancellationToken ct)
    {
        return await _db.MembershipPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .Select(p => MapToResponse(p))
            .ToListAsync(ct);
    }

    public async Task<MembershipPlanResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var plan = await _db.MembershipPlans.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");
        return MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> CreateAsync(CreateMembershipPlanRequest request, CancellationToken ct)
    {
        if (request.Price <= 0)
            throw new BusinessRuleException("Price must be positive.");

        var exists = await _db.MembershipPlans.AnyAsync(p => p.Name == request.Name, ct);
        if (exists)
            throw new ConflictException($"A plan named '{request.Name}' already exists.");

        var plan = new MembershipPlan
        {
            Name = request.Name,
            Description = request.Description,
            DurationMonths = request.DurationMonths,
            Price = request.Price,
            MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek,
            AllowsPremiumClasses = request.AllowsPremiumClasses
        };

        _db.MembershipPlans.Add(plan);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created membership plan {PlanName} with ID {PlanId}", plan.Name, plan.Id);
        return MapToResponse(plan);
    }

    public async Task<MembershipPlanResponse> UpdateAsync(int id, UpdateMembershipPlanRequest request, CancellationToken ct)
    {
        var plan = await _db.MembershipPlans.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        if (request.Price <= 0)
            throw new BusinessRuleException("Price must be positive.");

        var nameConflict = await _db.MembershipPlans.AnyAsync(p => p.Name == request.Name && p.Id != id, ct);
        if (nameConflict)
            throw new ConflictException($"A plan named '{request.Name}' already exists.");

        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.DurationMonths = request.DurationMonths;
        plan.Price = request.Price;
        plan.MaxClassBookingsPerWeek = request.MaxClassBookingsPerWeek;
        plan.AllowsPremiumClasses = request.AllowsPremiumClasses;
        plan.IsActive = request.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(plan);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var plan = await _db.MembershipPlans.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Membership plan with ID {id} not found.");

        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deactivated membership plan {PlanId}", id);
    }

    private static MembershipPlanResponse MapToResponse(MembershipPlan p) => new(
        p.Id, p.Name, p.Description, p.DurationMonths, p.Price,
        p.MaxClassBookingsPerWeek, p.AllowsPremiumClasses, p.IsActive,
        p.CreatedAt, p.UpdatedAt);
}
