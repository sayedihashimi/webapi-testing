namespace VetClinicApi.Models;

public sealed class MedicalRecord
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public required string Diagnosis { get; set; }
    public required string Treatment { get; set; }
    public string? Notes { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public Pet Pet { get; set; } = null!;
    public Veterinarian Veterinarian { get; set; } = null!;
    public ICollection<Prescription> Prescriptions { get; set; } = [];
}
