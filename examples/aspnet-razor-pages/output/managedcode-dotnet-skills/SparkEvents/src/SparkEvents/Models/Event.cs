using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public class Event
{
    public int Id { get; set; }

    [Required]
    [MaxLength(300)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(5000)]
    public required string Description { get; set; }

    [Required]
    public required int EventCategoryId { get; set; }

    [Required]
    public required int VenueId { get; set; }

    [Required]
    public required DateTime StartDate { get; set; }

    [Required]
    public required DateTime EndDate { get; set; }

    [Required]
    public required DateTime RegistrationOpenDate { get; set; }

    [Required]
    public required DateTime RegistrationCloseDate { get; set; }

    public DateTime? EarlyBirdDeadline { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int TotalCapacity { get; set; }

    public int CurrentRegistrations { get; set; }

    public int WaitlistCount { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;

    public bool IsFeatured { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public EventCategory EventCategory { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public ICollection<TicketType> TicketTypes { get; set; } = [];
    public ICollection<Registration> Registrations { get; set; } = [];
}
