using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public class MembershipDto
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

public class CreateMembershipDto
{
    [Required]
    public int MemberId { get; set; }

    [Required]
    public int MembershipPlanId { get; set; }

    public DateOnly? StartDate { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
}

public class FreezeMembershipDto
{
    [Range(7, 30, ErrorMessage = "Freeze duration must be between 7 and 30 days.")]
    public int FreezeDurationDays { get; set; }
}
