using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public record BookingDto(
    int Id,
    int ClassScheduleId,
    string ClassTypeName,
    DateTime ClassStartTime,
    DateTime ClassEndTime,
    string InstructorName,
    string Room,
    int MemberId,
    string MemberName,
    DateTime BookingDate,
    BookingStatus Status,
    int? WaitlistPosition,
    DateTime? CheckInTime,
    DateTime? CancellationDate,
    string? CancellationReason);

public record CreateBookingDto
{
    [Required]
    public int ClassScheduleId { get; init; }

    [Required]
    public int MemberId { get; init; }
}

public record CancelBookingDto
{
    public string? CancellationReason { get; init; }
}
