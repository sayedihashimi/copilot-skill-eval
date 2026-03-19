using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public sealed record BookingResponse(
    int Id,
    int ClassScheduleId,
    string ClassName,
    int MemberId,
    string MemberName,
    DateTime BookingDate,
    BookingStatus Status,
    int? WaitlistPosition,
    DateTime? CheckInTime,
    DateTime? CancellationDate,
    string? CancellationReason,
    DateTime ClassStartTime,
    DateTime ClassEndTime,
    string Room,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed class CreateBookingRequest
{
    [Required]
    public int ClassScheduleId { get; init; }

    [Required]
    public int MemberId { get; init; }
}

public sealed class CancelBookingRequest
{
    public string? Reason { get; init; }
}
