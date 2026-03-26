using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record CreateMedicalRecordDto(
    [Required] int AppointmentId,
    [Required] int PetId,
    [Required] int VeterinarianId,
    [Required, MaxLength(1000)] string Diagnosis,
    [Required, MaxLength(2000)] string Treatment,
    [MaxLength(2000)] string? Notes,
    DateOnly? FollowUpDate);

public sealed record UpdateMedicalRecordDto(
    [Required, MaxLength(1000)] string Diagnosis,
    [Required, MaxLength(2000)] string Treatment,
    [MaxLength(2000)] string? Notes,
    DateOnly? FollowUpDate);

public sealed record MedicalRecordDto(
    int Id,
    int AppointmentId,
    int PetId,
    int VeterinarianId,
    string Diagnosis,
    string Treatment,
    string? Notes,
    DateOnly? FollowUpDate,
    DateTime CreatedAt,
    IReadOnlyList<PrescriptionDto>? Prescriptions);
