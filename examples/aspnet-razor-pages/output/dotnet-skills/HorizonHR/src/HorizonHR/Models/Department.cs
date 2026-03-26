using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HorizonHR.Models;

public class Department
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Department Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    [Display(Name = "Department Code")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Manager")]
    public int? ManagerId { get; set; }

    [Display(Name = "Parent Department")]
    public int? ParentDepartmentId { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Updated At")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ManagerId))]
    public Employee? Manager { get; set; }

    [ForeignKey(nameof(ParentDepartmentId))]
    [Display(Name = "Parent Department")]
    public Department? ParentDepartment { get; set; }

    [InverseProperty(nameof(ParentDepartment))]
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

    [InverseProperty(nameof(Employee.Department))]
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
