using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

public sealed record CreateAppointmentDto(
    [Required] int PetId,
    [Required] int VeterinarianId,
    [Required] DateTime AppointmentDate,
    [Range(15, 120)] int DurationMinutes = 30,
    [Required, MaxLength(500)] string Reason = "",
    [MaxLength(2000)] string? Notes = null);

public sealed record UpdateAppointmentDto(
    [Required] int PetId,
    [Required] int VeterinarianId,
    [Required] DateTime AppointmentDate,
    [Range(15, 120)] int DurationMinutes = 30,
    [Required, MaxLength(500)] string Reason = "",
    [MaxLength(2000)] string? Notes = null);

public sealed record UpdateAppointmentStatusDto(
    [Required] AppointmentStatus Status,
    string? CancellationReason = null);

public sealed record AppointmentDto(
    int Id,
    int PetId,
    string PetName,
    int VeterinarianId,
    string VeterinarianName,
    DateTime AppointmentDate,
    int DurationMinutes,
    AppointmentStatus Status,
    string Reason,
    string? Notes,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record AppointmentDetailDto(
    int Id,
    int PetId,
    PetDto Pet,
    int VeterinarianId,
    VeterinarianDto Veterinarian,
    DateTime AppointmentDate,
    int DurationMinutes,
    AppointmentStatus Status,
    string Reason,
    string? Notes,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    MedicalRecordDto? MedicalRecord);
