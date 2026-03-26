using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public sealed class ManagerReviewModel(IReviewService reviewService) : PageModel
{
    public int ReviewId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string? SelfAssessment { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required, MaxLength(5000)]
        public string ManagerAssessment { get; set; } = string.Empty;

        [Required]
        public OverallRating Rating { get; set; }

        [MaxLength(2000)]
        public string? Strengths { get; set; }

        [MaxLength(2000)]
        public string? AreasForImprovement { get; set; }

        [MaxLength(5000)]
        public string? Goals { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var review = await reviewService.GetByIdAsync(id, ct);
        if (review == null || review.Status != ReviewStatus.ManagerReviewPending) return NotFound();

        ReviewId = id;
        EmployeeName = $"{review.Employee.FirstName} {review.Employee.LastName}";
        PeriodStart = review.ReviewPeriodStart;
        PeriodEnd = review.ReviewPeriodEnd;
        SelfAssessment = review.SelfAssessment;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ReviewId = id;
            return Page();
        }

        try
        {
            await reviewService.CompleteManagerReviewAsync(id, Input.ManagerAssessment, Input.Rating, Input.Strengths, Input.AreasForImprovement, Input.Goals, ct);
            TempData["SuccessMessage"] = "Review completed successfully.";
            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("Details", new { id });
        }
    }
}
