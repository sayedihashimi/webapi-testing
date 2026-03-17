using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record PrescriptionDto(
    int Id,
    int MedicalRecordId,
    string MedicationName,
    string Dosage,
    int DurationDays,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive,
    string? Instructions,
    DateTime CreatedAt);

public record CreatePrescriptionDto
{
    [Required]
    public int MedicalRecordId { get; init; }

    [Required, MaxLength(200)]
    public string MedicationName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string Dosage { get; init; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "DurationDays must be positive")]
    public int DurationDays { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    [MaxLength(500)]
    public string? Instructions { get; init; }
}
