using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public class DetailsModel(IReviewService reviewService, ApplicationDbContext db) : PageModel
{
    public PerformanceReview? Review { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Review = await reviewService.GetByIdAsync(id);
        if (Review is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        // Transition from Draft to SelfAssessmentPending
        var review = await db.PerformanceReviews.FindAsync(id);
        if (review is null) return NotFound();

        if (review.Status != ReviewStatus.Draft)
        {
            TempData["ErrorMessage"] = "Review must be in Draft status.";
            return RedirectToPage("Details", new { id });
        }

        review.Status = ReviewStatus.SelfAssessmentPending;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Review moved to self-assessment phase.";
        return RedirectToPage("Details", new { id });
    }
}
