using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public sealed class Lease
{
    public int Id { get; set; }

    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "MonthlyRentAmount must be positive.")]
    public decimal MonthlyRentAmount { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "DepositAmount must be positive.")]
    public decimal DepositAmount { get; set; }

    public DepositStatus DepositStatus { get; set; } = DepositStatus.Held;

    public LeaseStatus Status { get; set; } = LeaseStatus.Pending;

    public int? RenewalOfLeaseId { get; set; }
    public Lease? RenewalOfLease { get; set; }

    public DateOnly? TerminationDate { get; set; }

    [MaxLength(2000)]
    public string? TerminationReason { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    public ICollection<Lease> Renewals { get; set; } = new List<Lease>();
}
