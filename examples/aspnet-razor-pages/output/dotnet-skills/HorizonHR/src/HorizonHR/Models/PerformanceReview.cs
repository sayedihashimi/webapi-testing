using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HorizonHR.Models.Enums;

namespace HorizonHR.Models;

public class PerformanceReview
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required]
    [Display(Name = "Reviewer")]
    public int ReviewerId { get; set; }

    [Required]
    [Display(Name = "Review Period Start")]
    public DateOnly ReviewPeriodStart { get; set; }

    [Required]
    [Display(Name = "Review Period End")]
    public DateOnly ReviewPeriodEnd { get; set; }

    [Display(Name = "Status")]
    public ReviewStatus Status { get; set; } = ReviewStatus.Draft;

    [Display(Name = "Overall Rating")]
    public OverallRating? OverallRating { get; set; }

    [MaxLength(5000)]
    [Display(Name = "Self Assessment")]
    public string? SelfAssessment { get; set; }

    [MaxLength(5000)]
    [Display(Name = "Manager Assessment")]
    public string? ManagerAssessment { get; set; }

    [MaxLength(5000)]
    [Display(Name = "Goals")]
    public string? Goals { get; set; }

    [MaxLength(2000)]
    [Display(Name = "Strengths Noted")]
    public string? StrengthsNoted { get; set; }

    [MaxLength(2000)]
    [Display(Name = "Areas for Improvement")]
    public string? AreasForImprovement { get; set; }

    [Display(Name = "Completed Date")]
    public DateOnly? CompletedDate { get; set; }

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Updated At")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(ReviewerId))]
    [Display(Name = "Reviewer")]
    public Employee Reviewer { get; set; } = null!;
}
