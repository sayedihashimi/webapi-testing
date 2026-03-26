using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Models;

public class LeaveType
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Default days must be positive")]
    public int DefaultDaysPerYear { get; set; }

    public bool IsCarryOverAllowed { get; set; }
    public int MaxCarryOverDays { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public bool IsPaid { get; set; } = true;

    public ICollection<LeaveBalance> LeaveBalances { get; set; } = [];
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
}
