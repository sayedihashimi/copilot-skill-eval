using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public sealed record MemberResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone,
    DateOnly JoinDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed class CreateMemberRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; init; }

    [Required, MaxLength(200)]
    public string EmergencyContactName { get; init; } = string.Empty;

    [Required]
    public string EmergencyContactPhone { get; init; } = string.Empty;
}

public sealed class UpdateMemberRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    [Required, MaxLength(200)]
    public string EmergencyContactName { get; init; } = string.Empty;

    [Required]
    public string EmergencyContactPhone { get; init; } = string.Empty;
}
