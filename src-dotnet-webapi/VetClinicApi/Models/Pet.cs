namespace VetClinicApi.Models;

public sealed class Pet
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Species { get; set; }
    public string? Breed { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public decimal? Weight { get; set; }
    public string? Color { get; set; }
    public string? MicrochipNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Owner Owner { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = [];
    public ICollection<Vaccination> Vaccinations { get; set; } = [];
}
