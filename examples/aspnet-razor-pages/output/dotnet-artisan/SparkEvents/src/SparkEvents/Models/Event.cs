using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public sealed class Event
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    public int EventCategoryId { get; set; }
    public EventCategory EventCategory { get; set; } = null!;

    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RegistrationOpenDate { get; set; }
    public DateTime RegistrationCloseDate { get; set; }
    public DateTime? EarlyBirdDeadline { get; set; }

    [Range(1, int.MaxValue)]
    public int TotalCapacity { get; set; }

    public int CurrentRegistrations { get; set; }
    public int WaitlistCount { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;
    public bool IsFeatured { get; set; }

    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<TicketType> TicketTypes { get; set; } = [];
    public ICollection<Registration> Registrations { get; set; } = [];
}
