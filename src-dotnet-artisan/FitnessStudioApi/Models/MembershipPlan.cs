using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.Models;

public sealed class MembershipPlan
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, 24)]
    public int DurationMonths { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public int MaxClassBookingsPerWeek { get; set; }

    public bool AllowsPremiumClasses { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
