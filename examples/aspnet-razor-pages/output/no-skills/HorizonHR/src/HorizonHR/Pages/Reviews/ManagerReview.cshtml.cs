using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public class ManagerReviewModel : PageModel
{
    private readonly IReviewService _reviewService;

    public ManagerReviewModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public PerformanceReview? Review { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(5000)]
        public string ManagerAssessment { get; set; } = string.Empty;

        [Required]
        public OverallRating OverallRating { get; set; }

        [MaxLength(2000)]
        public string? StrengthsNoted { get; set; }

        [MaxLength(2000)]
        public string? AreasForImprovement { get; set; }

        [MaxLength(5000)]
        public string? Goals { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Review = await _reviewService.GetByIdAsync(id);
        if (Review == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Review = await _reviewService.GetByIdAsync(id);
        if (Review == null) return NotFound();

        if (!ModelState.IsValid) return Page();

        try
        {
            await _reviewService.CompleteManagerReviewAsync(id,
                Input.ManagerAssessment,
                Input.OverallRating,
                Input.StrengthsNoted,
                Input.AreasForImprovement,
                Input.Goals);

            TempData["Success"] = "Performance review completed.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return Page();
        }
    }
}
