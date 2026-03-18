using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Vaccination DTOs ---

public class CreateVaccinationRequest
{
    [Required]
    public int PetId { get; set; }

    [Required, MaxLength(200)]
    public string VaccineName { get; set; } = string.Empty;

    [Required]
    public DateOnly DateAdministered { get; set; }

    [Required]
    public DateOnly ExpirationDate { get; set; }

    public string? BatchNumber { get; set; }

    [Required]
    public int AdministeredByVetId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class VaccinationResponse
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateOnly DateAdministered { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
    public int AdministeredByVetId { get; set; }
    public string? Notes { get; set; }
    public bool IsExpired { get; set; }
    public bool IsDueSoon { get; set; }
    public DateTime CreatedAt { get; set; }
    public VeterinarianSummaryResponse? AdministeredByVet { get; set; }
}
