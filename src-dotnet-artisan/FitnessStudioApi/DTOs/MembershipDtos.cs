using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public sealed record MembershipResponse(
    int Id,
    int MemberId,
    string MemberName,
    int MembershipPlanId,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    MembershipStatus Status,
    PaymentStatus PaymentStatus,
    DateOnly? FreezeStartDate,
    DateOnly? FreezeEndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateMembershipRequest(
    int MemberId,
    int MembershipPlanId,
    DateOnly StartDate,
    PaymentStatus PaymentStatus = PaymentStatus.Paid);

public sealed record FreezeMembershipRequest(int DurationDays);
