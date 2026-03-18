using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public class CreateBookingRequest
{
    [Required]
    public int ClassScheduleId { get; set; }

    [Required]
    public int MemberId { get; set; }
}

public class CancelBookingRequest
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class BookingResponse
{
    public int Id { get; set; }
    public int ClassScheduleId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; }
    public int? WaitlistPosition { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime ScheduleStartTime { get; set; }
    public DateTime ScheduleEndTime { get; set; }
    public string Room { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
