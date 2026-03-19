using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record OwnerResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record OwnerDetailResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<PetSummaryResponse> Pets);

public sealed record CreateOwnerRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    public string? Address { get; init; }
    public string? City { get; init; }

    [MaxLength(2)]
    public string? State { get; init; }
    public string? ZipCode { get; init; }
}

public sealed record UpdateOwnerRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    public string? Address { get; init; }
    public string? City { get; init; }

    [MaxLength(2)]
    public string? State { get; init; }
    public string? ZipCode { get; init; }
}
