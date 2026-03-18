using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record CreateVaccinationRequest(
    [property: Required] int PetId,
    [property: Required, MaxLength(200)] string VaccineName,
    [property: Required] DateOnly DateAdministered,
    [property: Required] DateOnly ExpirationDate,
    string? BatchNumber,
    [property: Required] int AdministeredByVetId,
    [property: MaxLength(500)] string? Notes);

public record VaccinationResponse(
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
