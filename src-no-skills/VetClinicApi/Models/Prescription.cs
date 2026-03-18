using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.Models;

public class Prescription
{
    public int Id { get; set; }

    public int MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;

    [Required, MaxLength(200)]
    public string MedicationName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Dosage { get; set; } = string.Empty;

    public int DurationDays { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    [MaxLength(500)]
    public string? Instructions { get; set; }

    public bool IsActive => EndDate >= DateOnly.FromDateTime(DateTime.UtcNow);

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
