using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkEvents.Models;

public class Attendee
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Organization { get; set; }

    [MaxLength(500)]
    public string? DietaryNeeds { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    public ICollection<Registration> Registrations { get; set; } = [];
}
