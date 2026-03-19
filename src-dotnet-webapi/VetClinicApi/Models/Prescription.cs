namespace VetClinicApi.Models;

public sealed class Prescription
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public required string MedicationName { get; set; }
    public required string Dosage { get; set; }
    public int DurationDays { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }

    public MedicalRecord MedicalRecord { get; set; } = null!;
}
