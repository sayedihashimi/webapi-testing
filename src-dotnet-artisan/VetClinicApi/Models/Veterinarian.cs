using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.Models;

public sealed class Veterinarian
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public string? Specialization { get; set; }

    [Required]
    public string LicenseNumber { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    public DateOnly HireDate { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = [];
}
