namespace FitnessStudioApi.Models;

public sealed class Booking
{
    public int Id { get; set; }

    public int ClassScheduleId { get; set; }

    public int MemberId { get; set; }

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public int? WaitlistPosition { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CancellationDate { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ClassSchedule ClassSchedule { get; set; } = null!;

    public Member Member { get; set; } = null!;
}
