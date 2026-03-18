using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record CreateMedicalRecordRequest(
    [property: Required] int AppointmentId,
    [property: Required, MaxLength(1000)] string Diagnosis,
    [property: Required, MaxLength(2000)] string Treatment,
    [property: MaxLength(2000)] string? Notes,
    DateOnly? FollowUpDate);

public record UpdateMedicalRecordRequest(
    [property: Required, MaxLength(1000)] string Diagnosis,
    [property: Required, MaxLength(2000)] string Treatment,
    [property: MaxLength(2000)] string? Notes,
    DateOnly? FollowUpDate);

public record MedicalRecordResponse(
    int Id,
    int AppointmentId,
    int PetId,
    string PetName,
    int VeterinarianId,
    string VeterinarianName,
    string Diagnosis,
    string Treatment,
    string? Notes,
    DateOnly? FollowUpDate,
    DateTime CreatedAt);

public record MedicalRecordDetailResponse(
    int Id,
    int AppointmentId,
    int PetId,
    string PetName,
    int VeterinarianId,
    string VeterinarianName,
    string Diagnosis,
    string Treatment,
    string? Notes,
    DateOnly? FollowUpDate,
    DateTime CreatedAt,
    List<PrescriptionResponse> Prescriptions);
