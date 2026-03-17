using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record VaccinationDto(
    int Id,
    int PetId,
    string PetName,
    string VaccineName,
    DateOnly DateAdministered,
    DateOnly ExpirationDate,
    string? BatchNumber,
    int AdministeredByVetId,
    string VetName,
    string? Notes,
    bool IsExpired,
    bool IsDueSoon,
    DateTime CreatedAt);

public record CreateVaccinationDto
{
    [Required]
    public int PetId { get; init; }

    [Required, MaxLength(200)]
    public string VaccineName { get; init; } = string.Empty;

    [Required]
    public DateOnly DateAdministered { get; init; }

    [Required]
    public DateOnly ExpirationDate { get; init; }

    public string? BatchNumber { get; init; }

    [Required]
    public int AdministeredByVetId { get; init; }

    [MaxLength(500)]
    public string? Notes { get; init; }
}
