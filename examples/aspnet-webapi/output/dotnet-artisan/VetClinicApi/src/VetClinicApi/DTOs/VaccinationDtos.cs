using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record CreateVaccinationDto(
    [Required] int PetId,
    [Required, MaxLength(200)] string VaccineName,
    [Required] DateOnly DateAdministered,
    [Required] DateOnly ExpirationDate,
    string? BatchNumber,
    [Required] int AdministeredByVetId,
    [MaxLength(500)] string? Notes);

public sealed record VaccinationDto(
    int Id,
    int PetId,
    string VaccineName,
    DateOnly DateAdministered,
    DateOnly ExpirationDate,
    string? BatchNumber,
    int AdministeredByVetId,
    string? AdministeredByVetName,
    string? Notes,
    bool IsExpired,
    bool IsDueSoon,
    DateTime CreatedAt);
