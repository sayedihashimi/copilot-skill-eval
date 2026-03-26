using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Middleware;

namespace FitnessStudioApi.Services;

public class ClassTypeService : IClassTypeService
{
    private readonly FitnessDbContext _db;

    public ClassTypeService(FitnessDbContext db) => _db = db;

    public async Task<PaginatedResult<ClassTypeDto>> GetAllAsync(int page, int pageSize, string? difficulty, bool? isPremium)
    {
        var query = _db.ClassTypes.AsQueryable();
        if (isPremium.HasValue) query = query.Where(ct => ct.IsPremium == isPremium.Value);
        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var dl))
            query = query.Where(ct => ct.DifficultyLevel == dl);

        var total = await query.CountAsync();
        var items = await query.OrderBy(ct => ct.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(ct => ToDto(ct)).ToListAsync();

        return new PaginatedResult<ClassTypeDto>(items, total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize));
    }

    public async Task<ClassTypeDto?> GetByIdAsync(int id)
    {
        var ct = await _db.ClassTypes.FindAsync(id);
        return ct == null ? null : ToDto(ct);
    }

    public async Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto)
    {
        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409, "Conflict");

        if (!Enum.TryParse<DifficultyLevel>(dto.DifficultyLevel, true, out var dl))
            throw new BusinessRuleException($"Invalid difficulty level '{dto.DifficultyLevel}'. Valid values: Beginner, Intermediate, Advanced, AllLevels.");

        var classType = new ClassType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDurationMinutes = dto.DefaultDurationMinutes,
            DefaultCapacity = dto.DefaultCapacity,
            IsPremium = dto.IsPremium,
            CaloriesPerSession = dto.CaloriesPerSession,
            DifficultyLevel = dl
        };

        _db.ClassTypes.Add(classType);
        await _db.SaveChangesAsync();
        return ToDto(classType);
    }

    public async Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto)
    {
        var classType = await _db.ClassTypes.FindAsync(id)
            ?? throw new BusinessRuleException("Class type not found.", 404, "Not Found");

        if (await _db.ClassTypes.AnyAsync(ct => ct.Name == dto.Name && ct.Id != id))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409, "Conflict");

        if (!Enum.TryParse<DifficultyLevel>(dto.DifficultyLevel, true, out var dl))
            throw new BusinessRuleException($"Invalid difficulty level '{dto.DifficultyLevel}'.");

        classType.Name = dto.Name;
        classType.Description = dto.Description;
        classType.DefaultDurationMinutes = dto.DefaultDurationMinutes;
        classType.DefaultCapacity = dto.DefaultCapacity;
        classType.IsPremium = dto.IsPremium;
        classType.CaloriesPerSession = dto.CaloriesPerSession;
        classType.DifficultyLevel = dl;
        classType.IsActive = dto.IsActive;
        classType.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(classType);
    }

    private static ClassTypeDto ToDto(ClassType ct) => new(
        ct.Id, ct.Name, ct.Description, ct.DefaultDurationMinutes,
        ct.DefaultCapacity, ct.IsPremium, ct.CaloriesPerSession,
        ct.DifficultyLevel.ToString(), ct.IsActive, ct.CreatedAt, ct.UpdatedAt);
}
