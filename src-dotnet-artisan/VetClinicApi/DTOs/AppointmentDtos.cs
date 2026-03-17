using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

public record AppointmentDto(
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

public record AppointmentDetailDto(
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
    MedicalRecordDto? MedicalRecord);

public record CreateAppointmentDto
{
    [Required]
    public int PetId { get; init; }

    [Required]
    public int VeterinarianId { get; init; }

    [Required]
    public DateTime AppointmentDate { get; init; }

    [Range(15, 120)]
    public int DurationMinutes { get; init; } = 30;

    [Required, MaxLength(500)]
    public string Reason { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public record UpdateAppointmentDto
{
    [Required]
    public int PetId { get; init; }

    [Required]
    public int VeterinarianId { get; init; }

    [Required]
    public DateTime AppointmentDate { get; init; }

    [Range(15, 120)]
    public int DurationMinutes { get; init; } = 30;

    [Required, MaxLength(500)]
    public string Reason { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public record UpdateAppointmentStatusDto
{
    [Required]
    public AppointmentStatus Status { get; init; }

    public string? CancellationReason { get; init; }
}
