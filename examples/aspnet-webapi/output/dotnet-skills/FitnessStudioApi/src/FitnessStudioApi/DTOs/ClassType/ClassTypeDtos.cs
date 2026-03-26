namespace FitnessStudioApi.DTOs.ClassType;

public sealed class ClassTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public int DefaultCapacity { get; set; }
    public bool IsPremium { get; set; }
    public int? CaloriesPerSession { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateClassTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public int DefaultCapacity { get; set; }
    public bool IsPremium { get; set; }
    public int? CaloriesPerSession { get; set; }
    public string DifficultyLevel { get; set; } = "AllLevels";
}

public sealed class UpdateClassTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public int DefaultCapacity { get; set; }
    public bool IsPremium { get; set; }
    public int? CaloriesPerSession { get; set; }
    public string DifficultyLevel { get; set; } = "AllLevels";
}
