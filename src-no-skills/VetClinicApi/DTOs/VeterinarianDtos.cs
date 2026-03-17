using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public class CreateVeterinarianDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    [Required]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    public DateOnly HireDate { get; set; }
}

public class UpdateVeterinarianDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string Phone { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    [Required]
    public string LicenseNumber { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
}

public class VeterinarianDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateOnly HireDate { get; set; }
}

public class VeterinarianSummaryDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
}
