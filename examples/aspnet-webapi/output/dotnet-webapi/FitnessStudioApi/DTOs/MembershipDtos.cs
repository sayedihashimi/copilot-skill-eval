using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public sealed record CreateMembershipRequest
{
    [Required]
    public required int MemberId { get; init; }

    [Required]
    public required int MembershipPlanId { get; init; }

    [Required]
    public required DateOnly StartDate { get; init; }
}

public sealed record FreezeMembershipRequest
{
    [Range(7, 30)]
    public required int FreezeDurationDays { get; init; }
}

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
