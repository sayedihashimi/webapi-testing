using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Pet DTOs ---

public sealed record CreatePetRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [Required]
    public required string Species { get; init; }

    [MaxLength(100)]
    public string? Breed { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [Range(0.01, 9999.99)]
    public decimal? Weight { get; init; }

    public string? Color { get; init; }
    public string? MicrochipNumber { get; init; }

    [Required]
    public required int OwnerId { get; init; }
}

public sealed record UpdatePetRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [Required]
    public required string Species { get; init; }

    [MaxLength(100)]
    public string? Breed { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    [Range(0.01, 9999.99)]
    public decimal? Weight { get; init; }

    public string? Color { get; init; }
    public string? MicrochipNumber { get; init; }

    [Required]
    public required int OwnerId { get; init; }
}

public sealed record PetResponse(
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
    string OwnerFirstName,
    string OwnerLastName,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record PetSummaryResponse(
    int Id,
    string Name,
    string Species,
    string? Breed,
    bool IsActive);
