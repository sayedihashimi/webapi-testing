using System.ComponentModel.DataAnnotations;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public sealed class SelfAssessmentModel(IReviewService reviewService) : PageModel
{
    public int ReviewId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }

    [BindProperty, Required, MaxLength(5000)]
    public string SelfAssessmentText { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var review = await reviewService.GetByIdAsync(id, ct);
        if (review == null || review.Status != ReviewStatus.SelfAssessmentPending) return NotFound();

        ReviewId = id;
        EmployeeName = $"{review.Employee.FirstName} {review.Employee.LastName}";
        PeriodStart = review.ReviewPeriodStart;
        PeriodEnd = review.ReviewPeriodEnd;
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
            await reviewService.SubmitSelfAssessmentAsync(id, SelfAssessmentText, ct);
            TempData["SuccessMessage"] = "Self-assessment submitted.";
            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("Details", new { id });
        }
    }
}
