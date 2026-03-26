using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public class MaintenanceRequest
{
    public int Id { get; set; }

    [Required]
    public int UnitId { get; set; }

    [Required]
    public int TenantId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public MaintenancePriority Priority { get; set; }

    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Submitted;

    [Required]
    public MaintenanceCategory Category { get; set; }

    [MaxLength(200)]
    public string? AssignedTo { get; set; }

    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

    public DateTime? AssignedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    [MaxLength(2000)]
    public string? CompletionNotes { get; set; }

    public decimal? EstimatedCost { get; set; }

    public decimal? ActualCost { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Unit Unit { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
