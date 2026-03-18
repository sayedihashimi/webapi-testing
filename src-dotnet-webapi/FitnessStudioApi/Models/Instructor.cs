namespace FitnessStudioApi.Models;

public class Instructor
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Specializations { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ClassSchedule> ClassSchedules { get; set; } = [];
}
