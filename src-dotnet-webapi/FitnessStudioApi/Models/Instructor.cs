namespace FitnessStudioApi.Models;

public sealed class Instructor
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public string? Bio { get; set; }
    public string? Specializations { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
}
