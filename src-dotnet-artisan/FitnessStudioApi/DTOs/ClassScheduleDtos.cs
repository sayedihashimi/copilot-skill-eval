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
    int AvailableSpots,
    string Room,
    ClassScheduleStatus Status,
    string? CancellationReason,
    bool IsPremium,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateClassScheduleRequest(
    int ClassTypeId,
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    string Room);

public sealed record UpdateClassScheduleRequest(
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    string Room);

public sealed record CancelClassRequest(string? Reason);

public sealed record RosterEntryResponse(
    int BookingId,
    int MemberId,
    string MemberName,
    string Email,
    BookingStatus Status,
    DateTime BookingDate,
    DateTime? CheckInTime);
