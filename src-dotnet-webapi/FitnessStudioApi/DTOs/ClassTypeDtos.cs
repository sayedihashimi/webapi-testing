using System.ComponentModel.DataAnnotations;
using FitnessStudioApi.Models;

namespace FitnessStudioApi.DTOs;

// --- Requests ---

public sealed record CreateClassTypeRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(30, 120)]
    public required int DefaultDurationMinutes { get; init; }

    [Range(1, 50)]
    public required int DefaultCapacity { get; init; }

    public required bool IsPremium { get; init; }

    [Range(0, 2000)]
    public int? CaloriesPerSession { get; init; }

    public required DifficultyLevel DifficultyLevel { get; init; }
}

public sealed record UpdateClassTypeRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Range(30, 120)]
    public required int DefaultDurationMinutes { get; init; }

    [Range(1, 50)]
    public required int DefaultCapacity { get; init; }

    public required bool IsPremium { get; init; }

    [Range(0, 2000)]
    public int? CaloriesPerSession { get; init; }

    public required DifficultyLevel DifficultyLevel { get; init; }

    public required bool IsActive { get; init; }
}

// --- Response ---

public sealed record ClassTypeResponse(
    int Id,
    string Name,
    string? Description,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    bool IsPremium,
    int? CaloriesPerSession,
    DifficultyLevel DifficultyLevel,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
