namespace FitnessStudioApi.DTOs.ClassSchedule;

public sealed class ClassScheduleDto
{
    public int Id { get; set; }
    public int ClassTypeId { get; set; }
    public string ClassTypeName { get; set; } = string.Empty;
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public int WaitlistCount { get; set; }
    public int AvailableSpots => Math.Max(0, Capacity - CurrentEnrollment);
    public string Room { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateClassScheduleDto
{
    public int ClassTypeId { get; set; }
    public int InstructorId { get; set; }
    public DateTime StartTime { get; set; }
    public int? DurationMinutes { get; set; }
    public int? Capacity { get; set; }
    public string Room { get; set; } = string.Empty;
}

public sealed class UpdateClassScheduleDto
{
    public int? InstructorId { get; set; }
    public DateTime? StartTime { get; set; }
    public int? DurationMinutes { get; set; }
    public int? Capacity { get; set; }
    public string? Room { get; set; }
}

public sealed class CancelClassDto
{
    public string? Reason { get; set; }
}

public sealed class RosterEntryDto
{
    public int BookingId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime? CheckInTime { get; set; }
}
