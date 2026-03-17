using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public sealed record MembershipPlanResponse(
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

public sealed record CreateMembershipPlanRequest(
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses);

public sealed record UpdateMembershipPlanRequest(
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses,
    bool IsActive);
