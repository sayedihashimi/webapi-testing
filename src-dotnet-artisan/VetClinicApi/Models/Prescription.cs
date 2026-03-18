using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.Models;

public sealed class Prescription
{
    public int Id { get; set; }

    public int MedicalRecordId { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;

    [Required, MaxLength(200)]
    public string MedicationName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Dosage { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DurationDays { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate => StartDate.AddDays(DurationDays);

    public bool IsActive => EndDate >= DateOnly.FromDateTime(DateTime.UtcNow);

    [MaxLength(500)]
    public string? Instructions { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
