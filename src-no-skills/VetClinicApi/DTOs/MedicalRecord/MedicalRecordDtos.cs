using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs.MedicalRecord;

public class CreateMedicalRecordDto
{
    [Required]
    public int AppointmentId { get; set; }

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

public class MedicalRecordDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = string.Empty;
    public int VeterinarianId { get; set; }
    public string VeterinarianName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrescriptionSummaryDto> Prescriptions { get; set; } = new();
}

public class PrescriptionSummaryDto
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
