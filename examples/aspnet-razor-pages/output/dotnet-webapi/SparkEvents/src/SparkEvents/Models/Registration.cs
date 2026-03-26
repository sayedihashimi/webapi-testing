using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public sealed class Registration
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int AttendeeId { get; set; }
    public Attendee Attendee { get; set; } = null!;

    public int TicketTypeId { get; set; }
    public TicketType TicketType { get; set; } = null!;

    [Required, MaxLength(20)]
    public string ConfirmationNumber { get; set; } = string.Empty;

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;

    public decimal AmountPaid { get; set; }

    public int? WaitlistPosition { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public DateTime? CancellationDate { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    public DateTime? CheckInTime { get; set; }

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CheckIn? CheckIn { get; set; }
}
