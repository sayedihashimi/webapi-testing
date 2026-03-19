using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public class ClassTypeCreateDto
{
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

    [Required]
    public string DifficultyLevel { get; set; } = "AllLevels";
}

public class ClassTypeUpdateDto
{
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

    [Required]
    public string DifficultyLevel { get; set; } = "AllLevels";

    public bool IsActive { get; set; }
}

public class ClassTypeResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public int DefaultCapacity { get; set; }
    public bool IsPremium { get; set; }
    public int? CaloriesPerSession { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
