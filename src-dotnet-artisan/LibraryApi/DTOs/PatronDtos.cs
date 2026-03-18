using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

// --- Patron DTOs ---
public sealed record PatronResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    MembershipType MembershipType,
    DateOnly MembershipDate,
    bool IsActive);

public sealed record PatronDetailResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Address,
    MembershipType MembershipType,
    DateOnly MembershipDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ActiveLoansCount,
    decimal UnpaidFinesBalance);

public sealed record CreatePatronRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, MaxLength(200), EmailAddress]
    public string Email { get; init; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; init; }

    [MaxLength(500)]
    public string? Address { get; init; }

    public MembershipType MembershipType { get; init; } = MembershipType.Standard;
}

public sealed record UpdatePatronRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required, MaxLength(200), EmailAddress]
    public string Email { get; init; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; init; }

    [MaxLength(500)]
    public string? Address { get; init; }

    public MembershipType MembershipType { get; init; } = MembershipType.Standard;
}
