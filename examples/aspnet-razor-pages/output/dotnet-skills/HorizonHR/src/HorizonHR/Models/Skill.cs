using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Models;

public class Skill
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Skill Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
}
