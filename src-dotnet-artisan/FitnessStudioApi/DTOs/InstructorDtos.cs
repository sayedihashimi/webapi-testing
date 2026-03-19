using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public sealed record InstructorResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Bio,
    string? Specializations,
    DateOnly HireDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateInstructorRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Phone { get; init; }

    [MaxLength(1000)]
    public string? Bio { get; init; }

    public string? Specializations { get; init; }

    public required DateOnly HireDate { get; init; }
}

public sealed record UpdateInstructorRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Phone { get; init; }

    [MaxLength(1000)]
    public string? Bio { get; init; }

    public string? Specializations { get; init; }

    public bool IsActive { get; init; } = true;
}
