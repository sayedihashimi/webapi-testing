using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record CreateOwnerRequest(
    [property: Required, MaxLength(100)] string FirstName,
    [property: Required, MaxLength(100)] string LastName,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Phone,
    string? Address,
    string? City,
    [property: MaxLength(2)] string? State,
    string? ZipCode);

public record UpdateOwnerRequest(
    [property: Required, MaxLength(100)] string FirstName,
    [property: Required, MaxLength(100)] string LastName,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Phone,
    string? Address,
    string? City,
    [property: MaxLength(2)] string? State,
    string? ZipCode);

public record OwnerResponse(
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

public record OwnerDetailResponse(
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
    List<PetSummaryResponse> Pets);

public record PetSummaryResponse(
    int Id,
    string Name,
    string Species,
    string? Breed,
    bool IsActive);
