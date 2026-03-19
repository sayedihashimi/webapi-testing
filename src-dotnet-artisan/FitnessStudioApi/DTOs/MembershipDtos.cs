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
    MembershipStatus Status,
    PaymentStatus PaymentStatus,
    DateOnly? FreezeStartDate,
    DateOnly? FreezeEndDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateMembershipRequest
{
    public required int MemberId { get; init; }

    public required int MembershipPlanId { get; init; }

    public required DateOnly StartDate { get; init; }

    public PaymentStatus PaymentStatus { get; init; } = PaymentStatus.Pending;
}

public sealed record FreezeMembershipRequest
{
    public required DateOnly FreezeStartDate { get; init; }

    public required DateOnly FreezeEndDate { get; init; }
}
