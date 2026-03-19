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

public sealed class CreateMembershipRequest
{
    [Required]
    public int MemberId { get; init; }

    [Required]
    public int MembershipPlanId { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    public PaymentStatus PaymentStatus { get; init; } = PaymentStatus.Pending;
}

public sealed class FreezeMembershipRequest
{
    [Required]
    public DateOnly FreezeStartDate { get; init; }

    [Required]
    public DateOnly FreezeEndDate { get; init; }
}

public sealed class CancelMembershipRequest
{
    public string? Reason { get; init; }
}
