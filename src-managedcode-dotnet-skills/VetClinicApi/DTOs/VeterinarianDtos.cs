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

    [Required, Phone]
    public string Phone { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? Specialization { get; init; }

    [Required, MaxLength(50)]
    public string LicenseNumber { get; init; } = string.Empty;

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

    [Required, Phone]
    public string Phone { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? Specialization { get; init; }

    [Required, MaxLength(50)]
    public string LicenseNumber { get; init; } = string.Empty;

    public bool IsAvailable { get; init; } = true;
}
