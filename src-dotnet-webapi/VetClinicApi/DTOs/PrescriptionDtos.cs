using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record CreatePrescriptionRequest
{
    [Required]
    public required int MedicalRecordId { get; init; }

    [Required, MaxLength(200)]
    public required string MedicationName { get; init; }

    [Required, MaxLength(100)]
    public required string Dosage { get; init; }

    [Range(1, 3650)]
    public required int DurationDays { get; init; }

    [Required]
    public required DateOnly StartDate { get; init; }

    [MaxLength(500)]
    public string? Instructions { get; init; }
}

public sealed record PrescriptionResponse(
    int Id,
    int MedicalRecordId,
    string MedicationName,
    string Dosage,
    int DurationDays,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Instructions,
    bool IsActive,
    DateTime CreatedAt);
