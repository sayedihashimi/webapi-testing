using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public class SelfAssessmentModel : PageModel
{
    private readonly IReviewService _reviewService;

    public SelfAssessmentModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public PerformanceReview? Review { get; set; }

    [BindProperty, Required, MaxLength(5000)]
    public string SelfAssessmentText { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Review = await _reviewService.GetByIdAsync(id);
        if (Review == null) return NotFound();
        SelfAssessmentText = Review.SelfAssessment ?? string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Review = await _reviewService.GetByIdAsync(id);
        if (Review == null) return NotFound();

        if (!ModelState.IsValid) return Page();

        try
        {
            await _reviewService.SubmitSelfAssessmentAsync(id, SelfAssessmentText);
            TempData["Success"] = "Self-assessment submitted.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return Page();
        }
    }
}
