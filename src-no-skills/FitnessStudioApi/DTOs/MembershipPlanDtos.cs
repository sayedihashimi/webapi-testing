using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

// --- MembershipPlan DTOs ---
public class MembershipPlanCreateDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, 24)]
    public int DurationMonths { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
    public decimal Price { get; set; }

    public int MaxClassBookingsPerWeek { get; set; }

    public bool AllowsPremiumClasses { get; set; }
}

public class MembershipPlanUpdateDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(1, 24)]
    public int DurationMonths { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
    public decimal Price { get; set; }

    public int MaxClassBookingsPerWeek { get; set; }

    public bool AllowsPremiumClasses { get; set; }

    public bool IsActive { get; set; }
}

public class MembershipPlanResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
    public int MaxClassBookingsPerWeek { get; set; }
    public bool AllowsPremiumClasses { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
