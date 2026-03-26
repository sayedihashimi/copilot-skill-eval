using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public record ClassScheduleDto(
    int Id,
    int ClassTypeId,
    string ClassTypeName,
    int InstructorId,
    string InstructorName,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    int CurrentEnrollment,
    int WaitlistCount,
    int AvailableSpots,
    string Room,
    ClassStatus Status,
    string? CancellationReason);

public record ClassScheduleListDto(
    int Id,
    string ClassTypeName,
    string InstructorName,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    int CurrentEnrollment,
    int AvailableSpots,
    string Room,
    ClassStatus Status);

public record CreateClassScheduleDto
{
    [Required]
    public int ClassTypeId { get; init; }

    [Required]
    public int InstructorId { get; init; }

    [Required]
    public DateTime StartTime { get; init; }

    public int? DurationMinutes { get; init; }

    [Range(1, 100)]
    public int? Capacity { get; init; }

    [Required, MaxLength(50)]
    public string Room { get; init; } = string.Empty;
}

public record UpdateClassScheduleDto
{
    [Required]
    public int InstructorId { get; init; }

    [Required]
    public DateTime StartTime { get; init; }

    public int? DurationMinutes { get; init; }

    [Range(1, 100)]
    public int? Capacity { get; init; }

    [Required, MaxLength(50)]
    public string Room { get; init; } = string.Empty;
}

public record CancelClassDto
{
    public string? CancellationReason { get; init; }
}

public record RosterEntryDto(
    int BookingId,
    int MemberId,
    string MemberName,
    string Status,
    DateTime BookingDate,
    DateTime? CheckInTime);

public record WaitlistEntryDto(
    int BookingId,
    int MemberId,
    string MemberName,
    int? WaitlistPosition,
    DateTime BookingDate);
