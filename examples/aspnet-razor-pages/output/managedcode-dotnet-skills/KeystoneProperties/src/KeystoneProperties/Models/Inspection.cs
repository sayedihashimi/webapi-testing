using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public class Inspection
{
    public int Id { get; set; }

    [Required]
    public int UnitId { get; set; }

    [Required]
    public InspectionType InspectionType { get; set; }

    [Required]
    public DateOnly ScheduledDate { get; set; }

    public DateOnly? CompletedDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string InspectorName { get; set; } = string.Empty;

    public OverallCondition? OverallCondition { get; set; }

    [MaxLength(5000)]
    public string? Notes { get; set; }

    public bool FollowUpRequired { get; set; }

    public int? LeaseId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Unit Unit { get; set; } = null!;

    public Lease? Lease { get; set; }
}
