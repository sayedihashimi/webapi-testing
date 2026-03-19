using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public sealed record PatronResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    MembershipType MembershipType,
    bool IsActive,
    DateOnly MembershipDate);

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
    int ActiveLoansCount,
    decimal TotalUnpaidFines);

public sealed class CreatePatronRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; init; }

    [MaxLength(500)]
    public string? Address { get; init; }

    public MembershipType MembershipType { get; init; } = MembershipType.Standard;
}

public sealed class UpdatePatronRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; init; }

    [MaxLength(500)]
    public string? Address { get; init; }

    public MembershipType MembershipType { get; init; } = MembershipType.Standard;
}
