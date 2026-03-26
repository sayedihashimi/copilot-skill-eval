using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.Models;

public class Vaccination
{
    public int Id { get; set; }

    public int PetId { get; set; }

    [Required, MaxLength(200)]
    public string VaccineName { get; set; } = string.Empty;

    [Required]
    public DateOnly DateAdministered { get; set; }

    [Required]
    public DateOnly ExpirationDate { get; set; }

    public string? BatchNumber { get; set; }

    public int AdministeredByVetId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Computed
    public bool IsExpired => ExpirationDate < DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsDueSoon => !IsExpired && ExpirationDate <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    // Navigation
    public Pet Pet { get; set; } = null!;
    public Veterinarian AdministeredByVet { get; set; } = null!;
}
