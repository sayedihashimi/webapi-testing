using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record CreateOwnerRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    string? Address,
    string? City,
    [MaxLength(2)] string? State,
    string? ZipCode);

public sealed record UpdateOwnerRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string Phone,
    string? Address,
    string? City,
    [MaxLength(2)] string? State,
    string? ZipCode);

public sealed record OwnerResponse(
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

public sealed record OwnerDetailResponse(
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
    IReadOnlyList<PetSummaryResponse> Pets);

public sealed record PetSummaryResponse(
    int Id,
    string Name,
    string Species,
    string? Breed,
    bool IsActive);
