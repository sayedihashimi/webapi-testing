using System.ComponentModel.DataAnnotations;
using LibraryApi.Models.Enums;

namespace LibraryApi.DTOs.Patron;

public record PatronListDto(
    int Id, string FirstName, string LastName, string Email,
    MembershipType MembershipType, bool IsActive, DateOnly MembershipDate);

public record PatronDetailDto(
    int Id, string FirstName, string LastName, string Email,
    string? Phone, string? Address, DateOnly MembershipDate,
    MembershipType MembershipType, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt,
    int ActiveLoans, int TotalFinesUnpaid, decimal UnpaidFineAmount);

public class CreatePatronDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public MembershipType MembershipType { get; set; } = MembershipType.Standard;
}

public class UpdatePatronDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public MembershipType MembershipType { get; set; } = MembershipType.Standard;
    public bool IsActive { get; set; } = true;
}
