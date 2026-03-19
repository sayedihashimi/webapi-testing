using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Owner DTOs ---

public sealed record CreateOwnerRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Phone { get; init; }

    public string? Address { get; init; }
    public string? City { get; init; }

    [MaxLength(2)]
    public string? State { get; init; }

    public string? ZipCode { get; init; }
}

public sealed record UpdateOwnerRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Phone { get; init; }

    public string? Address { get; init; }
    public string? City { get; init; }

    [MaxLength(2)]
    public string? State { get; init; }

    public string? ZipCode { get; init; }
}

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
