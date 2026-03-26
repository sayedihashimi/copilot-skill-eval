using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public record MembershipDto(
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
    DateOnly? FreezeEndDate);

public record CreateMembershipDto
{
    [Required]
    public int MemberId { get; init; }

    [Required]
    public int MembershipPlanId { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    public string PaymentStatus { get; init; } = "Paid";
}

public record FreezeMembershipDto
{
    [Range(7, 30, ErrorMessage = "Freeze duration must be between 7 and 30 days")]
    public int FreezeDurationDays { get; init; }
}
