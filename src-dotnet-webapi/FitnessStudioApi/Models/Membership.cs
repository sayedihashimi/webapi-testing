namespace FitnessStudioApi.Models;

public sealed class Membership
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int MembershipPlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
    public DateOnly? FreezeStartDate { get; set; }
    public DateOnly? FreezeEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Member Member { get; set; } = null!;
    public MembershipPlan MembershipPlan { get; set; } = null!;
}

public enum MembershipStatus
{
    Active,
    Expired,
    Cancelled,
    Frozen
}

public enum PaymentStatus
{
    Paid,
    Pending,
    Refunded
}
