using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public class Unit
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Property")]
    public int PropertyId { get; set; }

    [Required, MaxLength(20)]
    [Display(Name = "Unit Number")]
    public string UnitNumber { get; set; } = string.Empty;

    public int? Floor { get; set; }

    [Required]
    [Range(0, 5, ErrorMessage = "Bedrooms must be between 0 and 5.")]
    public int Bedrooms { get; set; }

    [Required]
    [Range(0.5, 4.0, ErrorMessage = "Bathrooms must be between 0.5 and 4.")]
    public decimal Bathrooms { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Square feet must be positive.")]
    [Display(Name = "Square Feet")]
    public int SquareFeet { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive.")]
    [Display(Name = "Monthly Rent")]
    [DataType(DataType.Currency)]
    public decimal MonthlyRent { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be positive.")]
    [Display(Name = "Deposit Amount")]
    [DataType(DataType.Currency)]
    public decimal DepositAmount { get; set; }

    [Required]
    public UnitStatus Status { get; set; } = UnitStatus.Available;

    [MaxLength(1000)]
    public string? Amenities { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Property Property { get; set; } = null!;
    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
}
