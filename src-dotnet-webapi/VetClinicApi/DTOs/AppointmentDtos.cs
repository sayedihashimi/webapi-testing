using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models;

namespace VetClinicApi.DTOs;

// --- Appointment DTOs ---

public class CreateAppointmentRequest
{
    [Required]
    public int PetId { get; set; }

    [Required]
    public int VeterinarianId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Range(15, 120)]
    public int DurationMinutes { get; set; } = 30;

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class UpdateAppointmentRequest
{
    [Required]
    public int PetId { get; set; }

    [Required]
    public int VeterinarianId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Range(15, 120)]
    public int DurationMinutes { get; set; } = 30;

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class UpdateAppointmentStatusRequest
{
    [Required]
    public AppointmentStatus Status { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }
}

public class AppointmentResponse
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public PetSummaryResponse? Pet { get; set; }
    public VeterinarianSummaryResponse? Veterinarian { get; set; }
    public MedicalRecordSummaryResponse? MedicalRecord { get; set; }
}

public class AppointmentSummaryResponse
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
}
