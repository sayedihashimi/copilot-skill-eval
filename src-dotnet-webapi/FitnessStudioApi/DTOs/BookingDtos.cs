using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public sealed record BookingResponse(
    int Id,
    int ClassScheduleId,
    string ClassName,
    int MemberId,
    string MemberName,
    DateTime BookingDate,
    string Status,
    int? WaitlistPosition,
    DateTime? CheckInTime,
    DateTime? CancellationDate,
    string? CancellationReason,
    DateTime ClassStartTime,
    DateTime ClassEndTime,
    string Room,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreateBookingRequest
{
    [Required]
    public required int ClassScheduleId { get; init; }

    [Required]
    public required int MemberId { get; init; }
}

public sealed record CancelBookingRequest
{
    [MaxLength(500)]
    public string? Reason { get; init; }
}
