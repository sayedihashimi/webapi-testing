using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public class LeaveRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int LeaveTypeId { get; set; }
    public LeaveType LeaveType { get; set; } = null!;

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    public decimal TotalDays { get; set; }

    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Submitted;

    [Required, MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;

    public int? ReviewedById { get; set; }
    public Employee? ReviewedBy { get; set; }

    public DateTime? ReviewDate { get; set; }

    [MaxLength(1000)]
    public string? ReviewNotes { get; set; }

    public DateTime SubmittedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
