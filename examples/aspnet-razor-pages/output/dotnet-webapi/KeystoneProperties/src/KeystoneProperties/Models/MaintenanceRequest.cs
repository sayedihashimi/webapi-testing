using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public sealed class MaintenanceRequest
{
    public int Id { get; set; }

    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public MaintenancePriority Priority { get; set; }

    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Submitted;

    public MaintenanceCategory Category { get; set; }

    [MaxLength(200)]
    public string? AssignedTo { get; set; }

    public DateTime SubmittedDate { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    [MaxLength(2000)]
    public string? CompletionNotes { get; set; }

    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
