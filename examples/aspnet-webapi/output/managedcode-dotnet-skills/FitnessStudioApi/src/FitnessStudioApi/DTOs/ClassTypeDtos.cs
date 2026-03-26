using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public record ClassTypeDto(
    int Id,
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel,
    bool IsActive);

public record CreateClassTypeDto
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(30, 120)]
    public int DefaultDurationMinutes { get; init; }

    [Range(1, 50)]
    public int DefaultCapacity { get; init; }

    public bool IsPremium { get; init; }

    public int? CaloriesPerSession { get; init; }

    [Required]
    public DifficultyLevel DifficultyLevel { get; init; }
}

public record UpdateClassTypeDto
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(30, 120)]
    public int DefaultDurationMinutes { get; init; }

    [Range(1, 50)]
    public int DefaultCapacity { get; init; }

    public bool IsPremium { get; init; }

    public int? CaloriesPerSession { get; init; }

    [Required]
    public DifficultyLevel DifficultyLevel { get; init; }
}
