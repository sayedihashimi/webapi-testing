using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public class EmployeeSkill
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    [Required]
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    [Required]
    public ProficiencyLevel ProficiencyLevel { get; set; }

    [Range(0, 50)]
    public int? YearsOfExperience { get; set; }

    public DateOnly? LastAssessedDate { get; set; }
}
