using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public class CreatePrescriptionDto
{
    [Required]
    public int MedicalRecordId { get; set; }

    [Required, MaxLength(200)]
    public string MedicationName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Dosage { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "DurationDays must be positive")]
    public int DurationDays { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [MaxLength(500)]
    public string? Instructions { get; set; }
}

public class PrescriptionResponseDto
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
