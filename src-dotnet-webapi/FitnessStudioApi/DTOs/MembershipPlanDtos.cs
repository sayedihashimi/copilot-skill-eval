using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

// Membership Plan DTOs
public sealed record MembershipPlanResponse(
    int Id,
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreateMembershipPlanRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Required, Range(1, 24)]
    public required int DurationMonths { get; init; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
    public required decimal Price { get; init; }

    [Required]
    public required int MaxClassBookingsPerWeek { get; init; }

    public bool AllowsPremiumClasses { get; init; }
    public bool IsActive { get; init; } = true;
}

public sealed record UpdateMembershipPlanRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Required, Range(1, 24)]
    public required int DurationMonths { get; init; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive")]
    public required decimal Price { get; init; }

    [Required]
    public required int MaxClassBookingsPerWeek { get; init; }

    public bool AllowsPremiumClasses { get; init; }
    public bool IsActive { get; init; } = true;
}
