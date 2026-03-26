using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public class TicketType
{
    public int Id { get; set; }

    [Required]
    public required int EventId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal Price { get; set; }

    public decimal? EarlyBirdPrice { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int Quantity { get; set; }

    public int QuantitySold { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Event Event { get; set; } = null!;
    public ICollection<Registration> Registrations { get; set; } = [];
}
