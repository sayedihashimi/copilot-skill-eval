using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public interface IClassTypeService
{
    Task<IReadOnlyList<ClassTypeResponse>> GetAllAsync(string? difficulty, bool? isPremium);
    Task<ClassTypeResponse> GetByIdAsync(int id);
    Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request);
    Task<ClassTypeResponse> UpdateAsync(int id, UpdateClassTypeRequest request);
}

public class ClassTypeService(FitnessDbContext db, ILogger<ClassTypeService> logger) : IClassTypeService
{
    public async Task<IReadOnlyList<ClassTypeResponse>> GetAllAsync(string? difficulty, bool? isPremium)
    {
        var query = db.ClassTypes.Where(ct => ct.IsActive);

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var level))
            query = query.Where(ct => ct.DifficultyLevel == level);

        if (isPremium.HasValue)
            query = query.Where(ct => ct.IsPremium == isPremium.Value);

        return await query
            .OrderBy(ct => ct.Name)
            .Select(ct => MapToResponse(ct))
            .ToListAsync();
    }

    public async Task<ClassTypeResponse> GetByIdAsync(int id)
    {
        var classType = await db.ClassTypes.FindAsync(id)
            ?? throw new NotFoundException($"Class type with ID {id} not found");
        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request)
    {
        if (await db.ClassTypes.AnyAsync(ct => ct.Name == request.Name))
            throw new BusinessRuleException($"A class type with name '{request.Name}' already exists");

        if (!Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, true, out var level))
            throw new BusinessRuleException($"Invalid difficulty level: {request.DifficultyLevel}");

        var classType = new ClassType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            DefaultCapacity = request.DefaultCapacity,
            IsPremium = request.IsPremium,
            CaloriesPerSession = request.CaloriesPerSession,
            DifficultyLevel = level
        };

        db.ClassTypes.Add(classType);
        await db.SaveChangesAsync();
        logger.LogInformation("Created class type: {Name}", classType.Name);
        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> UpdateAsync(int id, UpdateClassTypeRequest request)
    {
        var classType = await db.ClassTypes.FindAsync(id)
            ?? throw new NotFoundException($"Class type with ID {id} not found");

        if (await db.ClassTypes.AnyAsync(ct => ct.Name == request.Name && ct.Id != id))
            throw new BusinessRuleException($"A class type with name '{request.Name}' already exists");

        if (!Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, true, out var level))
            throw new BusinessRuleException($"Invalid difficulty level: {request.DifficultyLevel}");

        classType.Name = request.Name;
        classType.Description = request.Description;
        classType.DefaultDurationMinutes = request.DefaultDurationMinutes;
        classType.DefaultCapacity = request.DefaultCapacity;
        classType.IsPremium = request.IsPremium;
        classType.CaloriesPerSession = request.CaloriesPerSession;
        classType.DifficultyLevel = level;
        classType.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated class type: {ClassTypeId}", id);
        return MapToResponse(classType);
    }

    private static ClassTypeResponse MapToResponse(ClassType ct) => new(
        ct.Id, ct.Name, ct.Description, ct.DefaultDurationMinutes, ct.DefaultCapacity,
        ct.IsPremium, ct.CaloriesPerSession, ct.DifficultyLevel.ToString(), ct.IsActive,
        ct.CreatedAt, ct.UpdatedAt);
}
