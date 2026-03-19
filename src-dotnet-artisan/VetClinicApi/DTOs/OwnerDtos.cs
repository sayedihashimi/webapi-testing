namespace VetClinicApi.DTOs;

public sealed record OwnerDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record OwnerDetailDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<PetSummaryDto> Pets);

public sealed record CreateOwnerDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Address,
    string? City,
    string? State,
    string? ZipCode);

public sealed record UpdateOwnerDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Address,
    string? City,
    string? State,
    string? ZipCode);
