using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record CreateOwnerDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    string? Address,
    string? City,
    [MaxLength(2)] string? State,
    string? ZipCode);

public sealed record UpdateOwnerDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    string? Address,
    string? City,
    [MaxLength(2)] string? State,
    string? ZipCode);

public sealed record OwnerDto(
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

public sealed record OwnerDetailDto(
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
    IReadOnlyList<PetSummaryDto> Pets);

public sealed record PetSummaryDto(
    int Id,
    string Name,
    string Species,
    string? Breed,
    bool IsActive);
