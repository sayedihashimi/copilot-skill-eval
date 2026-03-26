using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassTypeService(FitnessDbContext db) : IClassTypeService
{
    public async Task<List<ClassTypeResponse>> GetAllAsync(string? difficulty, bool? isPremium, CancellationToken ct = default)
    {
        var query = db.ClassTypes.AsNoTracking().Where(ct2 => ct2.IsActive);

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var level))
        {
            query = query.Where(ct2 => ct2.DifficultyLevel == level);
        }

        if (isPremium.HasValue)
        {
            query = query.Where(ct2 => ct2.IsPremium == isPremium.Value);
        }

        return await query
            .OrderBy(ct2 => ct2.Name)
            .Select(ct2 => MapToResponse(ct2))
            .ToListAsync(ct);
    }

    public async Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var classType = await db.ClassTypes.AsNoTracking().FirstOrDefaultAsync(ct2 => ct2.Id == id, ct);
        return classType is null ? null : MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct = default)
    {
        if (await db.ClassTypes.AnyAsync(ct2 => ct2.Name == request.Name, ct))
        {
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");
        }

        var classType = new ClassType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            DefaultCapacity = request.DefaultCapacity,
            IsPremium = request.IsPremium,
            CaloriesPerSession = request.CaloriesPerSession,
            DifficultyLevel = request.DifficultyLevel,
        };

        db.ClassTypes.Add(classType);
        await db.SaveChangesAsync(ct);

        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct = default)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct);
        if (classType is null)
        {
            return null;
        }

        if (await db.ClassTypes.AnyAsync(ct2 => ct2.Name == request.Name && ct2.Id != id, ct))
        {
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");
        }

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

    private static ClassTypeResponse MapToResponse(ClassType ct) => new(
        ct.Id, ct.Name, ct.Description, ct.DefaultDurationMinutes, ct.DefaultCapacity,
        ct.IsPremium, ct.CaloriesPerSession, ct.DifficultyLevel, ct.IsActive,
        ct.CreatedAt, ct.UpdatedAt);
}
