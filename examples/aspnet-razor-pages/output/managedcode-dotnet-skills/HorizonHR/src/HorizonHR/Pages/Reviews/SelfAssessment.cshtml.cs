using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public class SelfAssessmentModel(IReviewService reviewService) : PageModel
{
    public PerformanceReview? Review { get; set; }

    [BindProperty, Required, MaxLength(5000), Display(Name = "Self-Assessment")]
    public string SelfAssessmentText { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Review = await reviewService.GetByIdAsync(id);
        if (Review is null) return NotFound();
        SelfAssessmentText = Review.SelfAssessment ?? string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            Review = await reviewService.GetByIdAsync(id);
            return Page();
        }

        try
        {
            await reviewService.SubmitSelfAssessmentAsync(id, SelfAssessmentText);
            TempData["SuccessMessage"] = "Self-assessment submitted.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            Review = await reviewService.GetByIdAsync(id);
            return Page();
        }
    }
}
