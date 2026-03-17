using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.Models;

public class Pet
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Species { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Breed { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public decimal? Weight { get; set; }

    public string? Color { get; set; }

    public string? MicrochipNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = [];
    public ICollection<Vaccination> Vaccinations { get; set; } = [];
}
