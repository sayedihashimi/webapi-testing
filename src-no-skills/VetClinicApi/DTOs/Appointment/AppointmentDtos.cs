using System.ComponentModel.DataAnnotations;
using VetClinicApi.Models.Enums;

namespace VetClinicApi.DTOs.Appointment;

public class CreateAppointmentDto
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

public class UpdateAppointmentDto
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

public class UpdateAppointmentStatusDto
{
    [Required]
    public AppointmentStatus NewStatus { get; set; }

    public string? CancellationReason { get; set; }
}

public class AppointmentDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public int VeterinarianId { get; set; }
    public string VeterinarianName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AppointmentDetailDto : AppointmentDto
{
    public AppointmentPetDto Pet { get; set; } = null!;
    public AppointmentVetDto Veterinarian { get; set; } = null!;
    public MedicalRecordSummaryDto? MedicalRecord { get; set; }
}

public class AppointmentPetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public string OwnerName { get; set; } = string.Empty;
}

public class AppointmentVetDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
}

public class MedicalRecordSummaryDto
{
    public int Id { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
}
