using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public sealed class Unit
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Property")]
    public int PropertyId { get; set; }

    [Required, MaxLength(20)]
    [Display(Name = "Unit Number")]
    public string UnitNumber { get; set; } = string.Empty;

    public int? Floor { get; set; }

    [Required, Range(0, 5)]
    public int Bedrooms { get; set; }

    [Required, Range(0.5, 4.0)]
    public decimal Bathrooms { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Square feet must be positive")]
    [Display(Name = "Square Feet")]
    public int SquareFeet { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive")]
    [Display(Name = "Monthly Rent")]
    [DataType(DataType.Currency)]
    public decimal MonthlyRent { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be positive")]
    [Display(Name = "Deposit Amount")]
    [DataType(DataType.Currency)]
    public decimal DepositAmount { get; set; }

    public UnitStatus Status { get; set; } = UnitStatus.Available;

    [MaxLength(1000)]
    public string? Amenities { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Property Property { get; set; } = null!;
    public ICollection<Lease> Leases { get; set; } = [];
    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = [];
    public ICollection<Inspection> Inspections { get; set; } = [];
}
