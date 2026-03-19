namespace FitnessStudioApi.Models;

public sealed class ClassType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DefaultDurationMinutes { get; set; }
    public int DefaultCapacity { get; set; }
    public bool IsPremium { get; set; }
    public int? CaloriesPerSession { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced,
    AllLevels
}
