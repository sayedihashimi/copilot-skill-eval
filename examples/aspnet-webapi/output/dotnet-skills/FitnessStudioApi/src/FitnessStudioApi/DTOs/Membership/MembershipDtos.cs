namespace FitnessStudioApi.DTOs.Membership;

public sealed class MembershipDto
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int MembershipPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateOnly? FreezeStartDate { get; set; }
    public DateOnly? FreezeEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CreateMembershipDto
{
    public int MemberId { get; set; }
    public int MembershipPlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public string PaymentStatus { get; set; } = "Paid";
}

public sealed class FreezeMembershipDto
{
    public int FreezeDurationDays { get; set; }
}

public sealed class CancelMembershipDto
{
    public string? Reason { get; set; }
}
