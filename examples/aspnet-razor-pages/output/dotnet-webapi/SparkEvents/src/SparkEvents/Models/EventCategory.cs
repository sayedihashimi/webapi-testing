using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public sealed class EventCategory
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(7)]
    public string? ColorHex { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
