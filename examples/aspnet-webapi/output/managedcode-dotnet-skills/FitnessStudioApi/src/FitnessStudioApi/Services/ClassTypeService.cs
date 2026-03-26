using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public class ClassTypeService(FitnessDbContext db, ILogger<ClassTypeService> logger) : IClassTypeService
{
    public async Task<IReadOnlyList<ClassTypeDto>> GetAllAsync(
        string? difficulty, bool? isPremium, CancellationToken ct)
    {
        var query = db.ClassTypes.AsNoTracking().Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var dl))
            query = query.Where(c => c.DifficultyLevel == dl);

        if (isPremium.HasValue)
            query = query.Where(c => c.IsPremium == isPremium.Value);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync(ct);
    }

    public async Task<ClassTypeDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var classType = await db.ClassTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return classType is null ? null : MapToDto(classType);
    }

    public async Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto, CancellationToken ct)
    {
        if (await db.ClassTypes.AnyAsync(c => c.Name == dto.Name, ct))
            throw new ConflictException($"A class type with name '{dto.Name}' already exists.");

        var classType = new ClassType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDurationMinutes = dto.DefaultDurationMinutes,
            DefaultCapacity = dto.DefaultCapacity,
            IsPremium = dto.IsPremium,
            CaloriesPerSession = dto.CaloriesPerSession,
            DifficultyLevel = dto.DifficultyLevel
        };

        db.ClassTypes.Add(classType);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created class type {ClassName} with Id {ClassTypeId}", classType.Name, classType.Id);
        return MapToDto(classType);
    }

    public async Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto, CancellationToken ct)
    {
        var classType = await db.ClassTypes.FindAsync([id], ct)
            ?? throw new NotFoundException($"Class type with Id {id} not found.");

        if (await db.ClassTypes.AnyAsync(c => c.Name == dto.Name && c.Id != id, ct))
            throw new ConflictException($"A class type with name '{dto.Name}' already exists.");

        classType.Name = dto.Name;
        classType.Description = dto.Description;
        classType.DefaultDurationMinutes = dto.DefaultDurationMinutes;
        classType.DefaultCapacity = dto.DefaultCapacity;
        classType.IsPremium = dto.IsPremium;
        classType.CaloriesPerSession = dto.CaloriesPerSession;
        classType.DifficultyLevel = dto.DifficultyLevel;
        classType.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated class type {ClassTypeId}", id);

        return MapToDto(classType);
    }

    private static ClassTypeDto MapToDto(ClassType c) => new(
        c.Id, c.Name, c.Description, c.DefaultDurationMinutes,
        c.DefaultCapacity, c.IsPremium, c.CaloriesPerSession,
        c.DifficultyLevel, c.IsActive);
}
