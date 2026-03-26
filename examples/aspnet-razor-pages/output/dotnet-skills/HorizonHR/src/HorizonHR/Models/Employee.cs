using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    [Display(Name = "Employee Number")]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [Required]
    [Display(Name = "Date of Birth")]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    [Display(Name = "Hire Date")]
    public DateOnly HireDate { get; set; }

    [Required]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(200)]
    [Display(Name = "Job Title")]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Employment Type")]
    public EmploymentType EmploymentType { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than zero.")]
    [Display(Name = "Salary")]
    public decimal Salary { get; set; }

    [Display(Name = "Manager")]
    public int? ManagerId { get; set; }

    [Display(Name = "Status")]
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    [Display(Name = "Termination Date")]
    public DateOnly? TerminationDate { get; set; }

    [MaxLength(500)]
    [Display(Name = "Profile Image URL")]
    public string? ProfileImageUrl { get; set; }

    [MaxLength(500)]
    [Display(Name = "Address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [MaxLength(2)]
    [Display(Name = "State")]
    public string? State { get; set; }

    [MaxLength(10)]
    [Display(Name = "Zip Code")]
    public string? ZipCode { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Updated At")]
    public DateTime UpdatedAt { get; set; }

    // Computed property
    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";

    // Navigation properties
    [ForeignKey(nameof(DepartmentId))]
    public Department Department { get; set; } = null!;

    [ForeignKey(nameof(ManagerId))]
    [Display(Name = "Manager")]
    public Employee? Manager { get; set; }

    [InverseProperty(nameof(Manager))]
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    [InverseProperty(nameof(LeaveRequest.Employee))]
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    [InverseProperty(nameof(LeaveBalance.Employee))]
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();

    [InverseProperty(nameof(PerformanceReview.Employee))]
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();

    [InverseProperty(nameof(EmployeeSkill.Employee))]
    public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
}
