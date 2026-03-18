using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public class CreateInstructorRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public string? Specializations { get; set; }

    [Required]
    public DateOnly HireDate { get; set; }
}

public class UpdateInstructorRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public string? Specializations { get; set; }
    public bool IsActive { get; set; }
}

public class InstructorResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Specializations { get; set; }
    public DateOnly HireDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
