using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public class Venue
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

    [Required, Range(1, int.MaxValue, ErrorMessage = "Max capacity must be positive")]
    public int MaxCapacity { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
