using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public class Lease
{
    public int Id { get; set; }

    [Required]
    public int UnitId { get; set; }

    [Required]
    public int TenantId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MonthlyRentAmount { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal DepositAmount { get; set; }

    public DepositStatus DepositStatus { get; set; } = DepositStatus.Held;

    public LeaseStatus Status { get; set; } = LeaseStatus.Pending;

    public int? RenewalOfLeaseId { get; set; }

    public DateOnly? TerminationDate { get; set; }

    [MaxLength(2000)]
    public string? TerminationReason { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Unit Unit { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;

    public Lease? RenewalOfLease { get; set; }

    public ICollection<Payment> Payments { get; set; } = [];

    public ICollection<Inspection> Inspections { get; set; } = [];
}
