using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

public sealed record CreateAppointmentRequest(
    [Required] int PetId,
    [Required] int VeterinarianId,
    [Required] DateTime AppointmentDate,
    [Range(15, 120)] int DurationMinutes = 30,
    [Required, MaxLength(500)] string Reason = "",
    [MaxLength(2000)] string? Notes = null);

public sealed record UpdateAppointmentRequest(
    [Required] int PetId,
    [Required] int VeterinarianId,
    [Required] DateTime AppointmentDate,
    [Range(15, 120)] int DurationMinutes = 30,
    [Required, MaxLength(500)] string Reason = "",
    [MaxLength(2000)] string? Notes = null);

public sealed record UpdateAppointmentStatusRequest(
    [Required] AppointmentStatus Status,
    string? CancellationReason = null);

public sealed record AppointmentResponse(
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

public sealed record AppointmentDetailResponse(
    int Id,
    int PetId,
    string PetName,
    string PetSpecies,
    int OwnerId,
    string OwnerName,
    int VeterinarianId,
    string VeterinarianName,
    DateTime AppointmentDate,
    int DurationMinutes,
    AppointmentStatus Status,
    string Reason,
    string? Notes,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    MedicalRecordResponse? MedicalRecord);
