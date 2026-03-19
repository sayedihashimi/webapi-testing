using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

// --- Requests ---

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
    [MaxLength(500)]
    public string? Reason { get; init; }
}

// --- Responses ---

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
    ClassStatus Status,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record ClassRosterEntryResponse(
    int BookingId,
    int MemberId,
    string MemberName,
    string Email,
    BookingStatus Status,
    DateTime BookingDate,
    DateTime? CheckInTime);

public sealed record WaitlistEntryResponse(
    int BookingId,
    int MemberId,
    string MemberName,
    int? WaitlistPosition,
    DateTime BookingDate);
