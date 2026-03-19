using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.Models;

public sealed class ClassType
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(30, 120)]
    public int DefaultDurationMinutes { get; set; }

    [Range(1, 50)]
    public int DefaultCapacity { get; set; }

    public bool IsPremium { get; set; }

    public int? CaloriesPerSession { get; set; }

    public DifficultyLevel DifficultyLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
