using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public class PerformanceReview
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int ReviewerId { get; set; }
    public Employee Reviewer { get; set; } = null!;

    [Required]
    public DateOnly ReviewPeriodStart { get; set; }

    [Required]
    public DateOnly ReviewPeriodEnd { get; set; }

    public ReviewStatus Status { get; set; } = ReviewStatus.Draft;

    public OverallRating? OverallRating { get; set; }

    [MaxLength(5000)]
    public string? SelfAssessment { get; set; }

    [MaxLength(5000)]
    public string? ManagerAssessment { get; set; }

    [MaxLength(5000)]
    public string? Goals { get; set; }

    [MaxLength(2000)]
    public string? StrengthsNoted { get; set; }

    [MaxLength(2000)]
    public string? AreasForImprovement { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
