using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

public class AppointmentCreateDto
{
    [Required]
    public int PetId { get; set; }

    [Required]
    public int VeterinarianId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Range(15, 120)]
    public int DurationMinutes { get; set; } = 30;

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class AppointmentUpdateDto
{
    [Required]
    public int PetId { get; set; }

    [Required]
    public int VeterinarianId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Range(15, 120)]
    public int DurationMinutes { get; set; } = 30;

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class AppointmentStatusUpdateDto
{
    [Required]
    public AppointmentStatus Status { get; set; }

    public string? CancellationReason { get; set; }
}

public class AppointmentResponseDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public PetSummaryDto? Pet { get; set; }
    public int VeterinarianId { get; set; }
    public VeterinarianResponseDto? Veterinarian { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public MedicalRecordResponseDto? MedicalRecord { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
