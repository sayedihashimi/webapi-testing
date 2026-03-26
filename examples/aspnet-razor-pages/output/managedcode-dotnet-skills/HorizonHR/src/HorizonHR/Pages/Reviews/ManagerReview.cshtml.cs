using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public class ManagerReviewModel(IReviewService reviewService) : PageModel
{
    public PerformanceReview? Review { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(5000), Display(Name = "Manager Assessment")]
        public string ManagerAssessment { get; set; } = string.Empty;

        [Required, Display(Name = "Overall Rating")]
        public OverallRating Rating { get; set; }

        [MaxLength(2000)]
        public string? Strengths { get; set; }

        [MaxLength(2000), Display(Name = "Areas for Improvement")]
        public string? AreasForImprovement { get; set; }

        [MaxLength(5000)]
        public string? Goals { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Review = await reviewService.GetByIdAsync(id);
        if (Review is null) return NotFound();
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
            await reviewService.CompleteManagerReviewAsync(id,
                Input.ManagerAssessment, Input.Rating,
                Input.Strengths, Input.AreasForImprovement, Input.Goals);
            TempData["SuccessMessage"] = "Review completed.";
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
