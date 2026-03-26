using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public record MemberDto(
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
    MembershipSummaryDto? ActiveMembership);

public record MemberListDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly JoinDate,
    bool IsActive);

public record CreateMemberDto
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

public record UpdateMemberDto
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

public record MembershipSummaryDto(
    int Id,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);
