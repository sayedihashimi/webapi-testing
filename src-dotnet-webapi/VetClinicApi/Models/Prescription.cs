namespace VetClinicApi.Models;

public class Prescription
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsActive => EndDate >= DateOnly.FromDateTime(DateTime.UtcNow);

    public MedicalRecord MedicalRecord { get; set; } = null!;
}
