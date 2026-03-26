using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Models;

public class LeaveType
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, Range(1, 365)]
    public int DefaultDaysPerYear { get; set; }

    public bool IsCarryOverAllowed { get; set; } = false;
    public int MaxCarryOverDays { get; set; } = 0;
    public bool RequiresApproval { get; set; } = true;
    public bool IsPaid { get; set; } = true;

    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
