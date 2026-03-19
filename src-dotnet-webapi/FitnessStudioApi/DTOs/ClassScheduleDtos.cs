using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public sealed record ClassScheduleResponse(
    int Id,
    int ClassTypeId,
    string ClassTypeName,
    int InstructorId,
    string InstructorName,
    DateTime StartTime,
    DateTime EndTime,
    int Capacity,
    int CurrentEnrollment,
    int WaitlistCount,
    int AvailableSpots,
    string Room,
    string Status,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateClassScheduleRequest
{
    [Required]
    public required int ClassTypeId { get; init; }

    [Required]
    public required int InstructorId { get; init; }

    [Required]
    public required DateTime StartTime { get; init; }

    [Required]
    public required DateTime EndTime { get; init; }

    [Range(1, 100)]
    public required int Capacity { get; init; }

    [Required, MaxLength(50)]
    public required string Room { get; init; }
}

public sealed record UpdateClassScheduleRequest
{
    [Required]
    public required int InstructorId { get; init; }

    [Required]
    public required DateTime StartTime { get; init; }

    [Required]
    public required DateTime EndTime { get; init; }

    [Range(1, 100)]
    public required int Capacity { get; init; }

    [Required, MaxLength(50)]
    public required string Room { get; init; }
}

public sealed record CancelClassRequest
{
    [Required]
    public required string Reason { get; init; }
}

public sealed record ClassRosterResponse(
    int BookingId,
    int MemberId,
    string MemberName,
    string Status,
    DateTime BookingDate,
    DateTime? CheckInTime);

public sealed record ClassWaitlistResponse(
    int BookingId,
    int MemberId,
    string MemberName,
    int? WaitlistPosition,
    DateTime BookingDate);
