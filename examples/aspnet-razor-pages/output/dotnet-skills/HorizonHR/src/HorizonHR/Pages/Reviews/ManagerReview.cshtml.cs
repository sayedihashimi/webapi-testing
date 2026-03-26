using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Pages.Reviews;

public class ManagerReviewModel : PageModel
{
    private readonly IReviewService _reviewService;

    public ManagerReviewModel(IReviewService reviewService)
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
        [Display(Name = "Manager Assessment")]
        public string ManagerAssessment { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Overall Rating")]
        public OverallRating OverallRating { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Strengths Noted")]
        public string? StrengthsNoted { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Areas for Improvement")]
        public string? AreasForImprovement { get; set; }

        [MaxLength(5000)]
        [Display(Name = "Goals")]
        public string? Goals { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        Review = review;
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
            await _reviewService.CompleteManagerReviewAsync(
                id,
                Input.ManagerAssessment,
                Input.OverallRating,
                Input.StrengthsNoted,
                Input.AreasForImprovement,
                Input.Goals);

            TempData["SuccessMessage"] = "Manager review completed successfully.";
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
