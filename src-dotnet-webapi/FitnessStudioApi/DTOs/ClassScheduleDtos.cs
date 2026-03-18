using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public class CreateClassScheduleRequest
{
    [Required]
    public int ClassTypeId { get; set; }

    [Required]
    public int InstructorId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required, Range(1, 100)]
    public int Capacity { get; set; }

    [Required, MaxLength(50)]
    public string Room { get; set; } = string.Empty;
}

public class UpdateClassScheduleRequest
{
    [Required]
    public int InstructorId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required, Range(1, 100)]
    public int Capacity { get; set; }

    [Required, MaxLength(50)]
    public string Room { get; set; } = string.Empty;
}

public class CancelClassRequest
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class ClassScheduleResponse
{
    public int Id { get; set; }
    public int ClassTypeId { get; set; }
    public string ClassTypeName { get; set; } = string.Empty;
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public int WaitlistCount { get; set; }
    public int AvailableSpots => Math.Max(0, Capacity - CurrentEnrollment);
    public string Room { get; set; } = string.Empty;
    public ClassScheduleStatus Status { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ClassRosterEntry
{
    public int BookingId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime? CheckInTime { get; set; }
}

public class WaitlistEntry
{
    public int BookingId { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int? WaitlistPosition { get; set; }
    public DateTime BookingDate { get; set; }
}
