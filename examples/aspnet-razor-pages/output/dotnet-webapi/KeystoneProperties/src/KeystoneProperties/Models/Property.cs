using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public sealed class Property
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string State { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    public string ZipCode { get; set; } = string.Empty;

    public PropertyType PropertyType { get; set; }

    public int? YearBuilt { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "TotalUnits must be positive.")]
    public int TotalUnits { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
