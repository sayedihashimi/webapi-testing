using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record CreatePrescriptionRequest(
    [property: Required] int MedicalRecordId,
    [property: Required, MaxLength(200)] string MedicationName,
    [property: Required, MaxLength(100)] string Dosage,
    [property: Required, Range(1, 3650)] int DurationDays,
    [property: Required] DateOnly StartDate,
    [property: MaxLength(500)] string? Instructions);

public record PrescriptionResponse(
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
