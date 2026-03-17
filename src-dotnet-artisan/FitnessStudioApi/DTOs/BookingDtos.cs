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

public sealed record CreateBookingRequest(
    int ClassScheduleId,
    int MemberId);

public sealed record CancelBookingRequest(string? Reason);
