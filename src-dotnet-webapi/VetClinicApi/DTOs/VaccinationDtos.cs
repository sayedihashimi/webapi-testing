using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record CreateVaccinationRequest
{
    [Required]
    public required int PetId { get; init; }

    [Required, MaxLength(200)]
    public required string VaccineName { get; init; }

    [Required]
    public required DateOnly DateAdministered { get; init; }

    [Required]
    public required DateOnly ExpirationDate { get; init; }

    public string? BatchNumber { get; init; }

    [Required]
    public required int AdministeredByVetId { get; init; }

    [MaxLength(500)]
    public string? Notes { get; init; }
}

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
