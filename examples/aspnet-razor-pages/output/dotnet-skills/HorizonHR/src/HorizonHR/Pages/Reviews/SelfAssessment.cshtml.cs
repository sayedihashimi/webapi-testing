using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Pages.Reviews;

public class SelfAssessmentModel : PageModel
{
    private readonly IReviewService _reviewService;

    public SelfAssessmentModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public PerformanceReview Review { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [MaxLength(5000)]
        [Display(Name = "Self Assessment")]
        public string SelfAssessment { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        Review = review;

        if (!string.IsNullOrEmpty(review.SelfAssessment))
        {
            Input.SelfAssessment = review.SelfAssessment;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null) return NotFound();
            Review = review;
            return Page();
        }

        try
        {
            await _reviewService.SubmitSelfAssessmentAsync(id, Input.SelfAssessment);
            TempData["SuccessMessage"] = "Self-assessment submitted successfully.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null) return NotFound();
            Review = review;
            return Page();
        }
    }
}
