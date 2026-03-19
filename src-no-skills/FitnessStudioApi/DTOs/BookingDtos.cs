using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public class BookingCreateDto
{
    [Required]
    public int ClassScheduleId { get; set; }

    [Required]
    public int MemberId { get; set; }
}

public class BookingResponseDto
{
    public int Id { get; set; }
    public int ClassScheduleId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime ClassStartTime { get; set; }
    public DateTime ClassEndTime { get; set; }
    public string Room { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? WaitlistPosition { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CancelBookingDto
{
    public string? Reason { get; set; }
}

public class RosterEntryDto
{
    public int BookingId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public DateTime? CheckInTime { get; set; }
}

public class WaitlistEntryDto
{
    public int BookingId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int WaitlistPosition { get; set; }
    public DateTime BookingDate { get; set; }
}
