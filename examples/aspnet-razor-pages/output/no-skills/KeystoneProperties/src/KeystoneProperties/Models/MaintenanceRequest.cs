using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models.Enums;

namespace KeystoneProperties.Models;

public class MaintenanceRequest
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

    [Required]
    public MaintenancePriority Priority { get; set; }

    [Required]
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Submitted;

    [Required]
    public MaintenanceCategory Category { get; set; }

    [MaxLength(200)]
    [Display(Name = "Assigned To")]
    public string? AssignedTo { get; set; }

    [Display(Name = "Submitted Date")]
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Unit Unit { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
