using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public class Employee
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    public DateOnly HireDate { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    [Required, MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    public EmploymentType EmploymentType { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be positive")]
    public decimal Salary { get; set; }

    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    public DateOnly? TerminationDate { get; set; }

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(2)]
    public string? State { get; set; }

    [MaxLength(10)]
    public string? ZipCode { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<Employee> DirectReports { get; set; } = [];
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = [];
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = [];
    public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = [];
}
