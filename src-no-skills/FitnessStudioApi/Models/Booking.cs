using FitnessStudioApi.Models.Enums;

namespace FitnessStudioApi.Models;

public class Booking
{
    public int Id { get; set; }

    public int ClassScheduleId { get; set; }
    public ClassSchedule ClassSchedule { get; set; } = null!;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public int? WaitlistPosition { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CancellationDate { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
