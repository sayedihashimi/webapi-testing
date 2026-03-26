using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public sealed class Lease
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Unit")]
    public int UnitId { get; set; }

    [Required]
    [Display(Name = "Tenant")]
    public int TenantId { get; set; }

    [Required]
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; }

    [Required]
    [Display(Name = "End Date")]
    public DateOnly EndDate { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive")]
    [Display(Name = "Monthly Rent")]
    [DataType(DataType.Currency)]
    public decimal MonthlyRentAmount { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Deposit must be positive")]
    [Display(Name = "Deposit")]
    [DataType(DataType.Currency)]
    public decimal DepositAmount { get; set; }

    [Display(Name = "Deposit Status")]
    public DepositStatus DepositStatus { get; set; } = DepositStatus.Held;

    public LeaseStatus Status { get; set; } = LeaseStatus.Pending;

    [Display(Name = "Renewal Of")]
    public int? RenewalOfLeaseId { get; set; }

    [Display(Name = "Termination Date")]
    public DateOnly? TerminationDate { get; set; }

    [MaxLength(2000)]
    [Display(Name = "Termination Reason")]
    public string? TerminationReason { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Unit Unit { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public Lease? RenewalOfLease { get; set; }
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Inspection> Inspections { get; set; } = [];
}
