using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record CreateMedicalRecordRequest
{
    [Required]
    public required int AppointmentId { get; init; }

    [Required, MaxLength(1000)]
    public required string Diagnosis { get; init; }

    [Required, MaxLength(2000)]
    public required string Treatment { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }

    public DateOnly? FollowUpDate { get; init; }
}

public sealed record UpdateMedicalRecordRequest
{
    [Required, MaxLength(1000)]
    public required string Diagnosis { get; init; }

    [Required, MaxLength(2000)]
    public required string Treatment { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }

    public DateOnly? FollowUpDate { get; init; }
}

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
    DateTime CreatedAt);

public sealed record MedicalRecordDetailResponse(
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
    IReadOnlyList<PrescriptionResponse> Prescriptions);
