using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HorizonHR.Models;

public class LeaveBalance
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [Display(Name = "Leave Type")]
    public int LeaveTypeId { get; set; }

    [Required]
    [Display(Name = "Year")]
    public int Year { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Total Days")]
    public decimal TotalDays { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Used Days")]
    public decimal UsedDays { get; set; } = 0;

    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Carried Over Days")]
    public decimal CarriedOverDays { get; set; } = 0;

    // Computed property
    [Display(Name = "Remaining Days")]
    [NotMapped]
    public decimal RemainingDays => TotalDays + CarriedOverDays - UsedDays;

    // Navigation properties
    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(LeaveTypeId))]
    public LeaveType LeaveType { get; set; } = null!;
}
