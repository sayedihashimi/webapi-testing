using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

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
    ClassScheduleStatus Status,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateClassScheduleRequest
{
    public required int ClassTypeId { get; init; }

    public required int InstructorId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    [Range(1, 100)]
    public required int Capacity { get; init; }

    [Required, MaxLength(50)]
    public required string Room { get; init; }
}

public sealed record UpdateClassScheduleRequest
{
    public required int ClassTypeId { get; init; }

    public required int InstructorId { get; init; }

    public required DateTime StartTime { get; init; }

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

public sealed record ClassRosterEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    BookingStatus Status,
    DateTime BookingDate,
    DateTime? CheckInTime);

public sealed record ClassWaitlistEntry(
    int BookingId,
    int MemberId,
    string MemberName,
    int? WaitlistPosition,
    DateTime BookingDate);
