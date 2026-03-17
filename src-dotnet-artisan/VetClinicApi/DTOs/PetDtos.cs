using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record PetDto(
    int Id,
    string Name,
    string Species,
    string? Breed,
    DateOnly? DateOfBirth,
    decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    bool IsActive,
    int OwnerId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PetDetailDto(
    int Id,
    string Name,
    string Species,
    string? Breed,
    DateOnly? DateOfBirth,
    decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    bool IsActive,
    int OwnerId,
    string OwnerName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PetSummaryDto(int Id, string Name, string Species, string? Breed, bool IsActive);

public record CreatePetDto
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Species { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? Breed { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Weight must be positive")]
    public decimal? Weight { get; init; }

    public string? Color { get; init; }
    public string? MicrochipNumber { get; init; }

    [Required]
    public int OwnerId { get; init; }
}

public record UpdatePetDto
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string Species { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? Breed { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Weight must be positive")]
    public decimal? Weight { get; init; }

    public string? Color { get; init; }
    public string? MicrochipNumber { get; init; }

    [Required]
    public int OwnerId { get; init; }
}
