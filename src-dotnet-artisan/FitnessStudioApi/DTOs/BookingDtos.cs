using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public sealed record BookingResponse(
    int Id,
    int ClassScheduleId,
    string ClassName,
    int MemberId,
    string MemberName,
    DateTime BookingDate,
    BookingStatus Status,
    int? WaitlistPosition,
    DateTime? CheckInTime,
    DateTime? CancellationDate,
    string? CancellationReason,
    DateTime StartTime,
    DateTime EndTime,
    string Room,
    string InstructorName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateBookingRequest
{
    public required int ClassScheduleId { get; init; }

    public required int MemberId { get; init; }
}

public sealed record CancelBookingRequest
{
    public string? Reason { get; init; }
}
