using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Veterinarian DTOs ---

public class CreateVeterinarianRequest
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

    [Required]
    public DateOnly HireDate { get; set; }
}

public class UpdateVeterinarianRequest
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

    [Required]
    public DateOnly HireDate { get; set; }
}

public class VeterinarianResponse
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

public class VeterinarianSummaryResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
}
