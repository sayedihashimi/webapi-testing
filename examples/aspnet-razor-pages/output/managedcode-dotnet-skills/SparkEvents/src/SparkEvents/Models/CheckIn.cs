using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public class CheckIn
{
    public int Id { get; set; }

    [Required]
    public required int RegistrationId { get; set; }

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(200)]
    public required string CheckedInBy { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation property (one-to-one with Registration)
    public Registration Registration { get; set; } = null!;
}
