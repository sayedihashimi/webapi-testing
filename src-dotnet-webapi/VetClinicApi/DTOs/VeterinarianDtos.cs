using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

public record CreateVeterinarianRequest(
    [property: Required, MaxLength(100)] string FirstName,
    [property: Required, MaxLength(100)] string LastName,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Phone,
    string? Specialization,
    [property: Required] string LicenseNumber,
    [property: Required] DateOnly HireDate);

public record UpdateVeterinarianRequest(
    [property: Required, MaxLength(100)] string FirstName,
    [property: Required, MaxLength(100)] string LastName,
    [property: Required, EmailAddress] string Email,
    [property: Required] string Phone,
    string? Specialization,
    [property: Required] string LicenseNumber,
    bool IsAvailable,
    [property: Required] DateOnly HireDate);

public record VeterinarianResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Specialization,
    string LicenseNumber,
    bool IsAvailable,
    DateOnly HireDate);
