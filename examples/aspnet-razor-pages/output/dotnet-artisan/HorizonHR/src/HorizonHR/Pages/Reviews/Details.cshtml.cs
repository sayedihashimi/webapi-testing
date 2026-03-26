using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public class DetailsModel(IReviewService reviewService) : PageModel
{
    public PerformanceReview Review { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var review = await reviewService.GetByIdAsync(id);
        if (review is null) { return NotFound(); }
        Review = review;
        return Page();
    }
}
