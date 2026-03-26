using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public sealed class Employee
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
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

    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
    public ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
}
