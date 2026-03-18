namespace VetClinicApi.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public required string Reason { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Pet Pet { get; set; } = null!;
    public Veterinarian Veterinarian { get; set; } = null!;
    public MedicalRecord? MedicalRecord { get; set; }
}
