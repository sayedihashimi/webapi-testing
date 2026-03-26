using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public class LeaveRequest
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
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; }

    [Required]
    [Display(Name = "End Date")]
    public DateOnly EndDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Total Days")]
    public decimal TotalDays { get; set; }

    [Display(Name = "Status")]
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Submitted;

    [Required]
    [MaxLength(1000)]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;

    [Display(Name = "Reviewed By")]
    public int? ReviewedById { get; set; }

    [Display(Name = "Review Date")]
    public DateTime? ReviewDate { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Review Notes")]
    public string? ReviewNotes { get; set; }

    [Display(Name = "Submitted Date")]
    public DateTime SubmittedDate { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Updated At")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(LeaveTypeId))]
    public LeaveType LeaveType { get; set; } = null!;

    [ForeignKey(nameof(ReviewedById))]
    [Display(Name = "Reviewed By")]
    public Employee? ReviewedBy { get; set; }
}
