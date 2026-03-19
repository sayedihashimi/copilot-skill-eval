using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public record CreatePatronRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [MaxLength(20)] string? Phone,
    [MaxLength(500)] string? Address,
    MembershipType MembershipType = MembershipType.Standard);

public record UpdatePatronRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [MaxLength(20)] string? Phone,
    [MaxLength(500)] string? Address,
    MembershipType MembershipType = MembershipType.Standard);

public record PatronResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    MembershipType MembershipType,
    DateOnly MembershipDate,
    bool IsActive);

public record PatronDetailResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    MembershipType MembershipType,
    DateOnly MembershipDate,
    bool IsActive,
    int ActiveLoansCount,
    decimal TotalUnpaidFines,
    DateTime CreatedAt,
    DateTime UpdatedAt);
