using System.ComponentModel.DataAnnotations;
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

public sealed record CreateMembershipRequest
{
    [Required]
    public int MemberId { get; init; }

    [Required]
    public int MembershipPlanId { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    public PaymentStatus PaymentStatus { get; init; } = PaymentStatus.Paid;
}

public sealed record FreezeMembershipRequest
{
    [Range(7, 30)]
    public int FreezeDays { get; init; }
}

public sealed record CancelMembershipRequest
{
    public string? Reason { get; init; }
}
