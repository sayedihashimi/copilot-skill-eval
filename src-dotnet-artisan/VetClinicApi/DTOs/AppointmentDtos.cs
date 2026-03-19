using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

public sealed record AppointmentResponse(
    int Id,
    int PetId,
    string PetName,
    int VeterinarianId,
    string VeterinarianName,
    DateTime AppointmentDate,
    int DurationMinutes,
    string Status,
    string Reason,
    string? Notes,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record AppointmentDetailResponse(
    int Id,
    int PetId,
    PetResponse Pet,
    int VeterinarianId,
    VeterinarianResponse Veterinarian,
    DateTime AppointmentDate,
    int DurationMinutes,
    string Status,
    string Reason,
    string? Notes,
    string? CancellationReason,
    MedicalRecordResponse? MedicalRecord,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateAppointmentRequest
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

public sealed record UpdateAppointmentRequest
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

public sealed record UpdateAppointmentStatusRequest
{
    [Required]
    public AppointmentStatus Status { get; init; }

    public string? CancellationReason { get; init; }
}
