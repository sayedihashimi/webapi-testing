using System.ComponentModel.DataAnnotations;

namespace SparkEvents.Models;

public sealed class CheckIn
{
    public int Id { get; set; }

    public int RegistrationId { get; set; }
    public Registration Registration { get; set; } = null!;

    public DateTime CheckInTime { get; set; }

    [Required, MaxLength(200)]
    public string CheckedInBy { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
