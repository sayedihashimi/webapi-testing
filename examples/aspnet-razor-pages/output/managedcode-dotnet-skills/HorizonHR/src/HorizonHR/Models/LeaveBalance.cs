using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Models;

public class LeaveBalance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    [Required]
    public int Year { get; set; }

    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal CarriedOverDays { get; set; }

    public decimal RemainingDays => TotalDays + CarriedOverDays - UsedDays;
}
