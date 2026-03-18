using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.DTOs;

// --- Veterinarian DTOs ---

public sealed record CreateVeterinarianRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required, Phone] string Phone,
    string? Specialization,
    [Required] string LicenseNumber,
    bool IsAvailable,
    [Required] DateOnly HireDate);

public sealed record UpdateVeterinarianRequest(
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName,
    [Required, EmailAddress] string Email,
    [Required, Phone] string Phone,
    string? Specialization,
    [Required] string LicenseNumber,
    bool IsAvailable);

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
