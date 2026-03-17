using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassTypeService(FitnessDbContext db, ILogger<ClassTypeService> logger) : IClassTypeService
{
    private readonly FitnessDbContext _db = db;
    private readonly ILogger<ClassTypeService> _logger = logger;

    public async Task<IReadOnlyList<ClassTypeResponse>> GetAllAsync(DifficultyLevel? difficulty, bool? isPremium, CancellationToken ct)
    {
        var query = _db.ClassTypes.AsNoTracking().Where(c => c.IsActive);

        if (difficulty.HasValue)
            query = query.Where(c => c.DifficultyLevel == difficulty.Value);
        if (isPremium.HasValue)
            query = query.Where(c => c.IsPremium == isPremium.Value);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => MapToResponse(c))
            .ToListAsync(ct);
    }

    public async Task<ClassTypeResponse> GetByIdAsync(int id, CancellationToken ct)
    {
        var classType = await _db.ClassTypes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");
        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct)
    {
        var exists = await _db.ClassTypes.AnyAsync(c => c.Name == request.Name, ct);
        if (exists)
            throw new ConflictException($"A class type named '{request.Name}' already exists.");

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

        _db.ClassTypes.Add(classType);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created class type {Name} with ID {Id}", classType.Name, classType.Id);
        return MapToResponse(classType);
    }

    public async Task<ClassTypeResponse> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await _db.ClassTypes.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");

        var nameConflict = await _db.ClassTypes.AnyAsync(c => c.Name == request.Name && c.Id != id, ct);
        if (nameConflict)
            throw new ConflictException($"A class type named '{request.Name}' already exists.");

        classType.Name = request.Name;
        classType.Description = request.Description;
        classType.DefaultDurationMinutes = request.DefaultDurationMinutes;
        classType.DefaultCapacity = request.DefaultCapacity;
        classType.IsPremium = request.IsPremium;
        classType.CaloriesPerSession = request.CaloriesPerSession;
        classType.DifficultyLevel = request.DifficultyLevel;
        classType.IsActive = request.IsActive;
        classType.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return MapToResponse(classType);
    }

    private static ClassTypeResponse MapToResponse(ClassType c) => new(
        c.Id, c.Name, c.Description, c.DefaultDurationMinutes, c.DefaultCapacity,
        c.IsPremium, c.CaloriesPerSession, c.DifficultyLevel, c.IsActive,
        c.CreatedAt, c.UpdatedAt);
}
