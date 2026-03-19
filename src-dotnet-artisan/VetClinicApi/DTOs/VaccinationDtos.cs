namespace VetClinicApi.DTOs;

public sealed record VaccinationDto(
    int Id,
    int PetId,
    string VaccineName,
    DateOnly DateAdministered,
    DateOnly ExpirationDate,
    string? BatchNumber,
    int AdministeredByVetId,
    string AdministeredByVetName,
    string? Notes,
    DateTime CreatedAt,
    bool IsExpired,
    bool IsDueSoon);

public sealed record CreateVaccinationDto(
    int PetId,
    string VaccineName,
    DateOnly DateAdministered,
    DateOnly ExpirationDate,
    string? BatchNumber = null,
    int AdministeredByVetId = 0,
    string? Notes = null);
