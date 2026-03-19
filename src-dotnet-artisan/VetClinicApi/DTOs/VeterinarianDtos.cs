using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record VeterinarianResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Specialization,
    string LicenseNumber,
    bool IsAvailable,
    DateOnly HireDate);

public sealed record CreateVeterinarianRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    public string? Specialization { get; init; }

    [Required]
    public string LicenseNumber { get; init; } = string.Empty;

    [Required]
    public DateOnly HireDate { get; init; }
}

public sealed record UpdateVeterinarianRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    public string? Specialization { get; init; }

    [Required]
    public string LicenseNumber { get; init; } = string.Empty;

    public bool IsAvailable { get; init; } = true;
}
