using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkEvents.Models;

public class CheckIn
{
    public int Id { get; set; }

    [Required]
    public int RegistrationId { get; set; }

    [ForeignKey(nameof(RegistrationId))]
    public Registration Registration { get; set; } = null!;

    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(200)]
    public string CheckedInBy { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
