using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

// --- Appointment DTOs ---

public record AppointmentDto(
    int Id,
    int PetId,
    int VeterinarianId,
    DateTime AppointmentDate,
    int DurationMinutes,
    string Status,
    string Reason,
    string? Notes,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    PetSummaryDto? Pet = null,
    VeterinarianDto? Veterinarian = null,
    MedicalRecordDto? MedicalRecord = null);

public record CreateAppointmentDto
{
    [Required]
    public int PetId { get; init; }

    [Required]
    public int VeterinarianId { get; init; }

    [Required]
    public DateTime AppointmentDate { get; init; }

    [Range(15, 120)]
    public int DurationMinutes { get; init; } = 30;

    [Required, MaxLength(500)]
    public string Reason { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public record UpdateAppointmentDto
{
    [Required]
    public int PetId { get; init; }

    [Required]
    public int VeterinarianId { get; init; }

    [Required]
    public DateTime AppointmentDate { get; init; }

    [Range(15, 120)]
    public int DurationMinutes { get; init; } = 30;

    [Required, MaxLength(500)]
    public string Reason { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public record UpdateAppointmentStatusDto
{
    [Required]
    public AppointmentStatus Status { get; init; }

    public string? CancellationReason { get; init; }
}
