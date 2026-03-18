using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Pet DTOs ---

public sealed record CreatePetRequest(
    [Required, MaxLength(100)] string Name,
    [Required] string Species,
    [MaxLength(100)] string? Breed,
    DateOnly? DateOfBirth,
    [Range(0.01, double.MaxValue)] decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    [Required] int OwnerId);

public sealed record UpdatePetRequest(
    [Required, MaxLength(100)] string Name,
    [Required] string Species,
    [MaxLength(100)] string? Breed,
    DateOnly? DateOfBirth,
    [Range(0.01, double.MaxValue)] decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    [Required] int OwnerId);

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
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record PetDetailResponse(
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
