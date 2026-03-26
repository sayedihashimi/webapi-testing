using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HorizonHR.Models;

public sealed class LeaveBalance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    [Range(2000, 2100)]
    public int Year { get; set; }

    [Column(TypeName = "decimal(5,1)")]
    public decimal TotalDays { get; set; }

    [Column(TypeName = "decimal(5,1)")]
    public decimal UsedDays { get; set; }

    [Column(TypeName = "decimal(5,1)")]
    public decimal CarriedOverDays { get; set; }

    [NotMapped]
    public decimal RemainingDays => TotalDays + CarriedOverDays - UsedDays;
}
