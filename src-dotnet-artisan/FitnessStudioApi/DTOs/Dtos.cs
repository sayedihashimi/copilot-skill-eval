namespace FitnessStudioApi.DTOs;

// --- Pagination ---
public class PaginationParams
{
    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 10,
            > 50 => 50,
            _ => value
        };
    }
}

public class PaginatedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

// --- MembershipPlan DTOs ---
public record CreateMembershipPlanRequest(
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses);

public record UpdateMembershipPlanRequest(
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses);

public record MembershipPlanResponse(
    int Id,
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// --- Member DTOs ---
public record CreateMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone);

public record UpdateMemberRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone);

public record MemberResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone,
    DateOnly JoinDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record MemberDetailResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone,
    DateOnly JoinDate,
    bool IsActive,
    MembershipSummary? ActiveMembership,
    int TotalBookings,
    int UpcomingBookings,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record MembershipSummary(
    int Id,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);

// --- Membership DTOs ---
public record CreateMembershipRequest(
    int MemberId,
    int MembershipPlanId,
    DateOnly StartDate,
    string PaymentStatus);

public record MembershipResponse(
    int Id,
    int MemberId,
    string MemberName,
    int MembershipPlanId,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string PaymentStatus,
    DateOnly? FreezeStartDate,
    DateOnly? FreezeEndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record FreezeMembershipRequest(int FreezeDays);

// --- Instructor DTOs ---
public record CreateInstructorRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate);

public record UpdateInstructorRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate);

public record InstructorResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// --- ClassType DTOs ---
public record CreateClassTypeRequest(
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    string DifficultyLevel);

public record UpdateClassTypeRequest(
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    string DifficultyLevel);

public record ClassTypeResponse(
    int Id,
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    string DifficultyLevel,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// --- ClassSchedule DTOs ---
public record CreateClassScheduleRequest(
    int ClassTypeId,
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    string Room);

public record UpdateClassScheduleRequest(
    int ClassTypeId,
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    string Room);

public record ClassScheduleResponse(
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
    string Status,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CancelClassRequest(string? Reason);

// --- Booking DTOs ---
public record CreateBookingRequest(
    int ClassScheduleId,
    int MemberId);

public record BookingResponse(
    int Id,
    int ClassScheduleId,
    string ClassName,
    DateTime ClassStartTime,
    int MemberId,
    string MemberName,
    DateTime BookingDate,
    string Status,
    int? WaitlistPosition,
    DateTime? CheckInTime,
    DateTime? CancellationDate,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CancelBookingRequest(string? Reason);

// --- Roster/Waitlist ---
public record RosterEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    DateTime BookingDate,
    string Status);
