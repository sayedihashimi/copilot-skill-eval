using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs.ClassType;
using FitnessStudioApi.Middleware;
using FitnessStudioApi.Models;
using FitnessStudioApi.Models.Enums;
using FitnessStudioApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FitnessStudioApi.Services;

public sealed class ClassTypeService : IClassTypeService
{
    private readonly FitnessDbContext _context;
    private readonly ILogger<ClassTypeService> _logger;

    public ClassTypeService(FitnessDbContext context, ILogger<ClassTypeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ClassTypeDto>> GetAllAsync(string? difficulty, bool? isPremium)
    {
        var query = _context.ClassTypes.AsNoTracking().Where(ct => ct.IsActive);

        if (!string.IsNullOrWhiteSpace(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var level))
            query = query.Where(ct => ct.DifficultyLevel == level);

        if (isPremium.HasValue)
            query = query.Where(ct => ct.IsPremium == isPremium.Value);

        return await query
            .OrderBy(ct => ct.Name)
            .Select(ct => MapToDto(ct))
            .ToListAsync();
    }

    public async Task<ClassTypeDto> GetByIdAsync(int id)
    {
        var classType = await _context.ClassTypes.AsNoTracking()
            .FirstOrDefaultAsync(ct => ct.Id == id)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");

        return MapToDto(classType);
    }

    public async Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto)
    {
        if (await _context.ClassTypes.AnyAsync(ct => ct.Name == dto.Name))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409, "Duplicate Resource");

        var classType = new ClassType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDurationMinutes = dto.DefaultDurationMinutes,
            DefaultCapacity = dto.DefaultCapacity,
            IsPremium = dto.IsPremium,
            CaloriesPerSession = dto.CaloriesPerSession,
            DifficultyLevel = Enum.Parse<DifficultyLevel>(dto.DifficultyLevel, true)
        };

        _context.ClassTypes.Add(classType);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created class type: {ClassTypeName} (ID: {ClassTypeId})", classType.Name, classType.Id);
        return MapToDto(classType);
    }

    public async Task<ClassTypeDto> UpdateAsync(int id, UpdateClassTypeDto dto)
    {
        var classType = await _context.ClassTypes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Class type with ID {id} not found.");

        if (await _context.ClassTypes.AnyAsync(ct => ct.Name == dto.Name && ct.Id != id))
            throw new BusinessRuleException($"A class type with name '{dto.Name}' already exists.", 409, "Duplicate Resource");

        classType.Name = dto.Name;
        classType.Description = dto.Description;
        classType.DefaultDurationMinutes = dto.DefaultDurationMinutes;
        classType.DefaultCapacity = dto.DefaultCapacity;
        classType.IsPremium = dto.IsPremium;
        classType.CaloriesPerSession = dto.CaloriesPerSession;
        classType.DifficultyLevel = Enum.Parse<DifficultyLevel>(dto.DifficultyLevel, true);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated class type: {ClassTypeName} (ID: {ClassTypeId})", classType.Name, classType.Id);
        return MapToDto(classType);
    }

    private static ClassTypeDto MapToDto(ClassType ct) => new()
    {
        Id = ct.Id,
        Name = ct.Name,
        Description = ct.Description,
        DefaultDurationMinutes = ct.DefaultDurationMinutes,
        DefaultCapacity = ct.DefaultCapacity,
        IsPremium = ct.IsPremium,
        CaloriesPerSession = ct.CaloriesPerSession,
        DifficultyLevel = ct.DifficultyLevel.ToString(),
        IsActive = ct.IsActive,
        CreatedAt = ct.CreatedAt,
        UpdatedAt = ct.UpdatedAt
    };
}
