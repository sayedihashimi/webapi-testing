using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.Models;

public class Member
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    [Required, MaxLength(200)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required]
    public string EmergencyContactPhone { get; set; } = string.Empty;

    public DateOnly JoinDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Membership> Memberships { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}
