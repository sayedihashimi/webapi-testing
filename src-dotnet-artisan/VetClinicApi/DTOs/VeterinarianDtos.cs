namespace VetClinicApi.DTOs;

public sealed record VeterinarianDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Specialization,
    string LicenseNumber,
    bool IsAvailable,
    DateOnly HireDate);

public sealed record CreateVeterinarianDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Specialization,
    string LicenseNumber,
    DateOnly HireDate);

public sealed record UpdateVeterinarianDto(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Specialization,
    string LicenseNumber,
    bool IsAvailable);
