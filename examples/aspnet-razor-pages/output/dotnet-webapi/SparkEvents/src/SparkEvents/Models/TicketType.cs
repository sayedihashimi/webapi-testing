using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public sealed class TicketType
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative.")]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? EarlyBirdPrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive.")]
    public int Quantity { get; set; }

    public int QuantitySold { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
