using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public class Venue
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Address { get; set; }

    [Required]
    [MaxLength(100)]
    public required string City { get; set; }

    [Required]
    [MaxLength(2)]
    public required string State { get; set; }

    [Required]
    [MaxLength(10)]
    public required string ZipCode { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int MaxCapacity { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Event> Events { get; set; } = [];
}
