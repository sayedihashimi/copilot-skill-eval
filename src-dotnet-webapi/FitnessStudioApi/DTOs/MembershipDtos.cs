using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public sealed record MembershipResponse(
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
    DateTime UpdatedAt
);

public sealed record CreateMembershipRequest
{
    [Required]
    public required int MemberId { get; init; }

    [Required]
    public required int MembershipPlanId { get; init; }

    [Required]
    public required DateOnly StartDate { get; init; }

    public string PaymentStatus { get; init; } = "Paid";
}

public sealed record FreezeMembershipRequest
{
    [Required, Range(7, 30, ErrorMessage = "Freeze duration must be between 7 and 30 days")]
    public required int FreezeDurationDays { get; init; }
}
