namespace VetClinicApi.DTOs;

public sealed record PrescriptionDto(
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

public sealed record CreatePrescriptionDto(
    int MedicalRecordId,
    string MedicationName,
    string Dosage,
    int DurationDays,
    DateOnly StartDate,
    string? Instructions = null);
