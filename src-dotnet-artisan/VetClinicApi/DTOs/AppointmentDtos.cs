using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

public sealed record AppointmentDto(
    int Id,
    int PetId,
    string PetName,
    int VeterinarianId,
    string VeterinarianName,
    DateTime AppointmentDate,
    int DurationMinutes,
    AppointmentStatus Status,
    string Reason,
    string? Notes,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record AppointmentDetailDto(
    int Id,
    int PetId,
    PetSummaryDto Pet,
    int VeterinarianId,
    VeterinarianDto Veterinarian,
    DateTime AppointmentDate,
    int DurationMinutes,
    AppointmentStatus Status,
    string Reason,
    string? Notes,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    MedicalRecordDto? MedicalRecord);

public sealed record CreateAppointmentDto(
    int PetId,
    int VeterinarianId,
    DateTime AppointmentDate,
    int DurationMinutes = 30,
    string Reason = "",
    string? Notes = null);

public sealed record UpdateAppointmentDto(
    int PetId,
    int VeterinarianId,
    DateTime AppointmentDate,
    int DurationMinutes,
    string Reason,
    string? Notes);

public sealed record UpdateAppointmentStatusDto(
    AppointmentStatus NewStatus,
    string? CancellationReason = null);
