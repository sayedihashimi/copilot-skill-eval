using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public record MembershipPlanDto(
    int Id,
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses,
    bool IsActive);

public record CreateMembershipPlanDto
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(1, 24)]
    public int DurationMonths { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
    public decimal Price { get; init; }

    public int MaxClassBookingsPerWeek { get; init; }

    public bool AllowsPremiumClasses { get; init; }
}

public record UpdateMembershipPlanDto
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(1, 24)]
    public int DurationMonths { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
    public decimal Price { get; init; }

    public int MaxClassBookingsPerWeek { get; init; }

    public bool AllowsPremiumClasses { get; init; }
}
