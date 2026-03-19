using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public sealed record CreateVeterinarianRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Phone { get; init; }

    public string? Specialization { get; init; }

    [Required]
    public required string LicenseNumber { get; init; }

    [Required]
    public required DateOnly HireDate { get; init; }
}

public sealed record UpdateVeterinarianRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Phone { get; init; }

    public string? Specialization { get; init; }

    [Required]
    public required string LicenseNumber { get; init; }

    public required bool IsAvailable { get; init; }
}

public sealed record VeterinarianResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Specialization,
    string LicenseNumber,
    bool IsAvailable,
    DateOnly HireDate);
