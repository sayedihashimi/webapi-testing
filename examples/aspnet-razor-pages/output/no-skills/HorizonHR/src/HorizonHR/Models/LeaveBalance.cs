using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HorizonHR.Models;

public class LeaveBalance
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    [Required]
    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    [Required]
    public int Year { get; set; }

    [Required, Column(TypeName = "decimal(5,1)")]
    public decimal TotalDays { get; set; }

    [Column(TypeName = "decimal(5,1)")]
    public decimal UsedDays { get; set; } = 0;

    [Column(TypeName = "decimal(5,1)")]
    public decimal CarriedOverDays { get; set; } = 0;

    [NotMapped]
    public decimal RemainingDays => TotalDays + CarriedOverDays - UsedDays;
}
