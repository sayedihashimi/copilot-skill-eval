using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public record InstructorDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate,
    bool IsActive);

public record CreateInstructorDto
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

public record UpdateInstructorDto
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
}
