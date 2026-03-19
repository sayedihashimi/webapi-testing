using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

// --- Requests ---

public sealed record CreateMembershipRequest
{
    [Required]
    public required int MemberId { get; init; }

    [Required]
    public required int MembershipPlanId { get; init; }

    [Required]
    public required DateOnly StartDate { get; init; }

    public PaymentStatus PaymentStatus { get; init; } = PaymentStatus.Paid;
}

public sealed record FreezeMembershipRequest
{
    [Range(7, 30)]
    public required int DurationDays { get; init; }
}

// --- Response ---

public sealed record MembershipResponse(
    int Id,
    int MemberId,
    string MemberName,
    int MembershipPlanId,
    string PlanName,
    DateOnly StartDate,
    DateOnly EndDate,
    MembershipStatus Status,
    PaymentStatus PaymentStatus,
    DateOnly? FreezeStartDate,
    DateOnly? FreezeEndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);
