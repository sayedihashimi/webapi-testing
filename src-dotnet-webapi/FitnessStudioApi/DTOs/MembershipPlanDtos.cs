using System.ComponentModel.DataAnnotations;

namespace FitnessStudioApi.DTOs;

public class CreateMembershipPlanRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, Range(1, 24)]
    public int DurationMonths { get; set; }

    [Required, Range(0.01, 99999.99)]
    public decimal Price { get; set; }

    [Required, Range(-1, 100)]
    public int MaxClassBookingsPerWeek { get; set; }

    public bool AllowsPremiumClasses { get; set; }
}

public class UpdateMembershipPlanRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required, Range(1, 24)]
    public int DurationMonths { get; set; }

    [Required, Range(0.01, 99999.99)]
    public decimal Price { get; set; }

    [Required, Range(-1, 100)]
    public int MaxClassBookingsPerWeek { get; set; }

    public bool AllowsPremiumClasses { get; set; }
    public bool IsActive { get; set; }
}

public class MembershipPlanResponse
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
