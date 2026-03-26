using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

// --- MembershipPlan DTOs ---
public record MembershipPlanDto(
    int Id, string Name, string? Description, int DurationMonths,
    decimal Price, int MaxClassBookingsPerWeek, bool AllowsPremiumClasses,
    bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateMembershipPlanDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(1, 24)] int DurationMonths,
    [Range(0.01, double.MaxValue)] decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses);

public record UpdateMembershipPlanDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(1, 24)] int DurationMonths,
    [Range(0.01, double.MaxValue)] decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses,
    bool IsActive);

// --- Member DTOs ---
public record MemberDto(
    int Id, string FirstName, string LastName, string Email, string Phone,
    DateOnly DateOfBirth, string EmergencyContactName, string EmergencyContactPhone,
    DateOnly JoinDate, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt,
    MembershipSummaryDto? ActiveMembership);

public record MemberListDto(
    int Id, string FirstName, string LastName, string Email, string Phone,
    DateOnly JoinDate, bool IsActive);

public record CreateMemberDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    DateOnly DateOfBirth,
    [Required, MaxLength(200)] string EmergencyContactName,
    [Required] string EmergencyContactPhone);

public record UpdateMemberDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    [Required, MaxLength(200)] string EmergencyContactName,
    [Required] string EmergencyContactPhone);

// --- Membership DTOs ---
public record MembershipDto(
    int Id, int MemberId, string MemberName, int MembershipPlanId, string PlanName,
    DateOnly StartDate, DateOnly EndDate, string Status, string PaymentStatus,
    DateOnly? FreezeStartDate, DateOnly? FreezeEndDate,
    DateTime CreatedAt, DateTime UpdatedAt);

public record MembershipSummaryDto(
    int Id, string PlanName, DateOnly StartDate, DateOnly EndDate, string Status);

public record CreateMembershipDto(
    int MemberId,
    int MembershipPlanId,
    DateOnly StartDate);

public record FreezeMembershipDto(
    [Range(7, 30)] int FreezeDurationDays);

// --- Instructor DTOs ---
public record InstructorDto(
    int Id, string FirstName, string LastName, string Email, string Phone,
    string? Bio, string? Specializations, DateOnly HireDate, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);

public record CreateInstructorDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    [MaxLength(1000)] string? Bio,
    string? Specializations,
    DateOnly HireDate);

public record UpdateInstructorDto(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    [MaxLength(1000)] string? Bio,
    string? Specializations,
    bool IsActive);

// --- ClassType DTOs ---
public record ClassTypeDto(
    int Id, string Name, string? Description, int DefaultDurationMinutes,
    int DefaultCapacity, bool IsPremium, int? CaloriesPerSession,
    string DifficultyLevel, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateClassTypeDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(30, 120)] int DefaultDurationMinutes,
    [Range(1, 50)] int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    string DifficultyLevel);

public record UpdateClassTypeDto(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description,
    [Range(30, 120)] int DefaultDurationMinutes,
    [Range(1, 50)] int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    string DifficultyLevel,
    bool IsActive);

// --- ClassSchedule DTOs ---
public record ClassScheduleDto(
    int Id, int ClassTypeId, string ClassTypeName, int InstructorId, string InstructorName,
    DateTime StartTime, DateTime EndTime, int Capacity, int CurrentEnrollment,
    int WaitlistCount, int AvailableSpots, string Room, string Status,
    string? CancellationReason, DateTime CreatedAt, DateTime UpdatedAt);

public record ClassScheduleListDto(
    int Id, string ClassTypeName, string InstructorName,
    DateTime StartTime, DateTime EndTime, int Capacity, int CurrentEnrollment,
    int AvailableSpots, string Room, string Status);

public record CreateClassScheduleDto(
    int ClassTypeId,
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    int? Capacity,
    [Required, MaxLength(50)] string Room);

public record UpdateClassScheduleDto(
    int InstructorId,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    [Required, MaxLength(50)] string Room);

public record CancelClassDto(string? CancellationReason);

// --- Booking DTOs ---
public record BookingDto(
    int Id, int ClassScheduleId, string ClassName, int MemberId, string MemberName,
    DateTime BookingDate, string Status, int? WaitlistPosition,
    DateTime? CheckInTime, DateTime? CancellationDate, string? CancellationReason,
    DateTime CreatedAt, DateTime UpdatedAt);

public record CreateBookingDto(int ClassScheduleId, int MemberId);

public record CancelBookingDto(string? CancellationReason);

// --- Roster/Waitlist ---
public record RosterEntryDto(int BookingId, int MemberId, string MemberName, DateTime BookingDate, string Status);

public record WaitlistEntryDto(int BookingId, int MemberId, string MemberName, int WaitlistPosition, DateTime BookingDate);

// --- Pagination ---
public record PaginatedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);
