using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

public sealed record MembershipResponse(
    int Id,
    int MemberId,
    string MemberName,
    int MembershipPlanId,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string PaymentStatus,
    DateOnly? FreezeStartDate,
    DateOnly? FreezeEndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateMembershipRequest
{
    [Required]
    public required int MemberId { get; init; }

    [Required]
    public required int MembershipPlanId { get; init; }

    [Required]
    public required DateOnly StartDate { get; init; }

    [Required]
    public required PaymentStatus PaymentStatus { get; init; }
}

public sealed record FreezeMembershipRequest
{
    [Required]
    public required DateOnly FreezeStartDate { get; init; }

    [Required]
    public required DateOnly FreezeEndDate { get; init; }
}

public sealed record CancelMembershipRequest
{
    public string? Reason { get; init; }
}
