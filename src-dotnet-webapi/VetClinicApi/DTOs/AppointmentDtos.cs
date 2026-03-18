using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

public record CreateAppointmentRequest(
    [property: Required] int PetId,
    [property: Required] int VeterinarianId,
    [property: Required] DateTime AppointmentDate,
    [property: Range(15, 120)] int DurationMinutes = 30,
    [property: Required, MaxLength(500)] string Reason = "",
    [property: MaxLength(2000)] string? Notes = null);

public record UpdateAppointmentRequest(
    [property: Required] int PetId,
    [property: Required] int VeterinarianId,
    [property: Required] DateTime AppointmentDate,
    [property: Range(15, 120)] int DurationMinutes = 30,
    [property: Required, MaxLength(500)] string Reason = "",
    [property: MaxLength(2000)] string? Notes = null);

public record UpdateAppointmentStatusRequest(
    [property: Required] AppointmentStatus Status,
    string? CancellationReason);

public record AppointmentResponse(
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

public record AppointmentDetailResponse(
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
