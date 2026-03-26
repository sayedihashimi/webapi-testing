using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public class Unit
{
    public int Id { get; set; }

    [Required]
    public int PropertyId { get; set; }

    [Required]
    [MaxLength(20)]
    public string UnitNumber { get; set; } = string.Empty;

    public int? Floor { get; set; }

    [Required]
    [Range(0, 5)]
    public int Bedrooms { get; set; }

    [Required]
    [Range(0.5, 4)]
    public decimal Bathrooms { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int SquareFeet { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal MonthlyRent { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal DepositAmount { get; set; }

    public UnitStatus Status { get; set; } = UnitStatus.Available;

    [MaxLength(1000)]
    public string? Amenities { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Property Property { get; set; } = null!;

    public ICollection<Lease> Leases { get; set; } = [];

    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = [];

    public ICollection<Inspection> Inspections { get; set; } = [];
}
