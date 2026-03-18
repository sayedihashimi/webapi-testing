namespace FitnessStudioApi.Models;

public class Member
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
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Membership> Memberships { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}
