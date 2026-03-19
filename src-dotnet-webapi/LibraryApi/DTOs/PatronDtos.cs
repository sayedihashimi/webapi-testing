using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public sealed record PatronResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    DateOnly MembershipDate,
    MembershipType MembershipType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record PatronDetailResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    DateOnly MembershipDate,
    MembershipType MembershipType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ActiveLoans,
    decimal UnpaidFines);

public sealed record CreatePatronRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress, MaxLength(200)]
    public required string Email { get; init; }

    [MaxLength(20)]
    public string? Phone { get; init; }

    [MaxLength(500)]
    public string? Address { get; init; }

    public MembershipType MembershipType { get; init; } = MembershipType.Standard;
}

public sealed record UpdatePatronRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress, MaxLength(200)]
    public required string Email { get; init; }

    [MaxLength(20)]
    public string? Phone { get; init; }

    [MaxLength(500)]
    public string? Address { get; init; }

    public required MembershipType MembershipType { get; init; }
}
