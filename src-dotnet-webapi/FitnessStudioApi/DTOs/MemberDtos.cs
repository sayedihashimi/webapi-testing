using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public sealed record MemberResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateOnly DateOfBirth,
    string EmergencyContactName,
    string EmergencyContactPhone,
    DateOnly JoinDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record CreateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Phone { get; init; }

    [Required]
    public required DateOnly DateOfBirth { get; init; }

    [Required, MaxLength(200)]
    public required string EmergencyContactName { get; init; }

    [Required]
    public required string EmergencyContactPhone { get; init; }
}

public sealed record UpdateMemberRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Phone { get; init; }

    [Required]
    public required DateOnly DateOfBirth { get; init; }

    [Required, MaxLength(200)]
    public required string EmergencyContactName { get; init; }

    [Required]
    public required string EmergencyContactPhone { get; init; }
}
