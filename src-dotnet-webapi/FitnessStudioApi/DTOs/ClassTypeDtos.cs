using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public sealed record ClassTypeResponse(
    int Id,
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    string DifficultyLevel,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreateClassTypeRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Required, Range(30, 120)]
    public required int DefaultDurationMinutes { get; init; }

    [Required, Range(1, 50)]
    public required int DefaultCapacity { get; init; }

    public bool IsPremium { get; init; }

    [Range(0, 2000)]
    public int? CaloriesPerSession { get; init; }

    [Required]
    public required string DifficultyLevel { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed record UpdateClassTypeRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Required, Range(30, 120)]
    public required int DefaultDurationMinutes { get; init; }

    [Required, Range(1, 50)]
    public required int DefaultCapacity { get; init; }

    public bool IsPremium { get; init; }

    [Range(0, 2000)]
    public int? CaloriesPerSession { get; init; }

    [Required]
    public required string DifficultyLevel { get; init; }

    public bool IsActive { get; init; } = true;
}
