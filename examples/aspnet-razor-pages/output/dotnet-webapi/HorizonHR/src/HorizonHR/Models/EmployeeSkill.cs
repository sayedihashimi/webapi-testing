using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public sealed class EmployeeSkill
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public ProficiencyLevel ProficiencyLevel { get; set; }
    public int? YearsOfExperience { get; set; }
    public DateOnly? LastAssessedDate { get; set; }
}
