using Microsoft.EntityFrameworkCore;
using FitnessStudioApi.Data;
using FitnessStudioApi.DTOs;
using FitnessStudioApi.Models;
using FitnessStudioApi.Services.Interfaces;

namespace FitnessStudioApi.Services;

public class ClassTypeService : IClassTypeService
{
    private readonly FitnessDbContext _context;

    public ClassTypeService(FitnessDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClassTypeDto>> GetAllAsync(DifficultyLevel? difficulty, bool? isPremium)
    {
        var query = _context.ClassTypes.Where(ct => ct.IsActive);

        if (difficulty.HasValue)
            query = query.Where(ct => ct.DifficultyLevel == difficulty.Value);
        if (isPremium.HasValue)
            query = query.Where(ct => ct.IsPremium == isPremium.Value);

        return await query.OrderBy(ct => ct.Name).Select(ct => MapToDto(ct)).ToListAsync();
    }

    public async Task<ClassTypeDto?> GetByIdAsync(int id)
    {
        var ct = await _context.ClassTypes.FindAsync(id);
        return ct == null ? null : MapToDto(ct);
    }

    public async Task<ClassTypeDto> CreateAsync(CreateClassTypeDto dto)
    {
        if (await _context.ClassTypes.AnyAsync(ct => ct.Name == dto.Name))
            throw new InvalidOperationException($"A class type with name '{dto.Name}' already exists.");

        var ct = new ClassType
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultDurationMinutes = dto.DefaultDurationMinutes,
            DefaultCapacity = dto.DefaultCapacity,
            IsPremium = dto.IsPremium,
            CaloriesPerSession = dto.CaloriesPerSession,
            DifficultyLevel = dto.DifficultyLevel
        };

        _context.ClassTypes.Add(ct);
        await _context.SaveChangesAsync();
        return MapToDto(ct);
    }

    public async Task<ClassTypeDto?> UpdateAsync(int id, UpdateClassTypeDto dto)
    {
        var ct = await _context.ClassTypes.FindAsync(id);
        if (ct == null) return null;

        if (await _context.ClassTypes.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            throw new InvalidOperationException($"A class type with name '{dto.Name}' already exists.");

        ct.Name = dto.Name;
        ct.Description = dto.Description;
        ct.DefaultDurationMinutes = dto.DefaultDurationMinutes;
        ct.DefaultCapacity = dto.DefaultCapacity;
        ct.IsPremium = dto.IsPremium;
        ct.CaloriesPerSession = dto.CaloriesPerSession;
        ct.DifficultyLevel = dto.DifficultyLevel;
        ct.IsActive = dto.IsActive;
        ct.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(ct);
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
        DifficultyLevel = ct.DifficultyLevel,
        IsActive = ct.IsActive,
        CreatedAt = ct.CreatedAt,
        UpdatedAt = ct.UpdatedAt
    };
}
