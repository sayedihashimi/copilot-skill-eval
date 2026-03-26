using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public sealed record PatronResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    DateOnly MembershipDate,
    MembershipType MembershipType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int? ActiveLoansCount = null,
    decimal? TotalUnpaidFines = null);

public sealed record CreatePatronRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public string? Address { get; init; }

    public MembershipType MembershipType { get; init; } = MembershipType.Standard;
}

public sealed record UpdatePatronRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public string? Address { get; init; }

    public MembershipType MembershipType { get; init; } = MembershipType.Standard;
}
