using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Medical Record DTOs ---

public sealed record CreateMedicalRecordRequest(
    [Required] int AppointmentId,
    [Required, MaxLength(1000)] string Diagnosis,
    [Required, MaxLength(2000)] string Treatment,
    [MaxLength(2000)] string? Notes,
    DateOnly? FollowUpDate);

public sealed record UpdateMedicalRecordRequest(
    [Required, MaxLength(1000)] string Diagnosis,
    [Required, MaxLength(2000)] string Treatment,
    [MaxLength(2000)] string? Notes,
    DateOnly? FollowUpDate);

public sealed record MedicalRecordResponse(
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
    List<PrescriptionResponse>? Prescriptions);
