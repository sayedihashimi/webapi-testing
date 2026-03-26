using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkEvents.Models;

public class Event
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int EventCategoryId { get; set; }
    public EventCategory EventCategory { get; set; } = null!;

    [Required]
    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public DateTime RegistrationOpenDate { get; set; }

    [Required]
    public DateTime RegistrationCloseDate { get; set; }

    public DateTime? EarlyBirdDeadline { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "Total capacity must be positive")]
    public int TotalCapacity { get; set; }

    public int CurrentRegistrations { get; set; }

    public int WaitlistCount { get; set; }

    [Required]
    public EventStatus Status { get; set; } = EventStatus.Draft;

    public bool IsFeatured { get; set; }

    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TicketType> TicketTypes { get; set; } = new List<TicketType>();
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
