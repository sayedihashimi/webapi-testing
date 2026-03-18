using LibraryApi.Models;

namespace LibraryApi.DTOs;

public class PatronDto
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

public class PatronDetailDto : PatronDto
{
    public int ActiveLoansCount { get; set; }
    public decimal TotalUnpaidFines { get; set; }
}

public class CreatePatronDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public MembershipType MembershipType { get; set; } = MembershipType.Standard;
}

public class UpdatePatronDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public MembershipType MembershipType { get; set; }
    public bool IsActive { get; set; }
}
