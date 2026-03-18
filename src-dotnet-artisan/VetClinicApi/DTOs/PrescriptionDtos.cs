using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Prescription DTOs ---

public sealed record CreatePrescriptionRequest(
    [Required] int MedicalRecordId,
    [Required, MaxLength(200)] string MedicationName,
    [Required, MaxLength(100)] string Dosage,
    [Range(1, int.MaxValue)] int DurationDays,
    [Required] DateOnly StartDate,
    [MaxLength(500)] string? Instructions);

public sealed record PrescriptionResponse(
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
