using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Prescription DTOs ---

public class CreatePrescriptionRequest
{
    [Required]
    public int MedicalRecordId { get; set; }

    [Required, MaxLength(200)]
    public string MedicationName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Dosage { get; set; } = string.Empty;

    [Required, Range(1, 365)]
    public int DurationDays { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [MaxLength(500)]
    public string? Instructions { get; set; }
}

public class PrescriptionResponse
{
    public int Id { get; set; }
    public int MedicalRecordId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Instructions { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
