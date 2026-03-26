using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public class SelfAssessmentModel(IReviewService reviewService) : PageModel
{
    public PerformanceReview Review { get; set; } = null!;
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        [Required, MaxLength(5000)] public string SelfAssessment { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await reviewService.GetByIdAsync(id);
        if (review is null) { return NotFound(); }
        if (review.Status != ReviewStatus.SelfAssessmentPending) { return RedirectToPage("Details", new { id }); }
        Review = review;
        Input.Id = id;
        Input.SelfAssessment = review.SelfAssessment ?? "";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var review = await reviewService.GetByIdAsync(Input.Id);
        if (review is null) { return NotFound(); }
        Review = review;
        if (!ModelState.IsValid) { return Page(); }

        var error = await reviewService.SubmitSelfAssessmentAsync(Input.Id, Input.SelfAssessment);
        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return Page();
        }
        TempData["Success"] = "Self-assessment submitted.";
        return RedirectToPage("Details", new { id = Input.Id });
    }
}
