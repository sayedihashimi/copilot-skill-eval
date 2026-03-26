using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public sealed record InstructorResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateInstructorRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; init; }

    public string? Specializations { get; init; }

    [Required]
    public DateOnly HireDate { get; init; }
}

public sealed record UpdateInstructorRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; init; }

    public string? Specializations { get; init; }

    public bool IsActive { get; init; }
}
