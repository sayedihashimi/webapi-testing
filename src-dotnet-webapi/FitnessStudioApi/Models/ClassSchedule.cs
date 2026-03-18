namespace FitnessStudioApi.Models;

public class ClassSchedule
{
    public int Id { get; set; }
    public int ClassTypeId { get; set; }
    public int InstructorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public int WaitlistCount { get; set; }
    public string Room { get; set; } = string.Empty;
    public ClassScheduleStatus Status { get; set; } = ClassScheduleStatus.Scheduled;
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ClassType ClassType { get; set; } = null!;
    public Instructor Instructor { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = [];
}
