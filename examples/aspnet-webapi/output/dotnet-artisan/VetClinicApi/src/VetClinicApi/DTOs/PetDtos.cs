using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record CreatePetDto(
    [Required, MaxLength(100)] string Name,
    [Required] string Species,
    [MaxLength(100)] string? Breed,
    DateOnly? DateOfBirth,
    [Range(0.01, double.MaxValue, ErrorMessage = "Weight must be positive")] decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    [Required] int OwnerId);

public sealed record UpdatePetDto(
    [Required, MaxLength(100)] string Name,
    [Required] string Species,
    [MaxLength(100)] string? Breed,
    DateOnly? DateOfBirth,
    [Range(0.01, double.MaxValue, ErrorMessage = "Weight must be positive")] decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    [Required] int OwnerId);

public sealed record PetDto(
    int Id,
    string Name,
    string Species,
    string? Breed,
    DateOnly? DateOfBirth,
    decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    bool IsActive,
    int OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record PetDetailDto(
    int Id,
    string Name,
    string Species,
    string? Breed,
    DateOnly? DateOfBirth,
    decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    bool IsActive,
    int OwnerId,
    OwnerDto Owner,
    DateTime CreatedAt,
    DateTime UpdatedAt);
