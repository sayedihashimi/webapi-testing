using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.Models;

public class Instructor
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public string? Specializations { get; set; }

    public DateOnly HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
}
