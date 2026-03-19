using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public class MembershipCreateDto
{
    [Required]
    public int MemberId { get; set; }

    [Required]
    public int MembershipPlanId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public string PaymentStatus { get; set; } = "Paid";
}

public class MembershipResponseDto
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

public class FreezeMembershipDto
{
    [Range(7, 30, ErrorMessage = "Freeze duration must be between 7 and 30 days.")]
    public int FreezeDurationDays { get; set; }
}
