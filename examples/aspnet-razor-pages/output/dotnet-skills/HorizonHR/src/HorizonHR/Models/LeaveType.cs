using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Models;

public class LeaveType
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Leave Type")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 365)]
    [Display(Name = "Default Days Per Year")]
    public int DefaultDaysPerYear { get; set; }

    [Display(Name = "Carry Over Allowed")]
    public bool IsCarryOverAllowed { get; set; } = false;

    [Display(Name = "Max Carry Over Days")]
    public int MaxCarryOverDays { get; set; } = 0;

    [Display(Name = "Requires Approval")]
    public bool RequiresApproval { get; set; } = true;

    [Display(Name = "Is Paid")]
    public bool IsPaid { get; set; } = true;

    // Navigation properties
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
