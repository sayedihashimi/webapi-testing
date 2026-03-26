using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Reviews;

public class ManagerReviewModel(IReviewService reviewService) : PageModel
{
    public PerformanceReview Review { get; set; } = null!;
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        [Required, MaxLength(5000)] public string ManagerAssessment { get; set; } = string.Empty;
        [Required] public OverallRating OverallRating { get; set; }
        [MaxLength(2000)] public string? StrengthsNoted { get; set; }
        [MaxLength(2000)] public string? AreasForImprovement { get; set; }
        [MaxLength(5000)] public string? Goals { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await reviewService.GetByIdAsync(id);
        if (review is null || review.Status != ReviewStatus.ManagerReviewPending) { return NotFound(); }
        Review = review;
        Input.Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var review = await reviewService.GetByIdAsync(Input.Id);
        if (review is null) { return NotFound(); }
        Review = review;
        if (!ModelState.IsValid) { return Page(); }

        var error = await reviewService.CompleteManagerReviewAsync(
            Input.Id, Input.ManagerAssessment, Input.OverallRating,
            Input.StrengthsNoted, Input.AreasForImprovement, Input.Goals);

        if (error is not null)
        {
            ModelState.AddModelError("", error);
            return Page();
        }
        TempData["Success"] = "Review completed.";
        return RedirectToPage("Details", new { id = Input.Id });
    }
}
