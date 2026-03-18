using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Vaccination DTOs ---

public sealed record CreateVaccinationRequest(
    [Required] int PetId,
    [Required, MaxLength(200)] string VaccineName,
    [Required] DateOnly DateAdministered,
    [Required] DateOnly ExpirationDate,
    string? BatchNumber,
    [Required] int AdministeredByVetId,
    [MaxLength(500)] string? Notes);

public sealed record VaccinationResponse(
    int Id,
    int PetId,
    string PetName,
    string VaccineName,
    DateOnly DateAdministered,
    DateOnly ExpirationDate,
    string? BatchNumber,
    int AdministeredByVetId,
    string VeterinarianName,
    string? Notes,
    bool IsExpired,
    bool IsDueSoon,
    DateTime CreatedAt);
