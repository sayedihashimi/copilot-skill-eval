using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

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

public sealed record ClassScheduleListResponse(
    int Id,
    string ClassTypeName,
    string InstructorName,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    int CurrentEnrollment,
    int AvailableSpots,
    string Room,
    ClassScheduleStatus Status);

public sealed record CreateClassScheduleRequest
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
    public int? Capacity { get; init; }

    [Required, MaxLength(50)]
    public string Room { get; init; } = string.Empty;
}

public sealed record UpdateClassScheduleRequest
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
    public int? Capacity { get; init; }

    [Required, MaxLength(50)]
    public string Room { get; init; } = string.Empty;
}

public sealed record CancelClassRequest
{
    public string? Reason { get; init; }
}

public sealed record RosterEntryResponse(
    int BookingId,
    int MemberId,
    string MemberName,
    string Email,
    BookingStatus Status,
    DateTime BookingDate,
    DateTime? CheckInTime);

public sealed record WaitlistEntryResponse(
    int BookingId,
    int MemberId,
    string MemberName,
    int? WaitlistPosition,
    DateTime BookingDate);
