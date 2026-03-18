using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Medical Record DTOs ---

public class CreateMedicalRecordRequest
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

public class UpdateMedicalRecordRequest
{
    [Required, MaxLength(1000)]
    public string Diagnosis { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Treatment { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateOnly? FollowUpDate { get; set; }
}

public class MedicalRecordResponse
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public PetSummaryResponse? Pet { get; set; }
    public VeterinarianSummaryResponse? Veterinarian { get; set; }
    public List<PrescriptionResponse> Prescriptions { get; set; } = new();
}

public class MedicalRecordSummaryResponse
{
    public int Id { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
