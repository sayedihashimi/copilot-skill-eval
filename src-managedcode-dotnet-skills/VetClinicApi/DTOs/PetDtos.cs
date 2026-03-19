using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record PetResponse(
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
    string OwnerName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreatePetRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string Species { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? Breed { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [Range(0.01, 9999.99)]
    public decimal? Weight { get; init; }

    [MaxLength(50)]
    public string? Color { get; init; }

    [MaxLength(50)]
    public string? MicrochipNumber { get; init; }

    [Required]
    public int OwnerId { get; init; }
}

public sealed record UpdatePetRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string Species { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? Breed { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [Range(0.01, 9999.99)]
    public decimal? Weight { get; init; }

    [MaxLength(50)]
    public string? Color { get; init; }

    [MaxLength(50)]
    public string? MicrochipNumber { get; init; }

    [Required]
    public int OwnerId { get; init; }
}
