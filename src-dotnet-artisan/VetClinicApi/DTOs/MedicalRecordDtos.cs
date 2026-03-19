namespace VetClinicApi.DTOs;

public sealed record MedicalRecordDto(
    int Id,
    int AppointmentId,
    int PetId,
    int VeterinarianId,
    string Diagnosis,
    string Treatment,
    string? Notes,
    DateOnly? FollowUpDate,
    DateTime CreatedAt);

public sealed record MedicalRecordDetailDto(
    int Id,
    int AppointmentId,
    int PetId,
    int VeterinarianId,
    string Diagnosis,
    string Treatment,
    string? Notes,
    DateOnly? FollowUpDate,
    DateTime CreatedAt,
    IReadOnlyList<PrescriptionDto> Prescriptions);

public sealed record CreateMedicalRecordDto(
    int AppointmentId,
    string Diagnosis,
    string Treatment,
    string? Notes = null,
    DateOnly? FollowUpDate = null);

public sealed record UpdateMedicalRecordDto(
    string Diagnosis,
    string Treatment,
    string? Notes,
    DateOnly? FollowUpDate);
