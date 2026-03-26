using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public sealed class Unit
{
    public int Id { get; set; }

    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    [Required, MaxLength(20)]
    public string UnitNumber { get; set; } = string.Empty;

    public int? Floor { get; set; }

    [Required, Range(0, 5)]
    public int Bedrooms { get; set; }

    [Required, Range(0.5, 4.0)]
    public decimal Bathrooms { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "SquareFeet must be positive.")]
    public int SquareFeet { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "MonthlyRent must be positive.")]
    public decimal MonthlyRent { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "DepositAmount must be positive.")]
    public decimal DepositAmount { get; set; }

    public UnitStatus Status { get; set; } = UnitStatus.Available;

    [MaxLength(1000)]
    public string? Amenities { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
}
