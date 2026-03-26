using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkEvents.Models;

public class Registration
{
    public int Id { get; set; }

    [Required]
    public int EventId { get; set; }

    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; } = null!;

    [Required]
    public int AttendeeId { get; set; }

    [ForeignKey(nameof(AttendeeId))]
    public Attendee Attendee { get; set; } = null!;

    [Required]
    public int TicketTypeId { get; set; }

    [ForeignKey(nameof(TicketTypeId))]
    public TicketType TicketType { get; set; } = null!;

    [Required, MaxLength(20)]
    public string ConfirmationNumber { get; set; } = string.Empty;

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    public int? WaitlistPosition { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    public DateTime? CancellationDate { get; set; }

    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    public DateTime? CheckInTime { get; set; }

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public CheckIn? CheckIn { get; set; }
}
