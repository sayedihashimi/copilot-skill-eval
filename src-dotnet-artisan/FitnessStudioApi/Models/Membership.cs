using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.Models;

public sealed class Membership
{
    public int Id { get; set; }

    [Required]
    public int MemberId { get; set; }

    [Required]
    public int MembershipPlanId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public MembershipStatus Status { get; set; } = MembershipStatus.Active;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;

    public DateOnly? FreezeStartDate { get; set; }

    public DateOnly? FreezeEndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Member Member { get; set; } = null!;

    public MembershipPlan MembershipPlan { get; set; } = null!;
}
