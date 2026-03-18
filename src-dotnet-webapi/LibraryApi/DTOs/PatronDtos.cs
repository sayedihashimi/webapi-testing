using System.ComponentModel.DataAnnotations;
using LibraryApi.Models;

namespace LibraryApi.DTOs;

public class CreatePatronRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Required]
    public MembershipType MembershipType { get; set; }
}

public class UpdatePatronRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Required]
    public MembershipType MembershipType { get; set; }
}

public class PatronResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateOnly MembershipDate { get; set; }
    public MembershipType MembershipType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PatronDetailResponse : PatronResponse
{
    public int ActiveLoanCount { get; set; }
    public decimal UnpaidFinesTotal { get; set; }
    public int ReservationCount { get; set; }
}
