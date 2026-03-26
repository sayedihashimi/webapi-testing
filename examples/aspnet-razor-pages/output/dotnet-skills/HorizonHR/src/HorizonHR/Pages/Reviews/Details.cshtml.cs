using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Reviews;

public class DetailsModel : PageModel
{
    private readonly IReviewService _reviewService;

    public DetailsModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public PerformanceReview Review { get; set; } = null!;

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

    public async Task<IActionResult> OnPostStartAsync(int id)
    {
        try
        {
            await _reviewService.StartReviewAsync(id);
            TempData["SuccessMessage"] = "Review has been started successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage("Details", new { id });
    }
}
