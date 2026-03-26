using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.Models;

public class MedicalRecord
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public int PetId { get; set; }

    public int VeterinarianId { get; set; }

    [Required, MaxLength(1000)]
    public string Diagnosis { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Treatment { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateOnly? FollowUpDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Appointment Appointment { get; set; } = null!;
    public Pet Pet { get; set; } = null!;
    public Veterinarian Veterinarian { get; set; } = null!;
    public ICollection<Prescription> Prescriptions { get; set; } = [];
}
