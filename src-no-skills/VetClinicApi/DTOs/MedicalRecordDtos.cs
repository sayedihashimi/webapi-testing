using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public class CreateMedicalRecordDto
{
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public int PetId { get; set; }

    [Required]
    public int VeterinarianId { get; set; }

    [Required, MaxLength(1000)]
    public string Diagnosis { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Treatment { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateOnly? FollowUpDate { get; set; }
}

public class UpdateMedicalRecordDto
{
    [Required, MaxLength(1000)]
    public string Diagnosis { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Treatment { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateOnly? FollowUpDate { get; set; }
}

public class MedicalRecordResponseDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PetId { get; set; }
    public PetSummaryDto? Pet { get; set; }
    public int VeterinarianId { get; set; }
    public VeterinarianSummaryDto? Veterinarian { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrescriptionResponseDto> Prescriptions { get; set; } = new();
}

public class MedicalRecordSummaryDto
{
    public int Id { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
