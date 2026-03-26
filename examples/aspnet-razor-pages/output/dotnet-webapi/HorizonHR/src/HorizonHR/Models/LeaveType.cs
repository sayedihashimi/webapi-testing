using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Models;

public sealed class LeaveType
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DefaultDaysPerYear { get; set; }

    public bool IsCarryOverAllowed { get; set; }
    public int MaxCarryOverDays { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public bool IsPaid { get; set; } = true;

    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
