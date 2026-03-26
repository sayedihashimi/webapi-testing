using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public class Registration
{
    public int Id { get; set; }

    [Required]
    public required int EventId { get; set; }

    [Required]
    public required int AttendeeId { get; set; }

    [Required]
    public required int TicketTypeId { get; set; }

    [Required]
    public required string ConfirmationNumber { get; set; }

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;

    [Required]
    public required decimal AmountPaid { get; set; }

    public int? WaitlistPosition { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public DateTime? CancellationDate { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime? CheckInTime { get; set; }

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Event Event { get; set; } = null!;
    public Attendee Attendee { get; set; } = null!;
    public TicketType TicketType { get; set; } = null!;
    public CheckIn? CheckIn { get; set; }
}
