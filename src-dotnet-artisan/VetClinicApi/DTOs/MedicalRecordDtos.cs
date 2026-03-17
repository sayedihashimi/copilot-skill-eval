using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record MedicalRecordDto(
    int Id,
    int AppointmentId,
    int PetId,
    string PetName,
    int VeterinarianId,
    string VeterinarianName,
    string Diagnosis,
    string Treatment,
    string? Notes,
    DateOnly? FollowUpDate,
    DateTime CreatedAt,
    IReadOnlyList<PrescriptionDto> Prescriptions);

public sealed record CreateMedicalRecordRequest
{
    [Required]
    public int AppointmentId { get; init; }

    [Required, MaxLength(1000)]
    public string Diagnosis { get; init; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Treatment { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; init; }

    public DateOnly? FollowUpDate { get; init; }
}

public sealed record UpdateMedicalRecordRequest
{
    [Required, MaxLength(1000)]
    public string Diagnosis { get; init; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Treatment { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; init; }

    public DateOnly? FollowUpDate { get; init; }
}
