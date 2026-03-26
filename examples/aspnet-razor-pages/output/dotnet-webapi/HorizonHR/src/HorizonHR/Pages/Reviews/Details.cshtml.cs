using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Reviews;

public sealed class DetailsModel(IReviewService reviewService, HorizonDbContext db) : PageModel
{
    public PerformanceReview Review { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var review = await reviewService.GetByIdAsync(id, ct);
        if (review == null) return NotFound();
        Review = review;
        return Page();
    }

    public async Task<IActionResult> OnPostStartSelfAssessmentAsync(int id, CancellationToken ct)
    {
        var review = await db.PerformanceReviews.FindAsync([id], ct);
        if (review == null) return NotFound();

        if (review.Status == ReviewStatus.Draft)
        {
            review.Status = ReviewStatus.SelfAssessmentPending;
            await db.SaveChangesAsync(ct);
            TempData["SuccessMessage"] = "Review moved to Self-Assessment stage.";
        }

        return RedirectToPage("Details", new { id });
    }
}
