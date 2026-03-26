using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public class EmployeeSkill
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [Display(Name = "Skill")]
    public int SkillId { get; set; }

    [Required]
    [Display(Name = "Proficiency Level")]
    public ProficiencyLevel ProficiencyLevel { get; set; }

    [Range(0, 50)]
    [Display(Name = "Years of Experience")]
    public int? YearsOfExperience { get; set; }

    [Display(Name = "Last Assessed Date")]
    public DateOnly? LastAssessedDate { get; set; }

    // Navigation properties
    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(SkillId))]
    public Skill Skill { get; set; } = null!;
}
