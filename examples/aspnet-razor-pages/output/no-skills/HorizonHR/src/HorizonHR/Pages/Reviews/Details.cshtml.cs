using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public class DetailsModel : PageModel
{
    private readonly IReviewService _reviewService;
    private readonly Data.ApplicationDbContext _context;

    public DetailsModel(IReviewService reviewService, Data.ApplicationDbContext context)
    {
        _reviewService = reviewService;
        _context = context;
    }

    public PerformanceReview? Review { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Review = await _reviewService.GetByIdAsync(id);
        if (Review == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostStartSelfAssessmentAsync(int id)
    {
        var review = await _context.PerformanceReviews.FindAsync(id);
        if (review == null) return NotFound();

        if (review.Status == ReviewStatus.Draft)
        {
            review.Status = ReviewStatus.SelfAssessmentPending;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Review moved to Self-Assessment stage.";
        }

        return RedirectToPage("Details", new { id });
    }
}
