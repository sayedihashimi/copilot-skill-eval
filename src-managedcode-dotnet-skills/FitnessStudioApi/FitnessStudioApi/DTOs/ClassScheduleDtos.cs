using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public sealed record ClassScheduleResponse(
    int Id,
    int ClassTypeId,
    string ClassName,
    int InstructorId,
    string InstructorName,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    int CurrentEnrollment,
    int WaitlistCount,
    string Room,
    ClassScheduleStatus Status,
    string? CancellationReason,
    bool IsPremium,
    int AvailableSpots,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed class CreateClassScheduleRequest
{
    [Required]
    public int ClassTypeId { get; init; }

    [Required]
    public int InstructorId { get; init; }

    [Required]
    public DateTime StartTime { get; init; }

    [Required]
    public DateTime EndTime { get; init; }

    [Range(1, 50)]
    public int Capacity { get; init; }

    [Required, MaxLength(50)]
    public string Room { get; init; } = string.Empty;
}

public sealed class UpdateClassScheduleRequest
{
    [Required]
    public int InstructorId { get; init; }

    [Required]
    public DateTime StartTime { get; init; }

    [Required]
    public DateTime EndTime { get; init; }

    [Range(1, 50)]
    public int Capacity { get; init; }

    [Required, MaxLength(50)]
    public string Room { get; init; } = string.Empty;
}

public sealed class CancelClassRequest
{
    public string? Reason { get; init; }
}

public sealed record ClassRosterEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    BookingStatus Status,
    DateTime BookingDate,
    DateTime? CheckInTime);

public sealed record WaitlistEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    int? WaitlistPosition,
    DateTime BookingDate);
