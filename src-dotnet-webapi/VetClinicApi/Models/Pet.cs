namespace VetClinicApi.Models;

public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
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
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
}
