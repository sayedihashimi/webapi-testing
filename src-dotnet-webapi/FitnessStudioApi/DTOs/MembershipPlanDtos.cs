using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

// --- Requests ---

public sealed record CreateMembershipPlanRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(1, 24)]
    public required int DurationMonths { get; init; }

    [Range(0.01, 999999.99)]
    public required decimal Price { get; init; }

    [Range(-1, 100)]
    public required int MaxClassBookingsPerWeek { get; init; }

    public required bool AllowsPremiumClasses { get; init; }
}

public sealed record UpdateMembershipPlanRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(1, 24)]
    public required int DurationMonths { get; init; }

    [Range(0.01, 999999.99)]
    public required decimal Price { get; init; }

    [Range(-1, 100)]
    public required int MaxClassBookingsPerWeek { get; init; }

    public required bool AllowsPremiumClasses { get; init; }

    public required bool IsActive { get; init; }
}

// --- Response ---

public sealed record MembershipPlanResponse(
    int Id,
    string Name,
    string? Description,
    int DurationMonths,
    decimal Price,
    int MaxClassBookingsPerWeek,
    bool AllowsPremiumClasses,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
