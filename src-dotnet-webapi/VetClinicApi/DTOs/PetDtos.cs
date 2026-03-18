using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record CreatePetRequest(
    [property: Required, MaxLength(100)] string Name,
    [property: Required] string Species,
    [property: MaxLength(100)] string? Breed,
    DateOnly? DateOfBirth,
    [property: Range(0.01, 9999.99)] decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    [property: Required] int OwnerId);

public record UpdatePetRequest(
    [property: Required, MaxLength(100)] string Name,
    [property: Required] string Species,
    [property: MaxLength(100)] string? Breed,
    DateOnly? DateOfBirth,
    [property: Range(0.01, 9999.99)] decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    [property: Required] int OwnerId);

public record PetResponse(
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

public record PetDetailResponse(
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
