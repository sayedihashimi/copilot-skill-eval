using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class ClassTypeService(FitnessDbContext db) : IClassTypeService
{
    public async Task<List<ClassTypeResponse>> GetAllAsync(DifficultyLevel? difficulty, bool? isPremium, CancellationToken ct)
    {
        var query = db.ClassTypes.AsNoTracking().Where(c => c.IsActive);

        if (difficulty.HasValue)
            query = query.Where(c => c.DifficultyLevel == difficulty.Value);

        if (isPremium.HasValue)
            query = query.Where(c => c.IsPremium == isPremium.Value);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => MapToResponse(c))
            .ToListAsync(ct);
    }

    public async Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var classType = await db.ClassTypes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return classType is null ? null : MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct)
    {
        var exists = await db.ClassTypes.AsNoTracking()
            .AnyAsync(c => c.Name == request.Name, ct);

        if (exists)
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");

        var classType = new ClassType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            DefaultCapacity = request.DefaultCapacity,
            IsPremium = request.IsPremium,
            CaloriesPerSession = request.CaloriesPerSession,
            DifficultyLevel = request.DifficultyLevel,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.ClassTypes.Add(classType);
        await db.SaveChangesAsync(ct);

        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");

        var duplicate = await db.ClassTypes.AsNoTracking()
            .AnyAsync(c => c.Name == request.Name && c.Id != id, ct);

        if (duplicate)
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");

        classType.Name = request.Name;
        classType.Description = request.Description;
        classType.DefaultDurationMinutes = request.DefaultDurationMinutes;
        classType.DefaultCapacity = request.DefaultCapacity;
        classType.IsPremium = request.IsPremium;
        classType.CaloriesPerSession = request.CaloriesPerSession;
        classType.DifficultyLevel = request.DifficultyLevel;
        classType.IsActive = request.IsActive;
        classType.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return MapToResponse(classType);
    }

    private static ClassTypeResponse MapToResponse(ClassType c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        DefaultDurationMinutes = c.DefaultDurationMinutes,
        DefaultCapacity = c.DefaultCapacity,
        IsPremium = c.IsPremium,
        CaloriesPerSession = c.CaloriesPerSession,
        DifficultyLevel = c.DifficultyLevel,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
