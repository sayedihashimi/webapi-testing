namespace VetClinicApi.Models;

public class MedicalRecord
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public Pet Pet { get; set; } = null!;
    public Veterinarian Veterinarian { get; set; } = null!;
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
