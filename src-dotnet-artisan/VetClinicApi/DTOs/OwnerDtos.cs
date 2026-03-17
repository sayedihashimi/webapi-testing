using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record OwnerDto(
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

public record OwnerDetailDto(
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

public record CreateOwnerDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    public string? Address { get; init; }
    public string? City { get; init; }

    [MaxLength(2)]
    public string? State { get; init; }

    public string? ZipCode { get; init; }
}

public record UpdateOwnerDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    public string? Address { get; init; }
    public string? City { get; init; }

    [MaxLength(2)]
    public string? State { get; init; }

    public string? ZipCode { get; init; }
}
