namespace VetClinicApi.DTOs;

public sealed record PetDto(
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

public sealed record PetDetailDto(
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
    OwnerDto Owner,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record PetSummaryDto(
    int Id,
    string Name,
    string Species,
    string? Breed,
    bool IsActive);

public sealed record CreatePetDto(
    string Name,
    string Species,
    string? Breed,
    DateOnly? DateOfBirth,
    decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    int OwnerId);

public sealed record UpdatePetDto(
    string Name,
    string Species,
    string? Breed,
    DateOnly? DateOfBirth,
    decimal? Weight,
    string? Color,
    string? MicrochipNumber,
    int OwnerId);
