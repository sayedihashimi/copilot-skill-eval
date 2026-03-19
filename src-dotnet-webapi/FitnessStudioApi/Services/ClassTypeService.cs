using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassTypeService : IClassTypeService
{
    private readonly FitnessDbContext _db;

    public ClassTypeService(FitnessDbContext db) => _db = db;

    public async Task<PaginatedResponse<ClassTypeResponse>> GetAllAsync(string? difficulty, bool? isPremium, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.ClassTypes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var dl))
            query = query.Where(ct2 => ct2.DifficultyLevel == dl);

        if (isPremium.HasValue)
            query = query.Where(ct2 => ct2.IsPremium == isPremium.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(ct2 => ct2.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ct2 => ToResponse(ct2))
            .ToListAsync(ct);

        return new PaginatedResponse<ClassTypeResponse>(items, page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<ClassTypeResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var classType = await _db.ClassTypes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        return classType is null ? null : ToResponse(classType);
    }

    public async Task<ClassTypeResponse> CreateAsync(CreateClassTypeRequest request, CancellationToken ct)
    {
        if (await _db.ClassTypes.AnyAsync(c => c.Name == request.Name, ct))
            throw new BusinessRuleException($"A class type with name '{request.Name}' already exists.", 409);

        if (!Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, true, out var dl))
            throw new BusinessRuleException($"Invalid difficulty level '{request.DifficultyLevel}'. Valid values: Beginner, Intermediate, Advanced, AllLevels");

        var classType = new ClassType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultDurationMinutes = request.DefaultDurationMinutes,
            DefaultCapacity = request.DefaultCapacity,
            IsPremium = request.IsPremium,
            CaloriesPerSession = request.CaloriesPerSession,
            DifficultyLevel = dl,
            IsActive = request.IsActive
        };

        _db.ClassTypes.Add(classType);
        await _db.SaveChangesAsync(ct);
        return ToResponse(classType);
    }

    public async Task<ClassTypeResponse?> UpdateAsync(int id, UpdateClassTypeRequest request, CancellationToken ct)
    {
        var classType = await _db.ClassTypes.FindAsync([id], ct);
        if (classType is null) return null;

        if (await _db.ClassTypes.AnyAsync(c => c.Name == request.Name && c.Id != id, ct))
            throw new BusinessRuleException($"A class type with name '{request.Name}' already exists.", 409);

        if (!Enum.TryParse<DifficultyLevel>(request.DifficultyLevel, true, out var dl))
            throw new BusinessRuleException($"Invalid difficulty level '{request.DifficultyLevel}'.");

        classType.Name = request.Name;
        classType.Description = request.Description;
        classType.DefaultDurationMinutes = request.DefaultDurationMinutes;
        classType.DefaultCapacity = request.DefaultCapacity;
        classType.IsPremium = request.IsPremium;
        classType.CaloriesPerSession = request.CaloriesPerSession;
        classType.DifficultyLevel = dl;
        classType.IsActive = request.IsActive;
        classType.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return ToResponse(classType);
    }

    private static ClassTypeResponse ToResponse(ClassType c) => new(
        c.Id, c.Name, c.Description, c.DefaultDurationMinutes, c.DefaultCapacity,
        c.IsPremium, c.CaloriesPerSession, c.DifficultyLevel.ToString(),
        c.IsActive, c.CreatedAt, c.UpdatedAt);
}
