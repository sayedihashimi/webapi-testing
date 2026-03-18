using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public class MemberDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public DateOnly JoinDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MemberDetailDto : MemberDto
{
    public MembershipDto? ActiveMembership { get; set; }
    public int TotalBookings { get; set; }
    public int UpcomingBookings { get; set; }
    public int AttendedClasses { get; set; }
}

public class CreateMemberDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required, MaxLength(200)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required]
    public string EmergencyContactPhone { get; set; } = string.Empty;
}

public class UpdateMemberDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required]
    public string EmergencyContactPhone { get; set; } = string.Empty;
}
