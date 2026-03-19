using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

// --- Appointment DTOs ---

public sealed record CreateAppointmentRequest
{
    [Required]
    public required int PetId { get; init; }

    [Required]
    public required int VeterinarianId { get; init; }

    [Required]
    public required DateTime AppointmentDate { get; init; }

    [Range(15, 120)]
    public int DurationMinutes { get; init; } = 30;

    [Required, MaxLength(500)]
    public required string Reason { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record UpdateAppointmentRequest
{
    [Required]
    public required DateTime AppointmentDate { get; init; }

    [Range(15, 120)]
    public int DurationMinutes { get; init; } = 30;

    [Required]
    public required int VeterinarianId { get; init; }

    [Required, MaxLength(500)]
    public required string Reason { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record UpdateAppointmentStatusRequest
{
    [Required]
    public required AppointmentStatus Status { get; init; }

    [MaxLength(500)]
    public string? CancellationReason { get; init; }
}

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
    MedicalRecordResponse? MedicalRecord,
    DateTime CreatedAt,
    DateTime UpdatedAt);
