using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public class ClassScheduleCreateDto
{
    [Required]
    public int ClassTypeId { get; set; }

    [Required]
    public int InstructorId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Range(1, 50)]
    public int? Capacity { get; set; }

    [Required, MaxLength(50)]
    public string Room { get; set; } = string.Empty;
}

public class ClassScheduleUpdateDto
{
    [Required]
    public int ClassTypeId { get; set; }

    [Required]
    public int InstructorId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Range(1, 50)]
    public int? Capacity { get; set; }

    [Required, MaxLength(50)]
    public string Room { get; set; } = string.Empty;
}

public class ClassScheduleResponseDto
{
    public int Id { get; set; }
    public int ClassTypeId { get; set; }
    public string ClassTypeName { get; set; } = string.Empty;
    public int InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public int WaitlistCount { get; set; }
    public int AvailableSpots => Capacity - CurrentEnrollment;
    public string Room { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
    public bool IsPremium { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CancelClassDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}
