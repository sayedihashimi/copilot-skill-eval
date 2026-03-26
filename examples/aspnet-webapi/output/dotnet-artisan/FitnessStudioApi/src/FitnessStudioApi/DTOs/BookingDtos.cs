using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public sealed record BookingResponse(
    int Id,
    int ClassScheduleId,
    string ClassTypeName,
    DateTime ClassStartTime,
    DateTime ClassEndTime,
    string Room,
    string InstructorName,
    int MemberId,
    string MemberName,
    DateTime BookingDate,
    BookingStatus Status,
    int? WaitlistPosition,
    DateTime? CheckInTime,
    DateTime? CancellationDate,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateBookingRequest
{
    [Required]
    public int ClassScheduleId { get; init; }

    [Required]
    public int MemberId { get; init; }
}

public sealed record CancelBookingRequest
{
    public string? Reason { get; init; }
}
