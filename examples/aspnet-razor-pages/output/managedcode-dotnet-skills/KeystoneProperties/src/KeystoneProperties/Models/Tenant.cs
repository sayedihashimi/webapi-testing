using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public class Tenant
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    [MaxLength(200)]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string EmergencyContactPhone { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<Lease> Leases { get; set; } = [];

    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = [];
}
