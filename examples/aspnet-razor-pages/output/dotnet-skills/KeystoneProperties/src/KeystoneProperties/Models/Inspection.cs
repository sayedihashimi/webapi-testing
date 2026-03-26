using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public class Inspection
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Unit")]
    public int UnitId { get; set; }

    [Required]
    [Display(Name = "Inspection Type")]
    public InspectionType InspectionType { get; set; }

    [Required]
    [Display(Name = "Scheduled Date")]
    public DateOnly ScheduledDate { get; set; }

    [Display(Name = "Completed Date")]
    public DateOnly? CompletedDate { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Inspector Name")]
    public string InspectorName { get; set; } = string.Empty;

    [Display(Name = "Overall Condition")]
    public OverallCondition? OverallCondition { get; set; }

    [MaxLength(5000)]
    public string? Notes { get; set; }

    [Display(Name = "Follow-Up Required")]
    public bool FollowUpRequired { get; set; }

    [Display(Name = "Lease")]
    public int? LeaseId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Unit Unit { get; set; } = null!;
    public Lease? Lease { get; set; }
}
