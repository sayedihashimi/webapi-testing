using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public sealed class MaintenanceRequest
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Unit")]
    public int UnitId { get; set; }

    [Required]
    [Display(Name = "Tenant")]
    public int TenantId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public MaintenancePriority Priority { get; set; }

    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Submitted;

    public MaintenanceCategory Category { get; set; }

    [MaxLength(200)]
    [Display(Name = "Assigned To")]
    public string? AssignedTo { get; set; }

    [Display(Name = "Submitted Date")]
    public DateTime SubmittedDate { get; set; }

    [Display(Name = "Assigned Date")]
    public DateTime? AssignedDate { get; set; }

    [Display(Name = "Completed Date")]
    public DateTime? CompletedDate { get; set; }

    [MaxLength(2000)]
    [Display(Name = "Completion Notes")]
    public string? CompletionNotes { get; set; }

    [Display(Name = "Estimated Cost")]
    [DataType(DataType.Currency)]
    public decimal? EstimatedCost { get; set; }

    [Display(Name = "Actual Cost")]
    [DataType(DataType.Currency)]
    public decimal? ActualCost { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Unit Unit { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
