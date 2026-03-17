using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.Models;

public class ClassSchedule
{
    public int Id { get; set; }

    [Required]
    public int ClassTypeId { get; set; }
    public ClassType ClassType { get; set; } = null!;

    [Required]
    public int InstructorId { get; set; }
    public Instructor Instructor { get; set; } = null!;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Range(1, 100)]
    public int Capacity { get; set; }

    public int CurrentEnrollment { get; set; }

    public int WaitlistCount { get; set; }

    [Required, MaxLength(50)]
    public string Room { get; set; } = string.Empty;

    public ClassScheduleStatus Status { get; set; } = ClassScheduleStatus.Scheduled;

    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
