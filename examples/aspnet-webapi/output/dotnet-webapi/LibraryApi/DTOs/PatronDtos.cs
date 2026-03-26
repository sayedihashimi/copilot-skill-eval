using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

// --- Patron DTOs ---

public sealed record CreatePatronRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    public string? Phone { get; init; }
    public string? Address { get; init; }

    public MembershipType MembershipType { get; init; } = MembershipType.Standard;
}

public sealed record UpdatePatronRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    public string? Phone { get; init; }
    public string? Address { get; init; }

    public MembershipType MembershipType { get; init; } = MembershipType.Standard;
}

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
    DateTime UpdatedAt);

public sealed record PatronDetailResponse(
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
    int ActiveLoansCount,
    decimal TotalUnpaidFines);
