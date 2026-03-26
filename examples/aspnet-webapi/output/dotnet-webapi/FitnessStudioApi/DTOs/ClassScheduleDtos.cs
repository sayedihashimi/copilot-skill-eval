using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public sealed record CreateClassScheduleRequest
{
    [Required]
    public required int ClassTypeId { get; init; }

    [Required]
    public required int InstructorId { get; init; }

    [Required]
    public required DateTime StartTime { get; init; }

    [Range(1, 50)]
    public int? Capacity { get; init; }

    [Range(30, 120)]
    public int? DurationMinutes { get; init; }

    [Required, MaxLength(50)]
    public required string Room { get; init; }
}

public sealed record UpdateClassScheduleRequest
{
    [Required]
    public required int InstructorId { get; init; }

    [Required]
    public required DateTime StartTime { get; init; }

    [Range(1, 50)]
    public int? Capacity { get; init; }

    [Range(30, 120)]
    public int? DurationMinutes { get; init; }

    [Required, MaxLength(50)]
    public required string Room { get; init; }
}

public sealed record CancelClassRequest
{
    [MaxLength(500)]
    public string? CancellationReason { get; init; }
}

public sealed record ClassScheduleResponse(
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
    ClassScheduleStatus Status,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ClassRosterEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    BookingStatus Status,
    DateTime BookingDate,
    DateTime? CheckInTime);

public sealed record ClassWaitlistEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    int WaitlistPosition,
    DateTime BookingDate);
