using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public class CreateAppointmentDto
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

public class UpdateAppointmentDto
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

public class UpdateAppointmentStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    public string? CancellationReason { get; set; }
}

public class AppointmentDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AppointmentDetailDto : AppointmentDto
{
    public PetSummaryDto Pet { get; set; } = null!;
    public VeterinarianDto Veterinarian { get; set; } = null!;
    public MedicalRecordDto? MedicalRecord { get; set; }
}
