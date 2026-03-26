using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassTypeService(FitnessDbContext db, ILogger<ClassTypeService> logger)
    : IClassTypeService
{
    public async Task<IReadOnlyList<ClassTypeResponse>> GetAllAsync(
        DifficultyLevel? difficulty, bool? isPremium, CancellationToken ct)
    {
        var query = db.ClassTypes.AsNoTracking().AsQueryable();

        if (difficulty.HasValue)
            query = query.Where(ct => ct.DifficultyLevel == difficulty.Value);

        if (isPremium.HasValue)
            query = query.Where(ct => ct.IsPremium == isPremium.Value);

        return await query
            .Where(ct => ct.IsActive)
            .OrderBy(ct => ct.Name)
            .Select(ct => MapToResponse(ct))
            .ToListAsync(ct);
    }

    public async Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var classType = await db.ClassTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(ct => ct.Id == id, ct);

        return classType is null ? null : MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct)
    {
        var nameExists = await db.ClassTypes.AnyAsync(ct => ct.Name == request.Name, ct);
        if (nameExists)
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");

        var classType = new ClassType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            DefaultCapacity = request.DefaultCapacity,
            IsPremium = request.IsPremium,
            CaloriesPerSession = request.CaloriesPerSession,
            DifficultyLevel = request.DifficultyLevel
        };

        db.ClassTypes.Add(classType);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created class type {ClassTypeName} with ID {ClassTypeId}",
            classType.Name, classType.Id);

        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");

        var nameConflict = await db.ClassTypes.AnyAsync(c => c.Name == request.Name && c.Id != id, ct);
        if (nameConflict)
            throw new InvalidOperationException($"A class type with name '{request.Name}' already exists.");

        classType.Name = request.Name;
        classType.Description = request.Description;
        classType.DefaultDurationMinutes = request.DefaultDurationMinutes;
        classType.DefaultCapacity = request.DefaultCapacity;
        classType.IsPremium = request.IsPremium;
        classType.CaloriesPerSession = request.CaloriesPerSession;
        classType.DifficultyLevel = request.DifficultyLevel;
        classType.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Updated class type {ClassTypeId}", classType.Id);
        return MapToResponse(classType);
    }

    private static ClassTypeResponse MapToResponse(ClassType ct) =>
        new(ct.Id, ct.Name, ct.Description, ct.DefaultDurationMinutes,
            ct.DefaultCapacity, ct.IsPremium, ct.CaloriesPerSession,
            ct.DifficultyLevel, ct.IsActive, ct.CreatedAt, ct.UpdatedAt);
}
