using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public class CreateMembershipRequest
{
    public int MemberId { get; set; }
    public int MembershipPlanId { get; set; }
    public DateOnly StartDate { get; set; }

    [Required]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
}

public class FreezeMembershipRequest
{
    public DateOnly FreezeStartDate { get; set; }
    public DateOnly FreezeEndDate { get; set; }
}

public class MembershipResponse
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int MembershipPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public MembershipStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateOnly? FreezeStartDate { get; set; }
    public DateOnly? FreezeEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
